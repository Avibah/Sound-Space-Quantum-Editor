using System.Drawing;
using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSquare : Control
    {
        private Color color = Color.Transparent;
        private bool outline;

        public Color Color
        {
            get => color;
            set
            {
                if (value != color)
                {
                    color = value;
                    shouldUpdate = true;
                }
            }
        }

        public bool Outline
        {
            get => outline;
            set
            {
                if (value != outline)
                {
                    outline = value;
                    shouldUpdate = true;
                }
            }
        }

        public GuiSquare(float x, float y, float w, float h) : base(x, y, w, h)
        {
            cornerRadius = 0;
        }
        public GuiSquare() : this(0, 0, 1920, 1080) { }

        public override float[] Draw()
        {
            if (outline)
                return GLVerts.SquircleOutline(rect, lineThickness, cornerDetail, cornerRadius, color);
            else
                return GLVerts.Squircle(rect, cornerDetail, cornerRadius, color);
        }
    }
}
