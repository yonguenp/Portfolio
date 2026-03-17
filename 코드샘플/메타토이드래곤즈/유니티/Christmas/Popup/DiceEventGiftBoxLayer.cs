using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 선물 상자 관리 레이어
    /// </summary>
    public class DiceEventGiftBoxLayer : TabLayer, EventListener<DiceUIEvent>
    {
        [SerializeField]
        List<SubLayerInfoData> subLayerList = new List<SubLayerInfoData>();
        [SerializeField] GameObject reddot = null;

        [SerializeField]
        Sprite normalSprite = null;
        [SerializeField]
        Sprite selectedSprite = null;

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            ClearData();
            InitSubLayerIndex();
            EventManager.RemoveListener(this);
        }
        public override void InitUI(TabTypePopupData datas)
        {
            base.InitUI(datas);

            MoveLayer();
        }

        public override void RefreshUI()
        {
            subLayerList[SubLayerIndex].targetLayer.ForceUpdate();
        }
        public void MoveLayer()
        {
            if (SubLayerIndex < 0)
                SetSubLayerIndex(0);

            MoveLayer(SubLayerIndex);
        }
        public void MoveLayer(int index)
        {
            if (index < 0 || index >= subLayerList.Count)
                index = 0;

            foreach (var subLayerData in subLayerList)
            {
                if (subLayerData.targetLayer != null)
                    subLayerData.targetLayer.gameObject.SetActive(false);
            }

            SubLayer subLayer = subLayerList[index].targetLayer;
            subLayer.gameObject.SetActive(true);
            subLayer.Init();

            eventScheduleDesc tempDesc = new eventScheduleDesc(subLayerList[index].isShowDescText | subLayerList[index].isShowInfoIcon,
                subLayerList[index].isShowDescText, subLayerList[index].isShowInfoIcon);

            DiceUIEvent.RefreshDescInfo(tempDesc);

            RefreshReddot();
            SetSubTabButton(index);
            SetSubLayerIndex(index);
        }
        void SetSubTabButton(int _index)
        {
            if (subLayerList == null || subLayerList.Count <= 0)
                return;

            for (int i = 0; i < subLayerList.Count; i++)
            {
                var isSelected = _index == i;
                var button = subLayerList[i].targetButton;
                if (button == null)
                    continue;

                var image = button.GetComponent<Image>();
                if (image != null)
                    image.sprite = isSelected ? selectedSprite : normalSprite;
            }
        }
        public void OnClickChangeLayer(string customEventData)
        {
            var index = int.Parse(customEventData);
            MoveLayer(index);
        }

        public override GameObject[] GetSubLayerList()
        {
            List<GameObject> tempObjList = new List<GameObject>();

            if (subLayerList == null || subLayerList.Count <= 0)
                return tempObjList.ToArray();

            foreach (var sublayerData in subLayerList)
            {
                if (subLayerList == null)
                    continue;

                var targetData = sublayerData.targetLayer;
                if (targetData != null)
                    tempObjList.Add(targetData.gameObject);
            }

            return tempObjList.ToArray();
        }

        public override void RefreshReddot()
        {
            if (reddot != null)
                reddot.SetActive(DiceEventPopup.GetBoxReddotCondition());
        }

        public void OnEvent(DiceUIEvent eventType)
        {
            switch (eventType.type)
            {
                case DiceUIEvent.eDiceUI.REFRESH_TAB_REDDOT:
                    RefreshReddot();
                    break;
            }
        }
    }

}
