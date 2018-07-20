using System.Windows;

namespace MWInstaller
{
    public partial class MainWindow : Window
    {
        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            if(installPopup.Visibility == Visibility.Visible)
            {
                installPopup.Visibility = Visibility.Hidden;
                installReadyImage.Visibility = Visibility.Visible;
            }
            else
            {
                installPopup.Visibility = Visibility.Visible;
                installReadyImage.Visibility = Visibility.Hidden;
                buttonFullInstall.Focus();
            }
        }

        private void buttonFullInstall_Click(object sender, RoutedEventArgs e)
        {
            Installer.doFullInstall = true;
            installProgress.Maximum = Installer.GetProgressBarLength(packages.Count);
            Installer.PerformInstall(packages);
        }

        private void buttonDownloadOnly_Click(object sender, RoutedEventArgs e)
        {
            Installer.doFullInstall = false;
            installProgress.Maximum = Installer.GetProgressBarLength(packages.Count);
            Installer.PerformInstall(packages);
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

        // Installer functions

        private void BeginFullInstall()
        {
            installProgress.Maximum = Installer.GetProgressBarLength(packages.Count);
            Installer.PerformInstall(packages);
        }

        private void updateProgressBar(object sender, object[] args)
        {
            installProgress.Value = (int)args[0];
            installTask.Content = (string)args[1];
        }

        private void installerStart(object sender, System.EventArgs e)
        {
            installButton.IsEnabled = false;
            mainWindow.IsEnabled = false;
        }

        private void installerComplete(object sender, bool success)
        {
            mainWindow.Activate();
            installPopup.Visibility = Visibility.Hidden;

            string message;
            if(success)
                message = "Installation complete.";
            else
                message = "An exception occured. Please check mwinstaller.log for details.";

            if(MessageBox.Show(message) == MessageBoxResult.OK)
            {
                mainWindow.IsEnabled = true;
                installReadyImage.Visibility = Visibility.Hidden;
            }
        }
    }
}