using HearMe.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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
            _timer.Elapsed += _viewModel.UpdateSeekPosition;

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

        public void Previous(object sender, RoutedEventArgs e)
        {
            _viewModel.MovePlaylistSong(-1);
        }

        public void Next(object sender, RoutedEventArgs e)
        {
            _viewModel.MovePlaylistSong(1);
        }

        private void ClearPlaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.Playlist.Files.Clear();
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

        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _viewModel.SetVolume((float)volumeBar.Value / (float)volumeBar.Maximum);
        }

        private void PlaySongFromPlaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.PlayFile(playlist.SelectedIndex);
            _timer.Start();
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _viewModel.Play();
            _timer.Start();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _viewModel.Stop();
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
