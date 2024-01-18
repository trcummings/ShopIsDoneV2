shader_type spatial;

uniform sampler2D albedo : source_color;
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
	// Grab the color
	vec3 color = texture(albedo, UV).xyz;
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
		
		ALBEDO = color;
	}
	else
	{	
		ALBEDO = color;
	}
}
