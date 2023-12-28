// NOTE: Shader automatically converted from Godot Engine 3.5.stable.mono's SpatialMaterial.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx,unshaded;
uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color;
uniform float alpha_scissor_threshold;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;

uniform sampler2D gradient_texture;
uniform vec4 gradient_color : source_color;
uniform float gradient_angle : hint_range(0, 6.248) = 0;
uniform float gradient_strength: hint_range(0, 2.0) = 2.0;

void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}

vec2 rotateUV(vec2 uv, vec2 mid, float rotation)
{
    return vec2(
      cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
      cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );
}

void fragment() {
	vec4 albedo_tex = texture(texture_albedo,UV);

	// Get gradient texture
	vec2 new_UV = UV*uv1_scale.xy+uv1_offset.xy;
	vec2 new_pivot = vec2(0.5)*uv1_scale.xy+uv1_offset.xy;
	vec4 gradient = texture(gradient_texture, rotateUV(UV, new_pivot, gradient_angle));
	
	vec3 albedo_color = albedo.rgb * albedo_tex.rgb;
	ALBEDO = mix(albedo_color, gradient_color.rgb, gradient_strength * gradient.x);
	ALPHA = albedo.a * albedo_tex.a;
	ALPHA_SCISSOR_THRESHOLD=alpha_scissor_threshold;
}
