@tool
extends Polygon2D

func _ready():
	var use_custom_rect = true;
	var rect = get_viewport_rect();
	RenderingServer.canvas_item_set_custom_rect(get_canvas_item(), use_custom_rect, rect)
