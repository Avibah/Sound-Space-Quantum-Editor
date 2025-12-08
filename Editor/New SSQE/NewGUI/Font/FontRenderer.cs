using New_SSQE.Misc;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Font
{
    internal static class FontRenderer
    {
        static FontRenderer()
        {
            StbFont.InitUnicode(Path.Combine(Assets.FONTS, "Unifont-P0.png"), TextureUnit.Texture12);
        }

        public static bool Unicode = false;

        private static readonly Dictionary<string, TextureUnit> fontUnits = new()
        {
            {"main", TextureUnit.Texture15 },
            {"square", TextureUnit.Texture14 },
            {"squareo", TextureUnit.Texture13 },
            {"unicode", TextureUnit.Texture12 }
        };

        private static readonly Dictionary<string, StbFont> fonts = new()
        {
            {"main", new("main", fontUnits["main"]) },
            {"square", new("square", fontUnits["square"]) },
            {"squareo", new("squareo", fontUnits["squareo"]) }
        };

        public static Vector4[] Print(float x, float y, string text, float textSize, string font)
            => fonts[font].Print(x, y, text, textSize * Settings.fontScale.Value, Unicode);
        public static void PrintInto(Vector4[] array, int offset, float x, float y, string text, float textSize, string font)
            => fonts[font].PrintInto(array, offset, x, y, text, textSize * Settings.fontScale.Value, Unicode);
        public static float GetWidth(string text, float textSize, string font)
            => fonts[font].Extent(text, textSize * Settings.fontScale.Value, Unicode);
        public static float GetHeight(float textSize, string font)
            => fonts[font].Baseline(textSize * Settings.fontScale.Value, Unicode);

        private static (string, bool) activeFont = ("", false);
        private static Shader Shader => Unicode ? Shader.Unicode : Shader.Font;

        public static void SetActive(string font)
        {
            if (activeFont == (font, Unicode))
                return;
            activeFont = (font, Unicode);

            if (Unicode)
            {
                GLState.EnableTextureUnit(Shader, fontUnits["unicode"]);
                Shader.Uniform2("CharSize", (1 - 0.04f) / 256f, (1 - 0.04f) / 256f);
            }
            else
            {
                GLState.EnableTextureUnit(Shader, fontUnits[font]);
                Shader.Uniform4("TexLookup", fonts[font].AtlasMetrics);
                Shader.Uniform2("CharSize", fonts[font].CharSize);
            }
        }

        public static void SetColor(Color color)
        {
            Shader.Uniform4("TexColor", color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
        public static void SetColor(int r, int g, int b, int a = 255) => SetColor(Color.FromArgb(r, g, b, a));

        public static void RenderData(string font, Vector4[] data, float[]? alpha = null, int? count = null)
        {
            if (data.Length > 0)
            {
                alpha ??= new float[data.Length];

                GLState.BufferData(Unicode ? StbFont.UnicodeVBO_0 : fonts[font].VBO_0, data);
                GLState.BufferData(Unicode ? StbFont.UnicodeVBO_1 : fonts[font].VBO_1, alpha);
                GLState.DrawInstances(Unicode ? StbFont.UnicodeVAO : fonts[font].VAO, 0, 6, count ?? data.Length);
            }
        }

        public static string TrimText(string text, float textSize, string font, int width)
        {
            string end = "...";
            float endWidth = GetWidth(end, textSize, font);

            if (GetWidth(text, textSize, font) < width)
                return text;

            while (text.Length > 0 && GetWidth(text, textSize, font) >= width - endWidth)
                text = text[..^1];

            return text + end;
        }
    }
}
