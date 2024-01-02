shader_type canvas_item;

uniform sampler2D noise_texture;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap, repeat_enable;
uniform float distort_speed : hint_range(-5.0, 5.0) = 0.5; 
const float volume = 6.0; 
const float volume1 = 0.3; 
const float volume2 = 0.7; 
const float volume3 = 0.3;

void fragment()
{
	float factor; 
	highp vec4 k = vec4(TIME) * distort_speed;
	k.xy = UV * volume;

	float val1 = length(0.5-fract(k.xyw *= mat3(vec3(-2.0,-1.0,0.0), vec3(3.0,-1.0,1.0), vec3(1.0,-1.0,-1.0)) * volume1)); 
	float val2 = length(0.5-fract(k.xyw *= mat3(vec3(-2.0,-1.0,0.0), vec3(3.0,-1.0,1.0), vec3(1.0,-1.0,-1.0)) * volume2)); 
	float val3 = length(0.5-fract(k.xyw *= mat3(vec3(-2.0,-1.0,0.0), vec3(3.0,-1.0,1.0), vec3(1.0,-1.0,-1.0)) * volume3));
	factor = pow(min(min(val1,val2),val3), 7.0);

	// Get the blurred screen texture input
	vec2 distortedUV = SCREEN_UV + texture(noise_texture, SCREEN_UV + factor).r * 0.05;
	vec4 color = texture(SCREEN_TEXTURE, distortedUV - factor) - factor * 3.0;
	
	COLOR = color;
}