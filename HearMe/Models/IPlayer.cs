using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace HearMe.Models
{
    public interface IPlayer
    {
        float Volume { get; set; }
        long SongPosition { get; set; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalTime { get; }
        long Length { get; }
        WaveFormat SongFormat { get; }

        void Play();
        void Stop();
        void SetVolume(float volumeLevel);
        void SetPosition(TimeSpan newPosition);
    }
}
