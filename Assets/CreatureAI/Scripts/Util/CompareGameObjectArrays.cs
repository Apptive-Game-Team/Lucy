using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareGameObjectArrays : MonoBehaviour
{
    public static bool AreGameObjectArraysEqual(GameObject[] array1, GameObject[] array2)
    {
        if (array1.Length != array2.Length)
            return false;


        Dictionary<GameObject, int> dict1 = new Dictionary<GameObject, int>();

        foreach (GameObject obj in array1)
        {
            if (dict1.ContainsKey(obj))
                dict1[obj]++;
            else
                dict1[obj] = 1;
        }

        foreach (GameObject obj in array2)
        {
            if (dict1.ContainsKey(obj))
            {
                dict1[obj]--;
                if (dict1[obj] == 0)
                    dict1.Remove(obj);
            }
            else
            {
                return false;
            }
        }

        return dict1.Count == 0;
    }
}
