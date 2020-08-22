using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static Dictionary<string, ObjectPool> PoolDictionary = new Dictionary<string, ObjectPool>();

    void Awake()
    {
        List<ObjectPool> pools = new List<ObjectPool>(GameObject.FindObjectsOfType<ObjectPool>());
        foreach (ObjectPool pool in pools)
        {
            string index = pool.prefab.name;

            if (index == "EntityButton")
            {
                pool.ParentObject = GameObject.FindGameObjectWithTag("ContentPanel");
            }

            PoolDictionary.Add(index, pool);
        }
    }

}
