@tool
extends MarginContainer

@export var to_offset : Control
 
func _process(_delta):
	if get_theme_constant("margin_left") != to_offset.size.x:
		add_theme_constant_override("margin_left", int(to_offset.size.x))
