using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [SerializeField] public GameObject prefab;
    [SerializeField] public GameObject ParentObject;
    [SerializeField] private int startSize = 100;
    [SerializeField] private int actualSize;

    protected Queue<GameObject> objects = new Queue<GameObject>();

    protected UIManager ui;

    void Start()
    {
        ui = FindObjectOfType<UIManager>();
        if (ui == null)
            Debug.Log("UiManager not found by " + this.name + "!");

        if (ParentObject == null)
            ParentObject = gameObject;
        AddObjects(startSize);
    }

    void LateUpdate()
    {
        //actualSize = objects.Count;
    }

    //get object from pool, create one if drawing more than the pool contains
    private GameObject PopObject()
    {
        if (objects.Count == 0)
        {
            AddObjects(1);
        }

        return objects.Dequeue();
    }

    //add a number of objects to the pool
    public void AddObjects(int newObjectNumber)
    {
        for (int i = 0; i < newObjectNumber; i++)
        {
            GameObject newObject = Instantiate<GameObject>(prefab);
            newObject.transform.SetParent(ParentObject.transform, false);
            newObject.SetActive(false);
            objects.Enqueue(newObject);
        }

        actualSize += newObjectNumber;
    }

    //non physics objects are simply returned
    public virtual void ReturnObjectToPool(GameObject returningObject)
    {
        returningObject.SetActive(false);
        objects.Enqueue(returningObject);
    }

    //physics objects are properly destructed
    public void ReturnObjectToPool(PhysicsObject returningObject)
    {
        //set defaults
        returningObject.rb.mass = returningObject.defaultSettings.mass;
        returningObject.gameObject.transform.localScale = returningObject.defaultSettings.scale;
        returningObject.rb.velocity = returningObject.defaultSettings.velocity;
        returningObject.temperature = returningObject.defaultSettings.temperature;
        returningObject.trailRenderer.Clear();
        returningObject.Shattered = false;

        //remove UI entity panel
        ui.RemoveFromEntitiesPanel(returningObject.gameObject);

        returningObject.gameObject.SetActive(false);
        objects.Enqueue(returningObject.gameObject);

    }

    // Two overloads so that they can be used the similarly to Instantiate.
    public GameObject SpawnFromPool(Vector3 location, Quaternion rotation)
    {
        GameObject spawnObject = PopObject();
        spawnObject.transform.position = location;
        spawnObject.transform.rotation = rotation;
        spawnObject.SetActive(true);
        return spawnObject;
    }
}