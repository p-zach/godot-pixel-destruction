# Godot Pixel-Perfect Destruction

![](https://github.com/p-zach/godot-pixel-destruction/blob/main/github_files/Demo.gif)

#### With Debug: Visible Collision Shapes

![](https://github.com/p-zach/godot-pixel-destruction/blob/main/github_files/Demo_CollisionShapes.gif)

## Usage

### To use the demo scene:
- Press 1, 2, and 3 to destroy a circle, star, or diamond shape at the mouse position.
- Press Q and E to rotate the destruction shape.
- Press Space to spawn a ball at the mouse position.

### Scenes

#### `destructible`

This scene is the most important. Attaching `destructible.tscn` as a child of a `Sprite2D` makes the sprite destructible. Watching the tutorial that this scene (and 
`Destructible.cs`) were adapted from will be helpful in understanding the node's functionality; see Acknowledgement (1).

Children:
- SubViewport: Destruction sprites are drawn to this subviewport.
  - TextureCarver: Holds the script `texture_carver.gd` which draws destruction sprites to the viewport. If you are remaking `Destructible` from scratch, make sure this node's CanvasItem material is carried over--Blend Mode must be Subtract.
- Sprite2D: The destructible sprite, duplicated from the parent. The parent's sprite is removed so that only the destructible sprite remains.
- CollisionHolder: A StaticBody2D which holds the collision shapes for this destructible.
- CullTimer: A timer that destroys the parent sprite after duplication is complete.

If you don't need to do any procedural generation of destructible terrain, this is the only scene you need.

#### `destructible_texture`

A wrapper for a procedurally created destructible texture.

#### `brush_circle`, `brush_star`, `brush_diamond`

Demonstrative scenes for how destruction brush scenes should be formatted in order to be compatible with `destructible_terrain`'s `destroy_shape` function. When making custom brushes, they must have a child `Sprite2D` and `Polygon2D`. The `Polygon2D` should be shaped to cover the sprite. For sprites with curves, more vertices in the polygon means more exact destruction but also more resulting triangles, which harms performance at very large scales.

I would recommend that any project using `destructible.tscn` bundles destruction shapes with their sprites for ease of use, as seen in these examples.

### Scripts

#### `Destructible.cs`

The script that performs the setup and destruction of a sprite. `Destroy` is the most important method; it takes the destruction polygon in world space and texture (as well as the texture world position and rotation). An example use case not covered in the demo would be detecting if a collision shape collided with the `Destructible`, and if so, call `Destroy` with the colliding shape's polygon (in world space! Local polygon coordinates are not appropriate), texture, and world position and rotation. See `brush.gd` for transforming local polygon coordinates to world space.

#### `destructible_texture.gd`

Wraps a destructible for interfacing with a procedural generator.

#### `destructible_terrain.gd`

Procedurally generates `Destructible`s from a given terrain texture and provides a higher-level interface for destroying terrain with a `Brush`.

#### `texture_carver.gd`

Handles the drawing of destruction sprites to the destruction shader. Assumes that the `position` of the sprite is its center; this can be modified in `_draw` if necessary.

#### `destructible.gdshader`

Simple shader which destroys pixels in the sprite that have been drawn on in the `Destructible` SubViewport.

#### `brush.gd`

Defines the `Brush` class which represents an object that can be used to destroy part of a `Destructible`. To make a brush, inherit from `Brush` and implement the `_ready` function. Examples are `brush_circle.gd`, `brush_star.gd`, and `brush_diamond.gd`. `get_bounds()` can be used to find which `Destructible`s in the scene must be updated via checking for intersection with the `Rect2`, as seen in `destructible_terrain.gd`. Any node with a script attached that extends `Brush` must have a child `Sprite2D` and `Polygon2D`.

### Disclaimer

If you are porting the scenes to a different project, make sure the nodes are set up (especially Destructible) EXACTLY as they are here.
Forgetting to set or improperly setting a field will probably make the system fail (or do strange things).

## Acknowledgements

1. This system was adapted from [MitchMakesThings](https://www.youtube.com/c/MitchMakesThings)'s 
[Terrain Destruction Example](https://github.com/MitchMakesThings/Godot-Things/tree/main/Terrain-Destruction-Example) (MIT license),
which also has [tutorials on YouTube](https://www.youtube.com/watch?v=q9SV4o7ZZNk). These tutorials are instructive in learning how to use
the system and how the Destructible node is set up.

2. [godot-destructible-terrain](https://github.com/matterda/godot-destructible-terrain/tree/main) (MIT license) was helpful for handling
carving out the inside of a polygon (see Destructible.SplitPolygon).

## Novelty

This system builds on the two repositories acknowledged above by combining the removal of destroyed pixels from a texture (from 1) with the carving of collision polygons by an arbitrary polygon (from 2).
The functionality is further extended by the capability to remove an arbitrary texture shape from terrain. 
Of course, some performance is lost compared to (2) after the addition of sprite manipulation, so if you want just collision carving, use (2).

## Future work

I have no plans to continue to develop this system or turn it into a Godot plugin.
