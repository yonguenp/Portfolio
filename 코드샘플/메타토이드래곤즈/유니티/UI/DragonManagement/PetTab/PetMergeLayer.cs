using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [Serializable]
    public class PetMergeInfo
    {
        [SerializeField]
        PetPortraitFrame petPortraitFrame = null;
        [SerializeField]
        Text petNameText = null;
        
        public void SetPetInfo(UserPet userPet)
        {
            if (userPet == null || userPet.Tag <= 0)
            {
                OffPetFrame();
                return;
            }
            petPortraitFrame.gameObject.SetActive(true);
            petPortraitFrame.SetPetPortraitFrame(userPet);

            petNameText.text = userPet.Name();
            petNameText.gameObject.SetActive(true);
        }

        public void OffPetFrame()
        {
            petPortraitFrame.gameObject.SetActive(false);
            petNameText.gameObject.SetActive(false);
        }

        public void SetPetPortraitClickCallBack(VoidDelegate cb)
        {
            petPortraitFrame.SetCallback((str)=> cb?.Invoke());
        }
    }
    public class PetMergeLayer : SubLayer
    {
        const int PET_COMPOUND_MAX_SLOT_NUM = 2;//펫 합성 재료 등록 최대 갯수
        const int PET_MERGE_GRADE_NOTIE_STANDARD = 3;// >=3 값이면 알림 확인 창 출력(3: SR)

        [SerializeField]
        PetTabLayer petTabLayer = null;

        [SerializeField]
        PetListPanel petCompoundSubList = null;

        [Space(10)]
        [Header("Detail Info")]
        [SerializeField]
        PetMergeInfo[] petMergeInfos = null;

        [SerializeField]
        Button mergeBtn = null;

        [SerializeField]
        Image mergeNeedItemImg =  null;
        [SerializeField]
        Text mergeNeedGoldText = null;

        [Space(10)]
        [Header("Result Info")]
        [SerializeField]
        GameObject gradeLayerObject = null;
        [SerializeField]
        Image gradeBGImage = null;
        [SerializeField]
        Text gradeText = null;

        [Space(10)]
        [Header("Back Button")]
        [SerializeField]
        Button backBtn = null;

        int petTag = -1;
        int newPetTag = -1;

        int[] compoundPetTagList = null;//합성 요청할 재료 팻 태그 리스트
        bool isInitFlag = false;
        bool isNetwork = false;
        int mergeNeedAmount = 0;

        string costType = "";

        void OnEnable()
        {
            isInitFlag = false;
        }

        public override void Init()
        {
            compoundPetTagList = new int[] { 0, 0 };//펫 재료 리스트 초기화
            SetPetTag();//petinfo 태그값 먼저 받아서 선체크 
            SetPetSubList();
            SetDetailUIByTag();//태그값 기준으로 UI 세팅
            RefreshMergeNeedState();
        }

        void RefreshMergeNeedState(int targetGrade =0)
        {
            //to do .. grade 에 따른 합성 비용 세팅 targetGrade 다음 등급으로 가는데 필요한 골드
            var targetMergeData = PetMergeBaseData.GetByRewardGrade(targetGrade);
            if(targetMergeData != null )
            {
                mergeNeedAmount = targetMergeData.cost_num;
                costType = targetMergeData.cost_type;
                switch (costType)
                {
                    case "GEMSTONE":
                        mergeNeedGoldText.color = (User.Instance.GEMSTONE >= mergeNeedAmount) ? Color.white : Color.red;
                        mergeNeedItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                        break;
                    case "GOLD":
                    default:
                        mergeNeedGoldText.color = (User.Instance.GOLD >= mergeNeedAmount) ? Color.white : Color.red;
                        mergeNeedItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                        break;
                    
                }
                mergeNeedGoldText.text = SBFunc.CommaFromNumber(mergeNeedAmount);
            }
            else
            {
                mergeNeedGoldText.color = Color.white;
                mergeNeedGoldText.text = "-";
            }
        }

        void SetPetTag()
        {
            petTag = -1;
            newPetTag = -1;

            if (PopupManager.GetPopup<DragonManagePopup>().CurPetTag != 0)
            {
                petTag = PopupManager.GetPopup<DragonManagePopup>().CurPetTag;

                if (petTag > 0)
                {
                    var pet = User.Instance.PetData.GetPet(petTag);
                    if (pet.Level < GameConfigTable.GetPetLevelMax(pet.Grade()))
                    {//만렙 아니면 리턴
                        return;
                    }

                    if (pet.Reinforce < GameConfigTable.GetPetReinforceLevelMax(pet.Grade()))
                    {//만강 아니면 리턴
                        return;
                    }
                    PushMaterialList(petTag);
                }
            }
        }
        void SetPetSubList()
        {
            if (petCompoundSubList != null)
            {
                petCompoundSubList.ClickRegistCallback = RegistMaterialPetTag;
                petCompoundSubList.ClickReleaseCallback = ReleaseMaterialPetTag;
                petCompoundSubList.Init( ePetPopupState.Compound);//보유 펫리스트 시동
            }
        }



        void SetDetailUIByTag()
        {
            if (petTag > 0)
            {
                RefreshPetDetailUI();
            }
            else
            {
                isInitFlag = true;
                InitPetDetailUI();
            }
        }

        void RegistMaterialPetTag(string param)//리스트에서 재료 넣을 때
        {
            var tag = int.Parse(param);
            if (tag <= 0)
            {
                return;
            }

            PushMaterialList(tag);
            RefreshPetDetailUI();//재료 UI 부분 갱신
        }

        void ReleaseMaterialPetTag(string param)//리스트에서 재료 뺄 때
        {
            var tag = int.Parse(param);
            if (tag <= 0)
            {
                return;
            }

            // 처음 선택한 펫 클릭 시 처리 -> 현재는 제거 불가능하도록 처리 (23.7.28)
            //if (compoundPetTagList.Length == 1 && compoundPetTagList[0] == tag)
            //{
            //    // todo.. 필요 시 토스트팝업 추가
            //    return;
            //}

            PopMaterialList(int.Parse(param));
            RefreshPetDetailUI();//재료 UI 부분 갱신
        }

        int PopMaterialList(int tag)//pop되는 index 리턴
        {
            var index = Array.IndexOf(compoundPetTagList, tag);
            if (index > -1)
            {
                compoundPetTagList[index] = 0;
            }

            if (compoundPetTagList[0] ==0 && compoundPetTagList[1] == 0)  // 합성 할 목록에 어떠한 펫도 존재하지 않으면
            {
                mergeNeedAmount = 0;
            }
            return index;
        }

        void PushMaterialList(int tag)
        {
            var index = Array.IndexOf(compoundPetTagList, tag);
            if (index < 0)
            {
                var check = GetAvailableIndexByCompoundList();
                if (check >= 0)
                {
                    compoundPetTagList[check] = tag;
                }
            }
        }

        int GetAvailableIndexByCompoundList()//0자리를 찾아서 빈 값 가능 인덱스 찾기
        {
            if (compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return -1;
            }
            var index = -1;

            for (var i = 0; i < compoundPetTagList.Length; i++)
            {
                var tag = compoundPetTagList[i];
                if (tag <= 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        void PopMaterialProcess(string tag)//재료쪽에서 강제로 빼게 하는 프로세스
        {
            var selectTag = int.Parse(tag);
            if (selectTag <= 0)
            {
                return;
            }

            if (petCompoundSubList != null)//리스트에서 등록한 펫 해제 프로세스
            {
                var setIndex = PopMaterialList(selectTag);
                petMergeInfos[setIndex].OffPetFrame();
                petCompoundSubList.PopMaterialList(selectTag);
                petCompoundSubList.ViewDirty = true;
                petCompoundSubList.SetTableViewFlag(false);
                petCompoundSubList.InitCustomSort();//다시 그리기 요청
                petCompoundSubList.SetTableViewFlag(true);
            }
            RefreshPetDetailUI();
        }

        void RefreshPetDetailUI()//펫 정보 불러오기 성공 시 데이터 세팅
        {
            foreach (var petInfo in petMergeInfos)
            {
                petInfo.OffPetFrame();
            }
            if (compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return;
            }

            UserPet firstTargetPet = null;
            UserPet secondTargetPet = null;
            for (var i = 0; i < compoundPetTagList.Length; i++)
            {
                var tag = compoundPetTagList[i];
                var data = User.Instance.PetData.GetPet(tag);
                if (tag <= 0)
                {
                    data = null;
                }
                petMergeInfos[i].SetPetInfo(data);
                petMergeInfos[i].SetPetPortraitClickCallBack(() => { 
                    PopMaterialProcess(tag.ToString());
                }) ;

                if (i == 0)
                    firstTargetPet = data;
                if (i == 1)
                    secondTargetPet = data;
            }

            // 펫 승급 결과 레이어 구성
            if (firstTargetPet != null || secondTargetPet != null)
            {
                UserPet targetData = firstTargetPet != null ? firstTargetPet : secondTargetPet;
                int petGrade = targetData.Grade() + 1;
                gradeBGImage.sprite = SBFunc.GetGradeBGSprite(petGrade);
                gradeText.text = SBFunc.GetGradeConvertString(petGrade);
            }
            gradeLayerObject.SetActive(firstTargetPet != null || secondTargetPet != null);

            var checkResultCondition = IsResultCondition();
            SetResultButtonInteract(false);
            if (checkResultCondition)
            {
                var mergeData = GetMergeData();
                if (mergeData != null)
                {
                    var targetGrade = mergeData.reward_grade;
                    RefreshMergeNeedState(targetGrade);
                    SetResultButtonInteract(true);
                }
            }
            else
                RefreshMergeNeedState(0);
        }

        bool IsResultCondition()
        {
            if (compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return false;
            }
            var checkCount = 0;
            for (var i = 0; i < compoundPetTagList.Length; i++)
            {
                var tag = compoundPetTagList[i];
                if (tag > 0)
                {
                    checkCount++;
                }
            }
            return checkCount == PET_COMPOUND_MAX_SLOT_NUM;
        }

        void InitPetDetailUI()
        {
            foreach (var petStat in petMergeInfos)
            {
                petStat.OffPetFrame();
            }
            SetResultButtonInteract(false);
        }

        void SetResultButtonInteract(bool isInteract)
        {
            if (mergeBtn != null)
            {
                bool state = costType switch
                {
                    "GOLD" => User.Instance.GOLD >= mergeNeedAmount && isInteract,
                    "GEMSTONE" => User.Instance.GEMSTONE >= mergeNeedAmount && isInteract,
                    _ => User.Instance.GOLD >= mergeNeedAmount && isInteract
                };
                    
                mergeBtn.SetButtonSpriteState(state);
                mergeBtn.interactable = state;
            }
        }

        PetMergeBaseData GetMergeData()
        {
            if (compoundPetTagList == null)
            {
                return null;
            }

            List<int> array = new List<int>();
            array.Clear();
            var count = compoundPetTagList.Length;
            for (var i = 0; i < count; ++i)
            {
                var tag = compoundPetTagList[i];
                if (tag <= 0)
                {
                    continue;
                }
                var petInfo = User.Instance.PetData.GetPet(tag);
                if (petInfo == null)
                {
                    continue;
                }

                var petGrade = petInfo.Grade();
                array.Add(petGrade);
            }

            if (array.Count < PET_COMPOUND_MAX_SLOT_NUM)
            {
                return null;
            }
            return PetMergeBaseData.GetMergeData(array);
        }

        public void OnClickMerge()
        {
            var resultData = GetMergeData();
            if (resultData == null)
            {
                return;
            }
            var alertConditionCheck = IsCompoundAlertCondition();
            if (alertConditionCheck)
            {
                ShowAlertPopup();
            }
            else
            {
                RequestPetMerge();
            }
        }


        void ShowAlertPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("승급"), StringData.GetStringByStrKey("펫승급알림"),
                 RequestPetMerge, () => { },() => { } );
        }

        bool IsCompoundAlertCondition()
        {
            if (compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return false;
            }

            var resultCondition = IsResultCondition();
            if (!resultCondition)
            {
                return false;
            }
            var resultData = GetMergeData();
            var currentGrade = resultData.material_grade;

            return currentGrade >= PET_MERGE_GRADE_NOTIE_STANDARD;
        }

        void RequestPetMerge()
        {
            if (isNetwork)
            {
                return;
            }
            if (compoundPetTagList == null || compoundPetTagList.Length < PET_COMPOUND_MAX_SLOT_NUM)
            {
                return;
            }

            List<int> array = new List<int>();
            array.Clear();
            for (var i = 0; i < PET_COMPOUND_MAX_SLOT_NUM; ++i)
            {
                var tag = compoundPetTagList[i];
                if (tag > 0)
                {
                    array.Add(tag);
                }
            }

            if (array.Count >= PET_COMPOUND_MAX_SLOT_NUM)
            {
                isNetwork = true;
                SetResultButtonInteract(false);

                WWWForm data = new WWWForm();
                data.AddField("ptag", JsonConvert.SerializeObject(array.ToArray()));
                NetworkManager.Send("pet/merge", data, ResponseMerge, (string param) => { isNetwork = false; });
            }
        }

        protected void ResponseMerge(JObject jsonData)
        {
            if (jsonData == null)
            {
                return;
            }
            if ((eApiResCode)jsonData["rs"].Value<int>() != eApiResCode.OK)
            {
                isNetwork = false;
                ResponseMessage(jsonData["rs"]);
                SetResultButtonInteract(true);
                return;
            }

            //새로운 펫 태그 받아야함.

            var newTag = jsonData["ptag"].Value<int>();
            if (newTag <= 0)
            {
                //ToastManager.On("올바른 서버 응답이 아님 펫 태그: " + newTag);
                Debug.Log("올바른 서버 응답이 아님 펫 태그: " + newTag);
                SuccessResponseRefreshUI();
                SetResultButtonInteract(true);
                return;
            }
            newPetTag = newTag;

            SuccessResponseRefreshUI();
            ShowResultPopup(jsonData);
        }

        void ShowResultPopup(JObject jsonData)
        {
            if (jsonData.ContainsKey("ptag"))
            {
                var tag = jsonData["ptag"].Value<int>();
                var petData = User.Instance.PetData.GetPet(tag);
                if (petData == null)
                {
                    //ToastManager.On("올바른 서버 응답이 아님 펫 태그: " + tag);
                    Debug.Log("올바른 서버 응답이 아님 펫 태그: " + tag);
                    SuccessResponseRefreshUI();
                    return;
                }

                if (petData.Grade() == 5)
                {
                    eAchieveSystemMessageType messageType = eAchieveSystemMessageType.GET_EQUIPMENT;
                    ChatManager.Instance.SendAchieveSystemMessage(messageType, User.Instance.UserData.UserNick, petData.ID);
                }

                var baseData = petData.GetPetDesignData();
                PetPopupData popupData = new PetPopupData(false, true, tag, baseData);
                PopupManager.OpenPopup<PetCompoundResultPopup>(popupData);
            }
        }

        void SuccessResponseRefreshUI()
        {
            isNetwork = false;
            ForceUpdate();
        }

        public override void ForceUpdate()//초기화
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;
            Init();
        }

        protected void ResponseFail(string param)
        {
            isNetwork = false;
        }

        protected void ResponseMessage(JToken resCode)
        {
            switch ((eApiResCode)resCode.Value<int>())
            {
                case eApiResCode.PET_NO_SUCH_PET:
                case eApiResCode.PET_NOT_EXIST:
                {
                    ToastManager.On(100002557);
                }
                break;
                case eApiResCode.PET_NO_SUCH_DRAGON:
                {
                    ToastManager.On(100002548);
                }
                break;
                case eApiResCode.PET_ALREADY_EQUIPPED:
                {
                    ToastManager.On(100002554);
                }
                break;
                case eApiResCode.INVAILD_PET_MATERIALS:
                {
                    ToastManager.On(100002556);
                }
                break;
                case eApiResCode.PET_FULL:
                {
                    ToastManager.On(100002555);
                }
                break;
            }
        }

        public void OnClickBackButton()
        {
            if (isInitFlag)
            {
                petTabLayer.onClickChangeLayer("1");
            }
            else
            {
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;
                var tag = 0;
                if (petTag > 0)
                {
                    tag = petTag;
                }
                else if (newPetTag > 0)
                {//새로운 펫이 생성이 되었으면 새 펫 인덱스로 넘겨야함
                    tag = newPetTag;
                }
                else
                {
                    petTabLayer.onClickChangeLayer("1");
                    return;
                }
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = tag;
                petTabLayer.onClickChangeLayer("1");
            }
        }

        public void OnClickShowPetDetailPopup()
        {
            if (compoundPetTagList == null) return;

            UserPet leftData = null;
            UserPet rightData = null;
            for (var i = 0; i < compoundPetTagList.Length; i++)
            {
                int tag = compoundPetTagList[i];
                var data = User.Instance.PetData.GetPet(tag);
                if (tag <= 0)
                {
                    data = null;
                }

                if (i == 0)
                    leftData = data;
                else if (i == 1)
                    rightData = data;

            }

            // 선택된 펫 없을 시 예외처리
            if (leftData == null && rightData == null)
            {
                ToastManager.On(100005344);
                return;
            }

            PetDetailInfoPopupData newPopupData = new PetDetailInfoPopupData(leftData, rightData);
            PopupManager.OpenPopup<PetDetailInfoPopup>(newPopupData);
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