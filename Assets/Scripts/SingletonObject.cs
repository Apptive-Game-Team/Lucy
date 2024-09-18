using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    // �̱��� �ν��Ͻ� ���� ������Ƽ
    public static T Instance
    {
        get
        {
            // �ν��Ͻ��� �̹� �����ϴ��� Ȯ��
            if (_instance == null)
            {
                // �� ������ �ش� Ÿ���� ������Ʈ�� ã��
                _instance = FindObjectOfType<T>();

                // ���� ���� ���ٸ�, ���� ����
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
                else
                {
                    // �̹� �����ϴ� ���, �ߺ��Ǵ� ������Ʈ ����
                    T[] temp = FindObjectsOfType<T>();
                    foreach (T obj in temp)
                    {
                        if (!obj.Equals(_instance))
                        {
                            Destroy(obj.gameObject); // �ߺ� ������Ʈ ����
                        }
                    }
                }
            }

            return _instance;
        }
    }
    // �ν��Ͻ��� ������ �� �ʱ�ȭ
    protected virtual void Awake()
    {
        // ������ �ν��Ͻ��� ������ ���� ������Ʈ ����
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject); // �ش� ������Ʈ�� �� ��ȯ �Ŀ��� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ��� �̱��� ������Ʈ ����
        }
    }
}
