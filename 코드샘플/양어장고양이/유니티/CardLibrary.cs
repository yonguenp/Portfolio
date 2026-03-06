using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLibrary : MonoBehaviour
{
    public GameObject cloneTarget;
    public GameObject LibraryListContainer;
    public CardLibraryDetail LibraryDetail;
    public Image gauge;
    public Text desc;
    private void OnEnable()
    {
        RefreshLibraryList();        
    }

    void RefreshLibraryList()
    {
        cloneTarget.SetActive(true);

        foreach(Transform listItem in LibraryListContainer.transform)
        {
            if(cloneTarget.transform != listItem)
            {
                Destroy(listItem.gameObject);
            }
        }

        object obj;
        int total = 0;
        int cur = 0;
        List<game_data> card_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
        List<game_data> album = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ALBUM);
        foreach (game_data data in card_list)
        {
            uint cardID = 0;
            if (data.data.TryGetValue("card_id", out obj))
            {
                cardID = (uint)obj;
            }

            int state = 0;
            foreach(game_data uc in album)
            {
                if(uc.data.TryGetValue("card_id", out obj))
                {
                    if(cardID == (uint)obj)
                    {
                        if (uc.data.TryGetValue("flag", out obj))
                        {
                            state = int.Parse(((uint)obj).ToString());
                            break;
                        }
                    }
                }
            }

            GameObject listItem = Instantiate(cloneTarget);
            listItem.transform.SetParent(LibraryListContainer.transform);
            RectTransform rt = listItem.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;

            CardLibraryItem component = listItem.GetComponent<CardLibraryItem>();
            if(component)
            {
                component.SetLibraryItem(state, (card_define)data, this);
            }
            total++;
            if ((state & 2) != 0)
                cur++;
        }

        cloneTarget.SetActive(false);
        float ratio = (float)cur / total;
        gauge.fillAmount = ratio;
        string strPercent = ((int)(ratio * 100.0f)).ToString();
        desc.text = LocalizeData.GetText("32") + " " + strPercent + " %";
    }

    public void OnCardLibraryDetail(card_define data)
    {
        LibraryDetail.ShowLibraryDetail(data);
    }
}
