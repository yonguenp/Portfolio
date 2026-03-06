using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToyList_item : MonoBehaviour
{
    uint item_no;
    public Image item_img;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetToyListUI(user_items data)
    {
        object obj;
        if(data.data.TryGetValue("item_id", out obj))
        {
            item_no = (uint)obj;
        }

        List<game_data> item_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
        foreach (game_data item in item_list)
        {
            if (item.data.TryGetValue("item_id", out obj))
            {
                if(item_no == (uint)obj)
                {
                    if (item.data.TryGetValue("icon_img", out obj))
                    {
                        item_img.sprite = Resources.Load<Sprite>((string)obj);
                        if (item_img.sprite == null)
                        {
                            item_img.sprite = Resources.Load<Sprite>("Sprites/Items/toy_default");
                        }
                    }
                }
            }
        }
    }
}
