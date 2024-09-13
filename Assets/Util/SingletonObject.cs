using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
    T t;

    public static T _instance;
    public static T Instance
    {
        get
        {
            _instance = FindObjectOfType<T>();
            
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).ToString());
                _instance = singletonObject.AddComponent<T>();
            }

            DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }
}
