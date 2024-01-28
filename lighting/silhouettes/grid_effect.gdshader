shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform vec4 chroma_key:source_color = vec4(0.0, 1.0, 1.0, 1.0);

// Grid
uniform sampler2D grid_texture;
uniform float grid_fade = 0.0;
uniform vec2 grid_point;
uniform float grid_unit_size = 3.5;

// Sillhouettes
uniform vec4 object_sillhouette_color:source_color = vec4(0.25, 0.25, 0.25, 1.0);
uniform vec4 pawn_sillhouette_color:source_color = vec4(0.25, 0.25, 0.25, 1.0);
uniform sampler2D objects_texture;
uniform sampler2D pawns_texture;
uniform sampler2D light_volumes_texture;
uniform sampler2D widgets_texture;
uniform sampler2D dither_texture;

// Const for tile ratio, 57 pixel width, 28 pixel height
const vec2 grid_frag_size = vec2(0.0556640625, 0.04666666666);

float ndot(vec2 a, vec2 b ) { return a.x*b.x - a.y*b.y; }
float sd_rhombus( in vec2 p, in vec2 b ) {
    p = abs(p);
    float h = clamp( ndot(b-2.0*p,b)/dot(b,b), -1.0, 1.0 );
    float d = length( p-0.5*b*vec2(1.0-h,1.0+h) );
    return d * sign( p.x*b.y + p.y*b.x - b.x*b.y );
}

void fragment() {
	//vec4 original = texture(SCREEN_TEXTURE, SCREEN_UV);
	vec4 light_volumes = texture(light_volumes_texture, SCREEN_UV);
		
	// If we aren't inside the light volumes
	if (light_volumes.a == 0.) {
		// Get the grid color at this fragment
		vec4 grid = texture(grid_texture, SCREEN_UV);

		// Set the output color
		vec4 black = vec4(0.0, 0.0, 0.0, 1.0);
		COLOR = black;

		// Calculate the SDF distance for the ellipse
		vec2 offset_center = (FRAGCOORD.xy - grid_point) * SCREEN_PIXEL_SIZE;
		float dist = sd_rhombus(offset_center, grid_unit_size * grid_frag_size);
		float mix_ratio = 0.0;

		// If we're inside the signed distance field, update the mix ratio
		if (dist <= 0.0) {
			mix_ratio = 25.0 * abs(dist);
			mix_ratio = clamp(mix_ratio, 0.0, 1.0);

			COLOR.rgb = mix(black.rgb, (grid.rgb * grid_fade) + black.rgb, mix_ratio);
			COLOR.a = 1.0;
		}

		// Get the widget color at this fragment
		vec4 widget = texture(widgets_texture, SCREEN_UV);
		// Apply it to the render output
		if (widget.a > 0.0) COLOR.rgb = widget.rgb;
//
		//// Sillhouette Code
		//// Get the fragments
		//vec4 objects = texture(objects_texture, SCREEN_UV);
		//vec4 pawns = texture(pawns_texture, SCREEN_UV);
		//
		//// Only color overlapping fragments
		//if (objects.a > 0.0) {
			//COLOR.rgb = object_sillhouette_color.rgb;
		//}
		//if (pawns.a > 0.0) {
			//// Calculate dither threshold
			//ivec2 noise_size = textureSize(dither_texture, 0);
			//vec2 inv_noise_size = vec2(1.0 / float(noise_size.x), 1.0 / float(noise_size.y));
			//vec2 noise_uv = SCREEN_UV * inv_noise_size * vec2(float(SCREEN_PIXEL_SIZE.x), float(SCREEN_PIXEL_SIZE.y));
			//float threshold = texture(dither_texture, noise_uv).r;
		//
			//COLOR.rgb = pawns.rgb;
		//}
		//
		//// Add Top Level Widgets
		//vec4 top_level_widgets = texture(top_level_widgets_texture, SCREEN_UV);
		//// Apply it to the render output
		//if (top_level_widgets.a > 0.0) COLOR.rgb = top_level_widgets.rgb;
		
	}
	else discard;
}