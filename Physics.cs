using UnityEngine;
using System.Collections.Generic;
using System;

public class Physics
{
    private GameManager manager;
    private const float margin = 0.2f, overlapPreventer = 0.01f; //margin is the perpendicular distance to the bounds from which the rays fire, 
                                                                 // overlapPreventer is the parallel distance from the edges of the bounds and the first/last rays. 
                                                                 // overlapPreventer must be half the overlapModifier in solid. MARGIN WAS .2
    private const int terrainLayer = 8;                                                            

    public Physics(GameManager manager)
    {
        this.manager = manager;
    }

    public void PhysicsUpdate()
    {
        List<Collection> movables = GetMovables();
        Vector3[] positions = GetNewPositions(movables);
        SetNewPositions(movables, positions);
    }

    private List<Collection> GetMovables()
    {
        List<Collection> movables = new List<Collection>();

        foreach (Collection collection in manager.gameCollections)
        {
            if (collection.GetProperty<Mobile>() != null && collection.GetBehaviour<Action>() != null)
            {
                Solid solid = collection.GetProperty<Solid>() as Solid;

                if (solid != null)
                {
                    solid.collisions.Reset();
                }
                if (collection.gameObject.layer == terrainLayer && solid != null)
                {
                    movables.Insert(0, collection); //Inserts at beginning if moving platform because everything just works better that way
                }
                else
                {
                    movables.Add(collection);
                }
            }
        }

        return movables;
    }

    private Vector3[] GetNewPositions(List<Collection> movables)
    {
        Vector3[] positions = new Vector3[movables.Count];
        int i = 0;

        foreach (Collection collection in movables)
        {
            Mobile mobile = collection.GetProperty<Mobile>() as Mobile;
            Action action = collection.GetBehaviour<Action>() as Action;
            Vector2 impulse = action.impulse;
            Solid solid = collection.GetProperty<Solid>() as Solid;

            mobile.velocity.x = impulse.x * mobile.speed.x;
            mobile.velocity.y = mobile.gravity == 0? impulse.y * mobile.speed.y : impulse.y * mobile.speed.y + Mathf.Max(mobile.velocity.y + (mobile.gravity * Time.deltaTime), mobile.maxFall);

            if (solid != null)
            {
                if (solid.collisions.right == true && mobile.velocity.x > 0 || solid.collisions.left == true && mobile.velocity.x < 0)
                {
                    mobile.velocity.x = 0;
                }
                if (solid.collisions.below == true && mobile.velocity.y < 0 || solid.collisions.above == true && mobile.velocity.y > 0)
                {
                    mobile.velocity.y = mobile.gravity * Time.fixedDeltaTime;
                }
            }

            mobile.GetDistance(Time.fixedDeltaTime);

            //Debug.Log(collection.gameObject.name + " has a mobile.distance.y of " + mobile.distance.y);
            //Debug.Log(collection.gameObject.name + " is moving at " + mobile.velocity.ToString());

            if (solid != null)
            {
                if (collection.gameObject.layer == terrainLayer)
                {
                    MovePassengers(mobile, solid);
                }
                else
                {
                    CheckCollisions(mobile, solid);
                }
            }

            positions[i] = collection.gameObject.transform.position + new Vector3(mobile.distance.x, mobile.distance.y, 0);
            i++;
        }

        return positions;
    }

    private void SetNewPositions(List<Collection> movables, Vector3[] positions)
    {
        int i = 0;

        foreach (Collection collection in movables)
        {
            Mobile mobile = collection.GetProperty<Mobile>() as Mobile;
            Solid solid = collection.GetProperty<Solid>() as Solid;

            collection.gameObject.transform.position = positions[i];

            //experimental popout
            //seems to work but is too sensitive
            if (solid != null && collection.gameObject.layer != terrainLayer)
            {
                foreach (Collection otherCollection in movables)
                {
                    if (otherCollection.gameObject.name != collection.gameObject.name)
                    {
                        Solid otherSolid = otherCollection.GetProperty<Solid>() as Solid;

                        if (otherSolid != null && solid.overlap.bounds.Intersects(otherSolid.collider.bounds))
                        {
                            Vector2 differenceVector = solid.collider.bounds.center - otherSolid.collider.bounds.center;
                            float xProjection = solid.collider.bounds.extents.x + otherSolid.collider.bounds.extents.x - Mathf.Abs(differenceVector.x);
                            float yProjection = solid.collider.bounds.extents.y + otherSolid.collider.bounds.extents.y - Mathf.Abs(differenceVector.y);

                            if (xProjection < yProjection)
                            {
                                collection.gameObject.transform.position = collection.gameObject.transform.position + new Vector3(Mathf.Sign(differenceVector.x) * xProjection, 0, 0);
                            }
                            else
                            {
                                collection.gameObject.transform.position = collection.gameObject.transform.position + new Vector3(0, Mathf.Sign(differenceVector.y) * yProjection, 0);
                            }
                        }
                    }
                }
            }

            mobile.Reset(); //possibly reset collisions here as well, or move this to where collisions are reset
            i++;
        }
    }

