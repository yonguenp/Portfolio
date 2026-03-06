using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingConstructMarker : MonoBehaviour
{
    [SerializeField] GameObject SelectableUI;
    [SerializeField] GameObject DimmedUI;

    public void SetSelectable(bool selectable, bool switchable)
    {
        SelectableUI.SetActive(selectable);
        DimmedUI.SetActive(!selectable || switchable);
    }
}
