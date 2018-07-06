using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace MWInstaller
{
    /// <summary>
    /// Interaction logic for NexusWindow.xaml
    /// </summary>
    public partial class NexusWindow : Window
    {
        string nexusAPIURL;

        public NexusWindow()
        {
            InitializeComponent();
        }

        private void getURL_Click(object sender, RoutedEventArgs e)
        {
            nexusAPIURL = Nexus.NexusURLtoAPI(inputURL.Text);
            string json = Nexus.GetNexusFileList(nexusAPIURL);
            var list = NexusFileList.Deserialize(json);
            fileList.ItemsSource = list.files;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(fileList.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("file_id", ListSortDirection.Descending));
        }

        private void fileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 1)
            {
                NexusFiles files = e.AddedItems[0] as NexusFiles;
                apiURL.Text = nexusAPIURL + "/" + files.file_id;
            }
        }

        private void clipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(apiURL.Text);
        }

        private void apiURL_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(apiURL.Text != "")
            {
                clipboard.IsEnabled = true;
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
