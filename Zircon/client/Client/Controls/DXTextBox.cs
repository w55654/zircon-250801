using Client1000.RayDraw;
using Ray2D;
using Raylib_cs;
using System;
using System.Drawing;
using System.Text;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

//Cleaned
namespace Client.Controls
{
    public class DXTextBox : DXControl
    {
        public static DXTextBox CurrentFocused = null;

        private int _length;
        private int _framesCounter;
        private int fontSize = 22;

        public bool Multiline;
        public bool IsPassword;
        public TextFormat DrawFormat;

        private int cursorXpos;
        private bool ShowCursor;
        public bool KeepFocus;
        private int cursorDrawOffset;

        public int SelectionStart;
        public int SelectionLength;
        public int TextLength;

        public bool Password;

        public static DXTextBox ActiveTextBox
        {
            get => _ActiveTextBox;
            set
            {
                if (_ActiveTextBox == value) return;

                var oldValue = _ActiveTextBox;
                _ActiveTextBox = value;

                oldValue?.OnDeactivated();
                _ActiveTextBox?.OnActivated();
            }
        }

        private static DXTextBox _ActiveTextBox;

        public bool Editable
        {
            get => _Editable;
            set
            {
                if (_Editable == value) return;

                bool oldValue = _Editable;
                _Editable = value;

                OnEditableChanged(oldValue, value);
            }
        }

        private bool _Editable;

        public event EventHandler<EventArgs> EditableChanged;

        public virtual void OnEditableChanged(bool oValue, bool nValue)
        {
            EditableChanged?.Invoke(this, EventArgs.Empty);
        }

        public int MaxLength
        {
            get => _MaxLength;
            set
            {
                if (_MaxLength == value) return;

                int oldValue = _MaxLength;
                _MaxLength = value;
            }
        }

        private int _MaxLength;

        public bool ReadOnly
        {
            get => _ReadOnly;
            set
            {
                if (_ReadOnly == value) return;

                bool oldValue = _ReadOnly;
                _ReadOnly = value;
            }
        }

        private bool _ReadOnly;

        public MouseButton Button;

        //public string Text
        //{
        //    get => _Text;
        //    set => SetProp(ref _Text, value, OnTextChanged);
        //}

        //private string _Text = "";

        public override void OnTextChanged(string oValue, string nValue)
        {
            if (Password)
            {
                DrawText = new string('*', Text.Length);
            }
            else
            {
                DrawText = Text;
            }

            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> TextChanged, GotFocus;

        private string DrawText;
        private bool Activited = false;

        public DXTextBox()
        {
            Border = true;
            BorderColour = DefGoldColor;
            MaxLength = 30;
            Editable = true;
            DrawFormat = TextFormat.Left | TextFormat.HorizontalCenter;
            Size = new Size(100, 22);
        }

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            if (Size.Height < 22)
            {
            }
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            BorderColour = Color.Red;
            Raylib.SetMouseCursor(MouseCursor.IBeam);
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            BorderColour = DefGoldColor;
            Raylib.SetMouseCursor(MouseCursor.Default);
        }

        public override void Process()
        {
            base.Process();

            if (Activited)
            {
                int key = Raylib.GetCharPressed();
                while (key > 0)
                {
                    if (_length < MaxLength)
                    {
                        string str = char.ConvertFromUtf32(key);
                        //Console.WriteLine($"你输入的是：{str}");
                        Text += str;
                    }
                    key = Raylib.GetCharPressed();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Backspace))
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                    }
                }

