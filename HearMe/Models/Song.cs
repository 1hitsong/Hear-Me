using System;
using System.Collections.Generic;
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

        public Song(string file)
        {
            if (Path.GetExtension(file) != ".mp3")
                return;

            using (TagFile tags = TagFile.Create(@file))
            {
                FileName = file;
                Title = tags == null ? "Unknown Track" : tags.Tag.Title.Replace("\0", "");
                Album = tags == null ? "Unknown Album" : tags.Tag.Album.Replace("\0", "");
                Artist = tags == null ? "Unknown Artist" : tags.Tag.Performers.FirstOrDefault().Replace("\0", "");
                Length = tags.Properties.Duration;
            }
        }

        private static bool TryGetAlbumArtFromFile(string fileName, out BitmapImage albumArt)
        {
            albumArt = new BitmapImage();

            using (TagFile tags = TagFile.Create(fileName))
            {
                if (tags.Tag.Pictures.Any())
                {
                    using (MemoryStream ms = new MemoryStream(tags.Tag.Pictures.FirstOrDefault().Data.Data))
                    {
                        albumArt.BeginInit();
                        albumArt.StreamSource = ms;
                        albumArt.CacheOption = BitmapCacheOption.OnLoad;
                        albumArt.EndInit();
                        albumArt.Freeze();

                        return true;
                    }
                }
            }

            return false;
        }

        public static BitmapImage GetAlbumArt(string fileName)
        {
            if (!TryGetAlbumArtFromFile(fileName, out BitmapImage bitmap))
            {
                bitmap = GetFolderAlbumImage(fileName);
            }

            return bitmap;
        }

        public static BitmapImage GetFolderAlbumImage(string fileName)
        {
            BitmapImage tmp = new BitmapImage();

            string[] validImageTypes = { ".jpg", ".jpeg", ".gif", ".png" };
            string[] validFileNames = { "album", "default", "index", "cover", "folder" };

            List<string> imageFiles = Directory.GetFiles(Path.GetDirectoryName(fileName), "*.*", SearchOption.TopDirectoryOnly)
                  .Where(file => validImageTypes.Contains(Path.GetExtension(file).ToLower()) && validFileNames.Contains(Path.GetFileNameWithoutExtension(file).ToLower()))
                  .ToList();

            if (imageFiles.Any())
            {
                tmp.BeginInit();
                tmp.UriSource = new Uri(@imageFiles.FirstOrDefault());
                tmp.EndInit();
            }
            
            return tmp;
        }
    }
}
