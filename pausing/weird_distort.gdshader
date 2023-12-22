shader_type canvas_item;

uniform sampler2D noise_texture;
uniform float distort_strength: hint_range(0.0, 1.0) = 0.0;
uniform float lod: hint_range(0.0, 5) = 0.0;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float distort_factor : hint_range(0.0, 1.0) = 0.5;
const float speed = 0.5; 
const float volume = 6.0; 
const float volume1 = 0.3; 
const float volume2 = 0.7; 
const float volume3 = 0.3;

void fragment()
{	
	float factor; 
	highp vec4 k = vec4(TIME) * speed;
	k.xy = UV * volume; 

	float val1 = length(0.5-fract(k.xyw *= mat3(vec3(-2.0,-1.0,0.0), vec3(3.0,-1.0,1.0), vec3(1.0,-1.0,-1.0)) * volume1)); 
	float val2 = length(0.5-fract(k.xyw *= mat3(vec3(-2.0,-1.0,0.0), vec3(3.0,-1.0,1.0), vec3(1.0,-1.0,-1.0)) * volume2)); 
	float val3 = length(0.5-fract(k.xyw *= mat3(vec3(-2.0,-1.0,0.0), vec3(3.0,-1.0,1.0), vec3(1.0,-1.0,-1.0)) * volume3));
	factor = pow(min(min(val1,val2),val3), 7.0);

	// Get the blurred screen texture input
	vec4 color = texture(TEXTURE, UV - factor, lod) - factor * 3.0;
	vec4 distorted_color = texture(noise_texture, UV + factor, lod) + factor * 3.0;
	
	COLOR = mix(color, distorted_color, distort_strength * distort_factor);
}