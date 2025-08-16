using Client.Envir;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

//Cleaned
namespace Client.Controls
{
    public class DXControl : IDisposable
    {
        public static readonly Color EnabledColor = Color.FromArgb(128, 128, 128);
        public static readonly Color DefForeColor = Color.FromArgb(198, 166, 99);
        public static readonly Color DisableColor = Color.FromArgb(50, 50, 0, 0);
        public static readonly Color DefGoldColor = Color.FromArgb(198, 166, 99); // 暗金色
        public static readonly Color DefBackColor = Color.FromArgb(25, 25, 25);
        public static readonly Color WinBackColor = Color.FromArgb(50, 40, 20);

        #region Static

        public static List<DXControl> MessageBoxList = new List<DXControl>();

        public static DXControl MouseControl
        {
            get => _MouseControl;
            set
            {
                if (_MouseControl == value) return;

                DXControl oldControl = _MouseControl;
                _MouseControl = value;

                oldControl?.OnMouseLeave();

                _MouseControl?.OnMouseEnter();
            }
        }

        private static DXControl _MouseControl;

        public static DXControl FocusControl
        {
            get => _FocusControl;
            set
            {
                if (_FocusControl == value) return;

                DXControl oldControl = _FocusControl;
                _FocusControl = value;
                oldControl?.OnLostFocus();

                _FocusControl?.OnFocus();

                DXTextBox control = value as DXTextBox;

                if (DXTextBox.ActiveTextBox != null && DXTextBox.ActiveTextBox.KeepFocus && control == null) return;

                DXTextBox.ActiveTextBox = value as DXTextBox;
            }
        }

        private static DXControl _FocusControl;

        public static DXScene ActiveScene
        {
            get => _ActiveScene;
            set
            {
                if (_ActiveScene == value) return;

                _ActiveScene = value;

                _ActiveScene?.CheckIsVisible();
            }
        }

        private static DXScene _ActiveScene;

        public static int DefaultHeight { get; } = 24;
        public static int TabHeight { get; } = 22;
        public static int HeaderBarSize { get; }
        public static int HeaderSize { get; }
        public static int FooterSize { get; }
        public static int NoFooterSize { get; }
        public static int SmallButtonHeight { get; }

        public static DXLabel DebugLabel, HintLabel, PingLabel;
        protected static MirLibrary InterfaceLibrary;

        static DXControl()
        {
            DebugLabel = new DXLabel
            {
                BackColour = Color.FromArgb(125, 50, 50, 50),
                Border = true,
                BorderColour = Color.Black,
                Location = new Point(5, 5),
                IsVisible = Config.DebugLabel,
                Outline = false,
                ForeColour = Color.White,
            };
            HintLabel = new DXLabel
            {
                BackColour = Color.FromArgb(255, 255, 255, 150),//Color.FromArgb(120, 0, 0, 0)
                Border = true,
                BorderColour = Color.Black,//Color.Yellow,
                IsVisible = true,
                Outline = false,
                ForeColour = Color.Black,//Color.Yellow
                //PaddingBottom = 2
            };
            PingLabel = new DXLabel
            {
                BackColour = Color.FromArgb(125, 50, 50, 50),
                Border = true,
                BorderColour = Color.Black,
                Location = new Point(5, 19),
                IsVisible = Config.DebugLabel,
                Outline = false,
                ForeColour = Color.White,
            };

            CEnvir.LibraryList.TryGetValue(LibraryFile.Interface, out InterfaceLibrary);

            if (InterfaceLibrary == null) return;

            SmallButtonHeight = InterfaceLibrary.GetSize(41).Height;

            HeaderBarSize = InterfaceLibrary.GetSize(0).Height;

            HeaderSize = HeaderBarSize;
            HeaderSize += InterfaceLibrary.GetSize(3).Height;

            NoFooterSize = InterfaceLibrary.GetSize(2).Height;

            FooterSize = HeaderBarSize;
            FooterSize += InterfaceLibrary.GetSize(2).Height;
            FooterSize += InterfaceLibrary.GetSize(10).Height;
        }

        #endregion

        #region Properties

        protected internal List<DXControl> Controls { get; private set; } = new List<DXControl>();

        #region AllowDragOut

        public bool AllowDragOut
        {
            get => _AllowDragOut;
            set
            {
                if (_AllowDragOut == value) return;

                bool oldValue = _AllowDragOut;
                _AllowDragOut = value;

                OnAllowDragOutChanged(oldValue, value);
            }
        }

        private bool _AllowDragOut;

        public event EventHandler<EventArgs> AllowDragOutChanged;

