extends Node2D

@export var terrain_image: Image
@export var num_quadrants: int = 16

@export var destructible_texture: PackedScene

var _world_size: Vector2i
var _quadrant_size: Vector2i

var _quadrants: Array

func _ready():
	_world_size = terrain_image.get_size()
	_quadrant_size = _world_size / num_quadrants
	
	# Create quadrants of destructible textures
	for x in range(num_quadrants):
		_quadrants.append([])
		for y in range(num_quadrants):
			var bounds := Rect2i(x * _quadrant_size.x, y * _quadrant_size.y, _quadrant_size.x, _quadrant_size.y)
			var quadrant_texture := ImageTexture.create_from_image(terrain_image.get_region(bounds))
			
			var new_destructible = destructible_texture.instantiate()
			new_destructible.set_texture(quadrant_texture)
			new_destructible.position = bounds.position
			
			_quadrants[x].append(new_destructible)
			add_child(new_destructible)
			
func destroy_shape(brush: Brush):
	var bounds = brush.get_bounds()
	var quadrant_min := Vector2i(
		(int)(bounds.position.x / _quadrant_size.x),
		(int)(bounds.position.y / _quadrant_size.y)
	)
	var quadrant_max := Vector2i(
		(int)(bounds.end.x / _quadrant_size.x),
		(int)(bounds.end.y / _quadrant_size.y)
	)
	for x in range(quadrant_min.x, quadrant_max.x + 1):
		for y in range(quadrant_min.y, quadrant_max.y + 1):
			if x >= 0 && x < num_quadrants && y >= 0 && y < num_quadrants:
				_quadrants[x][y].destroy(brush.get_polygon(), brush.get_texture(), brush.position, brush.rotation)
