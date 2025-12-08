using New_SSQE.ExternalUtils;
using New_SSQE.Misc;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace New_SSQE.NewGUI
{
    internal static class Texturing
    {
        private static readonly Dictionary<string, (int, SKBitmap)> textures = [];

        public static int Generate(string texture, SKBitmap? image = null, bool smooth = false, TextureUnit texUnit = TextureUnit.Texture0)
        {
            if (textures.TryGetValue(texture, out (int, SKBitmap) value))
                return value.Item1;

            if (image == null)
            {
                string file = Path.Combine(Assets.TEXTURES, $"{texture}.png");

                if (!File.Exists(file))
                {
                    Logging.Log($"Failed to load texture: {texture} (File not found)", LogSeverity.WARN);
                    return 0;
                }

                using FileStream fs = File.OpenRead(file);
                image = SKBitmap.Decode(fs);
            }

            int id = GLState.NewTexture(texUnit, smooth);
            GLState.LoadTexture(id, image.Width, image.Height, image.GetPixels(), texUnit);

            return id;
        }
    }
}
