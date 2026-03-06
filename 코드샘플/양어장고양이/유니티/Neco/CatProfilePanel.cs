using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatProfilePanel : MonoBehaviour
{
    public GameObject catListPanel;

    public void OnClickCatListButton()
    {
        if (catListPanel != null)
        {
            catListPanel.SetActive(true);
        }
    }
}
