using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager2F : SingletonObject<PuzzleManager2F>, ISceneChangeListener
{
    [SerializeField] BridgeDataList bridgeDataList;
    private List<GameObject> bridges;
    private float CallBridgeActiveDelay = 1f;
    private GameObject square;
    private GameObject player;
    Dictionary<BridgeID, bool> isOns = new Dictionary<BridgeID, bool>();

    void InitIsOns()
    {
        foreach (BridgeID id in Enum.GetValues(typeof(BridgeID)))
        {
            isOns.Add(id, false);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        InitIsOns();
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        PortalManager.Instance.SetSceneChangeListener(this);
    }
    // Start is called before the first frame update
    void ISceneChangeListener.OnSceneChange()
    {
        bridges = new List<GameObject>(GameObject.FindGameObjectsWithTag("Bridge"));
        square = GameObject.Find("Square");
        player = GameObject.Find("Player");
        foreach (GameObject bridge in bridges)
        {
            Bridge_Puzzle2F bridgeComponent = bridge.GetComponent<Bridge_Puzzle2F>();
            if (isOns.ContainsKey(bridgeComponent.bridgeID))
            {
                bridge.SetActive(isOns[bridgeComponent.bridgeID]);
            }
        }
        if(player.transform.position.x > -40f && SceneManager.GetActiveScene().name == "Puzzle_2F")
        {
            square.transform.position = new Vector3(12.06f, 0.13f, 0f);
        }
        else if(player.transform.position.x <= -40f && SceneManager.GetActiveScene().name == "Puzzle_2F")
        {
            square.transform.position = new Vector3(-92.0f, 0.13f, 0f);
        }
    }

    private IEnumerator CallActivateBridge(BridgeID bridgeID)
    {
        yield return new WaitForSeconds(CallBridgeActiveDelay);
        foreach (GameObject bridge in bridges)
        {
            Bridge_Puzzle2F bridgeComponent = bridge.GetComponent<Bridge_Puzzle2F>();

            if (bridgeComponent.bridgeID == bridgeID)
            {
                bridge.SetActive(true);
                isOns[bridgeID] = true;
            }
        }
    }

    public void ActivateBridge(BridgeID bridgeID)
    {
        StartCoroutine(CallActivateBridge(bridgeID));
    }
}
