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
    public class TableManagerViewModel : ViewModelBase, IDisposable
    {
        //allow only one expander to be open at a time
        string _CurrentExpanded;
        public string CurrentExpanded
        {
            get
            {
                return _CurrentExpanded;
            }
            set
            {
                if (_CurrentExpanded != value)
                {
                    _CurrentExpanded = value;
                    //RaisePropertyChanged("CurrentExpanded");
                }
            }
        }


        //Setup Class Logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PinballSystem SelectedSystem { get; set; }
        public PinballTable SelectedTable { get; set; }

        public bool TopMost { get; set; } = false;

        public PinballData Data { get; set; }

        private string databasepath = "";

        public TableManagerViewModel(string databasepath)
        {
            logger.Info($"Starting Table Manager: {databasepath}");
            this.databasepath = databasepath;
            Data = new PinballData(this.databasepath);
            InitalizeDummyWindows();
        }

        public TableManagerViewModel(PinballData data)
        {
            logger.Info("Starting Table Manager");
            this.Data = data;
            InitalizeDummyWindows();
        }

        private Window PlayfieldWindowDummy;
        private Window BackglassWindowDummy;
        private Window DMDWindowDummy;

        // Commands
        public ICommand DummyWindowButton { get { return new RelayCommand(ShowHideDummyWindows); } }
        public ICommand GenerateThumbnailsCommand { get { return new RelayCommand(GenerateThumbnails); } }

        #region PreviewMedia
        //Preview Media
        public ICommand PreviewPlayfieldCommand { get { return new RelayCommand(PreviewPlayfield); } }
        public ICommand PreviewBackglassCommand { get { return new RelayCommand(PreviewBackglass); } }
        public ICommand PreviewDMDCommand { get { return new RelayCommand(PreviewDMD); } }
        public ICommand PreviewWheelCommand { get { return new RelayCommand(PreviewWheel); } }
        public ICommand PreviewTableAudioCommand { get { return new RelayCommand(PreviewTableAudio); } }
        public ICommand PreviewLaunchAudioCommand { get { return new RelayCommand(PreviewLaunchAudio); } }



        private void PreviewPlayfield(object obj)
        {
            if (SelectedTable == null)
                return;
            PreviewMusicVideo(SelectedTable.Playfield);
            SelectedTable.RunAudit();
        }

        private void PreviewBackglass(object obj)
        {
            if (SelectedTable == null)
                return;
            PreviewMusicVideo(SelectedTable.Backglass);
            SelectedTable.RunAudit();
        }

        private void PreviewDMD(object obj)
        {
            if (SelectedTable == null)
                return;
            PreviewMusicVideo(SelectedTable.DMD);
            SelectedTable.RunAudit();
        }

        private void PreviewWheel(object obj)
        {
            if (SelectedTable == null)
                return;
            PreviewImage(SelectedTable.Wheel);
            SelectedTable.RunAudit();
        }

        private void PreviewTableAudio(object obj)
        {
            if (SelectedTable == null)
                return;
            PreviewMusicVideo(SelectedTable.BGMusic);
            SelectedTable.RunAudit();
        }

        private void PreviewLaunchAudio(object obj)
        {
            if (SelectedTable == null)
                return;
            PreviewMusicVideo(SelectedTable.LMusic);
            SelectedTable.RunAudit();
        }

        private void PreviewMusicVideo(Uri path)
        {
            if (File.Exists(path.LocalPath))
            {
                var p = new Process();
                p.StartInfo.FileName = $@"{Properties.Settings.Default.PATH_VLC}\vlc.exe";
                p.StartInfo.Arguments = $"\"{path.LocalPath}\"";
                p.Start();
            }
        }

        private void PreviewImage(Uri path)
        {
            if (File.Exists(path.LocalPath))
            {
                var p = new Process();
                p.StartInfo.FileName = $@"{path.LocalPath}";
                p.Start();
            }
        }
        #endregion

        #region ImportMedia

        public ICommand GetPlayfieldCommand { get { return new RelayCommand(ImportPlayfield); } }
        public ICommand GetBackglassCommand { get { return new RelayCommand(ImportBackglass); } }
        public ICommand GetDMDCommand { get { return new RelayCommand(ImportDMD); } }
        public ICommand GetWheelCommand { get { return new RelayCommand(ImportWheel); } }
        public ICommand GetLaunchAudioCommand { get { return new RelayCommand(ImportLaunchAudio); } }
        public ICommand GetTableAudioCommand { get { return new RelayCommand(ImportTableAudio); } }

        public delegate void MediaImportStartedEventHandler(object source, PinballTable table);
        public event MediaImportStartedEventHandler MediaImportStarted;

        protected virtual void OnMediaImportStarted(PinballTable table)
        {
            MediaImportStarted?.Invoke(this, table);
        }

        public delegate void MediaImportFinishedEventHandler(object source, PinballTable table);
        public event MediaImportFinishedEventHandler MediaImportFinished;

        protected virtual void OnMediaImportFinished(PinballTable table)
        {
            MediaImportFinished?.Invoke(this, table);
        }

        private async void ImportPlayfield(object obj)
        {
            await Task.Run(() =>
            {
                ImportMedia(SelectedTable, ImportMediaType.Playfield, Data.MediaLocation.ConvertOnImport);

                SelectedTable.RunAudit();
            });

        }

        private async void ImportBackglass(object obj)
        {
            await Task.Run(() =>
            {
                ImportMedia(SelectedTable, ImportMediaType.Backglass, Data.MediaLocation.ConvertOnImport);
                SelectedTable.RunAudit();
            });

        }

        private async void ImportDMD(object obj)
        {
            await Task.Run(() =>
            {
                ImportMedia(SelectedTable, ImportMediaType.Dmd, Data.MediaLocation.ConvertOnImport);
                SelectedTable.RunAudit();
            });

        }

        private async void ImportWheel(object obj)
        {
            await Task.Run(() =>
            {
                ImportMedia(SelectedTable, ImportMediaType.Wheel);
                SelectedTable.RunAudit();
            });

        }

        private async void ImportLaunchAudio(object obj)
        {
            await Task.Run(() =>
            {
                ImportMedia(SelectedTable, ImportMediaType.LaunchAudio);
                SelectedTable.RunAudit();
            });

        }

        private async void ImportTableAudio(object obj)
        {
            await Task.Run(() =>
            {
                ImportMedia(SelectedTable, ImportMediaType.TableAudio);
                SelectedTable.RunAudit();
            });

        }

        private enum ImportFileType
        {
            Video,
            Audio,
            Image,
            Any
        }
        private enum ImportMediaType
        {
            Playfield,
            Backglass,
            Dmd,
            Wheel,
            LaunchAudio,
            TableAudio
        }

        private ImportFileType GetFileType(ImportMediaType mt)
        {
            switch (mt)
            {
                case ImportMediaType.Playfield:
                    return ImportFileType.Video;
                case ImportMediaType.Backglass:
                    return ImportFileType.Video;
                case ImportMediaType.Dmd:
                    return ImportFileType.Video;
                case ImportMediaType.Wheel:
                    return ImportFileType.Image;
                case ImportMediaType.LaunchAudio:
                    return ImportFileType.Audio;
                case ImportMediaType.TableAudio:
                    return ImportFileType.Audio;
            }
            return ImportFileType.Any;
        }

        private string ImportGetSourceFilePath(PinballTable table, ImportFileType type)
        {
            switch (type)
            {
                case ImportFileType.Video:
                    return FileUtility.OpenFileDatalogMP4();
                case ImportFileType.Audio:
                    return FileUtility.OpenFileDatalogMP3();
                case ImportFileType.Image:
                    return FileUtility.OpenFileDatalogPNG();
            }
            return null;
        }

        private string ImportGetDestinationFilePath(PinballTable table, ImportMediaType type)
        {
            switch (type)
            {
                case ImportMediaType.Playfield:
                    return table.Playfield.LocalPath;
                case ImportMediaType.Backglass:
                    return table.Backglass.LocalPath;
                case ImportMediaType.Dmd:
                    return table.DMD.LocalPath;
                case ImportMediaType.Wheel:
                    return table.Wheel.LocalPath;
                case ImportMediaType.LaunchAudio:
                    return table.LMusic.LocalPath;
                case ImportMediaType.TableAudio:
                    return table.LMusic.LocalPath;
            }
            return null;
        }

        private async void ImportMedia(PinballTable table, ImportMediaType type, bool convert = false)
        {
            if (table != null)
                if (table.GetType().Equals(typeof(PinballTable)))
                {
                    var tempTable = table; //Store a temp version
                    OnMediaImportStarted(tempTable); //Raise Event To Stop Videos
                    var sourceFilePath = ImportGetSourceFilePath(table, GetFileType(type));
                    var destinationFilePath = ImportGetDestinationFilePath(table, type);

                    //Exit if nothing selected
                    if (!File.Exists(sourceFilePath))
                    {
                        OnMediaImportFinished(tempTable);
                        return;
                    }


                    //Convert if video
                    if (convert && GetFileType(type) == ImportFileType.Video)
                        await Task.Run(() => ImportConvertFile(sourceFilePath, destinationFilePath, type));
                    else
                        await Task.Run(() => ImportFile(sourceFilePath, destinationFilePath));

                    OnMediaImportFinished(tempTable); //Raise Event To Start New Videos
                }
        }

        private async void ImportConvertFile(string sourcePath, string destinationPath, ImportMediaType type)
        {
            //make directory if it doesn't exist
            FileUtility.CreateDirectory(Path.GetDirectoryName(destinationPath));

            //Only work if source file exists
            if (File.Exists(sourcePath))
            {
                //Rotate for Playfield
                if (type == ImportMediaType.Playfield)
                    await Task.Run(() => ImportConvertVideo(sourcePath, destinationPath, ConvertOptions.Rotate180));
                else
                    await Task.Run(() => ImportConvertVideo(sourcePath, destinationPath, ConvertOptions.None));
            }
        }

        private async void ImportFile(string sourcePath, string destinationPath)
        {
            FileUtility.CreateDirectory(Path.GetDirectoryName(destinationPath));
            if (File.Exists(sourcePath))
                await Task.Run(() => File.Copy(sourcePath, destinationPath, true));
        }

        private enum ConvertOptions
        {
            None,
            Rotate180
        }

        private async void ImportConvertVideo(string sourcePath, string destinationPath, ConvertOptions options)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = $@"{Properties.Settings.Default.PATH_FFMPEG}\bin\ffmpeg.exe";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            switch (options)
            {
                case ConvertOptions.Rotate180:
                    proc.StartInfo.Arguments = $"-y -i \"{sourcePath}\" -c:a copy -vf \"transpose = 2,transpose = 2\" -c:v libx264 -bf 2 -flags +cgop -pix_fmt yuv420p -crf 17 -preset fast -movflags +faststart \"{destinationPath}\"";
                    break;
                default:
                    proc.StartInfo.Arguments = $"-y -i \"{sourcePath}\" -c:a copy -c:v libx264 -bf 2 -flags +cgop -pix_fmt yuv420p -crf 17 -preset fast -movflags +faststart \"{destinationPath}\"";
                    break;
            }

            proc.Start();
            await Task.Run(() => proc.WaitForExit());

        }

        #endregion

        #region Launch Table
        public ICommand StartTableCommand { get { return new RelayCommand(StartTable); } }

        private void StartTable(object obj)
        {
            PinballFunctions.StartTable(Data.FindSystem(SelectedTable), SelectedTable);
        }
        #endregion

        //windows used to show location where media will be played
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

        //Convert Videos to thumbnails
        private async void GenerateThumbnails(object obj)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = $@"{Properties.Settings.Default.PATH_FFMPEG}\bin\ffmpeg.exe";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            foreach (PinballTable pt in Data.TableList)
            {
                await Task.Run(() => GenerateThumbnail(pt));
            }
        }

        private async void GenerateThumbnail(PinballTable table)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = $@"{Properties.Settings.Default.PATH_FFMPEG}\bin\ffmpeg.exe";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            if (table.PlayfieldExists)
            {
                proc.StartInfo.Arguments = $"-y -i \"{table.Playfield.LocalPath}\" -vframes 1 -f image2 \"{table.PlayfieldImage.LocalPath}\"";
                proc.Start();
                await Task.Run(() => proc.WaitForExit());
            }
            if (table.BackglassExists)
            {
                proc.StartInfo.Arguments = $"-y -i \"{table.Backglass.LocalPath}\" -vframes 1 -f image2 \"{table.BackglassImage.LocalPath}\"";
                proc.Start();
                await Task.Run(() => proc.WaitForExit());
            }
            if (table.DMDExists)
            {
                proc.StartInfo.Arguments = $"-y -i \"{table.DMD.LocalPath}\" -vframes 1 -f image2 \"{table.DMDImage.LocalPath}\"";
                proc.Start();
                await Task.Run(() => proc.WaitForExit());
            }

            table.RunAudit(); //update visuals

        }

        //Show Dummy Windows for location
        private void ShowHideDummyWindows(object obj)
        {
            if (PlayfieldWindowDummy.IsVisible)
            {
                PlayfieldWindowDummy.Visibility = Visibility.Hidden;
                TopMost = false;
            }
            else
            {
                PlayfieldWindowDummy.Visibility = Visibility.Visible;
                TopMost = true;
            }


            if (BackglassWindowDummy.IsVisible)
            {
                BackglassWindowDummy.Visibility = Visibility.Hidden;
                TopMost = false;
            }
            else
            {
                BackglassWindowDummy.Visibility = Visibility.Visible;
                TopMost = true;
            }


            if (DMDWindowDummy.IsVisible)
            {
                DMDWindowDummy.Visibility = Visibility.Hidden;
                TopMost = false;
            }
            else
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


        //Clean things up and save database
        public void Dispose()
        {
            logger.Info("Closing Table Manager");
            if (databasepath != "")
                Data.SaveDatabase(databasepath);
            
        }

    }
}
