class_name Brush extends Node2D

# Get the polygon transformed to world space
func get_polygon() -> PackedVector2Array:
	return global_transform * $Polygon2D.polygon
	
func get_texture() -> Texture2D:
	return $Sprite2D.texture

# Get the bounds of the transformed polygon
func get_bounds() -> Rect2:
	var polygon := get_polygon()
		
	var min_pos := polygon[0]
	var max_pos := polygon[0]
	
	for p in polygon:
		if p.x < min_pos.x:
			min_pos.x = p.x
		elif p.x > max_pos.x:
			max_pos.x = p.x
		if p.y < min_pos.y:
			min_pos.y = p.y
		elif p.y > max_pos.y:
			max_pos.y = p.y
	return Rect2(min_pos, max_pos - min_pos)
