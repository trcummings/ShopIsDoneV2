@tool
extends EditorScenePostImport

var node_extras_dict = {}
var remove_nodes = []

# This sample changes all node names.
# Called right after the scene is imported and gets the root node.
func _post_import(scene):
	# Change all node names to "modified_[oldnodename]"

	var file = FileAccess.open(get_source_file(), FileAccess.READ)
	var content = file.get_as_text()
	
	var json = JSON.new()
	var error = json.parse(content)
	if error == OK:
		parseGLTF(json.data)
		#iterateScene(scene)
		#deleteExtras()
	
	print(node_extras_dict)
	#scene.set_script(load("res://gltf_pipeline/gltf_scene_init.gd"))
	scene.set_meta("run", true)
	
	return scene # Remember to return the imported scene

func parseGLTF(json):
	# go through each node and find ones which references meshes
	if "nodes" in json:
		for node in json["nodes"]:
			if "mesh" in node:
				var mesh_index = node["mesh"]
				var mesh = json["meshes"][mesh_index]
				if "extras" in mesh:
					prints(node, mesh)
					addExtrasToDict(node["name"], mesh["extras"])
				
			if "extras" in node:
				addExtrasToDict(node["name"], node["extras"])

func addExtrasToDict(nodeName, extras):
	var gNodeName = nodeName.replace(".", "_")
	if gNodeName not in node_extras_dict:
		node_extras_dict[gNodeName] = {}
	for extra in extras:
		node_extras_dict[gNodeName][extra] = str(extras[extra])