                _framesCounter++;
            }
            else
            {
                _framesCounter = 0;
            }
        }

        protected override void DrawControl()
        {
            Raylib.DrawRectangleRec(DisplayArea.ToRayRect(), Raylib_cs.Color.LightGray);

            RayFont.DrawText(fontSize, DrawText, new Point(DisplayArea.X + 2, DisplayArea.Y), Color.Maroon);

            if (Activited && (_framesCounter / 20) % 2 == 0)
            {
                var size = RayFont.GetTextSize(fontSize, DrawText);
                RayFont.DrawText(fontSize, "_", new Point(DisplayArea.X + size.Width, DisplayArea.Y), Color.Maroon);
            }
        }

        public void SelectAll()
        {
        }

        public int GetLineFromCharIndex(int index)
        {
            return index;
        }

        private void SetCursorPos(int x)
        {
            //if (string.IsNullOrEmpty(m_DrawText))
            //{
            //    cursorXpos = 0;
            //    cursorDrawOffset = 0;
            //    return;
            //}

            //cursorXpos = Math.Clamp(x, 0, m_DrawText.Length);

            //string sub = m_DrawText.Substring(0, cursorXpos);

            //m_SDLFont.GetTextSize(sub, out cursorDrawOffset, out int h);
        }

        public void InputText(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            bool isEnd = cursorXpos >= Text.Length;
            if (isEnd)
            {
                Text += str;
                SetCursorPos(Text.Length);
            }
            else
            {
                Text = Text.Insert(cursorXpos, str);
                SetCursorPos(cursorXpos + str.Length);
            }
        }

        public virtual void OnActivated()
        {
            Activited = true;
        }

        public virtual void OnDeactivated()
        {
            Activited = false;
            ShowCursor = false;
        }

        public bool CanFocus()
        {
            return Visible && Editable && IsEnabled;
        }

        public void SetFocus()
        {
            if (CanFocus())
            {
                ActiveTextBox = this;
            }
        }

        public void RemoveText()
        {
            Text = Text.Remove(cursorXpos - 1, 1);
        }

        public void calcCursorLocation(int mx, int my)
        {
            //if (string.IsNullOrEmpty(Text))
            //{
            //    SetCursorPos(0);
            //    _TextWidth = 0;
            //    _TextHeight = m_SDLFont.FontHeight();
            //    CalcDrawOffset();
            //    return;
            //}

            //int realx = mx;

            //int w, h;
            //for (var i = 1; i < m_DrawText.Length; i++)
            //{
            //    var ss = m_DrawText.Substring(0, i);
            //    m_SDLFont.GetTextSize(ss.ToString(), out w, out h);
            //    if (realx <= w)
            //    {
            //        SetCursorPos(i);
            //        return;
            //    }
            //}
            //SetCursorPos(m_DrawText.Length);
        }

        protected void OnDisposeManaged()
        {
            //base.OnDisposeManaged();

            _Editable = false;
            _MaxLength = 0;
            _ReadOnly = false;

            EditableChanged = null;

            if (_ActiveTextBox == this)
                _ActiveTextBox = null;
        }
    }

    [Flags]
    public enum TextFormat
    {
        None = 0,            // 无对齐，默认行为

        // 水平对齐（Horizontal Alignment）
        Left = 1 << 0,       // 左对齐，文本从控件左边开始绘制

        HorizontalCenter = 1 << 1,       // 水平居中，对齐到控件宽度的中间
        Right = 1 << 2,       // 右对齐，文本从控件右边开始绘制

        // 垂直对齐（Vertical Alignment）
        Top = 1 << 3,       // 顶部对齐，文本绘制在控件顶部

        VerticalCenter = 1 << 4,       // 垂直居中，对齐到控件高度的中间
        Bottom = 1 << 5,       // 底部对齐，文本绘制在控件底部

        // 常用组合（Optional common presets）
        TopLeft = Top | Left,             // 左上角对齐

        TopCenter = Top | HorizontalCenter, // 顶部水平居中
        TopRight = Top | Right,            // 右上角对齐
        MiddleLeft = VerticalCenter | Left,  // 左侧垂直居中
        MiddleCenter = VerticalCenter | HorizontalCenter, // 居中（水平+垂直）
        MiddleRight = VerticalCenter | Right, // 右侧垂直居中
        BottomLeft = Bottom | Left,           // 左下角对齐
        BottomCenter = Bottom | HorizontalCenter, // 底部水平居中
        BottomRight = Bottom | Right           // 右下角对齐
    }
}