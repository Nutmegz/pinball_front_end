using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using NLog;
using PinballFrontEnd.Model;
using System.Collections.ObjectModel;
using Vlc.DotNet.Wpf;
using MahApps.Metro.Controls;

//TODO: Conform with MVVM and move most code into the view model.

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for TableManagerView.xaml
    /// </summary>
    public partial class TableManagerView : MetroWindow
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ViewModel.TableManagerViewModel viewModel;
        private string databasepath = "";

        //Source Providers for the VLC players
        private VlcVideoSourceProvider playfieldSourceProvider;
        private VlcVideoSourceProvider backglassSourceProvider;
        private VlcVideoSourceProvider dmdSourceProvider;

        public TableManagerView(string databasepath)
        {
            InitializeComponent();

            this.databasepath = databasepath;
            viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel(this.databasepath);
            this.DataContext = viewModel;
            //SubscribeToViewModelEvents();
            //SetupVlcSourceProvider();
        }

        public TableManagerView(PinballData data)
        {
            InitializeComponent();

            viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel(data);
            this.DataContext = viewModel;
            //SubscribeToViewModelEvents();
            //SetupVlcSourceProvider();
        }

        

        private void SubscribeToViewModelEvents()
        {
            if(viewModel != null)
            {
                viewModel.MediaImportStarted += ViewModel_MediaImportStarted;
                viewModel.MediaImportFinished += ViewModel_MediaImportFinished;
            }
        }

        private void SetupVlcSourceProvider()
        {
            //Create Video Source Provider
            playfieldSourceProvider = new VlcVideoSourceProvider(this.Dispatcher);
            playfieldSourceProvider.CreatePlayer(Model.VlcGlobal.GetVlcLibrary());

            //Bind source provider to image
            //PlayfieldMedia.SetBinding(Image.SourceProperty, new System.Windows.Data.Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = playfieldSourceProvider });

            //Create Video Source Provider
            backglassSourceProvider = new VlcVideoSourceProvider(this.Dispatcher);
            backglassSourceProvider.CreatePlayer(Model.VlcGlobal.GetVlcLibrary());

            //Bind source provider to image
            //BackglassMedia.SetBinding(Image.SourceProperty, new System.Windows.Data.Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = backglassSourceProvider });

            //Create Video Source Provider
            dmdSourceProvider = new VlcVideoSourceProvider(this.Dispatcher);
            dmdSourceProvider.CreatePlayer(Model.VlcGlobal.GetVlcLibrary());

            //Bind source provider to image
            //DMDMedia.SetBinding(Image.SourceProperty, new System.Windows.Data.Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = dmdSourceProvider });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (databasepath != "")
                viewModel.Data.SaveDatabase(this.databasepath);
        }

        private void TableSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dg = (System.Windows.Controls.DataGrid)sender;

            //Console.WriteLine(dg.SelectedItem.ToString());
            if (dg.SelectedItem != null && dg.SelectedItem.ToString() != "{NewItemPlaceholder}")
            {
                var si = (PinballTable)dg.SelectedItem;




                StartMedia(playfieldSourceProvider, si.Playfield);
                StartMedia(backglassSourceProvider, si.Backglass);
                StartMedia(dmdSourceProvider, si.DMD);

                /*
                if (si.PlayfieldExists)
                    PlayfieldMedia.Visibility = Visibility.Visible;
                else
                    PlayfieldMedia.Visibility = Visibility.Hidden;
                if (si.BackglassExists)
                    BackglassMedia.Visibility = Visibility.Visible;
                else
                    BackglassMedia.Visibility = Visibility.Hidden;
                if (si.DMDExists)
                    DMDMedia.Visibility = Visibility.Visible;
                else
                    DMDMedia.Visibility = Visibility.Hidden;
                    */
            }

        }

        private void ViewModel_MediaImportFinished(object sender, PinballTable table)
        {
            //Check if on UI thread
            if (!CheckAccess())
            {
                //If not on UI thread move to UI thread.
                Dispatcher.Invoke(() => ViewModel_MediaImportStarted(sender, table));
                return;
            }
            //TableSelect.SelectedIndex = TableSelect.SelectedIndex;
            //TableSelect.SelectedIndex = tableSelectedIndex;
            StartMedia(playfieldSourceProvider, viewModel.SelectedTable.Playfield);
            StartMedia(backglassSourceProvider, viewModel.SelectedTable.Backglass);
            StartMedia(dmdSourceProvider, viewModel.SelectedTable.DMD);
            
        }

        private void ViewModel_MediaImportStarted(object sender, PinballTable table)
        {
            //Check if on UI thread
            if (!CheckAccess())
            {
                //If not on UI thread move to UI thread.
                Dispatcher.Invoke(() => ViewModel_MediaImportStarted(sender, table));
                return;
            }
            
            StopMedia(playfieldSourceProvider);
            StopMedia(backglassSourceProvider);
            StopMedia(dmdSourceProvider);
            //TableSelect.SelectedIndex = TableSelect.Items.Count;
        }

        private void StartMedia(VlcVideoSourceProvider sp, Uri media)
        {
            if (media != null && File.Exists(media.LocalPath) && sp != null)
                sp.MediaPlayer.Play(media, Model.VlcGlobal.GetVlcArguments());
        }

        private void StopMedia(VlcVideoSourceProvider sp)
        {
            //sp.MediaPlayer.Stop();
            sp.MediaPlayer.Play(new Uri("C:\\"),VlcGlobal.GetVlcArguments());

        }
    }
}
