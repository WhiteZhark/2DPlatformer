using UnityEngine;
using System.Collections;

public class Action : IBehaviour
{
    public Vector2 impulse;
    public IIntelligence intelligence;

    public Action(IIntelligence intelligence = null)
    {
        this.intelligence = intelligence;
    }

    public void OnUpdate()
    {
        impulse = intelligence.GetImpulse();
    }
}