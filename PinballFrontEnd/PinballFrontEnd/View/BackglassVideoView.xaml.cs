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
using NLog;

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for BackglassVideoView.xaml
    /// </summary>
    public partial class BackglassVideoView : Window
    {
        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BackglassVideoView()
        {
            InitializeComponent();
        }


        //Easy Video Repeat Function
        private void BackglassMediaRepeat(object sender, RoutedEventArgs e)
        {
            logger.Trace("Repeating Backglass Media");
            BackglassMedia.Position = TimeSpan.Zero;
            BackglassMedia.Play();
        }

    }
}
