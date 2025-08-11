using NAudio.Wave;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Client.Envir
{
    public sealed class DXSound
    {
        public string FileName { get; set; }

        public List<Sound> BufferList = new List<Sound>();

        private WaveFormat Format;
        private byte[] RawData;

        public DateTime ExpireTime { get; set; }
        public bool Loop { get; set; }

        public SoundType SoundType { get; set; }

        public int Volume { get; set; }

        public DXSound(string fileName, SoundType type)
        {
            FileName = fileName;
            SoundType = type;

            Volume = DXSoundManager.GetVolume(SoundType);
        }

        public void Play()
        {
        }

        public void Stop()
        {
        }

        public void DisposeSoundBuffer()
        {
        }

        public void SetVolume()
        {
        }

        public void UpdateFlags()
        {
        }
    }
}