using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collection
{
    public readonly int ID;
    public List<IProperty> properties = new List<IProperty>();
    public List<IBehaviour> behaviours = new List<IBehaviour>();
    public GameObject gameObject;

    public Collection(int id, string name = "name", int x = 0, int y = 0, int z = 0)
    {
        ID = id;
        gameObject = new GameObject(name);
        gameObject.transform.position = new Vector3(x, y, z);
    }

    public IProperty AddProperty(IProperty property)
    {
        properties.Add(property);
        return property;
    }

    public IBehaviour AddBehaviour(IBehaviour behaviour)
    {
        behaviours.Add(behaviour);
        return behaviour;
    }

    public void RemoveProperty<T>()
    {
        for (int i = properties.Count - 1; i >= 0; i--)
        {
            if (properties[i].GetType() == typeof(T))
            {
                properties.RemoveAt(i);
            }
        }
    }

    public void RemoveBehaviour<T>()
    {
        for (int i = behaviours.Count - 1; i >= 0; i--)
        {
            if (behaviours[i].GetType() == typeof(T))
            {
                behaviours.RemoveAt(i);
            }
        }
    }

    public IProperty GetProperty<T>()
    {
        return properties.Find(item => item.GetType() == typeof(T));
    }

    public IBehaviour GetBehaviour<T>()
    {
        return behaviours.Find(item => item.GetType() == typeof(T));
    }
}

//AddProperty/Behaviour accept [params]?
//Consolidate to one function that differentiates and assigns appropriately?