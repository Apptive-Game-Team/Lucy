using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    // 싱글톤 인스턴스 접근 프로퍼티
    public static T Instance
    {
        get
        {
            // 인스턴스가 이미 존재하는지 확인
            if (_instance == null)
            {
                // 씬 내에서 해당 타입의 오브젝트를 찾음
                _instance = FindObjectOfType<T>();

                // 만약 씬에 없다면, 새로 생성
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
                else
                {
                    // 이미 존재하는 경우, 중복되는 오브젝트 제거
                    T[] temp = FindObjectsOfType<T>();
                    foreach (T obj in temp)
                    {
                        if (!obj.Equals(_instance))
                        {
                            Destroy(obj.gameObject); // 중복 오브젝트 삭제
                        }
                    }
                }
            }

            return _instance;
        }
    }
    // 인스턴스가 생성될 때 초기화
    protected virtual void Awake()
    {
        // 기존에 인스턴스가 있으면 현재 오브젝트 삭제
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject); // 해당 오브젝트를 씬 전환 후에도 유지
        }
        else
        {
            Destroy(gameObject); // 중복된 싱글톤 오브젝트 삭제
        }
    }
}
