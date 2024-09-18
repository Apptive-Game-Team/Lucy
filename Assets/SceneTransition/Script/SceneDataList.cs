using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneID
{
    NONE = 0,
    FLOOR_FIRST = 1,
    STAIR_FIRST = 2,
    FLOOR_SECOND = 3,
    PUZZLE_2F = 4,
}


[Serializable]
public class SceneData
{
    public SceneID sceneID;
    public string sceneName;

}

[CreateAssetMenu(fileName = "SceneDataList", menuName = "ScriptableObject/New SceneDataList")]
public class SceneDataList : ScriptableObject
{

    //구현 예정 - 시작시 로드된 씬 리스트에 씬 데이터 리스트의 각 씬 네임이 대응이 되는가? 안되면 에러 메세지 출력

    public List<SceneData> sceneDatas;

    //강건성이 떨어짐
    public SceneData GetSceneDataByID(SceneID id)
    {
        SceneData sceneData = sceneDatas.Find(x => x.sceneID == id);
        if (sceneData == null) Debug.LogErrorFormat("There's No Scene! Please Check SceneDataList!");
        return sceneData;
    }
}