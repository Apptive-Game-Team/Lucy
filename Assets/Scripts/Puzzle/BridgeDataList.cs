using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BridgeID
{
    Switch1 = 1,
    Switch2 = 2,
    Switch3 = 3,
    Switch4 = 4,
    Switch5 = 5,
    Switch6 = 6,
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
