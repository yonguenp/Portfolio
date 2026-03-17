using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 하위 sublayer 탭에는 복주머니 강화, 상점 탭이 들어감.
/// </summary>

namespace SandboxNetwork
{
    public class LuckyBagEventTabLayer : TabLayer, EventListener<LuckyBagUIEvent>
    {
        [SerializeField]
        List<SubLayerInfoData> subLayerList = new List<SubLayerInfoData>();

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

            RefreshReddot();
            SetSubLayerIndex(index);
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

        public void OnEvent(LuckyBagUIEvent eventType)
        {
            switch (eventType.type)
            {
                case LuckyBagUIEvent.eLuckyBagUIType.REFRESH_TAB_REDDOT:
                    RefreshReddot();
                    break;
            }
        }
    }
}



