using UnityEngine;
using System.Collections;

public class Factory
{
    private GameManager manager;

    public Factory(GameManager manager)
    {
        this.manager = manager;
    }

    public Collection CreateCollection()
    {
        int id = manager.AssignID();
        Collection collection = new Collection(id, id.ToString());
        manager.gameCollections.Add(collection);
        return collection;
    }

    public Collection PlayerCreationTest(int x = 0, int y = 0)
    {
        Collection collection = CreateCollection();
        collection.gameObject.transform.position = new Vector3(x, y, 0);
        collection.AddBehaviour(new Appearance());
        collection.AddProperty(new Mobile(10, 20));
        Solid solid = collection.AddProperty(new Solid(collection.gameObject, 9)) as Solid;
        collection.AddBehaviour(new Action(new PlayerInput(solid)));
        collection.gameObject.layer = 9;
        Appearance appearance = collection.GetBehaviour<Appearance>() as Appearance;
        appearance.renderer.sprite = Resources.Load<Sprite>("100x100square") as Sprite;
        appearance.AssignTarget(collection.gameObject.transform);
        return collection;
    }

    public Collection PatrolTest(int x = 0, int y = 0)
    {
        Collection collection = CreateCollection();
        collection.gameObject.transform.position = new Vector3(x, y, 0);
        collection.AddBehaviour(new Appearance());
        collection.AddProperty(new Mobile(10, 10, 0));
        collection.AddBehaviour(new Action(new FixedPatrol(collection.gameObject, new Vector2(1,0))));
        collection.AddProperty(new Solid(collection.gameObject, 8, 512));
        Appearance appearance = collection.GetBehaviour<Appearance>() as Appearance;
        appearance.renderer.sprite = Resources.Load<Sprite>("100x100square") as Sprite;
        appearance.AssignTarget(collection.gameObject.transform);
        return collection;
    }
}