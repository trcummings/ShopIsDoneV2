extends Area3D

@export var timeline : Resource

# Called when the node enters the scene tree for the first time.
func _ready():
	body_entered.connect(_on_body_entered)

func _on_body_entered(_body: CharacterBody3D):
	#Dialogic.start(timeline)
	pass
