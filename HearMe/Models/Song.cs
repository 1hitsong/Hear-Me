using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TagFile = TagLib.File;

namespace HearMe
{
    public class Song
    {
        public string FileName { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public TimeSpan Length { get; set; }
        public BitmapImage AlbumArt { get; set; }

        public Song(string file)
        {
            if (Path.GetExtension(file) != ".mp3")
            {
                return;
            }

            using (TagFile tags = TagFile.Create(@file))
            {
                if (tags.Tag.Pictures.Any())
                {
                    using (MemoryStream ms = new MemoryStream(tags.Tag.Pictures.FirstOrDefault().Data.Data))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        AlbumArt = bitmap;
                    }
                }

                FileName = file;
                Title = tags == null ? "Unknown Track" : tags.Tag.Title.Replace("\0", "");
                Album = tags == null ? "Unknown Album" : tags.Tag.Album.Replace("\0", "");
                Artist = tags == null ? "Unknown Artist" : tags.Tag.Performers.FirstOrDefault().Replace("\0", "");
                Length = TimeSpan.FromSeconds(60);
            }
        }
    }
}
