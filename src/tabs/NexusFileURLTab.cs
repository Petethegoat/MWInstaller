using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace MWInstaller
{
    public partial class MainWindow : Window
    {
        string nexusAPIURL;
        NexusMod currentMod;

        private void getURL_Click(object sender, RoutedEventArgs e)
        {
            nexusAPIURL = Nexus.NexusURLtoAPI(inputURL.Text.Trim());
            string json = Nexus.GetNexusFileList(nexusAPIURL);
            var list = NexusFileList.Deserialize(json);
            fileList.ItemsSource = list.files;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(fileList.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("file_id", ListSortDirection.Descending));

            json = Nexus.GetNexusMod(nexusAPIURL);
            currentMod = NexusMod.Deserialize(json);
        }

        private void fileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(nexusAPIURL != null && e.AddedItems.Count == 1)
            {
                NexusFiles files = e.AddedItems[0] as NexusFiles;
                apiURL.Text = string.Format("{0}/files/{1}/download_link", nexusAPIURL, files.file_id);
            }
        }

        private void clipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(apiURL.Text);
        }

        private void quickPackage_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Package.CreatePackageString(currentMod.name, currentMod.author, apiURL.Text));
        }

        private void apiURL_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(apiURL.Text != "")
            {
                clipboard.IsEnabled = true;
                quickPackage.IsEnabled = true;
            }
        }

        private void inputURL_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(inputURL.Text != "")
            {
                getURL.IsEnabled = true;
            }
        }
    }
}
