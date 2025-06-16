using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiSquareRounded : GuiSquare
    {
        private int cornerDetail;
        public int CornerDetail
        {
            get => cornerDetail;
            set
            {
                cornerDetail = value;
                Update();
            }
        }

        private float cornerRadius;
        public float CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = value;
                Update();
            }
        }

        public GuiSquareRounded(float x, float y, float w, float h, int cornerDetail, float cornerRadius, Color? color = null, bool outline = false) : base(x, y, w, h, color, outline)
        {
            this.cornerDetail = cornerDetail;
            this.cornerRadius = cornerRadius;
        }

        public override float[] Draw()
        {
            if (outline)
                return GLVerts.SquircleOutline(rect, 2, cornerDetail, cornerRadius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            else
                return GLVerts.Squircle(rect, cornerDetail, cornerRadius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }
}
