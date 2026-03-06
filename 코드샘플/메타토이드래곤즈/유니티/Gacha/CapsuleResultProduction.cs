using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct CapsuleUIDataSet
    {
        public ProductionCapsule capsuleUI;
        public GachaResult resultData;
        public Vector2 originPos;
        public Vector3 originscale;
    }

    //캡슐 포지셔닝 스파인
    //캡슐 스파인
    //전부 열렸는지 상태
    public class CapsuleResultProduction : MonoBehaviour, EventListener<GachaEvent>
    {
        [SerializeField] List<ProductionCapsule> capsuleList = new List<ProductionCapsule>();
        [SerializeField] ProductionCapsule capsuleAnimation = null;
        [SerializeField] Button singleCapsuleTouchButton = null;
        [SerializeField] Animator capsulePositionAnimation = null;

        [SerializeField] GameObject heavyGachaPanel = null;
        [SerializeField] Button heavyExitBtn = null;
        [SerializeField] List<ProductionCapsule> heavycapsuleList = new List<ProductionCapsule>();
        [SerializeField] ScrollRect heavyGachaScroll = null;

        [SerializeField] Button skipButton = null;//스킵 버튼을 누르면 , 모든 드래곤이 바로 생성됨.

        [SerializeField] GameObject thumbnailParent = null;
        [SerializeField] GameObject dragonThumbnailPrefab = null;
        [SerializeField] GameObject petThumbnailPrefab = null;

        private Sequence clickCapsuleSequence = null;//캡슐 클릭 했을 때, 화면 중앙 이동 및 오브젝트 스케일 조절 시퀀스
        private Sequence startSequence = null;//캡슐 클릭 했을 때, 화면 중앙 이동 및 오브젝트 스케일 조절 시퀀스
        private List<GachaResult> resultDataList = new List<GachaResult>();
        private Action gachaCallback = null;
        private List<CapsuleUIDataSet> skipDataSetList = new List<CapsuleUIDataSet>();//스킵 했을 때, 바로 생성을 위한 단순 참조용 구조 리스트

        bool isOpenProduction = false;
        bool isOnce = false;
        bool isTenTimeAnimDone = false;
        eGachaType currentGachaType = eGachaType.NONE;
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        private void OnDestroy()
        {
            if (startSequence != null)
            {
                startSequence.Kill();
                startSequence = null;
            }
            if (clickCapsuleSequence != null)
            {
                clickCapsuleSequence.Kill();
                clickCapsuleSequence = null;
            }
        }

        public void OnEvent(GachaEvent eventType)
        {
            switch (eventType.Event)
            {
                case GachaEvent.GachaEventEnum.CheckCapsuleOpenCount:
                    CheckAllOpened(eventType.isSingleGacha);
                    break;
                case GachaEvent.GachaEventEnum.CapsuleOpenAnimationComplete:
                    CompleteAnimCheck();
                    break;
                case GachaEvent.GachaEventEnum.CapsuleIndexOpen:
                    SetCapsuleDataAfterAnimation(eventType.capsuleUIIndex,eventType.capsuleDataIndex);
                    break;
                case GachaEvent.GachaEventEnum.CapsuleAnimationShowEffect:
                    ShowOpenEffect(eventType.capsuleUIIndex);
                    break;
            }
        }

        public void InitProduction()
        {
            if (capsuleAnimation != null)
            {
                capsuleAnimation.Init();
                capsuleAnimation.SetVisible(false);
            }

            if (singleCapsuleTouchButton != null)
                singleCapsuleTouchButton.gameObject.SetActive(false);

            if (resultDataList == null)
                resultDataList = new List<GachaResult>();
            resultDataList.Clear();

            if(skipButton != null)
                skipButton.gameObject.SetActive(false);

            if (skipDataSetList == null)
                skipDataSetList = new List<CapsuleUIDataSet>();
            skipDataSetList.Clear();

            foreach (var capsule in capsuleList)
            {
                if (capsule != null)
                {
                    capsule.Init();
                    capsule.SetVisible(false);
                }
            }

            isOpenProduction = false;
            isOnce = false;
        }

        public void PlayFrontHitCapsuleAnimation(int _grade)
        {
            if (capsuleAnimation != null)
                capsuleAnimation.PlayFrontHitCapsuleAnimation(_grade);
        }

        public void SetCapsulePositioningSpine(List<GachaResult> results, eGachaType _type, Action _gachaCompleteCallback)//결과 갯수에 따라서 미리 데이터 세팅(펫, 드래곤 스파인)하고, 클릭 시 해당 캡슐 스파인 연출 제어, 다되면 끝처리
        {
            currentGachaType = _type;
            gachaCallback = _gachaCompleteCallback;
            resultDataList = results.ToList();

            gameObject.SetActive(true);
            heavyGachaPanel.SetActive(false);
            capsulePositionAnimation.gameObject.SetActive(false);

            //SetThumbnailData();//썸네일 UI 미리 세팅

            var firstCapsuleGrade = ((GachaResultDragonAndPet)results[0]).GRADE;
            var hitAnimLength = capsuleAnimation.GetSpineAnimationLength(capsuleAnimation.GetHitStateByGrade(firstCapsuleGrade));
            var isSingleGacha = results.Count == 1;
            if(!isSingleGacha)
                hitAnimLength = 0.0f;//단차가 아닌, 가챠(2뽑이상~)일 때는 단일 hit 를 출력하지 말자고함.

            if (startSequence != null)
            {
                startSequence.Kill();
                startSequence = null;
            }

            startSequence = DOTween.Sequence();
            startSequence.AppendCallback(() =>
            {
                if(isSingleGacha)//단일 가챠일 때만 단일 캡슐 hit 출력
                {
                    //capsuleAnimation.SetSpinePosition(isSingleGacha ? new Vector2(0, 628) : new Vector2(0, 428));//단차 일 때는 캡슐 애니 pos 변경
                    capsuleAnimation.SetVisible(true);
                    capsuleAnimation.SetSpineVisible(true, () => {
                        PlayFrontHitCapsuleAnimation(firstCapsuleGrade);//가장 앞단의 캡슐 hit anim 추가

                        //SoundManager.Instance.PlaySFX("sfx_gacha_open");//hit sound 추가

                    });

                    DOTween.Sequence().AppendInterval(0.15f).AppendCallback(() => { CompleteAnimCheck(); }).Play();//단차 일 때, skip 버튼 활성화
                }
                else
                    capsuleAnimation.SetVisible(false);//다중일 때는 단일 캡슐 hit 스파인 꺼버림.
            }).AppendInterval(hitAnimLength).//hit 시간 대기
            AppendCallback(() => {
                foreach (var capsule in capsuleList)
                {
                    if (capsule != null)
                    {
                        capsule.Init();
                        capsule.SetVisible(true);
                    }
                }

                if (isSingleGacha)//단차 일 때
                {
                    var resultData = (GachaResultDragonAndPet)results[0];
                    var resultGrade = resultData.GRADE;
                    var isNormalGrade = resultGrade <= 2;//노말, 언커먼은 자동 오픈

                    if (isNormalGrade)//노말 일 때 - 드래곤 미리 생성 - 캡슐 포지셔닝 애니메이션이 끝나면 해줘야함.
                        capsuleAnimation.AutoOpenNormalGrade(0,resultData, _type, true);
                    else
                    {
                        singleCapsuleTouchButton.gameObject.SetActive(true);
                        singleCapsuleTouchButton.onClick.AddListener(delegate { OnClickGachaSingleCapsule(resultData, _type); });
                        capsuleAnimation.SetIdleAnimation(resultGrade, true);
                    }
                }
                else
                {
                    SoundManager.Instance.PlaySFX("sfx_gacha_open2");//캡슐 낙하 사운드 추가
                    isTenTimeAnimDone = false;//10뽑 애니메이션 초기화
                    if (results.Count <= 10)
                    {                        
                        capsulePositionAnimation.gameObject.SetActive(true);
                        capsulePositionAnimation.SetInteger("showCount", 0);//idle animation 출력
                        capsuleAnimation.SetVisible(false);//단차 일 때만 capsule animation 사용
                        capsulePositionAnimation.SetInteger("showCount", results.Count);//idle animation 출력
                    }
                    else
                    {
                        heavyGachaPanel.SetActive(true);
                        heavyExitBtn.onClick.RemoveAllListeners();
                        GachaProductionController comp = FindObjectOfType<GachaProductionController>();
                        heavyExitBtn.onClick.AddListener(comp.OnClickDimmed);

                        int index = 0;
                        int grade = 0;
                        foreach(var capsule in heavycapsuleList)
                        {
                            bool soundskip = true;
                            if (results[index] != null)
                            {
                                int g = ((GachaResultSpine)results[index]).GRADE;
                                if (grade != g)
                                {
                                    grade = g;
                                    soundskip = false;
                                }
                            }
                            capsule.AutoOpenNormalGrade(0.01f * index, results[index], currentGachaType, false, soundskip);
                            capsule.SendGachaSystemMessage(results[index], _type == eGachaType.PET);

                            index++;
                        }
                        heavyGachaScroll.verticalNormalizedPosition = 1.0f;
                    }

                }
            });
            startSequence.Play();
        }
        /// <summary>
        /// 기존에는 SetCapsulePositioningSpine 이곳에서 유니티 animation과 동시에 세팅했는 데, animation으로 켜지는 부분이랑, 캡슐 spine 세팅 (start 타는 부분)부분이랑 안맞아서 
        /// unity Animation 안에 event를 넣어서 켜지고 난후에 세팅 하는 방식으로 변경
        /// </summary>
        /// <param name="_capsuleUIIndex"></param>몇 번째 spine이 들어간 UI 캡슐을 제어할지
        /// <param name="_capsuleDataIndex"></param>결과 데이터 리스트에서의 인덱스
        void SetCapsuleDataAfterAnimation(int _capsuleUIIndex, int _capsuleDataIndex)//유니티 애니메이션으로 켜진 이후에 애니메이션 세팅해주기
        {
            var uiIndex = _capsuleUIIndex - 1;
            var originData = resultDataList[_capsuleDataIndex - 1];
            var capsuleData = capsuleList[uiIndex];

            var resultData = (GachaResultDragonAndPet)originData;
            if (resultData == null)
                return;

            var resultGrade = resultData.GRADE;
            var isNormalGrade = resultGrade <= (int)eDragonGrade.Uncommon;//노말, 언커먼은 자동 오픈

            var buttonComp = capsuleData.GetComponentInChildren<Button>();//button event
            if (buttonComp == null)
                return;

            buttonComp.onClick.RemoveAllListeners();

            //capsule 객체의 visible이 켜질 때, ProductionCapsuleSpine -> start() 보다 일찍 타서 재생 안되는 현상 막기
            if (isNormalGrade)//노말 일 때 - 드래곤 미리 생성 - 캡슐 포지셔닝 애니메이션이 끝나면 해줘야함.
                capsuleData.AutoOpenNormalGrade(0.75f, originData, currentGachaType, false);//- 생성 타이밍 제어는 유니티 애니메이션으로 하기로
            else
            {
                buttonComp.onClick.AddListener(delegate { OnClickGachaCapsule(uiIndex, originData, currentGachaType); });
                capsuleData.SetIdleAnimation(resultGrade);
            }

            CapsuleUIDataSet tempDataSet = new CapsuleUIDataSet();
            tempDataSet.capsuleUI = capsuleData;
            tempDataSet.resultData = originData;
            var currentTransform = capsuleData.GetComponent<RectTransform>();
            tempDataSet.originPos = new Vector2(currentTransform.anchoredPosition.x, currentTransform.anchoredPosition.y);
            tempDataSet.originscale = new Vector3(currentTransform.localScale.x, currentTransform.localScale.y, currentTransform.localScale.z);
            skipDataSetList.Add(tempDataSet);
        }

        public void CompleteAnimCheck()
        {
            isTenTimeAnimDone = true;

            if (skipButton != null)
                skipButton.gameObject.SetActive(true);
        }

        void CheckAllOpened(bool _isSingle)//전부 열렸을 때를 체크 - 하위 캡슐 각각을 관리 하는 클래스 생성하기
        {
            bool isAllClicked;
            if(_isSingle)
            {
                isAllClicked = capsuleAnimation.IsProductionComplete;
            }
            else
                isAllClicked = IsAllClickedTenTimeGacha();

            if (isAllClicked)
            {
                if (skipButton != null)
                    skipButton.gameObject.SetActive(false);

                if (gachaCallback != null && !isOnce)
                {
                    isOnce = true;
                    gachaCallback();
                    //ShowThumbnail();
                }
            }
        }

        bool IsAllClickedTenTimeGacha()
        {
            if (resultDataList == null || resultDataList.Count <= 0)
                return false;

            int openTotalCount = 0;
            foreach (var capsule in capsuleList)
            {
                if (capsule == null)
                    continue;
                if (capsule.IsProductionComplete)
                    openTotalCount++;
            }
            return resultDataList.Count == openTotalCount;
        }

        public void OnClickGachaCapsule(int _index, GachaResult _data, eGachaType _type)//캡슐 클릭 시 - 드래곤 생성 및 연출
        {
            if (!isTenTimeAnimDone)//10뽑 애니메이션이 전부 끝나면
                return;

            if (isOpenProduction)//다른 캡슐의 open 연출이 시작되면
                return;

            if (capsuleList.Count <= _index)
                return;

            if (capsuleList[_index].IsCapsuleOpening)
                return;

            isOpenProduction = true;
            //화면 중앙 이동 pos 및 scale, 이동, 다른 캡슐 opacity 0 만들기 연출 시퀀스 추가
            if (clickCapsuleSequence != null)
                clickCapsuleSequence.Kill();

            clickCapsuleSequence = DOTween.Sequence();
            clickCapsuleSequence.AppendCallback(() =>
            {
                SetProductionOpacityCapsuleList(_index, 0.2f);
                capsuleList[_index].gameObject.GetComponent<RectTransform>().DOAnchorPos(new Vector3(0, 520f, 0), 0.5f);
                capsuleList[_index].gameObject.GetComponent<RectTransform>().DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.5f);
            }).AppendInterval(0.5f).AppendCallback(() =>
            {
                capsuleList[_index].OpenGachaCapsuleProcess(_data, _type, false, () =>
                {
                    DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() => {
                        isOpenProduction = false;
                        SetALLCapsuleOpacityCapsuleList();
                    });
                });//캡슐 객체에 연출 스파인 생성 요청//오픈이 다 끝나면 초기 상태 되돌림.
            });
            clickCapsuleSequence.Play();
        }

        void SetProductionOpacityCapsuleList(int _index, float _exceptDelay)//해당 인덱스를 제외하고 모든 capsuleList 의 opaicity 0 처리
        {
            var capsuleListCount = capsuleList.Count;
            for (int i = 0; i < capsuleListCount; i++)
            {
                var capsuleData = capsuleList[i];
                if (capsuleData == null)
                    continue;

                var isCorrectIndex = _index == capsuleData.CapsuleIndex - 1;
                if (isCorrectIndex)
                    continue;

                capsuleData.gameObject.GetComponent<RectTransform>().DOScale(Vector3.zero, _exceptDelay);
            }
        }
        void SetALLCapsuleOpacityCapsuleList()
        {
            var capsuleListCount = capsuleList.Count;
            for (int i = 0; i < capsuleListCount; i++)
            {
                var capsuleData = capsuleList[i];
                if (capsuleData == null)
                    continue;

                var uiIndex = capsuleData.CapsuleIndex;
                var modifyIndex = uiIndex - 1;

                capsuleData.gameObject.GetComponent<RectTransform>().anchoredPosition = GetCapsuleOriginPos(modifyIndex);
                capsuleData.gameObject.GetComponent<RectTransform>().DOScale(GetCapsuleOriginScale(modifyIndex), 0.2f);
            }
        }

        Vector2 GetCapsuleOriginPos(int _index)
        {
            Vector2 returnData = Vector2.zero;
            foreach (var dataSet in skipDataSetList)
            {
                var capsuleUI = dataSet.capsuleUI;
                if (capsuleUI == null)
                    continue;

                if (capsuleUI.CapsuleIndex - 1 == _index)
                    return dataSet.originPos;
            }
            return returnData;
        }

        Vector3 GetCapsuleOriginScale(int _index)
        {
            Vector3 returnData = Vector3.one;
            foreach (var dataSet in skipDataSetList)
            {
                var capsuleUI = dataSet.capsuleUI;
                if (capsuleUI == null)
                    continue;

                if (capsuleUI.CapsuleIndex - 1 == _index)
                    return dataSet.originscale;
            }
            return returnData;
        }

        int GetGradeByUIIndex(int _uiIndex)
        {
            int grade = 1;
            foreach (var dataSet in skipDataSetList)
            {
                var capsuleUI = dataSet.capsuleUI;
                if (capsuleUI == null)
                    continue;

                if (capsuleUI.CapsuleIndex == _uiIndex)
                    return ((GachaResultSpine)dataSet.resultData).GRADE;
            }
            return grade;
        }

        public void OnClickGachaSingleCapsule(GachaResult _data, eGachaType _type)//캡슐 하나일 때
        {
            if (capsuleAnimation.IsCapsuleOpening)
                return;

            capsuleAnimation.OpenGachaCapsuleProcess(_data, _type, true);//캡슐 객체에 연출 스파인 생성 요청
        }
        void ShowOpenEffect(int _uiIndex)
        {
            if (_uiIndex == 0)
                capsuleAnimation.ShowOpenEffect(((GachaResultSpine)resultDataList[0]).GRADE);
            else
            {
                if(capsuleList.Count > _uiIndex - 1)
                    capsuleList[_uiIndex - 1].ShowOpenEffect(GetGradeByUIIndex(_uiIndex));
            }
        }

        public void OnClickSkip()
        {
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);

            if (clickCapsuleSequence != null)
                clickCapsuleSequence.Kill();
            isOpenProduction = true;

            var isSingle = resultDataList.Count == 1;
            if (isSingle)
                capsuleAnimation.SkipProcess(resultDataList[0], currentGachaType, true);
            else
            {
                SetALLCapsuleOpacityCapsuleList();

                foreach (var dataSet in skipDataSetList)
                {
                    var resultData = dataSet.resultData;
                    var capsuleUI = dataSet.capsuleUI;
                    capsuleUI.SkipProcess(resultData, currentGachaType,false);
                }

                //ShowThumbnail(true);
            }
        }

        void SetThumbnailData()
        {
            SBFunc.RemoveAllChildrens(thumbnailParent.transform);

            thumbnailParent.gameObject.SetActive(false);

            if (resultDataList == null || resultDataList.Count <= 0)
                return;

            thumbnailParent.GetComponent<RectTransform>().localScale = currentGachaType == eGachaType.DRAGON ? new Vector3(0.6f, 0.6f, 0.6f) : new Vector3(1.5f, 1.5f, 1.5f);

            GameObject targetPrefab;
            foreach(var resultData in resultDataList)
            {
                if (resultData == null)
                    continue;

                targetPrefab = currentGachaType == eGachaType.DRAGON ? dragonThumbnailPrefab : petThumbnailPrefab;
                var clone = Instantiate(targetPrefab, thumbnailParent.transform);

                if(currentGachaType == eGachaType.DRAGON)
                {
                    var dragonFrame = clone.GetComponent<DragonPortraitFrame>();
                    dragonFrame.SetCustomPotraitFrame(resultData.ID , 1);
                    dragonFrame.setCallback((param)=> {
                        ClickDragonPortrait(int.Parse(param));
                    });
                }
                else if(currentGachaType == eGachaType.PET)
                {
                    var petFrame = clone.GetComponent<PetPortraitFrame>();
                    petFrame.SetCustomPotraitFrame(resultData.ID, 1);
                }
            }
        }

        void ClickDragonPortrait(int _dragonTag)
        {
            var popup = DragonManagePopup.OpenPopup(0, 1);
            popup.SetExitCallback(() => {
                
            });
            popup.CurDragonTag = _dragonTag;
            popup.ClearDragonInfoList();

            foreach (var resultData in resultDataList)
            {
                if (resultData == null)
                    continue;
                popup.DragonInfoList.Add(resultData.ID);
            }
            popup.ForceUpdate();
        }

        void ShowThumbnail(bool _isSkipped = false)
        {
            if (_isSkipped)
            {
                capsulePositionAnimation.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -430f, 0);
                thumbnailParent.gameObject.SetActive(true);
                gachaCallback();
            }
            else
            {
                DOTween.Sequence().AppendCallback(() =>
                {
                    capsulePositionAnimation.gameObject.GetComponent<RectTransform>().DOAnchorPosY(-430, 0.5f);
                }).AppendInterval(0.5f).AppendCallback(() => {
                    thumbnailParent.gameObject.SetActive(true);
                    gachaCallback();
                });
            }
        }
    }
}
