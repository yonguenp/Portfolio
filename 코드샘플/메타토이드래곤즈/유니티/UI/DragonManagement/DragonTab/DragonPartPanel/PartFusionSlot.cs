using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartFusionSlot : MonoBehaviour
{
    [SerializeField] PartSlotFrame MainPart;
    [SerializeField] PartSlotFrame[] MaterialParts;
    [SerializeField] Image GoldGem;
    [SerializeField] Text GoldGemCount;
    [SerializeField] PartSlotFrame ResultPart;
    [SerializeField] PartFusionPanel parent;
    
    [SerializeField] Sprite NormalGoldGem;
    [SerializeField] Sprite GrayGoldGem;

    UserPart MainPartData = null;
    UserPart[] MaterialPartDatas = null;

    public bool HasData { get; private set; } = false;
    public bool Ready { get; private set; } = false;

    public int NeedGold { get; private set; } = -1;
    public int NeedGoldGem { get; private set; } = -1;

    public void Clear()
    {
        SetData(null, null);
        NeedGoldGem = 0;
        NeedGold = 0;
    }
    public void SetData(UserPart part, UserPart[] materials)
    {
        MainPartData = part;
        MaterialPartDatas = materials;

        Refresh();
    }
    public void Refresh()
    {
        HasData = false;
        Ready = true;
        if (MainPartData == null)
        {
            MainPart.gameObject.SetActive(false);
            Ready = false;
        }
        else
        {
            MainPart.gameObject.SetActive(true);
            MainPart.SetPartSlotFrame(MainPartData.Tag, MainPartData.Reinforce);
            MainPart.SetCallback((tag) => {
                parent.RemoveMainPart(MainPartData.Tag);
            });

            HasData = true;
        }

        for(int i = 0; i < MaterialParts.Length; i++)
        {
            if (MaterialPartDatas != null && i < MaterialPartDatas.Length && MaterialPartDatas[i] != null)
            {
                MaterialParts[i].gameObject.SetActive(true);
                MaterialParts[i].SetPartSlotFrame(MaterialPartDatas[i].Tag, MaterialPartDatas[i].Reinforce);
                HasData = true;
            }
            else
            {
                MaterialParts[i].gameObject.SetActive(false);
                Ready = false;
            }
        }

        if (Ready)
        {
            GoldGem.sprite = NormalGoldGem;            
            NeedGoldGem = Mathf.Max(0, MainPartData.Reinforce - GameConfigTable.GetConfigIntValue("PART_FUSION_GOLDGEM_NEED", 10));
            GoldGemCount.text = NeedGoldGem.ToString();
            NeedGold = GameConfigTable.GetConfigIntValue("PART_FUSION_GOLD_NEED", 50000);
            ResultPart.gameObject.SetActive(true);
            ResultPart.SetPartSlotFrame(MainPartData.Tag, MainPartData.Reinforce);
        }
        else
        {
            GoldGem.sprite = GrayGoldGem;
            GoldGemCount.text = "";
            ResultPart.gameObject.SetActive(false);
        }
    }

    public void OnMaterialSelect()
    {
        if(MainPartData == null)
        {
            ToastManager.On("융합잼선택필요");
            return;
        }
        parent.OnMaterialShow(MainPartData.Tag);
    }

}
