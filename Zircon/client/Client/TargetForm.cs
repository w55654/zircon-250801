// TargetForm.cs — 极简 raylib-cs 输入兼容层
// 作用：每帧调用 PumpInput()，把鼠标/键盘事件转发到 DXControl.ActiveScene 的处理函数。
// 依赖：Raylib_cs 7.0.1、System.Windows.Forms（只用事件参数类型）

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Client.Controls;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Client
{
    public sealed class TargetForm
    {
        public bool Resizing { get; private set; } // 仅保留占位

        // 每帧在主循环里调用一次
        public void PumpInput()
        {
            // 修饰键（保持你原来的全局状态）
            Client.Envir.CEnvir.Shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
            Client.Envir.CEnvir.Alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
            Client.Envir.CEnvir.Ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);

            var scene = DXControl.ActiveScene;
            if (scene == null) return;

            // 鼠标位置与移动
            var mp = GetMousePosition();
            _mouse = new Point((int)mp.X, (int)mp.Y);

            if (_mouse != _prevMouse)
            {
                scene.OnMouseMove(new MouseEventArgs(_buttons, 0, _mouse.X, _mouse.Y, 0));
                _prevMouse = _mouse;
            }

            // 鼠标 Left/Right/Middle 按下/抬起/点击
            HandleMouseButton(MouseButton.Left, MouseButtons.Left, ref _downLPos, ref _downL);
            HandleMouseButton(MouseButton.Right, MouseButtons.Right, ref _downRPos, ref _downR);
            HandleMouseButton(MouseButton.Middle, MouseButtons.Middle, ref _downMPos, ref _downM);

            // 键盘：KeyDown（上沿），KeyUp（下沿），KeyPress（可打印字符）
            for (int k = GetKeyPressed(); k != 0; k = GetKeyPressed())
            {
                var kk = (KeyboardKey)k;
                _keysDown.Add(kk);
                scene.OnKeyDown(new KeyEventArgs(MapKey(kk)));
            }
            // KeyUp：对当前按下集合做下沿检测
            if (_keysDown.Count > 0)
            {
                _tmpKeys.Clear(); _tmpKeys.AddRange(_keysDown);
                foreach (var kk in _tmpKeys)
                {
                    if (IsKeyReleased(kk))
                    {
                        _keysDown.Remove(kk);
                        scene.OnKeyUp(new KeyEventArgs(MapKey(kk)));
                    }
                }
            }
            // KeyPress：逐个取出本帧输入的字符（Unicode）
            for (int ch = GetCharPressed(); ch != 0; ch = GetCharPressed())
                scene.OnKeyPress(new KeyPressEventArgs((char)ch));
        }

        // ---------------- 内部：鼠标映射与点击判定（无拖动阈值可自行加） ----------------
        private void HandleMouseButton(MouseButton rb, MouseButtons wb, ref Point downPos, ref bool isDown)
        {
            if (IsMouseButtonPressed(rb))
            {
                isDown = true;
                downPos = _mouse;
                _buttons |= wb;
                DXControl.ActiveScene?.OnMouseDown(new MouseEventArgs(_buttons, 1, _mouse.X, _mouse.Y, 0));
            }
            if (IsMouseButtonReleased(rb))
            {
                isDown = false;
                _buttons &= ~wb;
                DXControl.ActiveScene?.OnMouseUp(new MouseEventArgs(_buttons, 1, _mouse.X, _mouse.Y, 0));

                // 简化版 Click：按下后在同一位置附近抬起就算点击（可加距离/时间阈值）
                if (Distance2(downPos, _mouse) <= 4)
                    DXControl.ActiveScene?.OnMouseClick(new MouseEventArgs(wb, 1, _mouse.X, _mouse.Y, 0));
            }
        }

        private static int Distance2(Point a, Point b)
        {
            int dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Abs(dx) + Math.Abs(dy); // 替代 sqrt，够用
        }

        // 少量常用键映射到 WinForms Keys（够用就好）
        private static Keys MapKey(KeyboardKey k) => k switch
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
            KeyboardKey.LeftShift => Keys.ShiftKey,
            KeyboardKey.RightShift => Keys.ShiftKey,
            KeyboardKey.LeftControl => Keys.ControlKey,
            KeyboardKey.RightControl => Keys.ControlKey,
            KeyboardKey.LeftAlt => Keys.Menu,
            KeyboardKey.RightAlt => Keys.Menu,
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
            _ => Keys.None
        };

        // ---------------- 状态 ----------------
        private Point _mouse, _prevMouse;

        private MouseButtons _buttons;

        private Point _downLPos, _downRPos, _downMPos;
        private bool _downL, _downR, _downM;

        private readonly HashSet<KeyboardKey> _keysDown = new();
        private readonly List<KeyboardKey> _tmpKeys = new();
    }
}