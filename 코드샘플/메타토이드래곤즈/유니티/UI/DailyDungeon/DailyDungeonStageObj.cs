using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class DailyDungeonStageObj : MonoBehaviour
    {
        [SerializeField]
        private Text dungeonLvText = null;
        [SerializeField]
        private GameObject lockObj = null;
        [SerializeField]
        private GameObject openObj = null;

        [SerializeField]
        private Image bgImg = null;
        [SerializeField]
        private Color selectedColor = Color.white;
        [SerializeField]
        private Color noneSelectedColor = Color.white;

        bool lockState = false;
        int curLv = 0;

        public delegate void funcInt(int value);
        private funcInt clickCB = null;

        public void Init(int lv,bool isLock, funcInt cb)
        {
            dungeonLvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", lv);
            lockState = isLock;
            clickCB = cb;
            curLv = lv;
            lockObj.SetActive(isLock); 
            openObj.SetActive(!isLock);
        }

        public void SetColor(bool isSelected)
        {
            bgImg.color = isSelected ? selectedColor : noneSelectedColor;
        }

        public void OnClickLevel()
        {
            if (lockState)
            {
                ToastManager.On(100000629);
            }
            else
            {
                clickCB?.Invoke(curLv);
            }
        }
    }
}