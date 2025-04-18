using New_SSQE.Audio;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonPlayPause : GuiButton
    {
        public GuiButtonPlayPause(float x, float y, float w, float h) : base(x, y, w, h)
        {
            textures = [new("widgets", null, false, TextureUnit.Texture1) { TileSize = (2, 2) }];
        }

        public override float[] Draw()
        {
            textures[0].Draw(rect);
            return [];
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);
            textures[0].TileIndex = MusicPlayer.IsPlaying ? 1 : 0;
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);
            if (!Hovering)
                return;

            SliderSetting currentTime = Settings.currentTime.Value;

            if (MusicPlayer.IsPlaying)
                MusicPlayer.Pause();
            else
            {
                if (currentTime.Value >= currentTime.Max - 1)
                    currentTime.Value = 0;
                MusicPlayer.Play();
            }
        }
    }
}
