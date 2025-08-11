using Raylib_cs;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using Font = Raylib_cs.Font;
using Image = Raylib_cs.Image;

namespace Ray2D
{
    public class RayFont
    {
        private static readonly Dictionary<int, Font> fonts = new();
        private static readonly HashSet<int> loadedCodepoints = new();
        private static readonly HashSet<int> pendingCodepoints = new();

        private static double lastReloadTime = 0;

        private static string _fontPath = string.Empty;
        private static string comm_chars = string.Empty;

        private static int LoadFontSize = 48;

        static RayFont()
        {
        }

        public static void LoadFont(string fontPath)
        {
            _fontPath = fontPath;
        }

        public static void UnloadAll()
        {
            foreach (var font in fonts.Values)
                Raylib.UnloadFont(font);
            fonts.Clear();
        }

        public static void LoadCommChars(string charsPath)
        {
            // 初始化字体
            // $"{Config.AppPath}/Config/chars3500+.txt"
            comm_chars = File.ReadAllText(charsPath); // 3500 常用字
            comm_chars = comm_chars.Replace("\r", "").Replace("\n", "");

            ReloadFont(LoadFontSize);
        }

        public static void AddPendingText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var cps = GetCodePoints(text);
            foreach (var cp in cps)
            {
                if (!loadedCodepoints.Contains(cp))
                    pendingCodepoints.Add(cp);
            }
        }

        public static void Update()
        {
            double now = Raylib.GetTime();
            if (pendingCodepoints.Count > 0 && now - lastReloadTime > 3.0)
            {
                loadedCodepoints.UnionWith(pendingCodepoints);
                pendingCodepoints.Clear();

                foreach (var size in fonts.Keys.ToArray())
                {
                    ReloadFont(size);
                }

                lastReloadTime = now;
            }
        }

        public static System.Drawing.Size GetTextSize(int fontSize, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (fonts.TryGetValue(LoadFontSize, out var font))
                {
                    Vector2 size = Raylib.MeasureTextEx(font, text, fontSize, 0);
                    return size.ToSize();
                }
            }

            return System.Drawing.Size.Empty;
        }

        public static void DrawText(int fontSize, string text, System.Drawing.Point pt, System.Drawing.Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (!fonts.TryGetValue(LoadFontSize, out var font))
            {
                ReloadFont(LoadFontSize);
                font = fonts[fontSize];
            }
            Raylib.DrawTextEx(font, text, pt.ToVector2(), fontSize, 0, color.ToRayColor());
        }

        private static void ReloadFont(int fontSize)
        {
            if (loadedCodepoints.Count == 0)
            {
                var cps = GetCodePoints(comm_chars);
                loadedCodepoints.UnionWith(cps);
                comm_chars = string.Empty;
            }

            if (fonts.TryGetValue(fontSize, out var font))
            {
                Raylib.UnloadFont(font);
            }

            Font new_font = Raylib.LoadFontEx(_fontPath, fontSize, loadedCodepoints.ToArray(), loadedCodepoints.Count);

            Image fontImage = Raylib.LoadImageFromTexture(font.Texture);
            Raylib.ImageFormat(ref fontImage, PixelFormat.UncompressedGrayscale);
            Raylib.UnloadTexture(font.Texture);
            font.Texture = Raylib.LoadTextureFromImage(fontImage);
            Raylib.UnloadImage(fontImage);

            Raylib.SetTextureFilter(new_font.Texture, TextureFilter.Bilinear);
            fonts[fontSize] = new_font;
        }

        private static List<int> GetCodePoints(string text)
        {
            List<int> cps = new();
            var enumerator = StringInfo.GetTextElementEnumerator(text);
            while (enumerator.MoveNext())
            {
                int codepoint = char.ConvertToUtf32(enumerator.Current.ToString(), 0);
                cps.Add(codepoint);
            }
            return cps;
        }
    }
}