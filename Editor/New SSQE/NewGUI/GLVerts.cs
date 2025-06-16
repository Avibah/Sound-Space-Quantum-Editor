using OpenTK.Mathematics;
using System.Drawing;

namespace New_SSQE.NewGUI
{
    internal class GLVerts
    {
        public static float[] Rect(float x, float y, float w, float h, float r, float g, float b, float a = 1)
        {
            return
            [
                x, y, r, g, b, a,
                x + w, y, r, g, b, a,
                x, y + h, r, g, b, a,

                x + w, y + h, r, g, b, a,
                x, y + h, r, g, b, a,
                x + w, y, r, g, b, a
            ];
        }

        public static float[] Rect(RectangleF rect, float r, float g, float b, float a = 1)
            => Rect(rect.X, rect.Y, rect.Width, rect.Height, r, g, b, a);
        public static float[] Rect(float x, float y, float w, float h, Color color, float a = 1)
            => Rect(x, y, w, h, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);
        public static float[] Rect(RectangleF rect, Color color, float a = 1)
            => Rect(rect.X, rect.Y, rect.Width, rect.Height, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);



        public static float[] Outline(float x, float y, float w, float h, float lw, float r, float g, float b, float a = 1)
        {
            x -= lw / 2;
            y -= lw / 2;

            return
            [
                x, y, r, g, b, a,
                x + w, y, r, g, b, a,
                x, y + lw, r, g, b, a,

                x + w, y + lw, r, g, b, a,
                x, y + lw, r, g, b, a,
                x + w, y, r, g, b, a,

                x + w + lw, y, r, g, b, a,
                x + w + lw, y + h, r, g, b, a,
                x + w, y, r, g, b, a,

                x + w, y + h, r, g, b, a,
                x + w, y, r, g, b, a,
                x + w + lw, y + h, r, g, b, a,

                x + w + lw, y + h + lw, r, g, b, a,
                x + lw, y + h + lw, r, g, b, a,
                x + w + lw, y + h, r, g, b, a,

                x + lw, y + h, r, g, b, a,
                x + w + lw, y + h, r, g, b, a,
                x + lw, y + h + lw, r, g, b, a,

                x, y + h + lw, r, g, b, a,
                x, y + lw, r, g, b, a,
                x + lw, y + h + lw, r, g, b, a,

                x + lw, y + lw, r, g, b, a,
                x + lw, y + h + lw, r, g, b, a,
                x, y + lw, r, g, b, a
            ];
        }

        public static float[] Outline(RectangleF rect, float lw, float r, float g, float b, float a = 1)
            => Outline(rect.X, rect.Y, rect.Width, rect.Height, lw, r, g, b, a);
        public static float[] Outline(float x, float y, float w, float h, float lw, Color color, float a = 1)
            => Outline(x, y, w, h, lw, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);
        public static float[] Outline(RectangleF rect, float lw, Color color, float a = 1)
            => Outline(rect.X, rect.Y, rect.Width, rect.Height, lw, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);



        public static float[] Texture(float x, float y, float w, float h, float tx = 0, float ty = 0, float tw = 1, float th = 1, float a = 1)
        {
            return
            [
                x, y, tx, ty, a,
                x + w, y, tx + tw, ty, a,
                x, y + h, tx, ty + th, a,

                x + w, y + h, tx + tw, ty + th, a,
                x, y + h, tx, ty + th, a,
                x + w, y, tx + tw, ty, a
            ];
        }

        public static float[] TextureWithoutAlpha(float x, float y, float w, float h, float tx = 0, float ty = 0, float tw = 1, float th = 1)
        {
            return
            [
                x, y, tx, ty,
                x + w, y, tx + tw, ty,
                x, y + h, tx, ty + th,

                x + w, y + h, tx + tw, ty + th,
                x, y + h, tx, ty + th,
                x + w, y, tx + tw, ty
            ];
        }

        public static float[] Icon(float x, float y, float w, float h)
        {
            return
            [
                x, y, 0f, 0f, 0, 0,
                x + w, y, 1f, 0f, 0, 0,
                x, y + h, 0f, 1f, 0, 0,

                x + w, y + h, 1f, 1f, 0, 0,
                x, y + h, 0f, 1f, 0, 0,
                x + w, y, 1f, 0f, 0, 0
            ];
        }



