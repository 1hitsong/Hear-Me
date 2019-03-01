using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

using HearMe.Models;
using CSCore;

namespace HearMe.ViewModels
{
    class PlayerViewModel : IDisposable, INotifyPropertyChanged
    {
        AudioPlayer audioPlayer;
        MainWindow PlayerView;

        public PlayerViewModel(MainWindow view)
        {
            NavigationRequest += HandleNavigationRequest;

            DataUpdateRequest += UpdatePlayingSongPosition;
            DataUpdateRequest += UpdatePlayingSongDisplayText;

            PlayerView = view;
            Playlist = new Playlist();

            SetupKeyboardHooks();
        }

        private int PlayingSongPlaylistIndex { get; set; }

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

        public event EventHandler<NavigationEventArgs> NavigationRequest;
        public event EventHandler<EventArgs> DataUpdateRequest;


        public void AddFilesToPlaylist(string[] filesToAdd)
        {
            Playlist.Add(filesToAdd);
        }

        public void UnbindOnStopEvent()
        {
            if (audioPlayer != null)
                audioPlayer.OutputDevice.Stopped -= PlaybackDevicePlaybackStopped;
        }

        public void RemoveFromPlaylist(System.Collections.IList selectedSongs)
        {
            Playlist.Remove(selectedSongs);
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

            audioPlayer.OutputDevice.Stopped += PlaybackDevicePlaybackStopped;

            PlayingSongLength = TotalTime.TotalSeconds;

            audioPlayer.OutputDevice.Volume = Volume;
            
        }

        public void UpdateBoundData(object sender, System.Timers.ElapsedEventArgs e)
        {
            DataUpdateRequest.Invoke(this, null);
        }

        public void UpdatePlayingSongPosition(object sender, EventArgs e)
        {
            PlayingSongPosition = CurrentTime.TotalSeconds;
        }

        private void UpdatePlayingSongDisplayText(object sender, EventArgs e)
        {
            DisplayText = CurrentTime.ToString("mm\\:ss") + " / " + TotalTime.ToString("mm\\:ss");
        }

        public void HandleNavigationRequest(object sender, NavigationEventArgs e)
        {
            // Don't allow movement beyond last playlist song
            if (PlayingSongPlaylistIndex + e.Direction == Playlist.Files.Count)
            {
                return;
            }

            // Don't allow movement below 0
            if (PlayingSongPlaylistIndex + e.Direction < 0)
            {
                return;
            }

            PlayingSongPlaylistIndex += e.Direction;

            PlayFile(PlayingSongPlaylistIndex);
        }

        public void MovePlaylistSong(sbyte indexMovementDirection)
        {
            NavigationRequest.Invoke(this, new NavigationEventArgs {Direction = indexMovementDirection });
        }

        void PlaybackDevicePlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (audioPlayer.OutputDevice.PlaybackState == CSCore.SoundOut.PlaybackState.Playing)
            {
                return;
            }

            if (audioPlayer.IsPlaying())
            {
                MovePlaylistSong(1);
                return;
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
            {
                audioPlayer.Stop();
            }
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
            AlbumArt = playingSong.GetAlbumArt();
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
                audioPlayer.OutputDevice.Dispose();
                audioPlayer.OutputDevice = null;
            }

            if (audioPlayer.AudioFile != null)
            {
                audioPlayer.AudioFile.Dispose();
                audioPlayer.AudioFile = null;
            }

            audioPlayer = null;
        }

        private GlobalKeyboardHook _globalKeyboardHook;

        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                if (e.KeyboardData.VirtualCode == 46 || e.KeyboardData.VirtualCode == 8)
                    RemoveFromPlaylist(PlayerView.playlist.SelectedItems);

                if (e.KeyboardData.VirtualCode == GlobalKeyboardHook.VkMediaNext)
                    MovePlaylistSong(1);

                if (e.KeyboardData.VirtualCode == GlobalKeyboardHook.VkMediaPrevious)
                    MovePlaylistSong(-1);

                if (e.KeyboardData.VirtualCode == GlobalKeyboardHook.VkMediaPlay)
                {
                    if (audioPlayer.IsPlaying())
                    {
                        Stop();
                    }
                    else
                    {
                        Play();
                    }
                }

                e.Handled = true;
            }
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
