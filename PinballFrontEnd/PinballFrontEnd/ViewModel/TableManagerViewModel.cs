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

namespace PinballFrontEnd.ViewModel
{
    public class TableManagerViewModel : ViewModelBase
    {
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

        public TableManagerViewModel()
        {
            TableList = new ObservableCollection<PinballTable>();
        }

        // Commands
        public ICommand SaveTableList { get { return new RelayCommand(SaveTables); } }
        public ICommand OpenTableList { get { return new RelayCommand(OpenTables); } }

        //Save Tables
        private void SaveTables(object obj)
        {
            System.IO.File.WriteAllText(GetSaveFile(), JsonConvert.SerializeObject(TableList,Formatting.Indented));
        }

        private void OpenTables(object obj)
        {
            TableList = JsonConvert.DeserializeObject<ObservableCollection<PinballTable>>(System.IO.File.ReadAllText(GetOpenFile()));
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
