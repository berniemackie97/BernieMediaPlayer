using System;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using NAudio.Wave;

namespace BernieMedia
{
    public class MediaService : IMediaService
    {
        private IWavePlayer _waveOutDevice;
        private AudioFileReader _audioFileReader;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private DispatcherTimer _timer;
        public bool IsAudioPlaying { get; private set; }
        public bool IsVideoPlaying { get; private set; }
        public event Action MediaEnded;

        public MediaService(DispatcherTimer timer)
        {
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            _timer = timer;
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            MediaEnded?.Invoke();
        }

        public MediaPlayer GetMediaPlayer()
        {
            return _mediaPlayer;
        }

        public void PlayMedia(string filePath, IFileService fileService)
        {
            if (IsAudioPlaying || IsVideoPlaying)
            {
                if (fileService.IsAudioFile(filePath))
                {
                    _waveOutDevice?.Play();
                }
                else if (fileService.IsVideoFile(filePath))
                {
                    _mediaPlayer.Play();
                }
            }
            else
            {
                if (fileService.IsAudioFile(filePath))
                {
                    // Play audio
                    _waveOutDevice = new WaveOut();
                    _audioFileReader = new AudioFileReader(filePath);
                    _waveOutDevice.Init(_audioFileReader);
                    _waveOutDevice.Play();
                    IsAudioPlaying = true;
                    IsVideoPlaying = false;
                    _timer.Start();
                }
                else if (fileService.IsVideoFile(filePath))
                {
                    // Play video
                    Media? media = new Media(_libVLC, new Uri(filePath));
                    _mediaPlayer.Play(media);
                    IsAudioPlaying = false;
                    IsVideoPlaying = true;
                }
            }
        }


        public void PauseMedia()
        {
            if (IsAudioPlaying)
            {
                _waveOutDevice?.Pause();
            }
            else if (IsVideoPlaying)
            {
                _mediaPlayer.Pause();
            }
            _timer.Stop();
        }

        public void StopMedia()
        {
            if (IsAudioPlaying)
            {
                _waveOutDevice?.Stop();
                _waveOutDevice?.Dispose();
                _audioFileReader?.Dispose();
                IsAudioPlaying = false;
            }
            else if (IsVideoPlaying)
            {
                _mediaPlayer.Stop();
                IsVideoPlaying = false;
            }
            _timer.Stop();
        }

        public double GetCurrentTime()
        {
            if (IsAudioPlaying && _audioFileReader != null)
            {
                return _audioFileReader.CurrentTime.TotalSeconds;
            }
            else if (IsVideoPlaying && _mediaPlayer != null)
            {
                return _mediaPlayer.Time / 1000.0;
            }
            return 0;
        }

        public double GetTotalTime()
        {
            if (IsAudioPlaying && _audioFileReader != null)
            {
                return _audioFileReader.TotalTime.TotalSeconds;
            }
            else if (IsVideoPlaying && _mediaPlayer != null)
            {
                return _mediaPlayer.Length / 1000.0;
            }
            return 0;
        }

        public void SetCurrentTime(double seconds)
        {
            if (IsAudioPlaying && _audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(seconds);
            }
            else if (IsVideoPlaying && _mediaPlayer != null)
            {
                _mediaPlayer.Time = (long)(seconds * 1000);
            }
        }
    }
}
