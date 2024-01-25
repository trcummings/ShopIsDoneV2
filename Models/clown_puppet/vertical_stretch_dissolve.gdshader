// NOTE: Shader automatically converted from Godot Engine 3.5.stable.mono's SpatialMaterial.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx,unshaded;
uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color;

uniform float distortion : hint_range(0, 5);
uniform sampler2D noise_texture;


void vertex() {
	float height = texture(noise_texture, VERTEX.xz / 2.0).x; //divide by the size of the PlaneMesh
	VERTEX.y += height * distortion;
	VERTEX.x /= (distortion + 1.0);
}

void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo, UV);
	vec4 dissolve_tex = texture(noise_texture, UV);
	
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	float base_alpha = albedo.a * albedo_tex.a;
	ALPHA = mix(base_alpha, 0, distortion);
}
