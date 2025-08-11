using System.Numerics;
using System.Drawing;

namespace Ray2D
{
    public static class RayExtensions
    {
        public static Point ToPoint(this Vector2 v)
        {
            int x = (int)v.X;
            int y = (int)v.Y;
            return new Point(x, y);
        }

        public static PointF ToPointF(this Vector2 v)
        {
            float x = v.X;
            float y = v.Y;
            return new PointF(x, y);
        }

        public static Vector2 ToVector2(this Point p)
        {
            float x = p.X;
            float y = p.Y;
            return new Vector2(x, y);
        }

        public static Vector2 ToVector2(this PointF pf)
        {
            float x = pf.X;
            float y = pf.Y;
            return new Vector2(x, y);
        }

        public static Size ToSize(this Vector2 v)
        {
            return new Size((int)Math.Ceiling(v.X), (int)Math.Ceiling(v.Y));
        }

        public static Raylib_cs.Color ToRayColor(this System.Drawing.Color color)
        {
            return new Raylib_cs.Color(color.R, color.G, color.B, color.A);
        }

        public static Raylib_cs.Color ToRayColor(this System.Drawing.Color color, float alpha)
        {
            byte newA = (byte)(color.A * alpha);
            var new_color = new Raylib_cs.Color(color.R, color.G, color.B, newA);
            return new_color;
        }

        public static System.Drawing.Color ToSysColor(this Raylib_cs.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Raylib_cs.Rectangle ToRayRect(this System.Drawing.Rectangle rect)
        {
            return new Raylib_cs.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static System.Drawing.RectangleF ToSysRectF(this System.Drawing.Rectangle rect)
        {
            return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static System.Drawing.Rectangle ToSysRect(this Raylib_cs.Rectangle rect)
        {
            return new System.Drawing.Rectangle(
                (int)rect.X,
                (int)rect.Y,
                (int)rect.Width,
                (int)rect.Height
            );
        }
    }
}