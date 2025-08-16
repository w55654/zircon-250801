using Client.Envir;
using Ray2D;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Font = System.Drawing.Font;

//Cleaned
namespace Client.Controls
{
    public class DXLabel : DXControl
    {
        #region Properties

        #region AutoSize

        public bool AutoSize
        {
            get => _AutoSize;
            set
            {
                if (_AutoSize == value) return;

                bool oldValue = _AutoSize;
                _AutoSize = value;

                OnAutoSizeChanged(oldValue, value);
            }
        }

        private bool _AutoSize;

        public event EventHandler<EventArgs> AutoSizeChanged;

        public virtual void OnAutoSizeChanged(bool oValue, bool nValue)
        {
            CreateSize();

            AutoSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool WordWrap { get; set; } = false;
        public Size TextSize { get; private set; }// 文本绘制尺寸（测量出来的）

        public int FontSize { get; set; } = 20;

        #endregion

        #region DrawFormat

        public TextFormatFlags DrawFormat
        {
            get => _DrawFormat;
            set
            {
                if (_DrawFormat == value) return;

                TextFormatFlags oldValue = _DrawFormat;
                _DrawFormat = value;

                OnDrawFormatChanged(oldValue, value);
            }
        }

        private TextFormatFlags _DrawFormat;

        public event EventHandler<EventArgs> DrawFormatChanged;

        public virtual void OnDrawFormatChanged(TextFormatFlags oValue, TextFormatFlags nValue)
        {
            DrawFormatChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Outline

        public bool Outline
        {
            get => _Outline;
            set
            {
                if (_Outline == value) return;

                bool oldValue = _Outline;
                _Outline = value;

                OnOutlineChanged(oldValue, value);
            }
        }

        private bool _Outline;

        public event EventHandler<EventArgs> OutlineChanged;

        public virtual void OnOutlineChanged(bool oValue, bool nValue)
        {
            CreateSize();

            OutlineChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region DropShadow

        public bool DropShadow
        {
            get => _DropShadow;
            set
            {
                if (_DropShadow == value) return;

                bool oldValue = _DropShadow;
                _DropShadow = value;

                OnDropShadowChanged(oldValue, value);
            }
        }

        private bool _DropShadow;

        public event EventHandler<EventArgs> DropShadowChanged;

        public virtual void OnDropShadowChanged(bool oValue, bool nValue)
        {
            CreateSize();

            DropShadowChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region OutlineColour

        public Color OutlineColour
        {
            get => _OutlineColour;
            set
            {
                if (_OutlineColour == value) return;

                Color oldValue = _OutlineColour;
                _OutlineColour = value;

                OnOutlineColourChanged(oldValue, value);
            }
        }

        private Color _OutlineColour;

        public event EventHandler<EventArgs> OutlineColourChanged;

        public virtual void OnOutlineColourChanged(Color oValue, Color nValue)
        {
            OutlineColourChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region PaddingBottom

        public int PaddingBottom
        {
            get => _PaddingBottom;
            set
            {
                if (_PaddingBottom == value) return;

                int oldValue = _PaddingBottom;
                _PaddingBottom = value;

                OnPaddingBottomChanged(oldValue, value);
            }
        }

        private int _PaddingBottom;

        public event EventHandler<EventArgs> PaddingBottomChanged;

        public virtual void OnPaddingBottomChanged(int oValue, int nValue)
        {
            CreateSize();

            PaddingBottomChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public override void OnTextChanged(string oValue, string nValue)
        {
            base.OnTextChanged(oValue, nValue);

            CreateSize();
        }

        #endregion

        public DXLabel()
        {
            BackColour = Color.Empty;
            AutoSize = true;
            // wh Font = new Font(Config.FontName, CEnvir.FontSize(8F));
            DrawFormat = TextFormatFlags.WordBreak;
            Outline = true;
            ForeColour = Color.FromArgb(198, 166, 99);
            OutlineColour = Color.Black;
        }

        #region Methods

        private void CreateSize()
        {
            //if (!AutoSize) return;

            //Size = GetSize(Text, Outline, PaddingBottom);

            TextSize = MeasureTextSize();

            if (AutoSize)
            {
                Size = TextSize;
            }
        }

        private Size MeasureTextSize()
        {
            if (WordWrap)
            {
                return CalcWrappedText();
            }
            else
            {
                return RayFont.GetTextSize(FontSize, Text);
            }
        }

        protected override void DrawControl()
        {
            if (string.IsNullOrEmpty(Text))
                return;

            //RayFont.DrawText(FontSize, Text, new Point(Location.X, Location.Y), ForeColour);

            //if (!DrawTexture) return;

            //if (!TextureValid) CreateTexture();

            PresentString(Text, Parent, DisplayArea, IsEnabled ? Color.White : Color.FromArgb(75, 75, 75), Opacity, this, FontSize);

            //ExpireTime = CEnvir.Now + Config.CacheDuration;
        }

        //protected override void DrawBorder()
        //{
        //    base.DrawBorder();
        //    RayDraw.DrawRectLines(DisplayArea, 2F, Color.Gray);
        //}

        private Size CalcWrappedText()
        {
            int lineWidth = 0;
            int maxLineWidth = 0;
            int lines = 1;

            foreach (char c in Text)
            {
                if (c == '\n') // 手动换行
                {
                    lines++;
                    lineWidth = 0;
                    continue;
                }

                Size charSize = RayFont.GetTextSize(FontSize, c.ToString());
                lineWidth += charSize.Width;

                if (lineWidth > Size.Width)
                {
                    lines++;
                    lineWidth = charSize.Width; // 新行开始
                }

                if (lineWidth > maxLineWidth)
                    maxLineWidth = lineWidth;
            }

            int lineHeight = RayFont.GetTextSize(FontSize, "中").Height;
            return new Size(maxLineWidth, lines * lineHeight);
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _AutoSize = false;
                _DrawFormat = TextFormatFlags.Default;
                //_Font?.Dispose();
                //_Font = null;
                _Outline = false;
                _OutlineColour = Color.Empty;

                AutoSizeChanged = null;
                DrawFormatChanged = null;
                //FontChanged = null;
                OutlineChanged = null;
                OutlineColourChanged = null;
            }
        }

        #endregion
    }
}