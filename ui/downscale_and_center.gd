extends Control

func _process(_delta):
	var root = get_tree().root
	var reverse_scale = 2.0 - root.content_scale_factor
	scale = Vector2(reverse_scale, reverse_scale)
	var project_size = Vector2(
		ProjectSettings.get_setting("display/window/size/width"),
		ProjectSettings.get_setting("display/window/size/height")
	)
	var half_size = (project_size - size) / 2
	position = half_size
