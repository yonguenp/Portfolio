using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SBWebView : MonoBehaviour
{
    [SerializeField] protected GameObject targetPrefab = null;

    public abstract bool InitializeWebview(string url, GameObject parent, Action<string> cb = null, Action<string> started = null);
    public abstract void EvaluateJS(string method);
    public abstract void CloseBrowser();

    public abstract void ClearCache();
}
