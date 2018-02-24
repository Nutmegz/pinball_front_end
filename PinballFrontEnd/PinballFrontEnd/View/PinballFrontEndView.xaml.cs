using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Interaction logic for PinballFrontEndView.xaml
    /// </summary>
    public partial class PinballFrontEndView : Window
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();


        //Main View Model
        private PinballFrontEnd.ViewModel.PinballFrontEndViewModel viewModel;

        public PinballFrontEndView()
        {
            InitializeComponent();
            viewModel = new PinballFrontEnd.ViewModel.PinballFrontEndViewModel();
            this.DataContext = viewModel;

            logger.Trace("Loading PinballFrondEndView");

            var BackglassWindow = new BackglassVideoView();
            BackglassWindow.DataContext = viewModel;
            BackglassWindow.Show();

            var DMDWindow = new DMDVideoView();
            DMDWindow.DataContext = viewModel;
            DMDWindow.Show();

            this.Focus();

            PreventSleep();

        }

        //Easy Video Repeat Function
        private void PlayfieldMediaRepeat(object sender, RoutedEventArgs e)
        {
            logger.Trace("Repeating Playfield Media");
            PlayfieldMedia.Position = TimeSpan.Zero;
            PlayfieldMedia.Play();
        }




        /////////////////////////////////////////////////////////////////////////////
        // WINDOWS POWER OVERRIDE
        /////////////////////////////////////////////////////////////////////////////

        //https://stackoverflow.com/questions/241222/need-to-disable-the-screen-saver-screen-locking-in-windows-c-net

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            // Legacy flag, should not be used.
            // ES_USER_PRESENT   = 0x00000004,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
        }

        public static class SleepUtil
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        }

        public void PreventSleep()
        {
            if (SleepUtil.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS
                | EXECUTION_STATE.ES_DISPLAY_REQUIRED
                | EXECUTION_STATE.ES_SYSTEM_REQUIRED
                | EXECUTION_STATE.ES_AWAYMODE_REQUIRED) == 0) //Away mode for Windows >= Vista
                SleepUtil.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS
                    | EXECUTION_STATE.ES_DISPLAY_REQUIRED
                    | EXECUTION_STATE.ES_SYSTEM_REQUIRED); //Windows < Vista, forget away mode
        }

    }
}
