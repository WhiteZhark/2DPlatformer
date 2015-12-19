using UnityEngine;
using System.Collections;

public class FixedPatrol : IIntelligence
{
    private Vector2 impulse;
    private Transform transform;
    private Vector3 startPosition, endPosition;
    //private float routeDistance, currentDistance;
    private bool returning;

    public FixedPatrol(GameObject gameObject, Vector2 endPosition)
    {
        transform = gameObject.transform;
        startPosition = transform.position;
        this.endPosition = new Vector3(endPosition.x, endPosition.y, 0);
        Vector3 vector = this.endPosition - startPosition;
        float distance = vector.magnitude;
        Vector3 direction = vector / distance;
        impulse = new Vector2(direction.x, direction.y);
        returning = false;
    }

    public Vector2 GetImpulse()
    {
        SetDirection();
        return impulse;// Vector2.right;// 
    }

    private void SetDirection()
    {
        /*if (returning)
        {
            impulse = Vector2.right;
        }
        if (!returning)
        {
            returning = true;
        }*/
    }
}