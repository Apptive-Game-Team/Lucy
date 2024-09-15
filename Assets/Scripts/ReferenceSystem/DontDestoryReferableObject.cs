using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestoryReferableObject : ReferableObject
{
    protected override void Awake()
    {
        ReferenceManager.Instance.SetReferableObject(gameObject.name, this, true);
        DontDestroyOnLoad(gameObject);
    }
}
