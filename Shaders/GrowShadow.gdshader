// NOTE: Shader automatically converted from Godot Engine 3.5.stable.mono's SpatialMaterial.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_front,diffuse_burley,specular_schlick_ggx;

uniform sampler2D noise_img;
uniform float alpha_amount : hint_range(0, 1) = 1;

uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color;
uniform float specular;
uniform float metallic;
uniform float grow;
uniform float roughness : hint_range(0,1);
uniform float point_size : hint_range(0,128);
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;


void vertex() {
	UV=UV*uv1_scale.xy + uv1_offset.xy;
	VERTEX+=NORMAL*grow;
	VERTEX.x += 0.3 * sin(texture(noise_img, UV).r * TIME);
	VERTEX.y += 0.3 * cos(texture(noise_img, UV).g * TIME);
	VERTEX.z -= 0.3 * texture(noise_img, UV).b * (sin(TIME) + cos(TIME));
}

void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	
	// Noise
	vec2 uv1 = vec2(UV.x + TIME * 0.5, UV.y);
	vec2 uv2 = vec2(UV.x - TIME * 0.5, UV.y);
	vec2 uv3 = vec2(UV.x, TIME * 0.5 + UV.y);
	
	float noise_r = texture(noise_img, uv1).r;
	float noise_g = texture(noise_img, uv2).g;
	float noise_b = texture(noise_img, uv3).b;
	
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	ALPHA = (noise_r * noise_g * noise_b) * 5.0 * alpha_amount;
}
