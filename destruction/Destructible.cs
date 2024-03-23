/*
 * Terrain destruction system adapted from:
 * https://github.com/MitchMakesThings/Godot-Things/tree/main/Terrain-Destruction-Example (MIT)
 * and
 * https://github.com/matterda/godot-destructible-terrain/tree/main (MIT)
 */

using Godot;
using System.Collections.Generic;
using System.Linq;

namespace PixelDestructionDemo.Destruction 
{
	public partial class Destructible : Node2D
	{
		[Export]
		private Node2D _viewportDestroyer { get; set; }
		[Export]
		private StaticBody2D _collisionHolder { get; set; }

		[Export]
		private ShaderMaterial _destructionShader { get; set; }

		private Vector2I _worldSize;
		private Node2D _toCull;
		private ImageTexture _imageRepublishTexture;
		private ShaderMaterial _parentShader;

		private bool _repubSpriteQueued = false;

		public override void _Ready()
		{
			// Get nodes once
			var parent = GetParent<Sprite2D>();
			var subViewport = GetNode<SubViewport>("SubViewport");

			// Set parent shader to a duplicate of the destruction shader
			_parentShader = _destructionShader.Duplicate() as ShaderMaterial;
			parent.Material = _parentShader;
		
			// Get world size and set viewport size
			_worldSize = (Vector2I)parent.GetRect().Size;
			subViewport.Size = _worldSize;

			// Copy parent sprite and prepare to cull it
			// We cull the parent sprite because we want it in the subviewport
			// instead of as a parent, and having it in both places would make
			// the destruction invisible due to the parent covering it up.
			var duplicate = GetParent().Duplicate(0) as Node2D;
			_toCull = duplicate;

			// Add duplicated sprite as a child of the viewport
			subViewport.AddChild(duplicate);

			// Start timer to delete duplicated parent info
			GetNode<Timer>("CullTimer").Start();

			// Wait for viewports to re-render before building the image
			RenderingServer.FramePostDraw += BuildCollisionsFromImage;
		}
		
		/// <summary>
		/// Destroy an area of the sprite's texture and collision.
		/// </summary>
		/// <param name="polygon">The polygon to destroy.</param>
		/// <param name="texture">The texture shape to erase.</param>
		/// <param name="pos">The position of the texture.</param>
		/// <param name="rot">The rotation of the texture.</param>
		public void Destroy(Vector2[] polygon, Texture2D texture, Vector2 pos, float rot)
		{
			// Calculate new collisions
			RebuildCollisions(polygon);

			// Transform position to local space
			pos -= GlobalPosition;
			// Relocate viewport destruction shape
			_viewportDestroyer.Call("set_texture", [texture, pos, rot]);

			// Re-render the viewport into the texture
			RerenderTexture();

			// Wait until viewport redraw before pushing viewport to
			// destruction shader
			QueueRepublishSprite();
		}

		#region SPRITE_UPDATING

		/// <summary>
		/// Remove the parent sprite so that only the Destructible sprite is
		/// visible.
		/// </summary>
		private void CullForegroundDuplicates()
		{
			_toCull.QueueFree();
		}

		/// <summary>
		/// Force the target viewport to re-render the sprite.
		/// </summary>
		private void RerenderTexture()
		{
			GetNode<SubViewport>("SubViewport").RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
		}

		/// <summary>
		/// Update the sprite displayed by this Destructible.
		/// </summary>
		private void RepublishSprite()
		{
			RenderingServer.FramePostDraw -= RepublishSprite;
			_repubSpriteQueued = false;
			// Assume the image has changed and we need to update the ImageTexture
			_imageRepublishTexture = ImageTexture.CreateFromImage(GetNode<Sprite2D>("Sprite2D").Texture.GetImage());
			// Set parameter to carve out destruction shape
			_parentShader.SetShaderParameter("destruction_mask", _imageRepublishTexture);
		}

		/// <summary>
		/// Republish the sprite at the end of the current frame.
		/// </summary>
		private void QueueRepublishSprite()
		{
			if (!_repubSpriteQueued)
			{
				RenderingServer.FramePostDraw += RepublishSprite;
				_repubSpriteQueued = true;
			}
		}

		#endregion
		#region COLLISION_UPDATING

		/// <summary>
		/// Rebuild this Destructible's collisions after a destruction event.
		/// </summary>
		/// <param name="destroyPolygon">The shape to destroy.</param>
		private void RebuildCollisions(Vector2[] destroyPolygon)
		{
			// Check every polygon in the collider
			foreach (CollisionPolygon2D collisionPolygon in _collisionHolder.GetChildren().Cast<CollisionPolygon2D>())
			{
				Vector2[][] clippedPolygons = [.. Geometry2D.ClipPolygons(collisionPolygon.Polygon, destroyPolygon)];
				int numClippedPolygons = clippedPolygons.Length;

				switch (numClippedPolygons) 
				{
					case 0:
						// If the clip failed, we're probably trying to delete
						// the remnants of a small 'island'
						collisionPolygon.QueueFree();
					break;

					case 2:
						// If it's a hole, split the polygon into 2 and replace
						if (Geometry2D.IsPolygonClockwise(clippedPolygons[1]))
						{
							// Split into two polygons with the appropriate
							// shape carved out
							Vector2[][] splitPolygons = SplitPolygon(collisionPolygon.Polygon, destroyPolygon);
							
							// Remove the old polygon
							collisionPolygon.QueueFree();

							// Add the new polygon
							for (var i = 0; i < splitPolygons.Length; i++)
							{
								// Ignore clipped polygons that are too small to create
								if (splitPolygons[i].Length < 3)
									continue;

								var collider = new CollisionPolygon2D() {
									Polygon = splitPolygons[i]
								};
								// _collisionHolder.CallDeferred(MethodName.AddChild, collider);
								_collisionHolder.AddChild(collider);
							}
							break;
						}
						// If it's not a hole, behave as in default
						else goto default;

					default:
						UpdateCollider(collisionPolygon, clippedPolygons);
						break;
				}
			}
		}

