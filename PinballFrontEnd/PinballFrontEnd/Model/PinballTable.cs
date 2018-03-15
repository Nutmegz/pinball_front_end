using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using static System.IO.File;

namespace PinballFrontEnd.Model
{
    
    public class PinballTable : INotifyPropertyChanged
    {
        //Properties
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Rom { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Year { get; set; } = "";
        public bool ShowDMD { get; set; } = false;
        public bool ShowBackglass { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public String System { get; set; } = "";
        public String Keywords { get; set; } = "";

        //Media File Names
        [JsonIgnore]
        public Uri Playfield
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Playfield Video\\{Description}.mp4"); }
        }

        [JsonIgnore]
        public Uri PlayfieldImage
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Playfield Image\\{Description}.png"); }
        }

        [JsonIgnore]
        public Uri Backglass
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Backglass Video\\{Description}.mp4"); }
        }

        [JsonIgnore]
        public Uri BackglassImage
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Backglass Image\\{Description}.png"); }
        }

        [JsonIgnore]
        public Uri DMD
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\DMD Video\\{Description}.mp4"); }
        }

        [JsonIgnore]
        public Uri DMDImage
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\DMD Image\\{Description}.png"); }
        }

        [JsonIgnore]
        public Uri Wheel
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Wheel\\{Description}.png"); }
        }

        [JsonIgnore]
        public Uri BGMusic
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Table Audio\\{Description}.mp3"); }
        }

        [JsonIgnore]
        public Uri LMusic
        {
            get { return new Uri($"{ProgramPath.Value}Media\\{System}\\Launch Audio\\{Description}.mp3"); }
        }

        //MEDIA EXISTS PROPERTIES

        [JsonIgnore]
        public bool PlayfieldExists
        {
            get
            {
                return Exists(Playfield.LocalPath);
            }
        }
        [JsonIgnore]
        public bool PlayfieldImageExists
        {
            get
            {
                return Exists(PlayfieldImage.LocalPath);
            }
        }
        [JsonIgnore]
        public bool BackglassExists
        {
            get
            {
                return Exists(Backglass.LocalPath);
            }
        }
        [JsonIgnore]
        public bool BackglassImageExists
        {
            get
            {
                return Exists(BackglassImage.LocalPath);
            }
        }
        [JsonIgnore]
        public bool DMDExists
        {
            get
            {
                return Exists(DMD.LocalPath);
            }
        }
        [JsonIgnore]
        public bool DMDImageExists
        {
            get
            {
                return Exists(DMDImage.LocalPath);
            }
        }
        [JsonIgnore]
        public bool WheelExists
        {
            get
            {
                return Exists(Wheel.LocalPath);
            }
        }
        [JsonIgnore]
        public bool BGMusicExists
        {
            get
            {
                return Exists(BGMusic.LocalPath);
            }
        }
        [JsonIgnore]
        public bool LMusicExists
        {
            get
            {
                return Exists(LMusic.LocalPath);
            }
        }

        [JsonIgnore]
        public BitmapImage PlayfieldThumbnail { get; set; } = new BitmapImage();
        [JsonIgnore]
        public BitmapImage BackglassThumbnail { get; set; } = new BitmapImage();
        [JsonIgnore]
        public BitmapImage DMDThumbnail { get; set; } = new BitmapImage();
        [JsonIgnore]
        public BitmapImage WheelThumbnail { get; set; } = new BitmapImage();

        public void loadThumbnails(int scale)
        {
            try
            {

                if (PlayfieldImageExists) { PlayfieldThumbnail.BeginInit(); PlayfieldThumbnail.DecodePixelWidth = scale; PlayfieldThumbnail.CacheOption = BitmapCacheOption.OnLoad; PlayfieldThumbnail.UriSource = PlayfieldImage; PlayfieldThumbnail.EndInit(); }
                else { PlayfieldThumbnail.BeginInit(); PlayfieldThumbnail.CacheOption = BitmapCacheOption.OnLoad; PlayfieldThumbnail.UriSource = new Uri($@"{ProgramPath.Value}\Media\default.png"); PlayfieldThumbnail.EndInit(); }

                if (BackglassImageExists) { BackglassThumbnail.BeginInit(); BackglassThumbnail.DecodePixelWidth = scale; BackglassThumbnail.CacheOption = BitmapCacheOption.OnLoad; BackglassThumbnail.UriSource = BackglassImage; BackglassThumbnail.EndInit(); }
                else { BackglassThumbnail.BeginInit(); BackglassThumbnail.CacheOption = BitmapCacheOption.OnLoad; BackglassThumbnail.UriSource = new Uri($@"{ProgramPath.Value}\Media\default.png"); BackglassThumbnail.EndInit(); }

                if (DMDImageExists) { DMDThumbnail.BeginInit(); DMDThumbnail.DecodePixelWidth = scale; DMDThumbnail.CacheOption = BitmapCacheOption.OnLoad; DMDThumbnail.UriSource = DMDImage; DMDThumbnail.EndInit(); }
                else { DMDThumbnail.BeginInit(); DMDThumbnail.CacheOption = BitmapCacheOption.OnLoad; DMDThumbnail.UriSource = new Uri($@"{ProgramPath.Value}\Media\default.png"); DMDThumbnail.EndInit(); }

                if (WheelExists) { WheelThumbnail.BeginInit(); WheelThumbnail.CacheOption = BitmapCacheOption.OnLoad; WheelThumbnail.UriSource = Wheel; WheelThumbnail.EndInit(); }

            }
            catch (Exception)
            {

                throw;
            }
        }
       



        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyname)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

    }
}
