using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LunaQuestLayer : TabLayer
    {
        [SerializeField]
        SubLayer subLayer;

        public override void InitUI(TabTypePopupData datas)
        {
            base.InitUI(datas);
            
            MoveLayer();
        }

        public override void RefreshUI()
        {
            subLayer.ForceUpdate();
        }

        public void MoveLayer()
        {
            if (SubLayerIndex < 0)
                SetSubLayerIndex(0);

            MoveLayer(SubLayerIndex);
        }
        public void MoveLayer(int index)
        {
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
            tempObjList.Add(subLayer.gameObject);

            return tempObjList.ToArray();
        }
    }
}