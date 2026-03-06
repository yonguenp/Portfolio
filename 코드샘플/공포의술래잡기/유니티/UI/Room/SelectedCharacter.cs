using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCharacter : MonoBehaviour
{
    //[Serializable]
    //public class CharacterInfo
    //{
    //    public string resourceName;
    //    public SkeletonDataAsset asset;
    //}

    //[SerializeField]
    //CharacterInfo[] CharacterList;

    [SerializeField]
    SkeletonGraphic target;

    [SerializeField]
    string animationName = "f_idle_0";
    [SerializeField]
    bool flipX = false;
    public int characterid;

    [SerializeField]
    RectTransform backGround;
    [SerializeField]
    RectTransform frontGround;

    public void ClearUI()
    {
        target.gameObject.SetActive(false);
        ClearEquip();
    }
    public void SetCharacter(int type, UserEquipData equip)
    {
        SetCharacter(type, equip == null ? null : equip.equipData);
    }

    public void SetCharacter(int type, EquipInfo equipInfo = null)
    {
        characterid = type;
        ClearUI();

        var info = CharacterGameData.GetCharacterData(type).spine_resource;
        target.skeletonDataAsset = info;
        SetAnimation(animationName);

        if (target.Skeleton == null)
            return;

        target.Skeleton.ScaleX = flipX ? -1.0f : 1.0f;
        target.gameObject.SetActive(true);

        RefreshEquip(equipInfo);
    }

    public void SetCharacter(int type, int itemNo)
    {
        EquipInfo info = itemNo > 0 ? EquipInfo.GetEquipData(itemNo) : null;

        SetCharacter(type, info);
    }

    public void ClearEquip()
    {
        foreach (Transform child in backGround.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in frontGround.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void RefreshEquip(UserEquipData userEquipData)
    {
        if(userEquipData == null)
        {
            ClearEquip();
            return;
        }

        RefreshEquip(userEquipData.equipData);
    }

    public void RefreshEquip(EquipInfo equipData)
    {
        if (equipData == null)
        {
            ClearEquip();
            return;
        }

        //front와 back에 오브젝트 붙여주기
        RectTransform transform = target.transform as RectTransform;

        backGround.localPosition = transform.localPosition;
        backGround.anchoredPosition = transform.anchoredPosition;
        backGround.anchorMin = transform.anchorMin;
        backGround.anchorMax = transform.anchorMax;
        backGround.pivot = transform.pivot;
        backGround.localRotation = transform.localRotation;

        frontGround.localPosition = transform.localPosition;
        frontGround.anchoredPosition = transform.anchoredPosition;
        frontGround.anchorMin = transform.anchorMin;
        frontGround.anchorMax = transform.anchorMax;
        frontGround.pivot = transform.pivot;
        frontGround.localRotation = transform.localRotation;
        
        ClearEquip();
        
        CancelInvoke("RefreshEffectParticles");
        bool bAttached = false;
        if (!string.IsNullOrEmpty(equipData.sp_effect_resource))
        {
            var auraObj = Managers.Resource.InstantiateFromBundle(equipData.sp_effect_resource);
            if (auraObj != null)
            {
                auraObj.transform.parent = backGround;
                auraObj.transform.localPosition = Vector3.zero;
                auraObj.transform.localScale = Vector3.one;
                auraObj.layer = backGround.gameObject.layer;

                backGround.GetComponent<Coffee.UIExtensions.UIParticle>().ignoreCanvasScaler = true;
                backGround.GetComponent<Coffee.UIExtensions.UIParticle>().RefreshParticles();

                bAttached = true;
            }
        }

        if (!string.IsNullOrEmpty(equipData.sp_effect_resource_front))
        {
            var auraObj = Managers.Resource.InstantiateFromBundle(equipData.sp_effect_resource_front);
            if (auraObj != null)
            {
                auraObj.transform.parent = frontGround;
                auraObj.transform.localPosition = Vector3.zero;
                auraObj.transform.localScale = Vector3.one;
                auraObj.layer = frontGround.gameObject.layer;

                frontGround.GetComponent<Coffee.UIExtensions.UIParticle>().ignoreCanvasScaler = true;
                frontGround.GetComponent<Coffee.UIExtensions.UIParticle>().RefreshParticles();
                
                bAttached = true;
            }
        }

        if(bAttached)
            Invoke("RefreshEffectParticles", 0.5f);
    }

    void RefreshEffectParticles()
    {
        CancelInvoke("RefreshEffectParticles");

        backGround.GetComponent<Coffee.UIExtensions.UIParticle>().ignoreCanvasScaler = false;
        frontGround.GetComponent<Coffee.UIExtensions.UIParticle>().ignoreCanvasScaler = false;

        backGround.GetComponent<Coffee.UIExtensions.UIParticle>().RefreshParticles();
        frontGround.GetComponent<Coffee.UIExtensions.UIParticle>().RefreshParticles();

        Invoke("RefreshEffectParticles", 0.1f);
    }

    public void SetResultAnimation(bool win)
    {
        string animName = win ? "f_victory_0" : "f_failure_0";
        foreach (var animation in target.Skeleton.Data.Animations)
        {
            if (animation.Name == animName)
            {
                SetAnimation(animName);
                return;
            }
        }
    }

    public void SetAnimation(string animName)
    {
        target.startingAnimation = animName;
        target.Initialize(true);
    }

    public SkeletonGraphic GetSkeletonGraphic()
    {
        return target;
    }
}
