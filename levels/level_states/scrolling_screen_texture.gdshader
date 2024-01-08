shader_type canvas_item;

uniform vec2 direction = vec2(0.0, 0.0);
uniform float speed : hint_range(-5.0, 5.0) = 0;

void fragment() {
	COLOR = texture(TEXTURE, UV + (direction * TIME * speed));
}