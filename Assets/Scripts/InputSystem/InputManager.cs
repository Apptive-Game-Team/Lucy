using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// Action codes representing different player inputs in the game.
    /// </summary>
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

    /// <summary>
    /// Centralized input management system that handles key bindings, input detection,
    /// and notifies registered listeners of key events.
    /// Supports key remapping and enables/disables specific actions dynamically.
    /// </summary>
    public class InputManager : SingletonObject<InputManager>
    {
        private const float KEY_LISTENER_DELAY = 0.05f;
        private const float KET_DOWN_DELAY = 1f;


        private readonly Dictionary<ActionCode, bool> keyDownBools = new Dictionary<ActionCode, bool>();
        private readonly Dictionary<ActionCode, bool> keyDownBoolsForListener = new Dictionary<ActionCode, bool>();
        private readonly Dictionary<ActionCode, bool> keyUpBoolsForListener = new Dictionary<ActionCode, bool>();
        private readonly Dictionary<ActionCode, Coroutine> keyDownCounterCoroutine = new Dictionary<ActionCode, Coroutine>();
        private readonly Dictionary<ActionCode, bool> keyActiveFlags = new Dictionary<ActionCode, bool>();
        private readonly Dictionary<ActionCode, KeyCode> keyMappings = new Dictionary<ActionCode, KeyCode>()
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
        private readonly List<Vector2> directionList = new List<Vector2>();

        private readonly List<IKeyInputListener> inputListeners = new List<IKeyInputListener>();

        public bool isMoveActioncode(ActionCode action)
        {
            return (int)action >= (int)ActionCode.MoveUp && (int)action <= (int)ActionCode.MoveLeft;
        }

        protected override void Awake()
        {
            base.Awake();
            // Init in Awake: other scripts read key states from their own Start().
            InitKeyDownDictionarys();
        }

        private void Start()
        {
            StartCoroutine(CallListenersCoroutine());
        }

        public void SetKeyActive(ActionCode action, bool active)
        {
            keyActiveFlags[action] = active;
        }

        public void SetMovementState(bool active)
        {
            SetKeyActive(ActionCode.MoveUp, active);
            SetKeyActive(ActionCode.MoveDown, active);
            SetKeyActive(ActionCode.MoveLeft, active);
            SetKeyActive(ActionCode.MoveRight, active);
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
                if (!directionList.Contains(Vector2.up))
                {
                    directionList.Add(Vector2.up);
                }
            }
            else
            {
                directionList.Remove(Vector2.up);
            }

            if (GetKey(ActionCode.MoveDown))
            {
                if (!directionList.Contains(Vector2.down))
                {
                    directionList.Add(Vector2.down);
                }
            }
            else
            {
                directionList.Remove(Vector2.down);
            }

            if (GetKey(ActionCode.MoveLeft))
            {
                if (!directionList.Contains(Vector2.left))
                {
                    directionList.Add(Vector2.left);
                }
            }
            else
            {
                directionList.Remove(Vector2.left);
            }

            if (GetKey(ActionCode.MoveRight))
            {
                if (!directionList.Contains(Vector2.right))
                {
                    directionList.Add(Vector2.right);
                }
            }
            else
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

                    // GetKeyUp is only true for a single frame, so it has to be latched here
                    // instead of polled from CallListenersCoroutine.
                    if (Input.GetKeyUp(keyMappings[action]))
                    {
                        keyUpBoolsForListener[action] = true;
                    }
                }
            }

        }

        public void SetKeyListener(IKeyInputListener listener)
        {
            if (!inputListeners.Contains(listener))
            {
                inputListeners.Add(listener);
            }
        }

        public void RemoveKeyListener(IKeyInputListener listener)
        {
            inputListeners.Remove(listener);
        }

        private void InitKeyDownDictionarys()
        {
            foreach (ActionCode action in Enum.GetValues(typeof(ActionCode)))
            {
                keyDownBools.Add(action, false);
                keyDownCounterCoroutine.Add(action, null);
                keyActiveFlags.Add(action, true);
                keyDownBoolsForListener.Add(action, false);
                keyUpBoolsForListener.Add(action, false);
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

                        if (keyUpBoolsForListener[action])
                        {
                            keyUpBoolsForListener[action] = false;
                            CallOnKeyUpListeners(action);
                        }
                    }

                }
            }
        }


        /// <summary>
        /// Calls every registered listener, dropping the ones whose GameObject was destroyed
        /// (scene change) and swallowing listener exceptions.
        /// Without this a single dead listener kills CallListenersCoroutine and all input stops.
        /// Iterates backwards so removals and registrations during the call are safe.
        /// </summary>
        private void CallListeners(ActionCode action, Action<IKeyInputListener> call)
        {
            for (int i = inputListeners.Count - 1; i >= 0; i--)
            {
                IKeyInputListener listener = inputListeners[i];
                if (listener == null || (listener is MonoBehaviour behaviour && behaviour == null))
                {
                    inputListeners.RemoveAt(i);
                    continue;
                }

                try
                {
                    call(listener);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        private void CallOnKeyListeners(ActionCode action)
        {
            CallListeners(action, listener => listener.OnKey(action));
        }

        private void CallOnKeyDownListeners(ActionCode action)
        {
            CallListeners(action, listener => listener.OnKeyDown(action));
        }

        private void CallOnKeyUpListeners(ActionCode action)
        {
            CallListeners(action, listener => listener.OnKeyUp(action));
        }
    }
}