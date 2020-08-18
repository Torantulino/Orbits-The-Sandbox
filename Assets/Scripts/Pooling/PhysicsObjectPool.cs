using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectPool : ObjectPool
{
    public void ReturnObjectToPool(PhysicsObject returningObject)
    {
        //set defaults
        returningObject.rb.mass = returningObject.defaultSettings.mass;
        returningObject.gameObject.transform.localScale = returningObject.defaultSettings.scale;
        returningObject.rb.velocity = returningObject.defaultSettings.velocity;
        returningObject.temperature = returningObject.defaultSettings.temperature;
        returningObject.trailRenderer.Clear();

        returningObject.gameObject.SetActive(false);
        objects.Enqueue(returningObject.gameObject);
    }

}