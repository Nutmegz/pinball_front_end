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

        //VLC Source provider (Video Player)
        private VlcVideoSourceProvider sourceProvider;

        public PinballFrontEndView()
        {
            InitializeComponent(); //Start WPF Components

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP VLC LIBRARY
            ///////////////////////////////////////////////////////////////////////////////////////////////////

            //Create Video Source Provider
            sourceProvider = new VlcVideoSourceProvider(this.Dispatcher);
            sourceProvider.CreatePlayer(Model.VlcGlobal.GetVlcLibrary());
            sourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;

            //Bind source provider to image
            Playfield.SetBinding(Image.SourceProperty, new Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = sourceProvider });

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

            //Set to software render
            //RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            //Prevent the PC from going into sleep mode (keep monitors on)
            UnManaged.WindowControl.PreventSleep();
        }

        /// Controls the playback of the video depending if the window is visible or not
        private void pfe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var win = (Window)sender;
            if (win.Visibility == Visibility.Visible)
            {
                Keyboard.Focus(this);
                //Resume Video
                if (viewModel.CurrentTable != null)
                    if (viewModel.CurrentTable.PlayfieldExists)
                        sourceProvider.MediaPlayer.Play();
                //while (!Focus()) ;
                Focus();
            }
            else
            {
                sourceProvider.MediaPlayer.Pause();
            }


        }

        #endregion

        #region Preload Image Actions

        //Show new preload image when loaded and start a play video timer
        private void PreloadImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            //Get Image Handle
            var im = (Image)sender;
            im.Visibility = Visibility.Visible; //Make Preload Image Visiable

            //Reset Video Timer
            vidTimer.Stop();
            vidTimer.Start();
        }

        #endregion

        #region MediaPlayer Actions

        //Start a video visibility timer when the VLC player start playing
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
                try
                {
                    Dispatcher.Invoke(() => ShowTimer_Elapsed(sender, e));
                }
                catch (Exception)
                {
                }

                return;
            }
            //Console.WriteLine("Visibility Timer Tick");

            //TestMe.Visibility = Visibility.Visible;
            if (viewModel.CurrentTable != null)
                if (viewModel.CurrentTable.PlayfieldExists)
                    PreloadImage.Visibility = Visibility.Hidden;
        }

        private void VidTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Check if on UI thread
            if (!CheckAccess())
            {
                //If not on UI thread move to UI thread.
                try
                {
                    Dispatcher.Invoke(() => VidTimer_Elapsed(sender, e));
                }
                catch (Exception)
                {
                }

                return;
            }

            //Check to see if playfield video exists before playing.
            //This speeds up the interface vastly
            if (viewModel.CurrentTable != null)
                if (viewModel.CurrentTable.Playfield != null && viewModel.CurrentTable.PlayfieldExists)
                    sourceProvider.MediaPlayer.Play(viewModel.CurrentTable.Playfield, Model.VlcGlobal.GetVlcArguments());
        }

        #endregion

        #region Global Error Handling

        //Catch all program errors and log the exception.
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //logger.ErrorException("Fatal Application Error",(System.Exception)e.ExceptionObject);
            logger.Error((Exception)e.ExceptionObject, "Fatal Application Error");
            MessageBox.Show("Pinball Front End Fatal Error. Check Log File");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }



        #endregion

        private void pfe_ContentRendered(object sender, EventArgs e)
        {
            Focus();
            UnManaged.WindowControl.LockForground();
            //viewModel.LockItDown();
        }
    }
}
