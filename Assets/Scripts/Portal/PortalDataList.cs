using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PortalID
{
    NONE = 0,
    FLOOR_FIRST_TO_STAIR,
    STAIR_FIRST_TO_FLOOR_FIRST,
    STAIR_FIRST_TO_FLOOR_SECOND,
    FLOOR_SECOND_TO_STAIR_SECOND,
    FLOOR_SECOND_TO_PUZZLE_2F_RIGHT,
    FLOOR_SECOND_TO_PUZZLE_2F_LEFT,
    PUZZLE_2F_RIGHT_TO_FLOOR_SECOND,
    PUZZLE_2F_LEFT_TO_FLOOR_SECOND,
    FLOOR_SECOND_TO_FLOOR_MAZE,
    FLOOR_MAZE_TO_FLOOR_SECOND,
    FLOOR_MAZE_TO_SCENE_ENDING,
}

[Serializable]
public class PortalData
{
    public PortalID portalID;
    public SceneID sceneID;
    public Vector3 destination;
}

[CreateAssetMenu(fileName = "PortalDataList", menuName = "ScriptableObject/New PortalDataList")]
public class PortalDataList : ScriptableObject
{

    public List<PortalData> portalDatas;

    //°­°Ç¼ºÀÌ ¶³¾îÁü
    public PortalData GetPortalDataByID(PortalID id)
    {
        PortalData sceneData = portalDatas.Find(x => x.portalID == id);
        if (sceneData == null) Debug.LogErrorFormat("There's No Portal! Please Check PortalDataList!");
        return sceneData;
    }
}