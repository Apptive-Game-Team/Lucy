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

        public static List<T> ConcatenateListWithoutDuplicates<T>(List<T> list1, List<T> list2)
        {
            foreach (T value in list2)
            {
                if (!list1.Contains(value))
                {
                    list1.Add(value);
                }
            }
            return list1;
        }


        public static Vector3Int Vector3ToVector3Int(Vector3 vector)
        {
            return new Vector3Int(
                Mathf.RoundToInt(vector.x),
                Mathf.RoundToInt(vector.y),
                Mathf.RoundToInt(vector.z)
            );
        }
    }
}

