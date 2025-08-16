using Client.Controls;
using Client.Envir;
using Client.UserModels;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Drawing;
using System.Windows.Forms;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class ChatTextBox : DXWindow
    {
        #region Properties

        #region Mode

        public ChatMode Mode
        {
            get { return _Mode; }
            set
            {
                if (_Mode == value) return;

                ChatMode oldValue = _Mode;
                _Mode = value;

                OnModeChanged(oldValue, value);
            }
        }

        private ChatMode _Mode;

        public event EventHandler<EventArgs> ModeChanged;

        public void OnModeChanged(ChatMode oValue, ChatMode nValue)
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);

            if (ChatModeButton != null)
                ChatModeButton.Label.Text = Mode.ToString();
        }

        #endregion

        public string LastPM;

        public DXTextBox TextBox;
        public DXTextButton OptionsButton;
        public DXTextButton ChatModeButton;

        public override void OnParentChanged(DXControl oValue, DXControl nValue)
        {
            base.OnParentChanged(oValue, nValue);

            if (GameScene.Game.MainPanel == null) return;

            SetDefaultLocation();
        }

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            if (TextBox == null || ChatModeButton == null || OptionsButton == null) return;

            ChatModeButton.Location = new Point(ClientArea.Location.X, ClientArea.Y - 1);
            TextBox.Size = new Size(ClientArea.Width - ChatModeButton.Size.Width - 10 - OptionsButton.Size.Width, 100);
            TextBox.Location = new Point(ClientArea.Location.X + ChatModeButton.Size.Width + 5, ClientArea.Y);
            OptionsButton.Location = new Point(ClientArea.Location.X + TextBox.Size.Width + ChatModeButton.Size.Width + 10, ClientArea.Y - 1);
        }

        public override WindowType Type => WindowType.ChatTextBox;
        public override bool CustomSize => true;
        public override bool AutomaticVisibility => true;

        #endregion

        public ChatTextBox()
        {
            Size = new Size(400, 25);

            Opacity = 0.6F;

            HasTitle = false;
            HasFooter = false;
            HasTopBorder = false;
            CloseButton.Visible = false;

            AllowResize = true;
            CanResizeHeight = false;

            ChatModeButton = new DXTextButton
            {
                Index = 118,
                Size = new Size(60, SmallButtonHeight),
                Label = { Text = Mode.ToString() },
                Parent = this,
            };
            ChatModeButton.MouseClick += (o, e) => Mode = (ChatMode)(((int)(Mode) + 1) % 7);

            OptionsButton = new DXTextButton
            {
                Index = 118,
                Size = new Size(50, SmallButtonHeight),
                Label = { Text = CEnvir.Language.ChatTextBoxOptionsButtonLabel },
                Parent = this,
            };
            OptionsButton.MouseClick += (o, e) =>
            {
                GameScene.Game.ChatOptionsBox.Visible = !GameScene.Game.ChatOptionsBox.Visible;
            };

            TextBox = new DXTextBox
            {
                Size = new Size(350, 100),
                Parent = this,
                MaxLength = Globals.MaxChatLength,
                Opacity = 0.35f,
            };
            TextBox.KeyPress += TextBox_KeyPress;
            //TextBox.KeyDown += TextBox_KeyDown;
            //TextBox.KeyUp += TextBox_KeyUp;

            SetDefaultSize();

            ChatModeButton.Location = new Point(ClientArea.Location.X, ClientArea.Y - 1);
            TextBox.Location = new Point(ClientArea.Location.X + ChatModeButton.Size.Width + 5, ClientArea.Y);
            OptionsButton.Location = new Point(ClientArea.Location.X + TextBox.Size.Width + ChatModeButton.Size.Width + 10, ClientArea.Y - 1);
        }

        public void SetDefaultSize()
        {
            SetClientSize(new Size(TextBox.Size.Width + ChatModeButton.Size.Width + 15 + OptionsButton.Size.Width, TextBox.Size.Height));
        }

        public void SetDefaultLocation()
        {
            Location = new Point(GameScene.Game.MainPanel.Location.X, (GameScene.Game.MainPanel.Location.Y - Size.Height));
        }

        #region Methods

        private void TextBox_KeyPress(object sender, KeyEvent e)
        {
            switch (e.KeyCode)
            {
                case KeyboardKey.Enter:
                    e.Handled = true;
                    if (!string.IsNullOrEmpty(TextBox.Text))
                    {
                        CEnvir.Enqueue(new C.Chat
                        {
                            Text = TextBox.Text,
                        });

                        if (TextBox.Text[0] == '/')
                        {
                            string[] parts = TextBox.Text.Split(' ');

                            if (parts.Length > 0) LastPM = parts[0];
                        }
                    }

                    DXTextBox.ActiveTextBox = null;
                    TextBox.Text = string.Empty;

                    ToggleVisibility(e, true);
                    break;

                case KeyboardKey.Escape:
                    e.Handled = true;
                    DXTextBox.ActiveTextBox = null;
                    TextBox.Text = string.Empty;

                    ToggleVisibility(e, false);
                    break;
            }
        }

        public override void OnKeyPress(KeyEvent e)
        {
            base.OnKeyPress(e);

            switch (e.Char)
            {
                case '@':
                    TextBox.SetFocus();
                    TextBox.Text = @"@";
                    TextBox.Visible = true;
                    TextBox.SelectionLength = 0;
                    TextBox.SelectionStart = TextBox.Text.Length;
                    e.Handled = true;
                    break;

                case '!':
                    if (!Config.ShiftOpenChat) return;
                    TextBox.SetFocus();
                    TextBox.Text = @"!";
                    TextBox.Visible = true;
                    TextBox.SelectionLength = 0;
                    TextBox.SelectionStart = TextBox.Text.Length;
                    e.Handled = true;
                    break;

                case ' ':
                case (char)KeyboardKey.Enter:
                    OpenChat();
                    e.Handled = true;
                    break;

                case '/':
                    TextBox.SetFocus();
                    if (string.IsNullOrEmpty(LastPM))
                        TextBox.Text = "/";
                    else
                        TextBox.Text = LastPM + " ";
                    TextBox.Visible = true;
                    TextBox.SelectionLength = 0;
                    TextBox.SelectionStart = TextBox.Text.Length;
                    e.Handled = true;
                    break;
            }
        }

        public void ToggleVisibility(KeyEvent e, bool hide)
        {
            if (Visible)
            {
                if (hide)
                {
                    if (ChatTab.Tabs.Count > 0 && ChatTab.Tabs[0].Panel.TransparentCheckBox.Checked == true)
                    {
                        Visible = false;
                    }
                }
            }
            else
            {
                if (!hide)
                {
                    Visible = true;

                    OnKeyPress(e);
                }
            }
        }

        public void OpenChat()
        {
            if (string.IsNullOrEmpty(TextBox.Text))
                switch (Mode)
                {
                    case ChatMode.Shout:
                        TextBox.Text = @"!";
                        break;

                    case ChatMode.Whisper:
                        if (!string.IsNullOrWhiteSpace(LastPM))
                            TextBox.Text = LastPM + " ";
                        break;

                    case ChatMode.Group:
                        TextBox.Text = @"!!";
                        break;

                    case ChatMode.Guild:
                        TextBox.Text = @"!~";
                        break;

                    case ChatMode.Global:
                        TextBox.Text = @"!@";
                        break;

                    case ChatMode.Observer:
                        TextBox.Text = @"#";
                        break;
                }

            TextBox.SetFocus();
            TextBox.SelectionLength = 0;
            TextBox.SelectionStart = TextBox.Text.Length;
        }

        public void StartPM(string name)
        {
            TextBox.Text = $"/{name} ";
            TextBox.SetFocus();
            TextBox.SelectionLength = 0;
            TextBox.SelectionStart = TextBox.Text.Length;
        }

        public void LinkItem(ClientUserItem item)
        {
            if (item == null) return;

            TextBox.Text += $"[{item.Info.ItemName}:{item.Index}]";
            TextBox.SetFocus();
            TextBox.SelectionLength = 0;
            TextBox.SelectionStart = TextBox.Text.Length;
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _Mode = 0;
                ModeChanged = null;

                LastPM = null;

                if (TextBox != null)
                {
                    if (!TextBox.IsDisposed)
                        TextBox.Dispose();

                    TextBox = null;
                }

                if (OptionsButton != null)
                {
                    if (!OptionsButton.IsDisposed)
                        OptionsButton.Dispose();

                    OptionsButton = null;
                }

                if (ChatModeButton != null)
                {
                    if (!ChatModeButton.IsDisposed)
                        ChatModeButton.Dispose();

                    ChatModeButton = null;
                }
            }
        }

        #endregion
    }

    public enum ChatMode
    {
        Local,
        Whisper,
        Group,
        Guild,
        Shout,
        Global,
        Observer,
    }

    public class Message
    {
        public string Text { get; set; }
        public DateTime ReceivedTime { get; set; }
        public MessageType Type { get; set; }
    }
}