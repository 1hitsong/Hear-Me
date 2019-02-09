using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HearMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private string _songTimeText;

        public string songTimeText {
            get { return _songTimeText; }
            set {
                _songTimeText = value;
                OnPropertyChanged("songTimeText");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            songTime.DataContext = this;
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            outputDevice.Stop();
            songTimeText = "";
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Play()
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
            }

            if (audioFile == null)
            {
                audioFile = new AudioFileReader(@"sample.mp3");
                outputDevice.Init(audioFile);

                seekBar.Maximum = audioFile.Length;
            }

            outputDevice.Play();

            var timer = new System.Timers.Timer();
            timer.Interval = 300;
            timer.Elapsed += UpdateSeekPosition;
            timer.Start();
        }

        private void UpdateSeekPosition(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                seekBar.Value = audioFile.Position - (audioFile.WaveFormat.AverageBytesPerSecond * 2.5);
                UpdateSongInformation();
            });
        }

        private void UpdateSongInformation()
        {
            songTimeText = audioFile.CurrentTime.ToString("mm\\:ss") + " / " + audioFile.TotalTime.ToString("mm\\:ss");
        }

        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioFile.Volume = (float)volumeBar.Value / (float)volumeBar.Maximum;
        }

        private void GoToPosition(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            long newPos = (long)seekBar.Value + (long)(audioFile.WaveFormat.AverageBytesPerSecond * 2.5);
            // Force it to align to a block boundary
            if ((newPos % audioFile.WaveFormat.BlockAlign) != 0)
                newPos -= newPos % audioFile.WaveFormat.BlockAlign;
            // Force new position into valid range
            newPos = Math.Max(0, Math.Min(audioFile.Length, newPos));
            // set position
            audioFile.Position = newPos;

            UpdateSongInformation();

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
