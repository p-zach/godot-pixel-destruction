extends Node2D

var texture: Texture2D
var pos: Vector2
var rot: float

# Set the texture to be drawn and the transform to draw it with
func set_texture(t: Texture2D, p: Vector2, r: float):
	# Ensure we're visible
	visible = true

	texture = t
	pos = p
	rot = r

	queue_redraw()

# Note: in order to draw we need queue_redraw() to be called.
func _draw():
	draw_set_transform(pos, rot)
	# Texture is drawn from the top left corner but pos is the center, so we need to translate
	draw_texture(texture, Vector2(-texture.get_width() / 2.0, -texture.get_height() / 2.0))
