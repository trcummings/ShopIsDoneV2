extends Control

@export var microgames : Array[PackedScene]
@export var container : Control
@export var microgame_manager : Control
@export var button_template : Button

func _ready():
	for microgame in microgames:
		var button : Button = button_template.duplicate()
		container.add_child(button)
		button.text = microgame.resource_path
		button.pressed.connect(self._on_microgame_button_pressed.bind(microgame))
	button_template.hide()

func _on_microgame_button_pressed(scene: PackedScene):
	print(scene)
	var microgame = scene.instantiate()
	microgame_manager.RunMicrogame(microgame, null)
