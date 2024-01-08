shader_type spatial;
render_mode blend_mix,cull_back,diffuse_burley,specular_schlick_ggx;

uniform vec4 main_color : source_color = vec4(1, 1, 1, 0.5);
uniform vec4 intersection_color : source_color = vec4(1, 1, 1, 0.5);
uniform float intersection_threshold_max = 5; // Full object at 0, slim rim at 100
uniform sampler2D displacement_texture;
uniform float displacement_amount;
uniform sampler2D albedo_texture;
uniform float metallic = 1.0;
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;

varying vec4 screen_pos;

void vertex() {
	// calculate screen position
	vec4 vert = vec4(VERTEX, 1.0);
	vert = MODELVIEW_MATRIX * vert;
	vert = PROJECTION_MATRIX * vert;
	screen_pos = vert;
}

void fragment() {
	// Take the screen space coordinates and transform them into normalized 
	// device coordinates
	float depth = texture(DEPTH_TEXTURE, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
	
	// Get linear depth of this specific pixel
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;
	
	// modulate displacement UV across time
	vec2 disp_uv = UV - (TIME / 5.0);
	vec2 displacement = texture(displacement_texture, disp_uv).xy;
	displacement = ((displacement * 2.0) - vec2(1.0)) * displacement_amount;
	
	// Get a value from 0 to 1 based on how big the difference between the pixel's depth and
	// its screen position is
	float diff = clamp((intersection_threshold_max * (linear_depth - screen_pos.w)) + displacement.y, 0.0, 1.0);
	
	// Set the color based on an interpolation between the two given colors
	vec4 color = mix(intersection_color, main_color, step(0.5, diff));
	
	ALBEDO.rgb = color.rgb;
	ALPHA = color.a * texture(albedo_texture, UV).a;
	METALLIC = metallic;
}