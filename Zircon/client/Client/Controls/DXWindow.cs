﻿using Client.Envir;
using Client.UserModels;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Rectangle = System.Drawing.Rectangle;

//Cleaned
namespace Client.Controls
{
    public abstract class DXWindow : DXControl
    {
        #region Properties

        public static List<DXWindow> Windows = new List<DXWindow>();

        #region HasTopBorder

        public bool HasTopBorder
        {
            get => _HasTopBorder;
            set
            {
                if (_HasTopBorder == value) return;

                bool oldValue = _HasTopBorder;
                _HasTopBorder = value;

                OnHasTopBorderChanged(oldValue, value);
            }
        }

        private bool _HasTopBorder;

        public event EventHandler<EventArgs> HasTopBorderChanged;

        public virtual void OnHasTopBorderChanged(bool oValue, bool nValue)
        {
            HasTopBorderChanged?.Invoke(this, EventArgs.Empty);

            UpdateClientArea();
        }

        #endregion

        #region HasTitle

        public bool HasTitle
        {
            get => _HasTitle;
            set
            {
                if (_HasTitle == value) return;

                bool oldValue = _HasTitle;
                _HasTitle = value;

                OnHasTitleChanged(oldValue, value);
            }
        }

        private bool _HasTitle;

        public event EventHandler<EventArgs> HasTitleChanged;

        public virtual void OnHasTitleChanged(bool oValue, bool nValue)
        {
            HasTitleChanged?.Invoke(this, EventArgs.Empty);

            UpdateClientArea();
            if (TitleLabel == null) return;
            TitleLabel.Visible = HasTitle;
        }

        #endregion

        #region HasFooter

        public bool HasFooter
        {
            get => _HasFooter;
            set
            {
                if (_HasFooter == value) return;

                bool oldValue = _HasFooter;
                _HasFooter = value;

                OnHasFooterChanged(oldValue, value);
            }
        }

        private bool _HasFooter;

        public event EventHandler<EventArgs> HasFooterChanged;

        public virtual void OnHasFooterChanged(bool oValue, bool nValue)
        {
            HasFooterChanged?.Invoke(this, EventArgs.Empty);

            UpdateClientArea();
        }

        #endregion

        #region ClientArea

        public Rectangle ClientArea
        {
            get => _ClientArea;
            set
            {
                if (_ClientArea == value) return;

                Rectangle oldValue = _ClientArea;
                _ClientArea = value;

                OnClientAreaChanged(oldValue, value);
            }
        }

        private Rectangle _ClientArea;

        public event EventHandler<EventArgs> ClientAreaChanged;

        public virtual void OnClientAreaChanged(Rectangle oValue, Rectangle nValue)
        {
            ClientAreaChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public abstract WindowType Type { get; }
        public abstract bool CustomSize { get; }
        public abstract bool AutomaticVisibility { get; }

        public DXImageButton CloseButton { get; protected set; }
        public DXLabel TitleLabel { get; protected set; }

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            _BackImage.Size = Size;

            UpdateClientArea();
            UpdateLocations();

            if (Settings != null && IsResizing)
            {
                Settings.Size = nValue;
                Settings.Location = Location;
            }
        }

        public override void OnParentChanged(DXControl oValue, DXControl nValue)
        {
            base.OnParentChanged(oValue, nValue);

            if (Parent == null) return;

            UpdateClientArea();

            UpdateLocations();
        }

        public override void OnLocationChanged(Point oValue, Point nValue)
        {
            base.OnLocationChanged(oValue, nValue);

            if (Settings != null && IsMoving)
                Settings.Location = nValue;
        }

        public override void OnVisibleChanged(bool oValue, bool nValue)
        {
            base.OnVisibleChanged(oValue, nValue);

            if (IsVisible)
                BringToFront();

            if (Settings != null && AutomaticVisibility)
                Settings.Visible = nValue;
        }

        public WindowSetting Settings;

        #endregion

        private DXNineImage _BackImage;

