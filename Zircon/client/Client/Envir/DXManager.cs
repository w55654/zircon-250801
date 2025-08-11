// DXManager.cs — raylib-cs 7.0.1 版
// 目标：保留原 DXManager 的类与函数名/签名，用 raylib-cs 实现；
// 不可等价的行为留空并标注“注意”。

using Client.Controls;
using Library;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using UtilsShared;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

namespace SlimDX.Direct3D9
{
    // 包一层，让旧签名还吃 SlimDX.Texture；内部是真 Texture2D
    public class Texture : HDisposable
    {
        internal Texture2D RL;
        public int Width => RL.Width;
        public int Height => RL.Height;

        public Texture()
        { }

        public Texture(int w, int h, Color color = default)
        {
            // 创建一个 100x100 的纯白 Image
            Image image = Raylib.GenImageColor(w, h, color);

            // 画红色边框（上下左右各一条线）
            Raylib.ImageDrawRectangleLines(ref image, new Rectangle(0, 0, w, h), 1, Color.Red);

            // 将 Image 转换为 GPU 上的 Texture2D
            RL = Raylib.LoadTextureFromImage(image);

            //if (!IsValid())
            //{
            //    Logger.Print($"创建纹理文件失败!!!!!!!!!!", LogLevel.Error);
            //}
            Raylib.UnloadImage(image);  // 释放 image 占用的 CPU 内存
        }

        protected override void OnDisposeManaged()
        {
            if (RL.Id != 0)
            {
                Raylib.UnloadTexture(RL);
                RL = default;
            }
        }
    }
}

// ====================== DXManager：对外 API 保持不变 ======================
namespace Client.Envir
{
    public static class DXManager
    {
        public static Graphics Graphics { get; private set; }

        public static List<Size> ValidResolutions = new List<Size>();
        private static Size MinimumResolution = new Size(1024, 768);

        public static float Opacity { get; private set; } = 1f;
        public static bool Blending { get; private set; }
        public static float BlendRate { get; private set; } = 1f;
        public static BlendMode BlendMode { get; private set; } = BlendMode.NORMAL;

        public static bool DeviceLost { get; private set; } // raylib 不会丢设备

        public static List<DXControl> ControlList { get; } = new List<DXControl>();
        public static List<MirImage> TextureList { get; } = new List<MirImage>();
        public static List<DXSound> SoundList { get; } = new List<DXSound>();

        public static SlimDX.Direct3D9.Texture ScratchTexture { get; private set; }

        public static byte[] PalleteData { get; private set; }
        public static SlimDX.Direct3D9.Texture PoisonTexture { get; private set; }

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

            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(size.Width, size.Height, "Game");
            Raylib.SetTargetFPS(60);
            Raylib.InitAudioDevice();

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
            Raylib.CloseAudioDevice();
            if (_windowInited) Raylib.CloseWindow();
            _windowInited = false;
        }

        // ============== 一帧流程 ==============
        public static void BeginFrame(System.Drawing.Color clear)
        {
            BeginTextureIfNeeded(); // 在 RenderTexture 内绘制
            Raylib.ClearBackground(clear.ToRay());
        }

        public static void PresentToScreen()
        {
            EndTextureIfNeeded();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib_cs.Color.Gray);

            // 注意：RenderTexture 在 raylib 需要 Y 翻转：源矩形高度用负数
            Raylib_cs.Rectangle src = new Raylib_cs.Rectangle(0, 0, _mainTarget.Texture.Width, -_mainTarget.Texture.Height);
            Raylib_cs.Rectangle dst = new Raylib_cs.Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Raylib.DrawTexturePro(_mainTarget.Texture, src, dst, Vector2.Zero, 0, Raylib_cs.Color.White);

