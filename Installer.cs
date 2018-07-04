using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// Mod package installer.
/// 1) Reads a text file 'packageList.txt' in it's directory.
/// 2) Downloads a series of .json files listed in packageList.txt
/// 3) Downloads files from URLs listed in those .json files.
/// 4) Extracts the contents and cleans up after itself.
/// </summary>
class Installer
{
    const string dataFiles = "Data Files";
    static readonly IReadOnlyList<string> dataFolders = new List<string>() { "bookart", "distantland", "docs", "fonts", "icons", "meshes", "music", "mwse", "shaders", "sound", "splash", "textures", "video" };

    static string installLocation;
    static string sevenZipLocation;

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">
    /// Arguments. Currently none supported.
    /// </param
    [STAThread]
    static void Main(string[] args)
    {
        // Check to see if Morrowind is running; wait for it to close.
        if (IsMorrowindRunning())
        {
            Console.WriteLine("Waiting for Morrowind to close...");
            while (IsMorrowindRunning())
            {
                Thread.Sleep(1000);
            }
        }

        // Make sure that we actually have a valid install location.
        if(!GetInstallLocation(ref installLocation))
        {
            Console.WriteLine("ERROR: Could not determine Morrowind install location. Please place the program in the same folder as Morrowind.exe. Press any key to exit.");
            Console.ReadKey();
            return;
        }
        if(!Get7ZipLocation(ref sevenZipLocation))
        {
            Console.WriteLine("Could not find 7z.exe. Install 7zip, or place 7z.exe in the same folder. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        installLocation = "E:\\Games\\Morrowind Modpack Test";

        try
        {
            // Get our package list.
            string packagePath = Path.Combine(installLocation, "packageList.json");
            PackageList packageList;
            if(File.Exists(packagePath))
            {
                // TODO: This might cause issues on certain system languages, may need specific encoding.
                packageList = PackageList.Deserialize(File.ReadAllText(packagePath, Encoding.UTF8));
            }
            else
            {
                Console.WriteLine("Couldn't find {0}.", packagePath);
                Exit();
                return;
            }

            // Show the package list info, and ask to begin installation.
            Console.WriteLine("{0}, last updated {1}.\nCurated by {2}.\n\n{3}\n", packageList.name, packageList.lastUpdated, packageList.curator, packageList.description);
            Console.WriteLine("Press Y to begin installation to {0}", installLocation);
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while(key.KeyChar != char.Parse("y"))
            {
                key = Console.ReadKey(true);
                if(key.KeyChar == char.Parse("n"))  // N for Nexus
                {
                    Nexus.TestRequest();
                    return;
                }
            }


            Console.WriteLine("\nDownloading {0} package{1}...", packageList.packages.Length, packageList.packages.Length == 1 ? "" : "s");

            // Go through and download all the listed packages.
            var webClient = new WebClient();
            foreach(string s in packageList.packages)
            {
                var package = Package.Deserialize(webClient.DownloadString(s));
                try
                {
                    // Get package name and file URL from the .json
                    webClient.DownloadFile(package.fileURL, package.fileName);

                    Console.Write("Installing {0}, by {1}...", package.name, package.author);
                    if(ExtractArchive(packageList, package))
                        Console.Write(" Done.\n");

                    // Delete the package archive.
                    File.Delete(package.fileName);
                }
                catch(Exception e)
                {
                    Console.WriteLine("\nThere was a problem when downloading {0}: {1}", package.name, e.Message);
                    Console.ReadKey();
                }
            }
            Console.WriteLine("\nInstallion complete.");
            Exit();
        }
        catch(Exception e)
        {
            Console.WriteLine("Encountered exception: {0}", e.Message);
            Exit();
        }
    }

    /// <summary>
    /// Sets installLocation to the Morrowind directory.
    /// </summary>
    /// <returns>
    /// True if a valid directory could be found, otherwise false.
    /// </returns>
    static bool GetInstallLocation(ref string installLocation)
    {
        // First we'll check the current directory.
        if(File.Exists("Morrowind.exe"))
        {
            installLocation = AppDomain.CurrentDomain.BaseDirectory;
            return true;
        }
        // Fall back to the registry.
        else
        {
            string registryValue = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\bethesda softworks\\Morrowind", "Installed Path", null) as string;
            if(!string.IsNullOrEmpty(registryValue) && File.Exists(Path.Combine(registryValue, "Morrowind.exe")))
            {
                installLocation = registryValue;
                return true;
            }
        }

        // They don't seem to have it installed.
        return false;
    }

    static bool Get7ZipLocation(ref string sevenZipLocation)
    {
        // First we'll check the current directory.
        if(File.Exists("7z.exe"))
        {
            installLocation = AppDomain.CurrentDomain.BaseDirectory;
            return true;
        }
        else
        {
            string registryValue = Registry.GetValue("HKEY_CURRENT_USER\\Software\\7-Zip", "Path", null) as string;
            if(!string.IsNullOrEmpty(registryValue) && File.Exists(Path.Combine(registryValue, "7z.exe")))
            {
                sevenZipLocation = registryValue;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extract an archive to destination, overwriting files when necessary.
    /// </summary>
    static bool ExtractArchive(PackageList packageList, Package package)
    {
        var destination = Path.Combine(Path.GetTempPath(), packageList.name, package.name);
        Directory.CreateDirectory(destination);
        var success = false;

        success = Extract(package, destination);
        Console.ReadKey();
        InstallPackage(package, destination);

        Directory.Delete(destination, true);

        return success;
    }

    /// <summary>
    /// Uses 7zip (7z.exe) to extract any sort of archive we encounter.
    /// </summary>
    /// <returns>
    /// True if it didn't throw an exception, otherwise false.
    /// </returns>
    static bool Extract(Package package, string destination)
    {
        try
        {
            ProcessStartInfo p = new ProcessStartInfo
            {
                FileName = Path.Combine(sevenZipLocation, "7z.exe"),
                Arguments = string.Format("x {0} \"-o{1}\"", package.fileName, destination),
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process x = Process.Start(p);
            x.WaitForExit();
            return true;
        }
        catch(System.Exception Ex)
        {
            Console.WriteLine(Ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Determines if Morrowind is currently running in the background.
    /// </summary>
    /// <returns>
    /// True if the process could be found, otherwise false.
    /// </returns>
    static bool IsMorrowindRunning()
    {
        foreach(Process process in Process.GetProcesses())
        {
            if(process.ProcessName.Contains("Morrowind"))
            {
                Console.WriteLine(process.ProcessName);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Filters the package, and figures out how to install it.
    /// </summary>
    static void InstallPackage(Package package, string path)
    {
        DirectoryInfo d = new DirectoryInfo(path);
        foreach(FileInfo f in d.GetFiles("*", System.IO.SearchOption.AllDirectories))
        {
            // Regular extraction, check our filters etc.
            if(CheckFilters(package, f))
            {
                string newPath;
                // If it's an .esp, just put in it Data Files
                if(f.Extension.ToLower() == ".esp")
                {
                    newPath = Path.Combine(installLocation, dataFiles, f.Name);
                }
                // If it has one of the usual directory names, put them and it inside Data Files
                else if(ContainsList(f.FullName, dataFolders))
                {
                    newPath = Path.Combine(installLocation, dataFiles, MatchDataFolder(f.FullName));
                }
                // Otherwise, just try and stuff it in there and see what happens.
                else
                {
                    newPath = Path.Combine(installLocation, Utils.RelativePath(path, f.FullName));
                }

                if(newPath == f.FullName)
                    Console.WriteLine("WHAT THE FUCK"); 

                Console.WriteLine("original path:" + f.FullName);
                Console.WriteLine("new path:" + newPath);

                if(File.Exists(newPath))
                    File.Delete(newPath);

                if(!Directory.Exists(newPath))
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                File.Move(f.FullName, newPath);
            }
            else
            {
                // Handle special extraction if specified
                if(package.specialExtract.Length > 0)
                {
                    Console.Write("\nFalling back to special extraction...\n");
                    if(f.FullName.Contains(package.specialExtract[0]))
                    {
                        string partialPath = Utils.RelativePath(path, package.specialExtract[0]);
                        partialPath = Utils.RelativePath(partialPath, f.FullName);
                        Console.Write("\nPartial path: {0}\n", partialPath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// This handles most of the package installation instructions- blacklisted files/directories, whitelist regex, etc
    /// </summary>
    static bool CheckFilters(Package package, FileInfo file)
    {
        if(package.skimESPs && Path.GetExtension(file.Name).ToLower() != ".esp")
            return false;

        var regex = string.IsNullOrEmpty(package.filterWhitelist) ? new Regex(@"."): new Regex(package.filterWhitelist);
        if(!regex.IsMatch(file.Name))   //if the file doesn't match our whitelist
            return false;

        if(package.fileBlacklist.Contains(file.Name))  //if the file is on our blacklist
            return false;

        foreach(string blacklist in package.directoryBlacklist)
        {
            if(file.DirectoryName.ToLower().Contains(blacklist.ToLower()))  //if the directory the file is in is blacklisted
                return false;
        }

        return true;
    }

    static bool ContainsList(string s, IReadOnlyList<string> list)
    {
        foreach(string b in list)
        {
            if(s.ToLower().Contains(b.ToLower()))
                return true;
        }

        return false;
    }

    static string MatchDataFolder(string path)
    {
        string result = "";
        while(result == "")
        {
            foreach(string dataFolder in dataFolders)
            {
                string regex = string.Format(".*/({0}/.*)$", dataFolder);
                var match = Regex.Match(path, regex, RegexOptions.IgnoreCase);
                if(match.Success)
                {
                    result = match.Groups[0].Value;
                }
            }

        }
        return "pissqueen";
    }

    static public void Exit()
    {
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
        Environment.Exit(0);
    }
}