shader_type canvas_item;

uniform sampler2D noise_img;
uniform float speed = 1.0;
uniform vec4 smoke_color : source_color;
uniform float intensity = 5.0;

void fragment() {
	// Break out three separate UVs
	float speed_mod = TIME * speed;
	vec2 uv1 = vec2(UV.x + speed_mod, UV.y);
	vec2 uv2 = vec2(UV.x - speed_mod, UV.y);
	vec2 uv3 = vec2(UV.x, UV.y + speed_mod);
	
	float noise_r = texture(noise_img, uv1).r;
	float noise_g = texture(noise_img, uv2).g;
	float noise_b = texture(noise_img, uv3).b;
	
	vec3 new_color = vec3(noise_r, noise_g, noise_b);
	float new_alpha = noise_r * noise_g * noise_b;
	
	COLOR.rbg = smoke_color.rgb;
	COLOR.a = clamp(new_alpha * intensity * texture(TEXTURE, UV).a, 0.0, 1.0);
}