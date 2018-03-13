﻿using System;
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
//using System.Windows.Shapes;
using NLog;
using System.Windows.Media.Animation;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Vlc.DotNet.Wpf;
//using System.Reflection;

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

        //Main Timer to load video
        private System.Timers.Timer vidTimer;
        private System.Timers.Timer showTimer;

        #region PFE

        private string[] mediaOptions;

        private VlcVideoSourceProvider sourceProvider;

        public PinballFrontEndView()
        {
            InitializeComponent(); //Start WPF Components


            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP VLC LIBRARY
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            var currentAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            DirectoryInfo libDirectory;

            // Default libraries are stored here, but they are old, don't use them.
            // We need a better way to install them, see https://github.com/ZeBobo5/Vlc.DotNet/issues/288
            if (IntPtr.Size == 4)
                libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"libvlc\x86\"));
            else
                libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"libvlc\x64\"));

            //Create Video Source Provider
            sourceProvider = new VlcVideoSourceProvider(this.Dispatcher);
            sourceProvider.CreatePlayer(libDirectory);
            sourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;

            //Bind source provider to image
            Playfield.SetBinding(Image.SourceProperty, new Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = sourceProvider });

            mediaOptions = new string[]
            {
                ":input-repeat=2147483647",
                ":no-audio"
            };

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP View Model
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            viewModel = new PinballFrontEnd.ViewModel.PinballFrontEndViewModel();
            this.DataContext = viewModel;

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP Delay Timers
            ///////////////////////////////////////////////////////////////////////////////////////////////////

            //Start Video Update Timer
            vidTimer = new System.Timers.Timer(1000);
            vidTimer.Elapsed += VidTimer_Elapsed;

            //CurrentSource = new Uri(".");
            vidTimer.AutoReset = false;
            vidTimer.Start();

            showTimer = new System.Timers.Timer(1000);
            showTimer.Elapsed += ShowTimer_Elapsed;
            showTimer.AutoReset = false;
            showTimer.Start();

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // Other
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //Setup Error Logger
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Setup FFME
            //Unosquare.FFME.MediaElement.FFmpegDirectory = $@"C:\ffmpeg\bin";

            //Set to software render
            //RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            logger.Trace("Loading PinballFrondEndView");

            this.Focus();

            //Prevent the PC from going into sleep mode (keep monitors on)
            PreventSleep();
        }


        private void pfe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var win = (Window)sender;
            if (win.Visibility == Visibility.Visible)
            {
                //Resume Video
                sourceProvider.MediaPlayer.Play();
                this.Focus();
            }
            else
            {
                sourceProvider.MediaPlayer.Pause();
            }


        }

        private void pfe_LostFocus(object sender, RoutedEventArgs e)
        {
            logger.Error("FOCUS LOST SETTING FOCUS");
            UnManaged.WindowControl.SetFocus("Pinball Front End");
        }

        #endregion

        #region Preload Image Actions

        private void PreloadImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            //vidTimer.
            var im = (Image)sender;
            im.Visibility = Visibility.Visible;
            //Console.WriteLine("Image Changed");

            //TestMe.Source = viewModel.CurrentTable.Playfield;
            //CurrentSource = viewModel.CurrentTable.Playfield;


            //Console.WriteLine("Reset Timer");
            //Media.Stop();
            //Media.Visibility = Visibility.Hidden;
            vidTimer.Stop();
            vidTimer.Start();

            //Console.WriteLine(vidTimer.Enabled);

        }

        #endregion

        #region MediaPlayer Actions

        private void MediaPlayer_Playing(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {
            //Start Visibility Delay
            showTimer.Stop();
            showTimer.Start();
            //Console.WriteLine("Playing");
        }

        #endregion

        #region Delay Timers

        private void ShowTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Check if on UI thread
            if (!CheckAccess())
            {
                //If not on UI thread move to UI thread.
                Dispatcher.Invoke(() => ShowTimer_Elapsed(sender, e));
                return;
            }
            //Console.WriteLine("Visibility Timer Tick");

            //TestMe.Visibility = Visibility.Visible;
            PreloadImage.Visibility = Visibility.Hidden;
        }

        private void VidTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Check if on UI thread
            if (!CheckAccess())
            {
                //If not on UI thread move to UI thread.
                Dispatcher.Invoke(() => VidTimer_Elapsed(sender, e));
                return;
            }

            //this.VlcControl.SourceProvider.MediaPlayer.Play(viewModel.CurrentTable.Playfield, mediaOptions);
            if (viewModel.CurrentTable.Playfield != null && viewModel.CurrentTable.PlayfieldExists)
                sourceProvider.MediaPlayer.Play(viewModel.CurrentTable.Playfield, mediaOptions);
            //Console.WriteLine("RawR");
        }

        #endregion

        #region Global Error Handling

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //logger.ErrorException("Fatal Application Error",(System.Exception)e.ExceptionObject);
            logger.Error((Exception)e.ExceptionObject, "Fatal Application Error");
            MessageBox.Show("Pinball Front End Fatal Error. Check Log File");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        #endregion

        #region Power Override

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


        #endregion

    }
}
