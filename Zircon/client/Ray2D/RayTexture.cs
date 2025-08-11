using Raylib_cs;
using System.Collections;
using System.Drawing;
using System.Numerics;
using UtilsShared;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

namespace Ray2D
{
    public sealed class RayTexture : HDisposable
    {
        private static Vector2 def_origin = new Vector2(0, 0);

        public Texture2D Texture { get; private set; }

        private BitArray AlphaMask; // 每个位代表一个像素是否不透明

        public static RayTexture Create(string img_path)
        {
            Texture2D texture = Raylib.LoadTexture(img_path);

            if (!RayFunc.IsTextureValid(texture))
            {
                Logger.Print($"错误的Image文件", LogLevel.Error);
                return null;
            }

            RayTexture rayTexture = new RayTexture(texture);

            return rayTexture;
        }

        public static RayTexture Create(byte[] img_bytes, string img_type = ".png")
        {
            // 1. 从内存创建 Image（需要指定图片格式）
            Raylib_cs.Image image = Raylib.LoadImageFromMemory(img_type, img_bytes);

            // 2. 从 Image 创建 GPU 纹理
            Texture2D texture = Raylib.LoadTextureFromImage(image);

            if (!RayFunc.IsTextureValid(texture))
            {
                //throw new InvalidOperationException($"错误的Image文件");
                Raylib.UnloadImage(image);
                return null;
            }

            RayTexture rayTexture = new RayTexture(texture);
            rayTexture.InitMask(image);

            return rayTexture;
        }

        public RayTexture(Texture2D texture)
        {
            Texture = texture;
            // 使用了抗锯齿
            Raylib.SetTextureFilter(Texture, TextureFilter.Bilinear);
        }

        public RayTexture(int w, int h, Color color = default)
        {
            // 创建一个 100x100 的纯白 Image
            Image image = Raylib.GenImageColor(w, h, color);

            // 画红色边框（上下左右各一条线）
            Raylib.ImageDrawRectangleLines(ref image, new Rectangle(0, 0, w, h), 1, Color.Red);

            // 将 Image 转换为 GPU 上的 Texture2D
            Texture = Raylib.LoadTextureFromImage(image);

            if (!IsValid())
            {
                Logger.Print($"创建纹理文件失败!!!!!!!!!!", LogLevel.Error);
            }
            Raylib.UnloadImage(image);  // 释放 image 占用的 CPU 内存
        }

        private void InitMask(Raylib_cs.Image image)
        {
            if (!RayFunc.IsImageValid(image))
                return;

            AlphaMask = new BitArray(image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = Raylib.GetImageColor(image, x, y);
                    bool opaque = c.A > 0;
                    SetPixel(x, y, opaque);
                }
            }
        }

        // 设置某像素是否不透明
        public void SetPixel(int x, int y, bool value)
        {
            if (x < 0 || y < 0 || x >= Texture.Width || y >= Texture.Height)
                return;

            AlphaMask[y * Texture.Width + x] = value;
        }

        public bool GetPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Texture.Width || y >= AlphaMask.Length / Texture.Width)
                return false;

            return AlphaMask[y * Texture.Width + x];
        }

        private bool IsValid()
        {
            return RayFunc.IsTextureValid(Texture);
        }

        // DrawTexture .
        public void Draw(int x, int y, System.Drawing.Color color = default, float alpha = 1F, System.Drawing.Rectangle? clip = null, System.Drawing.Rectangle? dest = null)
        {
            // 判断纹理
            if (!IsValid())
                return;

            Raylib_cs.Rectangle clipRect = new Raylib_cs.Rectangle
            {
                X = 0,
                Y = 0,
                Width = Texture.Width,
                Height = Texture.Height,
            };

            // dstRect
            Raylib_cs.Rectangle destRect = new Raylib_cs.Rectangle
            {
                X = x,
                Y = y,
                Width = Texture.Width,
                Height = Texture.Height,
            };

            if (clip != null)
            {
                // clip rect
                clipRect.X = clip.Value.X;
                clipRect.Y = clip.Value.Y;
                clipRect.Width = clip.Value.Width;
                clipRect.Height = clip.Value.Height;

                // dest rect
                destRect.Width = clip.Value.Width;
                destRect.Height = clip.Value.Height;
            }

            // 渲染目标
            if (dest != null)
            {
                destRect.X += dest.Value.X;
                destRect.Y += dest.Value.Y;
                destRect.Width = dest.Value.Width;
                destRect.Height = dest.Value.Height;
            }

            Raylib.DrawTexturePro(Texture, clipRect, destRect, def_origin, 0, color.ToRayColor(alpha));
        }

        public void DrawPro(int x, int y, System.Drawing.Color color, float alpha, System.Drawing.Rectangle? clip = null, Size? dest = null)
        {
            // 判断纹理
            if (!IsValid())
                return;

            Raylib_cs.Rectangle clipRect = new Raylib_cs.Rectangle
            {
                X = 0,
                Y = 0,
                Width = Texture.Width,
                Height = Texture.Height,
            };

            // dstRect
            Raylib_cs.Rectangle destRect = new Raylib_cs.Rectangle
            {
                X = x,
                Y = y,
                Width = Texture.Width,
                Height = Texture.Height,
            };

            if (clip != null)
            {
                // clip rect
                clipRect.X = clip.Value.X;
                clipRect.Y = clip.Value.Y;
                clipRect.Width = clip.Value.Width;
                clipRect.Height = clip.Value.Height;

                // dest rect
                destRect.Width = clip.Value.Width;
                destRect.Height = clip.Value.Height;
            }

            if (dest != null)
            {
                destRect.Width = dest.Value.Width;
                destRect.Height = dest.Value.Height;
            }

            //Vector2 center_point = new Vector2()
            //{
            //    X = destRect.Width / 2,
            //    Y = destRect.Width / 2,
            //};

            Raylib.DrawTexturePro(Texture, clipRect, destRect, Vector2.Zero, 0, color.ToRayColor(alpha));
        }

        public void DrawEx(int x, int y, System.Drawing.Color color, float alpha, float rotation, float scale)
        {
            // 屏幕坐标
            Raylib.DrawTextureEx(Texture, new Vector2(x, y), rotation, scale, color.ToRayColor(alpha));
        }

        protected override void OnDisposeUnmanaged()
        {
            // 立即释放"非托管"资源
            if (!IsValid())
            {
                Console.WriteLine($"  Texture 为空 ????????????? ");
                throw new Exception();
            }

            if (IsValid())
            {
                Raylib.UnloadTexture(Texture);
                Texture = default;
            }

            AlphaMask = null;
        }

        protected override void OnDisposeManaged()
        {
            // 立即释放"托管"资源
        }
    }
}