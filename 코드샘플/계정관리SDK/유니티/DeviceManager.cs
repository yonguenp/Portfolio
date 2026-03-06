using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    protected static DeviceManager _instance;
    public static DeviceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DeviceManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(DeviceManager).Name;
                    _instance = obj.AddComponent<DeviceManager>();
                }
            }
            return _instance;
        }
    }

    public DeviceEnvironment deviceEnvironment = DeviceEnvironment.None;

    protected void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        deviceEnvironment = DeviceEnvironment.Google_Mobile;
#elif UNITY_IOS && !UNITY_EDITOR
        deviceEnvironment = DeviceEnvironment.IOS_Mobile;
#elif UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
        deviceEnvironment = DeviceEnvironment.None_Mobile;
#else
        Debug.Log("Not in android, but it called with GG");
#endif
        Debug.Log($"Connected deviceEnviromnet :: {deviceEnvironment}");
    }
}
