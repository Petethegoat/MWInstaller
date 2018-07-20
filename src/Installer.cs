using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace MWInstaller
{
    static class Installer
    {
        private const int progressChunksPerPackage = 4;

        public static bool doFullInstall = true;

        public static event EventHandler StartEvent;
        public static event EventHandler<bool> CompleteEvent;
        public static event EventHandler<object[]> ProgressEvent;

        public static BackgroundWorker worker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public static void Initialize()
        {
            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                List<Package> paks = (List<Package>)args.Argument;

                CleanUpTempFiles(paks[0].list);  //TODO

                foreach(Package pak in paks)
                {
                    float progressChunk = (paks.IndexOf(pak) + 1) * progressChunksPerPackage;

                    worker.ReportProgress((int)progressChunk - 4, string.Format("Downloading {0}...", pak.name));
                    System.Threading.Thread.Sleep(10);
                    DownloadPackage(pak);

                    if(doFullInstall)
                    {
                        worker.ReportProgress((int)progressChunk - 3, "Unpacking archive...");
                        System.Threading.Thread.Sleep(10);
                        Unpack(pak);
                        worker.ReportProgress((int)progressChunk - 2, string.Format("Installing {0}...", pak.name));
                        System.Threading.Thread.Sleep(10);
                        InstallPackage(pak);
                        worker.ReportProgress((int)progressChunk - 1, "Cleaning up...");
                        System.Threading.Thread.Sleep(10);
                        DeleteArchive(pak);
                    }
                }

                worker.ReportProgress(GetProgressBarLength(paks.Count), "Done!");
            };

            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs args)
            {
                ProgressEvent.Invoke(null, new object[] { args.ProgressPercentage, args.UserState });
            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                if(args.Error != null)
                {
                    Log.Write(args.Error);
                    if(args.Error.InnerException != null)
                        Log.Write(args.Error.InnerException);

                    CompleteEvent.Invoke(null, false);
                    return;
                }
                else
                {
                    CompleteEvent.Invoke(null, true);
                }

            };
        }

        public static void PerformInstall(List<Package> paks)
        {
            if(paks == null || paks.Count < 1)
            {
                Log.Write("List<Package> is null or empty.");
                return;
            }
            if(worker.IsBusy)
            {
                Log.Write("Tried to start install while install was in progress.");
                return;
            }

            StartEvent.Invoke(null, null);
            worker.RunWorkerAsync(paks);
        }



        private static void CleanUpTempFiles(PackageList list)
        {
            var destination = Path.Combine(Path.GetTempPath(), list.name);
            if(Directory.Exists(destination))
                Directory.Delete(destination, true);
        }

        private static void DownloadPackage(Package pak)
        {
            string url = pak.fileURL;

            if(pak.requiresNexus)
            {
                url = Nexus.GetNexusDownloadURL(pak.fileURL);
                pak.fileName = Nexus.FileNameFromNexusDownloadURL(url);
            }

            var webClient = new WebClient();
            webClient.DownloadFile(url, pak.fileName);
        }

        private static void Unpack(Package pak)
        {
            var destination = Path.Combine(Path.GetTempPath(), pak.list.name, pak.name);
            Extraction.Extract(pak, destination);
        }

        private static void InstallPackage(Package pak)
        {
            var packagePath = Path.Combine(Path.GetTempPath(), pak.list.name, pak.name);
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
            if(File.Exists(pak.fileName))
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

        public static int GetProgressBarLength(int length)
        {
            return length * progressChunksPerPackage;
        }
    }
}
