using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class FixedSizedQueue<T> : Queue<T>
{
    private readonly object syncObject = new object();

    public int MaxSize { get; private set; }

    public FixedSizedQueue(int maxSize)
    {
        MaxSize = maxSize;
    }

    public new void Enqueue(T obj)
    {
        base.Enqueue(obj);
        lock (syncObject)
        {
            while (base.Count > MaxSize)
            {
                T outObj = base.Dequeue();
            }
        }
    }

    public void setMaxSize(int size)
    {
        MaxSize = size;
    }
}