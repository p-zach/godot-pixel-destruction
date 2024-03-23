extends Brush

func _ready():
	# Here you can do something like spawn an explosive effect.
	print("Destroying a star shape at %v." % position)
	
	get_node("/root/Demo/DestructibleTerrain").destroy_shape(self)
	queue_free()