    private void CheckCollisions(Mobile mobile, Solid solid)
    {
        if (mobile.distance.y < 0)
        {
            DescendSlope(mobile, solid);
        }
        if (mobile.distance.x != 0)
        {
            CheckHorizontal(mobile, solid);
        }
        if (mobile.distance.y != 0)
        {
            CheckVertical(mobile, solid);
        }
    }

    private void CheckHorizontal(Mobile mobile, Solid solid)
    {
        float rayDistance = Mathf.Abs(mobile.distance.x) + margin;
        Vector2 direction = new Vector2(Mathf.Sign(mobile.distance.x), 0);
        Vector2 origin = direction.x == 1 ? new Vector2(solid.collider.bounds.max.x - margin, solid.collider.bounds.min.y + overlapPreventer) : 
            new Vector2(solid.collider.bounds.min.x + margin, solid.collider.bounds.min.y + overlapPreventer);

        for (int i = 0; i < solid.horizontalRays; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, solid.mask);
            Debug.DrawRay(new Vector3(origin.x, origin.y, 0), new Vector3(direction.x * rayDistance, 0, 0));
            origin.y += solid.horizontalRaySpacing;

            if (hit)
            {
                if (solid.collisions.descendingSlope)
                {
                    solid.collisions.descendingSlope = false;
                    mobile.distance.x = mobile.distanceUnmodified.x;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= solid.maxAscendAngle)
                {
                    float distanceToSlope = 0;

                    if (slopeAngle != solid.collisions.slopeAngleOld)
                    {
                        distanceToSlope = hit.distance - margin;
                        mobile.distance.x -= distanceToSlope * direction.x;
                    }

                    AscendSlope(mobile, solid, slopeAngle);
                    mobile.distance.x += distanceToSlope * direction.x;
                }

                if (!solid.collisions.ascendingSlope || slopeAngle > solid.maxAscendAngle)
                {
                    mobile.velocity.x = 0;
                    rayDistance = hit.distance;
                    mobile.distance.x = (rayDistance - margin) * direction.x;
                    solid.collisions.right = direction.x == 1 ? true : false;
                    solid.collisions.left = direction.x == -1 ? true : false;

                    if (solid.collisions.ascendingSlope)
                    {
                        mobile.distance.y = Mathf.Tan(solid.collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(mobile.distance.x);
                    }
                }
            }
        }
    }

