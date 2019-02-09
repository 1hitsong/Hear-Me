﻿using HearMe.Models;
using NAudio.Wave;

namespace HearMe.Controllers
{
    class PlayerController : IPlayer
    {
        MainWindow PlayerView;

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        public float Volume { get; set; }
        public double SongPosition {
            get { return audioFile.Position; }
            set { }
        }

        public string CurrentTime
        {
            get { return audioFile.CurrentTime.ToString("mm\\:ss"); }
        }

        public string TotalTime
        {
            get { return audioFile.TotalTime.ToString("mm\\:ss"); }
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

        public void SetPosition(long newPosition)
        {
            audioFile.Position = newPosition;
        }
    }
}
