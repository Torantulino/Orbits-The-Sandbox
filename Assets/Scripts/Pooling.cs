using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : MonoBehaviour where T : Component
{

    [SerializeField]
    private T prefab;
    private Queue<T> objects = new Queue<T>();

    //get object from pool, create one if drawing more than the pool contains
    public T Pop()
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
            T newObject = Instantiate<T>(prefab);
            newObject.gameObject.SetActive(false);
            objects.Enqueue(newObject);
        }
    }

    public void ReturnToPool(T returningObject)
    {
        returningObject.gameObject.SetActive(false);
        objects.Enqueue(returningObject);
    }

}