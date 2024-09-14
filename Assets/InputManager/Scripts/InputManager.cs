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
                    if (GetKeyDown(action))
                    {
                        CallOnKeyDownListeners(action);
                    }
                    else if (Input.GetKey(keyMappings[action]))
                    {
                        CallOnKeyListeners(action);
                    }
                    else if (Input.GetKeyDown(keyMappings[action]))
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