		/// <summary>
		/// Update the CollisionPolygon2D's polygon and create more 
		/// CollisionPolygon2D nodes if necessary.
		/// </summary>
		/// <param name="collisionPolygon">The CollisionPolygon2D to update.</param>
		/// <param name="clippedPolygons">The updating polygon(s).</param>
		private void UpdateCollider(CollisionPolygon2D collisionPolygon, Vector2[][] clippedPolygons)
		{
			for (var i = 0; i < clippedPolygons.Length; i++)
			{
				// Ignore clipped polygons that are too small to create
				if (clippedPolygons[i].Length < 3)
				{
					if (i == 0)
						collisionPolygon.QueueFree();
					else
						continue;
				}

				// Update if this is the existing polygon
				if (i == 0)
					collisionPolygon.Polygon = clippedPolygons[i];
				// Otherwise, clipping created new islands
				// Add a CollisionPolygon2D for each of them
				else
				{
					var collider = new CollisionPolygon2D() {
						Polygon = clippedPolygons[i]
					};
					_collisionHolder.AddChild(collider);
				}
			}
		}

		/// <summary>
		/// Create collision polygons from the sprite of this Destructible.
		/// </summary>
		private void BuildCollisionsFromImage()
		{
			// Detach this method from the RenderingServer event so it is not
			// called again next frame
			RenderingServer.FramePostDraw -= BuildCollisionsFromImage;

			// Create bitmap from the subviewport
			var bitmap = new Bitmap();
			bitmap.CreateFromImageAlpha(GetNode<Sprite2D>("Sprite2D").Texture.GetImage());

			Vector2I bitmapSize = bitmap.GetSize();

			// Create the polygons from the bitmap
			Vector2[][] polygons = [.. bitmap.OpaqueToPolygons(new Rect2I(Vector2I.Zero, bitmapSize))];

			// Add each polygon as a new child of the collision holder
			for (var i = 0; i < polygons.Length; i++)
			{
				var collider = new CollisionPolygon2D() {
					Polygon = polygons[i]
				};
				_collisionHolder.AddChild(collider);
			}
		}

		/// <summary>
		/// Splits a polygon into two polygons vertically split with a polygon
		/// carved out from the middle.
		/// </summary>
		/// <param name="clippedPolygon">The polygon that will be split and carved into.</param>
		/// <param name="clippingPolygon">The polygon that will be carved out.</param>
		private Vector2[][] SplitPolygon(Vector2[] clippedPolygon, Vector2[] clippingPolygon) 
		{
			// Find the midpoint of the clipping shape
			float midpoint = AveragePosition(clippingPolygon).X;
			// Slice the clipped shape bounds into two halves
			Vector2[] leftSubquadrant = [
				Vector2.Zero,
				new Vector2(midpoint, 0),
				new Vector2(midpoint, _worldSize.Y),
				_worldSize * Vector2.Down
			];
			Vector2[] rightSubquadrant = [
				leftSubquadrant[1],
				_worldSize * Vector2.Right,
				_worldSize,
				leftSubquadrant[2]
			];
			// Carve the clipping shape from the sliced bounds
			Vector2[] newLeftSubquadrant = Geometry2D.ClipPolygons(leftSubquadrant, clippingPolygon)[0];
			Vector2[] newRightSubquadrant = Geometry2D.ClipPolygons(rightSubquadrant, clippingPolygon)[0];
			var clippedSubquadrants = new List<Vector2[]>();
			// Intersect the clipped bounds with the existing collision polygon
			clippedSubquadrants.AddRange(Geometry2D.IntersectPolygons(newLeftSubquadrant, clippedPolygon));
			clippedSubquadrants.AddRange(Geometry2D.IntersectPolygons(newRightSubquadrant, clippedPolygon));
			for (var i = clippedSubquadrants.Count - 1; i >= 0; i--)
			{
				if (clippedSubquadrants[i].Length < 3)
					clippedSubquadrants.RemoveAt(i);
			}
			return [.. clippedSubquadrants];
		}

		/// <summary>
		/// Finds the average position of all of the points of a polygon.
		/// </summary>
		private static Vector2 AveragePosition(Vector2[] polygon)
		{
			Vector2 sum = Vector2.Zero;
			for (var i = 0; i < polygon.Length; i++)
			{
				sum += polygon[i];
			}
			return sum / polygon.Length;
		}

		#endregion
	}
}
