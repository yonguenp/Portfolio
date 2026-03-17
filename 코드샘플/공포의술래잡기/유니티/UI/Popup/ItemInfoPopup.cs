using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPopup : Popup
{
    [SerializeField]
    Text textComponent;
    [SerializeField]
    Text buttonTextComponent;

    [SerializeField]
    Vector2 PenalOffset = new Vector2(100, 200);

    [SerializeField]
    UIBundleItem cloneTarget;

    [SerializeField]
    ScrollRect scrollView;
    public void Init(string title, List<BundleInfo> infos)
    {
        Clear();
        textComponent.text = title;
        foreach(BundleInfo info in infos)
        {
            AddMultipleReward(info);
        }
    }

    private void Clear()
    {
        cloneTarget.gameObject.SetActive(false);

        foreach (Transform child in cloneTarget.transform.parent)
        {
            if (child == cloneTarget.transform)
                continue;

            Destroy(child.gameObject);
        }
    }
    private void AddMultipleReward(BundleInfo info)
    {
        cloneTarget.gameObject.SetActive(true);

        GameObject multiItemRow = Instantiate(cloneTarget.gameObject);
        multiItemRow.transform.SetParent(cloneTarget.transform.parent);
        multiItemRow.transform.localPosition = Vector3.zero;
        multiItemRow.transform.localScale = Vector3.one;

        UIBundleItem item = multiItemRow.GetComponent<UIBundleItem>();
        item.SetReward(info);

        cloneTarget.gameObject.SetActive(false);
        scrollView.horizontalNormalizedPosition = 0.0f;

        CancelInvoke("ResetHorizontalPos");
        Invoke("ResetHorizontalPos", 0.1f);
    }

    void ResetHorizontalPos()
    {
        CancelInvoke("ResetHorizontalPos");
        scrollView.horizontalNormalizedPosition = 0.0f;
    }
}
