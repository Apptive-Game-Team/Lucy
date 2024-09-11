using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class HandLightComponent : MonoBehaviour 
{
    public abstract void InitComponent();
}

public class HandLightRotator : MonoBehaviour, IKeyInputListener
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
        InputManager.Instance.SetKeyListener(this);
        if(handlightObject == null)
        {
            Debug.LogError("HandLight Object�� ���� �����ϴ�! ������ player ������ �ִ� ������Ʈ�� ã�Ƽ� �������� �ּ���!");
        }    
    }
    void IKeyInputListener.OnKey(ActionCode action)
    {
        if ((int) action >= (int)ActionCode.MoveUp && (int) action <= (int)ActionCode.MoveLeft)
        {
            Vector3 direction = InputManager.Instance.GetMoveVector();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            handlightObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            detector.SetLookingDirection(direction);
        }
    }
}
