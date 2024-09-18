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
    MoveLeft,
    OpenInventory,
    SelectClick,
}

public class InputManager : SingletonObject<InputManager>
{
    private const float KEY_LISTENER_DELAY = 0.05f;
    private const float KET_DOWN_DELAY = 1f;


    private Dictionary<ActionCode, bool> keyDownBools = new Dictionary<ActionCode, bool>();
    private Dictionary<ActionCode, bool> keyDownBoolsForListener = new Dictionary<ActionCode, bool>();
    private Dictionary<ActionCode, Coroutine> keyDownCounterCoroutine = new Dictionary<ActionCode, Coroutine>();
    private Dictionary<ActionCode, bool> keyActiveFlags = new Dictionary<ActionCode, bool>();
    private Dictionary<ActionCode, KeyCode> keyMappings = new Dictionary<ActionCode, KeyCode>()
    {
        { ActionCode.Interaction, KeyCode.Z },
        { ActionCode.MoveUp, KeyCode.UpArrow },
        { ActionCode.MoveDown, KeyCode.DownArrow },
        { ActionCode.MoveRight, KeyCode.RightArrow },
        { ActionCode.MoveLeft, KeyCode.LeftArrow },
        { ActionCode.OpenInventory, KeyCode.I },
        { ActionCode.SelectClick, KeyCode.Mouse0 },
    };

    Vector3 moveVector = new Vector3();
    private List<Vector2> directionList = new List<Vector2>();

    private List<IKeyInputListener> inputListeners = new List<IKeyInputListener>();

    public bool isMoveActioncode(ActionCode action)
    {
        return (int)action >= (int)ActionCode.MoveUp && (int)action <= (int)ActionCode.MoveLeft;
    }

    private void Start()
    {
        InitKeyDownDictionarys();
        StartCoroutine(CallListenersCoroutine());
    }

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
        if (Input.GetKey(keyMappings[ActionCode.MoveUp]))
        {
            if (!directionList.Contains(Vector2.up))
            {
                directionList.Add(Vector2.up);
            }
        }
        else if (!Input.GetKey(keyMappings[ActionCode.MoveUp]))
        {
            directionList.Remove(Vector2.up);
        }

        if (Input.GetKey(keyMappings[ActionCode.MoveDown]))
        {
            if (!directionList.Contains(Vector2.down))
            {
                directionList.Add(Vector2.down);
            }
        }
        else if (!Input.GetKey(keyMappings[ActionCode.MoveDown]))
        {
            directionList.Remove(Vector2.down);
        }

        if (Input.GetKey(keyMappings[ActionCode.MoveLeft]))
        {
            if (!directionList.Contains(Vector2.left))
            {
                directionList.Add(Vector2.left);
            }
        }
        else if (!Input.GetKey(keyMappings[ActionCode.MoveLeft]))
        {
            directionList.Remove(Vector2.left);
        }

        if (Input.GetKey(keyMappings[ActionCode.MoveRight]))
        {
            if (!directionList.Contains(Vector2.right))
            {
                directionList.Add(Vector2.right);
            }
        }
        else if (!Input.GetKey(keyMappings[ActionCode.MoveRight]))
        {
            directionList.Remove(Vector2.right);
        }

        if (directionList.Count > 0)
        {
            moveVector.x = directionList[^1].x;
            moveVector.y = directionList[^1].y;
        }
        else
        {
            moveVector.x = 0;
            moveVector.y = 0;
        }

        return moveVector;
    }

    public bool GetKey(ActionCode action)
    {
        return (Input.GetKey(keyMappings[action]) && keyActiveFlags[action]);
    }

    IEnumerator KeyDownCounter(ActionCode action)
    {
        yield return new WaitForSeconds(KET_DOWN_DELAY);
        keyDownBools[action] = false;
        keyDownCounterCoroutine[action] = null;
    }

    

    void Update()
    {
        foreach (ActionCode action in keyMappings.Keys)
        {
            if (keyActiveFlags[action])
            {
                if (Input.GetKeyDown(keyMappings[action]))
                {
                    keyDownBools[action] = true;
                    keyDownBoolsForListener[action] = true;
                    Coroutine tempCoroutine = keyDownCounterCoroutine[action];
                    if (tempCoroutine != null)
                    {
                        StopCoroutine(tempCoroutine);
                    }
                    keyDownCounterCoroutine[action] = StartCoroutine(KeyDownCounter(action));
                }
            }
        }

    }

    public void SetKeyListener(IKeyInputListener listener)
    {
        inputListeners.Add(listener);
    }

    private void InitKeyDownDictionarys()
    {
        foreach (ActionCode action in Enum.GetValues(typeof(ActionCode)))
        {
            keyDownBools.Add(action, false);
            keyDownCounterCoroutine.Add(action, null);
            keyActiveFlags.Add(action, true);
            keyDownBoolsForListener.Add(action, false);
        }
    }

    IEnumerator CallListenersCoroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(KEY_LISTENER_DELAY);
            foreach (ActionCode action in keyMappings.Keys)
            {
                if (keyActiveFlags[action])
                {
                    if (keyDownBoolsForListener[action])
                    {
                        keyDownBoolsForListener[action] = false;
                        CallOnKeyDownListeners(action);
                    }
                    else if (Input.GetKey(keyMappings[action]))
                    {
                        CallOnKeyListeners(action);
                    }
                    else if (Input.GetKeyUp(keyMappings[action]))
                    {
                        CallOnKeyUpListeners(action);
                    }
                }

            }
        }
    }


    private void CallOnKeyListeners(ActionCode action)
    {
        foreach (IKeyInputListener listener in inputListeners)
        {
            listener.OnKey(action);

        }
    }

    private void CallOnKeyDownListeners(ActionCode action)
    {
        foreach (IKeyInputListener listener in inputListeners)
        {
            listener.OnKeyDown(action);

        }
    }

    private void CallOnKeyUpListeners(ActionCode action)
    {
        foreach (IKeyInputListener listener in inputListeners)
        {
            listener.OnKeyUp(action);

        }
    }
}
