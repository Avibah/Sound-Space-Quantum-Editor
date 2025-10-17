using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiLabelToast : GuiLabel
    {
        private float toastTime = 0;
        private bool shouldShow = false;

        public GuiLabelToast(float x, float y, float w, float h) : base(x, y, w, h) { }

        public void Show(string text, Color? color = null)
        {
            Text = text;
            TextColor = color ?? Color.White;

            shouldShow = true;
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            float prevAlpha = toastTime;
            toastTime = Math.Clamp(toastTime + frametime, 0, 2);
            
            if (shouldShow)
            {
                toastTime = 0;
                shouldShow = false;
            }

            if (prevAlpha != toastTime)
            {
                float offset = 1;

                if (toastTime <= 0.5f)
                    offset = (float)Math.Sin(toastTime / 0.5f * MathHelper.PiOver2);
                else if (toastTime >= 1.75f)
                    offset = (float)Math.Cos((toastTime - 1.75f) / 0.25f * MathHelper.PiOver2);
                
                Alpha = (float)Math.Min(1, Math.Pow(offset, 3));
                yOffset = -offset * rect.Height;
                Update();
            }

            base.PostRender(mousex, mousey, frametime);
        }
    }
}
