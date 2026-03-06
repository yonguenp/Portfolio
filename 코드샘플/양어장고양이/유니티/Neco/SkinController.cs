using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinController : MonoBehaviour
{
    [Serializable]
    public class SkinData
    {
        public uint TargetItemID;
        public SkeletonGraphic spine;
        public string spineSkinName;

        public Image image;
        public Sprite sprite;
    }

    public int DefaultIndex = 0;
    public bool NeedNativeSize = true;
    public bool IsDoDo = false;
    public SkinData[] skinDatas;
    
    public void Refresh()
    {
        if (skinDatas.Length <= 0)
            return;

        int targetIndex = DefaultIndex;
        for(int i = 0; i < skinDatas.Length; i++)
        {
            if (i == DefaultIndex)
                continue;

            SkinData data = skinDatas[i];

            bool isEquiped = PlayerPrefs.GetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + data.TargetItemID, 0) > 0;
            if (isEquiped)
            {
                if (user_items.GetUserItemAmount(data.TargetItemID) <= 0)
                {
                    PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + data.TargetItemID, 0);
                }
                else
                {
                    targetIndex = i;
                }
            }
        }

        SkinData target = skinDatas[targetIndex];
        if (target.spine != null)
        {
            if(target.TargetItemID == 154 && PlayerPrefs.GetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + 162, 0) > 0 && IsDoDo)
            {
                target.spine.Skeleton.SetSkin("christmas_colorful");
                target.spine.Skeleton.SetSlotsToSetupPose();
            }
            else
            {
                target.spine.Skeleton.SetSkin(target.spineSkinName);
                target.spine.Skeleton.SetSlotsToSetupPose();
            }            
        }

        if (target.image != null)
        {
            target.image.sprite = target.sprite;
            if(NeedNativeSize)
                target.image.SetNativeSize();
        }
    }

    private void OnEnable()
    {
        Refresh();
    }
}
