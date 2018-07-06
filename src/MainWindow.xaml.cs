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

            Config.LoadConfig();
            PrefillConfiguration();

            Installer.startEvent += installerStart;
            Installer.completeEvent += installerComplete;
            Installer.progressEvent += updateProgressBar;
        }

        private void PrefillConfiguration()
        {
            morrowindLocationTextbox.Text = Morrowind.morrowindPath;
            sevenZipLocationTextbox.Text = Extraction.sevenZipPath;
            packageListLocationTextbox.Text = Config.packageListPath;
            nexusAPIKeyTextBox.Text = Nexus.apiKey;
            nexusAPIKeyButton_Click(null, null);
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
                    CheckInstallability();
                else
                    CheckInstallability(true);
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
            if(File.Exists(listPath))
            {
                packageList = PackageList.Deserialize(File.ReadAllText(listPath));

                PackageListUpdated();

                packageListSuccessTick.Visibility = Visibility.Visible;
            }
            else
            {
                packageListSuccessTick.Visibility = Visibility.Hidden;
            }

            CheckInstallability();
        }

        private void PackageListUpdated()
        {
            packageListTitle.Text = packageList.name;
            packageListCurator.Text = string.Format("Curated by: {0}", packageList.curator);
            packageListDescription.Text = packageList.description;
            packageListUpdated.Text = "Last updated:\n" + packageList.lastUpdated;
            packages = packageList.GetPackages();
            packagesView.ItemsSource = packages;

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
            NexusWindow nexus = new NexusWindow();
            nexus.Show();
        }
    }
}
