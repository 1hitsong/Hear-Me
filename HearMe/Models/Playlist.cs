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
    class Playlist : INotifyPropertyChanged
    {

        private ObservableCollection<M3uPlaylistEntry> _files;
        public ObservableCollection<M3uPlaylistEntry> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }

        public Playlist()
        {
            Files = new ObservableCollection<M3uPlaylistEntry>();
        }

        public void Add(Song songToAdd)
        {
            Files.Add(new M3uPlaylistEntry()
            {
                Album = songToAdd.Album,
                AlbumArtist = songToAdd.Artist,
                Duration = songToAdd.Length,
                Path = songToAdd.FileName,
                Title = songToAdd.Title
            });
        }

        public void Add(string[] filesToAdd)
        {
            foreach (string file in filesToAdd)
            {
                FileInfo pathInfo = new FileInfo(file);

                if (pathInfo.Attributes.ToString() == "Directory")
                {
                    Add(Directory.GetFiles(file, "*.mp3", SearchOption.AllDirectories));
                }

                if (pathInfo.Extension != ".mp3")
                {
                    continue;
                }

                Add(new Song(file));
            }
        }

        public void Remove(System.Collections.IList selectedSongs)
        {
            for (int i = selectedSongs.Count - 1; i >= 0; i--)
            {
                Files.RemoveAt(Files.IndexOf((M3uPlaylistEntry)selectedSongs[i]));
            }
        }

        public void Clear()
        {
            Files.Clear();
        }

        public void Save()
        {
            M3uPlaylist PlaylistFile = new M3uPlaylist
            {
                IsExtended = true
            };

            PlaylistFile.PlaylistEntries = Files.ToList();

            M3uContent content = new M3uContent();

            string toSave = content.ToText(PlaylistFile);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Playlist files (*.m3u)|*.m3u|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save Playlist";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
                {
                    // writing data in string
                    byte[] info = new UTF8Encoding(true).GetBytes(toSave);
                    fs.Write(info, 0, info.Length);

                    // writing data in bytes already
                    byte[] data = new byte[] { 0x0 };
                    fs.Write(data, 0, data.Length);
                }
            }
        }

        public void Open()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Playlist files (*.m3u)|*.m3u|All files (*.*)|*.*";
            openFileDialog1.Title = "Open Playlist";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != "")
            {
                Clear();

                M3uPlaylist playlist = new M3uPlaylist
                {
                    IsExtended = true
                };

                M3uContent content = new M3uContent();

                using (System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile())
                {
                    playlist = content.GetFromStream(fs);

                    // Remove final null char at end of last entry
                    playlist.PlaylistEntries.Last().Path = playlist.PlaylistEntries.Last().Path.Substring(0, playlist.PlaylistEntries.Last().Path.Length - 1);

                    Files = new ObservableCollection<M3uPlaylistEntry>(playlist.PlaylistEntries);
                }

                playlist = null;
            }
        }

        public M3uPlaylistEntry ElementAt(int index)
        {
            return index < Files.Count() ? Files.ElementAt(index) : null;
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
