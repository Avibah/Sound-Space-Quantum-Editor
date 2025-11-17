using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal class Gradient : Control
    {
        private float rotation = 0;
        private Color startColor = Color.FromArgb(0, 0, 0, 0);
        private Color endColor = Color.FromArgb(0, 0, 0, 0);

        public float Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    shouldUpdate = true;
                }
            }
        }

        public Color StartColor
        {
            get => startColor;
            set
            {
                if (startColor != value)
                {
                    startColor = value;
                    shouldUpdate = true;
                }
            }
        }

        public Color EndColor
        {
            get => endColor;
            set
            {
                if (endColor != value)
                {
                    endColor = value;
                    shouldUpdate = true;
                }
            }
        }

        public Gradient(float x, float y, float w, float h) : base(x, y, w, h)
        {
            
        }

        private float MaxProjection(Vector2 onto)
        {
            Vector2 tl = (-rect.Width / 2, -rect.Height / 2);
            Vector2 tr = (rect.Width / 2, -rect.Height / 2);
            Vector2 bl = (-rect.Width / 2, rect.Height / 2);
            Vector2 br = (rect.Width / 2, rect.Height / 2);

            float tlDist = Vector2.Dot(onto, tl);
            float trDist = Vector2.Dot(onto, tr);
            float blDist = Vector2.Dot(onto, bl);
            float brDist = Vector2.Dot(onto, br);

            float tMax = Math.Max(tlDist, trDist);
            float bMax = Math.Max(blDist, brDist);

            return Math.Max(tMax, bMax);
        }

        public override float[] Draw()
        {
            float rad = MathHelper.DegreesToRadians(rotation);
            float x = MathF.Cos(rad);
            float y = MathF.Sin(rad);
            Vector2 xy = (x, y);
            Vector2 xyPerp = xy.PerpendicularLeft;
            
            float xyProj = MaxProjection(xy);
            float xyPerpProj = MaxProjection(xyPerp);

            Vector2 offset = (rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            Vector2 startL = offset + xy * -xyProj + xyPerp * xyPerpProj;
            Vector2 startR = offset + xy * -xyProj + xyPerp * -xyPerpProj;
            Vector2 endL = offset + xy * xyProj + xyPerp * xyPerpProj;
            Vector2 endR = offset + xy * xyProj + xyPerp * -xyPerpProj;

            return GLVerts.Quad2C(startR, endR, startL, endL, startColor, endColor);
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
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
