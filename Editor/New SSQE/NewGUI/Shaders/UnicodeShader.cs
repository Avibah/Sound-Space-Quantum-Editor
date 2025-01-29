namespace New_SSQE.NewGUI.Shaders
{
    internal class UnicodeShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition; // 0-1, 0-1
layout (location = 1) in vec4 aCharLayout; // x, y, s, c
layout (location = 2) in float aCharAlpha;

out vec4 texColor;
out vec2 texCoord;

uniform vec2 CharSize;
uniform vec4 TexColor;

uniform mat4 Projection;
                                                
void main()
{
    int yOff = int(aCharLayout.w / 256);
    int xOff = int(aCharLayout.w - yOff * 256);

    float tx = (xOff + 0.02f) / 256.0f + aPosition.x * CharSize.x;
    float ty = (yOff + 0.02f) / 256.0f + aPosition.y * CharSize.y;

    gl_Position = Projection * vec4(aCharLayout.xy + aPosition * aCharLayout.z, 0.0f, 1.0f);

    texColor = vec4(TexColor.xyz, TexColor.w * (1.0f - aCharAlpha));
    texCoord = vec2(tx, ty);
}";

        private const string fragment = @"#version 330 core
out vec4 FragColor;

in vec4 texColor;
in vec2 texCoord;

uniform sampler2D texture0;
                                               
void main()
{
    FragColor = vec4(texColor.xyz, texture(texture0, texCoord).w * texColor.w);
}";

        public UnicodeShader() : base(vertex, fragment) { }
    }
}
