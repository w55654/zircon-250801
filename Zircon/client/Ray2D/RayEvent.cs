using Raylib_cs;
using System.Drawing;
using System.Numerics;

namespace Ray2D
{
    //
    public struct MouseEvent
    {
        public MouseButton Button { get; }
        public Point Location { get; }
        public Point WorldLocation { get; }
        public int Delta { get; }  // ¹öÂÖÔöÁ¿
        public int Clicks = 1;

        public MouseEvent(MouseButton button, Vector2 pos, Vector2 worldpos, int delta = 0)
        {
            Button = button;
            Location = pos.ToPoint();
            WorldLocation = worldpos.ToPoint();
            Delta = delta;
        }
    }

    public struct KeyEvent
    {
        public KeyboardKey KeyCode { get; }
        public bool Handled;
        public int Char;

        public KeyEvent(KeyboardKey keyCode)
        {
            KeyCode = keyCode;
        }
    }
}