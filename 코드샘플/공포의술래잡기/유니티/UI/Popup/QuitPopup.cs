using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitPopup : Popup
{
    public override void Close()
    {
        base.Close();
    }
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
    }

    public void OnQuitBtn()
    {
#if !UNITY_EDITOR
        Application.Quit();
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
