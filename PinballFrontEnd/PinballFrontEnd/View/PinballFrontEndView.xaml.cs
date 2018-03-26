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
            sourceProvider.MediaPlayer.Log += ((a, b) => { }); //Do nothing with log
            sourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;
            //sourceProvider.MediaPlayer.VideoOutChanged += MediaPlayer_VideoOutChanged;

            //Bind source provider to image
            Playfield.SetBinding(Image.SourceProperty, new Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = sourceProvider });

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP View Model
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            viewModel = new PinballFrontEnd.ViewModel.PinballFrontEndViewModel();
            this.DataContext = viewModel;

            viewModel.TableChanged += ViewModel_TableChanged;

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP Delay Timers
            ///////////////////////////////////////////////////////////////////////////////////////////////////

            //Start Video Update Timer
            vidTimer = new System.Timers.Timer(500); //Delays loading of video to allow fast switching
            vidTimer.Elapsed += VidTimer_Elapsed;

            //CurrentSource = new Uri(".");
            vidTimer.AutoReset = false;
            vidTimer.Start();

            showTimer = new System.Timers.Timer(500);
            showTimer.Elapsed += ShowTimer_Elapsed;
            showTimer.AutoReset = false;
            showTimer.Start();

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // Other
            ///////////////////////////////////////////////////////////////////////////////////////////////////
           
            //Set to software render
            //RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            //Prevent the PC from going into sleep mode (keep monitors on)
            Nutmegz.UnManaged.PowerControl.PreventSleep();
        }

        private void ViewModel_TableChanged(object sender, EventArgs e)
        {
            //Sync to UI thread
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => ViewModel_TableChanged(sender, e));
            }

            Playfield.Visibility = Visibility.Hidden;
            PreloadImage.Visibility = Visibility.Visible;

            vidTimer.Stop();
            vidTimer.Start();
            showTimer.Stop();
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


        #region MediaPlayer Actions

        //Start a video visibility timer when the VLC player start playing
        private void MediaPlayer_Playing(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {
            //Start Visibility Delay
            showTimer.Stop();
            showTimer.Start();
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

            //sourceProvider.MediaPlayer.Play();
            //TestMe.Visibility = Visibility.Visible;
            if (viewModel.CurrentTable != null)
                if (viewModel.CurrentTable.PlayfieldExists)
                {
                    Playfield.Visibility = Visibility.Visible;
                    PreloadImage.Visibility = Visibility.Hidden;
                }
                    
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

   
        private void pfe_ContentRendered(object sender, EventArgs e)
        {
            Focus();
            Nutmegz.UnManaged.WindowControl.LockForground();
            //viewModel.LockItDown();
        }
    }
}
