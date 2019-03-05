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
            {
                return;
            }

            using (TagFile tags = TagFile.Create(@file))
            {
                FileName = file;
                Title = tags == null ? "Unknown Track" : tags.Tag.Title.Replace("\0", "");
                Album = tags == null ? "Unknown Album" : tags.Tag.Album.Replace("\0", "");
                Artist = tags == null ? "Unknown Artist" : tags.Tag.Performers.FirstOrDefault().Replace("\0", "");
                Length = TimeSpan.FromSeconds(60);
            }
        }

        public static BitmapImage GetAlbumArt(string fileName)
        {
            BitmapImage bitmap = new BitmapImage();

            using (TagFile tags = TagFile.Create(fileName))
            {
                if (tags.Tag.Pictures.Any())
                {
                    using (MemoryStream ms = new MemoryStream(tags.Tag.Pictures.FirstOrDefault().Data.Data))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }
                else
                {
                    bitmap = GetFolderAlbumImage(fileName);
                }
            }

            return bitmap;
        }

        public static BitmapImage GetFolderAlbumImage(string fileName)
        {
            BitmapImage tmp = new BitmapImage();

            string[] validImageTypes = { ".jpg", ".jpeg", ".gif", ".png" };
            string[] validFileNames = { "album", "default", "index", "cover", "folder" };

            List<string> imageFiles = Directory.GetFiles(Path.GetDirectoryName(fileName), "*.*", SearchOption.TopDirectoryOnly)
                  .Where(file => validImageTypes.Contains(Path.GetExtension(file)) && validFileNames.Contains(Path.GetFileNameWithoutExtension(file)))
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
