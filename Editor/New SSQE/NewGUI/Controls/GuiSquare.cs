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

            Rounded = false;
        }
        public GuiSquare(Color? color = null, bool outline = false) : this(0, 0, 1920, 1080, color, outline) { }

        public override float[] Draw()
        {
            if (outline)
                return Rounded ? GLVerts.SquircleOutline(rect, 2, CornerDetail, CornerRadius, color) : GLVerts.Outline(rect, 2, color);
            else
                return Rounded ? GLVerts.Squircle(rect, CornerDetail, CornerRadius, color) : GLVerts.Rect(rect, color);
        }

        public void SetColor(Color? color = null)
        {
            this.color = color ?? this.color;
            Update();
        }
    }
}
