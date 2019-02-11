using NAudio.Wave;
using System.Collections.Generic;

namespace HearMe.Models
{
    public interface IPlayer
    {
        float Volume { get; set; }
        double SongPosition { get; set; }
        string CurrentTime { get; }
        string TotalTime { get; }
        long Length { get; }
        WaveFormat SongFormat { get; }

        void Play();
        void Stop();
        void SetVolume(float volumeLevel);
        void SetPosition(long newPosition);
    }
}
