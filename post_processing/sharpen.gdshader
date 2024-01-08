shader_type spatial;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture;
uniform float SHARPEN_FACTOR = 16.0;

void vertex() {
	POSITION = vec4(VERTEX, 1);
}

vec4 sharpen_mask(sampler2D tex, vec2 frag_coord, vec2 texel_size)
{
    // Sharpen detection matrix [0,1,0],[1,-4,1],[0,1,0]
    // Colors
    vec4 up = texture(tex, (frag_coord + vec2 (0, 1)) / texel_size);
    vec4 left = texture(tex, (frag_coord + vec2 (-1, 0))/texel_size);
    vec4 center = texture(tex, frag_coord / texel_size);
    vec4 right = texture(tex, (frag_coord + vec2 (1, 0))/texel_size);
    vec4 down = texture(tex, (frag_coord + vec2 (0, -1))/texel_size);
    
    // Return edge detection
    return (1.0 + 4.0 * SHARPEN_FACTOR) * center -SHARPEN_FACTOR * (up + left + right + down);
}

void fragment() {
	vec2 texel_size = VIEWPORT_SIZE;
	ALBEDO = sharpen_mask(screen_texture, FRAGCOORD.xy, texel_size).rbg;
}
