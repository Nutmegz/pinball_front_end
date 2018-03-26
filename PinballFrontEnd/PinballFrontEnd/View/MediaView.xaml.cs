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
//using System.Windows.Shapes;
using System.Threading;
using System.IO;
using Vlc.DotNet.Wpf;
using System.ComponentModel;

namespace PinballFrontEnd.View
{

    public partial class MediaView : Window
    {


        //Main Timer to load video
        private System.Timers.Timer vidTimer;
        private System.Timers.Timer showTimer;

        //VLC
        private VlcVideoSourceProvider sourceProvider;

        public Uri MediaUri
        {
            get { return (Uri)GetValue(MediaUriProperty); }
            set { SetValue(MediaUriProperty, value); }
        }

        public static readonly DependencyProperty MediaUriProperty =
            DependencyProperty.Register(
                "MediaUri",
                typeof(Uri),
                typeof(MediaView));




        public ViewModel.PinballFrontEndViewModel viewModel { get; set; }

        public MediaView(ViewModel.PinballFrontEndViewModel viewModel)
        {

            //this.DataContext = this;
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;

            this.viewModel.TableChanged += ViewModel_TableChanged;

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            // SETUP Delay Timers
            ///////////////////////////////////////////////////////////////////////////////////////////////////

            //Start Video Update Timer
            vidTimer = new System.Timers.Timer(500);
            vidTimer.Elapsed += VidTimer_Elapsed;

            //CurrentSource = new Uri(".");
            vidTimer.AutoReset = false;
            vidTimer.Start();

            showTimer = new System.Timers.Timer(500);
            showTimer.Elapsed += ShowTimer_Elapsed;
            showTimer.AutoReset = false;
            showTimer.Start();



            // SETUP VLC LIBRARY

            //Create Video Source Provider
            sourceProvider = new VlcVideoSourceProvider(this.Dispatcher);
            sourceProvider.CreatePlayer(Model.VlcGlobal.GetVlcLibrary());
            sourceProvider.MediaPlayer.Log += ((a, b) => { }); //Do nothing with log
            sourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;

            //Bind source provider to image
            VideoImage.SetBinding(Image.SourceProperty, new Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = sourceProvider });
        }

        private void ViewModel_TableChanged(object sender, EventArgs e)
        {
            //Sync to UI thread
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => ViewModel_TableChanged(sender, e));
            }

            VideoImage.Visibility = Visibility.Hidden;
            PreloadImage.Visibility = Visibility.Visible;

            vidTimer.Stop();
            vidTimer.Start();
            showTimer.Stop();
        }

        private void MediaPlayer_Playing(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {
            //Start Visibility Delay
            showTimer.Stop();
            showTimer.Start();
            //Console.WriteLine("Playing");
        }


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
            if (MediaUri != null)
                if (File.Exists(MediaUri.LocalPath))
                {
                    VideoImage.Visibility = Visibility.Visible;
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

                    //throw;
                }

                return;
            }

            if (MediaUri != null && File.Exists(MediaUri.LocalPath))
                sourceProvider.MediaPlayer.Play(MediaUri, Model.VlcGlobal.GetVlcArguments());
        }

        #endregion

        #region Preload Image Actions

        private void PreloadImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            //vidTimer.
            var im = (Image)sender;
            im.Visibility = Visibility.Visible;

            vidTimer.Stop();
            vidTimer.Start();

            Console.WriteLine(vidTimer.Enabled);

        }

        #endregion


        //Stop and Play videos when hiding window to free up resources.
        private void MediaViewWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var wnd = (Window)sender;

            if (wnd.Visibility == Visibility.Visible)
            {
                if (MediaUri != null)
                    if (File.Exists(MediaUri.LocalPath))
                        sourceProvider.MediaPlayer.Play();
            }
            else
            {
                sourceProvider.MediaPlayer.Pause();
            }
        }
    }
}
