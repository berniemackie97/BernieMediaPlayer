using LibVLCSharp.Shared;
using System;

namespace BernieMedia
{
    public interface IPlaylistService
    {
        void AddToPlaylist(string filePath);
        string GetNextMedia(int currentIndex);
        string GetPreviousMedia(int currentIndex);
        int GetPlaylistCount();
        string GetMediaAt(int index);
        bool IsPlaylistEmpty();
    }
}
