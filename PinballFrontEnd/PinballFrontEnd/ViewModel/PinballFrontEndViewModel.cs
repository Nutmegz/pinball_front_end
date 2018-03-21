using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using NLog;
using System.Diagnostics;
using System.Collections.ObjectModel;
using PinballFrontEnd.Model;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.Windows.Data;
using UnManaged;
using System.Windows.Threading;
using System.IO;

namespace PinballFrontEnd.ViewModel
{

    //Mode the front end is running at
    public enum Mode
    {
        SystemRunning,
        FrontEnd,
        Manager
    }

    public class PinballFrontEndViewModel : ViewModelBase
    {

        public static string ProgramName { get; set; } = "PFE Main Window";

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //Database Path
        private static string databasepath = $@"{ProgramPath.Value}database.json";

        //Tell Windows to be on top
        public bool TopMost { get; set; } = true;

        //Keybinding Statemachine
        public Mode RunMode { get; set; } = Mode.FrontEnd; // Start in front end mode

        //Table Manager Handle
        private View.TableManagerView tableManager;

        //Currently viewed table handle
        public PinballTable CurrentTable { get; set; } //Table where all media is played

        //Addon windows for the Backglass and DMD display
        public View.MediaView BackglassWindow { get; set; }
        public View.MediaView DMDWindow { get; set; }

        //Visibilty property for main window
        public Visibility PlayfieldVisibility { get; set; } = Visibility.Visible;

        //Data
        public PinballData Data { get; set; } = new PinballData();

        //Global Keyboard Listener Hook
        private LowLevelKeyboardListener _listener;

        //Default Constructor
        public PinballFrontEndViewModel()
        {


            logger.Info($"Starting Pinball Front End: {Model.ProgramPath.Value}");

            Data = new PinballData();
            Data.LoadDatabase(databasepath);

            //InitializePlayers();
            //LoadDatabase();

            InitializeKeyboardHook();
            InitalizeThumbnails();
            InitializeBGMusic();
            RandomTable();
            InitalizeWindows();


            //StartEverything();
        }

        private async void StartEverything()
        {
            Data = new PinballData();
            await Task.Run(() => Data.LoadDatabase(databasepath));

            //InitializePlayers();
            //LoadDatabase();
            await Task.Run(() => InitializeKeyboardHook());
            await Task.Run(() => InitalizeThumbnails());
            await Task.Run(() => InitializeBGMusic());
            RandomTable();
            InitalizeWindows();
        }

        //Startup addon windows
        private void InitalizeWindows()
        {

            //BGMusicPlayer.IsMuted = false;

            //Backglass Window
            BackglassWindow = new View.MediaView();
            BackglassWindow.WindowStyle = WindowStyle.None;
            BackglassWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            BackglassWindow.ResizeMode = ResizeMode.NoResize;

            BindWindow(BackglassWindow, Window.TopProperty, this, "Data.MediaLocation.BackglassLocationY", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.LeftProperty, this, "Data.MediaLocation.BackglassLocationX", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.HeightProperty, this, "Data.MediaLocation.BackglassSizeY", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.WidthProperty, this, "Data.MediaLocation.BackglassSizeX", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.TopmostProperty, this, "TopMost", BindingMode.TwoWay);
            BindWindow(BackglassWindow, View.MediaView.MediaUriProperty, this, "CurrentTable.Backglass", BindingMode.OneWay);
            BindWindow(BackglassWindow.PreloadImage, System.Windows.Controls.Image.SourceProperty, this, "CurrentTable.BackglassThumbnail", BindingMode.OneWay);

            BackglassWindow.Show();

            //DMD Window
            DMDWindow = new View.MediaView();
            DMDWindow.WindowStyle = WindowStyle.None;
            DMDWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            DMDWindow.ResizeMode = ResizeMode.NoResize;

            BindWindow(DMDWindow, Window.TopProperty, this, "Data.MediaLocation.DMDLocationY", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.LeftProperty, this, "Data.MediaLocation.DMDLocationX", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.HeightProperty, this, "Data.MediaLocation.DMDSizeY", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.WidthProperty, this, "Data.MediaLocation.DMDSizeX", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.TopmostProperty, this, "TopMost", BindingMode.TwoWay);
            BindWindow(DMDWindow, View.MediaView.MediaUriProperty, this, "CurrentTable.DMD", BindingMode.OneWay);
            BindWindow(DMDWindow.PreloadImage, System.Windows.Controls.Image.SourceProperty, this, "CurrentTable.DMDThumbnail", BindingMode.OneWay);

            DMDWindow.Show();

        }

