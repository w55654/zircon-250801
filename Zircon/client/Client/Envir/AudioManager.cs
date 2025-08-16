using Client.Envir;
using Library;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Client.DXDraw
{
    public static class AudioManager
    {
        private static Music? currentMusic;

        private static float effect_value = 0.1f;
        private static float music_value = 0.1f;

        public static void Init()
        {
        }

        public static void Close()
        {
            if (currentMusic.HasValue)
            {
                Raylib.UnloadMusicStream(currentMusic.Value);
                currentMusic = null;
            }
        }

        public static void PlayEffect(SoundIndex index)
        {
            int idx = (int)index;
            PlayEffect(idx);
        }

        public static void PlayEffect(int soundId)
        {
            if (!Config.OpenSound)
                return;

            //ResManager.ReqAsset($"wavs/effect/eff_{soundId}.ogg", AssetType.Sound, info =>
            //{
            //    if (info.Status == AssetStatus.Complete)
            //    {
            //        if (info.ResObject is Sound sound)
            //        {
            //            Raylib.SetSoundVolume(sound, effect_value);
            //            Raylib.PlaySound(sound);
            //        }
            //    }
            //});
        }

        public static void PlayMusic(int soundId)
        {
            if (!Config.OpenSound)
            {
                StopMusic();
                return;
            }

            //// 如果当前音乐已播放，则更换
            //if (currentMusic.HasValue)
            //{
            //    Raylib.StopMusicStream(currentMusic.Value);
            //    Raylib.UnloadMusicStream(currentMusic.Value);

            //    currentMusic = null;
            //}

            //ResManager.ReqAsset($"wavs/music/bk_{soundId}.ogg", AssetType.Music, info =>
            //{
            //    if (info.Status == AssetStatus.Complete)
            //    {
            //        currentMusic = Raylib.LoadMusicStream(info.LocalPath);

            //        Raylib.SetMusicVolume(currentMusic.Value, music_value);
            //        Raylib.PlayMusicStream(currentMusic.Value);
            //    }
            //});
        }

        public static void UpdateVolume()
        {
            //float music_val = UserSetting.MusicVolume / 100F;
            //float effect_val = UserSetting.EffectVolume / 100F;

            //music_value = Math.Clamp(music_val, 0F, 1F);
            //effect_value = Math.Clamp(effect_val, 0F, 1F);

            //if (currentMusic.HasValue)
            //    Raylib.SetMusicVolume(currentMusic.Value, music_value);
        }

        public static void Update()
        {
            if (currentMusic.HasValue)
                Raylib.UpdateMusicStream(currentMusic.Value);
        }

        public static void PauseMusic()
        {
            if (currentMusic.HasValue)
                Raylib.PauseMusicStream(currentMusic.Value);
        }

        public static void ResumeMusic()
        {
            if (currentMusic.HasValue)
                Raylib.ResumeMusicStream(currentMusic.Value);
        }

        public static void StopMusic()
        {
            if (currentMusic.HasValue)
                Raylib.StopMusicStream(currentMusic.Value);
        }
    }
}