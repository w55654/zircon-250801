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

        public static float Opacity { get; private set; } = 1f;

        public static List<DXControl> ControlList { get; } = new List<DXControl>();
        public static List<MirImage> TextureList { get; } = new List<MirImage>();
        public static List<DXSound> SoundList { get; } = new List<DXSound>();

        public static RayTexture ScratchTexture { get; private set; }

        public static byte[] PalleteData { get; private set; }
        public static RayTexture PoisonTexture { get; private set; }

        // ============== raylib 内部资源 ==============
        private static bool _windowInited;

        private static RenderTexture2D _mainTarget;    // 等价 MainSurface
        private static RenderTexture2D _scratchTarget; // 等价 Scratch
        private static RenderTexture2D _currentTarget; // 等价 CurrentSurface
        private static bool _textureModeBegun;

        // ============== 初始化/销毁 ==============
        public static void Create()
        {
            if (_windowInited) return;

            var size = DXControl.ActiveScene?.Size ?? new Size(1024, 768);

            _mainTarget = Raylib.LoadRenderTexture(size.Width, size.Height);
            _scratchTarget = Raylib.LoadRenderTexture(size.Width, size.Height);
            _currentTarget = _mainTarget;

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
            if (_mainTarget.Id != 0) Raylib.UnloadRenderTexture(_mainTarget);
            if (_scratchTarget.Id != 0) Raylib.UnloadRenderTexture(_scratchTarget);

            _windowInited = false;
        }

        // ============== 三个 SpriteDraw 重载：DrawTexturePro 实现 ==============
        public static void SpriteDraw(RayTexture texture, Vector2? center, Vector2? position, System.Drawing.Color color)
        {
            if (texture == null || texture.Texture.Id == 0)
                return;

            var pos = position.HasValue ? position.Value : Vector2.Zero;
            var org = center.HasValue ? center.Value : Vector2.Zero;

            var src = new Rectangle(0, 0, texture.Texture.Width, texture.Texture.Height);
            var dst = new Rectangle((int)pos.X, (int)pos.Y, texture.Texture.Width, texture.Texture.Height);

            Raylib.DrawTexturePro(texture.Texture, src, dst, org, 0f, color.ToRayColor(Opacity));
        }

        public static void SpriteDraw(RayTexture texture, System.Drawing.Rectangle? sourceRect, Vector2? center, Vector2? position, System.Drawing.Color color)
        {
            if (texture == null || texture.Texture.Id == 0)
                return;

            var pos = position.HasValue ? (Vector2)position.Value : Vector2.Zero;
            var org = center.HasValue ? (Vector2)center.Value : Vector2.Zero;

            Rectangle src = sourceRect.HasValue
                ? new Rectangle(sourceRect.Value.X, sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height)
                : new Rectangle(0, 0, texture.Texture.Width, texture.Texture.Height);

            Rectangle dst = new Rectangle((int)pos.X, (int)pos.Y, src.Width, src.Height);

            Raylib.DrawTexturePro(texture.Texture, src, dst, org, 0f, color.ToRayColor(Opacity));
        }

        public static void SpriteDraw(RayTexture texture, System.Drawing.Color color)
        {
            if (texture == null || texture.Texture.Id == 0)
                return;
            var src = new Rectangle(0, 0, texture.Texture.Width, texture.Texture.Height);
            var dst = new Rectangle(0, 0, texture.Texture.Width, texture.Texture.Height);

            Raylib.DrawTexturePro(texture.Texture, src, dst, Vector2.Zero, 0f, color.ToRayColor(Opacity));
        }

        // ============== 状态：Opacity / Blend / Colour ==============
        public static void SetOpacity(float opacity)
        {
            Opacity = Math.Max(0, Math.Min(1, opacity));
        }

        public static void SetColour(int colour)
        {
            // 旧的 TextureStage 调色接口保留但不做事。请使用 shader。
        }

        // ============== Surface/设备/重置（保留接口，最小实现） ==============
        public static void SetSurface()
        {
            _currentTarget = _mainTarget; // 注意：如需多 RTT，请扩展映射表
        }

        public static void ToggleFullScreen() => Raylib.ToggleFullscreen();

        public static void SetResolution(Size size)
        {
            if (!_windowInited) return;

            Raylib.SetWindowSize(size.Width, size.Height);

            if (_mainTarget.Id != 0) Raylib.UnloadRenderTexture(_mainTarget);
            if (_scratchTarget.Id != 0) Raylib.UnloadRenderTexture(_scratchTarget);

            _mainTarget = Raylib.LoadRenderTexture(size.Width, size.Height);
            _scratchTarget = Raylib.LoadRenderTexture(size.Width, size.Height);
            _currentTarget = _mainTarget;
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

        // ============== 内部绘制实现 ==============
        private static void DrawOne(Texture2D tex, Rectangle src, Rectangle dst, Vector2 origin, float rotationDeg, System.Drawing.Color c)
        {
            Raylib.DrawTexturePro(tex, src, dst, origin, rotationDeg, c.ToRayColor(Opacity));
        }

        private static void BeginTextureIfNeeded()
        {
            if (!_textureModeBegun)
            {
                Raylib.BeginTextureMode(_currentTarget);
                _textureModeBegun = true;
            }
        }

        private static void EndTextureIfNeeded()
        {
            if (_textureModeBegun)
            {
                Raylib.EndTextureMode();
                _textureModeBegun = false;
            }
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