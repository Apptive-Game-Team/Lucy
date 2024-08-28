using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

public class Logger : MonoBehaviour
{
    private MonoBehaviour component;
    private bool isDebug;
    private bool reducedLog;

    private HashSet<string> lastLogTexts = new HashSet<string>();
    private Dictionary<string, Coroutine> lastLogCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, int> lastLogCounters = new Dictionary<string, int>();

    Logger(MonoBehaviour component, bool isDebug = true, bool reducedLog = true)
    {
        this.component = component;
        this.isDebug = isDebug;
        this.reducedLog = reducedLog;
    }

    IEnumerator LogCounter(string text)
    {
        yield return new WaitForSeconds(1);
        if (lastLogTexts.Contains(text))
        {
            if (lastLogCounters[text] > 1)
            {
                Debug.Log(string.Format("{0} | {1}.cs : end {2} [{3}]", component.gameObject.name, component.name, text, lastLogCounters[text]));
            }
            lastLogTexts.Remove(text);
            lastLogCounters.Remove(text);
            lastLogCoroutines.Remove(text);
        }
    }

    public void Log(string text)
    {
        if (isDebug)
        {
            if (reducedLog)
            {
                if (!lastLogTexts.Contains(text)) {

                    Debug.Log(string.Format("{0} | {1}.cs : {2}", component.gameObject.name, component.name, text));
                    lastLogCounters.Add(text, 1);
                    lastLogTexts.Add(text);
                    lastLogCoroutines.Add(text, StartCoroutine(LogCounter(text)));
                } else
                {
                    lastLogCounters[text]++;
                    StopCoroutine(lastLogCoroutines[text]);
                    lastLogCoroutines[text] = StartCoroutine(LogCounter(text));
                }
            } else
            {
                Debug.Log(string.Format("{0} | {1}.cs : {2}", component.gameObject.name, component.name, text));
            }
        }
    }

    public void Clear()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    private void Start()
    {
        Logger logger = new Logger(this, true);  
        logger.Log("tlqkf");
    }
}
