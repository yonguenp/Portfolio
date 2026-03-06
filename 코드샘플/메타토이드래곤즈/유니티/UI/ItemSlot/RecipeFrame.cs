using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{
    public class RecipeFrame : MonoBehaviour
    {
        [SerializeField]
        GameObject itemInfoLayer = null;

        [SerializeField]
        Image receipeIcon = null;


        [SerializeField]
        Image BackGageImage = null;


        [SerializeField]
        Text prodCountText = null;

        [SerializeField]
        Image frameImage = null;

        [SerializeField]
        public Image sprChecker = null;

        private int frameIndex = -1;
        private TimeObject timeObj = null;
        private bool inInit = false;
        private int buildingTag = 0;
        private Popup<JObject> tPopup = null;

        private int prodAmount;

        public int Amount
        {
            get { return prodAmount; }
        }

        private int frameReqTime = 0;
        private int frameProdReqTime = 0;
        private int prodItemID = 0;
        private bool useOnclick  = true;
        public ProductData RecipeProductData { get; private set; } = null;
        private ProductAutoData productAutoData = null;
        private bool isClicked = false;

        private Tween tween = null;

        public delegate void func(GameObject node, int amount);

        private func onClickCallBack = null;

        public func OnClickCallback
        {
            set {
                if (onClickCallBack == null)
                {
                    onClickCallBack = value;
                }
            }
        }

        public delegate void voidFunc();
        private voidFunc timeRefreshCallBack = null;

        public voidFunc TimeRefreshCallBack
        {
            set
            {
                if (timeRefreshCallBack == null)
                {
                    timeRefreshCallBack = value;
                }
            }
        }

        public void SetFrameBlank()
        {
            inInit = false;
            SetProductingEffectState(false);
            BackGageImage.gameObject.SetActive(false);
            timeObj = BackGageImage.GetComponent<TimeObject>();
            receipeIcon.sprite = null;
            sprChecker.gameObject.SetActive(false);
            itemInfoLayer.SetActive(false);
            RecipeProductData = null;
        }

        /**
         * 아이콘만 있는 레시피 프레임
         * @param pData 레시피 정보 : ProductData
         */
        public void SetReceipeIcon(int index, ProductData pData,int reqTime,int tag = 0, voidFunc _timeRefreshCallBack = null)
        {
            RecipeProductData = pData;

            SetReceipeInfo(index, pData.ProductItem, pData.ICON_SPRITE, pData.PRODUCT_TIME, reqTime, tag, true, _timeRefreshCallBack);
        }

        public void SetReceipeIcon(int index, ProductAutoData pData, int reqTime, int tag = 0, voidFunc _timeRefreshCallBack = null)
        {
            productAutoData = pData;

            SetReceipeInfo(index, pData.ProductItem, pData.ProductItem.BaseData.ICON_SPRITE, pData.TERM, reqTime, tag, true, _timeRefreshCallBack);
        }

        /** 
         * 기획 데이터에 일반 생산과 자동생산이 나뉘어 있어
         * 일반 생산과 자동 생산 정보 Class인 ProductData를 동시 사용이 불가능하며
         * 해당 문제는 레시피 프레임의 내용을 중복하게 만듦.
         * 
         * 이후 모든 itemframe 관련 frame들을 
         * frame 하위 클래스들로 작성, 
         * 일반 frame, 재료 frame, 생산 레시피 frame(자동), 생산 레시피 frame(일반) 등으로 상세한 구현이 필요
        */

        public void SetReceipeInfo(int index, Item item, Sprite icon, int prodReqTime, int reqTime, int tag = 0, bool useonClick = true, voidFunc _timeRefreshCallBack = null)
        {
            useOnclick = useonClick;
            buildingTag = tag;
            frameIndex = index;
            inInit = true;
            timeObj = BackGageImage.GetComponent<TimeObject>();
            prodItemID = item.ItemNo;
            BackGageImage.gameObject.SetActive(false);
            SetProductingEffectState(false);
            frameReqTime = reqTime < 0 ? 0 : reqTime;
            
            receipeIcon.sprite = icon;
            bool isActive = reqTime == -1 || reqTime != 0 && reqTime <= TimeManager.GetTime();
            sprChecker.gameObject.SetActive(isActive);
            prodCountText.gameObject.SetActive(!isActive);
            prodAmount = item.Amount;
            frameProdReqTime = prodReqTime;
            itemInfoLayer.SetActive(true);
            if (_timeRefreshCallBack != null)
                timeRefreshCallBack = _timeRefreshCallBack;

            if (prodAmount > 1)
            {
                prodCountText.text = prodAmount.ToString();
            }
            else
            {
                prodCountText.text = "";
            }

            int totalTime = frameProdReqTime;
            float tt = frameReqTime - TimeManager.GetTime();
            BackGageImage.fillAmount = (totalTime - (frameReqTime - TimeManager.GetTime())) / (float)totalTime;
            if (timeObj != null && frameReqTime - TimeManager.GetTime() > 0)
            {
                timeObj.Refresh = delegate
                {
                    float remain = frameReqTime - TimeManager.GetTime();
                    BackGageImage.fillAmount = (totalTime - (frameReqTime - TimeManager.GetTime())) / (float)totalTime;

                    if (remain <= 0)
                    {
                        if (tPopup != null && tPopup?.gameObject != null)
                        {
                            //팝업 닫기 호출
                            tPopup.Close();
                            ToastManager.On(100002563);
                        }
                        tPopup = null;
                        BackGageImage.gameObject.SetActive(false);
                        timeObj.Refresh = null;
                        sprChecker.gameObject.SetActive(true);
                        prodCountText.gameObject.SetActive(false);

                        if (timeRefreshCallBack != null)
                            timeRefreshCallBack();
                    }
                };
            }
            isClicked = false;
        }

    /**
     * 레시피 프레임의 타이머 시작
     * setFrameBlank() 으로 이니셜라이징 된 경우 아무 효과없음
     */
        public void TimerStart()
        {
            if (!inInit)
                return;

            BackGageImage.gameObject.SetActive(true);
            SetProductingEffectState();
        }
        public bool IsTimeObjectRunning()
        {
            return timeObj.Refresh != null;
        }

        void SetProductingEffectState(bool isProducting = true)
        {
            if (isProducting)
            {
                if (tween == null)
                {
                    tween = receipeIcon.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
				}
                if(frameImage != null) { 
                    frameImage.color = Color.yellow;
                }
                // new Color(38/255f,222/255f,0);
            }
            else
            {
                if (tween != null)
                {
                    tween.Kill();
                }
                if (frameImage != null)
                {
                    frameImage.color = Color.white;
                }
            }
        }

    /**
     * 레시피 프레임 클릭 시 이벤트
     * @param event 
     * @param custom 
     */
        public void OnClick()
        {
            if (buildingTag == 0 || !useOnclick)//isQueueFrame 에 따라 서버 탈지 안탈지
                return;

            if (RecipeProductData == null)
                return;

            if (timeObj != null && frameReqTime - TimeManager.GetTime() > 0 && frameReqTime <= TimeManager.GetTime() + frameProdReqTime)//타이머가 흐르는 경우
            {
                AccelerationMainPopup.OpenPopup(eAccelerationType.JOB, buildingTag, frameProdReqTime, frameReqTime, frameIndex,()=> {
                    if (onClickCallBack != null)
                    {
                        onClickCallBack(null, -1);
                    }
                }
                , () =>
                {
                    timeRefreshCallBack();
                });

                return;
            }
            
            else if (sprChecker.gameObject.activeInHierarchy)//생산이 완료된 경우
            {
                List<Asset> resultItemList = new List<Asset>();
                resultItemList.Add(new Asset(eGoodType.ITEM, prodItemID, prodAmount));

                if (User.Instance.CheckInventoryGetItem(resultItemList))
                {
                    SetInventoryFullAlert();
                    return;
                }
            }
            else//취소할 수 있는 대기열인 경우
            {
                if(RecipeProductData.needitemLength == 0 && RecipeProductData.NEED_GOLD == 0) return;

                List<Asset> itemList = new List<Asset>();

                for (int i = 0; i < RecipeProductData.needitemLength; ++i)
                {
                    itemList.Add(new Asset(eGoodType.ITEM, RecipeProductData.NEED_ITEM[i].ItemNo, RecipeProductData.NEED_ITEM[i].Amount));
                }
          
                if(User.Instance.CheckInventoryGetItem(itemList))
                {
                    SetInventoryFullAlert();
                    return;
                }
            }

            if (isClicked)
                return;

            isClicked = true;
            WWWForm sendData = new WWWForm();
            sendData.AddField("tag", buildingTag);
            sendData.AddField("slot" , frameIndex);

            NetworkManager.SendWithCAPTCHA("produce/harvest", sendData, (jsonObj) => {
                if (SBFunc.IsJTokenType(jsonObj["rs"], JTokenType.Integer) && (eApiResCode)(jsonObj["rs"].Value<int>()) == eApiResCode.INVENTORY_FULL)
                {
                    SetInventoryFullAlert();
                    return;
                }

                int checkAmount = -1;
                if (SBFunc.IsJTokenCheck(jsonObj["rewards"]))
                {
                    var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));
                    InventoryIncomeEvent.Send(rewardList, buildingTag);

                    checkAmount = prodAmount;
                }

                // 콜백 처리
                onClickCallBack?.Invoke(gameObject, checkAmount);

                isClicked = false;
            });
        }
        void SetInventoryFullAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () =>
                {
                    PopupManager.OpenPopup<InventoryPopup>();                
                },
                () => {   //나가기
                    
                },
                () => {  //나가기
                    
                }
            );
        }

		private void OnDestroy()
		{
			if (tween != null) 
			{
				tween.Kill();
				tween = null;
			}
		}
	}
}
