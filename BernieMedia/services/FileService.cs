using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace BernieMedia.services
{
    internal class FileService : IFileService
    {
        public string SelectMediaFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp3;*.wav;*.mp4;*.avi)|*.mp3;*.wav;*.mp4;*.avi";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                if (IsAudioFile(filePath) || IsVideoFile(filePath))
                {
                    return filePath;
                }
                else
                {
                    MessageBox.Show("Invalid file type. Please select a valid media file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        public string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public bool IsAudioFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".mp3" || extension == ".wav";
        }

        public bool IsVideoFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".mp4" || extension == ".avi";
        }
    }
}
