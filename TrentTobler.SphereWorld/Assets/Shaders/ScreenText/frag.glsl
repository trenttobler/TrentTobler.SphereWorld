#version 330 core

in vec2 coord;

out vec4 FragColor;

uniform float alpha;
uniform vec2 screenSize;

uniform sampler2D font;
uniform sampler2D screen;
uniform sampler1D colors;

#define FONT_COLS               16
#define FONT_ROWS               6

float glyphPixel(float symbol)
{
    vec2 glyphOffset = fract(coord * screenSize);

    vec2 glyphCell = vec2(
        mod(symbol, FONT_COLS),
        floor(symbol / FONT_COLS));

    vec2 pixelCoord = (glyphCell + glyphOffset) * vec2(
        1.0/FONT_COLS,
        1.0/FONT_ROWS);

    float pixel = texture(font, pixelCoord).x;

    return pixel;
}

vec4 mapColor(float colorIndex)
{
    return texture(colors, (1.0 + 2.0 * colorIndex) / 32.0);
}

void main()
{
    vec2 symbol = round(texture(screen, coord).xy * 255);
    vec4 fgColor = mapColor(floor(symbol.y / 16.0));
    vec4 bgColor = mapColor(mod(symbol.y, 16.0));
    float pixel = glyphPixel(symbol.x);
    vec3 color = mix(bgColor, fgColor, pixel).xyz;
    FragColor =  vec4(color, alpha);
}

