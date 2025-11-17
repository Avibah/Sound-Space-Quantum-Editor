using New_SSQE.Audio;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Graphics.OpenGL;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonPlayPause : GuiButtonTextured
    {
        public GuiButtonPlayPause(float x, float y, float w, float h) : base(x, y, w, h, new("widgets", null, false, TextureUnit.Texture1))
        {
            TileSize = (2, 2);
            Style = ControlStyle.Transparent;
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);
            textures[0].TileIndex = MusicPlayer.IsPlaying ? 1 : 0;
        }

        public override void MouseClickLeft(float x, float y)
        {
            base.MouseClickLeft(x, y);

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
