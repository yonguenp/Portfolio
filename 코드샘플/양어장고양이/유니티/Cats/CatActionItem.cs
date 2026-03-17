using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatActionItem : MonoBehaviour
{
    private cat_action_def curCatActionData = null;

    public enum CAT_ACTION_SATE { 
        NORMAL,
        READY,
        LOCK,
    };

    public Text ActionName;
    public GameObject[] Background;
    public GameObject[] LockIcon;
    public Text[] GoldCondition;
    public Text[] LevelCondition;

    private CatInfoSub_Actions targetPanel = null;
    private CAT_ACTION_SATE curState = CAT_ACTION_SATE.LOCK;
    public CAT_ACTION_SATE SetActionItem(cat_action_def action, CatInfoSub_Actions CatInfoAction, bool ready)
    {
        curCatActionData = action;
        targetPanel = CatInfoAction;
        curState = CAT_ACTION_SATE.LOCK;

        if(curCatActionData.IsEnabled())
        {
            curState = CAT_ACTION_SATE.NORMAL;
        }
        else if(ready)
        {
            ready = false;
            cat_def cat = action.GetTargetCat();
            uint level = cat.GetUserCatInfo().GetCatLevel();
            
            for(uint i = 1; i <= level; i++)
            {
                cat_level_def lv = cat.GetLevelInfo(i);
                if(lv.GetCatNewAction().GetCatActionID() == action.GetCatActionID())
                {
                    ready = true;
                }
            }

            if (ready)
            {
                curState = CAT_ACTION_SATE.READY;
                Button curButton = GetComponent<Button>();
                curButton.interactable = true;
                curButton.onClick.AddListener(() =>
                {
                    targetPanel.OnTryCatActionOpen();
                });
            }
        }

        ActionName.text = curCatActionData.GetActionName();
        SetActionUI();

        gameObject.SetActive(true);

        return curState;
    }

    public void SetActionUI()
    {
        int curIndex = (int)curState;
        for(int i = 0; i < Background.Length; i++)
        {
            if(Background[i])
                Background[i].SetActive(i == curIndex);
        }

        for (int i = 0; i < LockIcon.Length; i++)
        {
            if (LockIcon[i])
                LockIcon[i].SetActive(i == curIndex);
        }

        for (int i = 0; i < GoldCondition.Length; i++)
        {
            if (GoldCondition[i])
            {
                if (i == curIndex)
                {
                    GoldCondition[i].gameObject.SetActive(true);
                    string desc = "";
                    if (curState == CAT_ACTION_SATE.NORMAL)
                        desc = curCatActionData.GetActionHungerDesc();
                    else
                        desc = curCatActionData.GetNeedGold().ToString("n0") + " 필요";

                    GoldCondition[i].text = desc;
                }
                else
                {
                    GoldCondition[i].gameObject.SetActive(false);
                }
            }
        }

        for (int i = 0; i < LevelCondition.Length; i++)
        {
            if (LevelCondition[i])
            {
                if (i == curIndex)
                {
                    LevelCondition[i].gameObject.SetActive(true);
                    LevelCondition[i].text = curCatActionData.GetActionOpenDesc();
                }
                else
                {
                    LevelCondition[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public CAT_ACTION_SATE CurState()
    {
        return curState;
    }

    public cat_action_def GetData()
    {
        return curCatActionData;
    }
}
