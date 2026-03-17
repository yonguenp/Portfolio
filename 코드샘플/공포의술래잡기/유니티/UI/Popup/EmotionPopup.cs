using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotionPopup : InventoryPanel
{
    [Serializable]
    public class EmotionSlot
    {
        public int SlotNo;
        public Image EmotionIcon;
        public GameObject PlusIcon;
        public Text EmotionText;
    }

    [SerializeField]
    EmotionSlot[] EmotionSlots;

    [SerializeField]
    RectTransform container;
    [SerializeField]
    EmoticonRow mine;
    [SerializeField]
    Transform line;
    [SerializeField]
    EmoticonRow notmine;

    private int SelectedSlotNo = 0;
    public int SelectedItemNo { get; private set; } = 0;


    EmoticonItemData GetEmotionItem(int slotNo)
    {
        int itemNo = 0;
        switch (slotNo)
        {
            case 1:
                itemNo = CacheUserData.GetInt("Emotion1", 100);
                break;
            case 2:
                itemNo = CacheUserData.GetInt("Emotion2", 101);
                break;
            case 3:
                itemNo = CacheUserData.GetInt("Emotion3", 102);
                break;
            case 4:
                itemNo = CacheUserData.GetInt("Emotion4", 103);
                break;
            default:
                return null;
        }

        EmoticonItemData ret = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.emoticon, itemNo) as EmoticonItemData;
        if(ret == null)
        {
            switch (slotNo)
            {
                case 1:
                    itemNo = CacheUserData.GetInt("Emotion1", 100);
                    break;
                case 2:
                    itemNo = CacheUserData.GetInt("Emotion2", 101);
                    break;
                case 3:
                    itemNo = CacheUserData.GetInt("Emotion3", 102);
                    break;
                case 4:
                    itemNo = CacheUserData.GetInt("Emotion4", 103);
                    break;
                default:
                    return null;
            }

            ret = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.emoticon, itemNo) as EmoticonItemData;
        }

        return ret;
    }

    public void OnSlotSelected(int slotNo)
    {
        if(SelectedSlotNo > 0)
        {
            int tmp1 = 0;
            switch (slotNo)
            {
                case 1:
                    tmp1 = CacheUserData.GetInt("Emotion1", 100);
                    break;
                case 2:
                    tmp1 = CacheUserData.GetInt("Emotion2", 101);
                    break;
                case 3:
                    tmp1 = CacheUserData.GetInt("Emotion3", 102);
                    break;
                case 4:
                    tmp1 = CacheUserData.GetInt("Emotion4", 103);
                    break;
                default:
                    tmp1 = 100;
                    break;
            }

            int tmp2 = 0;
            switch (SelectedSlotNo)
            {
                case 1:
                    tmp2 = CacheUserData.GetInt("Emotion1", 100);
                    break;
                case 2:
                    tmp2 = CacheUserData.GetInt("Emotion2", 101);
                    break;
                case 3:
                    tmp2 = CacheUserData.GetInt("Emotion3", 102);
                    break;
                case 4:
                    tmp2 = CacheUserData.GetInt("Emotion4", 103);
                    break;
                default:
                    tmp2 = 101;
                    break;
            }

            CacheUserData.SetInt("Emotion" + slotNo.ToString(), tmp2);
            CacheUserData.SetInt("Emotion" + SelectedSlotNo.ToString(), tmp1);

            RefreshUI();
            return;
        }

        SelectedSlotNo = slotNo;

        if (SelectedItemNo > 0)
        {
            OnEmotionSelect(SelectedItemNo);
            return;
        }


        for (int i = 0; i < 4; i++)
        {
            EmotionSlots[i].PlusIcon.SetActive(false);
        }

        EmotionSlots[SelectedSlotNo - 1].PlusIcon.SetActive(true);
    }

    public void OnEmotionSelect(EmoticonItemData data)
    {
        if (SelectedSlotNo <= 0 || SelectedSlotNo > 4)
        {
            if (SelectedItemNo == data.GetID())
                SelectedItemNo = 0;
            else
                SelectedItemNo = data.GetID();
            RefreshUI();
            return;
        }

        OnEmotionSelect(data.GetID());
    }

    void OnEmotionSelect(int itemNo)
    {
        int slotNo = SelectedSlotNo;
        SelectedItemNo = 0;
        CacheUserData.SetInt("Emotion" + slotNo.ToString(), itemNo);
        PopupCanvas.Instance.ShowFadeText("msg_emoticon_save");

        RefreshUI();

        int index = slotNo - 1;
        if (index < 0 || index >= EmotionSlots.Length)
            return;

        Image icon = EmotionSlots[index].EmotionIcon;

        Sequence sequence = DOTween.Sequence();

        //sequence.Append(this.gameObject.transform.DOScale(Vector3.zero, 0));
        sequence.Append(icon.gameObject.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.01f).SetEase(Ease.OutQuad));
        sequence.Append(icon.gameObject.transform.DOScale(Vector3.one, 0.005f));

        int angle = 30;
        for (int i = 0; i < 3; i++)
        {
            sequence.Append(icon.gameObject.transform.DORotate(new Vector3(0, 0, angle), 0.1f)).SetEase(Ease.OutQuad);
            sequence.Append(icon.gameObject.transform.DORotate(new Vector3(0, 0, 0), 0.1f)).SetEase(Ease.InQuad);
            sequence.Append(icon.gameObject.transform.DORotate(new Vector3(0, 0, -1 * angle), 0.1f)).SetEase(Ease.OutQuad);
            sequence.Append(icon.gameObject.transform.DORotate(new Vector3(0, 0, 0), 0.1f)).SetEase(Ease.InQuad);

            angle -= 10;
        }
    }

    public override void RefreshUI()
    {
        if(Managers.UserData.GetMyItemCount(100) <= 0)
        {
            Managers.UserData.GetAllMyItemData()[ItemGameData.GetItemData(100)] = 1;
        }
        if (Managers.UserData.GetMyItemCount(101) <= 0)
        {
            Managers.UserData.GetAllMyItemData()[ItemGameData.GetItemData(101)] = 1;
        }
        if (Managers.UserData.GetMyItemCount(102) <= 0)
        {
            Managers.UserData.GetAllMyItemData()[ItemGameData.GetItemData(102)] = 1;
        }
        if (Managers.UserData.GetMyItemCount(103) <= 0)
        {
            Managers.UserData.GetAllMyItemData()[ItemGameData.GetItemData(103)] = 1;
        }

        SelectedSlotNo = 0;

        List<GameData> emoticons = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.emoticon);
        List<EmoticonItemData> mines = new List<EmoticonItemData>();
        List<EmoticonItemData> notmines = new List<EmoticonItemData>();
        List<EmoticonItemData> uses = new List<EmoticonItemData>();

        foreach (EmoticonItemData emo in emoticons)
        {
            if(Managers.UserData.GetMyItemCount(emo.GetID()) > 0)
            {
                mines.Add(emo);
            }
            else
            {
                notmines.Add(emo);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            EmoticonItemData emoticon = GetEmotionItem(i + 1);
            EmotionSlots[i].EmotionIcon.sprite = emoticon.sprite;

            EmotionSlots[i].PlusIcon.SetActive(SelectedItemNo > 0);
            uses.Add(emoticon);
        }

        foreach (Transform child in container)
        {
            if (child == mine.transform)
                continue;
            if (child == notmine.transform)
                continue;
            if (child == line)
                continue;

            Destroy(child.gameObject);
        }

        mine.gameObject.SetActive(false);
        notmine.gameObject.SetActive(false);
        line.gameObject.SetActive(true);

        int sibling = 0;

        EmoticonRow curRow = null;        
        foreach (var m in mines)
        {
            if (curRow == null)
            {
                var obj = Instantiate(mine.gameObject, container);
                obj.transform.SetSiblingIndex(sibling++);
                obj.gameObject.SetActive(true);

                curRow = obj.GetComponent<EmoticonRow>();
                curRow.Init(this, 9);                
            }

            curRow.AddData(m, uses.Contains(m));

            if (curRow.IsFull())
                curRow = null;
        }

        line.transform.SetSiblingIndex(sibling++);
        curRow = null;

        foreach (var n in notmines)
        {
            if (curRow == null)
            {
                var obj = Instantiate(notmine.gameObject, container);
                obj.transform.SetSiblingIndex(sibling++);
                obj.gameObject.SetActive(true);

                curRow = obj.GetComponent<EmoticonRow>();
                curRow.Init(this, 9);
            }

            curRow.AddData(n);

            if (curRow.IsFull())
                curRow = null;
        }
    }
}
