using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScrollUIControllerItem : MonoBehaviour
{
    public delegate void ScrollItemSelectCallback(ScrollUIControllerItem item);

    public virtual void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public virtual void SetData(GameData data, ScrollItemSelectCallback cb = null)
    {
        if (cb != null)
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (cb != null)
                        cb.Invoke(this);
                });
            }
        }
    }
}