        public static float[] Line(float x1, float y1, float x2, float y2, float lw, float r, float g, float b, float a = 1)
        {
            Vector2 start = (x1, y1);
            Vector2 end = (x2, y2);
            Vector2 dir = end - start;
            dir.Normalize();

            Vector2 startL = start + dir.PerpendicularLeft * lw / 2;
            Vector2 startR = start + dir.PerpendicularRight * lw / 2;
            Vector2 endL = end + dir.PerpendicularLeft * lw / 2;
            Vector2 endR = end + dir.PerpendicularRight * lw / 2;
            
            return
            [
                startL.X, startL.Y, r, g, b, a,
                endL.X, endL.Y, r, g, b, a,
                endR.X, endR.Y, r, g, b, a,

                endR.X, endR.Y, r, g, b, a,
                startR.X, startR.Y, r, g, b, a,
                startL.X, startL.Y, r, g, b, a
            ];
        }

        public static float[] Line(float x1, float y1, float x2, float y2, float lw, Color color, float a = 1)
            => Line(x1, y1, x2, y2, lw, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);



        public static float[] Polygon(float x, float y, float radius, int sides, float angle, float r, float g, float b, float a = 1, float angleMult = 1, float angleStart = 0)
        {
            Vector2[] vertices = new Vector2[sides + 1];
            double rad = MathHelper.DegreesToRadians(angle);
            double radStart = MathHelper.DegreesToRadians(angleStart);

            for (int i = 0; i < sides + 1; i++)
            {
                double ar = rad + i / (float)sides * Math.PI * 2 * angleMult + radStart;
                double vx = Math.Cos(ar) * radius;
                double vy = -Math.Sin(ar) * radius;

                vertices[i] = ((float)vx + x, (float)vy + y);
            }

            float[] final = new float[sides * 18];
            int index = 0;

            for (int i = 0; i < sides; i++)
            {
                int next = i + 1;

                final[index++] = vertices[i].X;
                final[index++] = vertices[i].Y;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = x;
                final[index++] = y;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = vertices[next].X;
                final[index++] = vertices[next].Y;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;
            }

            return final;
        }

        public static float[] Polygon(float x, float y, float radius, int sides, float angle, Color color, float alpha = 1, float angleMult = 1, float angleStart = 0)
            => Polygon(x, y, radius, sides, angle, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * alpha, angleMult, angleStart);



        public static float[] PolygonOutline(float x, float y, float radius, float width, int sides, float angle, float r, float g, float b, float a = 1, float angleMult = 1)
        {
            Vector4[] vertices = new Vector4[sides + 1];
            double rad = MathHelper.DegreesToRadians(angle);

            for (int i = 0; i < sides + 1; i++)
            {
                double ar = rad + i / (float)sides * Math.PI * 2 * angleMult;
                float vx1 = (float)Math.Cos(ar) * (radius + width / 2);
                float vy1 = (float)-Math.Sin(ar) * (radius + width / 2);

                float vx2 = (float)Math.Cos(ar) * (radius - width / 2);
                float vy2 = (float)-Math.Sin(ar) * (radius - width / 2);

                vertices[i] = (vx1 + x, vy1 + y, vx2 + x, vy2 + y);
            }

            float[] final = new float[sides * 36];
            int index = 0;

            for (int i = 0; i < sides; i++)
            {
                int next = i + 1;

                final[index++] = vertices[i].X;
                final[index++] = vertices[i].Y;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = vertices[i].Z;
                final[index++] = vertices[i].W;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = vertices[next].X;
                final[index++] = vertices[next].Y;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = vertices[next].X;
                final[index++] = vertices[next].Y;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = vertices[next].Z;
                final[index++] = vertices[next].W;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;

                final[index++] = vertices[i].Z;
                final[index++] = vertices[i].W;
                final[index++] = r;
                final[index++] = g;
                final[index++] = b;
                final[index++] = a;
            }

            return final;
        }

        public static float[] PolygonOutline(float x, float y, float radius, float width, int sides, float angle, Color color, float a = 1, float angleMult = 1)
            => PolygonOutline(x, y, radius, width, sides, angle, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a, angleMult);



