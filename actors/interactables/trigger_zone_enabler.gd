extends Area3D

func enable_monitoring():
	set_deferred("monitoring", true)
	
func disable_monitoring():
	set_deferred("monitoring", false)
	
func enable_monitorable():
	set_deferred("monitorable", true)
	
func disable_monitorable():
	set_deferred("monitorable", false)
