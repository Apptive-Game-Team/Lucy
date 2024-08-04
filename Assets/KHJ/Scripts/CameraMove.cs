using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject Character;

    void Update()
    {
        CameraPosition();
    }

    private void CameraPosition()
    {
        Camera.main.transform.position = Character.transform.position + new Vector3(0,0,-10);
        Camera.main.transform.LookAt(Character.transform);
    }
}
