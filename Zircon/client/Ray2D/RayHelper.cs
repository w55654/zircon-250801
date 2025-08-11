using Raylib_cs;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ray2D
{
    public static class RayHelper
    {
        public static string GetClipboardText()
        {
            unsafe
            {
                return Marshal.PtrToStringUTF8((IntPtr)Raylib.GetClipboardText());
            }
        }

        public static void SetClipboardText(string text)
        {
            Raylib.SetClipboardText(text);
        }
    }
}