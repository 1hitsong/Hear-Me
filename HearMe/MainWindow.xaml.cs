using HearMe.Controllers;
using Id3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace HearMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlayerController _controller;

        private string _displayText;
        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                _displayText = value;
                OnPropertyChanged("DisplayText");
            }
        }

        private string _songTitle;
        public string SongTitle
        {
            get { return _songTitle; }
            set
            {
                _songTitle = value;
                OnPropertyChanged("SongTitle");
            }
        }

        private ObservableCollection<Song> _playlist;
        public ObservableCollection<Song> Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                OnPropertyChanged("Playlist");
            }
        }

        private double _songPosition;
        public double SongPosition
        {
            get { return _songPosition; }
            set
            {
                _songPosition = value;
                OnPropertyChanged("SongPosition");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Playlist = new ObservableCollection<Song>();

            _controller = new PlayerController(this);
            songTime.DataContext = this;
            seekBar.DataContext = this;
            songTitle.DataContext = this;
            playlist.DataContext = this;
        }

        public void PlayFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFile = (string[])e.Data.GetData(DataFormats.FileDrop);
                _controller.PlayFile(@droppedFile[0]);

                UpdateSongInformationDisplay(@droppedFile[0]);

                seekBar.Maximum = _controller.Length;

                var timer = new System.Timers.Timer();
                timer.Interval = 300;
                timer.Elapsed += UpdateSeekPosition;
                timer.Start();
            }
        }

        public void PlayFile(string songFilename)
        {
            _controller.PlayFile(songFilename);

            UpdateSongInformationDisplay(songFilename);

            seekBar.Maximum = _controller.Length;

            var timer = new System.Timers.Timer();
            timer.Interval = 300;
            timer.Elapsed += UpdateSeekPosition;
            timer.Start();
        }

        public void Previous(object sender, RoutedEventArgs e)
        {
            if (playlist.SelectedIndex == 0) return;

            _controller.Stop();
            playlist.SelectedIndex -= 1;

            Song selectedSong = (Song)playlist.SelectedItem;
            PlayFile(selectedSong.FileName);
        }

        public void Next(object sender, RoutedEventArgs e)
        {
            if (playlist.SelectedIndex + 1 == Playlist.Count) return;

            _controller.Stop();
            playlist.SelectedIndex += 1;

            Song selectedSong = (Song)playlist.SelectedItem;
            PlayFile(selectedSong.FileName);
        }

        private void PlaySongFromPlaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Song selectedSong = (Song)playlist.SelectedItem;
            PlayFile(selectedSong.FileName);
        }

        public void AddToPlaylist(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFile = (string[])e.Data.GetData(DataFormats.FileDrop);

                using (var mp3 = new Mp3(@droppedFile[0]))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);

                    Song addedSong = new Song{
                        FileName = @droppedFile[0],
                        Title = tag == null ? "Unknown Track" : tag.Title.ToString().Replace("\0", ""),
                        Artist = tag == null ? "Unknown Artist" : tag.Artists.ToString().Replace("\0", "")
                    };

                    Playlist.Add(addedSong);
                }
            }
        }

        public void UpdateSongInformationDisplay(string musicFile)
        {
            using (var mp3 = new Mp3(musicFile))
            {
                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                SongTitle = tag == null ? "Unknown Artist - Unknown Track" : tag.Artists.ToString().Replace("\0", "") + "-" + tag.Title.ToString().Replace("\0", "");
            }
        }

        private void UpdateSeekPosition(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SongPosition = _controller.SongPosition - (_controller.SongFormat.AverageBytesPerSecond * 2.5);
                UpdateSongInformation();
            });
        }

        private void UpdateSongInformation()
        {
            DisplayText = _controller.CurrentTime + " / " + _controller.TotalTime;
        }

        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _controller.SetVolume((float)volumeBar.Value / (float)volumeBar.Maximum);
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _controller.Play();
            seekBar.Maximum = _controller.Length;

            var timer = new System.Timers.Timer();
            timer.Interval = 300;
            timer.Elapsed += UpdateSeekPosition;
            timer.Start();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _controller.Stop();
        }

        private void GoToPosition(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            long newPos = (long)seekBar.Value + (long)(_controller.SongFormat.AverageBytesPerSecond * 2.5);
            // Force it to align to a block boundary
            if ((newPos % _controller.SongFormat.BlockAlign) != 0)
                newPos -= newPos % _controller.SongFormat.BlockAlign;
            // Force new position into valid range
            newPos = Math.Max(0, Math.Min(_controller.Length, newPos));
            // set position
            _controller.SetPosition(newPos);

            UpdateSongInformation();

        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
