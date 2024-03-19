shader_type canvas_item;

uniform sampler2D overlay_texture;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float wipe_speed = 1.0;
uniform float dissolve_value : hint_range(0.,1., .01) = .5;
uniform float time : hint_range(0.,2., .01) = 0;

void fragment() {	
	// Get the screen texture
	vec4 screen = texture(TEXTURE, UV);
	
	// Get modded time value
	float t = time * wipe_speed;
	
	// Scroll UV horizontally across the screen
	vec2 offset = UV - vec2(mod(t, 2.), 0.) + vec2(1., 0);
	vec4 noise_texture = texture(overlay_texture, offset);
	
	float threshold = smoothstep(noise_texture.x - dissolve_value, noise_texture.x, dissolve_value);
	float border = smoothstep(noise_texture.x, noise_texture.x + dissolve_value, dissolve_value);
	
	COLOR.a *= threshold;
	COLOR = vec4(screen.rgb, border);
}
