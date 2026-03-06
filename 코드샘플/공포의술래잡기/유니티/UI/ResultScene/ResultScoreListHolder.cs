using System.Collections.Generic;
using UnityEngine;

public class ResultScoreListHolder : MonoBehaviour
{
    [SerializeField] ResultScoreListItem item;

    List<ResultScoreListItem> childItem = new List<ResultScoreListItem>();

    private void Start()
    {
        
    }

    public void AddItem(string desc, int count, int unitScore)
    {
        var newItem = Instantiate(item);
        newItem.transform.SetParent(transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localScale = Vector3.one;
        newItem.gameObject.SetActive(true);

        newItem.Initialize(desc, count, unitScore);

        childItem.Add(newItem);
    }

    public void Clear()
    {
        item.gameObject.SetActive(false);

        if (childItem == null) return;

        foreach (ResultScoreListItem it in childItem)
        {
            Destroy(it.gameObject);
        }

        childItem.Clear();
    }
}
