using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPopup : Popup
{
    //[SerializeField]
    //CharacterListItem charSample;
    [SerializeField]
    UIBundleItem charSample;

    [SerializeField]
    ScrollRect scrollRect;


    //CharacterListItem selected = null;
    UIBundleItem selected = null;

    ItemGameData curItem = null;
    InventoryPopup parent = null;
    public void Init(ItemGameData itemData, InventoryPopup p)
    {
        Clear();

        parent = p;
        curItem = itemData;

        charSample.gameObject.SetActive(true);

        foreach(SelectBox data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.select_box))
        {
            if(data.group_id == itemData.value)
            {
                List<ShopPackageGameData> reward = ShopPackageGameData.GetRewardDataList(data.result);
                if (reward.Count == 0)
                    continue;
                if ((ASSET_TYPE)reward[0].goods_type == ASSET_TYPE.CHARACTER)
                {
                    CharacterGameData charData = CharacterGameData.GetCharacterData(reward[0].GetParam());
                    if (charData != null && charData.use)
                    {
                        GameObject item = Instantiate(charSample.gameObject);
                        item.transform.SetParent(charSample.transform.parent);
                        item.transform.localPosition = Vector3.zero;
                        item.transform.localScale = Vector3.one;

                        //CharacterListItem charListItem = item.GetComponent<CharacterListItem>();
                        //charListItem.name = data.result.ToString();
                        //charListItem.SetDataForChoiceTicket(charData, (item)=> {
                        //    CharacterListItem target = item as CharacterListItem;
                        //    OnSelectCharacter(target);
                        //});

                        UIBundleItem charListItem = item.GetComponent<UIBundleItem>();
                        charListItem.name = data.result.ToString();
                        charListItem.SetCharacterInfo(charData.GetID());
                        charListItem.SetTouchCallback(() => {
                            OnSelectCharacter(charListItem);
                        });
                    }
                }
                else if ((ASSET_TYPE)reward[0].goods_type == ASSET_TYPE.ITEM)
                {
                    ItemGameData id = ItemGameData.GetItemData(reward[0].GetParam());
                    if (id != null)
                    {
                        GameObject item = Instantiate(charSample.gameObject);
                        item.transform.SetParent(charSample.transform.parent);
                        item.transform.localPosition = Vector3.zero;
                        item.transform.localScale = Vector3.one;

                        //CharacterListItem charListItem = item.GetComponent<CharacterListItem>();
                        //charListItem.name = data.result.ToString();
                        //charListItem.SetDataForChoiceTicket(charData, (item)=> {
                        //    CharacterListItem target = item as CharacterListItem;
                        //    OnSelectCharacter(target);
                        //});

                        UIBundleItem charListItem = item.GetComponent<UIBundleItem>();
                        charListItem.name = data.result.ToString();
                        charListItem.SetItem(id, reward[0].goods_amount);
                        charListItem.SetTouchCallback(() => {
                            OnSelectCharacter(charListItem);
                        });
                    }
                }

            }
        }

        scrollRect.verticalNormalizedPosition = 0.0f;
        charSample.gameObject.SetActive(false);
    }

    void Clear()
    {
        selected = null;
        curItem = null;

        foreach (Transform child in charSample.transform.parent)
        {
            if (child == charSample.transform)
                continue;

            Destroy(child.gameObject);
        }

        charSample.gameObject.SetActive(false);
    }

    //public void OnSelectCharacter(CharacterListItem item)
    public void OnSelectCharacter(UIBundleItem item)
    {
        selected = item;
        foreach (Transform child in charSample.transform.parent)
        {
            if (child == charSample.transform)
                continue;

            UIBundleItem childItem = child.GetComponent<UIBundleItem>();
            childItem.SetChecker(childItem == item);
        }
    }

    public void OnConfirm()
    {
        if (selected == null)
            return;

        parent.OnChoiceCharacter(curItem.GetID(), selected.name);        
    }
}

public class SelectBox : GameData
{
    public int uid { get; private set; }
    public int group_id { get; private set; }
    public int result { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);
        group_id = Int(data["group_id"]);
        result = Int(data["result"]);
    }

}
