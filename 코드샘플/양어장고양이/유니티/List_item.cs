using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class List_item : MonoBehaviour
{
    public Image IconImage;
    public Image HideImage;

    public GameObject NeedItemObject;
    public Image NeedItemImage;
    public Text NeedItemText;

    public Button Button;
    public GameObject HeartIcon;
    public GameObject toyPanel;
    public Image toyImage;

    private GameCanvas GameCanvas = null;
    private game_data curData;
    // Start is called before the first frame update
    public bool SetTouchListUI(inter_touch data, TouchListUI parent)
    {
        uint touch_id = 0;
        GameCanvas = parent.GetGameCanvas();
        curData = data;

        object obj;

        if (data.data.TryGetValue("touch_id", out obj))
        {
            touch_id = (uint)obj;
        }

        if(data.data.TryGetValue("icon_img", out obj))
        {
            IconImage.sprite = Resources.Load<Sprite>((string)obj);
        }

        HeartIcon.SetActive(false);
        Button.enabled = false;

        List<game_data> userData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_INTER_TOUCH);
        if (userData_list != null)
        {
            foreach (game_data ud in userData_list)
            {
                if (ud.data.TryGetValue("touch_id", out obj))
                {
                    if(touch_id == (uint)obj)
                    {
                        SetUserTouchListUI((user_inter_touch)ud);
                        return true;
                    }
                }
            }
        }

        IconImage.gameObject.SetActive(false);
        HideImage.gameObject.SetActive(true);

        return false;
    }

    public void SetUserTouchListUI(user_inter_touch data)
    {
        bool isEnable = false;
        object obj;
        if(data.data.TryGetValue("today_run_count", out obj))
        {
            isEnable = (uint)obj <= 0;
        }
        Button.enabled = true;
        Button.onClick.AddListener(() => {
            GameCanvas.OnTouchItemSelect((inter_touch)curData);
        });

        HeartIcon.SetActive(isEnable);
    }

    public bool SetPlayListUI(inter_play data, PlayListUI parent)
    {
        GameCanvas = parent.GetGameCanvas();
        curData = data;

        toyPanel.SetActive(true);
        uint play_id = 0;

        object obj;

        if (data.data.TryGetValue("play_id", out obj))
        {
            play_id = (uint)obj;
        }

        if (data.data.TryGetValue("icon_img", out obj))
        {
            IconImage.sprite = Resources.Load<Sprite>((string)obj);
        }

        bool hasClip = false;
        bool hasItem = false;

        List<game_data> userData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_INTER_PLAY);
        if (userData_list != null)
        {
            foreach (game_data ud in userData_list)
            {
                if (ud.data.TryGetValue("play_id", out obj))
                {
                    if (play_id == (uint)obj)
                    {
                        hasClip = true;
                    }
                }
            }
        }

        if (data.data.TryGetValue("item_id", out obj))
        {
            uint item_id = (uint)obj;

            items item = items.GetItem(item_id);
            NeedItemText.text = item.GetItemName();
            toyImage.sprite = item.GetItemIcon();
            NeedItemImage.sprite = item.GetItemIcon();

            hasItem = user_items.GetUserItemAmount(item_id) > 0;            
        }

        Button.enabled = hasClip && hasItem;

        if (hasClip && hasItem)
        {
            Button.onClick.AddListener(() => {
                GameCanvas.OnPlayItemSelect((inter_play)curData);
            });
        }

        if (hasClip == false)
        {
            IconImage.gameObject.SetActive(false);
            HideImage.gameObject.SetActive(true);
        }

        if (hasItem == false)
        {
            toyPanel.SetActive(false);
            NeedItemObject.SetActive(true);
        }
        

        return hasClip && hasItem;
    }
}
