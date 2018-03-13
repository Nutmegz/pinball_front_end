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

        /******************************************
                  LOGGER
        *******************************************/

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //Tell Windows to be on top
        public bool TopMost { get; set; } = true;

        //Keybinding Statemachine
        public Mode RunMode { get; set; } = Mode.FrontEnd; // Start in front end mode


        //Table Manager Handle
        private View.TableManagerView tableManager;

        //Currently viewed table handle
        public PinballTable CurrentTable { get; set; } //Table where all media is played

        public Visibility PlayfieldVisibility { get; set; } = Visibility.Visible;

        public View.MediaView PlayfieldWindow { get; set; }
        public View.MediaView BackglassWindow { get; set; }
        public View.MediaView DMDWindow { get; set; }


        /******************************************
                 Properties
        ******************************************/
        /// DATA


        //Holds System Information
        public ObservableCollection<PinballSystem> SystemList { get; set; } = new ObservableCollection<PinballSystem>();
        //Hold Table Information
        public ObservableCollection<PinballTable> TableList { get; set; } = new ObservableCollection<PinballTable>();

        //Holds Display Information
        public MediaLocation MediaLocation { get; set; } = new MediaLocation();

        //Holds Keybindings
        public Keybindings BIND { get; set; } = new Keybindings();

        //Global Keyboard Listener Hook
        private LowLevelKeyboardListener _listener;

        #region Database

        //Load Database into memory
        private void LoadDatabase()
        {
            var databasepath = $"{Model.ProgramPath.Value}database.json";

            //If Database Exists
            if (!System.IO.File.Exists(databasepath))
            {
                NewDatabase();
            }

            logger.Info($"Loading Database: {databasepath}");
            //SystemList = JsonConvert.DeserializeObject<ObservableCollection<PinballSystem>>(System.IO.File.ReadAllText(databasepath));
            StoreData storedata = JsonConvert.DeserializeObject<StoreData>(System.IO.File.ReadAllText(databasepath));

            if (storedata.BIND != null)
            {
                BIND = storedata.BIND;
            }

            if (storedata.SystemList != null)
            {
                SystemList = storedata.SystemList;
            }

            if (storedata.TableList != null)
            {
                TableList = storedata.TableList;
            }

            if (storedata.MediaLocation != null)
            {
                MediaLocation = storedata.MediaLocation;
            }

        }

        private void NewDatabase()
        {
            logger.Info("Database not found. Creating New Database");
            BIND = new Model.Keybindings();
            SystemList = new ObservableCollection<PinballSystem>();
            TableList = new ObservableCollection<PinballTable>();
            MediaLocation = new MediaLocation();

            SaveDatabase();


        }

        //Save database to disk
        private void SaveDatabase()
        {
            SortSystemsTables();
            var databasepath = $"{Model.ProgramPath.Value}database.json";
            logger.Info($"Saving Database: {databasepath}");
            var storedata = new StoreData();
            storedata.BIND = BIND;
            storedata.TableList = TableList;
            storedata.SystemList = SystemList;
            storedata.MediaLocation = MediaLocation;
            System.IO.File.WriteAllText(databasepath, JsonConvert.SerializeObject(storedata, Formatting.Indented));
        }

        //Sort systems and tables by name
        private void SortSystemsTables()
        {
            try
            {
                logger.Trace("Sorting Systems and Tables");
                foreach (PinballSystem ps in SystemList)
                {
                    TableList = new ObservableCollection<PinballTable>(TableList.OrderBy(i => i.Description));
                }

                SystemList = new ObservableCollection<PinballSystem>(SystemList.OrderBy(i => i.Name));


            }
            catch (Exception e)
            {

                logger.Error(e.ToString);
            }
        }

        private PinballSystem FindSystem(PinballTable table)
        {
            return SystemList.Single(x => x.Name == table.System);
        }

        #endregion


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
            tableManager = new View.TableManagerView(SystemList, TableList, MediaLocation);
            WindowControl.ShowTaskbar();
            tableManager.Show();
        }

        private void ExitTableManager()
        {
            SortSystemsTables();
            logger.Trace($"{RunMode.ToString()} : Exit Table Manager");

            BackglassWindow.Visibility = Visibility.Visible;
            DMDWindow.Visibility = Visibility.Visible;
            PlayfieldVisibility = Visibility.Visible;
            ResumeBGMusic();

            tableManager.Close();
            TopMost = true;
            WindowControl.SetFocus("Pinball Front End");

            RunMode = Mode.FrontEnd;
            logger.Trace($"Setting Mode to {RunMode.ToString()}");
        }

        private void StartTable()
        {
            logger.Info($"{RunMode.ToString()}: Starting table: {CurrentTable.Name}");

            //Play Launch Music
            LMusicPlayer.Open(CurrentTable.LMusic);
            LMusicPlayer.Play();



            var system = FindSystem(CurrentTable);

            //Replace [TABLENAME]
            var regex = new Regex(@"\[TABLENAME\]");
            var param = regex.Replace(system.Parameters, CurrentTable.Name);

            //Replace [SYSTEMPATH]
            regex = new Regex(@"\[SYSTEMPATH\]");
            param = regex.Replace(param, system.WorkingPath);

            //set windows to be topmost
            //TopMost = true;

            logger.Debug(param);
            var proc = new Process();
            proc.StartInfo.FileName = system.WorkingPath + "\\" + system.Executable;
            proc.StartInfo.Arguments = param;
            proc.Start();

            //procID = proc.Id;
            //Console.WriteLine(proc.Id);

            //Make sure this program stays on top while loading
            //Show Loading Screen (Needs Completed)
            WindowControl.SetFocus("Pinball Front End");

            Stopwatch watchdog = new Stopwatch();
            watchdog.Reset();
            watchdog.Start();

            //Mute Process
            //Hunt for process
            int gameID = 0;
            //Console.WriteLine(game.Id);
            while (gameID == 0)
            {
                var procs = Process.GetProcesses();
                foreach (Process procy in procs)
                {
                    //Console.WriteLine(procy.ProcessName);
                    if (procy.ProcessName.IndexOf(FindSystem(CurrentTable).Name, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        gameID = procy.Id;
                        logger.Info($"Found Program: {FindSystem(CurrentTable).Name.ToLower()}");
                        break;
                    }
                    if (watchdog.ElapsedMilliseconds > 10000)
                    {
                        logger.Error("Can't Find System Process");
                        break;
                    }
                }
            }



            //Try and Mute
            watchdog.Restart();
            logger.Info("Muting System");
            while (VolumeMixer.GetApplicationMute(gameID) != true)
            {
                //Console.WriteLine("Trying to Mute");
                VolumeMixer.SetApplicationMute(gameID, true);
                WindowControl.SetFocus("Pinball Front End");
                if (watchdog.ElapsedMilliseconds > 10000)
                {
                    logger.Error("Can't Mute System");
                    break;
                }
            }




            Thread.Sleep(system.WaitTime * 1000); // sleep
            BGMusicPlayer.Pause();

            //Unmute
            watchdog.Restart();
            logger.Info("Unmuting System");
            while (VolumeMixer.GetApplicationMute(gameID) != false)
            {
                //Console.WriteLine("Trying to Unmute");
                VolumeMixer.SetApplicationMute(gameID, false);
                WindowControl.SetFocus("Pinball Front End");
                if (watchdog.ElapsedMilliseconds > 10000)
                {
                    logger.Error("Can't Unmute System");
                    break;
                }
            }


            //TopMost = false;
            WindowControl.SetFocus($"{system.Name}");



            //Hide Selected Windows
            logger.Trace("Setting Window Visibility");
            BackglassWindow.Visibility = CurrentTable.HideBackglass ? Visibility.Hidden : Visibility.Visible;
            DMDWindow.Visibility = CurrentTable.HideDMD ? Visibility.Hidden : Visibility.Visible;
            //PlayfieldWindow.Visibility = Visibility.Hidden;
            PlayfieldVisibility = Visibility.Hidden;


            RunMode = Mode.SystemRunning;
            logger.Trace($"Setting Mode to {RunMode.ToString()}");
        }

        private void ExitTable()
        {
            logger.Info($"{RunMode.ToString()} : Exit System");
            try
            {
                var procs = Process.GetProcesses();
                foreach (Process proc in procs)
                {
                    //Console.WriteLine(proc.ProcessName);
                    if (proc.ProcessName.IndexOf(FindSystem(CurrentTable).Name, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        logger.Info($"Ending Program: {FindSystem(CurrentTable).Name.ToLower()}");
                        proc.Kill();

                    }
                    //logger.Debug(proc.ProcessName);
                }

                //Show Media
                BackglassWindow.Visibility = Visibility.Visible;
                DMDWindow.Visibility = Visibility.Visible;
                PlayfieldVisibility = Visibility.Visible;

                //Resume Music
                ResumeBGMusic();

                WindowControl.SetFocus("Pinball Front End");

                RunMode = Mode.FrontEnd;
                logger.Trace($"Setting Mode to {RunMode.ToString()}");
                //var proc = Process.GetProcessById(procID);
                //proc.Kill();
                //if (proc.Length > 0)
                //{
                //    proc[0].Kill();
                //}

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void InitializeKeyboardHook()
        {
            //Start Keyboard Listener (Removed Keylogger)
            _listener = new LowLevelKeyboardListener();        //New Hook Object
            _listener.OnKeyPressed += KeyboardListner_OnKeyPressed;  //Subscribe to Event
            _listener.HookKeyboard();                          //Start Hook
        }


        //Handle Keyboard Message
        private void KeyboardListner_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            //EXIT KEY
            if (e.KeyPressed == BIND.KEYBIND_EXIT)
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
            if (e.KeyPressed == BIND.KEYBIND_START)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        StartTable();
                        break;
                }
            }

            //NEXT KEY
            if (e.KeyPressed == BIND.KEYBIND_NEXT)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        NextTable();
                        break;
                }
            }

            //PREV KEY
            if (e.KeyPressed == BIND.KEYBIND_PREV)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        PrevTable();
                        break;
                }
            }

            //RAND KEY
            if (e.KeyPressed == BIND.KEYBIND_RAND)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        RandomTable();
                        break;
                }
            }

            //TABLE MANAGER KEY
            if (e.KeyPressed == BIND.KEYBIND_TABLEMANAGER)
            {
                switch (RunMode)
                {
                    case Mode.FrontEnd:
                        StartTableManager();
                        break;
                }
            }

            //RECORD KEY
            if (e.KeyPressed == BIND.KEYBIND_RECORD)
            {
                switch (RunMode)
                {

                }
            }


            //DO NOTHING TO OTHER KEYS TO AVOID KEYLOGGER FUNCTION
        }



        private void ExitFrontEnd()
        {
            SaveDatabase();
            logger.Info($"{RunMode.ToString()} : Exit Front End");
            _listener.UnHookKeyboard(); //Unregister keyboard hook
            WindowControl.ShowTaskbar();
            System.Windows.Application.Current.Shutdown(); //Shut program down
        }




        #region MediaRecording

        private void RecordMedia1()
        {
            System.Windows.Forms.SendKeys.SendWait("%{F9}");
        }

        private void RecordMedia()
        {
            var ffmpeg_path = $@"{Model.ProgramPath.Value}ffmpeg\bin\ffmpeg.exe";

            //check to see if ffmpeg exists
            if (System.IO.File.Exists(ffmpeg_path))
            {
                //create parameters
                //string playfieldRecordParam = $"-y -t 10 -rtbufsize 1500M -f gdigrab -framerate 60 -offset_x {MediaLocation.PlayfieldLocationX} -offset_y {MediaLocation.PlayfieldLocationY} -video_size {MediaLocation.PlayfieldSizeX}x{MediaLocation.PlayfieldSizeY} -i desktop -c:v h264_nvenc -qp 0 -threads 8 \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                //string playfieldRecordParam = $"-y -t 10 -rtbufsize 500M -f gdigrab -framerate 60 -offset_x 0 -offset_y 0 -video_size 3440x1440 -i desktop -c:v h264_nvenc -preset lossless -profile high444p \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                //string playfieldRecordParam = $"-y -t 10 -rtbufsize 1500M -f dshow -r 60 -i video=\"screen-capture-recorder\":audio=\"virtual-audio-capturer\" -crf 0 -vcodec h264_nvenc -preset lossless -profile high444p -acodec pcm_s16le -ac 1 -ar 44100 \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                //string playfieldRecordParam = $"-y -rtbufsize 1500M -f dshow -i video=\"screen-capture-recorder\" -r 60 -t 10 -c:v h264_nvenc -preset lossless -profile high444p \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";


                //latest
                //string playfieldRecordParam = $"-y -framerate 60 -probesize 100M -rtbufsize 1500M -f gdigrab -offset_x {MediaLocation.PlayfieldLocationX} -offset_y {MediaLocation.PlayfieldLocationY} -video_size {MediaLocation.PlayfieldSizeX}x{MediaLocation.PlayfieldSizeY} -i desktop -t 30 -c:v h264_nvenc -profile:v high -preset losslesshp \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";

                string playfieldRecordParam = $"-y -framerate 60 -probesize 100M -rtbufsize 1500M -f gdigrab -offset_x {MediaLocation.PlayfieldLocationX} -offset_y {MediaLocation.PlayfieldLocationY} -video_size {MediaLocation.PlayfieldSizeX}x{MediaLocation.PlayfieldSizeY} -i desktop -t 30 -c:v h264_nvenc -profile:v high -preset losslesshp \"{Model.ProgramPath.Value}temp\\{CurrentTable.Description}.mp4\"";


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


        //Default Constructor
        public PinballFrontEndViewModel()
        {


            logger.Info($"Starting Pinball Front End: {Model.ProgramPath.Value}");
            //InitializePlayers();
            LoadDatabase();
            InitializeKeyboardHook();
            InitalizeThumbnails();
            InitializeBGMusic();
            RandomTable();



            InitalizeWindows();

            //WindowControl.HideTaskbar();
            //WindowControl.LockForground();


        }

        private void InitalizeWindows()
        {
            //Playfield Window
            //PlayfieldWindow = new View.MediaView();
            //PlayfieldWindow.WindowStyle = WindowStyle.None;
            //PlayfieldWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            //PlayfieldWindow.ResizeMode = ResizeMode.NoResize;

            BGMusicPlayer.IsMuted = false;

            //NewWindow();

            //PlayfieldWindow.ShowActivated = false;
            //PlayfieldWindow.WindowState = WindowState.Maximized;

            //BindWindow(PlayfieldWindow, Window.TopProperty, this, "MediaLocation.PlayfieldLocationY", BindingMode.TwoWay);
            //BindWindow(PlayfieldWindow, Window.LeftProperty, this, "MediaLocation.PlayfieldLocationX", BindingMode.TwoWay);
            //BindWindow(PlayfieldWindow, Window.HeightProperty, this, "MediaLocation.PlayfieldSizeY", BindingMode.TwoWay);
            //BindWindow(PlayfieldWindow, Window.WidthProperty, this, "MediaLocation.PlayfieldSizeX", BindingMode.TwoWay);
            //BindWindow(PlayfieldWindow, Window.BackgroundProperty, this, "PlayfieldBrush", BindingMode.OneWay);
            //BindWindow(PlayfieldWindow, Window.BackgroundProperty, this, "PlayfieldBrush", BindingMode.OneWay);
            //BindWindow(PlayfieldWindow, Window.TopmostProperty, this, "TopMost", BindingMode.TwoWay);
            //BindWindow(PlayfieldWindow, Window.VisibilityProperty, this, "PlayfieldVisibility");

            //PlayfieldWindow.Show();

            //Backglass Window
            BackglassWindow = new View.MediaView();
            BackglassWindow.WindowStyle = WindowStyle.None;
            BackglassWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            BackglassWindow.ResizeMode = ResizeMode.NoResize;
            //BackglassWindow.WindowState = WindowState.Maximized;
            //BackglassWindow.DataContext = this;

            BindWindow(BackglassWindow, Window.TopProperty, this, "MediaLocation.BackglassLocationY", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.LeftProperty, this, "MediaLocation.BackglassLocationX", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.HeightProperty, this, "MediaLocation.BackglassSizeY", BindingMode.TwoWay);
            BindWindow(BackglassWindow, Window.WidthProperty, this, "MediaLocation.BackglassSizeX", BindingMode.TwoWay);
            //BindWindow(BackglassWindow, Window.BackgroundProperty, this, "BackglassBrush", BindingMode.OneWay);
            BindWindow(BackglassWindow, Window.TopmostProperty, this, "TopMost", BindingMode.TwoWay);
            BindWindow(BackglassWindow, View.MediaView.MediaUriProperty, this, "CurrentTable.Backglass", BindingMode.OneWay);
            //BindWindow(BackglassWindow, View.MediaView.ThumbnailProperty, this, "CurrentTable.BackglassThumbnail", BindingMode.TwoWay);
            BindWindow(BackglassWindow.PreloadImage, System.Windows.Controls.Image.SourceProperty, this, "CurrentTable.BackglassThumbnail", BindingMode.OneWay);
            //BackglassWindow.Thumbnail = CurrentTable.BackglassThumbnail;
            //BindWindow(BackglassWindow, BackglassWindow.PreloadImage.Source, this, "CurrentTable.Backglass", BindingMode.OneWay);

            //BindWindow(BackglassWindow, Window.VisibilityProperty, this, "BackglassVisibility");

            BackglassWindow.Show();

            //DMD Window
            DMDWindow = new View.MediaView();
            DMDWindow.WindowStyle = WindowStyle.None;
            DMDWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            DMDWindow.ResizeMode = ResizeMode.NoResize;
            //DMDWindow.DataContext = this;

            BindWindow(DMDWindow, Window.TopProperty, this, "MediaLocation.DMDLocationY", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.LeftProperty, this, "MediaLocation.DMDLocationX", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.HeightProperty, this, "MediaLocation.DMDSizeY", BindingMode.TwoWay);
            BindWindow(DMDWindow, Window.WidthProperty, this, "MediaLocation.DMDSizeX", BindingMode.TwoWay);
            //BindWindow(DMDWindow, Window.BackgroundProperty, this, "DMDBrush", BindingMode.OneWay);
            BindWindow(DMDWindow, Window.TopmostProperty, this, "TopMost", BindingMode.TwoWay);
            BindWindow(DMDWindow, View.MediaView.MediaUriProperty, this, "CurrentTable.DMD", BindingMode.OneWay);
            //BindWindow(DMDWindow, View.MediaView.ThumbnailProperty, this, "CurrentTable.DMDThumbnail", BindingMode.OneWay);
            BindWindow(DMDWindow.PreloadImage, System.Windows.Controls.Image.SourceProperty, this, "CurrentTable.DMDThumbnail", BindingMode.OneWay);
            //DMDWindow.Thumbnail = CurrentTable.DMDThumbnail;
            //BindWindow(DMDWindow, Window.VisibilityProperty, this, "DMDVisibility");

            DMDWindow.Show();

        }

        #region Window Property Binding

        private void BindWindow(System.Windows.Controls.Image window, DependencyProperty property, Object source, String propertyPath, BindingMode mode)
        {



            Binding bind = new Binding()
            {
                Source = source,
                Path = new PropertyPath(propertyPath),
                Mode = mode,
                NotifyOnSourceUpdated = true,
                NotifyOnTargetUpdated = true
            };
            BindingOperations.SetBinding(window, property, bind);
        }

        private void BindWindow(Window window, DependencyProperty property, Object source, String propertyPath, BindingMode mode)
        {



            Binding bind = new Binding()
            {
                Source = source,
                Path = new PropertyPath(propertyPath),
                Mode = mode
            };
            BindingOperations.SetBinding(window, property, bind);
        }




        #endregion

        #region Table Select


        /// <summary>
        /// Return a handle to the next table in the tablelist
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private PinballTable NextTable(PinballTable table)
        {
            var pinballtable = TableList.ElementAt(TableList.IndexOf(table) < TableList.Count() - 1 ? TableList.IndexOf(table) + 1 : 0);
            if (pinballtable.Enabled)
            {
                return pinballtable;
            }
            else
            {
                //Ignore Disabled Tables
                return NextTable(pinballtable);
            }

        }


        /// <summary>
        /// Return a handle to the next table + advance in the table list
        /// </summary>
        /// <param name="table"></param>
        /// <param name="advance"></param>
        /// <returns></returns>
        private PinballTable NextTable(PinballTable table, int advance)
        {
            //Console.WriteLine($"Next Table: {table.Description} -> {advance}");
            if (advance > 0)
            {
                return NextTable(NextTable(table), advance - 1);
            }

            return table;
        }


        /// <summary>
        /// Returns a handle to the prev table in the tablelist
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private PinballTable PrevTable(PinballTable table)
        {
            var pinballtable = TableList.ElementAt(TableList.IndexOf(table) > 0 ? TableList.IndexOf(table) - 1 : TableList.Count() - 1);
            if (pinballtable.Enabled)
            {
                return pinballtable;
            }
            else
            {
                //Ignore Disabled Tables
                return PrevTable(pinballtable);
            }
        }


        /// <summary>
        /// Returns a hndle to the prev table - advance in the tablelist
        /// </summary>
        /// <param name="table"></param>
        /// <param name="advance"></param>
        /// <returns></returns>
        private PinballTable PrevTable(PinballTable table, int advance)
        {
            //Console.WriteLine($"Prev Table: {table.Description} -> {advance}");
            if (advance > 0)
            {
                return PrevTable(PrevTable(table), advance - 1);
            }

            return table;
        }

        /// <summary>
        /// Select a randome table from the TableList
        /// </summary>
        private void RandomTable()
        {
            logger.Info($"{RunMode.ToString()} : Random Table");
            if (TableList.Count > 0)
            {
                //Select Random table
                Random rnd = new Random();
                CurrentTable = TableList.ElementAt(rnd.Next(0, TableList.Count));
                //Update Background Music
                ChangeBGMusic();
            }


        }


        #endregion




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




        //Loads thumbnails of all tables into memory
        private void InitalizeThumbnails()
        {
            logger.Info($"{RunMode.ToString()} : Loading Thumbnails");
            foreach (PinballTable pt in TableList)
            {
                pt.loadThumbnails();
            }
        }

        #region Background Music

        //Change Background Music to the current table
        private void ChangeBGMusic()
        {
            BGMusicPlayer.Stop();
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
            if (CurrentTable.BGMusicExists)
                BGMusicPlayer.Play();
        }

        #endregion



        //Stat Machine to select next table
        private void NextTable()
        {
            logger.Info($"{RunMode.ToString()} : Next Table");
            CurrentTable = NextTable(CurrentTable);
            ChangeBGMusic();
        }

        //Stat Machine to select next table
        private void PrevTable()
        {
            logger.Info($"{RunMode.ToString()} : Prev Table");
            CurrentTable = PrevTable(CurrentTable);
            ChangeBGMusic();
        }



    }

}
