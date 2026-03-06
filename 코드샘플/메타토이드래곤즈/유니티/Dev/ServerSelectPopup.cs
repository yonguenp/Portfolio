using DG.Tweening;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelectPopup : MonoBehaviour
{
    [SerializeField] private Text ServerDesc = null;
    [SerializeField] private Transform ServerObjParent = null;
    [SerializeField] private GameObject ServerButtonClone = null;
    [SerializeField] private ScrollRect DescScroll = null;

    [SerializeField] private GameObject nickPanel = null;
    [SerializeField] private Text nick = null;

    int serverTag = 0;
    Dictionary<int, ServerItem> ServerButtons = new Dictionary<int, ServerItem>();
    Dictionary<int, string> ServerState = null;
    Action SelectCallback = null;
    Action CreateCallback = null;
    public void SetActive(bool enable, Dictionary<int, string> state = null, Action select = null, Action create = null)
    {
        if (enable)
        {
            ServerState = state;

            SelectCallback = select;
            CreateCallback = create;
        }

        gameObject.SetActive(enable);
    }

    public void OnEnable()
    {
        Init();
        RefreshUI();
    }

    void Init()
    {
        ServerButtons.Clear();
        foreach (Transform child in ServerObjParent)
        {
            if (child == ServerButtonClone.transform)
                continue;

            Destroy(child.gameObject);
        }

        ServerButtonClone.SetActive(true);

        var infos = NetworkManager.Instance.ServerInfo;
        
        foreach (ServerInfo info in infos.Values)
        {
            var clone = Instantiate(ServerButtonClone, ServerObjParent);
            var comp = clone.GetComponent<ServerItem>();
            comp.SetData(info, this);
            ServerButtons.Add(info.TAG, comp);
        }

        ServerButtonClone.SetActive(false);

        serverTag = SBGameManager.CurServerTag;        
    }

    public void OnServerSelect(int tag)
    {
        serverTag = tag;

        RefreshUI();
    }

    void RefreshUI()
    {
        if (ServerButtons.Count <= 0)
            return;

        if(!ServerButtons.ContainsKey(serverTag))
        {
            foreach(var btn in ServerButtons)
            {
                serverTag = btn.Key;
                break;
            }
        }

        ServerDesc.text = ServerButtons[serverTag].data.DESC;
        DescScroll.DOVerticalNormalizedPos(1.0f, 0.3f);
        foreach (var btn in ServerButtons.Values)
        {
            btn.SetNormal();
        }

        ServerButtons[serverTag].SetSelected();

        nickPanel.SetActive(ServerState != null && ServerState.ContainsKey(serverTag));
        nick.text = ServerState != null && ServerState.ContainsKey(serverTag) ? ServerState[serverTag] : "";
    }

    public void OnServerChange()
    {
        SBGameManager.CurServerTag = serverTag;
        SetActive(false);

        if (ServerState != null && ServerState.ContainsKey(serverTag))
            SelectCallback.Invoke();
        else
            CreateCallback.Invoke();
    }

}
