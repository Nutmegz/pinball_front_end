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

namespace PinballFrontEnd.ViewModel
{
    public class TableManagerViewModel : ViewModelBase
    {

        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<PinballTable> _tableList;
        public ObservableCollection<PinballTable> TableList
        {
            get
            {
                return _tableList;
            }
            set
            {
                _tableList = value;
                NotifyPropertyChanged("TableList");
            }
        }


        public ObservableCollection<PinballSystem> _systemList;
        public ObservableCollection<PinballSystem> SystemList
        {
            get
            {
                return _systemList;
            }
            set
            {
                _systemList = value;
                NotifyPropertyChanged("SystemList");
            }
        }


        public TableManagerViewModel()
        {
            TableList = new ObservableCollection<PinballTable>();
            SystemList = new ObservableCollection<PinballSystem>();
        }

        // Commands
        public ICommand SaveTableList { get { return new RelayCommand(SaveTables); } }
        public ICommand OpenTableList { get { return new RelayCommand(OpenTables); } }

        //Save Tables
        private void SaveTables(object obj)
        {
            var filepath = GetSaveFile();

            if (filepath != null)
            {
                var saveData = new SaveFile
                {
                    PinballSystems = SystemList,
                    PinballTables = TableList
                };

                logger.Info("Saving Database: %s", filepath);

                System.IO.File.WriteAllText(filepath, JsonConvert.SerializeObject(saveData, Formatting.Indented));
            }

           
        }

        private void OpenTables(object obj)
        {
            //TableList = JsonConvert.DeserializeObject<ObservableCollection<PinballTable>>(System.IO.File.ReadAllText(GetOpenFile()));
            var filepath = GetOpenFile();

            if (filepath != null && System.IO.File.Exists(filepath))
            {
                SaveFile saveData = JsonConvert.DeserializeObject<SaveFile>(System.IO.File.ReadAllText(filepath));
                TableList = saveData.PinballTables;
                SystemList = saveData.PinballSystems;
            }

            
        }


        private string GetOpenFile()
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "Pinball Table Database (*.json)|*.json";

            if (dlg.ShowDialog() == true)
            {
                var file = new FileInfo(dlg.FileName);
                return file.FullName;
            }

            return null;
        }

        private string GetSaveFile()
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "Pinball Table Database (*.json)|*.json";

            if(dlg.ShowDialog() == true)
            {
                var file = new FileInfo(dlg.FileName);
                return file.FullName;
            }

            return null;
        }

    }
}
