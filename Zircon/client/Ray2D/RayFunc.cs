using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image = Raylib_cs.Image;

namespace Ray2D
{
    public static class RayFunc
    {
        public static bool IsImageValid(Image img)
        {
            return img.Width > 0 && img.Height > 0;
        }

        public static bool IsTextureValid(Texture2D texture)
        {
            return texture.Id != 0 && texture.Width > 0 && texture.Height > 0;
        }

        public static void PointOffset(ref Point p, int dx, int dy)
        {
            p.Offset(dx, dy);
        }
    }
}