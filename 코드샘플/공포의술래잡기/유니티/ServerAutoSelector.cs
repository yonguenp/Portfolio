using UnityEngine;

public class ServerAutoSelector : MonoBehaviour
{
    [SerializeField] ServerSelector live_server;

    private void Start()
    {
        bool force =
#if UNITY_EDITOR
        false
#else
        true
#endif
        ;

        if (force)
            live_server.OnClick();
    }
}