            Raylib.EndDrawing();
        }

        // ============== Sprite 兼容层 ==============
        public static void SpriteBegin()
        {
            BeginTextureIfNeeded(); // raylib 默认批处理，这里只保证在 RTT 中
        }

        public static void SpriteEnd()
        { /* 留空，提交由 PresentToScreen 统一处理 */ }

        public static void SpriteFlush()
        { /* 不需要 */ }

        // ============== 三个 SpriteDraw 重载：DrawTexturePro 实现 ==============
        public static void SpriteDraw(SlimDX.Direct3D9.Texture texture, Vector2? center, Vector2? position, System.Drawing.Color color)
        {
            if (texture == null || texture.RL.Id == 0) return;
            var pos = position.HasValue ? position.Value : Vector2.Zero;
            var org = center.HasValue ? center.Value : Vector2.Zero;

            var src = new Rectangle(0, 0, texture.Width, texture.Height);
            var dst = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);

            DrawOne(texture.RL, src, dst, org, 0f, color);
        }

        public static void SpriteDraw(SlimDX.Direct3D9.Texture texture, System.Drawing.Rectangle? sourceRect, Vector2? center, Vector2? position, System.Drawing.Color color)
        {
            if (texture == null || texture.RL.Id == 0) return;
            var pos = position.HasValue ? (Vector2)position.Value : Vector2.Zero;
            var org = center.HasValue ? (Vector2)center.Value : Vector2.Zero;

            Rectangle src = sourceRect.HasValue
                ? new Rectangle(sourceRect.Value.X, sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height)
                : new Rectangle(0, 0, texture.Width, texture.Height);

            Rectangle dst = new Rectangle((int)pos.X, (int)pos.Y, src.Width, src.Height);

            DrawOne(texture.RL, src, dst, org, 0f, color);
        }

        public static void SpriteDraw(SlimDX.Direct3D9.Texture texture, System.Drawing.Color color)
        {
            if (texture == null || texture.RL.Id == 0) return;
            var src = new Rectangle(0, 0, texture.Width, texture.Height);
            var dst = new Rectangle(0, 0, texture.Width, texture.Height);
            DrawOne(texture.RL, src, dst, Vector2.Zero, 0f, color);
        }

        // ============== 状态：Opacity / Blend / Colour ==============
        public static void SetOpacity(float opacity)
        {
            Opacity = Math.Max(0, Math.Min(1, opacity));
        }

        public static void SetBlend(bool value, float rate = 1F, BlendMode mode = BlendMode.NORMAL)
        {
            Blending = value;
            BlendRate = rate;
            BlendMode = mode;
            // 具体混合在 DrawOne 里映射。INV/遮罩类请改 shader。
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
        public static SlimDX.Direct3D9.Texture CreateTextureFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            Image img = Raylib.LoadImage(path);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return new SlimDX.Direct3D9.Texture { RL = tex };
        }

        public static SlimDX.Direct3D9.Texture CreateTextureFromImage(byte[] imageBytes)
        {
            Raylib_cs.Image img = Raylib.LoadImageFromMemory(".png", imageBytes); // 7.0.1: ReadOnlySpan<byte> 重载
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return new SlimDX.Direct3D9.Texture { RL = tex };
        }

        // ============== 内部绘制实现 ==============
        private static void DrawOne(Texture2D tex, Rectangle src, Rectangle dst, Vector2 origin, float rotationDeg, System.Drawing.Color c)
        {
            BeginTextureIfNeeded();

            bool useBlend = Blending;
            if (useBlend) Raylib.BeginBlendMode(MapBlendMode(BlendMode));

            var tint = c.ToRay();
            tint.A = (byte)(tint.A * Opacity); // 透明度叠乘

            Raylib.DrawTexturePro(tex, src, dst, origin, rotationDeg, tint);

            if (useBlend) Raylib.EndBlendMode();
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

        private static SlimDX.Direct3D9.Texture CreatePoisonTexture()
        {
            Image img = Raylib.GenImageColor(6, 6, Color.White);
            // 7.0.1：ImageDrawRectangleLines(ref Image, Rectangle, int, Color)
            Raylib.ImageDrawRectangleLines(ref img, new Rectangle(0, 0, 6, 6), 1, Color.Green);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return new SlimDX.Direct3D9.Texture { RL = tex };
        }

        private static Raylib_cs.BlendMode MapBlendMode(BlendMode mode)
        {
            // 7.0.1 可用：BLEND_ALPHA / BLEND_ADDITIVE / BLEND_MULTIPLIED / BLEND_ADD_COLORS / BLEND_SUBTRACT_COLORS / BLEND_CUSTOM
            return mode switch
            {
                BlendMode.NORMAL => Raylib_cs.BlendMode.Alpha,
                BlendMode.LIGHT => Raylib_cs.BlendMode.Additive,
                BlendMode.HIGHLIGHT => Raylib_cs.BlendMode.Additive,
                BlendMode.COLORFY => Raylib_cs.BlendMode.AddColors,
                BlendMode.MASK => Raylib_cs.BlendMode.Alpha,
                _ => Raylib_cs.BlendMode.Alpha
            };
        }
    }

    // 保留原枚举
    public enum BlendMode : sbyte
    {
        NONE = -1,
        NORMAL = 0,
        LIGHT = 1,
        LIGHTINV = 2,
        INVNORMAL = 3,
        INVLIGHT = 4,
        INVLIGHTINV = 5,
        INVCOLOR = 6,
        INVBACKGROUND = 7,
        COLORFY = 8,
        MASK = 9,
        HIGHLIGHT = 10,
        EFFECTMASK = 11
    }

    internal static class ColorExt
    {
        public static Raylib_cs.Color ToRay(this System.Drawing.Color c) => new Raylib_cs.Color(c.R, c.G, c.B, c.A);
    }
}