    private void CheckVertical(Mobile mobile, Solid solid)
    {
        float rayDistance = Mathf.Abs(mobile.distance.y) + margin;
        Vector2 direction = new Vector2(0, Mathf.Sign(mobile.distance.y));
        Vector2 origin = direction.y == 1 ? new Vector2(solid.collider.bounds.min.x + mobile.distance.x + overlapPreventer, solid.collider.bounds.max.y - margin) :
            new Vector2(solid.collider.bounds.min.x + mobile.distance.x + overlapPreventer, solid.collider.bounds.min.y + margin);

        for (int i = 0; i < solid.verticalRays; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, solid.mask);
            Debug.DrawRay(new Vector3(origin.x, origin.y, 0), new Vector3(0, direction.y * rayDistance, 0));
            origin.x += solid.verticalRaySpacing;

            if (hit)
            {
                rayDistance = hit.distance;
                mobile.velocity.y = 0;
                solid.collisions.below = direction == Vector2.down ? true : false;
                solid.collisions.above = direction == Vector2.up ? true : false; //experimental line
                mobile.distance.y = (rayDistance - margin) * direction.y;

                if (direction.y == -1 && hit.transform.gameObject.name == solid.collisions.pushedBy)
                {
                    mobile.distance.y += mobile.downwardPull;
                }

                if (solid.collisions.ascendingSlope)
                {
                    mobile.distance.x = mobile.distance.y / Mathf.Tan(solid.collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(mobile.distance.x);
                }
            }
        }

        if (solid.collisions.ascendingSlope)
        {
            direction = new Vector2(Mathf.Sign(mobile.distance.x), 0);
            rayDistance = Mathf.Abs(mobile.distance.x) + margin;
            origin = direction.x == 1? new Vector2(solid.collider.bounds.max.x - margin, solid.collider.bounds.min.y + overlapPreventer + mobile.distance.y) :
                new Vector2(solid.collider.bounds.min.x + margin, solid.collider.bounds.min.y + overlapPreventer + mobile.distance.y);

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, solid.mask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != solid.collisions.slopeAngle)
                {
                    mobile.distance.x = (hit.distance - margin) * direction.x;
                    solid.collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    private void AscendSlope(Mobile mobile, Solid solid, float slopeAngle)
    {
        float verticalDistance = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(mobile.distance.x);

        if (mobile.distance.y <= verticalDistance)
        {
            mobile.distance.y = verticalDistance;
            mobile.distance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * mobile.distance.x;
            mobile.velocity.y = 0;

            solid.collisions.below = true;
            solid.collisions.ascendingSlope = true;
            solid.collisions.slopeAngle = slopeAngle;
        }
    }

    private void DescendSlope(Mobile mobile, Solid solid)
    {
        float directionX = Mathf.Sign(mobile.distance.x);
        Vector2 rayOrigin = directionX == 1 ? new Vector2(solid.collider.bounds.min.x + overlapPreventer, solid.collider.bounds.min.y + margin) :
            new Vector2(solid.collider.bounds.max.x - overlapPreventer, solid.collider.bounds.min.y + margin);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, solid.mask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= solid.maxDescendAngle && Mathf.Sign(hit.normal.x) == directionX && 
                hit.distance - margin <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(mobile.distance.x))
            {
                mobile.distance.y -= Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(mobile.distance.x); //THIS ORDERING IS IMPORTANT!  If done the other way, a post-modification mobile.distance.x is used in the equation.
                mobile.distance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * mobile.distance.x;
                mobile.velocity.y = 0;

                solid.collisions.slopeAngle = slopeAngle;
                solid.collisions.below = true;
                solid.collisions.descendingSlope = true;
            }
        }
    }

    private void MovePassengers(Mobile mobile, Solid solid)
    {
        List<GameObject> movedPassengers = new List<GameObject>();
        Vector2 direction = new Vector2(Mathf.Sign(mobile.distance.x), Mathf.Sign(mobile.distance.y));

        if (mobile.distance.y != 0)
        {
            float rayLength = Mathf.Abs(mobile.distance.y) + margin;
            Vector2 origin = direction.y == 1? new Vector2(solid.collider.bounds.min.x + overlapPreventer, solid.collider.bounds.max.y - margin) :
                new Vector2(solid.collider.bounds.min.x + overlapPreventer, solid.collider.bounds.min.y + margin);

            for (int i = 0; i < solid.verticalRays; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(0, direction.y), rayLength, solid.mask);
                Debug.DrawRay(new Vector3(origin.x, origin.y, 0), new Vector3(0, rayLength * direction.y, 0));
                origin.x += solid.verticalRaySpacing;

                if (hit && !movedPassengers.Contains(hit.transform.gameObject))
                {
                    movedPassengers.Add(hit.transform.gameObject);
                    Collection collection = manager.GetCollection(Int32.Parse(hit.transform.gameObject.name));
                    Mobile pushedMobile = collection.GetProperty<Mobile>() as Mobile;
                    Solid pushedSolid = collection.GetProperty<Solid>() as Solid;

                    pushedMobile.distancePushed.x += direction.y == 1 ? mobile.distance.x : 0;
                    pushedMobile.distancePushed.y += mobile.distance.y - (hit.distance - margin) * direction.y;

                    if (direction.y == 1)
                    {
                        pushedSolid.collisions.below = true;
                    }
                    if (direction.y == -1)
                    {
                        pushedSolid.collisions.above = true;
                    }
                }
            }
        }

