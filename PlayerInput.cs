using UnityEngine;
using System.Collections;

public class PlayerInput : IIntelligence
{
    private Vector2 impulse;
    private Solid solid;
    private float previousYInput;

    public PlayerInput(Solid solid)
    {
        this.solid = solid;
    }

    public Vector2 GetImpulse()
    {
        impulse.x = Input.GetAxisRaw("Horizontal");
        impulse.y = solid.collisions.below? Input.GetAxisRaw("Vertical") : 0;

        return impulse;
    }
}

//Replace raw inputs with remappable buttons eventually.
//Disallow holding down of jump button to endlessly jump.