        #region Window Property Binding

        //Set Databinding
        private void BindWindow(System.Windows.Controls.Image window, DependencyProperty property, Object source, String propertyPath, BindingMode mode)
        {



            Binding myBinding = new Binding()
            {
                Source = source,
                Path = new PropertyPath(propertyPath),
                Mode = mode,
                NotifyOnSourceUpdated = true,
                NotifyOnTargetUpdated = true
            };
            BindingOperations.SetBinding(window, property, myBinding);
        }

        private void BindWindow(Window window, DependencyProperty property, Object source, String propertyPath, BindingMode mode)
        {



            Binding myBinding = new Binding()
            {
                Source = source,
                Path = new PropertyPath(propertyPath),
                Mode = mode
            };
            BindingOperations.SetBinding(window, property, myBinding);
        }




        #endregion

        private void InitializeKeyboardHook()
        {
            //Start Keyboard Listener (Removed Keylogger)
            _listener = new LowLevelKeyboardListener();        //New Hook Object
            _listener.OnKeyPressed += KeyboardListner_OnKeyPressed;  //Subscribe to Event
            _listener.HookKeyboard();                          //Start Hook
        }

        private void StartTableManager()
        {
            logger.Info($"{RunMode.ToString()} : Starting Table Manager");
            RunMode = Mode.Manager;
            logger.Trace($"Setting Mode to {RunMode.ToString()}");

            BackglassWindow.Visibility = Visibility.Hidden;
            DMDWindow.Visibility = Visibility.Hidden;
            PlayfieldVisibility = Visibility.Hidden;
            StopBGMusic();

            TopMost = false;
            tableManager = new View.TableManagerView(Data);
            
            //Show taskbar and cursor
            WindowControl.ShowTaskbar();
            WindowControl.ShowCursor();

            //Start window
            tableManager.Show();
        }

        private async void ExitTableManager()
        {
            Data.SortSystemsTables();
            logger.Trace($"{RunMode.ToString()} : Exit Table Manager");

            BackglassWindow.Visibility = Visibility.Visible;
            DMDWindow.Visibility = Visibility.Visible;
            PlayfieldVisibility = Visibility.Visible;
            ResumeBGMusic();


            tableManager.Close();
            TopMost = true;

            //Focus PFE Main Window
            await Task.Run(() => WindowControl.SetFocusForeground(WindowControl.FindProcessWindow(Process.GetCurrentProcess().ProcessName, ProgramName),true));

            //Hide taskbar and cursor
            WindowControl.HideTaskbar();
            WindowControl.HideCursor();

            RunMode = Mode.FrontEnd;
            logger.Trace($"Setting Mode to {RunMode.ToString()}");
        }

