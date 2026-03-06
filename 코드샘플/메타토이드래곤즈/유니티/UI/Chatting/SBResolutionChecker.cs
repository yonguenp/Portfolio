using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SandboxNetwork
{
    public class SBResolutionChecker : UIBehaviour
    {

        protected override void Start()
        {

        }
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();


            initLog();
        }

        void initLog()//디바이스 해상도
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            //Debug.Log("change Resolution : width " + screenWidth + " height : " + screenHeight);


            var currentBeacon = PopupManager.Instance.Beacon;
            if (currentBeacon == null)
                return;

            var beaconRect = currentBeacon.GetComponent<RectTransform>();
            if (beaconRect == null)
                return;

            var RectData = beaconRect.rect;
            var size = beaconRect.sizeDelta;

            Debug.Log("change Resolution : width " + screenWidth + " height : " + screenHeight + " real resolution Width : " + RectData.width + " height : " + RectData.height);

            var diffX = (float)screenWidth / (float)RectData.width;
            var diffY = (float)screenHeight / (float)RectData.height;

            Debug.Log("width ratio : " + diffX + " height ratio :" + diffY);

            //19:6 비율로 따졌을 때의 수정값


            var resolutionValue = (float)1280f / (float)720f;
            var diffResolution = (float)screenWidth / (float)screenHeight;

            if(resolutionValue != diffResolution)
            {






            }

        }
    }
}