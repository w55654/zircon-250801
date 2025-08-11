using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Client.Scenes.Views;
using Ray2D;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using BlendMode = Raylib_cs.BlendMode;
using Color = Raylib_cs.Color;

namespace Client1000.RayDraw
{
    public sealed class RayApp
    {
        public static Camera2D Camera;

        private Vector2 lastMousePos;

        private Dictionary<MouseButton, float> lastClickTime = new();
        private float doubleClickThreshold = 0.3f;

        // 类成员，缓存枚举数组，避免每帧生成
        private static readonly MouseButton[] MouseButtons = Enum.GetValues<MouseButton>();

        private static readonly KeyboardKey[] KeyboardKeys = Enum.GetValues<KeyboardKey>();

        public RayApp(string title, Size size)
        {
            Raylib.SetWindowTitle(title);
            // 开启 4x MSAA 抗锯齿
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
            //Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

            Raylib.InitWindow(size.Width, size.Height, title);

            Raylib.InitAudioDevice();

            Raylib.SetTargetFPS(60);
            // 取消按 ESC 关闭窗口的默认行为
            Raylib.SetExitKey(KeyboardKey.Null);

            RayFont.LoadFont($"{Config.AppPath}/Data/Fonts/SourceHanSansSC-Bold.ttf");
            RayFont.LoadCommChars($"{Config.AppPath}/Data/Chars/chars3500.txt");

            Camera = new Camera2D
            {
                Offset = new Vector2(size.Width / 2f, size.Height / 2f),
                Rotation = 0.0f,
                Zoom = 1.5F,
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
            {
                Camera.Zoom = scale;
            }
            else
            {
                Camera.Zoom += scale;
                Camera.Zoom = Math.Clamp(Camera.Zoom, 0.5f, 5.0f);
            }
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                // 每帧更新窗口标题
                //titleUpdater.Update(dt);

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
            // 更新
            CEnvir.UpdateGame();
        }

        private void Render()
        {
            // 开始绘制帧
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);

            // 启动 2D 摄像机模式，渲染游戏场景
            Raylib.BeginMode2D(Camera);
            CEnvir.RenderGame();
            Raylib.EndMode2D();

            // === 光照遮罩 ===（放在 UI 前）
            //DrawLightingMask();

            if (GameScene.Game != null)
            {
                //NamePanel.Draw();
            }

            // 渲染 UI 层（在世界坐标之外）
            CEnvir.RenderUI();

            // 绘制屏幕中央十字辅助线
            //DrawCrosshair();

            Raylib.EndDrawing();
        }

        private bool IsMouseOverUI()
        {
            foreach (var window in DXWindow.Windows)
            {
                if (window.Visible && window.DisplayArea.Contains(Raylib.GetMousePosition().ToPoint()))
                    return true;
            }
            return false;
        }

        private void DrawLightingMask()
        {
            if (GameScene.Game == null) return;

            // 获取相机 target（玩家或跟随目标）
            Vector2 worldTarget = Camera.Target;

            // 转换为屏幕坐标
            Vector2 screenPos = Raylib.GetWorldToScreen2D(worldTarget, Camera);

            // 开启 alpha 混合遮罩
            Raylib.BeginBlendMode(BlendMode.Alpha);

            // 黑色遮罩（半透明）
            Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(0, 0, 0, 200));

            // 在相机中心挖一个光圈（100 像素半径）
            Raylib.DrawCircleV(screenPos, 100, new Color(0, 0, 0, 0));

            Raylib.EndBlendMode();
        }

        private void DrawCrosshair()
        {
            int centerX = Config.GameSize.Width / 2;
            int centerY = Config.GameSize.Height / 2;
            int crossSize = 5;

            Color crossColor = Color.Red;

            // 垂直线
            Raylib.DrawLine(centerX, centerY - crossSize, centerX, centerY + crossSize, crossColor);

            // 水平线
            Raylib.DrawLine(centerX - crossSize, centerY, centerX + crossSize, centerY, crossColor);
        }

        private void InputUpdate()
        {
            // === 获取鼠标位置与世界坐标（只转换一次） ===
            Vector2 mousePos = Raylib.GetMousePosition();
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mousePos, Camera);

            // === 鼠标滚轮 ===
            float wheelMove = Raylib.GetMouseWheelMove();

