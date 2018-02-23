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
using DesktopWPFAppLowLevelKeyboardHook;
using System.Text.RegularExpressions;

namespace PinballFrontEnd.ViewModel
{
    public class TableManagerViewModel : ViewModelBase, IDisposable
    {
        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //Global Keyboard Listener Hook
        private LowLevelKeyboardListener _listener;

        public ObservableCollection<PinballSystem> SystemList { get; set; }
        public PinballSystem SelectedSystem { get; set; }
        public PinballTable SelectedTable { get; set; }



        public TableManagerViewModel()
        {
            //TableList = new ObservableCollection<PinballTable>();
            SystemList = new ObservableCollection<PinballSystem>();


            //Start keyboard listener and subscribe
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
        }

        //De-Register Keyboard Hook
        public void Dispose()
        {
            _listener.UnHookKeyboard();
        }

        //Handle Global Key Events
        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {


            if (e.KeyPressed == Key.Escape)
            {
                try
                {
                    var proc = Process.GetProcessesByName("Pinball FX3");

                    if (proc.Length > 0)
                    {
                        proc[0].Kill();
                    }

                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        // Commands
        public ICommand SaveTableList { get { return new RelayCommand(SaveTables); } }
        public ICommand OpenTableList { get { return new RelayCommand(OpenTables); } }

        public ICommand Launch { get { return new RelayCommand(LaunchP); } }

        private void LaunchP(object obj)
        {

            var regex = new Regex(@"\[TABLE_NAME\]");
            var param = regex.Replace(SelectedSystem.Parameters, SelectedTable.Name);

            regex = new Regex(@"\[SYSTEM_PATH\]");

            logger.Debug(param);
            var proc = new Process();
            proc.StartInfo.FileName = SelectedSystem.WorkingPath + "\\" + SelectedSystem.Executable;
            proc.StartInfo.Arguments = param;
            proc.Start();

        }


        #region Select Media

        public ICommand SelectPlayfield { get { return new RelayCommand(SelectPlayfieldFile); } }

        private void SelectPlayfieldFile(object obj)
        {
            var filename = GetOpenFile(".mp4", "Media File (*.mp4)|*.mp4");
            if (filename != null && File.Exists(filename))
            {
                SelectedTable.Playfield = new Uri(filename);
            }
        }

        public ICommand SelectBackglass { get { return new RelayCommand(SelectBackglassFile); } }

        private void SelectBackglassFile(object obj)
        {
            var filename = GetOpenFile(".mp4", "Media File (*.mp4)|*.mp4");
            if (filename != null && File.Exists(filename))
            {
                SelectedTable.Backglass = new Uri(filename);
            }
        }

        public ICommand SelectDMD { get { return new RelayCommand(SelectDMDFile); } }

        private void SelectDMDFile(object obj)
        {
            var filename = GetOpenFile(".mp4", "Media File (*.mp4)|*.mp4");
            if (filename != null && File.Exists(filename))
            {
                SelectedTable.DMD = new Uri(filename);
            }
        }

        public ICommand SelectWheel { get { return new RelayCommand(SelectWheelFile); } }

        private void SelectWheelFile(object obj)
        {
            var filename = GetOpenFile(".png", "Media File (*.png)|*.png");
            if (filename != null && File.Exists(filename))
            {
                SelectedTable.Wheel = new Uri(filename);
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

                System.IO.File.WriteAllText(filepath, JsonConvert.SerializeObject(SystemList, Formatting.Indented));
            }


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

    }
}
