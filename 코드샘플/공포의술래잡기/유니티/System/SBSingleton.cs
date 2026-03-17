using UnityEngine;

/// <summary>
/// Singleton pattern.
/// </summary>
public class SBSingleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (!hasInstance())
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<T>();
                    obj.name = "$" + _instance.GetType().ToString();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    public static bool hasInstance()
    {
        return _instance != null;

    }

    public static void ClearInstance()
    {
        if (hasInstance())
        {
            DestroyImmediate(_instance.gameObject);
            _instance = null;
        }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _instance = this as T;
    }
}
