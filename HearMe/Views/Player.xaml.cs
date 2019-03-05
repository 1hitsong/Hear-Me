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

        public MainWindow()
        {
            _viewModel = new PlayerViewModel(this);

            InitializeComponent();

            DataContext = _viewModel;

            Closing += OnWindowClosing;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _viewModel.UnbindOnStopEvent();
            _viewModel.Stop();
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

        private void PlaySongFromPlaylist(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem clickedSong = (ListBoxItem)sender;

            _viewModel.PlayFile(_viewModel.Playlist.Files.IndexOf((M3uPlaylistEntry)clickedSong.Content));
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _viewModel.Play();
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
    }
}
