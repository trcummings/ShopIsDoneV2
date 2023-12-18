/**
 * This shader creates fisheye or barrel distortion by sliding values in the
 * desired direction. It aims to have a minimal amount of pixelation, and was
 * originally conceived of to procedurally generate cartoon planets and bodies
 * from easily generatable flat textures.
 *
 * Thanks to user Dan (6145) on Stack Overflow for providing the math. I also
 * stole some code from here for the texture placement:
 * https://gist.github.com/quiglemj/971f4cec1b128c58b4864c5200bfc579
 * This shader differs from the above gist in that it does not stretch the
 * center towards the edges, but rather crushes the edges in towards the
 * center, and thus eliminates pixelation entirely while achieving a near
 * identical effect.
 *
 * See the final result in the edited part of my SO question:
 * https://computergraphics.stackexchange.com/questions/9434/shader-or-formula-that-distorts-inward
 *
 * License: CC0
 * https://creativecommons.org/publicdomain/zero/1.0/
 */

shader_type canvas_item;

// Inspector params:
uniform float effect : hint_range(-4, 4, 0.01) = -1.1; // -1.0 is BARREL, 0.1 is PINCUSHION. For planets, ideally -1.1 to -4.
uniform float effect_scale : hint_range(-2, 2, 0.01) = 1.1; // Play with this to slightly vary the results.
uniform float x_offset : hint_range(-1, 1, 0.01) = 0.0;
uniform float y_offset : hint_range(-1, 1, 0.01) = 0.0;

vec2 distort(vec2 p, vec2 offset) {
	float d = length(p);
	float z = sqrt(1.0 + d * d * effect);
	float r = atan(d, z) / 3.14159;
	r *= effect_scale;
	float phi = atan(p.y + offset.y, p.x + offset.x);
	return vec2(r*cos(phi)+.5,r*sin(phi)+.5);
}

void fragment(){
	// Double the UV
	vec2 xy = (2.0 * UV);
	xy.x = xy.x - 1.0;
	xy.y = xy.y - 1.0;

	// Perform distortion if needed.
	vec4 tex;
	float d = length(xy);
	if (d < 1.5) {
		xy = distort(xy, vec2(x_offset, y_offset));
		tex = texture(TEXTURE, xy);
		COLOR = tex;
	}
	else {
		COLOR.a = 0.0;
	}
}