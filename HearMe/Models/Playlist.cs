using System.Collections.ObjectModel;
using System.Linq;

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

        public Playlist()
        {
            Files = new ObservableCollection<Song>();
        }

        public void Add(Song songToAdd)
        {
            Files.Add(songToAdd);
        }

        public Song ElementAt(int index)
        {
            return index < Files.Count() ? Files.ElementAt(index) : null;
        }
}
}
