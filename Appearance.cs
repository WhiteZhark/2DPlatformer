using UnityEngine;
using System.Collections;

public class Appearance : IBehaviour
{
    public GameObject gameObject;
    public SpriteRenderer renderer;
    protected Transform target;

    public Appearance()
    {
        gameObject = new GameObject("appearance");
        renderer = gameObject.AddComponent<SpriteRenderer>();
    }

    public void OnUpdate()
    {
        gameObject.transform.position = target.position;
    }

    public void AssignTarget(Transform transform)
    {
        target = transform;
    }
}