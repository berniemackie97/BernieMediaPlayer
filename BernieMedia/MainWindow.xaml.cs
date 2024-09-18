using System;
using System.Windows;
using System.Windows.Input;
using BernieMedia.helpers;
using BernieMedia.services;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using System.Threading.Tasks;

namespace BernieMedia
{
    public partial class MainWindow : Window
    {
        private readonly IMediaService _mediaService;
        private readonly IFileService _fileService;
        private readonly IPlaylistService _playlistService;
        private bool _isSliderDragging;

        public MainWindow()
        {
            InitializeComponent();

            _mediaService = new MediaService();
            _mediaService.MediaEnded += MediaService_MediaEnded;
            VideoView.MediaPlayer = _mediaService.GetMediaPlayer();

            _fileService = new FileService();
            _playlistService = new PlaylistService();

            DataContext = this;

            // Subscribe to the TimeChanged event
            VideoView.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
        }

        public ICommand UploadCommand => new RelayCommand(UploadMedia);
        public ICommand PlayCommand => new RelayCommand(PlayMedia);
        public ICommand StopCommand => new RelayCommand(StopMedia);
        public ICommand NextCommand => new RelayCommand(PlayNextMedia);
        public ICommand BackCommand => new RelayCommand(PlayPreviousMedia);

        private async void UploadMedia()
        {
            string filePath = _fileService.SelectMediaFile();
            if (!string.IsNullOrEmpty(filePath))
            {
                PlayButton.IsEnabled = true;
                StopButton.IsEnabled = true;

                bool wasPlaylistEmpty = _playlistService.IsPlaylistEmpty();
                _playlistService.AddToPlaylist(filePath);
                Playlist.Items.Add(filePath);

                if (wasPlaylistEmpty)
                {
                    await Task.Run(() => _mediaService.PlayMedia(filePath, _fileService));
                    Dispatcher.Invoke(() => PlayButton.Content = "Pause");
                }

                Playlist.SelectedIndex = Playlist.Items.Count - 1;
                UpdateNavigationButtons();
            }
        }

        private async void PlayMedia()
        {
            if (PlayButton.Content.ToString() == "Play")
            {
                if (Playlist.Items.Count > 0)
                {
                    string filePath = _playlistService.GetMediaAt(Playlist.SelectedIndex);
                    await Task.Run(() => _mediaService.PlayMedia(filePath, _fileService));
                    Dispatcher.Invoke(() =>
                    {
                        PlayButton.Content = "Pause";
                        StopButton.IsEnabled = true;
                    });
                }
            }
            else
            {
                await Task.Run(() => _mediaService.PauseMedia());
                Dispatcher.Invoke(() => PlayButton.Content = "Play");
            }
        }

        private async void StopMedia()
        {
            await Task.Run(() => _mediaService.StopMedia());
            Dispatcher.Invoke(() =>
            {
                PlayButton.Content = "Play";
                StopButton.IsEnabled = false;
            });
        }

        private async void PlayNextMedia()
        {
            int currentIndex = Playlist.SelectedIndex;
            string filePath = _playlistService.GetNextMedia(currentIndex);
            if (filePath != null)
            {
                await Task.Run(() => _mediaService.PlayMedia(filePath, _fileService));
                Dispatcher.Invoke(() =>
                {
                    Playlist.SelectedIndex = currentIndex + 1;
                    PlayButton.Content = "Pause";
                    StopButton.IsEnabled = true;
                });
            }
            UpdateNavigationButtons();
        }

        private async void PlayPreviousMedia()
        {
            int currentIndex = Playlist.SelectedIndex;
            string filePath = _playlistService.GetPreviousMedia(currentIndex);
            if (filePath != null)
            {
                await Task.Run(() => _mediaService.PlayMedia(filePath, _fileService));
                Dispatcher.Invoke(() =>
                {
                    Playlist.SelectedIndex = currentIndex - 1;
                    PlayButton.Content = "Pause";
                    StopButton.IsEnabled = true;
                });
            }
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            int currentIndex = Playlist.SelectedIndex;
            NextButton.IsEnabled = currentIndex < _playlistService.GetPlaylistCount() - 1;
            BackButton.IsEnabled = currentIndex > 0;
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            // Use the Dispatcher to update the UI from the UI thread
            Dispatcher.Invoke(() =>
            {
                if (!_isSliderDragging)
                {
                    // Update the slider value based on the current playback time
                    TimeSlider.Value = e.Time / 1000.0; // Convert milliseconds to seconds
                    TimeSlider.Maximum = VideoView.MediaPlayer.Length / 1000.0; // Convert milliseconds to seconds
                }
            });
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSliderDragging)
            {
                // Set the media player's current time based on the slider value
                VideoView.MediaPlayer.Time = (long)(TimeSlider.Value * 1000); // Convert seconds to milliseconds
            }
        }

        private void TimeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isSliderDragging = true;
        }

        private void TimeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isSliderDragging = false;
            // Set the media player's current time based on the slider value
            VideoView.MediaPlayer.Time = (long)(TimeSlider.Value * 1000); // Convert seconds to milliseconds
        }


        private void MediaService_MediaEnded()
        {
            PlayNextMedia();
        }
    }
}
