using Client.Envir;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Drawing;
using System.Numerics;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Client.Controls
{
    /// <summary>
    /// NinePatch 九宫格绘制工具
    /// </summary>
    public class DXNineImage : DXControl
    {
        private const int ClipBorder = 10;              // 九宫格边界宽度（像素）

        private Rectangle[] SrcRects;    // 贴图上的九宫格切片区域

        #region Index

        public int Index
        {
            get => _Index;
            set
            {
                if (_Index == value) return;

                int oldValue = _Index;
                _Index = value;

                OnIndexChanged(oldValue, value);
            }
        }

        private int _Index;

        public virtual void OnIndexChanged(int oValue, int nValue)
        {
            //TextureValid = false;
            CalcRealSize();
        }

        #endregion Index

        #region LibraryFile

        public MirLibrary SprLib;

        public LibraryFile LibraryFile
        {
            get => _LibraryFile;
            set
            {
                if (_LibraryFile == value) return;

                LibraryFile oldValue = _LibraryFile;
                _LibraryFile = value;

                OnLibraryFileChanged(oldValue, value);
            }
        }

        private LibraryFile _LibraryFile;

        public virtual void OnLibraryFileChanged(LibraryFile oValue, LibraryFile nValue)
        {
            CEnvir.LibraryList.TryGetValue(LibraryFile, out SprLib);

            CalcRealSize();
        }

        #endregion LibraryFile

        public DXNineImage()
        {
            BackColour = WinBackColor;
        }

        private void CalcRealSize()
        {
            if (SprLib != null && Index >= 0)
            {
                var size = SprLib.GetSize(Index);

                SrcRects = new Rectangle[9];

                // 左上角
                SrcRects[0] = new Rectangle(0, 0, ClipBorder, ClipBorder);
                // 上边中间
                SrcRects[1] = new Rectangle(ClipBorder, 0, size.Width - 2 * ClipBorder, ClipBorder);
                // 右上角
                SrcRects[2] = new Rectangle(size.Width - ClipBorder, 0, ClipBorder, ClipBorder);

                // 左边中间
                SrcRects[3] = new Rectangle(0, ClipBorder, ClipBorder, size.Height - 2 * ClipBorder);
                // 中间区域
                SrcRects[4] = new Rectangle(ClipBorder, ClipBorder, size.Width - 2 * ClipBorder, size.Height - 2 * ClipBorder);
                // 右边中间
                SrcRects[5] = new Rectangle(size.Width - ClipBorder, ClipBorder, ClipBorder, size.Height - 2 * ClipBorder);

                // 左下角
                SrcRects[6] = new Rectangle(0, size.Height - ClipBorder, ClipBorder, ClipBorder);
                // 下边中间
                SrcRects[7] = new Rectangle(ClipBorder, size.Height - ClipBorder, size.Width - 2 * ClipBorder, ClipBorder);
                // 右下角
                SrcRects[8] = new Rectangle(size.Width - ClipBorder, size.Height - ClipBorder, ClipBorder, ClipBorder);
            }
        }

        private void RealDraw(Rectangle rect, Color color)
        {
            int b = ClipBorder;

            // 中间区域宽高 = 总宽高 - 两边边框宽高
            int cw = rect.Width - 2 * b;
            int ch = rect.Height - 2 * b;

            // 依次绘制九个部分，每部分都使用 DrawTexturePro 进行拉伸
            // 注意：Vector2.Zero 表示旋转中心，0 表示无旋转角度

            // 左上角
            SprLib.DrawPro(Index, rect.X, rect.Y, color, 1F, SrcRects[0], new Size(b, b));
            // 上边中间
            SprLib.DrawPro(Index, rect.X + b, rect.Y, color, 1F, SrcRects[1], new Size(cw, b));
            // 右上角
            SprLib.DrawPro(Index, rect.X + b + cw, rect.Y, color, 1F, SrcRects[2], new Size(b, b));

            // 左边中间
            SprLib.DrawPro(Index, rect.X, rect.Y + b, color, 1F, SrcRects[3], new Size(b, ch));
            // 中间区域
            SprLib.DrawPro(Index, rect.X + b, rect.Y + b, color, 1F, SrcRects[4], new Size(cw, ch));
            // 右边中间
            SprLib.DrawPro(Index, rect.X + b + cw, rect.Y + b, color, 1F, SrcRects[5], new Size(b, ch));

            // 左下角
            SprLib.DrawPro(Index, rect.X, rect.Y + b + ch, color, 1F, SrcRects[6], new Size(b, b));
            // 下边中间
            SprLib.DrawPro(Index, rect.X + b, rect.Y + b + ch, color, 1F, SrcRects[7], new Size(cw, b));
            // 右下角
            SprLib.DrawPro(Index, rect.X + b + cw, rect.Y + b + ch, color, 1F, SrcRects[8], new Size(b, b));
        }

        protected override void OnBeforeDraw()
        {
            base.OnBeforeDraw();

            if (BackColour != Color.Empty)
            {
                RayDraw.DrawRect(DisplayArea, BackColour);
            }
        }

        protected override void DrawControl()
        {
            base.DrawControl();

            RealDraw(DisplayArea, ForeColour);

            //RayDraw.DrawRectLines(DisplayArea, 10F, Color.Red);
            //RayFont.DrawText(20, this.ToString(), new Point(DisplayArea.X + 2, DisplayArea.Y), Color.Red);
        }
    }
}