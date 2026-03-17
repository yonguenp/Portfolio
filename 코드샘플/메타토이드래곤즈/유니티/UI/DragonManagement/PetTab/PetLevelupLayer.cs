using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetLevelupLayer : SubLayer
    {
        const int CONSTRAINT_PET_LEVEL = 30;
        const string COMPOUND_CONSTRAINT_GOAL_TIME = "pet_compound_time";//오늘 하루 안보기를 누르면 켜야될 시간을 미리 계산해서 넘김

        [SerializeField]
        PetListPanel petLevelupSubList = null;

        [SerializeField]
        PetPortraitFrame petFrame = null;
        [SerializeField]
        GameObject[] petRankImgs = null;

        [Space(10)]
        [Header("progress info")]
        [SerializeField]
        Slider currentLevelBar = null;
        [SerializeField]
        Text currentExpLabel = null;
        [SerializeField]
        Slider levelUpBar = null;
        [SerializeField]
        Text currentLevelLabel = null;
        [SerializeField]
        Text nextLevelLabel = null;
        [SerializeField]
        GameObject tempTweenNode = null;

        [SerializeField]
        Button levelUpBtn = null;
        

        //[Space(10)]
        //[Header("over EXP info")]
        //[SerializeField]
        //GameObject overExpNode = null;
        //[SerializeField]
        //GameObject overExpPopupNode = null;
        //[SerializeField]
        //Text overExpLabel = null;

        [Space(10)]
        [Header("Pet Info")]
        [SerializeField]
        Text petNameText = null;
        [SerializeField]
        Text[] statTexts =  null;

        [SerializeField]
        List<GameObject> textChildNodeList = new List<GameObject>();

        [SerializeField]
        Text[] currentStatTexts = null;
        [SerializeField]
        Text[] nextStatTexts = null;

        [Space(10)]
        [Header("back Button")]
        [SerializeField]
        Button backBtn = null;

        bool isInitTable = false;

        int currentPetLevel = 0;//현재 선택된 펫 레벨
        int currentPetExp = 0;//현재 선택된 펫 경험치의 잔여 경험치 (총경험치 아님!)
        int currentPetTotalExp = 0;
        int currentPetGrade = 0;//현재 선택된 펫의 등급
        int currentPetReinForce = 0;
        int currentPetTag = 0;
        int petMaxLevel = 0;//펫 만렙 정의
        int petMaxLevelTotalExp = 0;//만렙 펫의 총 경험치

        bool tempMaxLevelCheckFlag = false;//펫 레벨 최대치 체크
        int tempPredictableLevel = 0;//다음 예상 레벨 임시 변수 - 레벨 라벨 표시용도
        int tempLevelCheck = -1;//레벨업 연출 할때 다음예상 레벨 몇인지 임시 변수

        List<int> materialPetTagList = new List<int>();//레벨업 요청할 재료 팻 태그 리스트

        bool viewDirty = true;
        List<UserPet> viewPets = null;//재료 등록 리스트 기반으로 펫 리스트 세팅

        Sequence tweenProgressAnim = null;
        public delegate void func();

        public override void Init()
        {
            if (tweenProgressAnim != null)
            {
                tweenProgressAnim.Kill();
            }
            tempMaxLevelCheckFlag = false;
            tempLevelCheck = -1;
            SetPetSubList();
            RefreshPetInfo();
            RefreshData();
        }

        public override void ForceUpdate()
        {
            Init();
        }



        void SetPetSubList()
        {
            if (materialPetTagList == null)
            {
                materialPetTagList = new List<int>();
            }
            materialPetTagList.Clear();//펫 재료 리스트 초기화

            if (petLevelupSubList != null)
            {
                petLevelupSubList.ClickRegistCallback = RegistMaterialPetTag;
                petLevelupSubList.ClickReleaseCallback = ReleaseMaterialPetTag;
                petLevelupSubList.ClickAutoRegistCallback = AutoRegistMaterialList;
                petLevelupSubList.ClickReleaseAllCallback = ReleaseAllMaterialPetTag;
                petLevelupSubList.Init(ePetPopupState.LevelUp);//보유 펫리스트 시동
            }
        }

        void RegistMaterialPetTag(string param)//리스트에서 재료 넣을 때
        {
            var tag = int.Parse(param);
            if (tag <= 0)
            {
                return;
            }

            if (tempMaxLevelCheckFlag)//만렙 초과 시
            {
                PushMaterialList(tag);
                PopMaterialProcess(new List<int>(new int[] { tag }));//오른쪽 리스트에 선택된 항목 취소

                Debug.Log("pet exp full");
                ToastManager.On(100000210);
            }
            else
            {
                petLevelupSubList.SetDetailButtonState(true);

                PushMaterialList(tag);

                //리스트에서 클릭한 펫 기준 데이터 가져와서 얼만큼의 경험치를 주는지 연산
                RefreshPetProgressBar();//현재 선택된 노드 기준 경험치량 계산

                //스크롤 다시 그리기
                RefreshData();
            }
        }

        void ReleaseMaterialPetTag(string param)//리스트에서 재료 뺄 때
        {
            PopMaterialList(int.Parse(param));
            //리스트에서 클릭한 빠지는 펫 기준 데이터 가져와서 얼만큼의 경험치를 뺄지 연산
            RefreshPetProgressBar();//현재 선택된 노드 기준 경험치량 계산

            //스크롤 다시 그리기
            RefreshData();
        }

        void ReleaseAllMaterialPetTag()
        {
            materialPetTagList = new List<int>();
            RefreshPetProgressBar();
            RefreshData();
        }

        void AutoRegistMaterialList(List<int> materialList)//자동 등록 프로세스
        {
            if (tempMaxLevelCheckFlag)//맥스 레벨일 때
            {
                PushMaterialListByList(materialList);
                PopMaterialProcess(materialList);//오른쪽 리스트에 선택된 항목 취소

                Debug.Log("pet exp full");
                ToastManager.On(100000210);
            }
            else
            {
                PushMaterialListByList(materialList);//일단 재료 리스트 세팅

                //materialList 기준으로 경험치 초과되기 시작하는 재료 리스트 구해야함
                var expectObtainExp = CalcTotalSelectNodeExp();//획득 가능한 경험치
                var expectMaxLevelCheck = currentPetTotalExp + expectObtainExp > petMaxLevelTotalExp;//경험치 획득 시 만렙뚫나?

                if (expectMaxLevelCheck)
                {
                    var diff = currentPetTotalExp + expectObtainExp - petMaxLevelTotalExp;//초과분
                    List<int> minusList = new List<int>();//빼야될 펫 리스트 계산
                    minusList.Clear();

                    for (var i = materialList.Count - 1; i >= 0; i--)//뒤에서 부터 뺌
                    {
                        var tag = materialList[i];
                        if (tag <= 0)
                        {
                            continue;
                        }

                        var exp = GetPetOfferExp(tag);
                        float rate = ServerOptionData.GetFloat("pet_exp", 1.0f);
                        diff -= (int)(exp * rate);
                        if (diff > 0)
                        {
                            minusList.Add(tag);
                        }
                    }

                    PopMaterialProcess(minusList);//빼야될 리스트 오른쪽 리스트에 선택된 항목 취소
                }

                RefreshPetProgressBar();//현재 선택된 노드 기준 경험치량 계산

                RefreshData();
            }
        }

        void RefreshData()
        {
            SetMaterialByList();
            viewDirty = true;
        }

        void SetMaterialByList()//현재 등록된 펫 재료 리스트 기반으로 userpet 리스트 만들기
        {
            if (materialPetTagList == null)
            {
                materialPetTagList = new List<int>();
            }

            if (viewPets == null)
            {
                viewPets = new List<UserPet>();
            }

            if (materialPetTagList.Count <= 0)
            {
                viewPets.Clear();
                return;
            }

            viewPets.Clear();

            for (var i = 0; i < materialPetTagList.Count; i++)
            {
                var tag = materialPetTagList[i];
                if (tag <= 0)
                {
                    continue;
                }

                var petData = User.Instance.PetData.GetPet(tag);
                if (petData == null)
                {
                    continue;
                }

                viewPets.Add(petData);
            }
        }

        

        void PopMaterialProcess(List<int> selectTagList)//재료쪽에서 강제로 빼게 하는 프로세스(경험치 초과시)
        {
            
            if (selectTagList == null || selectTagList.Count <= 0)
            {
                return;
            }

            if (petLevelupSubList != null)//리스트에서 등록한 펫 해제 프로세스
            {
                for (var i = 0; i < selectTagList.Count; i++)
                {
                    var selectTag = selectTagList[i];
                    if (selectTag <= 0)
                    {
                        continue;
                    }
                    PopMaterialList(selectTag);
                    petLevelupSubList.PopMaterialList(selectTag);
                }

                petLevelupSubList.ViewDirty = true;
                petLevelupSubList.SetTableViewFlag(false);
                petLevelupSubList.ForceUpdate();
                petLevelupSubList.SetTableViewFlag(true);
            }
        }

        void PopMaterialList(int tag)
        {
            var index = materialPetTagList.IndexOf(tag);
            if (index > -1)
            {
                materialPetTagList.RemoveAt(index);
            }
        }

        void PushMaterialList(int tag)
        {
            var index = materialPetTagList.IndexOf(tag);
            if (index < 0)
            {
                materialPetTagList.Add(tag);
            }
        }

        void PushMaterialListByList(List<int> materialList)
        {
            if (materialList == null || materialList.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < materialList.Count; i++)
            {
                var tag = materialList[i];
                if (tag <= 0)
                {
                    continue;
                }

                PushMaterialList(tag);
            }
        }



        void InitCurrentPetData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurPetTag != 0)//펫 태그값
            {
                var petTag = PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
                var petData = User.Instance.PetData;
                if (petData == null)
                {
                    Debug.Log("user's pet Data is null");
                    return;
                }

                var userPetInfo = petData.GetPet(petTag);
                if (userPetInfo == null)
                {
                    Debug.Log("user pet is null");
                    return;
                }

                currentPetTag = petTag;
                currentPetLevel = userPetInfo.Level;
                currentPetGrade = userPetInfo.Grade();
                currentPetTotalExp = userPetInfo.Exp;//펫 총 경험치
                currentPetReinForce = userPetInfo.Reinforce;
                currentPetExp = CalcModifyReduceEXP(userPetInfo.Exp);//현재 레벨에서 렙업을 위해 남은 경험치
                petMaxLevel = GameConfigTable.GetPetLevelMax(currentPetGrade);
                levelUpBtn.SetInteractable(currentPetLevel < petMaxLevel);
                levelUpBtn.SetButtonSpriteState(currentPetLevel < petMaxLevel);
                petMaxLevelTotalExp = PetExpData.GetCurrentAccumulateLevelExp(petMaxLevel, currentPetGrade);
            }
        }
        int CalcModifyReduceEXP(int sendtotalExp)
        {
            var requireTotalEXP = PetExpData.GetCurrentAccumulateLevelExp(currentPetLevel, currentPetGrade);//현재 레벨 요구 경험치
            return sendtotalExp - requireTotalEXP;
        }

        void RefreshPetFrame()
        {
            if (currentPetTag <= 0)
            {
                return;
            }

            var petData = User.Instance.PetData.GetPet(currentPetTag);
            if (petData == null)
            {
                return;
            }
            petFrame.SetPetPortraitFrame(petData);
            ClearPetInfo();
            petRankImgs[petData.Grade() - 1].SetActive(true);
            
            if (petNameText != null)
            {
                petNameText.text = petData.Name();
            }
        }

        void ClearPetInfo()
        {
            foreach(var obj in petRankImgs)
            {
                obj.SetActive(false);
            }
        }

        void RefreshPetInfo()
        {
            InitCurrentPetData();//현재 드래곤 레벨, 경험치 세팅
            InitLevelUPLabel();
            InitProgressBar();
            InitLevelLabel();
           // InitOverExpNode();
            RefreshPetProgressBar();//현재 프로그래스 갱신- 초기화
            RefreshPetFrame();//펫 초상화 갱신
        }

        //void InitOverExpNode()
        //{
        //    if (overExpNode != null)
        //    {
        //        overExpNode.SetActive(false);
        //    }

        //    if (overExpPopupNode != null)
        //    {
        //        overExpPopupNode.SetActive(false);
        //    }
        //}

        void RefreshPetProgressBar()//현재 경험치 총량 계산 - 경험치 아이템 클릭시 들어옴
        {
            var calcTotalExp = CalcTotalSelectNodeExp();
            if (currentPetLevel == 0) return;
            var final = PetExpData.GetLevelAddExp(currentPetLevel, currentPetExp, calcTotalExp, currentPetGrade);
            if (final == null) return;
            var level = final["finallevel"];//계산 완료 후 결과 레벨
            var reduceExp = final["reduceExp"];//계산 완료 후 나머지 경험치

            var maxLevelFlag = (petMaxLevel <= level);
            var levelChangeFlag = (level != currentPetLevel);

            RefreshLevelUPProgressBar(levelChangeFlag, level, reduceExp);
            RefreshLevelLabel(level);
            RefreshStatInfo(level);
            RefreshLevelUPLabel(maxLevelFlag, level, reduceExp);
            tempMaxLevelCheckFlag = maxLevelFlag;

            //if (tempMaxLevelCheckFlag)//경험치 초과분 표시 조건
            //{
            //    var overExp = final["overExp"];//초과분 경험치
            //    bool isOverExp = tempMaxLevelCheckFlag && overExp > 0;

            //    if (isOverExp)
            //    {
            //        overExpNode.SetActive(true);

            //        if (overExpLabel != null)
            //        {
            //            overExpLabel.text = string.Format(StringData.GetString(100002229), overExp);
            //        }

            //        if (IsShowOverPopup())
            //        {
            //            ShowOverExp();
            //        }
            //    }
            //}
            //else
            //{
            //    overExpNode.SetActive(false);
            //}
        }
        //public void onClickToggleOverExp()
        //{
        //    overExpPopupNode.SetActive(!overExpPopupNode.activeInHierarchy);
        //}

        //bool IsShowOverPopup()
        //{
        //    return overExpPopupNode.activeInHierarchy;
        //}

        //void ShowOverExp()
        //{
        //    overExpPopupNode.SetActive(true);
        //}

        //현재 선택한 재료 펫의 경험치 총량 계산
        int CalcTotalSelectNodeExp()
        {
            if (materialPetTagList == null || materialPetTagList.Count <= 0)
            {
                return 0;
            }

            int totalExp = 0;
            for (var i = 0; i < materialPetTagList.Count; i++)
            {
                var tag = materialPetTagList[i];
                if (tag <= 0)
                {
                    continue;
                }

                int exp = GetPetOfferExp(tag);
                float rate = ServerOptionData.GetFloat("pet_exp", 1.0f);
                totalExp += (int)(exp * rate);
            }

            return totalExp;
        }

        int GetPetOfferExp(int tag)
        {
            var totalExp = 0;
            var petData = User.Instance.PetData.GetPet(tag);
            if (petData == null)
            {
                return totalExp;
            }

            var petGrade = petData.Grade();
            var petLevel = petData.Level;
            var petReinforceLevel = petData.Reinforce;

            var petExpData = PetExpData.GetExpDataByGradeAndLevel(petLevel, petGrade);
            if (petExpData == null)
            {
                return totalExp;
            }

            var offerExp = petExpData.offer_exp;

            totalExp += offerExp;

            var petReinforceData = PetReinforceData.GetDataByGradeAndStep(petGrade, petReinforceLevel);
            if (petReinforceData == null)
            {
                return totalExp;
            }

            var levelBonus = petReinforceData.LEVEL_BONUS;
            totalExp += levelBonus;

            return totalExp;
        }

        /**
        * 
        * @param isLevelChangeFlag // 레벨 변경 플래그
        * @param level //변경 후 레벨
        * @param reduceExp //잔여 경험치
        * @returns 
        */
        Sequence tweenAnim = null;
        void RefreshLevelUPLabel(bool maxLevelFlag, int level, int reduceExp)
        {
            if (currentExpLabel == null)
            {
                return;
            }

            if (tweenAnim != null)
            {
                tweenAnim.Kill();
            }
            if (maxLevelFlag)
            {
                currentExpLabel.text = StringData.GetStringByIndex(100000113);
            }
            else
            {
                tweenAnim = DOTween.Sequence();

                var tweenRecttransform = tempTweenNode.GetComponent<RectTransform>();
                var totalExp = PetExpData.GetCurrentRequireLevelExp(level, currentPetGrade);//현재 레벨 요구 경험치
                if (level == currentPetLevel)
                {
                    tweenAnim.Append(tweenRecttransform.DOAnchorPos3D(new Vector3(reduceExp + currentPetExp, 0, 0), 0.2f)).OnUpdate(() =>
                    {
                        currentExpLabel.text = SBFunc.StrBuilder((int)tweenRecttransform.anchoredPosition3D.x, " / ", totalExp);
                    });
                }
                else
                {
                    tweenAnim.Append(tweenRecttransform.DOAnchorPos3D(new Vector3(0, reduceExp, totalExp), 0.2f)).OnUpdate(() =>
                    {
                        currentExpLabel.text = SBFunc.StrBuilder((int)tweenRecttransform.anchoredPosition3D.y, " / ", (int)tweenRecttransform.anchoredPosition3D.z);
                    });
                }

                tweenAnim.Play();
            }
        }

        void InitLevelUPLabel()
        {
            if (currentExpLabel == null)
            {
                return;
            }
            var totalExp = PetExpData.GetCurrentRequireLevelExp(currentPetLevel, currentPetGrade);//현재 레벨 요구 경험치
            currentExpLabel.text = SBFunc.StrBuilder(currentPetExp, " / ", totalExp);
            tempTweenNode.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(currentPetExp, 0, 0);
        }

        void InitProgressBar()//현재 레벨 및 경험치 기반으로 세팅
        {
            var totalExp = PetExpData.GetCurrentRequireLevelExp(currentPetLevel, currentPetGrade);//현재 레벨 요구 경험치

            float mod = (totalExp == 0) ? 1f : currentPetExp / (float)totalExp;
            if (levelUpBar != null && currentLevelBar)
            {
                levelUpBar.value = mod;
                currentLevelBar.value = mod;
            }
        }

        /**
         * @param levelChangeFlag //현재의 드래곤 레벨과 다른지 - 단순 동렙 체크용
         * @param level //들어온 레벨
         * @param reduceExp //잔여 경험치
         * @param isAutoClick //자동 선택버튼을 눌렀는지
         */
        void RefreshLevelUPProgressBar(bool levelChangeFlag, int level, int reduceExp)//레벨업 노티
        {
            var totalExp = PetExpData.GetCurrentRequireLevelExp(level, currentPetGrade);//현재 레벨 요구 경험치
            var maxLevelCheck = GameConfigTable.GetPetLevelMax(currentPetGrade);//펫 맥스 레벨 체크
            var isDragonMaxLevel = (maxLevelCheck <= level);//맥렙 체크
            var isDragonPrevLastLevel = (maxLevelCheck - 1 == level);//현재 들어온 레벨이 맥렙 바로 전인가 - 추후에 한계 레벨도 분류해야함
            float mod = 0;

            bool levelup = false;
            bool leveldown = false;
            if (tempLevelCheck < 0)
            {
                tempLevelCheck = level;
            }
            if (tempLevelCheck < level)//레벨이 증가하려함
            {
                leveldown = false;
                levelup = true;
            }
            else if (tempLevelCheck > level)//레벨이 떨어지려함
            {
                leveldown = true;
                levelup = false;
            }
            else
            {
                leveldown = false;
                levelup = false;
            }
            tempLevelCheck = level;


            if (totalExp != 0)
            {
                mod = reduceExp / (float)totalExp;
            }

            if (isDragonMaxLevel)//맥렙일때 - 맥렙이 된 경우
            {
                if (currentPetLevel >= maxLevelCheck)//이미 펫이 맥스레벨
                {
                    currentLevelBar.gameObject.SetActive(false);
                    levelUpBar.value = 1;
                    levelUpBar.gameObject.SetActive(true);
                    return;
                }
                if (leveldown == false && levelup == false)
                {
                    return;
                }
                playTweenAllMotion(mod, 5, levelChangeFlag);
            }
            else
            {
                if (levelChangeFlag)
                {
                    if (levelUpBar != null && currentLevelBar)
                    {
                        if (levelup == true && leveldown == false)
                        {
                            playTweenAllMotion(mod, 2, levelChangeFlag);
                        }
                        else if (levelup == false && leveldown == true)
                        {
                            if (isDragonPrevLastLevel)
                            {
                                playTweenAllMotion(mod, 4, levelChangeFlag);
                            }
                            else
                            {
                                playTweenAllMotion(mod, 3, levelChangeFlag);
                            }
                        }
                        else
                        {
                            playTweenAllMotion(mod, 1, levelChangeFlag);
                        }
                    }
                }
                else
                {
                    if (levelUpBar != null && currentLevelBar)
                    {

                        mod = (reduceExp + currentPetExp) / (float)totalExp;
                        var currentExp = PetExpData.GetCurrentRequireLevelExp(currentPetLevel, currentPetGrade);//현재 레벨 요구 경험치
                        var currentMode = (float)currentPetExp / currentExp;
                        currentLevelBar.value = currentMode;
                        if (leveldown)
                        {
                            if (isDragonPrevLastLevel)//맥렙 바로 전일때
                            {
                                playTweenAllMotion(mod, 4, levelChangeFlag);
                            }
                            else
                            {
                                playTweenAllMotion(mod, 3, levelChangeFlag);
                            }
                        }
                        else
                        {
                            playTweenAllMotion(mod, 1, levelChangeFlag);
                        }
                    }
                }
            }
        }

        void playTweenAllMotion(float modifyAmount, int caseIndex, bool isChangeCurrentLevel)
        {
            levelUpBar.gameObject.SetActive(true);

            if (tweenProgressAnim != null)
            {
                tweenProgressAnim.Kill();
            }
            tweenProgressAnim = DOTween.Sequence();

            switch (caseIndex)
            {
                case 1:
                    currentLevelBar.gameObject.SetActive(!isChangeCurrentLevel);
                    tweenProgressAnim.Append(levelUpBar.DOValue(modifyAmount, 0.2f));

                    break;
                case 2://레벨 업 평상렙
                    tweenProgressAnim.Append(levelUpBar.DOValue(1, 0.3f))
                        .AppendCallback(() => {
                            currentLevelBar.gameObject.SetActive(false);
                            levelUpBar.value = 0f;
                        })
                        .Append(levelUpBar.DOValue(modifyAmount, 0.3f));
                    break;
                case 3://레벨 다운 평상렙
                    tweenProgressAnim.Append(levelUpBar.DOValue(0, 0.3f)).Append(levelUpBar.DOValue(1, 0f)).AppendCallback(() => {
                        if (isChangeCurrentLevel == false)
                        {
                            currentLevelBar.gameObject.SetActive(true);
                        }
                    }).Append(levelUpBar.DOValue(modifyAmount, 0.3f));
                    break;
                case 4://레벨 다운(맥렙에서 맥렙 바로전)
                    tweenProgressAnim.Append(levelUpBar.DOValue(1, 0f)).OnComplete(() =>
                    {
                        levelUpBar.gameObject.SetActive(true);
                        if (!isChangeCurrentLevel)
                        {
                            currentLevelBar.gameObject.SetActive(true);
                        }
                    }).Append(levelUpBar.DOValue(modifyAmount, 0.3f));

                    break;
                case 5: //맥렙으로 레벨업
                    tweenProgressAnim.Append(levelUpBar.DOValue(1, 0.2f)).OnComplete(() =>
                    {
                        currentLevelBar.gameObject.SetActive(false);
                        levelUpBar.value = 1;
                        levelUpBar.gameObject.SetActive(true);
                    }).Append(levelUpBar.DOValue(1, 0.2f));
                    break;
            }

            if (tweenProgressAnim != null)
            {
                tweenProgressAnim.Play();
            }
        }
        void InitLevelLabel()
        {
            if (currentLevelLabel != null && nextLevelLabel != null)
            {
                var levelMod = string.Format("{0:D2}", currentPetLevel);
                currentLevelLabel.text = SBFunc.StrBuilder("Lv. ", levelMod);
                nextLevelLabel.text = SBFunc.StrBuilder("Lv. ", levelMod);
                tempPredictableLevel = currentPetLevel;
            }
        }
        void RefreshLevelLabel(int nextLevel)
        {
            if (currentLevelLabel != null && nextLevelLabel != null)
            {
                var levelMod = string.Format("{0:D2}", nextLevel);
                nextLevelLabel.text = SBFunc.StrBuilder("Lv. ", levelMod);
                tempPredictableLevel = nextLevel;
                
            }
        }


        Vector2 originSize = new Vector2(690, 50);
        Vector2 emptySize = new Vector2(1070, 50);
        void RefreshStatInfo(int nextLv)
        {
            foreach (var text in statTexts)
            {
                text.text = "---";
                text.alignment = TextAnchor.MiddleCenter;
                text.gameObject.GetComponent<RectTransform>().sizeDelta = emptySize;
            }

            foreach (var childNode in textChildNodeList)
                childNode.SetActive(false);

            if (currentPetTag <= 0) return;
            var stat = User.Instance.PetData.GetPet(currentPetTag).Stats;
            for(int i=0; i<stat.Count; ++i)
            {
                string statKey = stat[i].Key.ToString();
                PetStatData data = PetStatData.Get(statKey);
                bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;
                float curValue = PetStatData.GetStatValue(statKey, currentPetLevel, currentPetReinForce, stat[i].IsStatus1);
                float nextValue;
                if (currentPetLevel >= nextLv)
                {
                    nextValue = curValue;
                }
                else
                {
                    nextValue = PetStatData.GetStatValue(statKey, nextLv, currentPetReinForce, stat[i].IsStatus1);
                }
                SetBaseStatInfo(i, data.STAT_TYPE, curValue, nextValue, isPercent);
            }
        }
        void SetBaseStatInfo(int index, string statType, float optionValue, float nextOptionValue, bool isOptionValuePercent = false)
        {
            if (statTexts.Length > index && index >= 0)
            {
                string optionValueString = "+" + optionValue.ToString("F2");
                string nextOptionValueString = "+" + nextOptionValue.ToString("F2");
                if (isOptionValuePercent)
                {
                    optionValueString += "%";
                    nextOptionValueString += "%";
                }
                statTexts[index].gameObject.SetActive(true);
                statTexts[index].text = StatTypeData.GetDescStringByStatType(statType, isOptionValuePercent);
                statTexts[index].alignment = TextAnchor.MiddleLeft;
                currentStatTexts[index].text = optionValueString;
                nextStatTexts[index].text = nextOptionValueString;
                textChildNodeList[index].SetActive(true);
                statTexts[index].gameObject.GetComponent<RectTransform>().sizeDelta = originSize;
            }
        }


        public void OnClickLevelup()
        {
            if (materialPetTagList == null || materialPetTagList.Count <= 0)
            {
                Debug.Log("not select material pet");
                ToastManager.On(100001844);
                return;
            }

            bool conditionCheck = false;
            for (var i = 0; i < materialPetTagList.Count; i++)
            {
                var tag = materialPetTagList[i];
                if (tag <= 0)
                {
                    continue;
                }

                var check = IsAlertCondition(tag);
                if (check)
                {
                    conditionCheck = check;
                    break;
                }
            }

            if (conditionCheck)
            {
                ShowAlertPopup(COMPOUND_CONSTRAINT_GOAL_TIME ,()=>SendLevelUpRequest(), ()=> PopupManager.ClosePopup<PetLevelUpConstraintPopup>());
            }
            else
            {
                SendLevelUpRequest();
            }
        }

        void SendLevelUpRequest()
        {
            var data = new WWWForm();
            data.AddField("target", currentPetTag);
            data.AddField("materials", JsonConvert.SerializeObject(materialPetTagList.ToArray()));

            NetworkManager.Send("pet/levelup", data, (jsonData) =>
            {
                //Debug.Log("levelup success callback");
                if ( jsonData["rs"]!=null && (int)jsonData["rs"] == (int)eApiResCode.OK)
                { 
                    if (tempPredictableLevel > currentPetLevel)
                    {
                        PopupManager.OpenPopup<PetLevelUpPopup>(new PetLevelPopupData(currentPetTag,currentPetLevel,tempPredictableLevel,currentPetReinForce));
                    }

                    ForceUpdate();
                }
            });
        }

        /// <summary>
        /// 기획 변경 - 현재 조건 (자신 보다 높은 등급 & 30레벨 이상)
        /// 변경 조건 - 등급은 자신보다 높고 강화 단계 및 레벨 1이상
        /// </summary>
        /// <param name="clickPetTag"></param>
        /// <returns></returns>
        bool IsAlertCondition(int clickPetTag)
        {
            //기준 펫 데이터
            if (currentPetTag <= 0)
            {
                return false;
            }

            var petData = User.Instance.PetData.GetPet(currentPetTag);
            if (petData == null)
            {
                return false;
            }

            if (clickPetTag <= 0)
            {
                return false;
            }

            var clickPetData = User.Instance.PetData.GetPet(clickPetTag);
            if (clickPetData == null)
            {
                return false;
            }

            var petGrade = petData.Grade();
            var clickPetGrade = clickPetData.Grade();

            var clickPetLevel = clickPetData.Level;
            var clickReinforce = clickPetData.Reinforce;

            var isHighLevel = clickPetLevel > 1;
            var isHighReinforceLevel = clickReinforce > 0;
            var isHighGrade = petGrade < clickPetGrade;

            return (isHighReinforceLevel || isHighLevel || isHighGrade);
        }
        void ShowAlertPopup(string checkFlag, func ok_cb, func cancel_cb)//재료 펫레벨 30이상 이거나 선택한 펫이 레벨업 대상 펫보다 등급 높을 때
        {
            var valueCheck = SBFunc.HasTimeValue(checkFlag);
            if (valueCheck)
            {
                if (ok_cb != null)
                {
                    ok_cb();
                }
                return;//하루동안 보이지 않기 on
            }
            var popup = PopupManager.OpenPopup<PetLevelUpConstraintPopup>();
            popup.setMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("펫레벨업경고문구"));
            popup.setCallback(() =>
            {
                var checkValue = popup.toggle.isOn;
                if (checkValue)
                {
                    SBFunc.SetTimeValue(checkFlag);
                }
                if (ok_cb != null)
                {
                    ok_cb();
                }
                
            },
            () =>
            {
                //취소
                if (cancel_cb != null)
                {
                    cancel_cb();
                }
            },
            () =>
            {
                //x
                if (cancel_cb != null)
                {
                    cancel_cb();
                }
            });
        }
        public override bool backBtnCall()
        {
            if (backBtn != null)
            {
                backBtn.onClick.Invoke();
                return true;
            }
            return false;
        }




        
    }
}
