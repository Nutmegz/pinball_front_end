using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PinballFrontEnd.Model
{
    public static class FileUtility
    {

        public static string OpenFileDatalogPNG()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image FIle (*.png)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                    return openFileDialog.FileName;
            }

            return "";
        }

        public static string OpenFileDatalogMP4()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video File (*.mp4)|*.mp4";
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                    return openFileDialog.FileName;
            }

            return "";
        }

        public static string OpenFileDatalogMP3()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio File (*.mp3)|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                    return openFileDialog.FileName;
            }

            return "";
        }

        public static void CreateDirectory(string filePath)
        {
            if (!System.IO.Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }
        }

    }
}
