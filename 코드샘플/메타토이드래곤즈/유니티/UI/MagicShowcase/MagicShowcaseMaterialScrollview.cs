using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 현재 보유 재료 스크롤 & 업그레이드 재료(투입) 동시 관리
    /// </summary>
    /// 투입하기 UI & 제작하기 UI 에서 api response 들어오면 이쪽만 갱신하면 되니까.
    public class MagicShowcaseMaterialScrollview : MagicShowcaseComponent
    {
        [SerializeField] List<ItemFrame> userMaterialList = new List<ItemFrame>();//현재 보유재료 리스트
        [SerializeField] List<MagicShowcaseMaterialInputSlot> upgradeMaterialList = new List<MagicShowcaseMaterialInputSlot>();//업글재료 리스트
        [SerializeField] Button upgradeButton = null;
        [SerializeField] List<ItemFrame> movingMaterialList = new List<ItemFrame>();

        private List<ItemFrame>[] itemClonesList;
        
        [SerializeField] Transform targetTr = null;
        [SerializeField] Transform itemEffectParentTr = null;
        [SerializeField] ParticleSystem InputParticle =null;

        private Asset[] requireItemInfo = new Asset[3];

        private Coroutine CurEffectCor = null;
        public override void InitUI(int _tabIndex)
        {
            SetEffectOff();
            base.InitUI(_tabIndex);
            SetUserItemSlot();
            SetUpgradeMaterialSlot();
            RefreshUpGradeButton();
            
        }

        private void OnDisable()
        {
            InputParticle.gameObject.SetActive(false);
            InputParticle.Stop();
        }

        void SetEffectOff()
        {
            if(CurEffectCor != null)
                StopCoroutine(CurEffectCor);
            CurEffectCor = null;
            requireItemInfo[0] = null;
            requireItemInfo[1] = null;
            requireItemInfo[2] = null;
            if (itemClonesList == null)
                itemClonesList = new List<ItemFrame>[3] { new List<ItemFrame>(), new List<ItemFrame>(), new List<ItemFrame>() };
            SBFunc.RemoveAllChildrens(itemEffectParentTr);
            foreach (var items in itemClonesList)
            {
                foreach (var item in items)
                {
                    if(item != null)
                    {
                        item.transform.DOKill();
                        item.transform.Find("body").DOKill();
                        Destroy(item);
                    }
                    
                }
                items.Clear();
            }
            
            foreach (var item in movingMaterialList)
            {
                item.gameObject.SetActive(false);
            }
        }
        
        void SetUserItemSlot()//유저의 아이템 보유 상태
        {
            if (infoData == null)
                return;

            var itemList = infoData.GetStockList();
            if(itemList.Count == userMaterialList.Count)
            {
                for(int i = 0; i < userMaterialList.Count; i++)
                {
                    var requestItem = itemList[i];
                    if (requestItem == null)
                        continue;

                    var itemNo = requestItem.ItemNo;
                    var currentItemCount = User.Instance.GetItemCount(itemNo);
                    userMaterialList[i].SetFrameItemInfo(itemNo, currentItemCount, -1);
                }
            }
        }

        void SetUpgradeMaterialSlot()//업그레이드 재료 리스트 상태
        {
            if (infoData == null)
                return;

            var currentLevel = infoData.LEVEL;
            var currentTypeMaxLevel = MagicShowcaseData.GetMaxLevelByGroup(type);

            var upperLevel = currentLevel + 1;
            if (upperLevel <= currentTypeMaxLevel)
            {
                var nextTableData = MagicShowcaseData.GetDataByGroupAndLevel(type, upperLevel);
                if(nextTableData != null)
                {
                    var itemList = nextTableData.MATERIAL_LIST;//업글 요구 아이템 리스트

                    var currentItemList = infoData.GetStockList();//현재 투입상태

                    //현재 투입상태 (arraySize = 3)을 기준으로 업글 요구 아이템 리스트가 있는지를 세팅

                    for(int i = 0; i< currentItemList.Count; i++)
                    {
                        var upgradeObj = upgradeMaterialList[i];
                        upgradeObj.gameObject.SetActive(true);

                        var currentTargetItem = currentItemList[i];

                        var upgradeItem = itemList.Find(element => element.ItemNo == currentTargetItem.ItemNo);
                        if(upgradeItem != null && upgradeItem.ItemNo >= 0)
                        {
                            var itemData = upgradeItem;
                            var itemId = itemData.ItemNo;
                            var maxCount = itemData.Amount;
                            var curCount = infoData.GetInputAmount(itemId);
                            requireItemInfo[i] = itemData;
                            upgradeObj.InitUI(itemId, curCount, maxCount);
                            movingMaterialList[i].SetFrameItem(itemData);
                            movingMaterialList[i].SetFrameFuncNone();
                        }
                        else
                            upgradeObj.SetDimmed(currentTargetItem.ItemNo);
                    }
                }
            }
            else//max level
            {
                var stackList = infoData.GetStockList();//재료 투입 리스트 - 아이템 세팅용
                for(int i = 0; i< stackList.Count; i++)
                {
                    var item = stackList[i];
                    if (item == null)
                        continue;

                    var upgradeObj = upgradeMaterialList[i];
                    upgradeObj.gameObject.SetActive(true);
                    upgradeObj.InitUI(item.ItemNo, 0, 0);
                }
            }
        }

        public void OnClickRequestOpenProductPopupUI(int _btnIndex)//제작UI 팝업 요청 - 제작 데이터 없으면 (뽑기 - 마일상점으로 이동시키기)
        {
            if (_btnIndex < 0 || _btnIndex >= userMaterialList.Count)
                return;

            var itemFrame = userMaterialList[_btnIndex];
            var itemNo = itemFrame.GetItemID();
            if(itemNo > 0)
            {
                var isRecipeItem = RecipeBaseData.GetRecipeData(itemNo);
                if(isRecipeItem != null)//제작 팝업 오픈
                    MagicShowcaseBlockConstructPopup.OpenPopup(isRecipeItem);
                else//뽑기 - 마일 상점 이동
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("제작불가뽑기이동"),
                            () => {
                                SBFunc.MoveGachaScene(eGachaGroupMenu.LUCKYBOX);
                            },
                            null,
                            () => { }
                        , true, true, true);
                }
            }
        }

        public void OnClickRequestOpenInputPopupUI(int _btnIndex)//투입UI 팝업 요청
        {
            if (upgradeMaterialList == null || upgradeMaterialList.Count <= 0)
                return;

            if (_btnIndex < 0 || _btnIndex >= upgradeMaterialList.Count)
                return;

            var isMaxLevel = infoData.LEVEL >= MagicShowcaseData.GetMaxLevelByGroup(type);
            if (isMaxLevel)
            {
                ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_01"));
                return;
            }

            var upgradeInputMaterial = upgradeMaterialList[_btnIndex];
            var isStockItem = upgradeInputMaterial.IsCheckStock;//현재 단계에서 넣는게 맞는 아이템인가?
            if (!isStockItem)
                return;

            var itemNo = upgradeInputMaterial.ItemNo;
            var currentUserAmount = User.Instance.GetItemCount(itemNo);//유저의 보유량
            var currentStockAmount = infoData.GetInputAmount(itemNo);//유저의 투입량

            var currentTableData = infoData.TABLE_DATA;//현재 레벨업을 위한 테이블 데이터
            var currentUpgradeAssetList = currentTableData.MATERIAL_LIST;//업그레이드 재화 리스트
            var currentTargetItem = currentUpgradeAssetList.Find(element => element.ItemNo == itemNo);

            if (currentTargetItem != null && currentTargetItem.Amount <= currentStockAmount)
            {
                ToastManager.On(StringData.GetStringByStrKey("재료최대투입"));
                return;
            }

            MagicShowcaseBlockInputPopup.OpenPopup((int)type,itemNo,currentUserAmount,currentStockAmount,currentTableData,()=> {
                //서버 요청 성공 시 이후 액션
                InitUI(index);

                UIManager.Instance.MainPopupUI.RequestUpdateMagicShowcaseReddot();
            });
        }

        public bool IsUpGradeCondition(bool _isShowToastMsg = false)//투입량이 최대인지 체크
        {
            if (upgradeMaterialList == null || upgradeMaterialList.Count <= 0)
                return false;

            var isMaxLevel = MagicShowcaseData.GetMaxLevelByGroup(type) == infoData.LEVEL;
            if (isMaxLevel)
            {
                if (_isShowToastMsg)
                    ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_01"));

                return false;
            }

            var checkIndex = 0;
            var currentTableData = infoData.TABLE_DATA.MATERIAL_LIST;//현재 레벨업을 위한 테이블 데이터
            foreach(var data in currentTableData)
            {
                var isContainItem = upgradeMaterialList.Find(element => element.ItemNo == data.ItemNo);
                if (isContainItem != null && isContainItem.IsCheckStock && isContainItem.IsFullStocked())
                    checkIndex++;
            }

            var isUpgradeCondition = checkIndex == currentTableData.Count;

            if (_isShowToastMsg && !isUpgradeCondition)
                ToastManager.On(StringData.GetStringByStrKey("투입량확인"));

            return isUpgradeCondition;
        }

        public void RefreshUpGradeButton()
        {
            if (upgradeButton != null)
                upgradeButton.SetButtonSpriteState(IsUpGradeCondition());
        }

        public void OnClickUpgrade()
        {
            if (infoData == null)
                return;

            if (!IsUpGradeCondition(true))
                return;

            WWWForm data = new WWWForm();
            data.AddField("menutype", (int)type);

            NetworkManager.Send("magicshowcase/showcaselevelup", data, (jsonData) =>
            {
                JToken resultResponse = jsonData["rs"];
                if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                {
                    int rs = resultResponse.Value<int>();
                    switch ((eApiResCode)rs)
                    {
                        case eApiResCode.OK:

                            //ToastManager.On("업그레이드 완료 - 연출이 필요합니다.");
                            CurEffectCor = StartCoroutine(MoveEffectShow());

                            MagicShowcasePopup popup = PopupManager.GetPopup<MagicShowcasePopup>();
                            if(popup != null)
                            {
                                popup.OnLevelUp();
                            }
                            break;
                    }
                }
            });
        }

        IEnumerator MoveEffectShow()
        {
            InputParticle.Stop();
            int[] repeatCntArr = new int[3] {0,0,0};
            int maxRepeatCnt = 0;
            int curRepeatCnt = 0;
            float nextDelayTime = 0.05f;
            float scaleChangeTime = 0.2f; // 크기 변환 시간 전체 값
            float movingTime = 0.3f; // 이동 시간 전체 값
            for (int i = 0; i < 3; ++i)
            {
                if (requireItemInfo[i] != null)
                {
                    int repeatCnt = 0;
                    if (requireItemInfo[i].Amount < 200)
                    {
                        repeatCnt = 1;
                    }
                    else if (requireItemInfo[i].Amount < 1000)
                    {
                        repeatCnt = 1 + requireItemInfo[i].Amount / 200;
                    }
                    else
                    {
                        repeatCnt = 5 + requireItemInfo[i].Amount / 2000;
                    }
                    repeatCntArr[i] = repeatCnt;
                    maxRepeatCnt += repeatCnt;
                }
            }

            for (int i=0; i<3; ++i)
            {
                if (repeatCntArr[i] > 0)
                {
                    upgradeMaterialList[i].DoMoveSliderDown(scaleChangeTime +movingTime);
                    for (int j = 0; j < repeatCntArr[i]; ++j)
                    {
                        var itemClone = movingMaterialList[i];
                        var itemObj = Instantiate(itemClone, itemEffectParentTr);
                        itemObj.transform.position = movingMaterialList[i].transform.position + Vector3.one * ( 0.5f* SBFunc.RandomValue);
                        itemObj.gameObject.SetActive(true);
                        itemObj.SetInventoryAnimationItem(requireItemInfo[i].ItemNo);
                        var itemIcon = itemObj.transform.Find("body");
                        itemIcon.localScale = Vector3.one * 0.8f;
                        itemIcon.transform.DOScale(Vector3.one * 1.7f, scaleChangeTime).SetDelay(curRepeatCnt* nextDelayTime).OnComplete(() =>
                        {
                            itemIcon.transform.DOScale(Vector3.one * 0.8f, movingTime).SetEase(Ease.InExpo).OnComplete(() => SoundManager.Instance.PlaySFX("EF_SHOW_CASE_ITEM_INPUT"));
                            itemObj.transform.DOMove(targetTr.position, movingTime).SetEase(Ease.InQuad);
                            
                        });
                        
                        ++curRepeatCnt;
                        itemClonesList[i].Add(itemObj);
                    }
                   
                }
            }
            yield return new WaitForSeconds(scaleChangeTime + movingTime + nextDelayTime*maxRepeatCnt);
            //yield return new WaitForSeconds( Mathf.Min(maxTime, (scaleChangeTime+movingTime) * maxRepeatCnt));
            SBFunc.RemoveAllChildrens(itemEffectParentTr);
            InputParticle.Stop();
            InputParticle.gameObject.SetActive(true);
            InputParticle.Play();
            SoundManager.Instance.PlaySFX("EF_SHOW_CASE_ITEM_PUTIN");
            yield return new WaitForSeconds(0.3f);
            var popup = PopupManager.GetPopup<MagicShowcasePopup>();
            if (popup != null)
                popup.ForceUpdate(new TabTypePopupData(index, -1));//일단 임시
            UIManager.Instance.MainPopupUI.RequestUpdateMagicShowcaseReddot();
        }

        
    }
}