using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PuzzleManager2F : SingletonObject<PuzzleManager2F>, ISceneChangeListener
{
    [SerializeField] BridgeDataList bridgeDataList;
    private List<GameObject> bridges;
    private float CallBridgeActiveDelay = 1f;
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

        foreach (GameObject bridge in bridges)
        {
            Bridge_Puzzle2F bridgeComponent = bridge.GetComponent<Bridge_Puzzle2F>();
            if (isOns.ContainsKey(bridgeComponent.bridgeID))
            {
                bridge.SetActive(isOns[bridgeComponent.bridgeID]);
            }
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
