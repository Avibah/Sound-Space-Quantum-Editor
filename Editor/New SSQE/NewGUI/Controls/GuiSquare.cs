using System.Drawing;
using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSquare : Control
    {
        protected Color color;
        protected readonly bool outline;

        public GuiSquare(float x, float y, float w, float h, Color? color = null, bool outline = false) : base(x, y, w, h)
        {
            this.color = color ?? Color.Transparent;
            this.outline = outline;
        }
        public GuiSquare(Color? color = null, bool outline = false) : this(0, 0, 1920, 1080, color, outline) { }

        public override float[] Draw()
        {
            if (outline)
                return GLVerts.Outline(rect, 2, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            else
                return GLVerts.Rect(rect, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public void SetColor(Color? color = null)
        {
            this.color = color ?? this.color;
            Update();
        }
    }
}
