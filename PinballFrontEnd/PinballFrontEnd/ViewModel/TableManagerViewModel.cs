using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PinballFrontEnd.Model;
using System.IO;
using Microsoft.Win32;
using System.Windows.Input;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace PinballFrontEnd.ViewModel
{
    public class TableManagerViewModel : ViewModelBase
    {
        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //public ObservableCollection<PinballSystem> SystemList { get; set; } = new ObservableCollection<PinballSystem>();
        //public ObservableCollection<PinballTable> TableList { get; set; } = new ObservableCollection<PinballTable>();
        //public MediaLocation MediaLocation { get; set; } = new MediaLocation();

        public PinballSystem SelectedSystem { get; set; }
        public PinballTable SelectedTable { get; set; }

        public bool TopMost { get; set; } = false;

        public PinballData Data { get; set; }

        public TableManagerViewModel(string databasepath)
        {
            Data = new PinballData(databasepath);
            InitalizeDummyWindows();
        }

        public TableManagerViewModel(PinballData data)
        {
            this.Data = data;
            InitalizeDummyWindows();
        }

        private Window PlayfieldWindowDummy;
        private Window BackglassWindowDummy;
        private Window DMDWindowDummy;

        // Commands
        public ICommand DummyWindowButton { get { return new RelayCommand(ShowHideDummyWindows); } }

        private void InitalizeDummyWindows()
        {
            PlayfieldWindowDummy = new Window();
            PlayfieldWindowDummy.WindowStyle = WindowStyle.None;
            PlayfieldWindowDummy.WindowStartupLocation = WindowStartupLocation.Manual;
            PlayfieldWindowDummy.ResizeMode = ResizeMode.NoResize;
            PlayfieldWindowDummy.Background = System.Windows.Media.Brushes.Red;

            BindWindow(PlayfieldWindowDummy, Window.LeftProperty, this, "Data.MediaLocation.PlayfieldLocationX", BindingMode.TwoWay);
            BindWindow(PlayfieldWindowDummy, Window.TopProperty, this, "Data.MediaLocation.PlayfieldLocationY", BindingMode.TwoWay);
            BindWindow(PlayfieldWindowDummy, Window.WidthProperty, this, "Data.MediaLocation.PlayfieldSizeX", BindingMode.TwoWay);
            BindWindow(PlayfieldWindowDummy, Window.HeightProperty, this, "Data.MediaLocation.PlayfieldSizeY", BindingMode.TwoWay);

            BackglassWindowDummy = new Window();
            BackglassWindowDummy.WindowStyle = WindowStyle.None;
            BackglassWindowDummy.WindowStartupLocation = WindowStartupLocation.Manual;
            BackglassWindowDummy.ResizeMode = ResizeMode.NoResize;
            BackglassWindowDummy.Background = System.Windows.Media.Brushes.Blue;

            BindWindow(BackglassWindowDummy, Window.LeftProperty, this, "Data.MediaLocation.BackglassLocationX", BindingMode.TwoWay);
            BindWindow(BackglassWindowDummy, Window.TopProperty, this, "Data.MediaLocation.BackglassLocationY", BindingMode.TwoWay);
            BindWindow(BackglassWindowDummy, Window.WidthProperty, this, "Data.MediaLocation.BackglassSizeX", BindingMode.TwoWay);
            BindWindow(BackglassWindowDummy, Window.HeightProperty, this, "Data.MediaLocation.BackglassSizeY", BindingMode.TwoWay);

            DMDWindowDummy = new Window();
            DMDWindowDummy.WindowStyle = WindowStyle.None;
            DMDWindowDummy.WindowStartupLocation = WindowStartupLocation.Manual;
            DMDWindowDummy.ResizeMode = ResizeMode.NoResize;
            DMDWindowDummy.Background = System.Windows.Media.Brushes.Green;

            BindWindow(DMDWindowDummy, Window.LeftProperty, this, "Data.MediaLocation.DMDLocationX", BindingMode.TwoWay);
            BindWindow(DMDWindowDummy, Window.TopProperty, this, "Data.MediaLocation.DMDLocationY", BindingMode.TwoWay);
            BindWindow(DMDWindowDummy, Window.WidthProperty, this, "Data.MediaLocation.DMDSizeX", BindingMode.TwoWay);
            BindWindow(DMDWindowDummy, Window.HeightProperty, this, "Data.MediaLocation.DMDSizeY", BindingMode.TwoWay);


        }

        //Show Dummy Windows for location
        private void ShowHideDummyWindows(object obj)
        {
            if (PlayfieldWindowDummy.IsVisible)
            {
                PlayfieldWindowDummy.Visibility = Visibility.Hidden;
                TopMost = false;
            } else
            {
                PlayfieldWindowDummy.Visibility = Visibility.Visible;
                TopMost = true;
            }
                

            if (BackglassWindowDummy.IsVisible)
            {
                BackglassWindowDummy.Visibility = Visibility.Hidden;
                TopMost = false;
            } else
            {
                BackglassWindowDummy.Visibility = Visibility.Visible;
                TopMost = true;
            }
                

            if (DMDWindowDummy.IsVisible)
            {
                DMDWindowDummy.Visibility = Visibility.Hidden;
                TopMost = false;
            } else
            {
                DMDWindowDummy.Visibility = Visibility.Visible;
                TopMost = true;
            }
               
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


        /*
        #region Obsolete
        private int procID;


        // Commands
        public ICommand SaveTableList { get { return new RelayCommand(SaveTables); } }
        public ICommand OpenTableList { get { return new RelayCommand(OpenTables); } }

        public ICommand Launch { get { return new RelayCommand(LaunchP); } }

       

        private void LaunchP(object obj)
        {
            var system = FindSystem(SelectedTable);


            var regex = new Regex(@"\[TABLENAME\]");
            var param = regex.Replace(system.Parameters, SelectedTable.Name);

            regex = new Regex(@"\[SYSTEMPATH\]");
            param = regex.Replace(param, system.WorkingPath);

            logger.Debug(param);
            var proc = new Process();
            proc.StartInfo.FileName = system.WorkingPath + "\\" + system.Executable;
            proc.StartInfo.Arguments = param;
            proc.Start();
            procID = proc.Id;
            Console.WriteLine(proc.Id);

        }

        #region Select Media

        public ICommand SelectPlayfield { get { return new RelayCommand(SelectPlayfieldFile); } }

        private void SelectPlayfieldFile(object obj)
        {
            var filename = GetOpenFile(".mp4", "Media File (*.mp4)|*.mp4");
            if (filename != null && File.Exists(filename))
            {
                //SelectedTable.Playfield = new Uri(filename);
            }
        }

        public ICommand SelectBackglass { get { return new RelayCommand(SelectBackglassFile); } }

        private void SelectBackglassFile(object obj)
        {
            var filename = GetOpenFile(".mp4", "Media File (*.mp4)|*.mp4");
            if (filename != null && File.Exists(filename))
            {
                //SelectedTable.Backglass = new Uri(filename);
            }
        }

        public ICommand SelectDMD { get { return new RelayCommand(SelectDMDFile); } }

        private void SelectDMDFile(object obj)
        {
            var filename = GetOpenFile(".mp4", "Media File (*.mp4)|*.mp4");
            if (filename != null && File.Exists(filename))
            {
                //SelectedTable.DMD = new Uri(filename);
            }
        }

        public ICommand SelectWheel { get { return new RelayCommand(SelectWheelFile); } }

        private void SelectWheelFile(object obj)
        {
            var filename = GetOpenFile(".png", "Media File (*.png)|*.png");
            if (filename != null && File.Exists(filename))
            {
                //SelectedTable.Wheel = new Uri(filename);
            }

        }


        #endregion

        //Save Tables
        private void SaveTables(object obj)
        {
            var filepath = GetSaveFile(".json", "System Database (*.json)|*.json");

            if (filepath != null)
            {
                logger.Info("Saving Database: %s", filepath);
                SortSystemsTables();

                var storedata = new PinballData();
                storedata.SystemList = SystemList;
                storedata.TableList = TableList;

                System.IO.File.WriteAllText(filepath, JsonConvert.SerializeObject(storedata, Formatting.Indented));
            }


        }

        private void LoadDatabase()
        {
            var databasepath = $"{Model.ProgramPath.Value}database.json";
            logger.Info($"Loading Database: {databasepath}");
            //SystemList = JsonConvert.DeserializeObject<ObservableCollection<PinballSystem>>(System.IO.File.ReadAllText(databasepath));
            PinballData storedata = JsonConvert.DeserializeObject<PinballData>(System.IO.File.ReadAllText(databasepath));
            SystemList = storedata.SystemList;
            TableList = storedata.TableList;
        }

        private void SaveData()
        {
            var storedata = new PinballData();
            storedata.SystemList = SystemList;
            storedata.TableList = TableList;


        }

        private void OpenTables(object obj)
        {
            //TableList = JsonConvert.DeserializeObject<ObservableCollection<PinballTable>>(System.IO.File.ReadAllText(GetOpenFile()));
            var filepath = GetOpenFile(".json", "System Database (*.json)|*.json");

            if (filepath != null && System.IO.File.Exists(filepath))
            {



                SystemList = JsonConvert.DeserializeObject<ObservableCollection<PinballSystem>>(System.IO.File.ReadAllText(filepath));
            }


        }

        private void SortSystemsTables()
        {
            try
            {

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

        private string GetOpenFile(string ext, string filter)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ext;
            dlg.Filter = filter;

            if (dlg.ShowDialog() == true)
            {
                var file = new FileInfo(dlg.FileName);
                return file.FullName;
            }

            return null;
        }

        private string GetSaveFile(string ext, string filter)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ext;
            dlg.Filter = filter;

            if (dlg.ShowDialog() == true)
            {
                var file = new FileInfo(dlg.FileName);
                return file.FullName;
            }

            return null;
        }
        #endregion
        */
    }
}
