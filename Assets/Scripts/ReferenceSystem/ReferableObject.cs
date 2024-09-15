using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferableObject : MonoBehaviour
{
    protected virtual void Awake()
    {
        ReferenceManager.Instance.SetReferableObject(gameObject.name, this, false);
    }
}
