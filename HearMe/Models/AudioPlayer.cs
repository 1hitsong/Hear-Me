using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs.MP3;

namespace HearMe.Models
{
    class AudioPlayer
    {
        public ISoundOut OutputDevice;
        public IWaveSource AudioFile;

        private PlayerState State { get; set; }

        private enum PlayerState
        {
            Paused,
            Playing
        }

        public AudioPlayer(string audioFile)
        {
            AudioFile = new DmoMp3Decoder(audioFile);
            OutputDevice = GetSoundOut();
            OutputDevice.Initialize(AudioFile);
            State = PlayerState.Paused;
        }

        public bool IsPlaying()
        {
            return State == PlayerState.Playing;
        }

        public void Play()
        {
            OutputDevice.Play();
            State = PlayerState.Playing;
        }

        public void Stop()
        {
            OutputDevice.Stop();
            State = PlayerState.Paused;
        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
            {
                return new WasapiOut();
            }

            return new DirectSoundOut();
        }
    }
}
