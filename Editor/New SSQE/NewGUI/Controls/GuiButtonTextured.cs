using New_SSQE.NewGUI.Base;
using OpenTK.Mathematics;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiButtonTextured : GuiButton
    {
        public Vector2i TileSize
        {
            get => textures[0].TileSize;
            set => textures[0].TileSize = value;
        }
        
        public int TileIndex
        {
            get => textures[0].TileIndex;
            set => textures[0].TileIndex = value;
        }

        public GuiButtonTextured(float x, float y, float w, float h, Texture texture) : base(x, y, w, h)
        {
            textures = [texture];
        }

        public override float[] Draw()
        {
            textures[0].Draw(rect);
            return base.Draw();
        }
    }
}
