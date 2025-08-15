using Client.Envir;
using Client.Scenes;
using Client.Scenes.Views;
using Library;
using Ray2D;
using Raylib_cs;
using System;
using System.Drawing;

//Cleaned
namespace Client.Controls
{
    public abstract class DXScene : DXControl
    {
        #region Properties

        public MouseButton Buttons;

        public override sealed Size Size
        {
            get => base.Size;
            set => base.Size = value;
        }

        public override void OnLocationChanged(Point oValue, Point nValue)
        {
            base.OnLocationChanged(oValue, nValue);

            if (DebugLabel == null || PingLabel == null) return;

            DebugLabel.Location = new Point(Location.X + 5, Location.Y + 5);

            PingLabel.Location = new Point(Location.X + 5, Location.Y + 19);
        }

        public override void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            base.OnIsVisibleChanged(oValue, nValue);

            if (!IsVisible) return;

            foreach (DXComboBox box in DXComboBox.ComboBoxes)
                box.ListBox.Parent = this;
        }

        #endregion

        protected DXScene(Size size)
        {
            DrawTexture = false;

            Size = size;

            DXManager.SetResolution(size);
        }

        #region Methods

        public void HandleMouseDown(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (MouseControl != null && MouseControl != this)
                MouseControl.OnMouseDown(e);
            else
                base.OnMouseDown(e);

            DXControl listbox = MouseControl;

            while (listbox != null)
            {
                if (listbox is DXListBox) break;

                listbox = listbox.Parent;
            }

            foreach (DXComboBox box in DXComboBox.ComboBoxes)
            {
                if (box.ListBox != listbox)
                    box.Showing = false;
            }
        }

        public void HandleMouseUp(MouseEvent e)
        {
            if (GameScene.Game != null)
                GameScene.Game.MapControl.MapButtons = MouseButton.Back;

            if (!IsEnabled)
                return;

            if (MouseControl != null && MouseControl != this)
                MouseControl.OnMouseUp(e);
            else
                base.OnMouseUp(e);
        }

        public void HandleMouseMove(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (FocusControl != null && FocusControl != this && FocusControl is MapControl)
                FocusControl.OnMouseMove(e);
            else if (MouseControl != null && MouseControl != this && (MouseControl.IsMoving || MouseControl.IsResizing))
                MouseControl.OnMouseMove(e);
            else
                base.OnMouseMove(e);
        }

        public void HandleMouseClick(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (MouseControl != null && MouseControl != this)
            {
                if (MouseControl == FocusControl)
                    MouseControl.OnMouseClick(e);
            }
            else
                base.OnMouseClick(e);

            Buttons = e.Button;
        }

        public void HandleMouseDClick(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (MouseControl != null && MouseControl != this)
            {
                MouseControl.OnMouseDClick(e);
            }
        }

        public void HandleMouseWheel(MouseEvent e)
        {
            if (!IsEnabled) return;

            if (MouseControl != null && MouseControl != this)
                MouseControl.OnMouseWheel(e);
            else
                base.OnMouseWheel(e);
        }

        public void HandleKeyDown(KeyEvent e)
        {
            OnKeyDown(e);
        }

        public void HandleKeyUp(KeyEvent e)
        {
            OnKeyUp(e);
        }

        public void HandleKeyPress(KeyEvent e)
        {
            OnKeyPress(e);
        }

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();

            /*
            DXManager.Sprite.Flush();
            if (!Location.IsEmpty)
                DXManager.Device.Clear(ClearFlags.Target, Color.Black, 1, 0, new[]
                {
                    new Rectangle(0, 0, Location.X > 0 ? Location.X : ScreenSize.Width, Location.X == 0 ? Location.Y : ScreenSize.Height),
                    new Rectangle(Location.X > 0 ? Size.Width + Location.X : 0,
                                  Location.X == 0 ? Size.Height + Location.Y : 0,
                                  Location.X > 0 ? Location.X : ScreenSize.Width,
                                  Location.X == 0 ? Location.Y : ScreenSize.Height)
                });
            */

            DebugLabel.Draw();

            if (!string.IsNullOrEmpty(HintLabel.Text))
                HintLabel.Draw();

            if (!string.IsNullOrEmpty(PingLabel.Text))
                PingLabel.Draw();
        }

        protected internal override sealed void CheckIsVisible()
        {
            IsVisible = Visible && ActiveScene == this;

            foreach (DXControl control in Controls)
                control.CheckIsVisible();
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
            }
        }

        #endregion
    }
}