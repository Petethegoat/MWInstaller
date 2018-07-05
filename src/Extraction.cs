using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace MWInstaller
{
    static class Extraction
    {
        public static string sevenZipPath;

        /// <summary>
        /// Uses 7zip (7z.exe) to extract any sort of archive we encounter.
        /// </summary>
        /// <returns>
        /// True if it didn't throw an exception.
        /// </returns>
        public static bool Extract(Package package, string destination)
        {
            try
            {
                ProcessStartInfo p = new ProcessStartInfo
                {
                    FileName = Path.Combine(sevenZipPath, "7z.exe"),
                    Arguments = string.Format("x {0} \"-o{1}\"", package.fileName, destination),
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process x = Process.Start(p);
                x.WaitForExit();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public static string GetInstallLocation()
        {
            // First we'll check the current directory.
            if(File.Exists("7z.exe"))
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                string registryValue = Registry.GetValue("HKEY_CURRENT_USER\\Software\\7-Zip", "Path", null) as string;
                if(!string.IsNullOrEmpty(registryValue) && File.Exists(Path.Combine(registryValue, "7z.exe")))
                {
                    return registryValue;
                }
            }

            return "";
        }

        public static bool CheckLocation(string path)
        {
            if(path == "" || path == null)
                return false;
            if(File.Exists(Path.Combine(path, "7z.exe")))
                return true;

            return false;
        }
    }
}
