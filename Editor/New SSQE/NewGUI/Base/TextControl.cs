using New_SSQE.NewGUI.Font;
using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal enum CenterMode
    {
        None,
        X,
        Y,
        XY = 3
    }

    internal abstract class TextControl : Control
    {
        protected string text;
        protected int textSize;
        protected string font;
        protected float alpha = 1;
        protected Color textColor = Color.White;

        public CenterMode CenterMode = CenterMode.X;
        private Vector4[] verts = [];

        private float textX;
        private float textY;
        protected float xOffset = 0;
        protected float yOffset = 0;

        private bool shouldUpdate = false;

        public string Text
        {
            get => text;
            set
            {
                if (value != text)
                {
                    text = value;
                    shouldUpdate = true;
                }
            }
        }

        public float TextScale => Math.Min(rect.Width / startRect.Width, rect.Height / startRect.Height);

        public int TextSize
        {
            get => (int)(textSize * TextScale);
            set
            {
                if (value != textSize)
                {
                    textSize = value;
                    shouldUpdate = true;
                }
            }
        }
        
        public string Font
        {
            get => font;
            set
            {
                if (value != font)
                {
                    font = value;
                    shouldUpdate = true;
                }
            }
        }

        public TextControl(float x, float y, float w, float h, string text = "", int textSize = 0, string font = "main", CenterMode centerMode = CenterMode.XY) : base(x, y, w, h)
        {
            this.text = text;
            this.textSize = textSize;
            this.font = font;

            CenterMode = centerMode;
        }

        public override void Update()
        {
            base.Update();

            textX = rect.X + xOffset;
            textY = rect.Y + yOffset;

            if (CenterMode.HasFlag(CenterMode.X))
            {
                float width = FontRenderer.GetWidth(text, TextSize, font);
                textX = rect.X + rect.Width / 2 - width / 2 + xOffset;
            }
            if (CenterMode.HasFlag(CenterMode.Y))
            {
                float height = FontRenderer.GetHeight(TextSize, font) * text.Split('\n').Length;
                textY = rect.Y + rect.Height / 2 - height / 2 + yOffset;
            }

            verts = FontRenderer.Print(textX, textY, text, TextSize, font);
        }

        public override void PreRender(float mousex, float mousey, float frametime)
        {
            base.PreRender(mousex, mousey, frametime);
            if (shouldUpdate)
                Update();
        }

        public override void PostRender(float mousex, float mousey, float frametime)
        {
            base.PostRender(mousex, mousey, frametime);
            if (verts.Length == 0)
                return;

            float[] a = new float[verts.Length];
            Array.Fill(a, 1 - alpha);

            FontRenderer.SetActive(font);
            FontRenderer.SetColor(textColor);
            FontRenderer.RenderData(font, verts, a);
        }

        public virtual void SetText(string? text = null, int? textSize = null, string? font = null)
        {
            string prevText = this.text;
            int prevTextSize = this.textSize;
            string prevFont = this.font;

            this.text = text ?? this.text;
            this.textSize = textSize ?? this.textSize;
            this.font = font ?? this.font;

            if (prevText != this.text || prevTextSize != this.textSize || prevFont != this.font)
                shouldUpdate = true;
        }

        public virtual void SetColor(Color? color = null, float? alpha = null)
        {
            textColor = color ?? textColor;
            this.alpha = alpha ?? this.alpha;
        }
    }
}
