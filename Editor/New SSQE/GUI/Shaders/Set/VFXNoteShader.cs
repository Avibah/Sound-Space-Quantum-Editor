﻿namespace New_SSQE.GUI.Shaders.Set
{
    internal static class VFXNoteShader
    {
        public readonly static string Vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor; // unused but exists for compatibility
layout (location = 2) in vec4 aOffset; // x, y, z, c
                                               
out vec4 vertexColor;

uniform mat4 Transform;
uniform mat4 Projection;
uniform mat4 View;
uniform vec4 NoteColors[32];

uniform vec4 BCSB; // brightness contrast saturation blur
uniform vec3 Tint;
                                                
void main()
{
    // offset + vertex coords
    vec4 worldPos = vec4(aPosition + aOffset.xy, aOffset.z, 1.0f);
    // Transform in this case is an additional x/y/z offset without rotation
    gl_Position = Projection * Transform * View * worldPos;

    // rgb to hsv so color modification can be applied
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

    // Hue
    H = mod(H / 6.0f, 1.0f);
    // Saturation
    float S = clamp(C / Max * BCSB.z * 2.0f, 0.0f, 1.0f);
    // Value
    float V = clamp(Max * BCSB.x * 2.0f, 0.0f, 1.0f);
    
    // now it has to go back to RGB
    int i = int(H * 6.0f);
    float f = H * 6.0f - i;
    float p = V * (1.0f - S);
    float q = V * (1.0f - f * S);
    float t = V * (1.0f - (1.0f - f) * S);

    float r = 0.0f;
    float g = 0.0f;
    float b = 0.0f;

    int ih = int(mod(i, 6.0f));

    if (ih == 0) {
        r = V;
        g = t;
        b = p;
    }
    else if (ih == 1) {
        r = q;
        g = V;
        b = p;
    }
    else if (ih == 2) {
        r = p;
        g = V;
        b = t;
    }
    else if (ih == 3) {
        r = p;
        g = q;
        b = V;
    }
    else if (ih == 4) {
        r = t;
        g = p;
        b = V;
    }
    else {
        r = V;
        g = p;
        b = q;
    }

    color = vec3(r - 0.5f, g - 0.5f, b - 0.5f) * BCSB.y * 2.0f + vec3(0.5f, 0.5f, 0.5f);
    vertexColor = vec4(color * Tint, 1.0f);
}";

        public static string Fragment => MainShader.Fragment;
    }
}