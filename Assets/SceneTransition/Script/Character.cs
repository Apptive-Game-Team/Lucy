using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : SingletonObject<Character>
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
