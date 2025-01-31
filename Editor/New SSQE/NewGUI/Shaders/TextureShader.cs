using New_SSQE.NewGUI.Base;

namespace New_SSQE.NewGUI.Shaders
{
    internal class TextureShader : Shader
    {
        private const string vertex = @"#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aAlpha;

out vec2 texCoord;
out float alpha;

uniform mat4 Projection;
                                                
void main()
{
    gl_Position = Projection * vec4(aPosition, 0.0f, 1.0f);
    texCoord = aTexCoord;
    alpha = aAlpha;
}";

        private const string fragment = @"#version 330 core
out vec4 FragColor;
in vec2 texCoord;
in float alpha;

uniform sampler2D texture0;
                                                
void main()
{
    vec4 color = texture(texture0, texCoord);
    FragColor = vec4(color.xyz, color.w * alpha);
}";

        public TextureShader() : base(vertex, fragment) { }

    }
}
