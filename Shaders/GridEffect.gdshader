shader_type canvas_item;

uniform vec4 chroma_key:source_color = vec4(0.0, 1.0, 1.0, 1.0);

// Widgets
uniform sampler2D widget_texture;

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
uniform sampler2D world_and_lights_texture;
uniform sampler2D top_level_widgets_texture;
uniform sampler2D dither_texture;

// Const for tile ratio, 57 pixel width, 28 pixel height
const vec2 grid_frag_size = vec2(0.0556640625, 0.04666666666);

float ndot(vec2 a, vec2 b ) { return a.x*b.x - a.y*b.y; }
float sdRhombus( in vec2 p, in vec2 b ) {
    p = abs(p);
    float h = clamp( ndot(b-2.0*p,b)/dot(b,b), -1.0, 1.0 );
    float d = length( p-0.5*b*vec2(1.0-h,1.0+h) );
    return d * sign( p.x*b.y + p.y*b.x - b.x*b.y );
}

// SDF function for ellipse calculation
float sdEllipse( in vec2 p, in vec2 ab ) {
    p = abs(p); if( p.x > p.y ) {p=p.yx;ab=ab.yx;}
    float l = ab.y*ab.y - ab.x*ab.x;
    float m = ab.x*p.x/l;      float m2 = m*m; 
    float n = ab.y*p.y/l;      float n2 = n*n; 
    float c = (m2+n2-1.0)/3.0; float c3 = c*c*c;
    float q = c3 + m2*n2*2.0;
    float d = c3 + m2*n2;
    float g = m + m*n2;
    float co;
    if( d<0.0 )
    {
        float h = acos(q/c3)/3.0;
        float s = cos(h);
        float t = sin(h)*sqrt(3.0);
        float rx = sqrt( -c*(s + t + 2.0) + m2 );
        float ry = sqrt( -c*(s - t + 2.0) + m2 );
        co = (ry+sign(l)*rx+abs(g)/(rx*ry)- m)/2.0;
    }
    else
    {
        float h = 2.0*m*n*sqrt( d );
        float s = sign(q+h)*pow(abs(q+h), 1.0/3.0);
        float u = sign(q-h)*pow(abs(q-h), 1.0/3.0);
        float rx = -s - u - c*4.0 + 2.0*m2;
        float ry = (s - u)*sqrt(3.0);
        float rm = sqrt( rx*rx + ry*ry );
        co = (ry/sqrt(rm-rx)+2.0*g/rm-m)/2.0;
    }
    vec2 r = ab * vec2(co, sqrt(1.0-co*co));
    return length(r-p) * sign(p.y-r.y);
}

void fragment() {
	vec4 original = texture(SCREEN_TEXTURE, SCREEN_UV);
	vec4 world_and_lights = texture(world_and_lights_texture, SCREEN_UV);
	
	// If we aren't inside the light volumes
	if (distance(world_and_lights.rgb, chroma_key.rgb) > 0.1) {
		// Get the grid color at this fragment
		vec4 grid = texture(grid_texture, SCREEN_UV);

		// Set the output color
		vec4 black = vec4(0.0, 0.0, 0.0, 1.0);
		COLOR = black;

		// Calculate the SDF distance for the ellipse
		vec2 offset_center = (FRAGCOORD.xy - grid_point) * SCREEN_PIXEL_SIZE;
		float dist = sdRhombus(offset_center, grid_unit_size * grid_frag_size);
		float mix_ratio = 0.0;

		// If we're inside the signed distance field, update the mix ratio
		if (dist <= 0.0) {
			mix_ratio = 25.0 * abs(dist);
			mix_ratio = clamp(mix_ratio, 0.0, 1.0);

			COLOR.rgb = mix(black.rgb, (grid.rgb * grid_fade) + black.rgb, mix_ratio);
			COLOR.a = 1.0;
		}

		// Get the widget color at this fragment
		vec4 widget = texture(widget_texture, SCREEN_UV);
		// Apply it to the render output
		if (widget.a > 0.0) COLOR.rgb = widget.rgb;

		// Sillhouette Code
		// Get the fragments
		vec4 objects = texture(objects_texture, SCREEN_UV);
		vec4 pawns = texture(pawns_texture, SCREEN_UV);
		
		// Only color overlapping fragments
		if (objects.a > 0.0) {
			COLOR.rgb = object_sillhouette_color.rgb;
		}
		if (pawns.a > 0.0) {
			// Calculate dither threshold
			ivec2 noise_size = textureSize(dither_texture, 0);
			vec2 inv_noise_size = vec2(1.0 / float(noise_size.x), 1.0 / float(noise_size.y));
			vec2 noise_uv = SCREEN_UV * inv_noise_size * vec2(float(SCREEN_PIXEL_SIZE.x), float(SCREEN_PIXEL_SIZE.y));
			float threshold = texture(dither_texture, noise_uv).r;
		
			COLOR.rgb = pawns.rgb;
		}
		
		// Add Top Level Widgets
		vec4 top_level_widgets = texture(top_level_widgets_texture, SCREEN_UV);
		// Apply it to the render output
		if (top_level_widgets.a > 0.0) COLOR.rgb = top_level_widgets.rgb;
		
	}
	else COLOR = original;
}