using NAudio.Wave;
using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        public MainWindow()
        {
            InitializeComponent();

            Play();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            outputDevice.Stop();
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
            }

            outputDevice.Play();
        }
    }
}
