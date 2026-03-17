using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoticonRow : MonoBehaviour
{
    [SerializeField]
    EmoticonRowItem sample;

    List<EmoticonRowItem> rowEmoticons = new List<EmoticonRowItem>();    
    public EmotionPopup parent { get; private set; } = null;
    int MaxItemCount = 0;
    public void Init(EmotionPopup popup, int maxItemCount)
    {
        parent = popup;
        MaxItemCount = maxItemCount;
        sample.gameObject.SetActive(false);
        rowEmoticons.Clear();
    }
    public void AddData(EmoticonItemData data, bool bEquip = false)
    {
        var obj = Instantiate(sample.gameObject, transform);
        obj.gameObject.SetActive(true);

        var item = obj.GetComponent<EmoticonRowItem>();
        item.SetData(data, this, bEquip);

        rowEmoticons.Add(item);
    }

    public bool IsFull()
    {
        return rowEmoticons.Count >= MaxItemCount;
    }

    public void OnEmotionSelect(EmoticonItemData data)
    {
        parent.OnEmotionSelect(data);
    }
}
