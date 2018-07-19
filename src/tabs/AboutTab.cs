using System.Windows;

namespace MWInstaller
{
    public partial class MainWindow : Window
    {
        private void hideStartupWarning_Changed(object sender, RoutedEventArgs e)
        {
            Config.hideStartupWarning = hideStartupWarning.IsChecked.GetValueOrDefault();
        }

        private void githubButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Petethegoat/MWInstaller");
        }

        private void openLog_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Log.logPath);
        }
    }
}