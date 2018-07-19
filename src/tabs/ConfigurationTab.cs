using System.IO;
using System.Windows;

namespace MWInstaller
{
    public partial class MainWindow : Window
    {
        // Package List
        private void packageListLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(Config.packageListPath)
            };
            if(dialog.ShowDialog(this).GetValueOrDefault())
            {
                packageListLocationTextbox.Text = dialog.FileName;
                packageListLocationTextbox_TextChanged(sender, null);
            }
        }

        private void packageListLocationTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Config.packageListPath = packageListLocationTextbox.Text;
            PackageListUpdated(Config.packageListPath);
        }

        private void packageListRefresh_Click(object sender, RoutedEventArgs e)
        {
            packageListLocationTextbox_TextChanged(sender, null);
        }

        // Morrowind directory
        private void morrowindLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if(dialog.ShowDialog(this).GetValueOrDefault())
            {
                morrowindLocationTextbox.Text = dialog.SelectedPath;
            }
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

        // SevenZip directory
        private void sevenZipLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if(dialog.ShowDialog(this).GetValueOrDefault())
            {
                sevenZipLocationTextbox.Text = dialog.SelectedPath;
                Extraction.sevenZipPath = dialog.SelectedPath;
            }
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

        // Nexus API key
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
                }
            }
            else
            {
                System.Diagnostics.Process.Start("https://www.nexusmods.com/users/myaccount?tab=api+access");
            }
        }
    }
}