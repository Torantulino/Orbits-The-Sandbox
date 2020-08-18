using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectPool : ObjectPool
{
    public void ReturnObjectToPool(PhysicsObject returningObject, PhysicsObjectDefaults defaults)
    {
        //set defaults
        returningObject.rb.mass = defaults.mass;
        returningObject.gameObject.transform.localScale = new Vector3(defaults.size, defaults.size, defaults.size);
        returningObject.temperature = defaults.temperature;
        returningObject.trailRenderer.Clear();

        returningObject.gameObject.SetActive(false);
        objects.Enqueue(returningObject.gameObject);
    }

}