using HearMe.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using PlaylistsNET.Models;

namespace HearMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlayerViewModel _viewModel;
        private bool dragStarted = false;
        private System.Timers.Timer _timer;

        public MainWindow()
        {
            _viewModel = new PlayerViewModel(this);

            InitializeComponent();

            DataContext = _viewModel;

            _timer = new System.Timers.Timer();
            _timer.Interval = 300;
            _timer.Elapsed += _viewModel.UpdateBoundData;

            Closing += OnWindowClosing;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _viewModel.UnbindOnStopEvent();
            _viewModel.Stop();
            _timer.Dispose();
            _viewModel.Dispose();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void AddToPlaylist(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                _viewModel.AddFilesToPlaylist((string[])e.Data.GetData(DataFormats.FileDrop));                
            }
        }

        private void DeleteSelectedFile(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                _viewModel.RemoveFromPlaylist(playlist.SelectedItems);
            }
        }

        private void PlaySongFromPlaylist(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem clickedSong = (ListBoxItem)sender;

            _viewModel.PlayFile(_viewModel.Playlist.Files.IndexOf((M3uPlaylistEntry)clickedSong.Content));
            _timer.Start();
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _viewModel.Play();
            _timer.Start();
        }

        private void ShowPlaylist(object sender, RoutedEventArgs e)
        {
            DoubleAnimation db = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.3),
                From = -this.Left,
                To = this.Width
            };

            slideToggle.BeginAnimation(TranslateTransform.XProperty, db);
        }

        private void HidePlaylist(object sender, RoutedEventArgs e)
        {
            DoubleAnimation db = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.3),
                To = -this.Left,
                From = this.Width
            };

            slideToggle.BeginAnimation(TranslateTransform.XProperty, db);
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            dragStarted = true;
            _timer.Stop();
        }

        private void GoToPosition(object sender, DragCompletedEventArgs e)
        {
            if (!dragStarted) return;

            // set position
            _viewModel.SetPosition(new TimeSpan(0, (int)(Math.Floor(seekBar.Value / 60)), (int)(Math.Floor(seekBar.Value % 60))));            

            dragStarted = false;
            _timer.Start();

        }
    }
}
