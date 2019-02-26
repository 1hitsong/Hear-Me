using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using PlaylistsNET;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace HearMe.Models
{
    class Playlist
    {

        private ObservableCollection<Song> _files;
        public ObservableCollection<Song> Files
        {
            get { return _files; }
            set
            {
                _files = value;
            }
        }

        private M3uPlaylist PlaylistFile { get; set; }

        public Playlist()
        {
            Files = new ObservableCollection<Song>();
            PlaylistFile = new M3uPlaylist {
                IsExtended = true
            };
        }

        public void Add(Song songToAdd)
        {
            Files.Add(songToAdd);
            PlaylistFile.PlaylistEntries.Add(new M3uPlaylistEntry()
            {
                Album = songToAdd.Album,
                AlbumArtist = songToAdd.Artist,
                Duration = songToAdd.Length,
                Path = songToAdd.FileName,
                Title = songToAdd.Title
            });
        }

        public void Clear()
        {
            Files.Clear();
            PlaylistFile.PlaylistEntries.Clear();

            var test = "";
        }

        public void Save()
        {
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
            M3uPlaylist playlist;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Playlist files (*.m3u)|*.m3u|All files (*.*)|*.*";
            openFileDialog1.Title = "Open Playlist";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != "")
            {
                Clear();

                M3uContent content = new M3uContent();
                using (System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile())
                {
                    playlist = content.GetFromStream(fs);
                }

                // Remove final null char at end of last entry
                playlist.PlaylistEntries.Last().Path = playlist.PlaylistEntries.Last().Path.Substring(0, playlist.PlaylistEntries.Last().Path.Length - 1);

                foreach (M3uPlaylistEntry playlistEntry in playlist.PlaylistEntries.ToList())
                {
                    Add(new Song(@playlistEntry.Path));
                }

            }
        }

        public Song ElementAt(int index)
        {
            return index < Files.Count() ? Files.ElementAt(index) : null;
        }
    }
}
