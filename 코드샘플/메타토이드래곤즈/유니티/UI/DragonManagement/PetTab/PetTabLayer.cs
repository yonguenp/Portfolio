using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetTabLayer : TabLayer
    {
        [SerializeField]
        GameObject[] subLayerList = null;

        [SerializeField]
        Text dragonTitleText = null;



        [SerializeField]
        RectTransform titleTextLayoutRect = null;


        int currentLayerIndex = -1;

        Tween SelectObjMoveTween = null;
        public virtual UserPetData GetPetInfo()
        {
            return User.Instance.PetData;
        }
        public virtual UserDragonData GetDragonInfo()
        {
            return User.Instance.DragonData;
        }

        public virtual int PetTagInfo
        {
            get
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
            }
            set
            {
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = value;
            }
        }

        public virtual int DragonTagInfo
        {
            get
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            }
            set
            {
                PopupManager.GetPopup<DragonManagePopup>().CurDragonTag = value;
            }
        }
        void OnDisable()
        {
            currentLayerIndex = -1;
        }

        public override void InitUI(TabTypePopupData datas)
        {
            base.InitUI(datas);

            if(DragonTagInfo > 0)
            {
                string name = CharBaseData.Get(DragonTagInfo)._NAME;
                dragonTitleText.text = StringData.GetStringFormatByStrKey("드래곤이름의펫", StringData.GetStringByStrKey(name));
                LayoutRebuilder.ForceRebuildLayoutImmediate(titleTextLayoutRect);
            }
            else
            {
                dragonTitleText.text = string.Empty;
            }
            moveLayer();
        }

        public override void RefreshUI()
        {
            subLayerList[currentLayerIndex].GetComponent<SubLayer>().ForceUpdate();
        }
        public void moveLayer()
        {
            int index = 0;
            if (Datas != null)
                index = Datas.SubIndex;

            moveLayer(index);
        }
        public void moveLayer(int index)
        {
            if (index < 0 || index >= subLayerList.Length)
                index = 0;
            if (index == 5)
            {
                if (PetTagInfo <= 0) // 레벨업의 경우 선택된 펫이 있어야 만 가능
                {
                    ToastManager.On(100001844);
                    return;
                }
            }
            
            Array.ForEach(subLayerList, element =>
            {
                if (element != null)
                {
                    element.gameObject.SetActive(false);
                }
            });

            SubLayer subLayer = subLayerList[index].GetComponent<SubLayer>();
            subLayer.gameObject.SetActive(true);
            subLayer.Init();

            currentLayerIndex = index;
        }

        public void onClickChangeLayer(string customEventData)
        {
            var index = int.Parse(customEventData);
            moveLayer(index);
        }

        public override GameObject[] GetSubLayerList()
        {
            return subLayerList;
        }
    }
}
