using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReferenceManager : SingletonObject<ReferenceManager>
{
    private Dictionary<string, (MonoBehaviour, bool)> components = new Dictionary<string, (MonoBehaviour, bool)>();

    public void SetReferableObject(string key, MonoBehaviour component, bool isDontDestory)
    {
        try
        {
            components.Add(key, (component, isDontDestory));
        }
        catch
        {
            components[key] = (component, isDontDestory);
        }
        
    }

    public T FindComponentByName<T>(string key) where T : class
    {
        try
        {
            return components[key].Item1 as T;
        }
        catch
        {
            return null;
        }
    }

    public GameObject FindGameObjectByName(string key)
    {
        return components[key].Item1.gameObject;
    }
}
