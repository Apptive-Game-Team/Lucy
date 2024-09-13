using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance;
    [SerializeField] private GameObject[] bridges;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    [SerializeField] public bool isSafe = false;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < bridges.Length; i++)
        {
        bridges[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
