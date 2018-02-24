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

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for TableManagerView.xaml
    /// </summary>
    public partial class TableManagerView : Window
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TableManagerView(ObservableCollection<PinballSystem> systems, ObservableCollection<PinballTable> tables, MediaLocation medialoc)
        {
            InitializeComponent();

            var viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel();
            this.DataContext = viewModel;
            viewModel.TableList = tables;
            viewModel.SystemList = systems;
            viewModel.MediaLocation = medialoc;

        }



        //Easy Video Repeat Function
        private void PlayfieldMediaRepeat(object sender, RoutedEventArgs e)
        {
            logger.Trace("Repeating Playfield Media");
            PlayfieldMedia.Position = TimeSpan.Zero;
            PlayfieldMedia.Play();
        }
        private void BackglassMediaRepeat(object sender, RoutedEventArgs e)
        {
            logger.Trace("Repeating Backglass Media");
            BackglassMedia.Position = TimeSpan.Zero;
            BackglassMedia.Play();
        }
        private void DMDMediaRepeat(object sender, RoutedEventArgs e)
        {
            logger.Trace("Repeating DMD Media");
            DMDMedia.Position = TimeSpan.Zero;
            DMDMedia.Play();
        }
        private void WheelMediaRepeat(object sender, RoutedEventArgs e)
        {
            logger.Trace("Repeating Wheel Media");
            WheelMedia.Position = TimeSpan.Zero;
            WheelMedia.Play();
        }

    }
}
