using Client.Controls;
using Client.Envir;
using Ray2D;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Color = Raylib_cs.Color;

namespace Client1000.RayDraw
{
    public sealed class RayApp
    {
        public static Camera2D Camera;

        // 上一帧鼠标屏幕坐标
        private Vector2 _lastMousePos;

        // 双击判定：只针对“当前”按键
        private MouseButton _downBtn = MouseButton.Left;

        private Vector2 _downPos;
        private MouseButton _lastClickBtn = MouseButton.Left;
        private double _lastClickTime = -1000;

        private const double DblClickSec = 0.30; // 双击时间阈值（秒）
        private const float ClickMoveTol = 6f;   // 点击位移容忍（像素）

        // 预缓存枚举，避免每帧分配
        private static readonly MouseButton[] MouseButtons = { MouseButton.Left, MouseButton.Right, MouseButton.Middle };

        private static readonly KeyboardKey[] KeyboardKeys = Enum.GetValues<KeyboardKey>();

        public RayApp(string title, Size size)
        {
            Raylib.SetWindowTitle(title);
            //Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
            Raylib.InitWindow(size.Width, size.Height, title);
            Raylib.InitAudioDevice();
            Raylib.SetTargetFPS(60);
            Raylib.SetExitKey(KeyboardKey.Null);

            RayFont.LoadFont($"{Config.AppPath}/Data/Fonts/SourceHanSansSC-Regular.ttf");
            RayFont.LoadCommChars($"{Config.AppPath}/Data/Chars/chars3500.txt");

            Camera = new Camera2D
            {
                //Offset = new Vector2(size.Width / 2f, size.Height / 2f),
                Rotation = 0f,
                Zoom = 1.0f,
                Target = Vector2.Zero
            };
        }

        public static void ResetSize(Size size)
        {
            Raylib.SetWindowSize(size.Width, size.Height);
            Camera.Offset = new Vector2(size.Width / 2f, size.Height / 2f);
        }

        public static void SetCameraZoom(float scale, bool abs = false)
        {
            if (abs)
                Camera.Zoom = scale;
            else
                Camera.Zoom = Math.Clamp(Camera.Zoom + scale, 0.5f, 5f);
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                try
                {
                    Update();
                    Render();
                }
                catch (Exception ex)
                {
                    CEnvir.SaveError(ex.ToString());
                }

                Thread.Sleep(1);
            }

            RayFont.UnloadAll();
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }

        private void Update()
        {
            InputUpdate();
            CEnvir.UpdateGame();
        }

        private void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);

            Raylib.BeginMode2D(Camera);
            CEnvir.RenderGame();
            Raylib.EndMode2D();

            CEnvir.RenderUI();

            Raylib.EndDrawing();
        }

        private bool IsMouseOverUI()
        {
            foreach (var window in DXWindow.Windows)
                if (window.Visible && window.DisplayArea.Contains(Raylib.GetMousePosition().ToPoint()))
                    return true;
            return false;
        }

        private void InputUpdate()
        {
            // 修饰键状态
            CEnvir.Shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
            CEnvir.Ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
            CEnvir.Alt = Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt);

            // Alt+Enter 全屏
            if (CEnvir.Alt && Raylib.IsKeyPressed(KeyboardKey.Enter))
                Raylib.ToggleFullscreen();

            // 鼠标位置与转换
            Vector2 mousePos = Raylib.GetMousePosition();
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mousePos, Camera);

            if (mousePos != _lastMousePos)
            {
                _lastMousePos = mousePos;
                HandleMouseMove(new MouseEvent(MouseButton.Left, mousePos, mouseWorldPos));
            }

            // Ctrl + 滚轮缩放（不在 UI 上时）
            float wheelMove = Raylib.GetMouseWheelMove();
            if (wheelMove != 0 && CEnvir.Ctrl && !IsMouseOverUI())
                SetCameraZoom(wheelMove * 0.1f);

            // 滚轮事件转发（单位 120）
            if (wheelMove != 0)
                HandleMouseWheel(new MouseEvent(MouseButton.Middle, mousePos, mouseWorldPos, (int)(wheelMove * 120)));

            // 鼠标按键：按下/抬起/点击/双击（仅当前按键）
            double now = Raylib.GetTime();
            foreach (var mb in MouseButtons)
            {
                if (Raylib.IsMouseButtonPressed(mb))
                {
                    _downBtn = mb;
                    _downPos = mousePos;
                    HandleMouseDown(new MouseEvent(mb, mousePos, mouseWorldPos));
                }

                if (Raylib.IsMouseButtonReleased(mb))
                {
                    // 点击：同键按下抬起且位移未超阈值
                    if (mb == _downBtn && Vector2.Distance(_downPos, mousePos) <= ClickMoveTol)
                    {
                        HandleMouseClick(new MouseEvent(mb, mousePos, mouseWorldPos));

                        // 双击：同键且与上次点击时间差小于阈值
                        if (mb == _lastClickBtn && now - _lastClickTime <= DblClickSec)
                            HandleMouseDClick(new MouseEvent(mb, mousePos, mouseWorldPos));

                        _lastClickBtn = mb;
                        _lastClickTime = now;
                    }

                    HandleMouseUp(new MouseEvent(mb, mousePos, mouseWorldPos));
                }
            }

            // 键盘按下（上沿）
            for (int keyCode = Raylib.GetKeyPressed(); keyCode != 0; keyCode = Raylib.GetKeyPressed())
            {
                var e = new KeyEvent((KeyboardKey)keyCode);
                HandleKeyDown(e);
            }

            // 键盘抬起（下沿）
            foreach (var kk in KeyboardKeys)
            {
                if (Raylib.IsKeyReleased(kk))
                    HandleKeyUp(new KeyEvent(kk));
            }

            // 文本输入（Unicode）
            for (int ch = Raylib.GetCharPressed(); ch != 0; ch = Raylib.GetCharPressed())
            {
                OnKeyPress(new KeyEvent(KeyboardKey.Null) { Char = ch });
            }

            // 更新全局鼠标位置（旧逻辑兼容）
            CEnvir.MouseLocation = new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y);
        }

        // ===== 事件转发到现有处理函数 =====

        private void HandleMouseMove(MouseEvent e)
        {
            DXControl.ActiveScene?.HandleMouseMove(e);
        }

        private void HandleMouseDown(MouseEvent e)
        {
            DXControl.ActiveScene?.HandleMouseDown(e);
        }

        private void HandleMouseUp(MouseEvent e)
        {
            DXControl.ActiveScene?.HandleMouseUp(e);
        }

        private void HandleMouseClick(MouseEvent e)
        {
            DXControl.ActiveScene?.HandleMouseClick(e);
        }

        private void HandleMouseDClick(MouseEvent e)
        {
            DXControl.ActiveScene?.HandleMouseDClick(e);
        }

        private void HandleMouseWheel(MouseEvent e)
        {
            DXControl.ActiveScene?.HandleMouseWheel(e);
        }

        private void HandleKeyDown(KeyEvent e)
        {
            DXControl.ActiveScene?.HandleKeyDown(e);
        }

        private void HandleKeyUp(KeyEvent e)
        {
            DXControl.ActiveScene?.HandleKeyUp(e);
        }

        private void OnKeyPress(KeyEvent e)
        {
            DXControl.ActiveScene?.HandleKeyPress(e);
        }
    }
}