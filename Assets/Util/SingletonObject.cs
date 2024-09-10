using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonObject : MonoBehaviour
{

    private static SingletonObject _instance;
    public static SingletonObject Instance
    {
        get
        {
            return _instance;
        }
    }

    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
