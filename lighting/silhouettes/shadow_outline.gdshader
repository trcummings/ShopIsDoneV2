shader_type canvas_item;

uniform vec4 line_color : source_color = vec4(1);
uniform float line_thickness : hint_range(0, 10) = 1.0;

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

const vec2 OFFSETS[8] = {
	vec2(-1, -1), vec2(-1, 0), vec2(-1, 1), vec2(0, -1), vec2(0, 1), 
	vec2(1, -1), vec2(1, 0), vec2(1, 1)
};

void fragment() {
	vec2 size = TEXTURE_PIXEL_SIZE * line_thickness / 2.0;
	vec4 color = texture(TEXTURE, UV);
	
	float inline = 1.0;
	float outline = 0.0;
	// Loop over each offset and sample the texture's alpha at that point
	for (int i = 0; i < OFFSETS.length(); i++) {
		float sample = texture(TEXTURE, UV + size * OFFSETS[i]).a;
		outline += sample;
		inline *= sample;
	}
	// clamp the alpha values to 
	outline = min(1.0, outline) - color.a;
	inline = (1.0 - inline) * color.a;
	
	// Calculate the outlined result
	vec4 outlined_result = mix(color, line_color, outline + inline);
	// If the inline and outline are zero, we're not within either
	if (outline == 0. && inline == 0.) 
	{
		COLOR = color;
	}
	// Otherwise, we're within the outline
	else
	{
		float limit = get_limit(FRAGCOORD.xy * 0.5);
		if (0.1 < limit) discard;
		else COLOR = color;
	}
	//COLOR = mix(color, outlined_result, outlined_result.a);
	//COLOR = color;
}
