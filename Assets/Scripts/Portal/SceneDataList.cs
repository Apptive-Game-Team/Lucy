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
    FLOOR_MAZE = 5,
    ENDING_SCENE_MID_VER = 6,
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

    //���� ���� - ���۽� �ε�� �� ����Ʈ�� �� ������ ����Ʈ�� �� �� ������ ������ �Ǵ°�? �ȵǸ� ���� �޼��� ���
    [SerializeField]
    public List<SceneData> sceneDatas;

    //���Ǽ��� ������
    public SceneData GetSceneDataByID(SceneID id)
    {
        SceneData sceneData = sceneDatas.Find(x => x.sceneID == id);
        if (sceneData == null) Debug.LogErrorFormat("There's No Scene! Please Check SceneDataList!");
        return sceneData;
    }
}