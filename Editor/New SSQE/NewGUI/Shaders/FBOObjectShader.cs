using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Shaders
{
    internal class FBOObjectShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor; // unused but exists for compatibility
layout (location = 2) in vec4 aOffset; // x, y, z, c
                                               
out vec4 vertexColor;

uniform mat4 Projection;
uniform mat4 View;
uniform mat4 Transform;
uniform vec4 NoteColors[32];

uniform float Brightness;
uniform float Contrast;
uniform float Saturation;
uniform vec3 Tint;
                                                
void main()
{
    vec4 worldPos = vec4(aPosition + aOffset.xy, aOffset.z, 1.0f);
    gl_Position = Projection * Transform * View * worldPos;

    vec3 color = NoteColors[int(aOffset.w)].xyz;

    float Max = max(color.x, max(color.y, color.z));
    float Min = min(color.x, min(color.y, color.z));
    float C = Max - Min;

    float H = 0.0f;

    if (C != 0.0f) {
        float R = (Max - color.x) / C;
        float G = (Max - color.y) / C;
        float B = (Max - color.z) / C;

        if (Max == color.x) {
            H = B - G;
        }
        else if (Max == color.y) {
            H = 2.0f + R - B;
        }
        else {
            H = 4.0f + G - R;
        }
    }

    H = mod(H / 6.0f, 1.0f);
    float S = clamp(C / Max * Saturation * 2.0f, 0.0f, 1.0f);
    float V = clamp(Max * Brightness * 2.0f, 0.0f, 1.0f);
    
    float h6 = 6.0f * H;
    float r = abs(h6 - 3.0f) - 1.0f;
    float g = 2.0f - abs(h6 - 2.0f);
    float b = 2.0f - abs(h6 - 4.0f);

    float is = 1.0f - S;
    r = V * (S * clamp(r, 0.0f, 1.0f) + is);
    g = V * (S * clamp(g, 0.0f, 1.0f) + is);
    b = V * (S * clamp(b, 0.0f, 1.0f) + is);

    color = vec3(r - 0.5f, g - 0.5f, b - 0.5f) * Contrast * 2.0f + vec3(0.5f, 0.5f, 0.5f);
    vertexColor = vec4(color * Tint, 1.0f);
}";

        public FBOObjectShader() : base(vertex) { }
    }
}
