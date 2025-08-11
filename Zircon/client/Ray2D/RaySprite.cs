using Client1000.RayDraw;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UtilsShared;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

namespace R1000y
{
    public class RaySprite : HDisposable
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; } = 0f;
        public float Scale { get; set; } = 1f;
        public Color Tint { get; set; } = Color.White;
        public Rectangle? SourceRect { get; set; } = null;

        public RaySprite(string texturePath)
        {
            Texture = Raylib.LoadTexture(texturePath);

            if (!IsTextureValid(Texture))
            {
                throw new InvalidOperationException($"无法加载纹理文件：{texturePath}");
            }

            // 默认设置中心点为整张图中心
            Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        }

        public RaySprite(int w, int h, Color color = default)
        {
            // 创建一个 100x100 的纯白 Image
            Image image = Raylib.GenImageColor(w, h, color);

            // 将 Image 转换为 GPU 上的 Texture2D
            Texture = Raylib.LoadTextureFromImage(image);

            if (!IsTextureValid(Texture))
            {
                throw new InvalidOperationException($"创建纹理文件失败!!!!!!!!!!");
            }

            // 默认设置中心点为整张图中心
            Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        }

        public void Draw()
        {
            if (!IsTextureValid(Texture))
                return;

            Rectangle source = SourceRect ?? new Rectangle(0, 0, Texture.Width, Texture.Height);
            Vector2 size = new Vector2(source.Width, source.Height) * Scale;

            Vector2 originToUse = Origin;

            // 如果使用 SourceRect，则默认中心点应使用 SourceRect 中心
            if (SourceRect.HasValue && Origin == default)
            {
                originToUse = new Vector2(source.Width / 2f, source.Height / 2f);
            }

            Raylib.DrawTexturePro(Texture, source, new Rectangle(Position.X, Position.Y, size.X, size.Y),
                originToUse, Rotation, Tint);
        }

        public void Draw(int x, int y, System.Drawing.Color color)
        {
            if (!IsTextureValid(Texture))
                return;

            Raylib.DrawTexture(Texture, x, y, color.ToRayColor());
        }

        public void Draw(System.Drawing.Point pt, System.Drawing.Color color)
        {
            Draw(pt.X, pt.Y, color);
        }

        public void SetTexture(Texture2D newTexture)
        {
            if (!IsTextureValid(newTexture))
            {
                throw new ArgumentException("提供的纹理无效。");
            }

            // 卸载旧纹理
            if (IsTextureValid(Texture))
            {
                Raylib.UnloadTexture(Texture);
            }

            Texture = newTexture;

            // 重新计算中心点
            Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
        }

        protected override void OnDisposeManaged()
        {
            if (IsTextureValid(Texture))
            {
                Raylib.UnloadTexture(Texture);
                Texture = default;
            }
        }

        private bool IsTextureValid(Texture2D tex)
        {
            return tex.Id != 0 && tex.Width > 0 && tex.Height > 0;
        }
    }
}