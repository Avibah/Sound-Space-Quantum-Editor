using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Shaders
{
    internal class FBOTextureShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoords;

void main()
{
    gl_Position = vec4(aPosition, 0.0f, 1.0f);
    texCoords = aTexCoord;
}";

        private const string fragment = @"#version 330 core
out vec4 FragColor;
in vec2 texCoords;

uniform sampler2D texture0;

uniform float blur;

const float kernel[9] = float[](
    1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f,
    2.0f / 16.0f, 4.0f / 16.0f, 2.0f / 16.0f,
    1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f
);

void main()
{
    vec2 offsets[9] = vec2[](
        vec2(-blur, blur),
        vec2(0.0f, blur),
        vec2(blur, blur),
        vec2(-blur, 0.0f),
        vec2(0.0f, 0.0f),
        vec2(blur, 0.0f),
        vec2(-blur, -blur),
        vec2(0.0f, -blur),
        vec2(blur, -blur)
    );

    vec3 samples[9];
    for (int i = 0; i < 9; i++)
        samples[i] = vec3(texture(texture0, texCoords.st + offsets[i]));

    vec3 col = vec3(0.0f);
    for (int i = 0; i < 9; i++)
        col += samples[i] * kernel[i];

    FragColor = vec4(col, 1.0f);
}";

        public FBOTextureShader() : base(vertex, fragment) { }
    }
}
