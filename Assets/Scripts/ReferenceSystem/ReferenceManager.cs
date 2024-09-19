using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReferenceManager : SingletonObject<ReferenceManager>
{

    private Dictionary<string, int> keyToIndex = new Dictionary<string, int>();
    [SerializeField]
    private List<MonoBehaviour> indexToComponents = new List<MonoBehaviour>();
    //private Dictionary<string, (MonoBehaviour, bool)> components = new Dictionary<string, (MonoBehaviour, bool)>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetReferableObject(string key, MonoBehaviour component, bool isDontDestory)
    {
        if (keyToIndex.ContainsKey(key))
        {
            indexToComponents[keyToIndex[key]] = component;
        } else
        {
            int index = indexToComponents.Count;
            indexToComponents.Add(component);
            keyToIndex.Add(key, index);
        }
    }

    public T FindComponentByName<T>(string key) where T : class
    {
        try
        {
            return indexToComponents[keyToIndex[key]] as T;
        }
        catch
        {
            return null;
        }
    }

    public GameObject FindGameObjectByName(string key)
    {
        return indexToComponents[keyToIndex[key]].gameObject;
    }
}