            // === Ctrl + 滚轮控制缩放 ===
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && wheelMove != 0)
            {
                SetCameraZoom(wheelMove * 0.1f);

                //Camera.Zoom += wheelMove * 0.1f;
                //Camera.Zoom = Math.Clamp(Camera.Zoom, 0.5f, 5.0f);
            }

            // === 鼠标移动 ===
            if (mousePos != lastMousePos)
            {
                lastMousePos = mousePos;
                HandleMouseMove(new MouseEvent(MouseButton.Left, mousePos, mouseWorldPos));
            }

            // === 鼠标按钮事件处理（Down / Up / Click / DClick） ===
            float currentTime = (float)Raylib.GetTime();

            foreach (var mouseButton in MouseButtons)
            {
                bool down = Raylib.IsMouseButtonDown(mouseButton);
                bool up = Raylib.IsMouseButtonReleased(mouseButton);
                bool press = Raylib.IsMouseButtonPressed(mouseButton);

                if (!down && !up && !press)
                    continue; // 跳过无事件的按钮

                var mouseEvent = new MouseEvent(mouseButton, mousePos, mouseWorldPos);

                if (down)
                {
                    HandleMouseHold(mouseEvent);
                }

                if (up)
                {
                    HandleMouseUp(mouseEvent);
                }

                if (press)
                {
                    HandleMouseDown(mouseEvent);

                    if (lastClickTime.TryGetValue(mouseButton, out float last) &&
                        currentTime - last <= doubleClickThreshold)
                    {
                        HandleMouseDClick(mouseEvent);
                    }
                    lastClickTime[mouseButton] = currentTime;
                }
            }

            // === 鼠标滚轮事件 ===
            if (wheelMove != 0)
            {
                HandleMouseWheel(new MouseEvent(MouseButton.Middle, mousePos, mouseWorldPos, (int)wheelMove));
            }

            // === 键盘事件（Pressed / Released）===
            foreach (var key in KeyboardKeys)
            {
                bool pressed = Raylib.IsKeyPressed(key);
                bool released = Raylib.IsKeyReleased(key);

                if (pressed)
                {
                    var keyEvent = new KeyEvent(key);
                    HandleKeyDown(keyEvent);
                    OnKeyPress(keyEvent);
                }

                if (released)
                {
                    var keyEvent = new KeyEvent(key);
                    HandleKeyUp(keyEvent);
                }
            }
        }

        private void HandleMouseMove(MouseEvent e)
        {
            CEnvir.MouseLocation = e.Location;

            if (DXControl.ActiveScene is { } scene)
            {
                //scene.HandleMouseMove(e);

                //if (scene.HasInScene && GameScene.Game != null)
                //{
                //    CEnvir.MapControl?.HandleMouseMove(e);
                //}
            }
        }

        private void HandleMouseHold(MouseEvent e)
        {
            if (DXControl.ActiveScene is { } scene)
            {
                //if (scene.HasInScene)
                //{
                //    CEnvir.MapControl?.HandleMouseHold(e);
                //}
            }
        }

        private void HandleMouseUp(MouseEvent e)
        {
            //DXControl.ActiveScene?.HandleMouseUp(e);

            //if (GameScene.Game != null)
            //{
            //    GameScene.Game.SelectedCell = null;
            //    GameScene.Game.GoldPickedUp = false;
            //    return;
            //}
        }

        private void HandleMouseDown(MouseEvent e)
        {
            //if (GameScene.Game != null && e.Button == MouseButton.Right && (GameScene.Game.SelectedCell != null || GameScene.Game.GoldPickedUp))
            //{
            //    GameScene.Game.SelectedCell = null;
            //    GameScene.Game.GoldPickedUp = false;
            //    return;
            //}

            //if (DXControl.ActiveScene is { } scene)
            //{
            //    scene.HandleMouseDown(e);

            //    if (scene.HasInScene)
            //    {
            //        CEnvir.MapControl?.HandleMouseDown(e);
            //    }
            //}
        }

        private void HandleMouseDClick(MouseEvent e)
        {
            //if (DXControl.ActiveScene is { } scene)
            //{
            //    scene.HandleMouseDClick(e);

            //    if (scene.HasInScene)
            //    {
            //        CEnvir.MapControl?.HandleMouseDClick(e);
            //    }
            //}
        }

        private void HandleMouseWheel(MouseEvent e)
        {
            //DXControl.ActiveScene?.HandleMouseWheel(e);
        }

        private void HandleKeyDown(KeyEvent e)
        {
            // 每次处理新的键时刷新一次辅助键状态
            CEnvir.Shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
            CEnvir.Ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
            CEnvir.Alt = Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt);

            if (CEnvir.Alt && e.KeyCode == KeyboardKey.Enter)
            {
                Raylib.ToggleFullscreen();
                return;
            }

            //DXControl.ActiveScene?.OnKeyDown(e);
            e.Handled = true;
        }

        private void HandleKeyUp(KeyEvent e)
        {
            CEnvir.Shift = false;
            CEnvir.Ctrl = false;
            CEnvir.Alt = false;

            //DXControl.ActiveScene?.OnKeyUp(e);
            e.Handled = true;
        }

        private void OnKeyPress(KeyEvent e)
        {
            //DXControl.ActiveScene?.OnKeyPress(e);
            e.Handled = true;
        }
    }
}