using HearMe.Models;
using NAudio.Wave;
using System;
using System.IO;

namespace HearMe.Controllers
{
    class PlayerController : IPlayer, IDisposable
    {
        MainWindow PlayerView;

        private IWavePlayer outputDevice;
        private AudioFileReader audioFile;

        public float Volume { get; set; }
        public long SongPosition {
            get { return audioFile.Position; }
            set { }
        }

        public TimeSpan CurrentTime
        {
            get { return audioFile.CurrentTime; }
        }

        public TimeSpan TotalTime
        {
            get { return audioFile.TotalTime; }
        }

        public long Length
        {
            get { return audioFile.Length; }
        }

        public WaveFormat SongFormat
        {
            get { return audioFile.WaveFormat; }
        }

        public PlayerController(MainWindow view)
        {
            PlayerView = view;
        }

        public void PlayFile(string fileLocation)
        {

            if (!File.Exists(fileLocation))
            {
                return;
            }

            if (outputDevice != null || audioFile != null)
            {
                Dispose();
            }

            outputDevice = new WaveOut { DesiredLatency = 200 };
            audioFile = new AudioFileReader(@fileLocation);

            outputDevice.Init(audioFile);
            outputDevice.Play();

            outputDevice.PlaybackStopped += PlaybackDevicePlaybackStopped;
        }

        void PlaybackDevicePlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (SongPosition == Length)
            {
                PlayerView.Next(null, null);
            }
        }

        public void Play()
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

        public void Stop()
        {
            outputDevice.Stop();
        }

        public void SetVolume(float volumeLevel)
        {
            audioFile.Volume = volumeLevel;
        }

        public void SetPosition(TimeSpan newPosition)
        {
            audioFile.CurrentTime = newPosition;
        }

        public void Dispose()
        {
            outputDevice?.Dispose();
            outputDevice = null;

            audioFile?.Dispose();
            audioFile = null;

            SongPosition = 0;
        }
    }
}
