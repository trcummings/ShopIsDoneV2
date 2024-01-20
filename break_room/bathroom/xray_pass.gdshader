shader_type spatial;

render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color,filter_linear_mipmap,repeat_enable;
uniform float point_size : hint_range(0,128);
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_metallic : hint_default_white,filter_linear_mipmap,repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear_mipmap,repeat_enable;
uniform float specular;
uniform float metallic;
uniform sampler2D texture_normal : hint_roughness_normal,filter_linear_mipmap,repeat_enable;
uniform float normal_scale : hint_range(-16,16);
uniform sampler2D texture_ambient_occlusion : hint_default_white, filter_linear_mipmap,repeat_enable;
uniform vec4 ao_texture_channel;
uniform float ao_light_affect;

global uniform vec3 leader_pos;
global uniform float camera_to_leader_distance;
global uniform float camera_zoom;

float sd_vertical_capsule( vec3 pos, float height, float radius )
{
  pos.y -= clamp( pos.y, 0.0, height );
  return length( pos ) - radius;
}

float get_limit(vec2 fragcoord)
{
	int x = int(fragcoord.x) % 4;
	int y = int(fragcoord.y) % 4;
	int index = x + y * 4;
	float limit = 0.0;
	
	if (x < 8) {
	    if (index == 0) limit = 0.0625;
	    if (index == 1) limit = 0.5625;
	    if (index == 2) limit = 0.1875;
	    if (index == 3) limit = 0.6875;
	    if (index == 4) limit = 0.8125;
	    if (index == 5) limit = 0.3125;
	    if (index == 6) limit = 0.9375;
	    if (index == 7) limit = 0.4375;
	    if (index == 8) limit = 0.25;
	    if (index == 9) limit = 0.75;
	    if (index == 10) limit = 0.125;
	    if (index == 11) limit = 0.625;
	    if (index == 12) limit = 1.0;
	    if (index == 13) limit = 0.5;
	    if (index == 14) limit = 0.875;
	    if (index == 15) limit = 0.375;
	}
				
	return limit;
}

void fragment() {
	// Sweep from the camera, if we're outside that sweep, we're fair game
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float dist_from_vert_to_camera = distance(CAMERA_POSITION_WORLD, world_vertex);

	if (camera_to_leader_distance > dist_from_vert_to_camera + 1.5)
	{
		// Get the distance of this vertex to a point in a shape in clip space
		vec3 view_leader = (VIEW_MATRIX * vec4(leader_pos, 1.0)).xyz;
		vec3 clip_leader = (PROJECTION_MATRIX * vec4(view_leader, 1.0)).xyz;
		vec3 clip_point = (PROJECTION_MATRIX * vec4(VERTEX, 1.0)).xyz;
		float zoom_mod = 1. / camera_zoom;
		float clip_sdf = sd_vertical_capsule(clip_leader - clip_point, 0.225 * zoom_mod, 0.1 * zoom_mod);
		
		if (clip_sdf <= 0.)
		{
			float band_width = 0.02;
			if (clip_sdf > -band_width)
			{
				float limit = get_limit(FRAGCOORD.xy * 0.5);
				if (0.1 < limit) discard;
			}
			else discard;
		}
	}

	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo, base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
	float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;
	SPECULAR = specular;
	NORMAL_MAP = texture(texture_normal,base_uv).rgb;
	NORMAL_MAP_DEPTH = normal_scale;
	AO = dot(texture(texture_ambient_occlusion,base_uv),ao_texture_channel);
	AO_LIGHT_AFFECT = ao_light_affect;
}
