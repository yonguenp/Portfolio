using UnityEngine;

public class Logger : MonoBehaviour
{
    static public void Log(string msg, int level = 0)
    {
        if(level > 0)
            Debug.Log(msg);
    }
}
