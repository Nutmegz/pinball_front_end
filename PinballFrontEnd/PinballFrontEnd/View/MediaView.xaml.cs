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
    /// <summary>
    /// Interaction logic for MediaView.xaml
    /// </summary>
    public partial class MediaView : Window, INotifyPropertyChanged
    {


        //Main Timer to load video
        private System.Timers.Timer vidTimer;
        private System.Timers.Timer showTimer;

        //VLC
        private VlcVideoSourceProvider sourceProvider;
        private string[] mediaOptions;

        //public Model.PinballTable CurrentTable { get; set; }

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


        public BitmapImage Thumbnail
        {
            get { return (BitmapImage)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        //public static readonly DependencyProperty ThumbnailProperty =
        //    DependencyProperty.Register(
        //        "Thumbnail",
        //        typeof(BitmapImage),
        //        typeof(MediaView),
        //        new PropertyMetadata(null,new PropertyChangedCallback(ThumbnailChanged)));

        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
        "Thumbnail",
        typeof(BitmapImage),
        typeof(MediaView));

        //private void ThumbnailChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    Thumbnail = (BitmapImage)e.NewValue;
        //}

        //private static void ThumbnailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    MediaView mv = d as MediaView;
        //    mv.ThumbnailChanged(e);
        //}

        //public ViewModel.PinballFrontEndViewModel myView { get; set; }
        //private ViewModel.PinballFrontEndViewModel myView;

        public MediaView()
        {
            
            //this.DataContext = this;
            InitializeComponent();


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
            VideoImage.SetBinding(Image.SourceProperty, new Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = sourceProvider });

            mediaOptions = new string[]
            {
                "--input-repeat=2147483647",
                "--no-audio",
                "--no-video-on-top"
            };

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
            Console.WriteLine("Visibility Timer Tick");

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
            if (MediaUri != null && File.Exists(MediaUri.LocalPath))
                sourceProvider.MediaPlayer.Play(MediaUri, mediaOptions);
            Console.WriteLine("RawR");
        }

        #endregion

        #region Preload Image Actions

        private void PreloadImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            //vidTimer.
            var im = (Image)sender;
            im.Visibility = Visibility.Visible;
            Console.WriteLine("Image Changed1");

            //TestMe.Source = viewModel.CurrentTable.Playfield;
            //CurrentSource = viewModel.CurrentTable.Playfield;


            Console.WriteLine("Reset Timer1");
            //Media.Stop();
            //Media.Visibility = Visibility.Hidden;
            vidTimer.Stop();
            vidTimer.Start();

            Console.WriteLine(vidTimer.Enabled);

        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyname)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        //Stop and Play videos when hiding window to free up resources.
        private void MediaViewWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var wnd = (Window)sender;

            if(wnd.Visibility == Visibility.Visible)
            {
                sourceProvider.MediaPlayer.Play();
            } else
            {
                sourceProvider.MediaPlayer.Pause();
            }
        }
    }
}
