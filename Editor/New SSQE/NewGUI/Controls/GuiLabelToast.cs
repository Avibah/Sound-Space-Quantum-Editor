using New_SSQE.ExternalUtils;
using New_SSQE.NewGUI.Base;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiLabelToast : GuiLabel
    {
        private float toastTime = 0;
        private bool shouldShow = false;

        public GuiLabelToast(float x, float y, float w, float h, Setting<Color>? color = null, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h, color, text, textSize, font, centerMode)
        {
            
        }

        public void Show(string text, Color? color = null)
        {
            this.text = text;
            textColor = color ?? Color.White;

            shouldShow = true;
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            float prevAlpha = toastTime;
            toastTime = MathHelper.Clamp(toastTime + frametime, 0, 2);
            
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
                
                alpha = (float)Math.Min(1, Math.Pow(offset, 3));
                yOffset = -offset * rect.Height;
                Update();
            }

            base.PostRender(mousex, mousey, frametime);
        }
    }
}
