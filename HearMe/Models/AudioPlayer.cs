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

        public AudioPlayer(string audioFile)
        {
            AudioFile = new DmoMp3Decoder(audioFile);
            OutputDevice = GetSoundOut();
            OutputDevice.Initialize(AudioFile);
        }

        public void Play()
        {
            OutputDevice.Play();
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
