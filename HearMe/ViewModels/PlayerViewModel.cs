using System;
using System.ComponentModel;
using HearMe.Models;

using CSCore;
using System.IO;
using System.Windows.Media.Imaging;

namespace HearMe.ViewModels
{
    class PlayerViewModel : IDisposable, INotifyPropertyChanged
    {
        AudioPlayer audioPlayer;
        MainWindow PlayerView;

        public PlayerViewModel(MainWindow view)
        {
            PlayerView = view;
            Playlist = new Playlist();
        }

        private int PlayingSongPlaylistIndex { get; set;}

        private string _playingsongTitle;
        public string PlayingSongTitle
        {
            get { return _playingsongTitle; }
            set
            {
                _playingsongTitle = value;
                OnPropertyChanged("PlayingSongTitle");
            }
        }

        private BitmapImage _albumArt;
        public BitmapImage AlbumArt
        {
            get { return _albumArt; }
            set
            {
                _albumArt = value;
                OnPropertyChanged("AlbumArt");
            }
        }

        private Double _playingsongLength;
        public Double PlayingSongLength
        {
            get { return _playingsongLength; }
            set
            {
                _playingsongLength = value;
                OnPropertyChanged("PlayingSongLength");
            }
        }

        private double _playingsongPosition;
        public double PlayingSongPosition
        {
            get { return _playingsongPosition; }
            set
            {
                _playingsongPosition = value;
                OnPropertyChanged("PlayingSongPosition");
            }
        }

        public float Volume { get; set; }

        public TimeSpan CurrentTime
        {
            get { return (audioPlayer != null) ? audioPlayer.AudioFile.GetPosition() : TimeSpan.Zero; }
        }

        public TimeSpan TotalTime
        {
            get { return (audioPlayer != null) ? audioPlayer.AudioFile.GetLength() : TimeSpan.Zero; }
        }

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

        public Playlist Playlist { get; set; }


        public void AddFilesToPlaylist(string[] filesToAdd)
        {
            foreach (string file in filesToAdd)
            {
                FileInfo pathInfo = new FileInfo(file);

                if (pathInfo.Attributes.ToString() == "Directory")
                {
                    AddFilesToPlaylist(Directory.GetFiles(file, "*.mp3", SearchOption.AllDirectories));
                }

                if (pathInfo.Extension != ".mp3")
                {
                    continue;
                }

                Playlist.Add(new Song(file));
            }
        }

        public void UnbindOnStopEvent()
        {
            if (audioPlayer != null)
                audioPlayer.OutputDevice.Stopped -= PlaybackDevicePlaybackStopped;
        }

        public void PlayFile(int playlistIndex)
        {
            Song selectedSong = Playlist.ElementAt(playlistIndex);

            if (selectedSong == null)
            {
                return;
            }

            string fileLocation = selectedSong.FileName;

            if (!System.IO.File.Exists(fileLocation))
            {
                return;
            }

            if (audioPlayer != null)
            {
                Dispose();
            }

            PlayingSongPlaylistIndex = playlistIndex;

            UpdateSongInformationDisplay(selectedSong);

            audioPlayer = new AudioPlayer(@fileLocation);

            audioPlayer.Play();

            PlayingSongLength = TotalTime.TotalSeconds;

            audioPlayer.OutputDevice.Volume = Volume;

            audioPlayer.OutputDevice.Stopped += PlaybackDevicePlaybackStopped;
        }

        public void UpdateSeekPosition(object sender, System.Timers.ElapsedEventArgs e)
        {
            PlayingSongPosition = CurrentTime.TotalSeconds;

            UpdateSongInformation();
        }

        public void MovePlaylistSong(sbyte indexMovementDirection)
        {
            // Don't allow movement beyond last playlist song
            if (PlayingSongPlaylistIndex + indexMovementDirection == Playlist.Files.Count)
                return;

            // Don't allow movement below 0
            if (PlayingSongPlaylistIndex + indexMovementDirection < 0)
                return;

            PlayingSongPlaylistIndex += indexMovementDirection;

            PlayFile(PlayingSongPlaylistIndex);
        }

        private void UpdateSongInformation()
        {
            DisplayText = CurrentTime.ToString("mm\\:ss") + " / " + TotalTime.ToString("mm\\:ss");
        }

        void PlaybackDevicePlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (CurrentTime.Equals(TotalTime))
            {
                MovePlaylistSong(1);
            }
        }

        public void Play()
        {
            if (audioPlayer == null)
            {
                return;
            }

            audioPlayer.OutputDevice.Volume = Volume;

            audioPlayer.Play();
        }

        public void Stop()
        {
            if (audioPlayer != null)
                audioPlayer.OutputDevice.Stop();
        }

        public void SetVolume(float volumeLevel)
        {
            Volume = volumeLevel;

            if (audioPlayer != null)
                audioPlayer.OutputDevice.Volume = Volume;
        }

        public void SetPosition(TimeSpan newPosition)
        {
            audioPlayer.AudioFile.SetPosition(newPosition);
        }

        public void UpdateSongInformationDisplay(Song playingSong)
        {
            AlbumArt = playingSong.AlbumArt;
            PlayingSongTitle = playingSong.Artist + "-" + playingSong.Title;
            PlayerView.Title = PlayingSongTitle;
        }

        public void Dispose()
        {
            if (audioPlayer == null)
            {
                return;
            }

            if (audioPlayer.OutputDevice != null)
            {
                audioPlayer.OutputDevice?.Dispose();
                audioPlayer.OutputDevice = null;
            }

            if (audioPlayer.AudioFile != null)
            {
                audioPlayer.AudioFile?.Dispose();
                audioPlayer.AudioFile = null;
            }

            audioPlayer = null;
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
