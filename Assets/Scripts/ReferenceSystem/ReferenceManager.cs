using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public MonoBehaviour FindByName(string key)
    {
        return components[key].Item1;
    }
}
