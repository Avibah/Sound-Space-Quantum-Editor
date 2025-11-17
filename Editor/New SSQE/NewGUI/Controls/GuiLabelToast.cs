using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Controls
{
    internal class GuiLabelToast : GuiLabel
    {
        private bool showing = false;
        private bool shouldShow = false;

        public GuiLabelToast(float x, float y, float w, float h) : base(x, y, w, h)
        {
            Animator.AddKey("ToastTime", 2);
        }

        public void Show(string text, Color? color = null)
        {
            Text = text;
            TextColor = color ?? Settings.color1.Value;

            shouldShow = true;
            Animator.Stop();
        }

        public override void Reset()
        {
            base.Reset();
            showing = false;
            shouldShow = false;
        }

        public override float[] Draw()
        {
            float toastTime = Animator["ToastTime"] * 2;
            float offset = 1;

            if (toastTime <= 0.5f)
                offset = (float)Math.Sin(toastTime / 0.5f * MathHelper.PiOver2);
            else if (toastTime >= 1.75f)
                offset = (float)Math.Cos((toastTime - 1.75f) / 0.25f * MathHelper.PiOver2);

            Alpha = (float)Math.Min(1, Math.Pow(offset, 3));
            yOffset = -offset * rect.Height;

            return base.Draw();
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            if (showing)
            {
                Animator.Play();
                showing = false;
            }

            // delayed one frame to account for toasts shown immediately after heavy operations
            if (shouldShow)
            {
                showing = true;
                shouldShow = false;
            }

            base.PostRender(mousex, mousey, frametime);
        }
    }
}
