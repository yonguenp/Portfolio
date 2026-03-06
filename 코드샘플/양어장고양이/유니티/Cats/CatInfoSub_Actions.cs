using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatInfoSub_Actions : MonoBehaviour
{
    public GameObject Sample;
    public GameObject Container;
    public Button OpenButton;
    public Text OpenGold;
    public CatInfoUI CatInfoUI;
    private cat_def curCatData = null;

   
    public void ClearActionList()
    {
        foreach (Transform child in Container.transform)
        {
            if(child.gameObject != Sample)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void InitActionList(cat_def cat, CatInfoUI catInfo)
    {
        CatInfoUI = catInfo;
        curCatData = cat;
        Sample.SetActive(false);

        Refresh();
    }

    public void Refresh()
    {
        ClearActionList();
        RefreshLevelUpUI();

        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;
                
        bool readyFocusTarget = true;
        for (uint i = 1; i < 10; i++)
        {
            cat_level_def levelInfo = curCatData.GetLevelInfo(i);
            if (levelInfo == null)
                break;

            cat_action_def action = levelInfo.GetCatNewAction();
            if (action == null)
                continue;

            GameObject listItem = Instantiate(Sample);
            listItem.transform.SetParent(Container.transform);

            RectTransform rt = listItem.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            CatActionItem item = listItem.GetComponent<CatActionItem>();

            if (item.SetActionItem(action, this, readyFocusTarget) == CatActionItem.CAT_ACTION_SATE.READY)
                readyFocusTarget = false;
        }
    }

    public void RefreshLevelUpUI()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        uint catLv = curCatData.GetUserCatInfo().GetCatLevel();
        object obj;
        if (user.data.TryGetValue("level", out obj))
        {
            if((uint)obj < catLv)
            {
                OpenButton.interactable = false;
                OpenGold.text = "-";
                return;
            }
        }
            
        cat_level_def catLevel = curCatData.GetLevelInfo(catLv);
        uint needGold = catLevel.GetCatLevelNeedGold();
        if (needGold == 0)
        {
            OpenButton.interactable = false;
            OpenGold.text = "-";
            return;
        }

        OpenButton.interactable = true;
        OpenGold.text = needGold.ToString();
    }

    public void OnTryCatLevelUP()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "cat");
        data.AddField("op", 4);
        data.AddField("cat", curCatData.GetCatID().ToString());

        NetworkManager.GetInstance().SendApiRequest("cat", 4, data, (response) =>
        {
            if (!CatInfoUI.FarmCanvas.CheckErrorResponse(response))
                return;

            CatInfoUI.CatInfoStatus.Invoke("Refresh", 0.1f);
            CatInfoUI.FarmCanvas.FarmUIPanel.Invoke("Refresh", 0.1f);
            Invoke("Refresh", 0.1f);            
        });
    }

    public void OnTryCatActionOpen()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "cat");
        data.AddField("op", 3);
        data.AddField("cat", curCatData.GetCatID().ToString());

        NetworkManager.GetInstance().SendApiRequest("cat", 3, data, (response) =>
        {
            if (!CatInfoUI.FarmCanvas.CheckErrorResponse(response))
                return;

            CatInfoUI.CatInfoStatus.Invoke("Refresh", 0.1f);
            CatInfoUI.FarmCanvas.FarmUIPanel.Invoke("Refresh", 0.1f);
            Invoke("Refresh", 0.1f);            
        });
    }


}