        public static float[] Squircle(float x, float y, float w, float h, int sides, float radius, float r, float g, float b, float a = 1)
        {
            float min = Math.Min(w, h);
            radius = Math.Min(radius * min, min / 2);

            float innerXLeft = x + radius;
            float innerXRight = x + w - radius;
            float innerYTop = y + radius;
            float innerYBottom = y + h - radius;

            float[] left = Rect(x, innerYTop, radius, h - 2 * radius, r, g, b, a);
            float[] right = Rect(innerXRight, innerYTop, radius, h - 2 * radius, r, g, b, a);
            float[] center = Rect(innerXLeft, y, w - 2 * radius, h, r, g, b, a);

            float[] topLeft = Polygon(innerXLeft, innerYTop, radius, sides, 90, r, g, b, a, 0.25f);
            float[] topRight = Polygon(innerXRight, innerYTop, radius, sides, 0, r, g, b, a, 0.25f);
            float[] bottomLeft = Polygon(innerXLeft, innerYBottom, radius, sides, 180, r, g, b, a, 0.25f);
            float[] bottomRight = Polygon(innerXRight, innerYBottom, radius, sides, 270, r, g, b, a, 0.25f);

            return [..left, ..right, ..center, ..topLeft, ..topRight, ..bottomLeft, ..bottomRight];
        }

        public static float[] Squircle(RectangleF rect, int sides, float radius, float r, float g, float b, float a = 1)
            => Squircle(rect.X, rect.Y, rect.Width, rect.Height, sides, radius, r, g, b, a);
        public static float[] Squircle(float x, float y, float w, float h, int sides, float radius, Color color, float a = 1)
            => Squircle(x, y, w, h, sides, radius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);
        public static float[] Squircle(RectangleF rect, int sides, float radius, Color color, float a = 1)
            => Squircle(rect.X, rect.Y, rect.Width, rect.Height, sides, radius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);



        public static float[] SquircleOutline(float x, float y, float w, float h, float lw, int sides, float radius, float r, float g, float b, float a = 1)
        {
            float min = Math.Min(w, h);
            radius = Math.Min(radius * min, min / 2);

            float innerXLeft = x + radius;
            float innerXRight = x + w - radius;
            float innerYTop = y + radius;
            float innerYBottom = y + h - radius;

            float[] left = Line(x, innerYTop, x, innerYBottom, lw, r, g, b, a);
            float[] right = Line(x + w, innerYTop, x + w, innerYBottom, lw, r, g, b, a);
            float[] top = Line(innerXLeft, y, innerXRight, y, lw, r, g, b, a);
            float[] bottom = Line(innerXLeft, y + h, innerXRight, y + h, lw, r, g, b, a);

            float[] topLeft = PolygonOutline(innerXLeft, innerYTop, radius, lw, sides, 90, r, g, b, a, 0.25f);
            float[] topRight = PolygonOutline(innerXRight, innerYTop, radius, lw, sides, 0, r, g, b, a, 0.25f);
            float[] bottomLeft = PolygonOutline(innerXLeft, innerYBottom, radius, lw, sides, 180, r, g, b, a, 0.25f);
            float[] bottomRight = PolygonOutline(innerXRight, innerYBottom, radius, lw, sides, 270, r, g, b, a, 0.25f);

            return [..left, ..right, ..top, ..bottom, ..topLeft, ..topRight, ..bottomLeft, ..bottomRight];
        }

        public static float[] SquircleOutline(RectangleF rect, float lw, int sides, float radius, float r, float g, float b, float a = 1)
            => SquircleOutline(rect.X, rect.Y, rect.Width, rect.Height, lw, sides, radius, r, g, b, a);
        public static float[] SquircleOutline(float x, float y, float w, float h, float lw, int sides, float radius, Color color, float a = 1)
            => SquircleOutline(x, y, w, h, lw, sides, radius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);
        public static float[] SquircleOutline(RectangleF rect, float lw, int sides, float radius, Color color, float a = 1)
            => SquircleOutline(rect.X, rect.Y, rect.Width, rect.Height, lw, sides, radius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f * a);
    }
}
