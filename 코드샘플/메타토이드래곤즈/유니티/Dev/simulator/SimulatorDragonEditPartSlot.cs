using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SimulatorDragonEditPartSlot : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        public int SlotIndex { get { return slotIndex; } }

        [SerializeField] private Image iconTarget = null;
        [SerializeField] private Sprite emptySprite = null;
        [SerializeField] private Button partLevelButton = null;
        [SerializeField] private Button presetSaveButton = null;
        [SerializeField] private GameObject[] subOptionSlotList = null;
        [SerializeField] private Color[] subOptionColorList = null;
        [SerializeField] private Sprite subOptionSprite = null;

        private int currentSelectPartTag = -1;
        public int PartTag { get { return currentSelectPartTag; } }

        UserPart currentSelectPart = null;
        public UserPart Part { get { return currentSelectPart; }}

        UserPart prevPart = null;//이전 파츠 백업용도
        public UserPart PrevPart { get { return prevPart; } }

        PartTable partTable = null;
        SubOptionTable partOptionTable = null;


        public void initSavePrevPart(int partTag = -1)//이전 데이터 들고있기
        {
            if(partTag <= 0)
            {
                prevPart = null;
                return;
            }

            var partData = User.Instance.PartData.GetPart(partTag);//기존 장비
            if(partData == null)
            {
                return;
            }

            UserPart p = new UserPart(partData.Tag, partData.ID, partData.Obtain, partData.LinkDragonTag, partData.Reinforce);
            var partOptionList = partData.SubOptionList.ToList();
            for(var i = 0; i< partOptionList.Count; i++)
            {
                p.SetPartOption(partOptionList[i], i);
            }

            prevPart = p;
        }

        public int dumpPrevPart()//이전 장비로 씌우고 이전 파트 태그 가져오기
        {
            int prevPartTag;
            if(prevPart != null)
            {
                User.Instance.PartData.AddUserPart(prevPart);
                prevPartTag = prevPart.Tag;
                prevPart = null;
            }
            else//이전에 장비가 없었을 때
            {
                User.Instance.PartData.DeleteUserPart(currentSelectPartTag);
                currentSelectPartTag = -1;
                prevPartTag = 0;
            }
            return prevPartTag;
        }

        public void RefreshPartInfo(int partTag = -1)
        {
            if(partTable == null)
            {
                partTable = TableManager.GetTable<PartTable>();
            }

            if(partOptionTable == null)
            {
                partOptionTable = TableManager.GetTable<SubOptionTable>();
            }

            partLevelButton.gameObject.SetActive(partTag > 0);
            presetSaveButton.gameObject.SetActive(partTag > 0);

            currentSelectPartTag = partTag;

            SetPartData();//장비 데이터 가져옴

            refreshPartIcon();//장비 아이콘 갱신
            refreshButtonLabel();//장비 레벨 라벨 갱신
            refreshSubOptionSlot();//장비 부옵 갱신
        }

        void SetPartData()
        {
            var partData = User.Instance.PartData.GetPart(currentSelectPartTag);
            if (partData == null)
            {
                return;
            }

            currentSelectPart = partData;
        }


        void refreshPartIcon()
        {
            if(iconTarget == null)
            {
                return;
            }

            if(currentSelectPartTag <= 0)
            {
                iconTarget.sprite = emptySprite;
                return;
            }

            iconTarget.sprite = currentSelectPart.ICON_SPRITE;
        }

        void refreshButtonLabel()
        {
            var text = partLevelButton.GetComponentInChildren<Text>();
            if(text != null && currentSelectPart != null)
            {
                text.text = currentSelectPart.Reinforce.ToString();
            }
            else
            {
                text.text = "0";
            }
        }

        void refreshSubOptionSlot()
        {
            bool isVaildTag = currentSelectPartTag > 0;

            if (!isVaildTag)
            {
                AllHideSubOptionSlot();
            }
            else
            {
                if(currentSelectPart == null)
                {
                    return;
                }

                var partId = currentSelectPart.ID;
                var currentOpenSlotCount = partTable.GetCurrentReinforceSlotCount(partId, currentSelectPart.Reinforce);//해금 가능 최대 갯수

                ShowSubOptionSlot(currentOpenSlotCount);
                SetSubOptionColor();
            }
        }

        void AllHideSubOptionSlot()
        {
            if(subOptionSlotList == null || subOptionSlotList.Length <= 0)
            {
                return;
            }

            for(var i = 0; i < subOptionSlotList.Length; i++)
            {
                var go = subOptionSlotList[i];
                if(go == null)
                {
                    continue;
                }
                go.SetActive(false);
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

        void ShowSubOptionSlot(int count)//현재 레벨에 따른 슬롯 갯수보다 데이터가 잡혀있을 경우 강제로 삭제
        {
            if (subOptionSlotList == null || subOptionSlotList.Length <= 0)
            {
                return;
            }

            var subOptionList = currentSelectPart.SubOptionList;

            for (var i = 0; i < subOptionSlotList.Length; i++)
            {
                var go = subOptionSlotList[i];
                if (go == null)
                {
                    continue;
                }

                var isShow = i < count;
                go.SetActive(isShow);

                if(!isShow &&  subOptionList.Count > i)
                {
                    subOptionList[i] = default;
                }
            }
        }

        void SetSubOptionColor()//부옵 옵션에 따른 색상 표시하기
        {
            var currentPartOption = currentSelectPart.SubOptionList;

            if(currentPartOption == null || currentPartOption.Count <= 0)
            {
                initAllSubOptionSlot();
                return;
            }

            Sprite targetSprite = null;
            Color targetColor = Color.white;
            string targetText = "";
            for(var i = 0; i< currentPartOption.Count; i++)
            {
                var data = currentPartOption[i];

                int key = data.Key;
                if (key < 1)
                    continue;

                float value = data.Value;

                var partData = partOptionTable.Get(key);
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

                var targetImage = SBFunc.GetChildrensByName(subOptionSlotList[i].transform, "icon").GetComponent<Image>();
                var targetLabel = SBFunc.GetChildrensByName(subOptionSlotList[i].transform, "label").GetComponent<Text>();
                targetImage.sprite = targetSprite;
                targetImage.color = targetColor;
                targetLabel.text = targetText;
            }
        }
    }
}
