using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSeqTester : MonoBehaviour
{
    public void OnTest()
    {
        for (uint i = 1; i <= 13; i++)
        {
            if (i == 4)
                continue;

            if (!neco_cat.GetNecoCat(i).IsGainCat())
                NecoCanvas.GetGameCanvas().CallCat(i, 0, 0, 0);
        }

        List<uint> itemList = new List<uint>();
        itemList.Add(106);
        itemList.Add(107);
        itemList.Add(108);
        itemList.Add(109);
        itemList.Add(110);
        itemList.Add(111);
        itemList.Add(112);
        itemList.Add(113);
        itemList.Add(114);
        itemList.Add(115);
        itemList.Add(116);
        itemList.Add(117);
        itemList.Add(118);
        itemList.Add(119);
        itemList.Add(120);
        itemList.Add(121);
        itemList.Add(122);
        itemList.Add(123);
        itemList.Add(124);
        itemList.Add(146);

        foreach (uint itemID in itemList)
        {
            WWWForm param = new WWWForm();
            param.AddField("api", "chore");
            param.AddField("op", 1);

            param.AddField("item", itemID.ToString());
            param.AddField("cnt", 1);

            NetworkManager.GetInstance().SendApiRequest("chore", 1, param, (response) =>
            {

            });
        }
    }

    public uint objectID = 0;
    public uint catID = 0;
    public uint slotID = 0;
    public neco_cat.CAT_SUDDEN_STATE state;
    public void OnCallcat()
    {
        NecoCanvas.GetGameCanvas().CallCat(catID, objectID, slotID, (uint)state);
    }

    [Range(0.0f, 1.0f)]
    public float colorValue = 1.0f;

    public void OnColorSet()
    {
        foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>(true))
        {   
            graphic.color = new Color(colorValue, colorValue, colorValue, graphic.color.a);
        }

        
    }
}
