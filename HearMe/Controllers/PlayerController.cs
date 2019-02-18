using System;
using System.IO;

using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs.MP3;

namespace HearMe.Controllers
{
    class PlayerController : IDisposable
    {
        MainWindow PlayerView;

        private ISoundOut outputDevice;
        private IWaveSource audioFile;

        public float Volume { get; set; }
        public long SongPosition {
            get { return (audioFile != null) ? audioFile.Position : 0; }
            set { }
        }

        public TimeSpan CurrentTime
        {
            get { return (audioFile != null) ? audioFile.GetPosition() : TimeSpan.Zero; }
        }

        public TimeSpan TotalTime
        {
            get { return (audioFile != null) ? audioFile.GetLength() : TimeSpan.Zero; }
        }

        public long Length
        {
            get { return (audioFile != null) ? audioFile.Length : 0; }
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

            //Contains the sound to play
            outputDevice = GetSoundOut();
            audioFile = new DmoMp3Decoder(@fileLocation);

            outputDevice.Initialize(audioFile);
            outputDevice.Play();

            outputDevice.Volume = Volume;

            outputDevice.Stopped += PlaybackDevicePlaybackStopped;
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
            if (outputDevice == null || audioFile == null)
            {
                return;
            }

            outputDevice.Volume = Volume;

            outputDevice.Play();
        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        public void Stop()
        {
            if (outputDevice != null)
                outputDevice.Stop();
        }

        public void SetVolume(float volumeLevel)
        {
            Volume = volumeLevel;

            if (outputDevice != null)
                outputDevice.Volume = Volume;
        }

        public void SetPosition(TimeSpan newPosition)
        {
            audioFile.SetPosition(newPosition);
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
