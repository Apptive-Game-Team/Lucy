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

    Detector detector;

    private void Awake()
    {
        detector = gameObject.GetComponent<Detector>();
    }

    private void Start()
    {
        if(handlightObject == null)
        {
            Debug.LogError("HandLight Object�� ���� �����ϴ�! ������ player ������ �ִ� ������Ʈ�� ã�Ƽ� �������� �ּ���!");
        }    
    }
    
    void Update()
    {
        Vector3 direction = InputManager.Instance.GetMoveVector();
        if (!direction.Equals(Vector3.zero))
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            handlightObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            detector.SetLookingDirection(direction);
        }
    }
}
