using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private int nextID;
    private Physics physics;
    private Factory factory;
    public List<Collection> gameCollections = new List<Collection>();

    private GameManager()
    {
        nextID = 0;
        physics = new Physics(this);
        factory = new Factory(this);
    }

    public int AssignID()
    {
        nextID++;
        return nextID - 1;
    }

    public Collection GetCollection(int ID)
    {
        return gameCollections.Find(item => item.ID == ID);
    }

    public void ApplyBehaviours()
    {
        foreach (Collection collection in gameCollections)
        {
            foreach (IBehaviour behaviour in collection.behaviours)
            {
                behaviour.OnUpdate();
            }
        }
    }

    void Awake()
    {
        factory.PatrolTest(0, 0);
        factory.PlayerCreationTest(0, 1);
    }

    void FixedUpdate()
    {
        physics.PhysicsUpdate();
    }

    void Update()
    {
        ApplyBehaviours();
    }
}

//BUG: if gameObject is in the solid layer, it cannot move!!!! (detecting collisions on self)
//Fix: shoot rays from just outside collider
//ACTUAL SOLUTION: In Unity2D settings, turn off the checkbox that tells rays to detect colliders they start inside of.