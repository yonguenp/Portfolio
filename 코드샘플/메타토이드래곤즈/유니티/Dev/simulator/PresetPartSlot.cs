using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG
namespace SandboxNetwork
{
    public class PresetPartSlot : MonoBehaviour
    {
        [SerializeField] private Image iconTarget = null;
        [SerializeField] private Sprite emptySprite = null;
        [SerializeField] private Button partLevelButton = null;

        [SerializeField] private GameObject[] subOptionSlotList = null;
        [SerializeField] private Color[] subOptionColorList = null;
        [SerializeField] private Sprite subOptionSprite = null;

        [SerializeField] private GameObject clickNode = null;

        bool isClicked = false;
        
        //장비 프리펩 갱신(레벨, 슬롯, 부옵 , 

        PresetPart presetData = null;

        public delegate void func(PresetPart CustomEventData);

        private func clickCallback = null;

        public void SetData(PresetPart _data)
        {
            if(_data == null)
            {
                iconTarget.sprite = emptySprite;
                var textLabel = partLevelButton.GetComponentInChildren<Text>();
                if(textLabel != null)
                    textLabel.text = "none";

                initAllSubOptionSlot();
                return;
            }
            presetData = _data;

            refreshPartIcon();
            refreshButtonLabel();
            refreshSubOptionSlot();
        }


        void refreshPartIcon()
        {
            if (iconTarget == null)
            {
                return;
            }
            var partData = PartBaseData.Get(presetData.GetKey());
            
            if(partData != null)
                iconTarget.sprite = partData.ITEM.ICON_SPRITE;
        }

        void refreshButtonLabel()
        {
            var text = partLevelButton.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = SBFunc.StrBuilder("level ", presetData.Level.ToString());
            }
            else
            {
                text.text = SBFunc.StrBuilder("level ", 0);
            }
        }

        void refreshSubOptionSlot()
        {
            var subs = presetData.Subs;//일단은 최대값으로 세팅
            bool isVaildTag = (subs != null && subs.Length > 0);

            if (!isVaildTag)
            {
                AllHideSubOptionSlot();
            }
            else
            {
                ShowSubOptionSlot(subs.Length);
                SetSubOptionColor();
            }
        }
        void AllHideSubOptionSlot()
        {
            if (subOptionSlotList == null || subOptionSlotList.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < subOptionSlotList.Length; i++)
            {
                var go = subOptionSlotList[i];
                if (go == null)
                {
                    continue;
                }
                go.SetActive(false);
            }
        }

        void ShowSubOptionSlot(int count)//현재 레벨에 따른 슬롯 갯수보다 데이터가 잡혀있을 경우 강제로 삭제
        {
            if (subOptionSlotList == null || subOptionSlotList.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < subOptionSlotList.Length; i++)
            {
                var go = subOptionSlotList[i];
                if (go == null)
                {
                    continue;
                }

                var isShow = i < count;
                go.SetActive(isShow);
            }
        }
        void SetSubOptionColor()//부옵 옵션에 따른 색상 표시하기
        {
            var currentPartOption = presetData.Subs;
            if (currentPartOption == null || currentPartOption.Length <= 0)
            {
                initAllSubOptionSlot();
                return;
            }

            Sprite targetSprite = null;
            Color targetColor = Color.white;
            string targetText = "";
            for (var i = 0; i < currentPartOption.Length; i++)
            {
                var optionID = currentPartOption[i];

                if (optionID <= 0)
                {
                    targetSprite = emptySprite;
                    targetColor = Color.white;
                    targetText = "";
                }
                else
                {
                    var partData = SubOptionData.Get(optionID);
                    var colorIndex = 0;
                    var valueType = partData.VALUE_TYPE;
                    switch (partData.STAT_TYPE)
                    {
                        case "ATK":
                            colorIndex = 0;
                            break;
                        case "DEF":
                            colorIndex = 1;
                            break;
                        case "HP":
                            colorIndex = 2;
                            break;
                        case "CRI":
                            colorIndex = 3;
                            break;

                    }
                    targetColor = subOptionColorList[colorIndex];
                    targetSprite = subOptionSprite;
                    targetText = valueType == "PERCENT" ? "%" : "+";
                }

                var targetImage = SBFunc.GetChildrensByName(subOptionSlotList[i].transform, "icon").GetComponent<Image>();
                var targetLabel = SBFunc.GetChildrensByName(subOptionSlotList[i].transform, "label").GetComponent<Text>();
                targetImage.sprite = targetSprite;
                targetImage.color = targetColor;
                targetLabel.text = targetText;
            }
        }
        void initAllSubOptionSlot()
        {
            if (subOptionSlotList == null || subOptionSlotList.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < subOptionSlotList.Length; i++)
            {
                var go = subOptionSlotList[i];
                if (go == null)
                {
                    continue;
                }

                var targetImage = SBFunc.GetChildrensByName(subOptionSlotList[i].transform, "icon").GetComponent<Image>();
                var targetLabel = SBFunc.GetChildrensByName(subOptionSlotList[i].transform, "label").GetComponent<Text>();
                targetImage.sprite = emptySprite;
                targetImage.color = Color.white;
                targetLabel.text = "";
            }
        }

        public void setCallback(func ok_cb)
        {
            //this.eFuncType = FrameFunctioal.CallBack;

            if (ok_cb != null)
            {
                clickCallback = ok_cb;
            }
        }

        public void onClickSlot()
        {
            if(clickCallback != null)
            {
                clickCallback(presetData);
            }
        }
        public void SetVisibleClickNode(bool isVisible)
        {
            if (clickNode != null)
            {
                clickNode.SetActive(isVisible);
            }
            isClicked = isVisible;
        }
    }
}

#endif