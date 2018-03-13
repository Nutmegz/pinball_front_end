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

namespace PinballFrontEnd.ViewModel
{
    public class TableManagerViewModel : ViewModelBase
    {
        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<PinballSystem> SystemList { get; set; } = new ObservableCollection<PinballSystem>();
        public ObservableCollection<PinballTable> TableList { get; set; } = new ObservableCollection<PinballTable>();
        public MediaLocation MediaLocation { get; set; } = new MediaLocation();

        public PinballSystem SelectedSystem { get; set; }
        public PinballTable SelectedTable { get; set; }


        #region Obsolete
        private int procID;


        // Commands
        public ICommand SaveTableList { get { return new RelayCommand(SaveTables); } }
        public ICommand OpenTableList { get { return new RelayCommand(OpenTables); } }

        public ICommand Launch { get { return new RelayCommand(LaunchP); } }

        private PinballSystem FindSystem(PinballTable table)
        {
            return SystemList.Single(x => x.Name == table.System);
        }

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

                var storedata = new StoreData();
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
            StoreData storedata = JsonConvert.DeserializeObject<StoreData>(System.IO.File.ReadAllText(databasepath));
            SystemList = storedata.SystemList;
            TableList = storedata.TableList;
        }

        private void SaveData()
        {
            var storedata = new StoreData();
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

    }
}
