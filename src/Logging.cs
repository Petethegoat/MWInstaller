using System.IO;
using System.Diagnostics;

namespace MWInstaller
{
    class Log
    {
        const string logPath = "mwinstaller.log";

        public static void Clear()
        {
            File.WriteAllText(logPath, string.Empty);
        }

        public static void Write(string log)
        {
            File.AppendAllText(logPath, log, System.Text.Encoding.UTF8);
        }

        public static void Write(System.Exception e)
        {
            //string log = string.Format(e.)
            File.AppendAllText(logPath, string.Format("\nEXCEPTION:\n{0}{1}", e.StackTrace, e.Message), System.Text.Encoding.UTF8);
        }
    }
}
