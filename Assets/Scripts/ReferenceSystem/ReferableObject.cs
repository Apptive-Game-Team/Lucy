using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferableObject : MonoBehaviour
{
    protected void Awake()
    {
        ReferenceManager.Instance.SetReferableObject(gameObject.name, this, false);
    }
}
