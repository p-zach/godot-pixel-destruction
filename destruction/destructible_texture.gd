extends Node2D

@export var destructible: Node2D

@export var sprite: Sprite2D

func set_texture(s: Texture2D):
	sprite.texture = s

# Passes the destroy command from the quadrant manager to this texture's destructible.
func destroy(polygon, texture, pos, rot):
	# Transform polygon to local coordinates
	var pol = polygon * global_transform
	
	destructible.Destroy(pol, texture, pos, rot)
