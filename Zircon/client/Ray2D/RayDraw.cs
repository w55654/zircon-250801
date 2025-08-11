using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray2D
{
    public static class RayDraw
    {
        public static readonly Raylib_cs.Color BlankColor = new Raylib_cs.Color(0, 0, 0, 0);
        public static readonly Raylib_cs.Color CloneTextColor = new Raylib_cs.Color(255, 255, 255, 64);

        public static void DrawRect(System.Drawing.Rectangle rect, System.Drawing.Color color)
        {
            Raylib.DrawRectangleRec(rect.ToRayRect(), color.ToRayColor());
        }

        public static void DrawRectLines(System.Drawing.Rectangle rect, float lineThick, System.Drawing.Color color)
        {
            Raylib.DrawRectangleLinesEx(rect.ToRayRect(), lineThick, color.ToRayColor());
        }

        public static void DrawRectangleRounded(System.Drawing.Rectangle rect, float roundness, int segments, System.Drawing.Color color)
        {
            Raylib.DrawRectangleRounded(rect.ToRayRect(), roundness, segments, color.ToRayColor());
        }
    }
}