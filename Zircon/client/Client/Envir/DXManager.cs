// DXManager.cs — raylib-cs 7.0.1 版
// 目标：保留原 DXManager 的类与函数名/签名，用 raylib-cs 实现；
// 不可等价的行为留空并标注“注意”。

using Client.Controls;
using Library;
using Microsoft.Extensions.Logging;
using Ray2D;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using UtilsShared;
using static System.Net.Mime.MediaTypeNames;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

// ====================== DXManager：对外 API 保持不变 ======================
namespace Client.Envir
{
    public static class DXManager
    {
        public static Graphics Graphics { get; private set; }

        public static List<Size> ValidResolutions = new List<Size>();
        private static Size MinimumResolution = new Size(1024, 768);

        public static List<DXControl> ControlList { get; } = new List<DXControl>();
        public static List<MirImage> TextureList { get; } = new List<MirImage>();
        public static List<DXSound> SoundList { get; } = new List<DXSound>();

        public static RayTexture ScratchTexture { get; private set; }

        public static byte[] PalleteData { get; private set; }
        public static RayTexture PoisonTexture { get; private set; }

        // ============== raylib 内部资源 ==============
        private static bool _windowInited;

        // ============== 初始化/销毁 ==============
        public static void Create()
        {
            if (_windowInited) return;

            var size = DXControl.ActiveScene?.Size ?? new Size(1024, 768);

            _windowInited = true;

            // 分辨率列表（简单够用）
            ValidResolutions.Clear();
            int mon = Raylib.GetCurrentMonitor();
            int mw = Raylib.GetMonitorWidth(mon);
            int mh = Raylib.GetMonitorHeight(mon);
            for (int w = MinimumResolution.Width; w <= mw; w *= 2)
            {
                int h = (int)((double)w * size.Height / size.Width);
                if (h >= MinimumResolution.Height && h <= mh)
                    ValidResolutions.Add(new Size(w, h));
            }

            PoisonTexture = CreatePoisonTexture(); // 6x6 黑框白心，占位纹理

            // 注意：你老的 Palette/调色阶段等固定管线功能，7.0.1 下请改 shader；这里不再用。
        }

        public static void Unload()
        {
            _windowInited = false;
        }

        public static void ToggleFullScreen() => Raylib.ToggleFullscreen();

        public static void SetResolution(Size size)
        {
            if (!_windowInited) return;

            Raylib.SetWindowSize(size.Width, size.Height);
        }

        public static void ConfigureGraphics(Graphics graphics)
        { Graphics = graphics; }

        public static void MemoryClear()
        { /* 由你的资源系统统一回收 */ }

        // ============== 纹理加载（便于旧路径过渡，7.0.1 友好写法） ==============
        public static RayTexture CreateTextureFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            Image img = Raylib.LoadImage(path);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return new RayTexture(tex);
        }

        public static RayTexture CreateTextureFromImage(byte[] imageBytes)
        {
            Raylib_cs.Image img = Raylib.LoadImageFromMemory(".png", imageBytes); // 7.0.1: ReadOnlySpan<byte> 重载
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return new RayTexture(tex);
        }

        private static RayTexture CreatePoisonTexture()
        {
            Image img = Raylib.GenImageColor(6, 6, Color.White);
            // 7.0.1：ImageDrawRectangleLines(ref Image, Rectangle, int, Color)
            Raylib.ImageDrawRectangleLines(ref img, new Rectangle(0, 0, 6, 6), 1, Color.Green);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return new RayTexture(tex);
        }
    }
}