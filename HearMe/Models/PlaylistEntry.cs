using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using PlaylistsNET;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace HearMe.Models
{
    class PlaylistEntry : GalaSoft.MvvmLight.ObservableObject
    {
        private int _playlistIndex;
        public int PlaylistIndex
        {
            get { return _playlistIndex; }
            set
            {
                _playlistIndex = value;
                RaisePropertyChanged("PlaylistIndex");
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }

        public Song SongData { get; set; }


        public PlaylistEntry(Song newSong)
        {
            IsPlaying = false;
            SongData = newSong;
        }
    }
}
