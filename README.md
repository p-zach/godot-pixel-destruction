# Godot Pixel-Perfect Destruction

## Demo

![](https://github.com/p-zach/godot-pixel-destruction/blob/main/github_files/Demo.gif)

#### With Debug: Visible Collision Shapes

![](https://github.com/p-zach/godot-pixel-destruction/blob/main/github_files/Demo_CollisionShapes.gif)

## Usage

TODO

#### Disclaimer

If you are porting the scenes to a different project, make sure to set up the nodes (especially Destructible) EXACTLY as they are here.
Forgetting to set or improperly setting a field will probably make the system fail (or do strange things).

### Acknowledgements

1. This system was adapted from [MitchMakesThings](https://www.youtube.com/c/MitchMakesThings)'s 
[Terrain Destruction Example](https://github.com/MitchMakesThings/Godot-Things/tree/main/Terrain-Destruction-Example) (MIT license),
which also has [tutorials on YouTube](https://www.youtube.com/watch?v=q9SV4o7ZZNk). These tutorials are instructive in learning how to use
the system and how the Destructible node is set up.

2. [godot-destructible-terrain](https://github.com/matterda/godot-destructible-terrain/tree/main) (MIT license) was helpful for handling
carving out the inside of a polygon (see Destructible.SplitPolygon).

### Novelty

This system builds on the two repositories acknowledged above by combining the removal of destroyed pixels from a texture (from 1) with the carving of collision polygons by an arbitrary polygon (from 2).
The functionality is further extended by the capability to remove an arbitrary texture shape from terrain. 
Of course, some performance is lost compared to (2) after the addition of sprite manipulation, so if you want just collision carving, use (2).

### Future

I have no plans to continue to develop this system or turn it into a Godot plugin.
