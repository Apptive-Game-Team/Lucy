using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ActionCode
{
    Interaction,
    MoveUp,
    MoveDown,
    MoveRight,
    MoveLeft
}

public class InputManager : MonoBehaviour
{
    #region Singleton
    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(InputManager).ToString());
                    _instance = singletonObject.AddComponent<InputManager>();

                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); 
        }
    }
    #endregion

    private void Start()
    {
        InitKeyDownDictionarys();
    }

    private Dictionary<ActionCode, KeyCode> keyMappings = new Dictionary<ActionCode, KeyCode>()
    {
        { ActionCode.Interaction, KeyCode.Z },
        { ActionCode.MoveUp, KeyCode.UpArrow },
        { ActionCode.MoveDown, KeyCode.DownArrow },
        { ActionCode.MoveRight, KeyCode.RightArrow },
        { ActionCode.MoveLeft, KeyCode.LeftArrow }
    };

    private Dictionary<ActionCode, bool> keyDownBools = new Dictionary<ActionCode, bool>();

    private Dictionary<ActionCode, Coroutine> keyDownCounterCoroutine = new Dictionary<ActionCode, Coroutine>();

    private Dictionary<ActionCode, bool> keyActiveFlags = new Dictionary<ActionCode, bool>();

    Vector3 moveVector = new Vector3();

    public void SetKeyActive(ActionCode action, bool active)
    {
        keyActiveFlags[action] = active;
    }

    public bool GetKeyActive(ActionCode action)
    {
        return keyActiveFlags[action];
    }

    public void SetKey(ActionCode actionCode, KeyCode newKey)
    {
        if (keyMappings.ContainsKey(actionCode))
        {
            keyMappings[actionCode] = newKey;
        }
    }

    public bool GetKeyDown(ActionCode action)
    {
        if (keyDownBools[action])
        {
            keyDownBools[action] = false;
            return true;
        } else
        {
            return false;
        }
    }

    public Vector3 GetMoveVector()
    {
        if (GetKey(ActionCode.MoveUp))
        {
            moveVector.y = 1;
        } else if (GetKey(ActionCode.MoveDown))
        {
            moveVector.y = -1;
        } else
        {
            moveVector.y = 0;
        }

        if (GetKey(ActionCode.MoveLeft))
        {
            moveVector.x = -1;
        } else if (GetKey(ActionCode.MoveRight))
        {
            moveVector.x = +1;
        } else
        {
            moveVector.x = 0;
        }

        return moveVector;
    }

    public bool GetKey(ActionCode action)
    {
        return (Input.GetKey(keyMappings[action]) && keyActiveFlags[action]);
    }

    IEnumerator KeyDownCounter(ActionCode action)
    {
        yield return new WaitForSeconds(0.5f);
        keyDownBools[action] = false;
        keyDownCounterCoroutine[action] = null;
    }

    void Update()
    {
        foreach (ActionCode action in keyMappings.Keys)
        {
            if (Input.GetKeyDown(keyMappings[action]) && keyActiveFlags[action])
            {
                keyDownBools[action] = true;
                Coroutine tempCoroutine = keyDownCounterCoroutine[action];
                if (tempCoroutine != null)
                {
                    StopCoroutine(tempCoroutine);
                }
                keyDownCounterCoroutine[action] = StartCoroutine(KeyDownCounter(action));
            }
        }

    }

    private void InitKeyDownDictionarys()
    {
        foreach (ActionCode action in Enum.GetValues(typeof(ActionCode)))
        {
            keyDownBools.Add(action, false);
            keyDownCounterCoroutine.Add(action, null);
            keyActiveFlags.Add(action, true);
        }
    }
}
