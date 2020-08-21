using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static Dictionary<string, ObjectPool> PoolDictionary = new Dictionary<string, ObjectPool>();

    void Awake()
    {
        PoolDictionary.Add("shards", GameObject.Find("ShardPool").GetComponent<ObjectPool>());
        PoolDictionary.Add("entityPanels", GameObject.Find("EntityPanelPool").GetComponent<ObjectPool>());
    }

}
