using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace MWInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PackageList packageList;
        List<Package> packages;

        public MainWindow()
        {
            InitializeComponent();

            Log.Clear();
            Log.Write("MW Installer started.\n");

            Installer.Initialize();
            Config.LoadConfig();
            PrefillConfiguration();

            Installer.StartEvent += installerStart;
            Installer.CompleteEvent += installerComplete;
            Installer.ProgressEvent += updateProgressBar;

            if(!Config.hideStartupWarning)
                MessageBox.Show("MW Installer is a work in progress. It should only be used on a fresh, clean installation of Morrowind.");
        }

        private void PrefillConfiguration()
        {
            morrowindLocationTextbox.Text = Morrowind.morrowindPath;
            sevenZipLocationTextbox.Text = Extraction.sevenZipPath;
            packageListLocationTextbox.Text = Config.packageListPath;
            nexusAPIKeyTextBox.Text = Nexus.apiKey;
            hideStartupWarning.IsChecked = Config.hideStartupWarning;

            if(nexusAPIKeyTextBox.Text.Length > 0)
            {
                nexusAPIKeyButton_Click(null, null);
            }
        }

        private void PackageListUpdated(string path)
        {
            if(File.Exists(path))
            {
                packageList = PackageList.Deserialize(File.ReadAllText(path));
                if(packageList != null)
                {
                    packages = packageList.GetPackages();
                    if(packages != null)
                    {
                        packageListTitle.Text = packageList.name;
                        packageListCurator.Text = string.Format("Curated by: {0}", packageList.curator);
                        packageListDescription.Text = packageList.description;
                        packageListUpdated.Text = "Last updated:\n" + packageList.lastUpdated;
                        packagesView.ItemsSource = packages;
                        packageListSuccessTick.Visibility = Visibility.Visible;
                        packageListRefresh.Visibility = Visibility.Hidden;
                        packageTab.IsEnabled = true;
                        CheckInstallability();
                        packageListInfo.Text = string.Empty;
                        return;
                    }
                    else
                    {
                        packageListRefresh.Visibility = Visibility.Visible;
                        packageListInfo.Text = string.Format("{0} could not be deserialized due to bad package URLs. Double check the paths, or notify the curator.", Path.GetFileName(path));
                    }
                }
                else
                {
                    packageListRefresh.Visibility = Visibility.Visible;
                    packageListInfo.Text = string.Format("{0} could not be deserialized due to malformed JSON. Check for corruption, or notify the curator.", Path.GetFileName(path));
                }
            }
            else
            {
                packageListRefresh.Visibility = Visibility.Hidden;
                packageListInfo.Text = "Select a package list to install.";
            }

            // Path didn't exist, or the package list didn't deserialize properly.
            packageTab.IsEnabled = false;
            packageListSuccessTick.Visibility = Visibility.Hidden;
            CheckInstallability();
        }

        private void CheckInstallability(bool forceFail = false)
        {
            bool readyForInstall = true;

            if(!Morrowind.CheckLocation(Morrowind.morrowindPath))
                readyForInstall = false;
            if(!Extraction.CheckLocation(Extraction.sevenZipPath))
                readyForInstall = false;
            if(!File.Exists(packageListLocationTextbox.Text))
                readyForInstall = false;

            bool nexusWarnings = false;
            if(packages != null)
            {
                //Update Nexus requirement
                bool reqNex = false;
                foreach(Package p in packages)
                {
                    if(!p.requiresNexus)
                        continue;
                    reqNex = true;
                    break;
                }

                nexusWarnings = (reqNex && !(nexusAPIKeyTextBox.Text.Trim().Length > 0)) || forceFail;
            }
            else
            {
                readyForInstall = false;
            }

            nexusWarning.Visibility = nexusWarnings ? Visibility.Visible : Visibility.Hidden;
            nexusAPIKeyWarning.Visibility = nexusWarnings ? Visibility.Visible : Visibility.Hidden;
            nexusSuccessTick.Visibility = !nexusWarnings ? Visibility.Visible : Visibility.Hidden;

            installButton.IsEnabled = !nexusWarnings && readyForInstall;
            installReadyImage.Visibility = !nexusWarnings && readyForInstall ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
