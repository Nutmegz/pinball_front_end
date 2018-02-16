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

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for TableManagerView.xaml
    /// </summary>
    public partial class TableManagerView : Window
    {
        public TableManagerView()
        {
            InitializeComponent();

            var viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel();
            this.DataContext = viewModel;

        }

        private void PlayfieldMediaRepeat(object sender, RoutedEventArgs e)
        {
            PlayfieldMedia.Position = TimeSpan.Zero;
            PlayfieldMedia.Play();
        }
        private void BackglassMediaRepeat(object sender, RoutedEventArgs e)
        {
            BackglassMedia.Position = TimeSpan.Zero;
            BackglassMedia.Play();
        }
        private void DMDMediaRepeat(object sender, RoutedEventArgs e)
        {
            DMDMedia.Position = TimeSpan.Zero;
            DMDMedia.Play();
        }
        private void WheelMediaRepeat(object sender, RoutedEventArgs e)
        {
            WheelMedia.Position = TimeSpan.Zero;
            WheelMedia.Play();
        }

    }
}