        private async void StartTable(int mainThread)
        {
            logger.Info($"{RunMode.ToString()}: Starting table: {CurrentTable.Name}");
            RunMode = Mode.SystemRunning;
            logger.Trace($"Setting Mode to {RunMode.ToString()}");

            //Play Launch Music
            LMusicPlayer.Open(CurrentTable.LMusic);
            LMusicPlayer.Play();

            var system = Data.FindSystem(CurrentTable);           // get system to launch
            PinballFunctions.StartTable(system, CurrentTable);    // launch system



            Stopwatch watchdog = new Stopwatch();
            watchdog.Reset();
            watchdog.Start();


            //Wait for process to start
            logger.Info($"Starting Game Process, Name: {system.Name}");
            await Task.Run(() => { while (Process.GetProcessesByName(system.Name).Length <= 0) ; } );
            var game = Process.GetProcessesByName(system.Name).First();
            logger.Info($"Game Process Found, Handle: {game.Id}");

            //keep hiding while window is starting
            await Task.Run(() =>
            {
                do
                {
                    WindowControl.HideAllProcessWindows(system.Name);
                } while (WindowControl.FindProcessWindow(system.Name, system.WindowName) == IntPtr.Zero);
                WindowControl.HideAllProcessWindows(system.Name);
            });
            var windowHandle = WindowControl.FindProcessWindow(system.Name, system.WindowName);
            logger.Info($"Game Window Found, Process: {system.Name}, Name: {system.WindowName}, Handle: {windowHandle}");
            





            //Try and Mute System
            watchdog.Restart();
            logger.Info("Muting System");
            await Task.Run(() =>
            {
                while (VolumeMixer.GetApplicationMute(game.Id) != true)
                {
                    //Console.WriteLine("Trying to Mute");
                    VolumeMixer.SetApplicationMute(game.Id, true);
                    //WindowControl.SetFocus(ProgramName);
                    if (watchdog.ElapsedMilliseconds > 10000)
                    {
                        logger.Warn($"Can't Mute System, Process ID: {game.Id}");
                        break;
                    }
                }
            });


           

            //Wait for Game to load
            await Task.Run(() => Task.Delay(system.WaitTime * 1000));
            BGMusicPlayer.Pause();

            //Unmute
            watchdog.Restart();
            logger.Info("Unmuting System");
            while (VolumeMixer.GetApplicationMute(game.Id) != false)
            {
                //Console.WriteLine("Trying to Unmute");
                VolumeMixer.SetApplicationMute(game.Id, false);
                //WindowControl.SetFocus(ProgramName);
                if (watchdog.ElapsedMilliseconds > 10000)
                {
                    logger.Warn($"Can't Unmute System, Process ID: {game.Id}");
                    break;
                }
            }


            //Show Game Window
            WindowControl.ShowWindow(windowHandle, true);
         
            //Focus GAME
            WindowControl.SetFocusForeground(windowHandle, true);

            //Hide Selected Windows
            logger.Trace("Setting Window Visibility");
            BackglassWindow.Visibility = CurrentTable.ShowBackglass ? Visibility.Visible : Visibility.Hidden;
            DMDWindow.Visibility = CurrentTable.ShowDMD ? Visibility.Visible : Visibility.Hidden;
            PlayfieldVisibility = Visibility.Hidden;

        }

        private async void ExitTable()
        {
            logger.Info($"{RunMode.ToString()} : Exit System");
            try
            {

                //Show Media (must show before exiting system otherwise there is a focus issue)
                BackglassWindow.Visibility = Visibility.Visible;
                DMDWindow.Visibility = Visibility.Visible;
                PlayfieldVisibility = Visibility.Visible; //must be last one shown


                var proc = WindowControl.FindProcess(Data.FindSystem(CurrentTable).Name);
                proc.CloseMainWindow();
                await Task.Run(() => proc.WaitForExit(2000)); //Give the window 2 seconds the exit gracefully
                if (!proc.HasExited)
                    proc.Kill(); //Force close

                //Resume Music
                ResumeBGMusic();

                WindowControl.SetFocusForeground(ProgramName);
                logger.Info($"Foreground Window: {WindowControl.GetForegroundWindow()}");
                logger.Info($"Foreground Title: {WindowControl.GetActiveWindowTitle()}");

                RunMode = Mode.FrontEnd;
                logger.Trace($"Setting Mode to {RunMode.ToString()}");

            }
            catch (Exception)
            {

                throw;
            }
        }