        if (mobile.distance.x != 0)
        {
            float rayLength = Mathf.Abs(mobile.distance.x) + margin;
            Vector2 origin = direction.x == 1 ? new Vector2(solid.collider.bounds.max.x - margin, solid.collider.bounds.min.y + overlapPreventer) :
                new Vector2(solid.collider.bounds.min.x + margin, solid.collider.bounds.min.y + overlapPreventer);

            for (int i = 0; i < solid.horizontalRays; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(direction.x, 0), rayLength, solid.mask);
                Debug.DrawRay(new Vector3(origin.x, origin.y, 0), new Vector3(rayLength * direction.x, 0, 0));
                origin.y += solid.horizontalRaySpacing;

                if (hit && !movedPassengers.Contains(hit.transform.gameObject))
                {
                    movedPassengers.Add(hit.transform.gameObject);
                    Collection collection = manager.GetCollection(Int32.Parse(hit.transform.gameObject.name));
                    Mobile pushedMobile = collection.GetProperty<Mobile>() as Mobile;
                    Solid pushedSolid = collection.GetProperty<Solid>() as Solid;

                    pushedMobile.distancePushed.x += mobile.distance.x - (hit.distance - margin) * direction.x;

                    if (direction.x == 1)
                    {
                        pushedSolid.collisions.left = true;
                    }
                    if (direction.x == -1)
                    {
                        pushedSolid.collisions.right = true;
                    }
                }
            }
        }

        if (direction.y == -1 || (mobile.distance.x != 0 && mobile.distance.y == 0))
        {
            float rayLength = margin / 2 * Mathf.Abs(mobile.velocity.y) + margin;
            Vector2 origin = new Vector2(solid.collider.bounds.min.x + overlapPreventer, solid.collider.bounds.max.y - margin);

            for (int i = 0; i < solid.verticalRays; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, rayLength, solid.mask);
                Debug.DrawRay(new Vector3(origin.x, origin.y, 0), Vector2.up * rayLength);
                origin.x += solid.verticalRaySpacing;

                if (hit && !movedPassengers.Contains(hit.transform.gameObject))
                {
                    movedPassengers.Add(hit.transform.gameObject);
                    Collection collection = manager.GetCollection(Int32.Parse(hit.transform.gameObject.name));
                    Mobile pushedMobile = collection.GetProperty<Mobile>() as Mobile;
                    Solid pushedSolid = collection.GetProperty<Solid>() as Solid;

                    pushedMobile.distancePushed += mobile.distance;
                    pushedMobile.downwardPull += mobile.distance.y;
                    pushedSolid.collisions.pushedBy = solid.name;
                    //Adjust velocity y of vacuumed creature?
                }
            }
        }
    }
}

//Add acceleration math?

//Slope handling - IMPLEMENTED

//Moving platforms - Still working out bugs.

//Try moving in whole numbers only, storing the remainder and using it in calculations.

//SOLVED BUG: sometimes having a large series of debug.log messages causes weird bugs, ex: shooting up into the sky when jumping next to a wall.

//BUG: When moving up a slope and encountering a new slope, object passes right through it sometimes. (second ray not colliding?) <- really annoying
//SOLUTION: doubled margin so that if the corner was in the terrain somehow the horizontal ray wouldn't start inside of it. (hopefully this continues to work)

//BUG: first jump off slope is tiny for some reason (possibly due to modding distance.y too early?)
//SOLUTION: velocity.y was not being reset to zero when climbing the slope, resulting in massively negative y velocities that only applied when you tried to jump.

//BUG: When standing on corner of colider, cannot move towards it (bottom horizontal ray hitting it seems like) (does this mean the collider is getting inside of another collider somehow? rounding errors?)
//It appears that by default the edges of the bounding box overlap with the edges of the every other bounding box, which means the ray starts inside of it and therefore ignores it... until you move
//off of it and and then back on.  Possible solution, don't fire rays at the very edge?
//SOLUTION: the first and last rays should be fired from slightly within the edges of the bounds (parallel distance) to prevent being fired from within the edges of other colliders.
//PS maybe clean this up a little and make sure the physics and solid constants are tied together? also, for optimization, change the overlapModifier constant to just raw numbers so it doesn't
//have to be stored.

