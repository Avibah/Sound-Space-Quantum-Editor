using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal class Gradient : Control
    {
        public float Rotation = 0;
        public float Offset = 0;
        public Color StartColor = Color.Transparent;
        public Color EndColor = Color.Transparent;

        private float _alpha = 0;

        public Gradient() : base(0, 0, 0, 0)
        {
            
        }

        public override float[] Draw()
        {
            float x = rect.X;
            if (Offset < 0)
                x += Offset * rect.Width;

            return GLVerts.Rect2C(x, rect.Y, rect.Width * (Math.Abs(Offset) + 1), rect.Height, StartColor, EndColor);
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            _alpha += frametime;
            Offset = (float)Math.Sin(_alpha);
            Update();

            GLState.PreStencilMask();
            base.PreRender(mousex, mousey, frametime);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            GLState.StencilMask();
            base.Render(mousex, mousey, frametime);
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            GLState.PostStencilMask();
            base.PostRender(mousex, mousey, frametime);
        }
    }
}
