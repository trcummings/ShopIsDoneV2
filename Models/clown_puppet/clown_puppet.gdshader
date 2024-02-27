// NOTE: Shader automatically converted from Godot Engine 4.2.1.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
uniform sampler2D texture_albedo : source_color,filter_linear_mipmap,repeat_enable;
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear_mipmap,repeat_enable;
// Affine
uniform bool affine_mapping = false;
uniform float alpha_scissor : hint_range(0, 1) = 0.5;
uniform float jitter: hint_range(0, 1) = 0.25;
uniform ivec2 resolution = ivec2(320, 240);
// Vertical stretch
uniform float distortion : hint_range(0, 1);
uniform sampler2D noise_texture;

// For affine
vec4 snap_to_position(vec4 base_position, float jitter_val)
{
	vec4 snapped_position = base_position;
	snapped_position.xyz = base_position.xyz / base_position.w;
	
	vec2 snap_resolution = floor(vec2(resolution) * (1.0 - jitter_val));
	snapped_position.x = floor(snap_resolution.x * snapped_position.x) / snap_resolution.x;
	snapped_position.y = floor(snap_resolution.y * snapped_position.y) / snap_resolution.y;
	
	snapped_position.xyz *= base_position.w;
	return snapped_position;
}

void vertex() {
		
	// Vertical stretch
	float height = texture(noise_texture, VERTEX.xz * 2.).x; //divide by the size of the PlaneMesh
	VERTEX.y += height * distortion;
	VERTEX.x /= (distortion + 1.0);

	// Affine
	// Get distance from camera to vertex 
	vec3 a = CAMERA_POSITION_WORLD * VERTEX;
	float d = distance(a.xyz, CAMERA_POSITION_WORLD);
	// Adjust jitter by a ratio of distance to camera
	float adjusted_jitter = clamp(smoothstep(jitter, 0, d / 5.), 0, .5);
	// Calculate snapped vertex position for affine mapping shader
	vec4 to_position = PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0);
	vec4 snapped_position = snap_to_position(to_position, adjusted_jitter);
	if (affine_mapping)
	{
		POSITION = snapped_position;
		POSITION /= abs(POSITION.w);
	}
	else
	{
		POSITION = snapped_position;
	}
}


void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
	float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;
	
	// Vertical dissolve
	vec4 dissolve_tex = texture(noise_texture, UV);
	if (dissolve_tex.r < distortion) discard;
	
	// Basics
	ALPHA = albedo_tex.a;
	ALPHA_SCISSOR_THRESHOLD = alpha_scissor;
	ALBEDO = albedo_tex.rgb;
}
