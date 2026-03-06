using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    public virtual void RefreshUI()
    {

    }

    public virtual void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
