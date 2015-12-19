using UnityEngine;
using System.Collections;

public class Mobile : IProperty
{
    public Vector2 speed, velocity, distance, distanceUnmodified, distancePushed;
    public float gravity, maxFall, downwardPull;
    
    public Mobile (int speedX = 0, int speedY = 0, float gravity = -50, float maxFall = -90)
    {
        this.speed = new Vector2(speedX, speedY);
        this.velocity = Vector2.zero;
        this.gravity = gravity;
        this.maxFall = maxFall;
        distance = Vector2.zero;
        distancePushed = Vector2.zero;
    }

    public Vector2 GetDistance(float time)
    {
        distance = (velocity * time) + distancePushed;
        distanceUnmodified = distance;
        return distance;
    }

    public void Reset()
    {
        distancePushed = Vector2.zero;
        downwardPull = 0;
    }
}