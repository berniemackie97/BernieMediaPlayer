using System.Collections.Generic;
using System.Windows.Controls;

namespace BernieMedia.services
{
    public class PlaylistService : IPlaylistService
    {
        private List<string> playlist = new List<string>();

        public void AddToPlaylist(string filePath)
        {
            playlist.Add(filePath);
        }

        public string GetNextMedia(int currentIndex)
        {
            if (currentIndex < playlist.Count - 1)
            {
                return playlist[currentIndex + 1];
            }
            return null;
        }

        public string GetPreviousMedia(int currentIndex)
        {
            if (currentIndex > 0)
            {
                return playlist[currentIndex - 1];
            }
            return null;
        }

        public int GetPlaylistCount()
        {
            return playlist.Count;
        }

        public string GetMediaAt(int index)
        {
            if (index >= 0 && index < playlist.Count)
            {
                return playlist[index];
            }
            return null;
        }

        public bool IsPlaylistEmpty()
        {
            return playlist.Count == 0;
        }

    }
}
