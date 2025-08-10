// DXManager.cs — raylib-cs 7.0.1 版
// 目标：保留原 DXManager 的类与函数名/签名，用 raylib-cs 实现；
// 不可等价的行为留空并标注“注意”。

using Client.Controls;
using Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using Raylib_cs;
using static Raylib_cs.Raylib;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;
using Image = Raylib_cs.Image;

// ====================== SlimDX 名字壳（签名占位，不做真实 D3D9） ======================
namespace SlimDX
{
    public struct Matrix
    {
        public static Matrix Identity => new Matrix();
    }

    public struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z = 0)
        { X = x; Y = y; Z = z; }

        public static implicit operator Vector2(Vector3 v) => new Vector2(v.X, v.Y);
    }

    public struct Color4
    {
        public float Alpha, Red, Green, Blue;

        public Color4(float a, float r, float g, float b)
        { Alpha = a; Red = r; Green = g; Blue = b; }

        public static implicit operator Raylib_cs.Color(Color4 c)
            => new Raylib_cs.Color((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));

        public static implicit operator System.Drawing.Color(Color4 c)
            => System.Drawing.Color.FromArgb((int)(c.Alpha * 255), (int)(c.Red * 255), (int)(c.Green * 255), (int)(c.Blue * 255));
    }
}

namespace SlimDX.Direct3D9
{
    [Flags] public enum SpriteFlags { None = 0 }

    public class PresentParameters
    { }

    public class Device
    {
        public void Clear(int flags, System.Drawing.Color color, float z, int stencil)
        { }

        public void Present()
        { }
    }

    public class Surface
    { } // 概念占位（映射为 RenderTexture）

    // 包一层，让旧签名还吃 SlimDX.Texture；内部是真 Texture2D
    public class Texture
    {
        internal Texture2D RL;
        public int Width => RL.Width;
        public int Height => RL.Height;
    }

    public class Sprite
    { public SlimDX.Matrix Transform { get; set; } }

    public class Line
    { }
}

// ====================== DXManager：对外 API 保持不变 ======================
namespace Client.Envir
{
    public static class DXManager
    {
        public static Graphics Graphics { get; private set; }

        public static List<Size> ValidResolutions = new List<Size>();
        private static Size MinimumResolution = new Size(1024, 768);

        public static SlimDX.Direct3D9.PresentParameters Parameters { get; private set; }
        public static SlimDX.Direct3D9.Device Device { get; private set; } = new SlimDX.Direct3D9.Device();

        public static SlimDX.Direct3D9.Sprite Sprite { get; private set; } = new SlimDX.Direct3D9.Sprite();

        public static SlimDX.Matrix SpriteTransform
        {
            get => Sprite?.Transform ?? SlimDX.Matrix.Identity;
            set { if (Sprite != null) Sprite.Transform = value; }
        }

        public static SlimDX.Direct3D9.Line Line { get; private set; } = new SlimDX.Direct3D9.Line();

        public static SlimDX.Direct3D9.Surface CurrentSurface { get; private set; }
        public static SlimDX.Direct3D9.Surface MainSurface { get; private set; }

        public static float Opacity { get; private set; } = 1f;
        public static bool Blending { get; private set; }
        public static float BlendRate { get; private set; } = 1f;
        public static BlendMode BlendMode { get; private set; } = BlendMode.NORMAL;

        public static bool DeviceLost { get; private set; } // raylib 不会丢设备

        public static List<DXControl> ControlList { get; } = new List<DXControl>();
        public static List<MirImage> TextureList { get; } = new List<MirImage>();
        public static List<DXSound> SoundList { get; } = new List<DXSound>();

        public static SlimDX.Direct3D9.Texture ScratchTexture { get; private set; }
        public static SlimDX.Direct3D9.Surface ScratchSurface { get; private set; }

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

            SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow);
            InitWindow(size.Width, size.Height, "Game");
            SetTargetFPS(Config.VSync ? 60 : 0);
            InitAudioDevice();

            _mainTarget = LoadRenderTexture(size.Width, size.Height);
            _scratchTarget = LoadRenderTexture(size.Width, size.Height);
            _currentTarget = _mainTarget;

            _windowInited = true;

