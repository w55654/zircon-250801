using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.Scenes;
using Library;
using SlimDX.Windows;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Font = System.Drawing.Font;

namespace Client
{
    public sealed class TargetForm : RenderForm
    {
        public bool Resizing { get; private set; }

        public TargetForm() : base(Globals.ClientName)
        {
            AutoScaleMode = AutoScaleMode.None;

            AutoScaleDimensions = new SizeF(96F, 96F);

            ClientSize = new Size(1024, 768);

            Icon = Properties.Resources.Zircon;

            FormBorderStyle = (Config.FullScreen || Config.Borderless) ? FormBorderStyle.None : FormBorderStyle.FixedSingle;

            MaximizeBox = false;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (GameScene.Game != null)
                GameScene.Game.MapControl.MapButtons = MouseButtons.None;

            CEnvir.Shift = false;
            CEnvir.Alt = false;
            CEnvir.Ctrl = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Config.ClipMouse && Focused)
                Cursor.Clip = RectangleToScreen(ClientRectangle);
            else
                Cursor.Clip = Rectangle.Empty;

            CEnvir.MouseLocation = e.Location;

            try
            {
                DXControl.ActiveScene?.OnMouseMove(e);
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (GameScene.Game != null && e.Button == MouseButtons.Right && (GameScene.Game.SelectedCell != null || GameScene.Game.CurrencyPickedUp != null))
            {
                GameScene.Game.SelectedCell = null;
                GameScene.Game.CurrencyPickedUp = null;
                return;
            }

            try
            {
                DXControl.ActiveScene?.OnMouseDown(e);
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (GameScene.Game != null)
                GameScene.Game.MapControl.MapButtons &= ~e.Button;

            try
            {
                DXControl.ActiveScene?.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            try
            {
                DXControl.ActiveScene?.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            try
            {
                DXControl.ActiveScene?.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            try
            {
                DXControl.ActiveScene?.OnMouseWheel(e);
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            CEnvir.Shift = e.Shift;
            CEnvir.Alt = e.Alt;
            CEnvir.Ctrl = e.Control;

            try
            {
                if (e.Alt && e.KeyCode == Keys.Enter)
                {
                    DXManager.ToggleFullScreen();
                    return;
                }

                DXControl.ActiveScene?.OnKeyDown(e);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            CEnvir.Shift = e.Shift;
            CEnvir.Alt = e.Alt;
            CEnvir.Ctrl = e.Control;

            if (e.KeyCode == Keys.Pause || e.KeyCode == Keys.PrintScreen)
            {
                CreateScreenShot();
            }

            try
            {
                DXControl.ActiveScene?.OnKeyUp(e);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                DXControl.ActiveScene?.OnKeyPress(e);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (GameScene.Game != null && !GameScene.Game.ExitBox.Exiting)
                {
                    GameScene.Game.ExitBox.Visible = true;
                    e.Cancel = true;
                }
            }
            catch { }
        }

        public static void CreateScreenShot()
        {
        }
    }
}