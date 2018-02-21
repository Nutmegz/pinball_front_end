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
//using DesktopWPFAppLowLevelKeyboardHook;

namespace PinballFrontEnd.View
{
    /// <summary>
    /// Interaction logic for TableManagerView.xaml
    /// </summary>
    public partial class TableManagerView : Window
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TableManagerView()
        {
            InitializeComponent();

            var viewModel = new PinballFrontEnd.ViewModel.TableManagerViewModel();
            this.DataContext = viewModel;


            //Start keyboard listener
            //_listener = new LowLevelKeyboardListener();
            //_listener.OnKeyPressed += _listener_OnKeyPressed;
            //_listener.HookKeyboard();
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






        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////

        //private LowLevelKeyboardListener _listener;

        //void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        //{
        //    Console.WriteLine(e.KeyPressed.ToString());
        //}
        //////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////

    }
}
