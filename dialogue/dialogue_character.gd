@tool
extends Resource
class_name DialogueCharacter

@export var default_portrait:String = ""
@export var portraits:Dictionary = {}
@export var custom_info:Dictionary = {}
@export var text_noise:AudioStream
