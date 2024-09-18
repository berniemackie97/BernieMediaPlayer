using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BernieMedia.services;
using LibVLCSharp.WPF;

namespace BernieMedia
{
    public partial class MainWindow : Window
    {
        private readonly IMediaService _mediaService;
        private readonly IFileService _fileService;
        private readonly IPlaylistService _playlistService;

        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += Timer_Tick;

            _mediaService = new MediaService(timer);
            _mediaService.MediaEnded += MediaService_MediaEnded;
            VideoView.MediaPlayer = _mediaService.GetMediaPlayer();

            _fileService = new FileService();
            _playlistService = new PlaylistService();

            DataContext = this;
        }

        public ICommand UploadCommand => new RelayCommand(UploadMedia);
        public ICommand PlayCommand => new RelayCommand(PlayMedia);
        public ICommand StopCommand => new RelayCommand(StopMedia);
        public ICommand NextCommand => new RelayCommand(PlayNextMedia);
        public ICommand BackCommand => new RelayCommand(PlayPreviousMedia);

        private void UploadMedia()
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
                    _mediaService.PlayMedia(filePath, _fileService);
                    PlayButton.Content = "Pause";
                }

                Playlist.SelectedIndex = Playlist.Items.Count - 1;
                UpdateNavigationButtons();
            }
        }

        private void PlayMedia()
        {
            if (PlayButton.Content.ToString() == "Play")
            {
                if (Playlist.Items.Count > 0)
                {
                    string filePath = _playlistService.GetMediaAt(Playlist.SelectedIndex);
                    _mediaService.PlayMedia(filePath, _fileService);
                    PlayButton.Content = "Pause";
                    StopButton.IsEnabled = true;
                }
            }
            else
            {
                _mediaService.PauseMedia();
                PlayButton.Content = "Play";
            }
        }

        private void StopMedia()
        {
            _mediaService.StopMedia();
            PlayButton.Content = "Play";
            StopButton.IsEnabled = false;
        }

        private void PlayNextMedia()
        {
            int currentIndex = Playlist.SelectedIndex;
            string filePath = _playlistService.GetNextMedia(currentIndex);
            if (filePath != null)
            {
                _mediaService.PlayMedia(filePath, _fileService);
                Playlist.SelectedIndex = currentIndex + 1;
                PlayButton.Content = "Pause";
                StopButton.IsEnabled = true;
            }
            UpdateNavigationButtons();
        }

        private void PlayPreviousMedia()
        {
            int currentIndex = Playlist.SelectedIndex;
            string filePath = _playlistService.GetPreviousMedia(currentIndex);
            if (filePath != null)
            {
                _mediaService.PlayMedia(filePath, _fileService);
                Playlist.SelectedIndex = currentIndex - 1;
                PlayButton.Content = "Pause";
                StopButton.IsEnabled = true;
            }
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            int currentIndex = Playlist.SelectedIndex;
            NextButton.IsEnabled = currentIndex < _playlistService.GetPlaylistCount() - 1;
            BackButton.IsEnabled = currentIndex > 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSlider.Value = _mediaService.GetCurrentTime();
            TimeSlider.Maximum = _mediaService.GetTotalTime();
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaService.SetCurrentTime(TimeSlider.Value);
        }

        private void MediaService_MediaEnded()
        {
            PlayNextMedia();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