        public virtual void OnAllowDragOutChanged(bool oValue, bool nValue)
        {
            AllowDragOutChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region AllowResize

        public bool AllowResize
        {
            get => _AllowResize;
            set
            {
                if (_AllowResize == value) return;

                bool oldValue = _AllowResize;
                _AllowResize = value;

                OnAllowResizeChanged(oldValue, value);
            }
        }

        private bool _AllowResize;

        public event EventHandler<EventArgs> AllowResizeChanged;

        public virtual void OnAllowResizeChanged(bool oValue, bool nValue)
        {
            AllowResizeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region BackColour

        public Color BackColour
        {
            get => _BackColour;
            set
            {
                if (_BackColour == value) return;

                Color oldValue = _BackColour;
                _BackColour = value;

                OnBackColourChanged(oldValue, value);
            }
        }

        private Color _BackColour;

        public event EventHandler<EventArgs> BackColourChanged;

        public virtual void OnBackColourChanged(Color oValue, Color nValue)
        {
            TextureValid = false;
            BackColourChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Border

        public bool Border
        {
            get => _Border;
            set
            {
                if (_Border == value) return;

                bool oldValue = _Border;
                _Border = value;

                OnBorderChanged(oldValue, value);
            }
        }

        private bool _Border;

        public event EventHandler<EventArgs> BorderChanged;

        public virtual void OnBorderChanged(bool oValue, bool nValue)
        {
            BorderChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region BorderColour

        public Color BorderColour
        {
            get => _BorderColour;
            set
            {
                if (_BorderColour == value) return;

                Color oldValue = _BorderColour;
                _BorderColour = value;

                OnBorderColourChanged(oldValue, value);
            }
        }

        private Color _BorderColour;

        public event EventHandler<EventArgs> BorderColourChanged;

        public virtual void OnBorderColourChanged(Color oValue, Color nValue)
        {
            BorderColourChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region BorderSize

        public float BorderSize
        {
            get => _BorderSize;
            set
            {
                if (_BorderSize == value) return;

                float oldValue = _BorderSize;
                _BorderSize = value;

                OnBorderSizeChanged(oldValue, value);
            }
        }

        private float _BorderSize;

        public event EventHandler<EventArgs> BorderSizeChanged;

        public virtual void OnBorderSizeChanged(float oValue, float nValue)
        {
            BorderSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region CanResizeHeight

        public bool CanResizeHeight
        {
            get => _CanResizeHeight;
            set
            {
                if (_CanResizeHeight == value) return;

                bool oldValue = _CanResizeHeight;
                _CanResizeHeight = value;

                OnCanResizeHeightChanged(oldValue, value);
            }
        }

        private bool _CanResizeHeight;

        public event EventHandler<EventArgs> CanResizeHeightChanged;

        public virtual void OnCanResizeHeightChanged(bool oValue, bool nValue)
        {
            CanResizeHeightChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region CanResizeWidth

        public bool CanResizeWidth
        {
            get => _CanResizeWidth;
            set
            {
                if (_CanResizeWidth == value) return;

                bool oldValue = _CanResizeWidth;
                _CanResizeWidth = value;

                OnCanResizeWidthChanged(oldValue, value);
            }
        }

        private bool _CanResizeWidth;

        public event EventHandler<EventArgs> CanResizeWidthChanged;

        public virtual void OnCanResizeWidthChanged(bool oValue, bool nValue)
        {
            CanResizeWidthChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region DrawTexture

        public bool DrawTexture
        {
            get => _DrawTexture;
            set
            {
                if (_DrawTexture == value) return;

                bool oldValue = _DrawTexture;
                _DrawTexture = value;

                OnDrawTextureChanged(oldValue, value);
            }
        }

        private bool _DrawTexture;

        public event EventHandler<EventArgs> DrawTextureChanged;

        public virtual void OnDrawTextureChanged(bool oValue, bool nValue)
        {
            TextureValid = false;
            DrawTextureChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region DisplayArea

        public Rectangle DisplayArea
        {
            get => _DisplayArea;
            set
            {
                if (_DisplayArea == value) return;

                Rectangle oldValue = _DisplayArea;
                _DisplayArea = value;

                OnDisplayAreaChanged(oldValue, value);
            }
        }

        private Rectangle _DisplayArea;

        public event EventHandler<EventArgs> DisplayAreaChanged;

        public virtual void OnDisplayAreaChanged(Rectangle oValue, Rectangle nValue)
        {
            foreach (DXControl control in Controls)
                control.UpdateDisplayArea();

            DisplayAreaChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Enabled

        public bool Enabled
        {
            get => _Enabled;
            set
            {
                if (_Enabled == value) return;

                bool oldValue = _Enabled;
                _Enabled = value;

                OnEnabledChanged(oldValue, value);
            }
        }

        private bool _Enabled;

        public event EventHandler<EventArgs> EnabledChanged;

        public virtual void OnEnabledChanged(bool oValue, bool nValue)
        {
            CheckIsEnabled();
            EnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region ForeColour

        public Color ForeColour
        {
            get => _ForeColour;
            set
            {
                if (_ForeColour == value) return;

                Color oldValue = _ForeColour;
                _ForeColour = value;

                OnForeColourChanged(oldValue, value);
            }
        }

        private Color _ForeColour;

        public event EventHandler<EventArgs> ForeColourChanged;

        public virtual void OnForeColourChanged(Color oValue, Color nValue)
        {
            ForeColourChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Hint

        public string Hint
        {
            get => _Hint;
            set
            {
                if (_Hint == value) return;

                string oldValue = _Hint;
                _Hint = value;

                OnHintChanged(oldValue, value);
            }
        }

        private string _Hint;

        public event EventHandler<EventArgs> HintChanged;

        public virtual void OnHintChanged(string oValue, string nValue)
        {
            HintChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region HintPosition

        public HintPosition HintPosition
        {
            get => _HintPosition;
            set
            {
                if (_HintPosition == value) return;

                HintPosition oldValue = _HintPosition;
                _HintPosition = value;

                OnHintPositionChanged(oldValue, value);
            }
        }

        private HintPosition _HintPosition;

        public event EventHandler<EventArgs> HintPositionChanged;

        public virtual void OnHintPositionChanged(HintPosition oValue, HintPosition nValue)
        {
            HintPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IsControl

        public bool IsControl
        {
            get => _IsControl;
            set
            {
                if (_IsControl == value) return;

                bool oldValue = _IsControl;
                _IsControl = value;

                OnIsControlChanged(oldValue, value);
            }
        }

        private bool _IsControl;

        public event EventHandler<EventArgs> IsControlChanged;

        public virtual void OnIsControlChanged(bool oValue, bool nValue)
        {
            if (!IsControl)
            {
                if (FocusControl == this)
                    FocusControl = null;

                if (MouseControl == this)
                    MouseControl = null;
            }

            IsControlChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Location

        public Point Location
        {
            get => _Location;
            set
            {
                if (_Location == value) return;

                Point oldValue = _Location;
                _Location = value;

                OnLocationChanged(oldValue, value);
            }
        }

        private Point _Location;

        public event EventHandler<EventArgs> LocationChanged;

        public virtual void OnLocationChanged(Point oValue, Point nValue)
        {
            UpdateDisplayArea();
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Modal

        public bool Modal
        {
            get => _Modal;
            set
            {
                if (_Modal == value) return;

                bool oldValue = _Modal;
                _Modal = value;

                OnModalChanged(oldValue, value);
            }
        }

        private bool _Modal;

        public event EventHandler<EventArgs> ModalChanged;

        public virtual void OnModalChanged(bool oValue, bool nValue)
        {
            ModalChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Movable

        public bool Movable
        {
            get => _Movable;
            set
            {
                if (_Movable == value) return;

                bool oldValue = _Movable;
                _Movable = value;

                OnMovableChanged(oldValue, value);
            }
        }

        private bool _Movable;

        public event EventHandler<EventArgs> MovableChanged;

        public virtual void OnMovableChanged(bool oValue, bool nValue)
        {
            MovableChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IgnoreMoveBounds

        public bool IgnoreMoveBounds
        {
            get => _IgnoreMoveBounds;
            set
            {
                if (_IgnoreMoveBounds == value) return;

                bool oldValue = _IgnoreMoveBounds;
                _IgnoreMoveBounds = value;

                OnIgnoreMoveBoundsChanged(oldValue, value);
            }
        }

        private bool _IgnoreMoveBounds;

        public event EventHandler<EventArgs> IgnoreMoveBoundsChanged;

        public virtual void OnIgnoreMoveBoundsChanged(bool oValue, bool nValue)
        {
            IgnoreMoveBoundsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Clip

        public bool Clip
        {
            get => _Clip;
            set
            {
                if (_Clip == value) return;

                bool oldValue = _Clip;
                _Clip = value;

                OnClipChanged(oldValue, value);
            }
        }

        private bool _Clip;

        public event EventHandler<EventArgs> ClipChanged;

        public virtual void OnClipChanged(bool oValue, bool nValue)
        {
            ClipChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Opacity

        public float Opacity
        {
            get => _Opacity;
            set
            {
                if (_Opacity == value) return;

                float oldValue = _Opacity;
                _Opacity = value;

                OnOpacityChanged(oldValue, value);
            }
        }

        private float _Opacity;

        public event EventHandler<EventArgs> OpacityChanged;

        public virtual void OnOpacityChanged(float oValue, float nValue)
        {
            OpacityChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Parent

        public DXControl Parent
        {
            get => _Parent;
            set
            {
                if (_Parent == value) return;

                DXControl oldValue = _Parent;
                _Parent = value;

                OnParentChanged(oldValue, value);
            }
        }

        private DXControl _Parent;

        public event EventHandler<EventArgs> ParentChanged;

        public virtual void OnParentChanged(DXControl oValue, DXControl nValue)
        {
            oValue?.Controls.Remove(this);
            Parent?.Controls.Add(this);

            CheckIsVisible();
            CheckIsEnabled();

            UpdateDisplayArea();

            ParentChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region PassThrough

        public bool PassThrough
        {
            get => _PassThrough;
            set
            {
                if (_PassThrough == value) return;

                bool oldValue = _PassThrough;
                _PassThrough = value;

                OnPassThroughChanged(oldValue, value);
            }
        }

        private bool _PassThrough;

        public event EventHandler<EventArgs> PassThroughChanged;

        public virtual void OnPassThroughChanged(bool oValue, bool nValue)
        {
            PassThroughChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Size

        public virtual Size Size
        {
            get => _Size;
            set
            {
                if (_Size == value) return;

                Size oldValue = _Size;
                _Size = value;

                OnSizeChanged(oldValue, value);
            }
        }

        private Size _Size;

        public event EventHandler<EventArgs> SizeChanged;

        public virtual void OnSizeChanged(Size oValue, Size nValue)
        {
            UpdateDisplayArea();
            TextureValid = false;

            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Sort

        public bool Sort
        {
            get => _Sort;
            set
            {
                if (_Sort == value) return;

                bool oldValue = _Sort;
                _Sort = value;

                OnSortChanged(oldValue, value);
            }
        }

        private bool _Sort;

        public event EventHandler<EventArgs> SortChanged;

        public virtual void OnSortChanged(bool oValue, bool nValue)
        {
            BringToFront();
            SortChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Sound

        public SoundIndex Sound
        {
            get => _Sound;
            set
            {
                if (_Sound == value) return;

                SoundIndex oldValue = _Sound;
                _Sound = value;

                OnSoundChanged(oldValue, value);
            }
        }

        private SoundIndex _Sound;

        public event EventHandler<EventArgs> SoundChanged;

        public virtual void OnSoundChanged(SoundIndex oValue, SoundIndex nValue)
        {
            SoundChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Tag

        public object Tag
        {
            get => _Tag;
            set
            {
                if (_Tag == value) return;

                object oldValue = _Tag;
                _Tag = value;

                OnTagChanged(oldValue, value);
            }
        }

        private object _Tag;

        public event EventHandler<EventArgs> TagChanged;

        public virtual void OnTagChanged(object oValue, object nValue)
        {
            TagChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Text

        public string Text
        {
            get => _Text;
            set
            {
                if (_Text == value) return;

                string oldValue = _Text;
                _Text = value;

                OnTextChanged(oldValue, value);
            }
        }

        private string _Text = string.Empty;

        public event EventHandler<EventArgs> TextChanged;

        public virtual void OnTextChanged(string oValue, string nValue)
        {
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Visible

        public bool Visible
        {
            get => _Visible;
            set
            {
                if (_Visible == value) return;

                bool oldValue = _Visible;
                _Visible = value;

                OnVisibleChanged(oldValue, value);
            }
        }

        private bool _Visible;

        public event EventHandler<EventArgs> VisibleChanged;

        public virtual void OnVisibleChanged(bool oValue, bool nValue)
        {
            CheckIsVisible();

            VisibleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IsEnabled

        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (_IsEnabled == value) return;

                bool oldValue = _IsEnabled;
                _IsEnabled = value;

                OnIsEnabledChanged(oldValue, value);
            }
        }

        private bool _IsEnabled;

        public event EventHandler<EventArgs> IsEnabledChanged;

        public virtual void OnIsEnabledChanged(bool oValue, bool nValue)
        {
            foreach (DXControl control in Controls)
                control.CheckIsEnabled();

            IsEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IsVisible

        public bool IsVisible
        {
            get => _IsVisible;
            set
            {
                if (_IsVisible == value) return;

                bool oldValue = _IsVisible;
                _IsVisible = value;

                OnIsVisibleChanged(oldValue, value);
            }
        }

        private bool _IsVisible;

        public event EventHandler<EventArgs> IsVisibleChanged;

        public virtual void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            if (!IsVisible)
            {
                if (FocusControl == this)
                    FocusControl = null;

                if (MouseControl == this)
                    MouseControl = null;
            }

            List<DXControl> checks = new List<DXControl>(Controls);

            foreach (DXControl control in checks)
                control.CheckIsVisible();

            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IsMoving

        public bool IsMoving
        {
            get => _IsMoving;
            set
            {
                if (_IsMoving == value) return;

                bool oldValue = _IsMoving;
                _IsMoving = value;

                OnIsMovingChanged(oldValue, value);
            }
        }

        private bool _IsMoving;

        public event EventHandler<EventArgs> IsMovingChanged;

        public virtual void OnIsMovingChanged(bool oValue, bool nValue)
        {
            // todo w
            //if (IsMoving)
            //    CEnvir.Target.SuspendLayout();
            //else
            //    CEnvir.Target.ResumeLayout();

            IsMovingChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IsResizing

        public bool IsResizing
        {
            get => _IsResizing;
            set
            {
                if (_IsResizing == value) return;

                bool oldValue = _IsResizing;
                _IsResizing = value;

                OnIsResizingChanged(oldValue, value);
            }
        }

        private bool _IsResizing;

        public event EventHandler<EventArgs> IsResizingChanged;

        public virtual void OnIsResizingChanged(bool oValue, bool nValue)
        {
            IsResizingChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public const int ResizeBuffer = 9;
        protected internal Point MovePoint;
        private Point ResizePoint;
        public bool ResizeLeft, ResizeRight, ResizeUp, ResizeDown;

        #region Texture

        public bool TextureValid { get; set; }
        public RenderTexture2D ControlTexture { get; set; }
        public Size TextureSize { get; set; }
        public DateTime ExpireTime { get; protected set; }

        protected virtual void CreateTexture()
        {
            // 尺寸变了就重建离屏目标
            if (!ControlTexture.IsValid() || DisplayArea.Size != TextureSize)
            {
                DisposeTexture();                            // 你原有的回收逻辑，记得里头要卸载 _controlRT

                TextureSize = DisplayArea.Size;

                // 1) 建离屏渲染目标
                ControlTexture = Raylib.LoadRenderTexture(TextureSize.Width, TextureSize.Height);

                //DXManager.ControlList.Add(this);
            }

            // 在控件自己的 RTT 上作画
            Raylib.BeginTextureMode(ControlTexture);

            // 清背景（注意 BackColour 是 System.Drawing.Color）
            Raylib.ClearBackground(Raylib_cs.Color.Gray);

            // 你原来的钩子，通常在这里把控件自身的静态内容画进去
            OnClearTexture();

            Raylib.EndTextureMode();

            TextureValid = true;
            ExpireTime = CEnvir.Now + Config.CacheDuration;
        }

        protected virtual void OnClearTexture()
        {
        }

        public virtual void DisposeTexture()
        {
            if (ControlTexture.IsValid())
            {
                Raylib.UnloadRenderTexture(ControlTexture);
                ControlTexture = default;
            }

            TextureSize = Size.Empty;
            ExpireTime = DateTime.MinValue;

            TextureValid = false;

            //DXManager.ControlList.Remove(this);
        }

        #endregion

        public event EventHandler<EventArgs> MouseEnter, MouseLeave, Focus, LostFocus;

        public event EventHandler<MouseEvent> MouseDown, MouseUp, MouseMove, Moving, MouseClick, MouseDoubleClick, MouseWheel;

        public event EventHandler<KeyEvent> KeyDown, KeyUp;

        public event EventHandler<KeyEvent> KeyPress;

        public Action ProcessAction;

        #endregion

        public DXControl()
        {
            BackColour = Color.Empty;
            Enabled = true;
            IsControl = true;
            Opacity = 1F;
            BorderSize = 1;
            Visible = true;
            ForeColour = Color.White;
            CanResizeHeight = true;
            CanResizeWidth = true;
        }

        #region Methods

        public virtual void Process()
        {
            ProcessAction?.Invoke();

            foreach (DXControl control in Controls)
            {
                if (!control.IsVisible) continue;

                control.Process();
            }
        }

        protected internal virtual void CheckIsVisible()
        {
            IsVisible = Visible && Parent != null && Parent.IsVisible;
        }

        protected internal virtual void CheckIsEnabled()
        {
            IsEnabled = Enabled && (Parent == null || Parent.IsEnabled);
        }

        protected internal virtual void UpdateDisplayArea()
        {
            Rectangle area = new Rectangle(Location, Size);

            if (Parent != null)
                area.Offset(Parent.DisplayArea.Location);

            DisplayArea = area;
        }

        public virtual void ResolutionChanged()
        {
        }

        public virtual void OnSorted()
        {
        }

        public void BringToFront()
        {
            if (Parent == null) return;

            Parent.BringToFront();

            if (!Sort || Parent.Controls[Parent.Controls.Count - 1] == this) return;

            Parent.Controls.Remove(this);
            Parent.Controls.Add(this);

            OnSorted();
        }

        public void SendToBack()
        {
            if (Parent == null) return;

            Parent.SendToBack();

            if (!Sort || Parent.Controls[0] == this) return;

            Parent.Controls.Remove(this);
            Parent.Controls.Insert(0, this);
        }

        public void InvokeMouseClick()
        {
            if (!IsEnabled) return;

            MouseClick?.Invoke(this, default);
        }

        public virtual bool IsMouseOver(Point p)
        {
            if (!IsVisible || !IsControl) return false;

            if (!DisplayArea.Contains(p)) return Modal;

            if (!PassThrough) return true;

            if (AllowResize)
            {
                bool left = false, right = false, top = false, bottom = false;
                if (CanResizeWidth)
                {
                    if (p.X - DisplayArea.Left < ResizeBuffer)
                        left = true;
                    else if (DisplayArea.Right - p.X < ResizeBuffer)
                        right = true;
                }

                if (CanResizeHeight)
                {
                    if (p.Y - DisplayArea.Top < ResizeBuffer)
                        top = true;
                    else if (DisplayArea.Bottom - p.Y < ResizeBuffer)
                        bottom = true;
                }

                if (left || right || top || bottom) return true;
            }

            for (int i = Controls.Count - 1; i >= 0; i--)
                if (Controls[i].IsMouseOver(p))
                    return true;

            return false;
        }

        public virtual void OnMouseEnter()
        {
            if (!IsEnabled) return;

            MouseEnter?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMouseLeave()
        {
            if (!IsEnabled) return;

            MouseLeave?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMouseMove(MouseEvent e)
        {
            if (!IsEnabled)
            {
                MouseControl = this;
                return;
            }

            bool left = false, right = false, top = false, bottom = false;
            if (IsResizing)
            {
                Point tempPoint = new Point(e.Location.X - ResizePoint.X, e.Location.Y - ResizePoint.Y);

                if (Parent == null || tempPoint.IsEmpty) return;

                Point nLocation = Location;
                Size nSize = Size;

                if (ResizeUp)
                {
                    nLocation = new Point(nLocation.X, nLocation.Y + tempPoint.Y);

                    if (DisplayArea.Y + tempPoint.Y < 0) nLocation.Y += DisplayArea.Y - tempPoint.Y;

                    nSize = new Size(nSize.Width, nSize.Height - (nLocation.Y - Location.Y));

                    if (nSize.Height < ResizeBuffer * 2)
                    {
                        nLocation.Y = Location.Y + Size.Height - ResizeBuffer * 2;
                        nSize.Height = ResizeBuffer * 2;
                    }
                    ResizePoint = new Point(ResizePoint.X, ResizePoint.Y + (nLocation.Y - Location.Y));
                }
                else if (ResizeDown)
                {
                    nSize = new Size(nSize.Width, nSize.Height + tempPoint.Y);

                    if (nSize.Height + nLocation.Y >= ActiveScene.Size.Height)
                        nSize.Height = ActiveScene.Size.Height - nLocation.Y;

                    if (nSize.Height < ResizeBuffer * 2)
                        nSize.Height = ResizeBuffer * 2;

                    ResizePoint = new Point(ResizePoint.X, ResizePoint.Y + (nSize.Height - Size.Height));
                }

                if (ResizeLeft)
                {
                    nLocation = new Point(nLocation.X + tempPoint.X, nLocation.Y);

                    if (DisplayArea.X + tempPoint.X < 0) nLocation.X += DisplayArea.X - tempPoint.X;

                    nSize = new Size(nSize.Width - (nLocation.X - Location.X), nSize.Height);

                    if (nSize.Width < ResizeBuffer * 2)
                    {
                        nLocation.X = Location.X + Size.Width - ResizeBuffer * 2;
                        nSize.Width = ResizeBuffer * 2;
                    }

                    ResizePoint = new Point(ResizePoint.X + (nLocation.X - Location.X), ResizePoint.Y);
                }
                else if (ResizeRight)
                {
                    nSize = new Size(nSize.Width + tempPoint.X, nSize.Height);

                    if (nSize.Width + nLocation.X >= ActiveScene.Size.Width)
                        nSize.Width = ActiveScene.Size.Width - nLocation.X;

                    if (nSize.Width < ResizeBuffer * 2)
                        nSize.Width = ResizeBuffer * 2;
                    ResizePoint = new Point(ResizePoint.X + (nSize.Width - Size.Width), ResizePoint.Y);
                }

                Size oldSize = nSize;
                nSize = GetAcceptableResize(oldSize);

                if (ResizeUp)
                {
                    nLocation = new Point(nLocation.X, nLocation.Y - nSize.Height + oldSize.Height);
                    ResizePoint = new Point(ResizePoint.X, ResizePoint.Y - nSize.Height + oldSize.Height);
                }
                else if (ResizeDown)
                {
                    ResizePoint = new Point(ResizePoint.X, ResizePoint.Y + nSize.Height - oldSize.Height);
                }

                if (ResizeLeft)
                {
                    nLocation = new Point(nLocation.X - nSize.Width + oldSize.Width, nLocation.Y);
                    ResizePoint = new Point(ResizePoint.X - nSize.Width + oldSize.Width, ResizePoint.Y);
                }
                else if (ResizeRight)
                {
                    ResizePoint = new Point(ResizePoint.X + nSize.Width - oldSize.Width, ResizePoint.Y);
                }
                Location = nLocation;
                Size = nSize;
            }
            else if (AllowResize)
            {
                if (CanResizeWidth)
                {
                    if (e.Location.X - DisplayArea.Left < ResizeBuffer)
                        left = true;
                    else if (DisplayArea.Right - e.Location.X < ResizeBuffer)
                        right = true;
                }

                if (CanResizeHeight)
                {
                    if (e.Location.Y - DisplayArea.Top < ResizeBuffer)
                        top = true;
                    else if (DisplayArea.Bottom - e.Location.Y < ResizeBuffer)
                        bottom = true;
                }

                if (left)
                {
                    if (top)
                        Cursor.Current = Cursors.SizeNWSE;
                    else if (bottom)
                        Cursor.Current = Cursors.SizeNESW;
                    else
                        Cursor.Current = Cursors.SizeWE;
                }
                else if (right)
                {
                    if (top)
                        Cursor.Current = Cursors.SizeNESW;
                    else if (bottom)
                        Cursor.Current = Cursors.SizeNWSE;
                    else
                        Cursor.Current = Cursors.SizeWE;
                }
                else if (top || bottom)
                    Cursor.Current = Cursors.SizeNS;
            }

            if (IsMoving)
            {
                Point tempPoint = new Point(e.Location.X - MovePoint.X, e.Location.Y - MovePoint.Y);

                if (!AllowDragOut && !IgnoreMoveBounds)
                {
                    if (Parent == null) return;

                    if (tempPoint.X + DisplayArea.Width > Parent.DisplayArea.Width) tempPoint.X = Parent.DisplayArea.Width - DisplayArea.Width;
                    if (tempPoint.Y + DisplayArea.Height > Parent.DisplayArea.Height) tempPoint.Y = Parent.DisplayArea.Height - DisplayArea.Height;

                    if (tempPoint.X < 0) tempPoint.X = 0;
                    if (tempPoint.Y < 0) tempPoint.Y = 0;
                }

                if (Clip && IgnoreMoveBounds)
                {
                    if (Size.Width > Parent.Size.Width)
                    {
                        if (tempPoint.X > 0) tempPoint.X = 0;
                        else if (tempPoint.X + DisplayArea.Width < Parent.DisplayArea.Width) tempPoint.X = Parent.DisplayArea.Width - DisplayArea.Width;
                    }
                    else
                    {
                        tempPoint.X = Location.X;
                    }

                    if (Size.Height > Parent.Size.Height)
                    {
                        if (tempPoint.Y > 0) tempPoint.Y = 0;
                        else if (tempPoint.Y + DisplayArea.Height < Parent.DisplayArea.Height) tempPoint.Y = Parent.DisplayArea.Height - DisplayArea.Height;
                    }
                    else
                    {
                        tempPoint.Y = Location.Y;
                    }
                }

                if (Tag is Size)
                {
                    Size clipSize = (Size)Tag;
                    Point change = new Point(tempPoint.X - Location.X, tempPoint.Y - Location.Y);

                    if (DisplayArea.X + change.X < ActiveScene.Location.X) tempPoint.X -= DisplayArea.X + change.X - ActiveScene.Location.X;
                    if (DisplayArea.Y + change.Y < ActiveScene.Location.Y) tempPoint.Y -= DisplayArea.Y + change.Y - ActiveScene.Location.Y;

                    if (DisplayArea.X + clipSize.Width + change.X - ActiveScene.Location.X >= ActiveScene.DisplayArea.Width) tempPoint.X -= DisplayArea.X + clipSize.Width + change.X - ActiveScene.Location.X - ActiveScene.DisplayArea.Width;
                    if (DisplayArea.Y + clipSize.Height + change.Y - ActiveScene.Location.Y >= ActiveScene.DisplayArea.Height) tempPoint.Y -= DisplayArea.Y + clipSize.Height + change.Y - ActiveScene.Location.Y - ActiveScene.DisplayArea.Height;
                }

                Location = tempPoint;
                Moving?.Invoke(this, e);
            }

            if (!IsMoving && !IsResizing && !left && !right && !top && !bottom)
                for (int i = Controls.Count - 1; i >= 0; i--)
                    if (Controls[i].IsMouseOver(e.Location))
                    {
                        Controls[i].OnMouseMove(e);
                        return;
                    }

            MouseControl = this;

            MouseMove?.Invoke(this, e);
        }

        public virtual Size GetAcceptableResize(Size size)
        {
            return size;
        }

        public virtual void OnMouseDown(MouseEvent e)
        {
            if (!IsEnabled) return;

            FocusControl = this;

            BringToFront();

            if (AllowResize)
            {
                if (CanResizeWidth)
                {
                    if (e.Location.X - DisplayArea.Left < ResizeBuffer)
                        ResizeLeft = true;
                    else if (DisplayArea.Right - e.Location.X < ResizeBuffer)
                        ResizeRight = true;
                }

                if (CanResizeHeight)
                {
                    if (e.Location.Y - DisplayArea.Top < ResizeBuffer)
                        ResizeUp = true;
                    else if (DisplayArea.Bottom - e.Location.Y < ResizeBuffer)
                        ResizeDown = true;
                }

                IsResizing = ResizeLeft || ResizeRight || ResizeUp || ResizeDown;
                ResizePoint = new Point(e.Location.X, e.Location.Y);

                if (ResizeLeft)
                {
                    if (ResizeUp)
                        Cursor.Current = Cursors.SizeNWSE;
                    else if (ResizeDown)
                        Cursor.Current = Cursors.SizeNESW;
                    else
                        Cursor.Current = Cursors.SizeWE;
                }
                else if (ResizeRight)
                {
                    if (ResizeUp)
                        Cursor.Current = Cursors.SizeNESW;
                    else if (ResizeDown)
                        Cursor.Current = Cursors.SizeNWSE;
                    else
                        Cursor.Current = Cursors.SizeWE;
                }
                else if (ResizeUp || ResizeDown)
                    Cursor.Current = Cursors.SizeNS;
            }

            if (!IsResizing && Movable && e.Button.HasFlag(MouseButton.Left) && (!Modal || DisplayArea.Contains(e.Location)))
            {
                IsMoving = true;
                MovePoint = new Point(e.Location.X - Location.X, e.Location.Y - Location.Y);
                Parent.Controls.Remove(this);
                Parent.Controls.Add(this);
            }

            MouseDown?.Invoke(this, e);
        }

        public virtual void OnMouseUp(MouseEvent e)
        {
            if (!IsEnabled) return;

            FocusControl = null;

            MouseUp?.Invoke(this, e);
        }

        public virtual void OnMouseClick(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (Sound != SoundIndex.None)
                DXSoundManager.Play(Sound);

            MouseClick?.Invoke(this, e);
        }

        public virtual void OnMouseDClick(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (MouseDoubleClick != null)
            {
                if (Sound != SoundIndex.None)
                    DXSoundManager.Play(Sound);

                MouseDoubleClick?.Invoke(this, e);
            }
            else
                OnMouseClick(e);
        }

        public virtual void OnMouseWheel(MouseEvent e)
        {
            if (!IsEnabled) return;

            MouseWheel?.Invoke(this, e);
        }

        public virtual void OnFocus()
        {
            IsMoving = false;
            ResizePoint = Point.Empty;
            MovePoint = Point.Empty;

            Focus?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnLostFocus()
        {
            if (IsMoving)
            {
                IsMoving = false;
                MovePoint = Point.Empty;
            }

            if (IsResizing)
            {
                IsResizing = false;
                ResizeLeft = false;
                ResizeRight = false;
                ResizeUp = false;
                ResizeDown = false;
                ResizePoint = Point.Empty;
            }

            LostFocus?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnKeyPress(KeyEvent e)
        {
            if (!IsEnabled) return;

            if (Controls != null)
                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    if (!Controls[i].IsVisible) continue;

                    Controls[i].OnKeyPress(e);
                    if (e.Handled || Modal) return;
                }

            KeyPress?.Invoke(this, e);
        }

        public virtual void OnKeyDown(KeyEvent e)
        {
            if (!IsEnabled) return;

            if (Controls != null)
                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    if (!Controls[i].IsVisible) continue;

                    Controls[i].OnKeyDown(e);
                    if (e.Handled || Modal) return;
                }

            KeyDown?.Invoke(this, e);
        }

        public virtual void OnKeyUp(KeyEvent e)
        {
            if (!IsEnabled) return;

            if (Controls != null)
                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    if (!Controls[i].IsVisible) continue;

                    Controls[i].OnKeyUp(e);
                    if (e.Handled || Modal) return;
                }

            KeyUp?.Invoke(this, e);
        }

        #region Drawing

        public event EventHandler<EventArgs> BeforeDraw, AfterDraw, BeforeChildrenDraw;

        public virtual void Draw()
        {
            if (!IsVisible || DisplayArea.Width <= 0 || DisplayArea.Height <= 0)
                return;

            OnBeforeDraw();
            DrawControl();
            OnBeforeChildrenDraw();
            DrawChildControls();
            DrawBorder();
            OnAfterDraw();
        }

        protected virtual void OnBeforeDraw()
        {
            BeforeDraw?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBeforeChildrenDraw()
        {
            BeforeChildrenDraw?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAfterDraw()
        {
            AfterDraw?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DrawBorder()
        {
            //if (!Border || BorderInformation == null) return;

            //if (DXManager.Line.Width != BorderSize)
            //    DXManager.Line.Width = BorderSize;

            //Surface old = DXManager.CurrentSurface;
            //DXManager.SetSurface(DXManager.ScratchSurface);

            //DXManager.Device.Clear(ClearFlags.Target, 0, 0, 0);

            //DXManager.Line.Draw(BorderInformation, BorderColour);

            //DXManager.SetSurface(old);

            //PresentTexture(DXManager.ScratchTexture, Parent, Rectangle.Inflate(DisplayArea, 1, 1), Color.White, this);
        }

        protected virtual void DrawChildControls()
        {
            foreach (DXControl control in Controls)
            {
                control.Draw();
            }
        }

        protected virtual void DrawControl()
        {
            if (!DrawTexture) return;

            if (!TextureValid)
            {
                CreateTexture();

                if (!TextureValid) return;
            }

            PresentTexture(ControlTexture.Texture, Parent, DisplayArea, IsEnabled ? Color.White : Color.FromArgb(75, 75, 75), Opacity, this, true);

            ExpireTime = CEnvir.Now + Config.CacheDuration;
        }

        public void DrawClippedTexture(Texture2D texture, Color color, bool FlipY = false)
        {
            // 初始裁剪区域 = 场景可见范围 ∩ 控件显示区域
            Rectangle clipArea = Rectangle.Intersect(ActiveScene.DisplayArea, DisplayArea);
            if (clipArea.IsEmpty)
                return;

            // 沿父控件链逐层裁剪
            if (!IsMoving || !AllowDragOut)
            {
                DXControl p = Parent;
                while (p != null)
                {
                    if (p.IsMoving && p.AllowDragOut)
                    {
                        clipArea = Rectangle.Intersect(ActiveScene.DisplayArea, DisplayArea);
                        break;
                    }

                    clipArea = Rectangle.Intersect(clipArea, p.DisplayArea);
                    if (clipArea.IsEmpty) return;

                    if (p.DisplayArea.IntersectsWith(DisplayArea))
                    {
                        p = p.Parent;
                        continue;
                    }

                    return;
                }
            }

            // 将裁剪区域转换到贴图局部坐标
            clipArea.Offset(-DisplayArea.X, -DisplayArea.Y);

            // 最终绘制位置
            float dstX = DisplayArea.X + clipArea.X;
            float dstY = DisplayArea.Y + clipArea.Y;

            // 源矩形
            var srcRect = clipArea.ToRayRect();

            // 目标矩形
            var dstRect = new Raylib_cs.Rectangle
            {
                X = dstX,
                Y = dstY,
                Width = clipArea.Width,
                Height = FlipY ? -clipArea.Height : clipArea.Height
            };

            Raylib.DrawTexturePro(texture, srcRect, dstRect, Vector2.Zero, 0f, color.ToRayColor(Opacity));
        }

        public static void PresentTexture(RayTexture texture, DXControl parent, Rectangle displayArea, Color colour, float alpha, DXControl control)
        {
            PresentTexture(texture.Texture, parent, displayArea, colour, alpha, control, false);
        }

        public static void PresentTexture(Texture2D texture, DXControl parent, Rectangle displayArea, Color colour, float alpha, DXControl control, bool flipY)
        {
            // 初始裁剪区域 = 场景可见范围 ∩ 控件显示区域
            Rectangle clipArea = Rectangle.Intersect(ActiveScene.DisplayArea, displayArea);
            if (clipArea.IsEmpty) return;

            // 沿父控件链逐层裁剪（除非遇到“允许拖出”的正在移动控件）
            if (!control.IsMoving || !control.AllowDragOut)
            {
                while (parent != null)
                {
                    // 遇到“正在移动且允许拖出”的控件 → 只用场景范围裁剪
                    if (parent.IsMoving && parent.AllowDragOut)
                    {
                        clipArea = Rectangle.Intersect(ActiveScene.DisplayArea, displayArea);
                        break;
                    }

                    // 普通父控件继续裁剪
                    clipArea = Rectangle.Intersect(clipArea, parent.DisplayArea);
                    if (clipArea.IsEmpty) return;

                    if (parent.DisplayArea.IntersectsWith(displayArea))
                    {
                        parent = parent.Parent;
                        continue;
                    }

                    // 完全不相交直接退出
                    return;
                }
            }

            // 将裁剪区域转换到贴图局部坐标
            clipArea.Offset(-displayArea.X, -displayArea.Y);

            // 最终绘制位置
            float dstX = displayArea.X + clipArea.X;
            float dstY = displayArea.Y + clipArea.Y;

            // 源矩形（贴图中截取的部分）
            var srcRect = clipArea.ToRayRect();

            // 目标矩形，高度翻转则取负值
            var dstRect = new Raylib_cs.Rectangle
            {
                X = dstX,
                Y = dstY,
                Width = clipArea.Width,
                Height = flipY ? -clipArea.Height : clipArea.Height
            };

            Raylib.DrawTexturePro(texture, srcRect, dstRect, Vector2.Zero, 0f, colour.ToRayColor(alpha));
        }

        public static void PresentString(string text, DXControl parent, Rectangle displayArea, Color colour, float alpha, DXControl control, int fontSize)
        {
            // 初始裁剪区域 = 场景可见范围 ∩ 控件显示区域
            Rectangle clipArea = Rectangle.Intersect(ActiveScene.DisplayArea, displayArea);
            if (clipArea.IsEmpty) return;

            // 沿父控件链逐层裁剪（除非遇到“允许拖出”的正在移动控件）
            if (!control.IsMoving || !control.AllowDragOut)
            {
                while (parent != null)
                {
                    // 遇到“正在移动且允许拖出”的控件 → 只用场景范围裁剪
                    if (parent.IsMoving && parent.AllowDragOut)
                    {
                        clipArea = Rectangle.Intersect(ActiveScene.DisplayArea, displayArea);
                        break;
                    }

                    // 普通父控件继续裁剪
                    clipArea = Rectangle.Intersect(clipArea, parent.DisplayArea);
                    if (clipArea.IsEmpty) return;

                    if (parent.DisplayArea.IntersectsWith(displayArea))
                    {
                        parent = parent.Parent;
                        continue;
                    }

                    // 完全不相交直接退出
                    return;
                }
            }

            // 将裁剪区域转换到贴图局部坐标
            clipArea.Offset(-displayArea.X, -displayArea.Y);

            // 最终绘制位置
            int dstX = displayArea.X + clipArea.X;
            int dstY = displayArea.Y + clipArea.Y;

            RayFont.DrawText(fontSize, text, new Point(dstX, dstY), colour);

            //Raylib.DrawTexturePro(texture, srcRect, dstRect, Vector2.Zero, 0f, colour.ToRayColor(alpha));
        }

        #endregion

        #endregion

#if DEBUG

        public override string ToString()
        {
            var names = new List<string>();

            // 从自己开始，往上收集所有类型名
            var current = this;
            while (current != null)
            {
                names.Add(current.GetType().Name);
                current = current.Parent;
            }

            // 反转顺序，从最顶层父开始到自己
            names.Reverse();

            string fullName = string.Join(" -> ", names);

            return $"{fullName} {DisplayArea}";
        }

#endif

        #region IDisposable

        public event EventHandler Disposing;

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(!IsDisposed);
            GC.SuppressFinalize(this);
        }

        ~DXControl()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disposing?.Invoke(this, EventArgs.Empty);

                IsDisposed = true;
                Disposing = null;

                //Free Managed Resources
                if (Controls != null)
                {
                    while (Controls.Count > 0)
                        Controls[0].Dispose();

                    Controls = null;
                }

                DisposeTexture();

                _AllowDragOut = false;
                _AllowResize = false;
                _BackColour = Color.Empty;
                _Border = false;
                _BorderColour = Color.Empty;
                _BorderSize = 0;
                _CanResizeHeight = false;
                _CanResizeWidth = false;
                _DrawTexture = false;
                _DisplayArea = Rectangle.Empty;
                _Enabled = false;
                _ForeColour = Color.Empty;
                _Hint = null;
                _IsControl = false;
                _Location = Point.Empty;
                _Modal = false;
                _Movable = false;
                _IgnoreMoveBounds = false;
                _Opacity = 0F;
                _Parent?.Controls.Remove(this);
                _Parent = null;
                _PassThrough = false;
                _Size = Size.Empty;
                _Sort = false;
                _Sound = SoundIndex.None;
                _Tag = null;
                _Text = null;
                _Visible = false;

                _IsEnabled = false;
                _IsVisible = false;
                _IsMoving = false;
                _IsResizing = false;

                MovePoint = Point.Empty;
                ResizePoint = Point.Empty;
                ResizeLeft = false;
                ResizeRight = false;
                ResizeUp = false;
                ResizeDown = false;

                AllowDragOutChanged = null;
                AllowResizeChanged = null;
                BackColourChanged = null;
                BorderChanged = null;
                BorderColourChanged = null;
                BorderSizeChanged = null;
                CanResizeHeightChanged = null;
                CanResizeWidthChanged = null;
                DrawTextureChanged = null;
                DisplayAreaChanged = null;
                EnabledChanged = null;
                ForeColourChanged = null;
                HintChanged = null;
                IsControlChanged = null;
                LocationChanged = null;
                ModalChanged = null;
                MovableChanged = null;
                IgnoreMoveBoundsChanged = null;
                OpacityChanged = null;
                ParentChanged = null;
                PassThroughChanged = null;
                SizeChanged = null;
                SortChanged = null;
                SoundChanged = null;
                TextChanged = null;
                VisibleChanged = null;
                IsEnabledChanged = null;
                IsVisibleChanged = null;
                IsMovingChanged = null;
                IsResizingChanged = null;

                MouseEnter = null;
                MouseLeave = null;
                Focus = null;
                LostFocus = null;
                MouseDown = null;
                MouseUp = null;
                MouseMove = null;
                Moving = null;
                MouseClick = null;
                MouseDoubleClick = null;
                MouseWheel = null;
                KeyDown = null;
                KeyUp = null;
                KeyPress = null;

                BeforeDraw = null;
                BeforeChildrenDraw = null;
                AfterDraw = null;

                ProcessAction = null;
            }

            if (_MouseControl == this) _MouseControl = null;
            if (_FocusControl == this) _FocusControl = null;
            if (_ActiveScene == this) _ActiveScene = null;

            DisposeTexture();
        }

        #endregion
    }
}