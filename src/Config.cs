using Microsoft.Win32;
using System.IO;

namespace MWInstaller
{
    // We store a few things in the registry.
    // It's probably not optimal but I like it more than having yet another config file floating around.
    public class Config
    {
        static public string packageListPath;
        static public bool hideStartupWarning;

        const string K_morrowindPath = "morrowindPath";
        const string K_packageListPath = "packageListPath";
        const string K_sevenZipPath = "sevenZipPath";
        const string K_nexusAPIKey = "nexusAPIKey";
        const string K_hideStartupWarning = "hideStartupWarning";

        private const string registry = "SOFTWARE\\MWInstaller";

        public static void SaveConfig()
        {
            Log.Write("\nSaving config...");
            SetRegistryValue(K_morrowindPath, Morrowind.morrowindPath);
            SetRegistryValue(K_packageListPath, Config.packageListPath);
            SetRegistryValue(K_sevenZipPath, Extraction.sevenZipPath);
            SetRegistryValue(K_nexusAPIKey, Nexus.apiKey);
            SetRegistryValue(K_hideStartupWarning, Config.hideStartupWarning.ToString());
            Log.Write(" done.");
        }

        public static void LoadConfig()
        {
            Log.Write("\nLoading config...");
            string reg;
            reg = GetRegistryValue(K_morrowindPath);
            Morrowind.morrowindPath = reg == "" ? Morrowind.GetInstallLocation() : reg;

            reg = GetRegistryValue(K_packageListPath);
            Config.packageListPath = reg == "" ? Path.Combine(Morrowind.morrowindPath, "packageList.json") : reg;

            reg = GetRegistryValue(K_sevenZipPath);
            Extraction.sevenZipPath = reg == "" ? Extraction.GetInstallLocation() : reg;

            reg = GetRegistryValue(K_nexusAPIKey);
            Nexus.apiKey = reg;

            Config.hideStartupWarning = GetRegistryBool(K_hideStartupWarning);
            Log.Write(" done.");
        }

        private static string GetRegistryValue(string key)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(registry);
            rk = Registry.CurrentUser.OpenSubKey(registry, false);
            if(rk != null)
            {
                object o = rk.GetValue(key);
                if(o != null)
                    return o.ToString();
            }
            return "";
        }

        private static bool GetRegistryBool(string key)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(registry);
            rk = Registry.CurrentUser.OpenSubKey(registry, false);
            if(rk != null)
            {
                object o = rk.GetValue(key);
                if(o != null)
                    return bool.Parse(o.ToString());
            }
            return false;
        }

        private static void SetRegistryValue(string key, string value)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(registry);
            rk = Registry.CurrentUser.OpenSubKey(registry, true);
            if(rk != null)
            {
                rk.SetValue(key, value);
            }
        }
    }
}
