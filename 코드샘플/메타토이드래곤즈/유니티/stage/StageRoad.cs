using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static SandboxNetwork.BattleDragonListView;

namespace SandboxNetwork { 
    public class StageRoad : MonoBehaviour
    {

        [SerializeField]
        Image AbleRootImage=null;

        [SerializeField]
        Image DisAbleRootImage = null;

        [SerializeField]
        Color AbleColor = Color.white;
        [SerializeField]
        Color DisableColor = Color.white;

        public delegate void callback(int world, int stage);
        callback clickCallBack;


        bool isOpen = false;
        int worldIndex = 0;
        int stageIndex = 0;
        public void SetRoad(bool state, int world, int stage, bool last)
        {
            if(DisAbleRootImage != null)
            {
                if (last)
                {
                    DisAbleRootImage.gameObject.SetActive(false);
                    AbleRootImage.gameObject.SetActive(false);
                }
                else
                {
                    DisAbleRootImage.gameObject.SetActive(!state);
                    AbleRootImage.gameObject.SetActive(state);
                }
            }
            else
            {
                if (last)
                    AbleRootImage.gameObject.SetActive(false);
                else
                    AbleRootImage.gameObject.SetActive(true);
                AbleRootImage.color = state ? AbleColor : DisableColor;
            }
            isOpen = state;
            worldIndex = world;
            stageIndex = stage;
        }
        public void SetLastRoad()
        {
            GetComponent<RectTransform>().sizeDelta = Vector2.one * 150f;
        }

        public void SetRoadClickCallBack(callback cb)
        {
            if(cb != null)
            {
                clickCallBack = cb;
            }
        }
        public void OnClickRoad()
        {
            if (isOpen==false) return;
            if(clickCallBack !=null)
            {
                clickCallBack(worldIndex, stageIndex);
            }
            
        }
    }
}