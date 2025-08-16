using Client.DXDraw;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Drawing;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

//Cleaned
namespace Client.Controls
{
    [Flags]
    public enum ButtonStatus
    {
        Normal = 1 << 0,
        Hover = 1 << 1,
        Pressed = 1 << 2,
    }

    public abstract class DXButton : DXControl
    {
        protected static readonly Color NormalColor = Color.White;
        protected static readonly Color HoverColor = Color.FromArgb(192, 235, 250);      // 淡青蓝
        protected static readonly Color PressColor = Color.FromArgb(100, 180, 255);      // 明亮蓝

        protected ButtonStatus BtnStatus;

        public bool Pressed { get; set; }

        #region Sound

        public SoundIndex Sound
        {
            get => _Sound;
            set => _Sound = value;// SetProp(ref _Sound, value, OnSoundChanged);
        }

        private SoundIndex _Sound;

        public event EventHandler<EventArgs> SoundChanged;

        public virtual void OnSoundChanged(SoundIndex oValue, SoundIndex nValue)
        {
            SoundChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Sound

        public DXButton()
        {
            Border = false;
        }

        private void UpdateColor()
        {
            // 按下.
            if (BtnStatus.HasFlag(ButtonStatus.Pressed) || Pressed)
            {
                ForeColour = PressColor;
            }
            else
            {
                // 未按下.
                if (BtnStatus.HasFlag(ButtonStatus.Hover))
                    ForeColour = HoverColor;
                else
                    ForeColour = NormalColor;
            }
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            BtnStatus |= ButtonStatus.Hover;

            UpdateColor();
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            BtnStatus &= ~ButtonStatus.Hover;

            UpdateColor();
        }

        public override void OnMouseDown(MouseEvent e)
        {
            base.OnMouseDown(e);

            if (Sound > 0)
                AudioManager.PlayEffect(Sound);

            BtnStatus |= ButtonStatus.Pressed;

            UpdateColor();
        }

        public override void OnMouseUp(MouseEvent e)
        {
            base.OnMouseUp(e);

            BtnStatus &= ~ButtonStatus.Pressed;

            UpdateColor();
        }

        public override void OnMouseDClick(MouseEvent e)
        {
            base.OnMouseDClick(e);

            if (Sound > 0)
                AudioManager.PlayEffect(Sound);
        }
    }

    public class DXTextButton : DXButton
    {
        private DXNineImage _backImage;
        public DXLabel Label { get; private set; }

        public int Index
        {
            get => _backImage.Index;
            set => _backImage.Index = value;
        }

        public LibraryFile LibraryFile
        {
            get => _backImage.LibraryFile;
            set => _backImage.LibraryFile = value;
        }

        public DXTextButton()
        {
            Sound = SoundIndex.ButtonA;

            _backImage = new DXNineImage()
            {
                Parent = this,
                IsControl = false,
                SprLib = InterfaceLibrary,
                Index = 116,
            };

            Label = new DXLabel()
            {
                Parent = this,
                Text = "按钮",
                IsControl = false,
                AutoSize = true,
                //Align = UIAlign.MiddleCenter,
            };
        }

        public override void OnForeColourChanged(Color oValue, Color nValue)
        {
            base.OnForeColourChanged(oValue, nValue);
            if (_backImage != null)
            {
                _backImage.ForeColour = nValue;
            }
        }

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            _backImage.Size = nValue;
        }

        public override void Process()
        {
            // 按下.
            if (BtnStatus.HasFlag(ButtonStatus.Pressed) || Pressed)
            {
                ForeColour = PressColor;
            }
            else
            {
                // 未按下.
                if (BtnStatus.HasFlag(ButtonStatus.Hover))
                    ForeColour = HoverColor;
                else
                    ForeColour = NormalColor;
            }
        }

        //protected override void OnDisposeManaged()
        //{
        //    base.OnDisposeManaged();

        //    if (_backImage != null)
        //    {
        //        _backImage.Dispose();
        //        _backImage = null;
        //    }

        //    if (Label != null)
        //    {
        //        Label.Dispose();
        //        Label = null;
        //    }
        //}
    }

    public class DXImageButton : DXButton
    {
        private DXImageControl _backImage;

        public string _ImagePath = "";

        public int Index
        {
            get => _backImage.Index;
            set => _backImage.Index = value;
        }

        public LibraryFile LibraryFile
        {
            get => _backImage.LibraryFile;
            set => _backImage.LibraryFile = value;
        }

        public DXImageButton()
        {
            Sound = SoundIndex.ButtonA;

            //Border = true;
            //BorderColour = Color.Red;

            _backImage = new DXImageControl()
            {
                Parent = this,
                IsControl = false,
            };

            _backImage.SizeChanged += (o, e) =>
            {
                Size = _backImage.Size;
            };
        }

        public override void OnForeColourChanged(Color oValue, Color nValue)
        {
            base.OnForeColourChanged(oValue, nValue);
            if (_backImage != null)
            {
                _backImage.ForeColour = nValue;
            }
        }

        public override void Process()
        {
            // 按下.
            if (BtnStatus.HasFlag(ButtonStatus.Pressed) || Pressed)
            {
                _backImage.ForeColour = PressColor;
            }
            else
            {
                // 未按下.
                if (BtnStatus.HasFlag(ButtonStatus.Hover))
                    ForeColour = HoverColor;
                else
                    ForeColour = NormalColor;
            }
        }

        //protected override void OnDisposeManaged()
        //{
        //    base.OnDisposeManaged();

        //    if (_backImage != null)
        //    {
        //        _backImage.Dispose();
        //        _backImage = null;
        //    }
        //}
    }
}