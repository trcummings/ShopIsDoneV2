shader_type canvas_item;

uniform float wave_frequency = 3.0;
uniform float wave_amplitude = 0.03;
uniform float intensity : hint_range(.0, 1., .1) = .5;
uniform sampler2D screen_texture : hint_screen_texture;

void fragment()
{
    //Main distortion   (wavyness)
    float time1 = TIME * 0.6;
	vec2 uv = UV;
	vec2 modded_uv = UV;
	modded_uv.x += sin(time1 + modded_uv.y * wave_frequency) * wave_amplitude;
	modded_uv.y += sin(time1 + modded_uv.x * wave_frequency) * wave_amplitude;
        
    vec2 differenceInUV = modded_uv - uv;
	vec4 surfaceColor = texture(TEXTURE, uv + differenceInUV * intensity);

	COLOR.rgb = surfaceColor.rgb;
}