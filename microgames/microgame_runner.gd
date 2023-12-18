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
		button.pressed.connect(_on_microgame_button_pressed.bind(microgame))
	button_template.hide()

func _on_microgame_button_pressed(scene: PackedScene):
	# disable all buttons
	for button in container.get_children():
		button.disabled = true
	var microgame = scene.instantiate()
	microgame_manager.RunMicrogame(microgame, null)
	await microgame_manager.MicrogameFinishedRunning
	# re-enable all buttons
	for button in container.get_children():
		button.disabled = false
