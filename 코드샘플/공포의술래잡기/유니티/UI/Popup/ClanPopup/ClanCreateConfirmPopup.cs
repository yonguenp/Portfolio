using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanCreateConfirmPopup : MonoBehaviour
{
    [SerializeField] UIClanEmblem clanEmblem;
    [SerializeField] Text clanName;

    public void SetData(string name, int emblem)
    {
        clanName.text = name;
        clanEmblem.Init(emblem);
    }
}
