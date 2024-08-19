using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class HandLightComponent : MonoBehaviour 
{
    public abstract void InitComponent();
}

public class HandLightRotator : MonoBehaviour
{
    [Tooltip("Please Find HandLight GameObject and Refer on it!")]
    [SerializeField] GameObject handlightObject;

    private void Start()
    {
     if(handlightObject == null)
        {
            Debug.LogError("HandLight Object�� ���� �����ϴ�! ������ player ������ �ִ� ������Ʈ�� ã�Ƽ� �������� �ּ���!");
        }    
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            handlightObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if(Input.GetKey(KeyCode.A))
        {
            handlightObject.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if(Input.GetKey(KeyCode.S))
        {
            handlightObject.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if(Input.GetKey(KeyCode.D))
        {
            handlightObject.transform.rotation = Quaternion.Euler(0, 0, 270);
        }
    }
}