            // 分辨率列表（简单够用）
            ValidResolutions.Clear();
            int mon = GetCurrentMonitor();
            int mw = GetMonitorWidth(mon);
            int mh = GetMonitorHeight(mon);
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
            if (_mainTarget.Id != 0) UnloadRenderTexture(_mainTarget);
            if (_scratchTarget.Id != 0) UnloadRenderTexture(_scratchTarget);
            CloseAudioDevice();
            if (_windowInited) CloseWindow();
            _windowInited = false;
        }

        // ============== 一帧流程 ==============
        public static void BeginFrame(System.Drawing.Color clear)
        {
            BeginTextureIfNeeded(); // 在 RenderTexture 内绘制
            ClearBackground(clear.ToRay());
        }

        public static void PresentToScreen()
        {
            EndTextureIfNeeded();

            BeginDrawing();
            ClearBackground(Color.Gray);

            // 注意：RenderTexture 在 raylib 需要 Y 翻转：源矩形高度用负数
            Rectangle src = new Rectangle(0, 0, _mainTarget.Texture.Width, -_mainTarget.Texture.Height);
            Rectangle dst = new Rectangle(0, 0, GetScreenWidth(), GetScreenHeight());
            DrawTexturePro(_mainTarget.Texture, src, dst, Vector2.Zero, 0, Color.White);

            EndDrawing();
        }

        // ============== Sprite 兼容层 ==============
        public static void SpriteBegin(SlimDX.Direct3D9.SpriteFlags flags)
        {
            BeginTextureIfNeeded(); // raylib 默认批处理，这里只保证在 RTT 中
        }

        public static void SpriteEnd()
        { /* 留空，提交由 PresentToScreen 统一处理 */ }

        public static void SpriteFlush()
        { /* 不需要 */ }

        // ============== 三个 SpriteDraw 重载：DrawTexturePro 实现 ==============
        public static void SpriteDraw(SlimDX.Direct3D9.Texture texture, SlimDX.Vector3? center, SlimDX.Vector3? position, SlimDX.Color4 color)
        {
            if (texture == null || texture.RL.Id == 0) return;
            var pos = position.HasValue ? (Vector2)position.Value : Vector2.Zero;
            var org = center.HasValue ? (Vector2)center.Value : Vector2.Zero;

            var src = new Rectangle(0, 0, texture.Width, texture.Height);
            var dst = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);

            DrawOne(texture.RL, src, dst, org, 0f, color);
        }

        public static void SpriteDraw(SlimDX.Direct3D9.Texture texture, System.Drawing.Rectangle? sourceRect, SlimDX.Vector3? center, SlimDX.Vector3? position, SlimDX.Color4 color)
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

        public static void SpriteDraw(SlimDX.Direct3D9.Texture texture, SlimDX.Color4 color)
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
        public static void SetSurface(SlimDX.Direct3D9.Surface surface)
        {
            CurrentSurface = surface ?? MainSurface;
            _currentTarget = _mainTarget; // 注意：如需多 RTT，请扩展映射表
        }

        public static void ResetDevice()
        { }

        public static void AttemptReset()
        { }

        public static void AttemptRecovery()
        { }

        public static void ToggleFullScreen() => Raylib.ToggleFullscreen();

        public static void SetResolution(Size size)
        {
            if (!_windowInited) return;

            SetWindowSize(size.Width, size.Height);

            if (_mainTarget.Id != 0) UnloadRenderTexture(_mainTarget);
            if (_scratchTarget.Id != 0) UnloadRenderTexture(_scratchTarget);

            _mainTarget = LoadRenderTexture(size.Width, size.Height);
            _scratchTarget = LoadRenderTexture(size.Width, size.Height);
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
            Image img = LoadImage(path);
            Texture2D tex = LoadTextureFromImage(img);
            UnloadImage(img);
            return new SlimDX.Direct3D9.Texture { RL = tex };
        }

        public static SlimDX.Direct3D9.Texture CreateTextureFromImage(byte[] imageBytes)
        {
            Image img = LoadImageFromMemory(".png", imageBytes); // 7.0.1: ReadOnlySpan<byte> 重载
            Texture2D tex = LoadTextureFromImage(img);
            UnloadImage(img);
            return new SlimDX.Direct3D9.Texture { RL = tex };
        }

        // ============== 内部绘制实现 ==============
        private static void DrawOne(Texture2D tex, Rectangle src, Rectangle dst, Vector2 origin, float rotationDeg, SlimDX.Color4 c)
        {
            BeginTextureIfNeeded();

            bool useBlend = Blending;
            if (useBlend) BeginBlendMode(MapBlendMode(BlendMode));

            var tint = ((System.Drawing.Color)c).ToRay();
            tint.A = (byte)(tint.A * Opacity); // 透明度叠乘

            DrawTexturePro(tex, src, dst, origin, rotationDeg, tint);

            if (useBlend) EndBlendMode();
        }

        private static void BeginTextureIfNeeded()
        {
            if (!_textureModeBegun)
            {
                BeginTextureMode(_currentTarget);
                _textureModeBegun = true;
            }
        }

        private static void EndTextureIfNeeded()
        {
            if (_textureModeBegun)
            {
                EndTextureMode();
                _textureModeBegun = false;
            }
        }

        private static SlimDX.Direct3D9.Texture CreatePoisonTexture()
        {
            Image img = GenImageColor(6, 6, Color.White);
            // 7.0.1：ImageDrawRectangleLines(ref Image, Rectangle, int, Color)
            ImageDrawRectangleLines(ref img, new Rectangle(0, 0, 6, 6), 1, Color.Green);
            Texture2D tex = LoadTextureFromImage(img);
            UnloadImage(img);
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