        //Handle Keyboard Message
        private void KeyboardListner_OnKeyPressed(object sender, KeyPressedArgs e)
        {

            //Keep window in focus always when in Front End Mode
            if (RunMode == Mode.FrontEnd)
            {
                //WindowControl.UnlockForground();
                //WindowControl.SetFocusForeground(ProgramName);
                //WindowControl.LockForground();
            }


            //EXIT KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_EXIT)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        ExitFrontEnd();
                        break;
                    case Mode.SystemRunning:
                        ExitTable();
                        break;
                    case Mode.Manager:
                        ExitTableManager();
                        break;
                }
            }

            //START KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_START)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        WindowControl.UnlockForground();
                        StartTable(Process.GetCurrentProcess().Id);
                        break;
                }
            }

            //NEXT KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_NEXT)
            {
                //WindowControl.ShowWindow(Data.FindSystem(CurrentTable).WindowName);
                //WindowControl.HideTaskbar();
                //WindowControl.HideCursor();
                //WindowControl.HideWindow(ProgramName,true);
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        NextTable();
                        break;
                }
            }

            //PREV KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_PREV)
            {
                //WindowControl.HideWindow(Data.FindSystem(CurrentTable).WindowName);
                //WindowControl.ShowTaskbar();
                //WindowControl.ShowCursor();
                //WindowControl.ShowWindow(ProgramName,true);
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        PrevTable();
                        break;
                }
            }

            //RAND KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_RAND)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        RandomTable();
                        break;
                }
            }

            //TABLE MANAGER KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_TABLEMANAGER)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        StartTableManager();
                        break;
                }
            }

            //RECORD KEY
            if (e.KeyPressed == Data.BIND.KEYBIND_RECORD)
            {
                switch (RunMode)
                {
                    case Mode.SystemRunning:
                        RecordMedia1(); //Use NVIDA shadow play to recorde for 65 seconds.
                        break;

                }
            }


            //DO NOTHING TO OTHER KEYS TO AVOID KEYLOGGER FUNCTION
        }

        //Exit Program
        private void ExitFrontEnd()
        {
            Data.SaveDatabase(databasepath);
            logger.Info($"{RunMode.ToString()} : Exit Front End");
            _listener.UnHookKeyboard(); //Unregister keyboard hook
            WindowControl.ShowTaskbar();
            System.Windows.Application.Current.Shutdown(); //Shut program down
        }

        #region MediaRecording (NOT USED RIGHT NOW)

        //Start NVIDIA SHADOW PLAY
        private async void RecordMedia1()
        {
            System.Windows.Forms.SendKeys.SendWait("%{F9}");
            await Task.Delay(new TimeSpan(0, 1, 5));
            System.Windows.Forms.SendKeys.SendWait("%{F9}");
        }

        private void RecordMedia()
        {
            var ffmpeg_path = $@"{Model.ProgramPath.Value}ffmpeg\bin\ffmpeg.exe";

            //check to see if ffmpeg exists
            if (System.IO.File.Exists(ffmpeg_path))
            {
                //create parameters
                //string playfieldRecordParam = $"-y -t 10 -rtbufsize 1500M -f gdigrab -framerate 60 -offset_x {Data.MediaLocation.PlayfieldLocationX} -offset_y {Data.MediaLocation.PlayfieldLocationY} -video_size {Data.MediaLocation.PlayfieldSizeX}x{Data.MediaLocation.PlayfieldSizeY} -i desktop -c:v h264_nvenc -qp 0 -threads 8 \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                //string playfieldRecordParam = $"-y -t 10 -rtbufsize 500M -f gdigrab -framerate 60 -offset_x 0 -offset_y 0 -video_size 3440x1440 -i desktop -c:v h264_nvenc -preset lossless -profile high444p \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                //string playfieldRecordParam = $"-y -t 10 -rtbufsize 1500M -f dshow -r 60 -i video=\"screen-capture-recorder\":audio=\"virtual-audio-capturer\" -crf 0 -vcodec h264_nvenc -preset lossless -profile high444p -acodec pcm_s16le -ac 1 -ar 44100 \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                //string playfieldRecordParam = $"-y -rtbufsize 1500M -f dshow -i video=\"screen-capture-recorder\" -r 60 -t 10 -c:v h264_nvenc -preset lossless -profile high444p \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";


                //latest
                //string playfieldRecordParam = $"-y -framerate 60 -probesize 100M -rtbufsize 1500M -f gdigrab -offset_x {Data.MediaLocation.PlayfieldLocationX} -offset_y {Data.MediaLocation.PlayfieldLocationY} -video_size {Data.MediaLocation.PlayfieldSizeX}x{Data.MediaLocation.PlayfieldSizeY} -i desktop -t 30 -c:v h264_nvenc -profile:v high -preset losslesshp \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                string playfieldRecordParam = $"-y -framerate 60 -probesize 100M -rtbufsize 1500M -f gdigrab -offset_x {Data.MediaLocation.PlayfieldLocationX} -offset_y {Data.MediaLocation.PlayfieldLocationY} -video_size {Data.MediaLocation.PlayfieldSizeX}x{Data.MediaLocation.PlayfieldSizeY} -i desktop -t 30 -c:v h264_nvenc -profile:v high -preset losslesshp \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";


                Console.WriteLine(playfieldRecordParam);
                Console.WriteLine(CurrentTable.Playfield.LocalPath);


                Process playfieldRecord = new Process();
                playfieldRecord.StartInfo.FileName = ffmpeg_path;
                playfieldRecord.StartInfo.Arguments = playfieldRecordParam;
                playfieldRecord.StartInfo.CreateNoWindow = true; //run in background
                playfieldRecord.StartInfo.UseShellExecute = false;
                playfieldRecord.StartInfo.RedirectStandardOutput = true;
                playfieldRecord.Start();

                Console.WriteLine(playfieldRecord.StandardOutput.ReadToEnd());

                playfieldRecord.WaitForExit();

                Console.WriteLine(playfieldRecord.StandardOutput.ReadToEnd());
            }
        }

        #endregion

        //Loads thumbnails of all tables into memory
        private void InitalizeThumbnails()
        {
            logger.Info($"{RunMode.ToString()} : Loading Thumbnails");
            foreach (PinballTable pt in Data.TableList)
            {
                pt.LoadThumbnails(Data.MediaLocation.ThumbnailResolution);
            }
        }

        #region Background / Play Music

        //Audio Players
        public MediaPlayer BGMusicPlayer { get; set; } = new MediaPlayer();  //Background Music
        public MediaPlayer LMusicPlayer { get; set; } = new MediaPlayer();  //Launch Music


        //Initialize the background music player for repeating
        private void InitializeBGMusic()
        {
            BGMusicPlayer.MediaEnded += Media_Repeat;
        }

        private void Media_Repeat(object sender, EventArgs e)
        {
            var mp = (MediaPlayer)sender;
            mp.Position = TimeSpan.Zero;
            mp.Play();
        }

        //Change Background Music to the current table
        private void ChangeBGMusic()
        {
            BGMusicPlayer.Stop();
            if (CurrentTable != null)
                if (CurrentTable.BGMusicExists)
                {
                    BGMusicPlayer.Open(CurrentTable.BGMusic);
                    BGMusicPlayer.Play();
                }

        }

        private void StopBGMusic()
        {
            BGMusicPlayer.Stop();
        }

        private void ResumeBGMusic()
        {
            if (CurrentTable != null)
                if (CurrentTable.BGMusicExists)
                    BGMusicPlayer.Play();
        }

        #endregion

        #region Table Select

        //Select Next Table
        private void NextTable()
        {
            logger.Info($"{RunMode.ToString()} : Next Table");
            CurrentTable = Data.NextTable(CurrentTable);
            ChangeBGMusic();
        }

        //Select Previous Table
        private void PrevTable()
        {
            logger.Info($"{RunMode.ToString()} : Previous Table");
            CurrentTable = Data.PrevTable(CurrentTable);
            ChangeBGMusic();
        }

        //Select Random Table
        private void RandomTable()
        {
            logger.Info($"{RunMode.ToString()} : Random Table");
            CurrentTable = Data.RandomTable();
            ChangeBGMusic();
        }

        #endregion

    }

}
