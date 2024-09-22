using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BridgeID
{
    Switch1 = 0,
    Switch2 = 1,
    Switch3 = 2,
    Switch4 = 3,
    Switch5 = 4,
    Switch6 = 5,
}

[Serializable]
public class BridgeData
{
    public BridgeID bridgeID;
}

[CreateAssetMenu(fileName = "BridgeDataList", menuName = "ScriptableObject/New BridgeDataList")]
public class BridgeDataList : ScriptableObject
{
    public List<BridgeData> bridgeDatas;
}