//BUG: Bounding Box often ends up with its corner partly inside the slope, which fixes itself when you stop moving.
//SOLUTION: When going up a slope, fire a horizontal ray at the predicted height and adjust distance.x and slopeAngle accordingly.

//BUG: Descending slopes doesn't work; the conditionals go off but somehow the positional adjustment is wrong.
//Possibly being affected by the other collision checks? (not snapping downward vertically, but slowing x movement correctly)
//SOLUTION: Use mobile.distance.x to find new mobile.distance.y BEFORE finding new mobile.distance.x.

//BUG: If collided horizontally while descending a slope, object would stop horizontally not snap to slope vertically immediately and would instead sink slowly for a couple of frames.
//SOLUTION: Instead of a horizontal collision while descending causing the mobile.distance to rever to mobile.distanceOld, it now only reverts the x value.

//BUG: If colliding above while climbing a slope, character collides correctly but then sinks down again by a tiny amount so that the top of the collider no longer touches the bottom of the object.
//Possibly not a bug?  At any rate, can be safely ignored as far as I can tell.

//BUG: Jittering when being pushed by a moving platform.
//SOLUTION: The core issue was that both entities would read each other's old position and move to it, such that even after being pushed they were still slightly overlapping.  This caused the weird jittering behaviour.
//The solution was to, before checking collisions, check if there was a push force in opposiiton to the mobile's distance.  If so, set the mobile's distance to the same as the push force, and null both velocity and the push force.
//For the other case, where the push occurs after the collision check, the push force is added into the final position calculation.
//STILL BUGGY: If the creature goes before the platform in the update cycle, still jittery.
//STILL SOLUTION: See the preemptive solution below.

//POSSIBLE FUTURE BUG: I think the way moving platforms work currently, they could in fact shove a creature into terrain if the platform is moved after the creature in the physics update. It might be necessary to update all
//moving platforms before creatures.
//PREEMPTIVE SOLUTION: Moving platforms are inserted at the beginning of the movables list so they are updated first.

//REAL LIFE BUG: Ask Natori to smoke their stupid weed somewhere that isn't right next to my room...

//BUG: able to jump twice as high off upward moving platform due to the fixedupdate getting called twice before the creature stops being 'grounded'

//BUG: when riding a platform vertically, not stopped by overhead obstacles
//SOLUTION: see below

//BUG: when riding a platform horizontally, cannot move in the opposite direction
//SOLUTION: put in collision state checks to determine velocities instead of other method

//BUG: when riding a platform down, cannot jump
//SOLUTION: Add checks for collisions on sides to determine direction of rays

//BUG: when riding a platform downward, always hover slightly above it due to the vertical check putting you on top of it before it moves down.
//SOLUTION: the pushing object deposits its name in the collisions struct of the pushed object and the push distance in distancePushed as well as 'downwardPull'.  If the creature then downwardly collides with the moving
//platform, it adds downwardPull back into its distance.y.
//RESURFACED BUG: it appears to not be fixed 100% of time.  Possibly due to the small scale currently being used or floating point rounding errors?
//SOLUTION 2.0: instead of setting velocity.y to 0 if there's a collision, set it to mobile.gravity * Time.fixedDeltaTime

//BUG: objects passing through each other at high speeds?
//SOLUTION: either lower the fixed time step in project settings or prevent high speed collisions from happening to begin with.
//POSSIBLE BETTER SOLUTION: check for overlaps after setting positions and then adjust?

//BUG: riding downward on a quickly moving platform causes bouncing due to the reset of the y velocity upon a solid collision.  y velocity resets are temporarily commented out.
//SOLUTION: Actual problem was the 'vacuum' rays fired upwards weren't long enough to make contact when moving downward at high speed.  This has been fixed by changing it from a fixed value to function of velocity.y and the margin.

//BUG: the overlap popout algorithm, while it successfully pops one out, seems to trigger constantly when riding upward on a fast moving platform.

//POSSIBLE FUTURE BUG: moving platforms do not fire their rays predictively, unlike in CheckCollisions.

//BUG?: If you stand too close to the front edge of a quickly horizontally moving platform, you simply fall off of it due to the simultaneous nature of the updates