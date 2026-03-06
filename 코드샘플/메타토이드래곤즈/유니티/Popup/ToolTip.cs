using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//용도에 맞게 상속받아 구현- 일단 itemToolTip 따로 만듦.
namespace SandboxNetwork
{
    public class ToolTip : Popup<TooltipPopupData>
    {
        [Space(10)]
        [Header("Default Setting")]
        [SerializeField]
        Text labelTitle = null;
        [SerializeField]
        Text labelBody = null;
        [SerializeField]
        protected GameObject nodeBody = null;

        [Space(10)]
        [Header("Bubble Setting")]
        [SerializeField]
        private int tailPadding = 200;
        [SerializeField]
        VerticalLayoutGroup TailLayoutGroup = null;

        [Header("[Item Tool Tip]")]
        [SerializeField] protected GameObject moveButtonObject = null;
        [SerializeField] protected RateBoardBtn rateBtn = null;

        protected GameObject parentObject = null;
        protected ItemFrame itemFrameData = null;
        protected ItemBaseData itemData = null;
        
        [SerializeField]
        GameObject[] tails = null; // LB RB LT RT 순

        /*
         * 사용법 - 세팅데이터 ToolTipData 이건 필요한만큼 자유롭게 튜닝해도 무방
         * PopupManager.OpenPopup<ToolTip>(), ToolTipData.Instantiate(skillName,skillDescName,
         * SBFunc.GetChildrensByName(this.gameObject.transform, new string[] { "툴팁을 만들기 위한 부모 게임 오브젝트" }).gameObject), true);
         */
        public override void InitUI()
        {
            SetData();
            SetMessage();
            SetPosition();
        }

        void SetData()
        {
            itemData = null;

            if (Data == null || Data.TipData == null) return;

            parentObject = Data.TipData.Parent;
            itemFrameData = parentObject.GetComponent<ItemFrame>();
            if (itemFrameData != null)
            {
                itemData = ItemBaseData.Get(itemFrameData.GetItemID());
            }
            SetRateBtn();
        }

        public virtual void  SetRateBtn(int itemNo = 0)
        {
            if (rateBtn == null)
                return;

            if(itemNo == 0) 
                rateBtn.gameObject.SetActive(false);
            else
            {
                var itemData = ItemBaseData.Get(itemNo);
                if (itemData != null)
                {
                    if(itemData.KIND == eItemKind.GACHA)
                    {
                        rateBtn.SetRateType(itemNo);
                        rateBtn.SetClickCallBack(() => ClosePopup());
                        rateBtn.gameObject.SetActive(true);
                    }
                    else
                        rateBtn.gameObject.SetActive(itemData.ENABLE_RATE_TABLE);
                }
                else
                    rateBtn.gameObject.SetActive(false);
            }
        }

        void SetMessage()
        {
            if(Data == null)
            {
                return;
            }

            var titleStr = Data.TipData.TitleStr;
            var contentStr = Data.TipData.ContentStr;

            var titleLabelFlag = Data.TipData.Flag.HasFlag(eToolTipDataFlag.TITLE);
            var contentLabelFlag = Data.TipData.Flag.HasFlag(eToolTipDataFlag.DESC);

            if (labelTitle != null)
            {
                labelTitle.gameObject.SetActive(titleLabelFlag);
                labelTitle.text = titleStr;
            }
            if (labelBody != null)
            {
                labelBody.gameObject.SetActive(contentLabelFlag);
                labelBody.text = contentStr;
            }
        }

        protected virtual void SetPosition()
        {
            if(parentObject == null) { return;}
            if (nodeBody == null) { return; }

            var parentPos = parentObject.transform.position;
            var parentScale = parentObject.transform.parent.localScale;//오브젝트에 걸려있는 부모의 스케일
            var parentSizeY = parentObject.GetComponent<RectTransform>().sizeDelta.y;//높이
            var isReverse = Data.TipData.IsReverse;
            var isUpDown = Data.TipData.IsUpSideDown;

            foreach(var tail in tails)
            {
                tail.SetActive(false);
            }

            if (TailLayoutGroup != null)
            {
                TailLayoutGroup.padding.right = isReverse? -tailPadding : tailPadding;
            }

            nodeBody.transform.position = parentPos;//일단 가운대로 세팅

            var bgNode = SBFunc.GetChildrensByName(nodeBody.transform, new string[] { "bg" }).gameObject;//기존 (0,0)으로 잡혀있는 좌표 수정
            if (bgNode != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(bgNode.GetComponent<RectTransform>());//contentSize Fitter로 인한 sizeDelta 0 버그 해소용 코드
                var bgNodeHeight = bgNode.GetComponent<RectTransform>().sizeDelta.y;
                var xPos = isReverse ? -180f : 180f;

                var scaleOneVec = new Vector2(xPos, bgNodeHeight * 0.5f + parentSizeY * 0.5f * parentScale.y);//1스케일 기준 좌표 ,x좌표는 일단 하드코딩
                bgNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(scaleOneVec.x , isUpDown ? -scaleOneVec.y :scaleOneVec.y);
                int tailIndex = 0;
                tailIndex += isUpDown ? 2 : 0;
                tailIndex += isReverse ? 1 : 0;
                tails[tailIndex].SetActive(true);
            }

            // 바로가기 버튼
            if (moveButtonObject != null)
            {
                moveButtonObject?.SetActive(IsAvailMoveItem());
            }
        }

