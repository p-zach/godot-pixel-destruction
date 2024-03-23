extends Node2D

@export var ball: PackedScene
@export var brush1: PackedScene
@export var brush2: PackedScene
@export var brush3: PackedScene

var angle: float
const rot_speed := 1.0

func _process(delta):
	if Input.is_action_pressed("rotate_left"):
		angle -= rot_speed * delta
	if Input.is_action_pressed("rotate_right"):
		angle += rot_speed * delta
	
	if Input.is_action_just_pressed("brush1"):
		destroy_with_brush(brush1, get_global_mouse_position(), angle)
	if Input.is_action_just_pressed("brush2"):
		destroy_with_brush(brush2, get_global_mouse_position(), angle)
	if Input.is_action_just_pressed("brush3"):
		destroy_with_brush(brush3, get_global_mouse_position(), angle)
		
	if Input.is_action_just_pressed("spawn_ball"):
		spawn_ball(get_global_mouse_position())
	
func destroy_with_brush(brush: PackedScene, pos: Vector2, rot: float):
	var instance = brush.instantiate()
	instance.position = pos
	instance.rotation = rot
	add_child(instance)

func spawn_ball(pos: Vector2):
	var instance = ball.instantiate()
	instance.position = pos
	add_child(instance)
