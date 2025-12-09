using UnityEngine;

/// <summary>
/// Base class for implementing the Singleton pattern in Unity.
/// Ensures only one instance of the derived class exists across scene loads.
/// </summary>
/// <typeparam name="T">The type of the singleton MonoBehaviour</typeparam>
public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>
    /// Gets the singleton instance. Creates one if it doesn't exist.
    /// The instance persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance != null)
                {
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }

            return _instance;
        }
    }
    
    /// <summary>
    /// Unity Awake callback. Ensures singleton pattern by destroying duplicate instances.
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
