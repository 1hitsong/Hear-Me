﻿using System.IO;
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
        public string Length { get; set; }
        public BitmapImage AlbumArt { get; set; }

        public Song(string file)
        {
            using (TagFile tags = TagFile.Create(file))
            {
                using(MemoryStream ms = new MemoryStream(tags.Tag.Pictures.FirstOrDefault().Data.Data))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    AlbumArt = bitmap;
                }

                FileName = file;
                Title = tags == null ? "Unknown Track" : tags.Tag.Title.Replace("\0", "");
                Artist = tags == null ? "Unknown Artist" : tags.Tag.Performers.FirstOrDefault().Replace("\0", "");
            }
        }
    }
}
