using UnityEngine;

public class Solid : IProperty
{
    public BoxCollider2D collider, overlap;
    public Collisions collisions;
    public int horizontalRays, verticalRays, mask;
    public float horizontalRaySpacing, verticalRaySpacing, maxAscendAngle, maxDescendAngle;
    public const float overlapModifier = 0.02f; //must be double the overlapPreventer constant in physics.
    public readonly string name;

    public Solid(GameObject gameObject, int layer = 8, int mask = 256, int sizeX = 1, int sizeY = 1, float maxAscendAngle = 80, float maxDescendAngle = 75)
    {
        gameObject.layer = layer;
        name = gameObject.name;
        collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(sizeX, sizeY);
        horizontalRays = (int)Mathf.Round(collider.size.x * 6);
        verticalRays = (int)Mathf.Round(collider.size.y * 6);
        horizontalRaySpacing = (collider.bounds.size.y - overlapModifier) / (horizontalRays - 1);
        verticalRaySpacing = (collider.bounds.size.x - overlapModifier) / (verticalRays - 1);
        this.maxAscendAngle = maxAscendAngle;
        this.maxDescendAngle = maxDescendAngle;
        this.mask = mask;
        overlap = gameObject.AddComponent<BoxCollider2D>();
        overlap.size = new Vector2(sizeX - overlapModifier, sizeY - overlapModifier);
        collisions.Reset();
        collisions.slopeAngleOld = 0;
    }

    public struct Collisions
    {
        Bounds overlapBounds;
        public bool below, above, right, left, ascendingSlope, descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public string pushedBy;

        public void Reset()
        {
            below = above = right = left = ascendingSlope = descendingSlope = false;
            pushedBy = "";
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}