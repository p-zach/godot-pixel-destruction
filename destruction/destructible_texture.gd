extends Node2D

@export var destructible: Node2D

@export var sprite: Sprite2D

func set_texture(s: Texture2D):
	sprite.texture = s

func destroy(polygon, texture, pos, rot):
	var pol = polygon * global_transform
	
	destructible.Destroy(pol, texture, pos, rot)
