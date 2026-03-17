using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerList : MonoBehaviour
{
    [SerializeField] ScriptTool parent = null;
    [SerializeField] TriggerItem sample = null;
    [SerializeField] ScrollRect scroll = null;
    public void OnEnable()
    {
        foreach(Transform item in scroll.content.transform)
        {
            if (item == sample.transform)
                continue;

            Destroy(item.gameObject);
        }

        sample.gameObject.SetActive(true);
        foreach (var data in TableManager.GetTable<ScriptTriggerTable>().GetAllList())
        {
            var trigger = Instantiate(sample, scroll.content).GetComponent<TriggerItem>();
            trigger.SetData(data, this);
        }
        sample.gameObject.SetActive(false);
    }
    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public void OnSelectTrigger(ScriptTriggerData data)
    {
        parent.SetState(data);
    }
}
