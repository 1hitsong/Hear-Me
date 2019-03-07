using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

using HearMe.Models;
using HearMe.Helpers;
using CSCore;
using System.Linq;
using PlaylistsNET.Models;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace HearMe.ViewModels
{
    class PlayerViewModel : IDisposable, INotifyPropertyChanged
    {
        AudioPlayer audioPlayer;
        MainWindow PlayerView;

        private string _keyValue;
        public string KeyValue
        {
            get { return _keyValue; }
            set
            {
                _keyValue = value;
                OnPropertyChanged("KeyValue");
            }
        }

        private System.Timers.Timer _timer;

        public DelegateCommand NextCommand { get; private set; }
        public DelegateCommand PreviousCommand { get; private set; }
        public DelegateCommand StopCommand { get; private set; }
        public DelegateCommand PlayCommand { get; private set; }
        public DelegateCommand OpenPlaylistCommand { get; private set; }
        public DelegateCommand SavePlaylistCommand { get; private set; }
        public DelegateCommand ClearPlaylistCommand { get; private set; }

        public RelayCommand<DragEventArgs> DropCommand { get; private set; }
        public RelayCommand<KeyEventArgs> DeleteSelectedCommand { get; private set; }

        public PlayerViewModel(MainWindow view)
        {
            NavigationRequest += HandleNavigationRequest;

            DataUpdateRequest += UpdatePlayingSongPosition;
            DataUpdateRequest += UpdatePlayingSongDisplayText;

            PlayerView = view;
            Playlist = new Playlist();

            SetupKeyboardHooks();

            NextCommand = new DelegateCommand(Next, null);
            PreviousCommand = new DelegateCommand(Previous, null);
            StopCommand = new DelegateCommand(Stop, null);
            PlayCommand = new DelegateCommand(Play, null);
            OpenPlaylistCommand = new DelegateCommand(OpenPlaylist, null);
            SavePlaylistCommand = new DelegateCommand(SavePlaylist, null);
            ClearPlaylistCommand = new DelegateCommand(ClearPlaylist, null);

            DropCommand = new RelayCommand<DragEventArgs>((e) => AddToPlaylist(e), (e) => true);
            DeleteSelectedCommand = new RelayCommand<KeyEventArgs>((e) => DeleteSelectedFile(e), (e) => true);

            _timer = new System.Timers.Timer
            {
                Interval = 300
            };

            _timer.Elapsed += UpdateBoundData;
        }

        private void DeleteSelectedFile(KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                ListBox playlistListBox = (ListBox)e.Source;
                RemoveFromPlaylist(playlistListBox.SelectedItems);
            }
        }

        public void AddToPlaylist(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                AddFilesToPlaylist((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }

        void ClearPlaylist(object arg)
        {
            Playlist.Clear();
            PlayingSongPlaylistIndex = -1;
        }

        void SavePlaylist(object arg)
        {
            Playlist.Save();
        }

        void OpenPlaylist(object arg)
        {
            Playlist.Open();
        }

        void Next(object arg)
        {
            MovePlaylistSong(1);
        }

        void Previous(object arg)
        {
            MovePlaylistSong(-1);
        }

        void Play(object arg)
        {
            Play();
        }

        void Stop(object arg)
        {
            Stop();
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
                SetPosition(value);
            }
        }

        private float _volume;
        public float Volume
        {
            get { return _volume; }
            set
            {
                if(_volume != value)
                {
                    SetVolume(value);
                    _volume = value;
                    OnPropertyChanged("Volume");
                } 
            }
        }

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
            M3uPlaylistEntry selectedSong = Playlist.ElementAt(playlistIndex);

            if (selectedSong == null)
            {
                return;
            }

            string fileLocation = selectedSong.Path;

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

            _timer.Start();
            
        }

        public void UpdateBoundData(object sender, System.Timers.ElapsedEventArgs e)
        {
            DataUpdateRequest.Invoke(this, null);
        }

        public void UpdatePlayingSongPosition(object sender, EventArgs e)
        {
            _playingsongPosition = CurrentTime.TotalSeconds;
            OnPropertyChanged("PlayingSongPosition");
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
            _timer.Start();
        }

        public void Stop()
        {
            if (audioPlayer != null)
            {
                audioPlayer.Stop();
                _timer.Stop();
            }
        }

        public void SetVolume(float volumeLevel)
        {
            if (audioPlayer != null)
                audioPlayer.OutputDevice.Volume = Volume;
        }

        public void SetPosition(double newPosition)
        {
            TimeSpan seekPosition = new TimeSpan(0, (int)(Math.Floor(newPosition / 60)), (int)(Math.Floor(newPosition % 60)));
            audioPlayer.AudioFile.SetPosition(seekPosition);
        }

        public void UpdateSongInformationDisplay(M3uPlaylistEntry playingSong)
        {
            AlbumArt = Song.GetAlbumArt(playingSong.Path);
            PlayingSongTitle = playingSong.AlbumArtist + "-" + playingSong.Title;
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
                //Console.WriteLine(e.KeyboardData.);
                KeyValue = e.KeyboardData.VirtualCode.ToString();

                int[] validKeys = new int[] {GlobalKeyboardHook.VkMediaNext, GlobalKeyboardHook.VkMediaPrevious, GlobalKeyboardHook.VkMediaPlay};

                if (!validKeys.Contains(e.KeyboardData.VirtualCode))
                {
                    return;
                }

                if (e.KeyboardData.VirtualCode == GlobalKeyboardHook.VkMediaNext)
                    MovePlaylistSong(1);

                if (e.KeyboardData.VirtualCode == GlobalKeyboardHook.VkMediaPrevious)
                    MovePlaylistSong(-1);

                if (e.KeyboardData.VirtualCode == GlobalKeyboardHook.VkMediaPlay)
                {
                    if (audioPlayer == null)
                        return;

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
