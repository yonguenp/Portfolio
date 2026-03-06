using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptTool : MonoBehaviour
{
    enum ToolState { 
        LIST,
        SCRIPT_VIEW
    }

    [SerializeField] TriggerList list = null;
    [SerializeField] ScriptController scriptView = null;

    ToolState curState = ToolState.LIST;

    Game gameController = null;
    private void Start()
    {
        Clear();    
    }

    void Clear()
    {
        if(gameController == null)
        {
            StartCoroutine(DataLoad(() =>
            {
                SetState(null);
            }));
        }
        else
        {
            SetState(null);
        }        
    }
    IEnumerator DataLoad(System.Action cb)
    {
        var go = new GameObject();
        gameController = go.AddComponent<Game>();
        DontDestroyOnLoad(go);

        yield return SBGameManager.Instance.GameDataSyncAndLoad(null);

        cb?.Invoke();
    }

    public void SetState(ScriptTriggerData trigger)
    {
        list.SetActive(trigger == null);
        scriptView.SetData(trigger, Clear);
    }
}
