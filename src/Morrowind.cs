using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace MWInstaller
{
    static class Morrowind
    {
        public static string morrowindPath;
        public const string dataFiles = "Data Files";
        public static string dataFoldersRegex = "bookart|distantland|docs|fonts|icons|meshes|music|mwse|shaders|sound|splash|textures|video";

        /// <summary>
        /// Determines if Morrowind is currently running in the background.
        /// </summary>
        /// <returns>
        /// True if the process could be found, otherwise false.
        /// </returns>
        public static bool IsMorrowindRunning()
        {
            foreach(Process process in Process.GetProcesses())
            {
                if(process.ProcessName.Contains("Morrowind"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets installLocation to the Morrowind directory.
        /// </summary>
        /// <returns>
        /// True if a valid directory could be found, otherwise false.
        /// </returns>
        public static string GetInstallLocation()
        {
            // First we'll check the current directory.
            if(File.Exists("Morrowind.exe"))
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            // Fall back to the registry.
            else
            {
                string registryValue = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\bethesda softworks\\Morrowind", "Installed Path", null) as string;
                if(!string.IsNullOrEmpty(registryValue) && File.Exists(Path.Combine(registryValue, "Morrowind.exe")))
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
            if(File.Exists(Path.Combine(path, "Morrowind.exe")))
                return true;

            return false;
        }
    }
}
