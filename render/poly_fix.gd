@tool
extends Polygon2D

func _ready():
	var use_custom_rect = true;
	var rect = Rect2(0, 0, 2000, 2000)
	# if we're in the editor, just use the viewport rect
	if Engine.is_editor_hint():
		rect = get_viewport_rect();
	RenderingServer.canvas_item_set_custom_rect(get_canvas_item(), use_custom_rect, rect)
