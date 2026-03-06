using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartFusionPanel : MonoBehaviour
{
    [SerializeField] GameObject MainPartListLayer;
    [SerializeField] GameObject MaterialPartListLayer;

    [SerializeField] GameObject FusionListLayer;
    [SerializeField] DragonPartInfoPanel PartInfoLayer;


    [SerializeField] GameObject GemClone;
    [SerializeField] ScrollRect MyGemsScroll;

    [SerializeField] ScrollRect MaterialGemsScroll;

    [SerializeField] PartFusionSlot[] FusionSlot;

    [SerializeField] Text MyGemAmount;
    [SerializeField] Text NeedGoldGemAmount;

    [SerializeField] Text NeedGoldAmount;
    [SerializeField] Button run;

    [SerializeField] GameObject sortDropdown;
    [SerializeField] Text curSortText;

    [SerializeField] GameObject FusionResult;
    [SerializeField] ScrollRect FusionResultScroll;
    [SerializeField] GameObject ResultColClone;
    public bool IsOpen { get { return gameObject.activeInHierarchy; } }
    public Dictionary<UserPart, List<UserPart>> Selected = new Dictionary<UserPart, List<UserPart>>();
    UserPart curMainPart = null;

    List<PartSlotFrame> partFrames = new List<PartSlotFrame>();
    List<PartSlotFrame> materialFrames = new List<PartSlotFrame>();
    int curSort = 0;
    public void Show()
    {
        gameObject.SetActive(true);

        Init();

        PopupManager.Instance.Top.SetInvenItemUI(110000006);
        PopupManager.Instance.Top.SetActiveChildObject(mainUIObjectType.invenItemCount, true, true);        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        DragonPartEvent.RefreshList();//일반 리스트 갱신 요청
        DragonPartEvent.HideInfoPanel();
        PopupManager.Instance.Top.SetActiveChildObject(mainUIObjectType.invenItemCount, false, false);
    }

    void Clear()
    {
        Selected.Clear();
        curMainPart = null;
        GemClone.SetActive(false);
        partFrames.Clear();
        SBFunc.RemoveAllChildrens(MyGemsScroll.content);
        
        foreach(var slot in FusionSlot)
        {
            slot.Clear();
        }

        sortDropdown.SetActive(false);
        FusionResult.SetActive(false);
    }
    public void Init()
    {
        Clear();

        GemClone.SetActive(true);
        var parts = new List<UserPart>();
        foreach(var part in User.Instance.PartData.GetAllUserParts())
        {
            if (part.Reinforce < GameConfigTable.GetConfigIntValue("PART_FUSION_MAIN_MIN_REINFORCE", 12))
                continue;

            if (part.LinkDragonTag > 0)
                continue;

            //if(User.Instance.Lock.IsLockPart(part.Tag))            
            //    continue;

            //융합한건 제외
            if (part.IsFusion)            
                continue;

            parts.Add(part);

            var clone = Instantiate(GemClone, MyGemsScroll.content);
            var partFrame = clone.GetComponent<PartSlotFrame>();
            partFrames.Add(partFrame);
            partFrame.SetPartSlotFrame(part.Tag, part.Reinforce);
            partFrame.SetCallback((param) => {
                int tag = int.Parse(param);
                if (partFrame.isClicked)
                {
                    partFrame.SetVisibleClickNode(false);

                    var p = User.Instance.PartData.GetPart(tag);
                    Selected.Remove(p);

                    if (curMainPart == p)
                        OnMaterialHide();
                    else
                        RefreshRight();
                }
                else
                {
                    if (Selected.Count > FusionSlot.Length)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("융합동시최대횟수초과"));
                        return;
                    }

                    foreach (var key in Selected.Keys)
                    {
                        if (key.Tag == tag)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("융합재료사용중"));
                            return;
                        }

                        foreach (var mat in Selected[key])
                        {
                            if (mat.Tag == tag)
                            {
                                ToastManager.On(StringData.GetStringByStrKey("융합재료사용중"));
                                return;
                            }
                        }
                    }

                    partFrame.SetVisibleClickNode(true);
                    Selected.Add(User.Instance.PartData.GetPart(tag), new List<UserPart>());
                    RefreshRight();

                    OnInfoShow(tag);
                }
            });
        }
        GemClone.SetActive(false);

        MyGemAmount.text = StringData.GetStringFormatByStrKey("융합가능젬수량", parts.Count);

        MainPartListLayer.SetActive(true);
        MaterialPartListLayer.SetActive(false);

        FusionListLayer.SetActive(true);
        PartInfoLayer.gameObject.SetActive(false);

        run.interactable = false;
        NeedGoldAmount.text = "0";
        NeedGoldGemAmount.text = "-";

        OnClickCustomSort(0);
    }

    void RefreshRight()
    {
        var keys = Selected.Keys.ToList();
        bool ready = keys.Count > 0;
        int costgold = 0;
        int costgoldgem = 0;
        for (int i = 0; i < FusionSlot.Length; i++)
        {
            if (keys.Count > i)
            {
                FusionSlot[i].SetData(keys[i], Selected[keys[i]].ToArray());

                if (!FusionSlot[i].Ready)
                    ready = false;

                if (FusionSlot[i].HasData)
                {
                    costgold += FusionSlot[i].NeedGold;
                    costgoldgem += FusionSlot[i].NeedGoldGem;
                }
            }
            else
            {
                FusionSlot[i].Clear();
            } 
        }

        if (costgold > User.Instance.GOLD)
            ready = false;
        if (costgoldgem > User.Instance.Inventory.GetItem(110000006).Amount)
            ready = false;

        run.interactable = ready;
        NeedGoldAmount.text = SBFunc.CommaFromNumber(costgold);
        NeedGoldGemAmount.text = SBFunc.CommaFromNumber(costgoldgem);
    }


    public void RemoveMainPart(int tag)
    {
        foreach(var part in partFrames)
        {
            if(part.PartTag == tag)
            {
                part.onClickFrame();
            }
        }
    }

    public void OnMaterialHide()
    {
        MainPartListLayer.SetActive(true);
        MaterialPartListLayer.SetActive(false);

        RefreshRight();
    }
    public void OnMaterialShow(int tag)
    {
        MainPartListLayer.SetActive(false);
        MaterialPartListLayer.SetActive(true);
        
        curMainPart = User.Instance.PartData.GetPart(tag);

        SBFunc.RemoveAllChildrens(MaterialGemsScroll.content);
        materialFrames.Clear();

        var parts = new List<UserPart>();
        GemClone.SetActive(true);

        parts.AddRange(Selected[curMainPart]);
        parts.AddRange(GetMaterialList());

        foreach (var part in parts)
        {
            var clone = Instantiate(GemClone, MaterialGemsScroll.content);
            var partFrame = clone.GetComponent<PartSlotFrame>();
            materialFrames.Add(partFrame);

            partFrame.SetPartSlotFrame(part.Tag, part.Reinforce);
            partFrame.SetCallback((param) => {
                int tag = int.Parse(param);
                var partData = User.Instance.PartData.GetPart(tag);
                if (partFrame.isClicked)
                {
                    partFrame.SetVisibleClickNode(false);
                    Selected[curMainPart].Remove(partData);
                    RefreshRight();

                    foreach(var part in partFrames)
                    {
                        if (part.PartTag == tag)
                        {
                            part.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (Selected[curMainPart].Count >= 6)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("융합재료최대치"));
                        return;
                    }

                    partFrame.SetVisibleClickNode(true);
                    Selected[curMainPart].Add(partData);
                    RefreshRight();

                    foreach (var part in partFrames)
                    {
                        if (part.PartTag == tag)
                        {
                            part.gameObject.SetActive(false);
                        }
                    }

                    OnInfoShow(tag);
                }
            });

            partFrame.SetVisibleClickNode(Selected[curMainPart].Contains(part));
        }
        GemClone.SetActive(false);

        OnClickCustomSort(curSort);
    }
    public void OnInfoShow(int tag)
    {
        PartInfoLayer.ShowDetailInfo(tag);
        FusionListLayer.SetActive(false);
    }
    public void OnInfoHide()
    {
        PartInfoLayer.gameObject.SetActive(false);
        FusionListLayer.SetActive(true);
    }

    List<UserPart> GetMaterialList()
    {
        List<UserPart> ret = new List<UserPart>();

        var selected = new List<int>();
        foreach (var key in Selected.Keys)
        {
            selected.Add(key.Tag);
            foreach (var mat in Selected[key])
            {
                selected.Add(mat.Tag);
            }
        }

        foreach (var part in User.Instance.PartData.GetAllUserParts())
        {
            if (part.Reinforce < GameConfigTable.GetConfigIntValue("PART_FUSION_SUB_MIN_REINFORCE", 9))
                continue;

            if (part.LinkDragonTag > 0)
                continue;

            if (User.Instance.Lock.IsLockPart(part.Tag))
                continue;

            if (selected.Contains(part.Tag))
                continue;

            ret.Add(part);
        }

        ret.Sort((a, b) => {
            switch (curSort)
            {
                case 0:
                    return a.Grade().CompareTo(b.Grade());
                case 1:
                    return b.Grade().CompareTo(a.Grade());
                case 2:
                    return a.Reinforce.CompareTo(b.Reinforce);
                case 3:
                    return b.Reinforce.CompareTo(a.Reinforce);
            }
            return 0;
        });
        return ret;
    }
    public void OnAutoFill()
    {
        Init();

        List<int> usedgem = new List<int>();
        int mainIndex = 0;
        List<UserPart> materials = GetMaterialList();
        for (int i = 0; i < FusionSlot.Length; i++)
        {
            if (partFrames.Count <= mainIndex)
                break;

            var frame = partFrames[mainIndex++];
            int maintag = frame.PartTag;
            if (usedgem.Contains(maintag))
                continue;

            usedgem.Add(maintag);

            var mat = new List<UserPart>();
            foreach(var m in materials)
            {
                if (usedgem.Contains(m.Tag))
                    continue;

                mat.Add(m);
                
                if (mat.Count == 6)
                    break;
            }

            if (mat.Count != 6)
                break;

            foreach (var m in mat)
                usedgem.Add(m.Tag);

            Selected.Add(User.Instance.PartData.GetPart(maintag), mat);
            frame.SetVisibleClickNode(true);
        }

        RefreshRight();
    }

    public void OnAutoFillMaterial()
    {
        if (Selected[curMainPart].Count >= 6)
        {
            ToastManager.On(StringData.GetStringByStrKey("융합재료최대치"));
        }

        foreach (var mat in materialFrames)
        {
            if (Selected[curMainPart].Count >= 6)
                break;

            if (mat.isClicked)
                continue;

            Selected[curMainPart].Add(User.Instance.PartData.GetPart(mat.PartTag));
            mat.SetVisibleClickNode(true);

            foreach (var part in partFrames)
            {
                if (part.PartTag == mat.PartTag)
                {
                    part.gameObject.SetActive(true);
                }
            }
        }
        RefreshRight();
    }
    public void OnSortButton()
    {
        sortDropdown.SetActive(!sortDropdown.activeSelf);
    }

    public void OnClickCustomSort(int checker)
    {
        string[] sortText = {
            "희귀도내림차",
            "희귀도오름차",
            "강화내림차",
            "강화오름차",
        };
        if (checker < 0 || sortText.Length <= checker)
            return;

        curSort = checker;

        SetList();
        
        curSortText.text = StringData.GetStringByStrKey(sortText[checker]);
        sortDropdown.SetActive(false);
    }

    void SetList()
    {
        List<int> mainParts = new List<int>();
        List<UserPart> parts = new List<UserPart>();
        foreach (var part in partFrames)
        {
            parts.Add(User.Instance.PartData.GetPart(part.PartTag));
        }

        partFrames.Sort((a, b) => { 
            switch(curSort)
            {
                case 0:
                    return User.Instance.PartData.GetPart(b.PartTag).Grade().CompareTo(User.Instance.PartData.GetPart(a.PartTag).Grade());
                case 1:
                    return User.Instance.PartData.GetPart(a.PartTag).Grade().CompareTo(User.Instance.PartData.GetPart(b.PartTag).Grade());
                case 2:
                    return User.Instance.PartData.GetPart(b.PartTag).Reinforce.CompareTo(User.Instance.PartData.GetPart(a.PartTag).Reinforce);
                case 3:
                    return User.Instance.PartData.GetPart(a.PartTag).Reinforce.CompareTo(User.Instance.PartData.GetPart(b.PartTag).Reinforce);
            }
            return 0;
        });

        foreach (var part in partFrames)
        {
            part.transform.SetAsLastSibling();
        }
    }


    public void OnFusion()
    {
        var baselist = Selected.Keys.ToList();
        List<int> base_tags = new List<int>();
        List<int> material_tags = new List<int>();
        foreach (var selected in baselist)
        {
            if (Selected[selected].Count != 6)
            {
                ToastManager.On(StringData.GetStringByStrKey("융합재료갯수확인"));
                return;
            }

            base_tags.Add(selected.Tag);
            foreach (var s in Selected[selected])
            {
                material_tags.Add(s.Tag);
            }
        }

        WWWForm data = new WWWForm();

        data.AddField("base_tags", SBFunc.ListToString(base_tags));        
        data.AddField("material_tags", SBFunc.ListToString(material_tags));

        NetworkManager.Send("part/fusion", data, (jsonData) =>   // 성공 보상은 jsonData["rewards"]["success"] 로, 실패 보상은 jsonData["rewards"]["fail"]로 옴
        {
            foreach (Transform child in FusionResultScroll.content)
            {
                Destroy(child.gameObject);
            }

            ResultColClone.SetActive(true);
            GameObject col = null;
            int index = 0;
            foreach (var tag in base_tags)
            {
                var part = User.Instance.PartData.GetPart(tag);
                if (part == null)
                    continue;

                if (index % 2 == 0)
                {
                    col = Instantiate(ResultColClone, FusionResultScroll.content);
                    foreach (Transform child in col.transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                var target = col.transform.GetChild(index % 2);
                target.gameObject.SetActive(true);

                var partframe = target.GetComponentInChildren<PartSlotFrame>();
                partframe.SetPartSlotFrame(tag);

                var FusionDesc = target.Find("fusionDescNode").Find("fusionDesc").GetComponent<Text>();
                var data = PartFusionData.Get(part.FusionStatKey);
                if (data != null)
                {
                    FusionDesc.text = StringData.GetStringFormatByStrKey(data._DESC, part.FusionStatValue);
                }

                index++;
            }
            ResultColClone.SetActive(false);


            Init();
            FusionResult.SetActive(true);
        });
    }
}
