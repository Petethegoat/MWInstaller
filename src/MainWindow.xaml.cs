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

        NexusWindow nexusWindow;

        public MainWindow()
        {
            InitializeComponent();

            Log.Clear();
            Log.Write("MW Installer started.\n");

            Config.LoadConfig();
            PrefillConfiguration();

            Installer.startEvent += installerStart;
            Installer.completeEvent += installerComplete;
            Installer.progressEvent += updateProgressBar;

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

        private void nexusAPIKeyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(nexusAPIKeyTextBox.Text.Length > 0)
                nexusAPIKeyButton.Content = "Validate";
            else
                nexusAPIKeyButton.Content = "Open Nexus";
        }

        private void nexusAPIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if(nexusAPIKeyTextBox.Text.Length > 0)
            {
                if(Nexus.ValidateAPIKey(nexusAPIKeyTextBox.Text))
                {
                    CheckInstallability();
                    creatorTab.IsEnabled = true;
                }
                else
                {
                    CheckInstallability(true);
                    creatorTab.IsEnabled = false;
                    if(nexusWindow != null)
                    {
                        nexusWindow.Close();
                    }
                }
            }
            else
            {
                System.Diagnostics.Process.Start("https://www.nexusmods.com/users/myaccount?tab=api+access");
            }
        }

        private void packageListLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if(dialog.ShowDialog(this).GetValueOrDefault())
            {
                packageListLocationTextbox.Text = dialog.FileName;
                packageListLocationTextbox_TextChanged(sender, null);
            }
        }

        private void morrowindLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if(dialog.ShowDialog(this).GetValueOrDefault())
            {
                morrowindLocationTextbox.Text = dialog.SelectedPath;
            }
        }

        private void sevenZipLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if(dialog.ShowDialog(this).GetValueOrDefault())
            {
                sevenZipLocationTextbox.Text = dialog.SelectedPath;
                Extraction.sevenZipPath = dialog.SelectedPath;
            }
        }

        private void packageListLocationTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var listPath = packageListLocationTextbox.Text;
            PackageListUpdated(listPath);
        }

        private void PackageListUpdated(string path)
        {
            if(File.Exists(path))
            {
                packageList = PackageList.Deserialize(File.ReadAllText(path));
                if(packageList != null)
                {
                    packageListTitle.Text = packageList.name;
                    packageListCurator.Text = string.Format("Curated by: {0}", packageList.curator);
                    packageListDescription.Text = packageList.description;
                    packageListUpdated.Text = "Last updated:\n" + packageList.lastUpdated;
                    packages = packageList.GetPackages();
                    packagesView.ItemsSource = packages;
                    packageListSuccessTick.Visibility = Visibility.Visible;
                    packageListRefresh.Visibility = Visibility.Hidden;
                    packageTab.IsEnabled = true;
                    CheckInstallability();
                    packageListInfo.Text = string.Empty;
                    return;
                }

                packageListRefresh.Visibility = Visibility.Visible;
                packageListInfo.Text = string.Format("{0} could not be deserialized due to malformed JSON. Check for corruption, or notify the curator.", Path.GetFileName(path));
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

        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            installProgress.Maximum = packageList.packages.Length;
            Installer.PerformInstall(packages, packageList);
        }

        private void updateProgressBar(object sender, int percentage)
        {
            installProgress.Value = percentage;
        }

        private void installerStart(object sender, System.EventArgs e)
        {
            installButton.IsEnabled = false;
        }

        private void installerComplete(object sender, System.EventArgs e)
        {
            installReadyImage.Visibility = Visibility.Hidden;

            MessageBox.Show("Installation complete.");
        }

        private void sevenZipLocationTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Extraction.sevenZipPath = sevenZipLocationTextbox.Text;

            if(Extraction.CheckLocation(Extraction.sevenZipPath))
                sevenZipSuccessTick.Visibility = Visibility.Visible;
            else
                sevenZipSuccessTick.Visibility = Visibility.Hidden;

            CheckInstallability();
        }

        private void morrowindLocationTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Morrowind.morrowindPath = morrowindLocationTextbox.Text;

            if(Morrowind.CheckLocation(Morrowind.morrowindPath))
                morrowindSuccessTick.Visibility = Visibility.Visible;
            else
                morrowindSuccessTick.Visibility = Visibility.Hidden;

            CheckInstallability();
        }

        private void creatorButton_Click(object sender, RoutedEventArgs e)
        {
            nexusWindow = new NexusWindow();
            nexusWindow.Show();
        }

        private void packagesView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var p = packagesView.SelectedItem as Package;
            Log.Write("\n" + p.name + "\n");
            if(p.malformed)
            {
                packages = packageList.GetPackages();
                packagesView.ItemsSource = packages;
                packagesView.UpdateLayout();
            }
        }

        private void githubButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Petethegoat/MWInstaller");
        }

        private void openLog_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Log.logPath);
        }

        private void packageListRefresh_Click(object sender, RoutedEventArgs e)
        {
            packageListLocationTextbox_TextChanged(sender, null);
        }

        private void hideStartupWarning_Changed(object sender, RoutedEventArgs e)
        {
            Config.hideStartupWarning = hideStartupWarning.IsChecked.GetValueOrDefault();
        }
    }
}
