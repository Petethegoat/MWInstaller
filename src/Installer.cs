using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MWInstaller
{
    static class Installer
    {
        public static BackgroundWorker worker = new BackgroundWorker();
        public static event EventHandler startEvent;
        public static event EventHandler completeEvent;
        public static event EventHandler<int> progressEvent;

        public static void PerformInstall(List<Package> paks, PackageList list)
        {
            if(worker.IsBusy)
                return;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            startEvent.Invoke(null, null);

            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                DeleteTempFiles(list);

                foreach(Package pak in paks)
                {
                    DownloadPackage(pak);
                    Unpack(pak, list);
                    DeleteArchive(pak);
                    InstallPackage(pak, list);
                    worker.ReportProgress(paks.IndexOf(pak) + 1);
                }
            };

            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs args)
            {
                progressEvent.Invoke(null, args.ProgressPercentage);
            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                completeEvent.Invoke(null, null);
            };

            worker.RunWorkerAsync();
        }



        private static void DeleteTempFiles(PackageList list)
        {
            var destination = Path.Combine(Path.GetTempPath(), list.name);
            Directory.Delete(destination, true);
        }

        private static void DownloadPackage(Package pak)
        {
            string url = pak.fileURL;

            if(pak.requiresNexus)
                url = Nexus.GetNexusDownloadURL(pak.fileURL);

            var webClient = new WebClient();
            webClient.DownloadFile(url, pak.fileName);
        }

        private static void Unpack(Package pak, PackageList list)
        {
            var destination = Path.Combine(Path.GetTempPath(), list.name, pak.name);
            Extraction.Extract(pak, destination);
        }

        private static void InstallPackage(Package pak, PackageList list)
        {
            var packagePath = Path.Combine(Path.GetTempPath(), list.name, pak.name);
            DirectoryInfo d = new DirectoryInfo(packagePath);

            foreach(FileInfo f in d.GetFiles("*", SearchOption.AllDirectories))
            {
                // First, extract special files. They ignore all filters and normal installation rules, in favor of going directly to a certain directory.
                if(SpecialExtract(f, pak))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("File specially extracted: {0}", f.FullName));
                    continue;
                }

                // Check for filter exclusions.
                if(ExcludedByFilter(f, pak))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("File excluded: {0}", f.FullName));
                    continue;
                }

                string installPath;
                Match hasDataFiles = Regex.Match(Path.GetDirectoryName(f.FullName), string.Format(".*((?:{0}).*)", Morrowind.dataFiles), RegexOptions.IgnoreCase);
                Match hasDataFolder = Regex.Match(Path.GetDirectoryName(f.FullName), string.Format(".*((?:{0}).*)", Morrowind.dataFoldersRegex), RegexOptions.IgnoreCase);
                bool warnOnNameChange = true;

                if(f.Extension.Fix() == ".esp") // If it's an .esp
                {
                    installPath = Path.Combine(Morrowind.morrowindPath, Morrowind.dataFiles, f.Name);
                }
                else if(hasDataFiles.Success) // If it has data files in the path.
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format("DataFiles Success: {0}", hasDataFiles.Groups[1].Value));
                    installPath = Path.Combine(Morrowind.morrowindPath, hasDataFiles.Groups[1].Value, f.Name);
                }
                else if(hasDataFolder.Success) // If it has a data folder (meshes, textures) in the path.
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format("DataFolder Success: {0}", hasDataFolder.Groups[1].Value));
                    installPath = Path.Combine(Morrowind.morrowindPath, Morrowind.dataFiles, hasDataFolder.Groups[1].Value, f.Name);
                }
                else if(f.Name.Fix().Contains("readme")) // If it appears to be a readme.
                {
                    installPath = Path.Combine(Morrowind.morrowindPath, Morrowind.dataFiles, "docs\\", string.Format("{0}_{1}", pak.name, f.Name));
                    warnOnNameChange = false;
                }
                else // If it doesn't match anything else, just put it in.
                {
                    installPath = Path.Combine(Morrowind.morrowindPath, Utils.RelativePath(packagePath, f.FullName));
                    System.Diagnostics.Debug.WriteLine(string.Format("Uncertain install: {0}", installPath));
                }

                // Warning if name changed.
                if(warnOnNameChange && Path.GetFileName(installPath) != f.Name)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Filename changed: {0} to {1}", f.FullName, installPath));
                }

                InstallFile(f, installPath);
            }
        }

        private static void DeleteArchive(Package pak)
        {
            File.Delete(pak.fileName);
        }

        private static bool ExcludedByFilter(FileInfo file, Package pak)
        {
            // Whitelist
            if(!string.IsNullOrEmpty(pak.filterWhitelist))
            {
                if(!Regex.Match(file.FullName.Fix(), pak.filterWhitelist, RegexOptions.IgnoreCase).Success)
                    return true;
            }

            // File Blacklist
            foreach(string s in pak.fileBlacklist)
            {
                if(s.Fix().Contains(file.Name.Fix()))
                    return true;
            }

            // Directory Blacklist
            foreach(string s in pak.directoryBlacklist)
            {
                if(file.FullName.Fix().Contains(s.Fix()))
                    return true;
            }

            // This file wasn't filtered out! Nice!
            return false;
        }

        private static bool SpecialExtract(FileInfo file, Package pak)
        {
            foreach(string key in pak.specialExtract.Keys)
            {
                if(file.FullName.Fix().Contains(key.Fix()))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("File {0} was extracted to {1}", file.FullName, pak.specialExtract[key]));
                    InstallFile(file, Path.Combine(Morrowind.morrowindPath, pak.specialExtract[key]));
                    return true;
                }
            }

            return false;
        }

        private static void InstallFile(FileInfo file, string installPath)
        {
            if(File.Exists(installPath))
                File.Delete(installPath);

            if(!Directory.Exists(installPath))
                Directory.CreateDirectory(Path.GetDirectoryName(installPath));

            File.Move(file.FullName, installPath);
        }

        /// <summary>
        /// Lowers case, and replaces backslashes with forward slashes for consistent comparisons.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Fixed string.</returns>
        private static string Fix(this string path)
        {
            return path.ToLower().Replace("\\", "/");
        }
    }
}
