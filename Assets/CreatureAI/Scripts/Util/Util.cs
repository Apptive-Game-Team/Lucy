using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team6203
{
    public class Util
    {
        public static bool AreArraysEqual<T>(T[] array1, T[] array2)
        {
            if (array1.Length != array2.Length)
                return false;


            Dictionary<T, int> dict1 = new Dictionary<T, int>();

            foreach (T obj in array1)
            {
                if (dict1.ContainsKey(obj))
                    dict1[obj]++;
                else
                    dict1[obj] = 1;
            }

            foreach (T obj in array2)
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
}
