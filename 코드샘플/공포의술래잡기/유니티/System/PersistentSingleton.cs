using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<T>();
                }

                _instance.gameObject.name = "$" + _instance.GetType().ToString();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public void ClearSingleton()
    {
        if(_instance != null)
        {
            DestroyImmediate(_instance.gameObject);
        }
        _instance = null;
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_instance == null)
        {
            _instance = this as T;

            gameObject.name = "$" + _instance.GetType().ToString();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (this != _instance)
            {
                SBDebug.LogWarning("중복 PersistentSingleton : " + _instance.GetType().ToString());
                DestroyImmediate(gameObject);
            }
        }
    }
}
