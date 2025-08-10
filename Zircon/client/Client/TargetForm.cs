// TargetForm.cs — raylib-cs 7.0.1 版输入兼容层
// 目的：保留类与函数名/签名，把原本 WinForms 的事件改为“每帧轮询并转发”。
// 注意：不再继承 SlimDX.Windows.RenderForm。窗口由 raylib 创建（DXManager.Create）。

using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Library;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Client
{
    public sealed class TargetForm /* : RenderForm (已弃用) */
    {
        // ====== 与原文件保持的外观 ======
        public bool Resizing { get; private set; } // 保留占位

        // 注意：原构造里设置了 WinForms 尺寸/图标/边框，这些现在都由 raylib 管。
        public TargetForm() /* : base(Globals.ClientName) */
        {
            // no-op：窗口在 DXManager.Create() 里初始化
        }

        // ========== 轮询输入并转发到原来的 Scene 事件 ==========
        // 使用位置：在你的主循环里，每帧调用一次 PumpInput();
        public void PumpInput()
        {
            if (DXControl.ActiveScene == null) return;

            // 1) 鼠标位置与裁剪（替代 IsWindowHovered：用窗口焦点 + 坐标在窗口范围）
            bool windowActive = IsWindowFocused();
            var mp = GetMousePosition();
            int sw = GetScreenWidth();
            int sh = GetScreenHeight();
            bool mouseInWindow = (mp.X >= 0 && mp.Y >= 0 && mp.X < sw && mp.Y < sh);

            if (Config.ClipMouse && windowActive && mouseInWindow)
            {
                int clampedX = Math.Max(0, Math.Min((int)mp.X, sw - 1));
                int clampedY = Math.Max(0, Math.Min((int)mp.Y, sh - 1));
                if (clampedX != (int)mp.X || clampedY != (int)mp.Y)
                    SetMousePosition(clampedX, clampedY);
                _lastMouse = new System.Drawing.Point(clampedX, clampedY);
            }
            else
            {
                _lastMouse = new System.Drawing.Point((int)mp.X, (int)mp.Y);
            }

            if (_lastMouse != _prevMouse)
            {
                Try(() => DXControl.ActiveScene?.OnMouseMove(
                    new MouseEventArgs(_lastButtons, 0, _lastMouse.X, _lastMouse.Y, 0)));
                _prevMouse = _lastMouse;
            }

            // 2) 鼠标按键（左/右/中）
            HandleMouseButton(MouseButton.Left, MouseButtons.Left);
            HandleMouseButton(MouseButton.Right, MouseButtons.Right);
            HandleMouseButton(MouseButton.Middle, MouseButtons.Middle);

            // 3) 鼠标滚轮（raylib 返回的是增量）
            float wheel = GetMouseWheelMove();
            if (Math.Abs(wheel) > float.Epsilon)
            {
                int delta = (int)(wheel * 120); // WinForms 习惯单位
                Try(() => DXControl.ActiveScene?.OnMouseWheel(
                    new MouseEventArgs(_lastButtons, 0, _lastMouse.X, _lastMouse.Y, delta)));
            }

            // 4) 修饰键状态与 Alt+Enter 全屏
            CEnvir.Shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
            CEnvir.Alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
            CEnvir.Ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);

            if (CEnvir.Alt && IsKeyPressed(KeyboardKey.Enter))
            {
                DXManager.ToggleFullScreen();
                // 原逻辑这里直接返回以避免额外分发；如果你要继续分发就别 return
                // return;
            }

            // 5) 键盘按下/弹起与 KeyPress（可打印字符）
            foreach (var key in _keysToTrack)
            {
                bool nowDown = IsKeyDown(key);
                bool tracked = _downKeys.Contains(key);

                if (nowDown && !tracked)
                {
                    _downKeys.Add(key);
                    var e = new KeyEventArgs(MapKeyToWinForms(key));
                    Try(() => { DXControl.ActiveScene?.OnKeyDown(e); e.Handled = true; });
                }
                else if (!nowDown && tracked)
                {
                    _downKeys.Remove(key);
                    var e = new KeyEventArgs(MapKeyToWinForms(key));

                    // 截图在 KeyUp 处理，和你旧逻辑对齐
                    if (key == KeyboardKey.PrintScreen || key == KeyboardKey.Pause)
                        CreateScreenShot();

                    Try(() => { DXControl.ActiveScene?.OnKeyUp(e); e.Handled = true; });

                    // KeyPress：仅对可打印字符触发一次
                    if (TryMapPrintableChar(key, out char ch))
                        Try(() => DXControl.ActiveScene?.OnKeyPress(new KeyPressEventArgs(ch)));
                }
            }
        }

        // ========== 与原类同名的“事件方法”保留（供反射或直接调用） ==========
        // 这些方法在 raylib 模式下不会被框架自动调用，但保留签名避免外部代码引用崩。
        protected void OnDeactivate(EventArgs e)
        {
            if (GameScene.Game != null)
                GameScene.Game.MapControl.MapButtons = MouseButtons.None;

            CEnvir.Shift = false;
            CEnvir.Alt = false;
            CEnvir.Ctrl = false;
        }

        protected void OnMouseMove(MouseEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnMouseDown(MouseEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnMouseUp(MouseEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnMouseClick(MouseEventArgs e)
        { /* TODO: 如需严格区分 Click，可在 HandleMouseButton 里统计 */ }

        protected void OnMouseDoubleClick(MouseEventArgs e)
        { /* TODO: 需要双击时间阈值判定；raylib 无内建双击 */ }

        protected void OnMouseWheel(MouseEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnKeyDown(KeyEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnKeyUp(KeyEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnKeyPress(KeyPressEventArgs e)
        { /* 轮询里已处理 */ }

        protected void OnFormClosing(FormClosingEventArgs e)
        { /* 原逻辑依赖 WinForms 生命周期，这里不使用 */ }

        // ========== 截图：实现原来的 CreateScreenShot（之前是空） ==========
        public static void CreateScreenShot()
        {
            try
            {
                Directory.CreateDirectory("Screenshots");
                string file = Path.Combine("Screenshots", $"shot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                TakeScreenshot(file); // raylib-cs 7.0.1 直接保存
                // 如果你要回传 UI 提示，在这里加日志
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        // ========== 私有：鼠标与键盘的辅助 ==========
        private System.Drawing.Point _prevMouse;

        private System.Drawing.Point _lastMouse;
        private MouseButtons _lastButtons;

        private readonly HashSet<KeyboardKey> _downKeys = new();

        // 选择一组常用键做轮询；需要更多键自己加
        private static readonly KeyboardKey[] _keysToTrack = new[]
        {
            KeyboardKey.Space, KeyboardKey.Enter, KeyboardKey.Escape,
            KeyboardKey.Tab, KeyboardKey.Backspace,
            KeyboardKey.Left, KeyboardKey.Right, KeyboardKey.Up, KeyboardKey.Down,
            KeyboardKey.Zero, KeyboardKey.One, KeyboardKey.Two, KeyboardKey.Three, KeyboardKey.Four,
            KeyboardKey.Five, KeyboardKey.Six, KeyboardKey.Seven, KeyboardKey.Eight, KeyboardKey.Nine,
            KeyboardKey.A, KeyboardKey.B, KeyboardKey.C, KeyboardKey.D, KeyboardKey.E, KeyboardKey.F,
            KeyboardKey.G, KeyboardKey.H, KeyboardKey.I, KeyboardKey.J, KeyboardKey.K, KeyboardKey.L,
            KeyboardKey.M, KeyboardKey.N, KeyboardKey.O, KeyboardKey.P, KeyboardKey.Q, KeyboardKey.R,
            KeyboardKey.S, KeyboardKey.T, KeyboardKey.U, KeyboardKey.V, KeyboardKey.W, KeyboardKey.X,
            KeyboardKey.Y, KeyboardKey.Z,
            KeyboardKey.LeftShift, KeyboardKey.RightShift, KeyboardKey.LeftControl, KeyboardKey.RightControl,
            KeyboardKey.LeftAlt, KeyboardKey.RightAlt,
            KeyboardKey.PrintScreen, KeyboardKey.Pause
        };

        private void HandleMouseButton(MouseButton rb, MouseButtons wb)
        {
            bool downNow = IsMouseButtonDown(rb);
            bool pressed = IsMouseButtonPressed(rb);
            bool released = IsMouseButtonReleased(rb);

            if (pressed)
            {
                _lastButtons |= wb;
                // 原逻辑里右键按下会清除选中等（在 GameScene 那边做），这里只转发事件 :contentReference[oaicite:8]{index=8}
                Try(() => DXControl.ActiveScene?.OnMouseDown(
                    new MouseEventArgs(_lastButtons, 1, _lastMouse.X, _lastMouse.Y, 0)));
            }
            if (released)
            {
                _lastButtons &= ~wb;
                Try(() => DXControl.ActiveScene?.OnMouseUp(
                    new MouseEventArgs(_lastButtons, 1, _lastMouse.X, _lastMouse.Y, 0)));
                // Click 事件：简单起见，按一次释放就算一次点击；需要双击可加时间阈值
                Try(() => DXControl.ActiveScene?.OnMouseClick(
                    new MouseEventArgs(wb, 1, _lastMouse.X, _lastMouse.Y, 0)));
            }
        }

        private static Keys MapKeyToWinForms(KeyboardKey key)
        {
            return key switch
            {
                KeyboardKey.Enter => Keys.Enter,
                KeyboardKey.Escape => Keys.Escape,
                KeyboardKey.Space => Keys.Space,
                KeyboardKey.Tab => Keys.Tab,
                KeyboardKey.Backspace => Keys.Back,
                KeyboardKey.Left => Keys.Left,
                KeyboardKey.Right => Keys.Right,
                KeyboardKey.Up => Keys.Up,
                KeyboardKey.Down => Keys.Down,
                KeyboardKey.Zero => Keys.D0,
                KeyboardKey.One => Keys.D1,
                KeyboardKey.Two => Keys.D2,
                KeyboardKey.Three => Keys.D3,
                KeyboardKey.Four => Keys.D4,
                KeyboardKey.Five => Keys.D5,
                KeyboardKey.Six => Keys.D6,
                KeyboardKey.Seven => Keys.D7,
                KeyboardKey.Eight => Keys.D8,
                KeyboardKey.Nine => Keys.D9,
                KeyboardKey.A => Keys.A,
                KeyboardKey.B => Keys.B,
                KeyboardKey.C => Keys.C,
                KeyboardKey.D => Keys.D,
                KeyboardKey.E => Keys.E,
                KeyboardKey.F => Keys.F,
                KeyboardKey.G => Keys.G,
                KeyboardKey.H => Keys.H,
                KeyboardKey.I => Keys.I,
                KeyboardKey.J => Keys.J,
                KeyboardKey.K => Keys.K,
                KeyboardKey.L => Keys.L,
                KeyboardKey.M => Keys.M,
                KeyboardKey.N => Keys.N,
                KeyboardKey.O => Keys.O,
                KeyboardKey.P => Keys.P,
                KeyboardKey.Q => Keys.Q,
                KeyboardKey.R => Keys.R,
                KeyboardKey.S => Keys.S,
                KeyboardKey.T => Keys.T,
                KeyboardKey.U => Keys.U,
                KeyboardKey.V => Keys.V,
                KeyboardKey.W => Keys.W,
                KeyboardKey.X => Keys.X,
                KeyboardKey.Y => Keys.Y,
                KeyboardKey.Z => Keys.Z,
                KeyboardKey.LeftShift => Keys.ShiftKey,
                KeyboardKey.RightShift => Keys.ShiftKey,
                KeyboardKey.LeftControl => Keys.ControlKey,
                KeyboardKey.RightControl => Keys.ControlKey,
                KeyboardKey.LeftAlt => Keys.Menu,
                KeyboardKey.RightAlt => Keys.Menu,
                KeyboardKey.PrintScreen => Keys.PrintScreen,
                KeyboardKey.Pause => Keys.Pause,
                _ => Keys.None
            };
        }

        private static bool TryMapPrintableChar(KeyboardKey key, out char ch)
        {
            // 非本地化的简易映射：A-Z/0-9/空格
            if (key >= KeyboardKey.A && key <= KeyboardKey.Z)
            {
                bool shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
                ch = (char)((shift ? 'A' : 'a') + (key - KeyboardKey.A));
                return true;
            }
            if (key >= KeyboardKey.Zero && key <= KeyboardKey.Nine)
            {
                ch = (char)('0' + (key - KeyboardKey.Zero));
                return true;
            }
            if (key == KeyboardKey.Space) { ch = ' '; return true; }
            ch = '\0';
            return false;
        }

        private static void Try(Action action)
        {
            try { action(); }
            catch (Exception ex) { CEnvir.SaveException(ex); }
        }
    }
}