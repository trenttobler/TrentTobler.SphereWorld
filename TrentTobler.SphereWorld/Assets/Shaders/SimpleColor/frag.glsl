#version 330 core

out vec4 FragColor;

in vec2 texCoord;
in vec4 vertexColor;

uniform sampler2D texImage;

void main()
{
    vec4 texPixel = texture(texImage, texCoord);
    FragColor = texPixel * vertexColor;
}