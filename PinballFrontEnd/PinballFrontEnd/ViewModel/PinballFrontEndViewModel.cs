using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using NLog;
using DesktopWPFAppLowLevelKeyboardHook;
using System.Diagnostics;
using System.Collections.ObjectModel;
using PinballFrontEnd.Model;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
        //////////////////////////////////////////////////////////////////
        // KEYBINDINGS
        /////////////////////////////////////////////////////////////////
        //public const Key KEYBIND_EXIT = Key.Escape;
        //public const Key KEYBIND_TABLEMANAGER = Key.F1;
        //public const Key KEYBIND_NEXT = Key.Right;
        //public const Key KEYBIND_PREV = Key.Left;
        //public const Key KEYBIND_RAND = Key.F2;
        //public const Key KEYBIND_START = Key.Enter;

        public Mode RunMode { get; set; } = Mode.FrontEnd; // Start in front end mode

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //Table Manager Launcher
        private View.TableManagerView tableManager;

        #region KeyboardListener
        //Global Keyboard Listener Hook
        private LowLevelKeyboardListener _listener;

        //Handle Global Key Events
        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            //Change Keybindings on Mode Select
            switch (RunMode)
            {

                ////////////////////////////////////////////
                // System Running Mode
                ////////////////////////////////////////////
                case Mode.SystemRunning:

                    //Exit System and go back to frontend
                    if (e.KeyPressed == BIND.KEYBIND_EXIT)
                    {
                        logger.Trace("Exit System");
                        try
                        {
                            var procs = Process.GetProcesses();
                            foreach (Process proc in procs)
                            {
                                //Console.WriteLine(proc.ProcessName);
                                if (proc.ProcessName.Contains(FindSystem(CurrentTable).Name))
                                {
                                    logger.Info($"Ending Program: {FindSystem(CurrentTable).Name}");
                                    proc.Kill();
                                }
                            }

                            //Show Media
                            MediaLocation.BackglassVisable = true;
                            MediaLocation.DMDVisable = true;

                            RunMode = Mode.FrontEnd;
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




                    break;



                ////////////////////////////////////////////
                // Front End Mode
                ////////////////////////////////////////////
                case Mode.FrontEnd:


                    //Exit Program
                    if (e.KeyPressed == BIND.KEYBIND_EXIT)
                    {
                        SaveDatabase();
                        logger.Trace("Exit Program");
                        _listener.UnHookKeyboard(); //Unregister keyboard hook
                        System.Windows.Application.Current.Shutdown(); //Shut program down
                    }

                    //Start Table Manager
                    if (e.KeyPressed == BIND.KEYBIND_TABLEMANAGER)
                    {
                        logger.Trace("Starting Table Manager");
                        RunMode = Mode.Manager;
                        tableManager = new View.TableManagerView(SystemList, TableList, MediaLocation);
                        tableManager.ShowDialog();
                    }

                    //Next Table
                    if (e.KeyPressed == BIND.KEYBIND_NEXT)
                    {
                        logger.Trace("Next Table");
                        NextTable();
                    }

                    //Previous Table
                    if (e.KeyPressed == BIND.KEYBIND_PREV)
                    {
                        logger.Trace("Previous Table");
                        PrevTable();
                    }

                    //Random Table
                    if (e.KeyPressed == BIND.KEYBIND_RAND)
                    {
                        logger.Trace("Random Table");
                        RandomTable();
                    }

                    //Start Table
                    if (e.KeyPressed == BIND.KEYBIND_START)
                    {
                        logger.Trace($"Starting table: {CurrentTable.Name}");


                        //Hide Selected Windows
                        MediaLocation.BackglassVisable = !CurrentTable.HideBackglass;
                        MediaLocation.DMDVisable = !CurrentTable.HideDMD;

                        var system = FindSystem(CurrentTable);


                        var regex = new Regex(@"\[TABLENAME\]");
                        var param = regex.Replace(system.Parameters, CurrentTable.Name);

                        regex = new Regex(@"\[SYSTEMPATH\]");
                        param = regex.Replace(param, system.WorkingPath);

                        logger.Debug(param);
                        var proc = new Process();
                        proc.StartInfo.FileName = system.WorkingPath + "\\" + system.Executable;
                        proc.StartInfo.Arguments = param;
                        proc.Start();
                        //procID = proc.Id;
                        //Console.WriteLine(proc.Id);

                        RunMode = Mode.SystemRunning;
                    }


                    break;

                ////////////////////////////////////////////
                // Table Manager Mode
                ////////////////////////////////////////////
                case Mode.Manager:

                    //Exit Table Manager
                    if (e.KeyPressed == BIND.KEYBIND_EXIT)
                    {
                        SortSystemsTables();
                        logger.Trace("Exit Table Manager");
                        RunMode = Mode.FrontEnd;
                        tableManager.Close();
                    }


                    break;


                ////////////////////////////////////////////
                // Default Mode (Should not get here)
                ////////////////////////////////////////////
                default:
                    break;
            }

        }

        #endregion

        //Properties

        //Holds Table Information
        public ObservableCollection<PinballSystem> SystemList { get; set; } = new ObservableCollection<PinballSystem>();
        public ObservableCollection<PinballTable> TableList { get; set; } = new ObservableCollection<PinballTable>();

        public MediaLocation MediaLocation { get; set; } = new MediaLocation();

        public Keybindings BIND { get; set; } = new Keybindings();



        #region Database
        private void LoadDatabase()
        {
            var databasepath = $"{Model.ProgramPath.Value}database.json";
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


        #region MediaLocation
        //public int PlayfieldLocationX { get; set; } = 0;
        //public int PlayfieldLocationY { get; set; } = 0;
        //public int PlayfieldSizeX { get; set; } = 1920;
        //public int PlayfieldSizeY { get; set; } = 1080;
        //public bool PlayfieldVisable { get; set; } = true;

        //public int BackglassLocationX { get; set; } = 2000;
        //public int BackglassLocationY { get; set; } = 400;
        //public int BackglassSizeX { get; set; } = 800;
        //public int BackglassSizeY { get; set; } = 300;
        //public bool BackglassVisable { get; set; } = true;

        //public int DMDLocationX { get; set; } = 2000;
        //public int DMDLocationY { get; set; } = 800;
        //public int DMDSizeX { get; set; } = 400;
        //public int DMDSizeY { get; set; } = 400;
        //public bool DMDVisable { get; set; } = true;

        #endregion





        //Default Constructor
        public PinballFrontEndViewModel()
        {

            logger.Info($"Starting Pinball Front End: {Model.ProgramPath.Value}");
            LoadDatabase();

            RandomTable();

            //Start Keyboard Listener
            _listener = new LowLevelKeyboardListener();        //New Hook Object
            _listener.OnKeyPressed += _listener_OnKeyPressed;  //Subscribe to Event
            _listener.HookKeyboard();                          //Start Hook
        }

        #region Table Select

        public PinballTable CurrentTable { get; set; }

        private void NextTable()
        {
            CurrentTable = TableList.ElementAt(TableList.IndexOf(CurrentTable) >= 0 && TableList.IndexOf(CurrentTable) < TableList.Count() - 1 ? TableList.IndexOf(CurrentTable) + 1 : 0);
        }
        private void PrevTable()
        {
            CurrentTable = TableList.ElementAt(TableList.IndexOf(CurrentTable) > 0 && TableList.IndexOf(CurrentTable) < TableList.Count() ? TableList.IndexOf(CurrentTable) - 1 : TableList.Count() - 1);
        }
        private void RandomTable()
        {
            //Select Random table
            Random rnd = new Random();
            CurrentTable = TableList.ElementAt(rnd.Next(0, TableList.Count));
        }
        #endregion



    }
}