        protected bool IsAvailMoveItem()
        {
            if (itemData == null)
                return false;

            return itemData.KIND switch
            {
                eItemKind.EXP => true,
                eItemKind.RECEIPE => true,
                eItemKind.PRODUCT => true,
                _ => false
            };
        }

        public static ToolTip OnToolTip(Asset asset, GameObject targetObject)
        {
            int itemID;
            string itemName;
            string itemDESC;

            switch (asset.GoodType)
            {
                case eGoodType.GOLD:
                {
                    itemID = 10000001;
                    itemName = "item_base:name:10000001";
                    itemDESC = "item_base:desc:10000001";
                }
                break;
                case eGoodType.ACCOUNT_EXP:
                {
                    itemID = 10000003;
                    itemName = "item_base:name:10000003";
                    itemDESC = "item_base:desc:10000003";
                }
                break;
                case eGoodType.GEMSTONE:
                {
                    // todo - 추후 무료/유료 재화 구분 추가 필요함
                    //...
                    if (asset.ItemNo == 10000006)
                    {
                        itemID = 10000006;
                        itemName = "item_base:name:10000006";
                        itemDESC = "item_base:desc:10000006";
                    }
                    else
                    {
                        itemID = 10000005;
                        itemName = "item_base:name:10000005";
                        itemDESC = "item_base:desc:10000005";
                    }
                }
                break;
                case eGoodType.ENERGY:
                {
                    itemID = 10000002;
                    itemName = "item_base:name:10000002";
                    itemDESC = "item_base:desc:10000002";
                }
                break;
                case eGoodType.ARENA_TICKET:
                {
                    itemID = 10000007;
                    itemName = "item_base:name:10000007";
                    itemDESC = "item_base:desc:10000007";
                }
                break;
                case eGoodType.MAGNET:
                {
                    itemID = 10000009;
                    itemName = "item_base:name:10000009";
                    itemDESC = "item_base:desc:10000009";
                }
                break;
                case eGoodType.ARENA_POINT:
                {
                    itemID = 10000010;
                    itemName = "item_base:name:10000010";
                    itemDESC = "item_base:desc:10000010";
                }
                break;
                case eGoodType.FRIENDLY_POINT:
                {
                    itemID = 10000008;
                    itemName = "item_base:name:10000008";
                    itemDESC = "item_base:desc:10000008";
                }
                break;
                case eGoodType.GUILD_EXP:
                {
                    itemID = 10000022;
                    itemName = "item_base:name:10000022";
                    itemDESC = "item_base:desc:10000022";
                }
                break;
                case eGoodType.GUILD_POINT:
                {
                    itemID = 10000023;
                    itemName = "item_base:name:10000023";
                    itemDESC = "item_base:desc:10000023";
                }
                break;
                case eGoodType.MAGNITE:
                {
                    itemID = 10000024;
                    itemName = "item_base:name:10000024";
                    itemDESC = "item_base:desc:10000024";
                }
                break;
                case eGoodType.CHARACTER:
                {
                    var charData = CharBaseData.Get(asset.ItemNo);
                    if (charData != null)
                    {
                        itemName = StringData.GetStringByStrKey(charData._NAME);
                        //레전드드래곤의 경우 드래곤 설명이 너무 길어서 UI가 깨져보여서 설명을 없앰.
                        //itemDESC = StringData.GetStringByStrKey(charData._DESC);
                        itemDESC = "";
                    }
                    else
                    {
                        itemName = "";
                        itemDESC = "";
                    }

                    itemID = 0;

                    if (string.IsNullOrEmpty(itemDESC) || itemDESC == "0")
                    {
                        return NameTagToolTip.OpenPopup(itemName, targetObject);
                    }
                }
                break;
                default:
                    itemID = asset.ItemNo;
                    itemDESC = itemName = string.Empty;
                    break;
            }

            ItemBaseData itemInfo = ItemBaseData.Get(itemID);
            if (itemInfo != null)
            {
                itemName = itemInfo.NAME;
                itemDESC = itemInfo.DESC;
            }
            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetObject.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.Instance.GetComponent<Canvas>().GetComponent<RectTransform>(), screenPos, Camera.main, out Vector2 localPos);
            var parent = PopupManager.Instance.Beacon;
            var beaconScale = parent.transform.localScale;
            bool reverseFlag = !(localPos.x < 800 * beaconScale.x);//640 기준 일단 하드코딩(1280/2) -> 2배로 늘림
            bool upDownFlag = !(localPos.y < 300 * beaconScale.y);
            ItemFrame frame = null;
            frame = targetObject.GetComponent<ItemFrame>();
            var tooltip = PopupManager.OpenPopup<ItemToolTip>(new TooltipPopupData(new ItemToolTipData(itemName, itemDESC, targetObject, reverseFlag, upDownFlag, eToolTipDataFlag.Default, frame)));
            tooltip.SetRateBtn(itemID);
            return tooltip;

        }
    }
}
