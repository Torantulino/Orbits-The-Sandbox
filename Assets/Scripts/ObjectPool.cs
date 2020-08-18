using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [SerializeField] private GameObject prefab;
    [SerializeField] private int defaultSize = 50;
    private Queue<GameObject> objects = new Queue<GameObject>();

    //get object from pool, create one if drawing more than the pool contains
    public GameObject Pop()
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
            newObject.gameObject.SetActive(false);
            objects.Enqueue(newObject);
        }
    }

    public void ReturnObjectToPool(GameObject returningObject)
    {
        returningObject.SetActive(false);
        objects.Enqueue(returningObject);
    }

}