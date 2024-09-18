using LibVLCSharp.Shared;
using System;

namespace BernieMedia
{
    public interface IMediaService
    {
        MediaPlayer GetMediaPlayer();
        void PlayMedia(string filePath, IFileService fileService);
        void PauseMedia();
        void StopMedia();
        double GetCurrentTime();
        double GetTotalTime();
        void SetCurrentTime(double seconds);
        bool IsAudioPlaying { get; }
        bool IsVideoPlaying { get; }
        event Action MediaEnded;
    }
}
