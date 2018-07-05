using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ComponentModel;

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
                    InstallPackage(pak);
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
                url = Nexus.GetNexusDownloadURL(pak.fileURL, Nexus.apiKey);

            var webClient = new WebClient();
            webClient.DownloadFile(url, pak.fileName);
        }

        private static void Unpack(Package pak, PackageList list)
        {
            var destination = Path.Combine(Path.GetTempPath(), list.name, pak.name);
            Extraction.Extract(pak, destination);
        }

        private static void InstallPackage(Package pak)
        {

        }

        private static void DeleteArchive(Package pak)
        {
            File.Delete(pak.fileName);
        }
    }
}
