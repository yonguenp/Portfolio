using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerItem : MonoBehaviour
{
    [SerializeField]
    Image image;
    [SerializeField]
    Text text;
    [SerializeField]
    Sprite Selected;
    [SerializeField]
    Sprite Normal;

    [SerializeField]
    Color SelectedColor;
    [SerializeField]
    Color NormalColor;
    [SerializeField]
    GameObject NewIcon;
    [SerializeField]
    GameObject EventIcon;

    public ServerInfo data { get; private set; } = null;
    ServerSelectPopup parent = null;
    public void SetData(ServerInfo item, ServerSelectPopup p)
    {
        data = item;
        parent = p;

        text.text = item.NAME;

        NewIcon.SetActive(item.NewFlag);
        EventIcon.SetActive(item.EventFlag);
    }

    public void OnClick()
    {
        parent.OnServerSelect(data.TAG);
    }

    public void SetNormal()
    {
        text.color = NormalColor;
        image.sprite = Normal;
    }
    public void SetSelected()
    {
        text.color = SelectedColor;
        image.sprite = Selected;
    }
}
