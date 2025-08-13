// MirLibrary.cs — raylib-cs 7.0.1 替换版
// 目标：不改类名/函数名/签名；把 DXT1/DXT5 数据解码为 RGBA，用 raylib 生成纹理；
// 不能等价的 SlimDX 行为（锁显存/设备状态）全部删除或标注“注意”。

using Library;
using Ray2D;
using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using static Raylib_cs.Raylib;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Client.Envir
{
    public sealed class MirLibrary : UtilsShared.HDisposable
    {
        public readonly object LoadLocker = new object();

        public int Version;
        public string FileName;

        private FileStream _FStream;
        private BinaryReader _BReader;

        public bool Loaded, Loading;
        public MirImage[] Images;

        public MirLibrary(string fileName)
        {
            _FStream = File.OpenRead(fileName);
            _BReader = new BinaryReader(_FStream);
        }

        public void ReadLibrary()
        {
            lock (LoadLocker)
            {
                if (Loading) return;
                Loading = true;
            }

            if (_BReader == null)
            {
                Loaded = true;
                return;
            }

            using (MemoryStream mstream = new MemoryStream(_BReader.ReadBytes(_BReader.ReadInt32())))
            using (BinaryReader reader = new BinaryReader(mstream))
            {
                int value = reader.ReadInt32();

                int count = value & 0x1FFFFFF;
                Version = (value >> 25) & 0x7F;

                if (Version == 0) count = value; // 老库 V0

                Images = new MirImage[count];

                for (int i = 0; i < Images.Length; i++)
                {
                    if (!reader.ReadBoolean()) continue;
                    Images[i] = new MirImage(reader, Version);
                }
            }

            Loaded = true;
        }

        public Size GetSize(int index)
        {
            if (!CheckImage(index)) return Size.Empty;
            return new Size(Images[index].Width, Images[index].Height);
        }

        public Point GetOffSet(int index)
        {
            if (!CheckImage(index)) return Point.Empty;
            return new Point(Images[index].OffSetX, Images[index].OffSetY);
        }

        public MirImage GetImage(int index)
        {
            if (!CheckImage(index)) return null;
            return Images[index];
        }

        public MirImage CreateImage(int index, ImageType type)
        {
            if (!CheckImage(index)) return null;

            MirImage image = Images[index];
            RayTexture texture;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    break;

                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;
                    break;

                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    break;

                default:
                    return null;
            }

            if (texture == null) return null;
            return image;
        }

        private bool CheckImage(int index)
        {
            if (!Loaded) ReadLibrary();
            while (!Loaded) Thread.Sleep(1);
            return index >= 0 && index < Images.Length && Images[index] != null;
        }

        public bool VisiblePixel(int index, Point location, bool accurate = true, bool offSet = false)
        {
            if (!CheckImage(index)) return false;

            MirImage image = Images[index];

            if (offSet)
                location = new Point(location.X - image.OffSetX, location.Y - image.OffSetY);

            return image.VisiblePixel(location, accurate);
        }

        // 注意：原版这里严重依赖 SpriteTransform 做矩阵变换（D3D9 的 Sprite），
        // raylib 没这玩意儿。我们保留调用，但当前只按位置绘制，旋转=0。
        // 如果你确实需要矩阵效果，请把变换下放到 DXManager.SpriteDraw 或直接把 angle/scale 传进去算 dst 矩形。
        public void Draw(int index, float x, float y, Color colour, Rectangle area, float opacity, ImageType type, byte shadow = 0)
        {
            if (!CheckImage(index)) return;
            MirImage image = Images[index];

            RayTexture texture;
            float oldOpacity = DXManager.Opacity;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    break;

                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;

                    if (texture == null)
                    {
                        // 注意：老代码这里用 SpriteTransform 画“假阴影”:contentReference[oaicite:2]{index=2}
                        // raylib 版本暂不复刻（需要 shader 或手动拉伸/斜切），先跳过。
                        return;
                    }
                    break;

                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    break;

                default:
                    return;
            }

            if (texture == null) return;

            DXManager.SetOpacity(opacity);
            DXManager.SpriteDraw(texture, area, Vector2.Zero, new Vector2(x, y), colour);
            CEnvir.DPSCounter++;
            DXManager.SetOpacity(oldOpacity);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }

        public void Draw(int index, float x, float y, Color colour, bool useOffSet, float opacity, ImageType type, float scale = 1F)
        {
            if (!CheckImage(index)) return;
            MirImage image = Images[index];

            RayTexture texture;
            float oldOpacity = DXManager.Opacity;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    if (useOffSet) { x += image.OffSetX; y += image.OffSetY; }
                    break;

                case ImageType.Shadow:
                    // 注意：老版的斜切阴影/半高阴影用矩阵+Point 采样；这里不给等价实现
                    // 需要时请改 shader 或者自定义 DrawShadow。
                    return;

                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    if (useOffSet) { x += image.OffSetX; y += image.OffSetY; }
                    break;

                default:
                    return;
            }

            if (texture == null) return;

            DXManager.SetOpacity(opacity);

            // 注意：scale 目前未生效（原来靠矩阵缩放）。如需要缩放，请把 SpriteDraw 改成支持缩放/旋转。
            DXManager.SpriteDraw(texture, Vector2.Zero, new Vector2(x, y), colour);

            CEnvir.DPSCounter++;
            DXManager.SetOpacity(oldOpacity);

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }

        public void DrawBlend(int index, float size, Color colour, float x, float y, float angle, float opacity, ImageType type, bool useOffSet = false, byte shadow = 0)
        {
            if (!CheckImage(index)) return;
            MirImage image = Images[index];

            RayTexture texture;
            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    if (useOffSet) { x += image.OffSetX; y += image.OffSetY; }
                    break;

                case ImageType.Shadow:
                    return; // 同上：老阴影不复刻
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    if (useOffSet) { x += image.OffSetX; y += image.OffSetY; }
                    break;

                default:
                    return;
            }
            if (texture == null) return;

            // 注意：size/angle 原来靠 SpriteTransform，这里暂未生效。
            DXManager.SpriteDraw(texture, Vector2.Zero, new Vector2(x, y), colour);

            CEnvir.DPSCounter++;

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }

        public void DrawPro(int index, int x, int y, Color color, float alpha, Rectangle clip, Size? dest = null)
        {
            MirImage image = GetImage(index);

            if (image == null)
                return;

            image.DrawPro(x, y, color, alpha, clip, dest);
        }

        public void DrawBlend(int index, float x, float y, Color colour, bool useOffSet, float rate, ImageType type, byte shadow = 0)
        {
            if (!CheckImage(index))
                return;

            MirImage image = Images[index];

            RayTexture texture;
            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid)
                        image.CreateImage(_BReader);
                    texture = image.Image;
                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;

                case ImageType.Shadow:
                    return;

                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    if (useOffSet) { x += image.OffSetX; y += image.OffSetY; }
                    break;

                default:
                    return;
            }
            if (texture == null) return;

            DXManager.SpriteDraw(texture, Vector2.Zero, new Vector2(x, y), colour);
            CEnvir.DPSCounter++;

            image.ExpireTime = Time.Now + Config.CacheDuration;
        }

        protected override void OnDisposeManaged()
        {
            if (Images != null)
            {
                foreach (MirImage image in Images)
                    image?.Dispose();
            }
            Images = null;

            _BReader?.Dispose(); _BReader = null;
            _FStream?.Dispose(); _FStream = null;

            Loading = false;
            Loaded = false;
        }
    }

    public sealed class MirImage : IDisposable
    {
        public int Version;
        public int Position;

        #region Texture fields (保留签名)

        public short Width;
        public short Height;
        public short OffSetX;
        public short OffSetY;

        public byte ShadowType;
        public RayTexture Image;
        public bool ImageValid { get; private set; }
        public unsafe byte* ImageData;      // 注意：不再使用，保留为 null

        public int ImageDataSize
        {
            get
            {
                int w = Width + (4 - Width % 4) % 4;
                int h = Height + (4 - Height % 4) % 4;
                return (Version > 0) ? w * h : (w * h / 2); // V0 DXT1；其他 DXT5
            }
        }

        #endregion

        #region Shadow

        public short ShadowWidth;
        public short ShadowHeight;

        public short ShadowOffSetX;
        public short ShadowOffSetY;

        public RayTexture Shadow;
        public bool ShadowValid { get; private set; }
        public unsafe byte* ShadowData;     // 注意：不再使用，保留为 null

        public int ShadowDataSize
        {
            get
            {
                int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
                int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;
                return (Version > 0) ? w * h : (w * h / 2);
            }
        }

        #endregion

        #region Overlay

        public short OverlayWidth;
        public short OverlayHeight;

        public RayTexture Overlay;
        public bool OverlayValid { get; private set; }
        public unsafe byte* OverlayData;    // 注意：不再使用，保留为 null

        public int OverlayDataSize
        {
            get
            {
                int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
                int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;
                return (Version > 0) ? w * h : (w * h / 2);
            }
        }

        #endregion

        public DateTime ExpireTime;

        // 解码后的像素缓存（RGBA）
        private Raylib_cs.Color[] _imgPixels;

        private Raylib_cs.Color[] _shadowPixels;
        private Raylib_cs.Color[] _overlayPixels;

        public MirImage(BinaryReader reader, int version)
        {
            Version = version;

            Position = reader.ReadInt32();

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            OffSetX = reader.ReadInt16();
            OffSetY = reader.ReadInt16();

            ShadowType = reader.ReadByte();
            ShadowWidth = reader.ReadInt16();
            ShadowHeight = reader.ReadInt16();
            ShadowOffSetX = reader.ReadInt16();
            ShadowOffSetY = reader.ReadInt16();

            OverlayWidth = reader.ReadInt16();
            OverlayHeight = reader.ReadInt16();
        }

        // 可见像素：看 alpha>0 即可；accurate=true 也没必要再走 DXT bit 魔法了
        public bool VisiblePixel(Point p, bool accurate)
        {
            if (p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height) return false;
            if (!ImageValid || _imgPixels == null) return false;
            var c = _imgPixels[p.Y * Width + p.X];
            return c.A > 0; // 注意：准确度判断和旧版一致性取决于资源是否用 DXT1 带透明索引 3
        }

        public void DisposeTexture()
        {
            try
            {
                if (Image != null && Image.Texture.Id != 0) UnloadTexture(Image.Texture);
                if (Shadow != null && Shadow.Texture.Id != 0) UnloadTexture(Shadow.Texture);
                if (Overlay != null && Overlay.Texture.Id != 0) UnloadTexture(Overlay.Texture);
            }
            catch { /* 忽略释放期异常 */ }

            Image = null; Shadow = null; Overlay = null;
            _imgPixels = null; _shadowPixels = null; _overlayPixels = null;

            ImageValid = false; ShadowValid = false; OverlayValid = false;
            ExpireTime = DateTime.MinValue;

            DXManager.TextureList.Remove(this);
        }

        // 需要在项目里启用 unsafe（csproj <AllowUnsafeBlocks>true</AllowUnsafeBlocks>）
        public unsafe void CreateImage(BinaryReader reader)
        {
            if (Position == 0) return;

            // DXT 4x4 padding
            int pw = (Width + 3) & ~3;
            int ph = (Height + 3) & ~3;
            if (pw <= 0 || ph <= 0) return;

            byte[] blockData;
            lock (reader)
            {
                reader.BaseStream.Seek(Position, SeekOrigin.Begin);
                blockData = reader.ReadBytes(ImageDataSize);
            }

            bool useDXT1 = (Version == 0);

            // 软件解码到 RGBA8（真实尺寸）
            _imgPixels = useDXT1
                ? DecodeDXT1(blockData, Width, Height, pw, ph)
                : DecodeDXT5(blockData, Width, Height, pw, ph);

            // ① 自己做黑色 colorkey + 预乘，别调用 ImageAlpha* 系列
            SoftKeyFromBlackAndPremultiply(_imgPixels, cutoff: 2, feather: 96, mode: 0); // tol=0..255，按资源调

            // 旧纹理卸载
            if (Image != null && Image.Texture.Id != 0)
                Raylib_cs.Raylib.UnloadTexture(Image.Texture);

            // ② 构造 Image 指向 pinned 像素，只做“上传”，不再调用任何会改 data 的 Image 函数
            fixed (Raylib_cs.Color* p = _imgPixels)
            {
                Raylib_cs.Image img = new Raylib_cs.Image
                {
                    Data = p,
                    Width = Width,
                    Height = Height,
                    Mipmaps = 1,
                    Format = Raylib_cs.PixelFormat.UncompressedR8G8B8A8,
                };

                Raylib_cs.Texture2D tex = Raylib_cs.Raylib.LoadTextureFromImage(img);

                // ③ 采样设置，避免边缘吸黑
                Raylib_cs.Raylib.SetTextureWrap(tex, Raylib_cs.TextureWrap.Clamp);
                Raylib_cs.Raylib.SetTextureFilter(tex, Raylib_cs.TextureFilter.Bilinear);

                Image = new RayTexture(tex);
            }

            ImageValid = true;
            ExpireTime = CEnvir.Now + Config.CacheDuration;
            if (!DXManager.TextureList.Contains(this))
                DXManager.TextureList.Add(this);

            unsafe { ImageData = null; } // 别再用旧指针
        }

        // 纯托管处理：按“黑度”软键控 + 预乘
        // cutoff: 低于这个亮度直接透明（0..255）
        // feather: 从 cutoff 到完全不透明的过渡宽度（0..255），越大过渡越柔
        // mode: 0=亮度Y(更接近人眼)，1=HSV的V=max(R,G,B)（更保留高饱和边缘）
        private static void SoftKeyFromBlackAndPremultiply(Raylib_cs.Color[] px, byte cutoff = 4, byte feather = 32, int mode = 0)
        {
            float t = cutoff / 255f;
            float fw = Math.Max(1e-6f, feather / 255f);
            float invFw = 1f / fw;

            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];

                // 归一化到 0..1
                float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;

                // “黑度反向”权重：越亮 -> 值越大
                float w;
                if (mode == 0)
                {
                    // BT.709 亮度，更符合视觉
                    w = 0.2126f * r + 0.7152f * g + 0.0722f * b;
                }
                else
                {
                    // HSV 的 V = max(R,G,B)，更保留高饱和色边缘
                    w = MathF.Max(r, MathF.Max(g, b));
                }

                // 软阈值：<=t 全透明；>=t+fw 保留原 alpha；中间线性渐变
                float k = (w - t) * invFw;
                if (k < 0) k = 0;
                if (k > 1) k = 1;

                // 新 alpha：原 alpha * k
                byte a = (byte)MathF.Round(c.A * k);

                // 预乘 alpha，去黑边
                px[i] = new Raylib_cs.Color(
                    (byte)MathF.Round(c.R * a / 255f),
                    (byte)MathF.Round(c.G * a / 255f),
                    (byte)MathF.Round(c.B * a / 255f),
                    a);
            }
        }

        public void CreateShadow(BinaryReader reader)
        {
        }

        public void CreateOverlay(BinaryReader reader)
        {
        }

        public void UpdateExpireTime()
        {
            ExpireTime = DateTime.Now + TimeSpan.FromMinutes(30);
        }

        public void DrawPro(int x, int y, Color color, float alpha, Rectangle clip, Size? dest = null)
        {
            if (Image == null || !ImageValid)
                return;

            //if (useOffset)
            //{
            //    x += OffSetX;
            //    y += OffSetY;
            //}

            Image.DrawPro(x, y, color, alpha, clip, dest);

            UpdateExpireTime();
        }

        #region IDisposable Support

        public bool IsDisposed { get; private set; }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                Position = 0;

                Width = 0; Height = 0; OffSetX = 0; OffSetY = 0;
                ShadowWidth = 0; ShadowHeight = 0; ShadowOffSetX = 0; ShadowOffSetY = 0;
                OverlayWidth = 0; OverlayHeight = 0;

                DisposeTexture();
            }
        }

        public void Dispose()
        { Dispose(!IsDisposed); GC.SuppressFinalize(this); }

        ~MirImage()
        {
            Dispose(false);
        }

        #endregion

        // =================== DXT 解码实现 ===================
        // 注意：这里按块扫描（4x4），写入真实尺寸范围内的像素，忽略边缘填充
        private static Raylib_cs.Color[] DecodeDXT1(byte[] data, int realW, int realH, int paddedW, int paddedH)
        {
            var outPixels = new Raylib_cs.Color[realW * realH];
            int blocksX = paddedW / 4;
            int blocksY = paddedH / 4;

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    int offset = (by * blocksX + bx) * 8;
                    ushort c0 = (ushort)(data[offset] | (data[offset + 1] << 8));
                    ushort c1 = (ushort)(data[offset + 2] | (data[offset + 3] << 8));
                    uint bits = BitConverter.ToUInt32(data, offset + 4);

                    var col0 = RGB565(c0);
                    var col1 = RGB565(c1);
                    var pal = new Raylib_cs.Color[4];

                    pal[0] = new Raylib_cs.Color(col0.r, col0.g, col0.b, (byte)255);
                    pal[1] = new Raylib_cs.Color(col1.r, col1.g, col1.b, (byte)255);

                    if (c0 > c1)
                    {
                        pal[2] = LerpColor(pal[0], pal[1], 1, 2);
                        pal[3] = LerpColor(pal[0], pal[1], 2, 1);
                    }
                    else
                    {
                        pal[2] = LerpColor(pal[0], pal[1], 1, 1);
                        pal[3] = new Raylib_cs.Color(0, 0, 0, 0); // 透明
                    }

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int code = (int)((bits >> (2 * (py * 4 + px))) & 0x3);
                            int x = bx * 4 + px;
                            int y = by * 4 + py;
                            if (x < realW && y < realH)
                                outPixels[y * realW + x] = pal[code];
                        }
                    }
                }
            }
            return outPixels;
        }

        private static Raylib_cs.Color[] DecodeDXT5(byte[] data, int realW, int realH, int paddedW, int paddedH)
        {
            var outPixels = new Raylib_cs.Color[realW * realH];
            int blocksX = paddedW / 4;
            int blocksY = paddedH / 4;

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    int offset = (by * blocksX + bx) * 16;

                    byte a0 = data[offset + 0];
                    byte a1 = data[offset + 1];

                    ulong alphaBits = 0;
                    for (int i = 0; i < 6; i++)
                        alphaBits |= ((ulong)data[offset + 2 + i]) << (8 * i);

                    // 颜色块
                    ushort c0 = (ushort)(data[offset + 8] | (data[offset + 9] << 8));
                    ushort c1 = (ushort)(data[offset + 10] | (data[offset + 11] << 8));
                    uint bits = BitConverter.ToUInt32(data, offset + 12);

                    var col0 = RGB565(c0);
                    var col1 = RGB565(c1);
                    var pal = new Raylib_cs.Color[4];
                    pal[0] = new Raylib_cs.Color(col0.r, col0.g, col0.b, (byte)255);
                    pal[1] = new Raylib_cs.Color(col1.r, col1.g, col1.b, (byte)255);
                    pal[2] = LerpColor(pal[0], pal[1], 1, 2);
                    pal[3] = LerpColor(pal[0], pal[1], 2, 1);

                    // 预算 alpha palette
                    byte[] aPal = new byte[8];
                    aPal[0] = a0; aPal[1] = a1;
                    if (a0 > a1)
                    {
                        for (int i = 1; i <= 6; i++)
                            aPal[i + 1] = (byte)((((7 - i) * a0) + (i * a1)) / 7);
                    }
                    else
                    {
                        for (int i = 1; i <= 4; i++)
                            aPal[i + 1] = (byte)((((5 - i) * a0) + (i * a1)) / 5);
                        aPal[6] = 0; aPal[7] = 255;
                    }

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int colorCode = (int)((bits >> (2 * (py * 4 + px))) & 0x3);
                            int alphaCode = (int)((alphaBits >> (3 * (py * 4 + px))) & 0x7);
                            byte a = aPal[alphaCode];

                            int x = bx * 4 + px;
                            int y = by * 4 + py;
                            if (x < realW && y < realH)
                            {
                                var c = pal[colorCode];
                                outPixels[y * realW + x] = new Raylib_cs.Color(c.R, c.G, c.B, a);
                            }
                        }
                    }
                }
            }
            return outPixels;
        }

        private static (byte r, byte g, byte b) RGB565(ushort v)
        {
            // 5:6:5 扩展到 8bit
            byte r = (byte)(((v >> 11) & 0x1F) * 255 / 31);
            byte g = (byte)(((v >> 5) & 0x3F) * 255 / 63);
            byte b = (byte)((v & 0x1F) * 255 / 31);
            return (r, g, b);
        }

        private static Raylib_cs.Color LerpColor(Raylib_cs.Color a, Raylib_cs.Color b, int num, int den)
        {
            byte r = (byte)((a.R * num + b.R * den) / (num + den));
            byte g = (byte)((a.G * num + b.G * den) / (num + den));
            byte bl = (byte)((a.B * num + b.B * den) / (num + den));
            return new Raylib_cs.Color(r, g, bl, (byte)255);
        }
    }

    public enum ImageType
    {
        Image,
        Shadow,
        Overlay,
    }
}