using System.Windows;
using System.IO;
using System.Collections.Generic;

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

            PrefillConfiguration();

            Installer.startEvent += installerStart;
            Installer.completeEvent += installerComplete;
            Installer.progressEvent += updateProgressBar;
        }

        private void PrefillConfiguration()
        {
            morrowindLocationTextbox.Text = Morrowind.GetInstallLocation();
            sevenZipLocationTextbox.Text = Extraction.GetInstallLocation();
            Extraction.sevenZipPath = sevenZipLocationTextbox.Text;
            packageListLocationTextbox.Text = Path.Combine(morrowindLocationTextbox.Text, "packageList.json");
        }

        private void nexusAPIKeyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(nexusAPIKeyTextBox.Text.Length > 0)
                nexusAPIKeyButton.Content = "Validate API Key";
            else
                nexusAPIKeyButton.Content = "Open Nexus";
        }

        private void nexusAPIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if(nexusAPIKeyTextBox.Text.Length > 0)
            {
                var success = Nexus.ValidateAPIKey(nexusAPIKeyTextBox.Text);
                if(success)
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
            }

            CheckInstallability();
        }

        private void PackageListUpdated()
        {
            packageListTitle.Text = packageList.name;
            packageListCurator.Text = string.Format("Curated by: {0}", packageList.curator);
            packageListDescription.Text = packageList.description;
            packages = packageList.GetPackages();
            packagesView.ItemsSource = packages;

            CheckInstallability();
        }

        private void CheckInstallability(bool forceFail = false)
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

            bool showWarnings = (reqNex && !(nexusAPIKeyTextBox.Text.Trim().Length > 0)) || forceFail;

            installButton.IsEnabled = !showWarnings;
            nexusWarning.Visibility = showWarnings ? Visibility.Visible : Visibility.Hidden;
            nexusAPIKeyWarning.Visibility = showWarnings ? Visibility.Visible : Visibility.Hidden;
            installReadyImage.Visibility = !showWarnings ? Visibility.Visible : Visibility.Hidden;
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

            MessageBoxResult result = MessageBox.Show("Installation complete.");
        }
    }
}
