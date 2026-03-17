using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanEmblemSubPopup : MonoBehaviour
{
    [SerializeField]
    GameObject emblemSample;

    [SerializeField]
    UIClanEmblem curEmblem;

    ClanCreateSubPopup createPopup = null;
    private void OnEnable()
    {
        foreach(Transform child in emblemSample.transform.parent)
        {
            if (child == emblemSample.transform)
                continue;

            Destroy(child.gameObject);
        }

        List<GameData> emblems = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_emblem);
        List<ClanEmblemData> myEmblems = new List<ClanEmblemData>();
        foreach (ClanEmblemData data in emblems)
        {
            switch (data.type)
            {
                case ClanEmblemData.EMBLEM_TYPE.CHECK_ITEM:
                    {
                        if (Managers.UserData.GetMyItemCount(data.param) > 0)
                        {
                            myEmblems.Add(data);
                        }
                    }
                    break;
                default:
                    myEmblems.Add(data);
                    break;
            }
        }

        foreach (ClanEmblemData data in myEmblems)
        {
            var obj = Instantiate(emblemSample.gameObject, emblemSample.transform.parent);
            obj.gameObject.SetActive(true);

            obj.GetComponent<UIClanEmblem>().Init(data);
            obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnSelectEmblem(data);
            });
        }

        emblemSample.SetActive(false);
    }

    public void OnCreateEmblem(int index, ClanCreateSubPopup parent)
    {
        SetCurEmblem(index);
        createPopup = parent;
    }
    public void SetCurEmblem(int index)
    {
        curEmblem.Init(index);
    }

    public void OnSelectEmblem(ClanEmblemData data)
    {
        curEmblem.Init(data);
    }


    public void OnConfirmButton()
    {
        createPopup.OnEmblemChanged(curEmblem.curData.GetID());

        PopupCanvas.Instance.ShowFadeText("엠블럼변경");
        gameObject.SetActive(false);
    }
}
