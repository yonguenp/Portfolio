using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleDragonSelectTabLayer : TabLayer
    {
        [SerializeField]
        GameObject[] subLayerList = null;

        [SerializeField]
        GameObject gotoListObject = null;//UI가 풀 스크린 형태로 전체적으로 바뀌면 지울 것.(드래곤 리스트만 풀스크린이라 임시처리)

        int currentLayerIndex = -1;

        void OnDisable()
        {
            currentLayerIndex = -1;
        }

        public override void InitUI(TabTypePopupData datas)
        {
            base.InitUI(datas);
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

            if (currentLayerIndex == index)
            {
                return;
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
