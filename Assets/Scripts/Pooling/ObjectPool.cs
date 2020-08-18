using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [SerializeField] private GameObject prefab;
    [SerializeField] private int defaultSize = 100;
    protected Queue<GameObject> objects = new Queue<GameObject>();

    void Awake()
    {
        AddObjects(defaultSize);

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
            newObject.SetActive(false);
            objects.Enqueue(newObject);
        }
    }

    public virtual void ReturnObjectToPool(GameObject returningObject)
    {
        returningObject.SetActive(false);
        objects.Enqueue(returningObject);
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