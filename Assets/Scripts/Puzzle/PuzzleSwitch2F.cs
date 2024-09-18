using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleSwitch2F : MonoBehaviour
{
    [SerializeField] private GameObject[] bridges;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            StartCoroutine(OnBridge());
        }
    }
    private IEnumerator OnBridge()
    {
        yield return new WaitForSecondsRealtime(1f);
        for (int i = 0; i < bridges.Length; i++)
        {
            bridges[i].SetActive(true);
        }
    }
}