        protected DXWindow()
        {
            Windows.Add(this);

            BackColour = Color.FromArgb(16, 8, 8);
            HasTitle = true;
            Movable = true;
            HasTopBorder = true;
            Sort = true;

            _BackImage = new DXNineImage()
            {
                Parent = this,
                LibraryFile = LibraryFile.Interface,
                Index = 126,
                IsControl = false,
            };

            CloseButton = new DXImageButton
            {
                Parent = this,
                Index = 15,
                LibraryFile = LibraryFile.Interface,
                Hint = CEnvir.Language.CommonControlClose,
                HintPosition = HintPosition.TopLeft,
                Visible = true,
            };
            CloseButton.MouseClick += (o, e) => Visible = false;

            TitleLabel = new DXLabel
            {
                Text = "Window",
                Parent = this,
                // wh Font = new Font(Config.FontName, CEnvir.FontSize(10F), FontStyle.Bold),
                ForeColour = Color.FromArgb(198, 166, 99),
                Outline = true,
                OutlineColour = Color.Black,
                Visible = HasTitle,
                IsControl = false,
            };
            TitleLabel.SizeChanged += (o, e) => TitleLabel.Location = new Point((Size.Width - TitleLabel.Size.Width) / 2, 8);
        }

        #region Methods

        public override void ResolutionChanged()
        {
            Settings = null;

            base.ResolutionChanged();
        }

        private void UpdateLocations()
        {
            if (CloseButton != null)
                CloseButton.Location = new Point(DisplayArea.Width - CloseButton.Size.Width - 3, 3);

            if (TitleLabel != null)
                TitleLabel.Location = new Point((DisplayArea.Width - TitleLabel.Size.Width) / 2, 8);
        }

        public override void OnKeyDown(KeyEvent e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case KeyboardKey.Escape:
                    if (CloseButton.Visible)
                    {
                        CloseButton.InvokeMouseClick();
                        if (!Config.EscapeCloseAll)
                            e.Handled = true;
                    }
                    break;
            }
        }

        public void UpdateClientArea()
        {
            ClientArea = GetClientArea(Size);
        }

        public void SetClientSize(Size clientSize)
        {
            Size = GetSize(clientSize);
        }

        public Size GetSize(Size clientSize)
        {
            int w = 3 + 6 + 6 + 3; //Border Padding Padding Border
            int h = 6 + 6; //Padding Padding

            if (!HasTopBorder)
                h += NoFooterSize;
            else if (HasTitle)
                h += HeaderSize;
            else
                h += HeaderBarSize;

            if (!HasFooter)
                h += NoFooterSize;
            else
                h += FooterSize;

            return new Size(clientSize.Width + w, clientSize.Height + h);
        }

        public Rectangle GetClientArea(Size size)
        {
            int x = 6 + 3;
            int y = 6;

            if (!HasTopBorder)
                y += NoFooterSize;
            else if (HasTitle)
                y += HeaderSize;
            else
                y += HeaderBarSize;

            int w = size.Width - x * 2;
            int h = size.Height - y - 6;

            if (!HasFooter)
                h -= NoFooterSize;
            else
                h -= FooterSize;

            return new Rectangle(x, y, w, h);
        }

        public override void Draw()
        {
            if (!IsVisible || Size.Width == 0 || Size.Height == 0)
                return;

            OnBeforeDraw();
            DrawControl();
            OnBeforeChildrenDraw();
            DrawChildControls();
            DrawBorder();
            OnAfterDraw();
        }

        protected override void DrawControl()
        { }

        public void LoadSettings()
        {
            if (Type == WindowType.None || !CEnvir.Loaded) return;

            Settings = CEnvir.WindowSettings.Binding.FirstOrDefault(x => x.Resolution == Config.GameSize && x.Window == Type);

            if (Settings != null)
            {
                ApplySettings();
                return;
            }

            UpdateSettings();
        }

        public void UpdateSettings()
        {
            Settings ??= CEnvir.WindowSettings.CreateNewObject();

            Settings.Resolution = Config.GameSize;
            Settings.Window = Type;
            Settings.Size = Size;
            Settings.Visible = Visible;
            Settings.Location = Location;
        }

        public virtual void ApplySettings()
        {
            if (Settings == null) return;

            Location = Settings.Location;

            if (AutomaticVisibility)
                Visible = Settings.Visible;

            if (CustomSize)
                Size = Settings.Size;
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _HasTopBorder = false;
                _HasTitle = false;
                _HasFooter = false;
                _ClientArea = Rectangle.Empty;

                if (CloseButton != null)
                {
                    if (!CloseButton.IsDisposed)
                        CloseButton.Dispose();
                    CloseButton = null;
                }

                if (TitleLabel != null)
                {
                    if (!TitleLabel.IsDisposed)
                        TitleLabel.Dispose();
                    TitleLabel = null;
                }

                HasTopBorderChanged = null;
                HasTitleChanged = null;
                HasFooterChanged = null;
                ClientAreaChanged = null;

                Settings = null;
                Windows.Remove(this);
            }
        }

        #endregion
    }
}