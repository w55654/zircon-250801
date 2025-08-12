using Client.Envir;
using Client.UserModels;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Client.Controls
{
    public sealed class DXKeyBindWindow : DXWindow
    {
        private KeyBindTree BindTree;

        private DXButton SaveButton, CancelButton, DefaultButton;

        public override WindowType Type => WindowType.None;
        public override bool CustomSize => false;
        public override bool AutomaticVisibility => false;

        public override void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            base.OnIsVisibleChanged(oValue, nValue);

            Location = new Point((Config.GameSize.Width - Size.Width) / 2, (Config.GameSize.Height - Size.Height) / 2);

            BindTree.TreeList.Clear();

            if (!CEnvir.Loaded || !IsVisible) return;

            LoadList();
        }

        public DXKeyBindWindow()
        {
            HasFooter = true;
            TitleLabel.Text = CEnvir.Language.CommonControlDXKeyBindWindowTitle;
            Modal = true;
            CloseButton.Visible = false;

            SetClientSize(new Size(430, 330));

            DXLabel label = new DXLabel
            {
                Parent = this,
                Text = CEnvir.Language.CommonControlDXKeyBindWindowTipLabel,
            };
            label.Location = new Point(ClientArea.Right - label.Size.Width, ClientArea.Y);

            BindTree = new KeyBindTree
            {
                Parent = this,
                Location = new Point(ClientArea.X, ClientArea.Y + 20),
                Size = new Size(ClientArea.Width, ClientArea.Height - 20)
            };

            SaveButton = new DXButton
            {
                Location = new Point(Size.Width - 190, Size.Height - 43),
                Size = new Size(80, DefaultHeight),
                Parent = this,
                Label = { Text = CEnvir.Language.CommonControlApply }
            };
            SaveButton.MouseClick += SaveButton_MouseClick;

            CancelButton = new DXButton
            {
                Location = new Point(Size.Width - 100, Size.Height - 43),
                Size = new Size(80, DefaultHeight),
                Parent = this,
                Label = { Text = CEnvir.Language.CommonControlClose }
            };
            CancelButton.MouseClick += (o, e) => Visible = false;

            DefaultButton = new DXButton
            {
                Location = new Point(ClientArea.X, Size.Height - 43),
                Size = new Size(80, DefaultHeight),
                Parent = this,
                Label = { Text = CEnvir.Language.CommonControlDefaults }
            };
            DefaultButton.MouseClick += (o, e) =>
            {
                DXMessageBox box = new DXMessageBox(CEnvir.Language.CommonControlDXKeyBindWindowDefaultConfirmMessage, CEnvir.Language.CommonControlDXKeyBindWindowDefaultConfirmCaption, DXMessageBoxButtons.YesNo);

                box.YesButton.MouseClick += (o1, e1) =>
                {
                    CEnvir.ResetKeyBinds();
                    LoadList();
                };
            };
        }

        private void SaveButton_MouseClick(object sender, MouseEvent e)
        {
            foreach (KeyValuePair<string, List<TempBindInfo>> pair in BindTree.TreeList)
            {
                foreach (TempBindInfo bind in pair.Value)
                    bind.Update();
            }
        }

        public override void OnKeyDown(KeyEvent e)
        {
            e.Handled = true;
            base.OnKeyDown(e);

            if (BindTree.SelectedEntry == null) return;

            bool modifiers = true;

            switch (e.KeyCode)
            {
                //case KeyboardKey.ControlKey:
                //case KeyboardKey.Shift:
                //case KeyboardKey.Alt:
                //case KeyboardKey.LShiftKey:
                //case KeyboardKey.RShiftKey:
                //case KeyboardKey.LControlKey:
                //case KeyboardKey.RControlKey:
                //case KeyboardKey.LMenu:
                //case KeyboardKey.RMenu:
                //case KeyboardKey.ShiftKey:
                case KeyboardKey.Menu:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Null;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Null;
                            break;
                    }

                    break;

                case KeyboardKey.Escape:
                    BindTree.SelectedEntry.KeyBindInfo.Load();

                    modifiers = false;
                    break;

                //case KeyboardKey.Control:
                //case KeyboardKey.KeyCode:
                //case KeyboardKey.Modifiers:
                //case KeyboardKey.None:
                //case KeyboardKey.LButton:
                //case KeyboardKey.RButton:
                //case KeyboardKey.Cancel:
                //case KeyboardKey.MButton:
                //case KeyboardKey.XButton1:
                //case KeyboardKey.XButton2:
                //case KeyboardKey.LineFeed:
                //case KeyboardKey.Clear:
                //case KeyboardKey.Return:
                //case KeyboardKey.KanaMode:
                //case KeyboardKey.JunjaMode:
                //case KeyboardKey.FinalMode:
                //case KeyboardKey.HanjaMode:
                //case KeyboardKey.IMEConvert:
                //case KeyboardKey.IMENonconvert:
                //case KeyboardKey.IMEAccept:
                //case KeyboardKey.IMEModeChange:
                //case KeyboardKey.Select:
                //case KeyboardKey.Print:
                //case KeyboardKey.Execute:
                //case KeyboardKey.Snapshot:
                //case KeyboardKey.Help:
                //case KeyboardKey.LWin:
                //case KeyboardKey.RWin:
                //case KeyboardKey.Apps:
                //case KeyboardKey.Sleep:
                //case KeyboardKey.BrowserBack:
                //case KeyboardKey.BrowserForward:
                //case KeyboardKey.BrowserRefresh:
                //case KeyboardKey.BrowserStop:
                //case KeyboardKey.BrowserSearch:
                //case KeyboardKey.BrowserFavorites:
                //case KeyboardKey.BrowserHome:
                //case KeyboardKey.VolumeMute:
                //case KeyboardKey.VolumeDown:
                //case KeyboardKey.VolumeUp:
                //case KeyboardKey.MediaNextTrack:
                //case KeyboardKey.MediaPreviousTrack:
                //case KeyboardKey.MediaStop:
                //case KeyboardKey.MediaPlayPause:
                //case KeyboardKey.LaunchMail:
                //case KeyboardKey.SelectMedia:
                //case KeyboardKey.LaunchApplication1:
                //case KeyboardKey.LaunchApplication2:
                //case KeyboardKey.OemPipe:
                //case KeyboardKey.ProcessKey:
                //case KeyboardKey.Packet:
                //case KeyboardKey.Attn:
                //case KeyboardKey.Crsel:
                //case KeyboardKey.Exsel:
                //case KeyboardKey.EraseEof:
                //case KeyboardKey.Play:
                //case KeyboardKey.Zoom:
                //case KeyboardKey.NoName:
                //case KeyboardKey.Pa1:
                //case KeyboardKey.OemClear:
                //    modifiers = false;
                //    break;

                case KeyboardKey.Kp0:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp0;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp0;
                            break;
                    }

                    break;

                case KeyboardKey.Kp1:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp1;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp1;
                            break;
                    }
                    break;

                case KeyboardKey.Kp2:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp2;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp2;
                            break;
                    }
                    break;

                case KeyboardKey.Kp3:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp3;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp3;
                            break;
                    }
                    break;

                case KeyboardKey.Kp4:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp4;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp4;
                            break;
                    }
                    break;

                case KeyboardKey.Kp5:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp5;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp5;
                            break;
                    }
                    break;

                case KeyboardKey.Kp6:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp6;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp6;
                            break;
                    }
                    break;

                case KeyboardKey.Kp7:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp7;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp7;
                            break;
                    }
                    break;

                case KeyboardKey.Kp8:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp8;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp8;
                            break;
                    }
                    break;

                case KeyboardKey.Kp9:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = KeyboardKey.Kp9;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = KeyboardKey.Kp9;
                            break;
                    }
                    break;

                default:
                    switch (BindTree.KeyMode)
                    {
                        case 1:
                            BindTree.SelectedEntry.KeyBindInfo.Key1 = e.KeyCode;
                            break;

                        case 2:
                            BindTree.SelectedEntry.KeyBindInfo.Key2 = e.KeyCode;
                            break;
                    }
                    break;
            }

            if (modifiers)
            {
                switch (BindTree.KeyMode)
                {
                    case 1:
                        BindTree.SelectedEntry.KeyBindInfo.Control1 = CEnvir.Ctrl;
                        BindTree.SelectedEntry.KeyBindInfo.Shift1 = CEnvir.Shift;
                        BindTree.SelectedEntry.KeyBindInfo.Alt1 = CEnvir.Alt;
                        break;

                    case 2:
                        BindTree.SelectedEntry.KeyBindInfo.Control2 = CEnvir.Ctrl;
                        BindTree.SelectedEntry.KeyBindInfo.Shift2 = CEnvir.Shift;
                        BindTree.SelectedEntry.KeyBindInfo.Alt2 = CEnvir.Alt;
                        break;
                }
            }

            BindTree.SelectedEntry.RefreshKeyLabel();
        }

        public void LoadList()
        {
            BindTree.TreeList.Clear();

            foreach (KeyBindInfo bind in CEnvir.KeyBinds.Binding)
            {
                List<TempBindInfo> list;
                TempBindInfo tempBind = new TempBindInfo(bind);

                if (!BindTree.TreeList.TryGetValue(tempBind.Category, out list))
                    BindTree.TreeList[tempBind.Category] = list = new List<TempBindInfo>();

                list.Add(tempBind);
            }

            foreach (KeyValuePair<string, List<TempBindInfo>> pair in BindTree.TreeList)
                pair.Value.Sort((x1, x2) => String.Compare(x1.Action.ToString(), x2.Action.ToString(), StringComparison.Ordinal));

            BindTree.ListChanged();
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (BindTree != null)
                {
                    if (!BindTree.IsDisposed)
                        BindTree.Dispose();

                    BindTree = null;
                }

                if (SaveButton != null)
                {
                    if (!SaveButton.IsDisposed)
                        SaveButton.Dispose();

                    SaveButton = null;
                }

                if (CancelButton != null)
                {
                    if (!CancelButton.IsDisposed)
                        CancelButton.Dispose();

                    CancelButton = null;
                }

                if (DefaultButton != null)
                {
                    if (!DefaultButton.IsDisposed)
                        DefaultButton.Dispose();

                    DefaultButton = null;
                }
            }
        }

        #endregion
    }

    public class KeyBindTree : DXControl
    {
        #region Properties

        #region SelectedEntry

        public KeyBindTreeEntry SelectedEntry
        {
            get => _SelectedEntry;
            set
            {
                KeyBindTreeEntry oldValue = _SelectedEntry;
                _SelectedEntry = value;

                OnSelectedEntryChanged(oldValue, value);
            }
        }

        private KeyBindTreeEntry _SelectedEntry;

        public event EventHandler<EventArgs> SelectedEntryChanged;

        public virtual void OnSelectedEntryChanged(KeyBindTreeEntry oValue, KeyBindTreeEntry nValue)
        {
            SelectedEntryChanged?.Invoke(this, EventArgs.Empty);

            if (oValue != null)
                oValue.Selected = false;

            if (nValue != null)
                nValue.Selected = true;
        }

        #endregion

        public static Dictionary<string, bool> ExpandedInfo = new Dictionary<string, bool>();
        public Dictionary<string, List<TempBindInfo>> TreeList = new Dictionary<string, List<TempBindInfo>>();

        private DXVScrollBar ScrollBar;

        public List<DXControl> Lines = new List<DXControl>();

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            ScrollBar.Size = new Size(14, Size.Height);
            ScrollBar.Location = new Point(Size.Width - 14, 0);
            ScrollBar.VisibleSize = Size.Height;
        }

        public int KeyMode = 0;

        #endregion

        public KeyBindTree()
        {
            Border = true;
            BorderColour = Color.FromArgb(198, 166, 99);

            ScrollBar = new DXVScrollBar
            {
                Parent = this,
                Change = 22,
            };
            ScrollBar.ValueChanged += (o, e) => UpdateScrollBar();

            MouseWheel += ScrollBar.DoMouseWheel;
        }

        #region Methods

        public void UpdateScrollBar()
        {
            ScrollBar.MaxValue = Lines.Count * 22;

            for (int i = 0; i < Lines.Count; i++)
                Lines[i].Location = new Point(Lines[i].Location.X, i * 22 - ScrollBar.Value);
        }

        public void ListChanged()
        {
            TempBindInfo selectedKeyBind = SelectedEntry?.KeyBindInfo;

            foreach (DXControl control in Lines)
                control.Dispose();

            Lines.Clear();

            _SelectedEntry = null;
            KeyBindTreeEntry firstEntry = null;

            foreach (KeyValuePair<string, List<TempBindInfo>> pair in TreeList)
            {
                KeyBindTreeHeader header = new KeyBindTreeHeader
                {
                    Parent = this,
                    Location = new Point(1, Lines.Count * 22),
                    Size = new Size(Size.Width - 17, 20),
                    HeaderLabel = { Text = pair.Key }
                };
                header.ExpandButton.MouseClick += (o, e) => ListChanged();
                header.MouseWheel += ScrollBar.DoMouseWheel;
                Lines.Add(header);

                bool expanded;

                if (!ExpandedInfo.TryGetValue(header.HeaderLabel.Text, out expanded))
                    ExpandedInfo[header.HeaderLabel.Text] = expanded = true;

                header.Expanded = expanded;

                if (!header.Expanded) continue;

                foreach (TempBindInfo KeyBind in pair.Value)
                {
                    KeyBindTreeEntry entry = new KeyBindTreeEntry
                    {
                        Parent = this,
                        Location = new Point(1, Lines.Count * 22),
                        Size = new Size(Size.Width - 17, 20),
                        KeyBindInfo = KeyBind,
                        Selected = KeyBind == selectedKeyBind,
                    };

                    entry.MouseWheel += ScrollBar.DoMouseWheel;

                    entry.KeyLabel1.MouseWheel += ScrollBar.DoMouseWheel;
                    entry.KeyLabel2.MouseWheel += ScrollBar.DoMouseWheel;

                    entry.KeyLabel1.MouseClick += (o, e) =>
                    {
                        KeyMode = 1;
                        SelectedEntry = entry;
                    };

                    entry.KeyLabel2.MouseClick += (o, e) =>
                    {
                        KeyMode = 2;
                        SelectedEntry = entry;
                    };

                    if (firstEntry == null)
                        firstEntry = entry;

                    if (entry.Selected)
                        SelectedEntry = entry;

                    Lines.Add(entry);
                }
            }

            UpdateScrollBar();
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                TreeList.Clear();
                TreeList = null;

                _SelectedEntry = null;
                SelectedEntryChanged = null;

                if (ScrollBar != null)
                {
                    if (!ScrollBar.IsDisposed)
                        ScrollBar.Dispose();

                    ScrollBar = null;
                }

                if (Lines != null)
                {
                    for (int i = 0; i < Lines.Count; i++)
                    {
                        if (Lines[i] != null)
                        {
                            if (!Lines[i].IsDisposed)
                                Lines[i].Dispose();

                            Lines[i] = null;
                        }
                    }

                    Lines.Clear();
                    Lines = null;
                }
            }
        }

        #endregion
    }

    public sealed class KeyBindTreeHeader : DXControl
    {
        #region Properties

        #region Expanded

        public bool Expanded
        {
            get => _Expanded;
            set
            {
                if (_Expanded == value) return;

                bool oldValue = _Expanded;
                _Expanded = value;

                OnExpandedChanged(oldValue, value);
            }
        }

        private bool _Expanded;

        public event EventHandler<EventArgs> ExpandedChanged;

        public void OnExpandedChanged(bool oValue, bool nValue)
        {
            ExpandButton.Index = Expanded ? 4871 : 4870;
            KeyBindTree.ExpandedInfo[HeaderLabel.Text] = Expanded;

            ExpandedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public DXButton ExpandButton;
        public DXLabel HeaderLabel;

        #endregion

        public KeyBindTreeHeader()
        {
            ExpandButton = new DXButton
            {
                Parent = this,
                LibraryFile = LibraryFile.GameInter,
                Index = 4870,
                Location = new Point(2, 2)
            };
            ExpandButton.MouseClick += (o, e) => Expanded = !Expanded;

            HeaderLabel = new DXLabel
            {
                Parent = this,
                ForeColour = Color.White,
                IsControl = false,
                Location = new Point(25, 2)
            };
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _Expanded = false;
                ExpandedChanged = null;

                if (ExpandButton != null)
                {
                    if (!ExpandButton.IsDisposed)
                        ExpandButton.Dispose();

                    ExpandButton = null;
                }

                if (HeaderLabel != null)
                {
                    if (!HeaderLabel.IsDisposed)
                        HeaderLabel.Dispose();

                    HeaderLabel = null;
                }
            }
        }

        #endregion
    }

    public sealed class KeyBindTreeEntry : DXControl
    {
        #region Properties

        #region Selected

        public bool Selected
        {
            get => _Selected;
            set
            {
                if (_Selected == value) return;

                bool oldValue = _Selected;
                _Selected = value;

                OnSelectedChanged(oldValue, value);
            }
        }

        private bool _Selected;

        public event EventHandler<EventArgs> SelectedChanged;

        public void OnSelectedChanged(bool oValue, bool nValue)
        {
            SelectedChanged?.Invoke(this, EventArgs.Empty);
            Border = Selected;

            if (!Selected)
            {
                if (KeyBindInfo.Key1 == KeyboardKey.Null)
                {
                    KeyBindInfo.Alt1 = false;
                    KeyBindInfo.Control1 = false;
                    KeyBindInfo.Shift1 = false;
                }

                if (KeyBindInfo.Key2 == KeyboardKey.Null)
                {
                    KeyBindInfo.Alt2 = false;
                    KeyBindInfo.Control2 = false;
                    KeyBindInfo.Shift2 = false;
                }
                RefreshKeyLabel();
            }

            BackColour = Selected ? Color.FromArgb(80, 80, 125) : Color.FromArgb(25, 20, 0);
        }

        #endregion

        #region  KeyBindInfo

        public TempBindInfo KeyBindInfo
        {
            get => _KeyBindInfo;
            set
            {
                TempBindInfo oldValue = _KeyBindInfo;
                _KeyBindInfo = value;

                OnKeyBindInfoChanged(oldValue, value);
            }
        }

        private TempBindInfo _KeyBindInfo;

        public event EventHandler<EventArgs> KeyBindInfoChanged;

        public void OnKeyBindInfoChanged(TempBindInfo oValue, TempBindInfo nValue)
        {
            KeyBindInfoChanged?.Invoke(this, EventArgs.Empty);

            Type type = KeyBindInfo.Action.GetType();

            MemberInfo[] infos = type.GetMember(KeyBindInfo.Action.ToString());

            DescriptionAttribute description = infos[0].GetCustomAttribute<DescriptionAttribute>();
            Actionlabel.Text = description?.Description ?? KeyBindInfo.Action.ToString();

            RefreshKeyLabel();
        }

        #endregion

        public DXLabel Actionlabel;
        public DXLabel KeyLabel1, KeyLabel2;

        #endregion

        public KeyBindTreeEntry()
        {
            DrawTexture = true;
            BackColour = Color.FromArgb(25, 20, 0);

            BorderColour = Color.FromArgb(198, 166, 99);

            Actionlabel = new DXLabel
            {
                Parent = this,
                Location = new Point(5, 2),
                IsControl = false,
                AutoSize = false,
                Size = new Size(135, 16),
                DrawFormat = TextFormatFlags.Right | TextFormatFlags.VerticalCenter,
            };

            KeyLabel1 = new DXLabel
            {
                Parent = this,
                Location = new Point(153, 2),
                AutoSize = false,
                Border = true,
                BackColour = Color.Black,
                BorderColour = Color.FromArgb(198, 166, 99),
                Size = new Size(125, 16),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                ForeColour = Color.White
            };
            KeyLabel1.MouseClick += (o, e) =>
            {
                KeyBindInfo.Control1 = false;
                KeyBindInfo.Alt1 = false;
                KeyBindInfo.Shift1 = false;
                KeyBindInfo.Key1 = KeyboardKey.Null;
                RefreshKeyLabel();
                InvokeMouseClick();
            };

            KeyLabel2 = new DXLabel
            {
                Parent = this,
                Location = new Point(283, 2),
                AutoSize = false,
                Border = true,
                BackColour = Color.Black,
                BorderColour = Color.FromArgb(198, 166, 99),
                Size = new Size(125, 16),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                ForeColour = Color.White
            };
            KeyLabel2.MouseClick += (o, e) =>
            {
                KeyBindInfo.Control2 = false;
                KeyBindInfo.Alt2 = false;
                KeyBindInfo.Shift2 = false;
                KeyBindInfo.Key2 = KeyboardKey.Null;
                RefreshKeyLabel();
                InvokeMouseClick();
            };
        }

        public void RefreshKeyLabel()
        {
            string text = "";
            if (KeyBindInfo.Control1)
                text += "Ctrl + ";

            if (KeyBindInfo.Alt1)
                text += "Alt + ";

            if (KeyBindInfo.Shift1)
                text += "Shift + ";

            text += CEnvir.GetText(KeyBindInfo.Key1);

            KeyLabel1.Text = text;

            text = "";
            if (KeyBindInfo.Control2)
                text += "Ctrl + ";

            if (KeyBindInfo.Alt2)
                text += "Alt + ";

            if (KeyBindInfo.Shift2)
                text += "Shift + ";

            text += CEnvir.GetText(KeyBindInfo.Key2);

            KeyLabel2.Text = text;
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _Selected = false;
                SelectedChanged = null;

                _KeyBindInfo = null;
                KeyBindInfoChanged = null;

                if (Actionlabel != null)
                {
                    if (!Actionlabel.IsDisposed)
                        Actionlabel.Dispose();

                    Actionlabel = null;
                }

                if (KeyLabel1 != null)
                {
                    if (!KeyLabel1.IsDisposed)
                        KeyLabel1.Dispose();

                    KeyLabel1 = null;
                }

                if (KeyLabel2 != null)
                {
                    if (!KeyLabel2.IsDisposed)
                        KeyLabel2.Dispose();

                    KeyLabel2 = null;
                }
            }
        }

        #endregion
    }

    public class TempBindInfo
    {
        public KeyBindInfo Bind;

        public string Category;

        public KeyBindAction Action;

        public bool Control1;
        public bool Alt1;
        public bool Shift1;
        public KeyboardKey Key1;

        public bool Control2;
        public bool Alt2;
        public bool Shift2;
        public KeyboardKey Key2;

        public TempBindInfo(KeyBindInfo bind)
        {
            Bind = bind;
            Category = Bind.Category ?? string.Empty;
            Action = Bind.Action;

            Load();
        }

        public void Load()
        {
            Control1 = Bind.Control1;
            Alt1 = Bind.Alt1;
            Shift1 = Bind.Shift1;
            Key1 = Bind.Key1;

            Control2 = Bind.Control2;
            Alt2 = Bind.Alt2;
            Shift2 = Bind.Shift2;
            Key2 = Bind.Key2;
        }

        public void Update()
        {
            Bind.Key1 = Key1;
            Bind.Control1 = Control1;
            Bind.Alt1 = Alt1;
            Bind.Shift1 = Shift1;

            Bind.Key2 = Key2;
            Bind.Control2 = Control2;
            Bind.Alt2 = Alt2;
            Bind.Shift2 = Shift2;
        }
    }
}