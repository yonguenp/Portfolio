using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetCompoundLayer : SubLayer
    {
        const int PET_COMPOUND_MAX_SLOT_NUM = 2;//펫 합성 재료 등록 최대 갯수
        const int PET_MERGE_GRADE_NOTIE_STANDARD = 3;// >=3 값이면 알림 확인 창 출력(3: SR)

        [SerializeField]
        PetTabLayer petTabLayer = null;
    
        [SerializeField]
        PetCompoundListPanel petCompoundSubList = null;

        [Space(10)]
        [Header("DetailInfo")]
        [SerializeField]
        private Sprite[] SkillBGSprite = null;
        [SerializeField]
        GameObject firstNode = null;
        [SerializeField]
        GameObject secondNode = null;

        [SerializeField]
        private GameObject skillDescTargetNode = null;
        
        [Space(10)]
        [Header("resultInfo")]
        [SerializeField]
        GameObject questionNode = null;
        [SerializeField]
        PetPortraitFrame resultPortrait = null;
        [SerializeField]
        Button resultBtn = null;


        [Space(10)]
        [Header("Back Button")]
        [SerializeField]
        Button backBtn = null;

        int petTag = -1;
        int newPetTag = -1;
    
        int[] compoundPetTagList = null;//합성 요청할 재료 팻 태그 리스트
        bool isInitFlag = false;
        bool isNetwork = false;

        void OnEnable()
        {
            isInitFlag = false;
        }

        public override void Init()
        {
            setPetCompoundList();
            setPetSubList();
            SetPetTag();//petinfo 태그값 먼저 받아서 선체크 
            SetDetailUIByTag();//태그값 기준으로 UI 세팅
            SetSkillDescTarget();//스킬 상세 정보 팝업 끄기
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
                    pushMaterialList(petTag);
                }
            }
        }
        void setPetSubList()//현재 갖고 있는 배터리 타입 전체 갱신
        {
            if (petCompoundSubList != null)
            {
                petCompoundSubList.ClickRegistCallback = RegistMaterialPetTag;
                petCompoundSubList.ClickReleaseCallback = ReleaseMaterialPetTag;
                petCompoundSubList.Init();//보유 펫리스트 시동
            }
        }

        void setPetCompoundList()
        {
            compoundPetTagList = new int[] { 0, 0 };//펫 재료 리스트 초기화
        }

        void SetDetailUIByTag()
        {
            if (isSelectTag())
            {
                RefreshPetDetailUI();
            }
            else
            {
                isInitFlag = true;
                initPetDetailUI();
            }
        }

        void SetSkillDescTarget()
        {
            if (skillDescTargetNode != null)
            {
                skillDescTargetNode.SetActive(false);
            }
        }

        bool isSelectTag()
        {
            return petTag > 0;
        }

        void RegistMaterialPetTag(string param)//리스트에서 재료 넣을 때
        {
            var tag = int.Parse(param);
            if (tag <= 0)
            {
                return;
            }

            pushMaterialList(tag);
            RefreshPetDetailUI();//재료 UI 부분 갱신
        }

        void ReleaseMaterialPetTag(string param)//리스트에서 재료 뺄 때
        {
            var tag = int.Parse(param);
            if (tag <= 0)
            {
                return;
            }

            popMaterialList(int.Parse(param));
            RefreshPetDetailUI();//재료 UI 부분 갱신
        }

        int popMaterialList(int tag)//pop되는 index 리턴
        {
            var index = Array.IndexOf(compoundPetTagList, tag);
            if (index > -1)
            {
                compoundPetTagList[index] = 0;
            }
            return index;
        }

        void pushMaterialList(int tag)
        {
            var index = Array.IndexOf(compoundPetTagList, tag);
            if (index < 0)
            {
                var check = GetAvailableIndexByCompoundList();
                compoundPetTagList[check] = tag;
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

        void popMaterialProcess(string tag)//재료쪽에서 강제로 빼게 하는 프로세스
        {
            var selectTag = int.Parse(tag);
            if (selectTag <= 0)
            {
                return;
            }

            if (petCompoundSubList != null)//리스트에서 등록한 펫 해제 프로세스
            {
                var setIndex = popMaterialList(selectTag);
                if (setIndex >= 0)
                {
                    initPetDetailSlot(setIndex);
                }

                petCompoundSubList.popMaterialList(selectTag);
                petCompoundSubList.ViewDirty = true;
                petCompoundSubList.SetTableViewFlag(false);
                petCompoundSubList.InitCustomSort();//다시 그리기 요청
                petCompoundSubList.SetTableViewFlag(true);
            }
        }

        void RefreshPetDetailUI()//펫 정보 불러오기 성공 시 데이터 세팅
        {
            if (compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < compoundPetTagList.Length; i++)
            {
                var tag = compoundPetTagList[i];
                var data = User.Instance.PetData.GetPet(tag);
                GameObject targetNode = null;

                if (tag <= 0)
                {
                    data = null;
                }

                switch (i)
                {
                    case 0:
                        targetNode = firstNode;
                        break;

                    case 1:
                        targetNode = secondNode;
                        break;
                }

                RefreshPetDetailSlot(data, targetNode);
            }

            var checkResultCondition = isResultCondition();
            initResultSlot(!checkResultCondition);
            SetResultButtonInteract(false);
            if (checkResultCondition && resultPortrait != null)
            {
                var mergeData = GetMergeData();
                if (mergeData != null)
                {
                    var targetGrade = mergeData.reward_grade;
                    resultPortrait.SetUpperPortrait(targetGrade);//결과 펫 초상화 세팅    
                    SetResultButtonInteract(true);
                }
            }
        }

        bool isResultCondition()
        {
            if(compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return false;
            }

            var checkCount = 0;
            for(var i = 0 ; i< compoundPetTagList.Length; i++)
            {
                var tag = compoundPetTagList[i];
                if (tag > 0)
                {
                    checkCount++;
                }
            }

            return checkCount == PET_COMPOUND_MAX_SLOT_NUM;
        }

        void RefreshPetDetailSlot(UserPet petInfo, GameObject targetNode)
        {
            var skillIconNodeList = initCommonSkillIconButton(targetNode);
            initPortrait(petInfo, targetNode);
            initPetName(petInfo, targetNode);

            if (petInfo == null)
            {
                return;
            }

            if (skillIconNodeList == null || skillIconNodeList.Length <= 0)
            {
                return;
            }

            initSkillInfoIcon(false, targetNode);
            var uniqueSkillLayer = SBFunc.GetChildrensByName(targetNode.transform, new string[] { "PetSkillInfo", "UniqueSkillLayer" }).gameObject;
            if (uniqueSkillLayer != null)
            {
                uniqueSkillLayer.SetActive(true);
            }

            var petSkillList = petInfo.SkillsID;//고유스킬은 빠진다고함
            for (var i = 0; i < petSkillList.Length; i++)
            {
                var totalNode = skillIconNodeList[i];
                if (totalNode == null)
                {
                    continue;
                }
                var iconNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "skill_icon"}).gameObject;
                if (iconNode == null)
                {
                    continue;
                }
                var spriteComp = iconNode.GetComponent<Image>();
                if (spriteComp == null)
                {
                    continue;
                }

                var petSkillID = petSkillList[i];
                if (petSkillID <= 0)
                {
                    continue;
                }

                var skillData = PetSkillNormalData.Get(petSkillID.ToString());
                if (skillData == null)
                {
                    continue;
                }

                totalNode.SetActive(true);

                var skillStat = skillData.stat;
                var tempBGIndex = 0;
                switch (skillStat)//0 : 파랑(def), 1 : 빨강(atk), 2 : 보라(cri), 3 : 초록 (hp)
                {
                    case "DEF":
                        tempBGIndex = 0;
                        break;
                    case "ATK":
                        tempBGIndex = 1;
                        break;
                    case "CRI_RATE":
                        tempBGIndex = 2;
                        break;
                    case "HP":
                        tempBGIndex = 3;
                        break;
                }

                var spriteBGComp = totalNode.GetComponent<Image>();
                if (spriteBGComp != null && tempBGIndex < SkillBGSprite.Length)
                {
                    spriteBGComp.sprite = SkillBGSprite[tempBGIndex];
                }

                //임시로 스킬 아이콘 버프로 씁니다.
                spriteComp.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, skillData.icon);
            }
        }

        GameObject[] initCommonSkillIconButton(GameObject targetNode)
        {
            var uniqueSkillLayer = SBFunc.GetChildrensByName(targetNode.transform, new string[] { "PetSkillInfo", "UniqueSkillLayer" }).gameObject;
            var commonSkillLayer = SBFunc.GetChildrensByName(targetNode.transform, new string[] { "PetSkillInfo", "CommonSkillLayer" }).gameObject; 
            var skillIconNodeList = SBFunc.GetChildren(commonSkillLayer);

            if (skillIconNodeList == null || skillIconNodeList.Length <= 0)
            {
                return skillIconNodeList;
            }

            for (var i = 0; i < skillIconNodeList.Length; i++)
            {
                var imageNode = skillIconNodeList[i];
                if (imageNode == null)
                {
                    continue;
                }

                imageNode.SetActive(false);
            }

            if (uniqueSkillLayer != null)
            {
                uniqueSkillLayer.SetActive(false);
            }

            return skillIconNodeList;
        }

        void initPortrait(UserPet petInfo ,GameObject targetNode)
        {
            var portrait = SBFunc.GetChildrensByName(targetNode.transform , new string[] { "PetPortraitSlot" }); 
            if (portrait != null)
            {
                var isVisible = (petInfo != null);
                portrait.gameObject.SetActive(isVisible);

                if (isVisible)
                {
                    var petPortraitFrame = portrait.GetComponent<PetPortraitFrame>();
                    if (petPortraitFrame == null)
                    {
                        return;
                    }

                    petPortraitFrame.SetPetPortraitFrame(petInfo);
                    petPortraitFrame.SetCallback(popMaterialProcess);
                }
            }
        }

        void initPetName(UserPet petInfo, GameObject targetNode)
        {
            var petNameNode = SBFunc.GetChildrensByName(targetNode.transform, new string[] { "LevelDataLabelNode", "nameLabel" });
            if (petNameNode == null)
            {
                return;
            }

            var petNameLabel = petNameNode.GetComponent<Text>();
            if (petNameLabel == null)
            {
                return;
            }

            if (petInfo == null)
            {
                petNameLabel.text = string.Format("[{0}]",StringData.GetStringByIndex(100001844));
            }
            else
            {
                petNameLabel.text = SBFunc.StrBuilder("[" , petInfo.Name(), "]");
            }
        }

        void initSkillInfoIcon(bool init ,GameObject targetNode )
        {
            var iconNode = SBFunc.GetChildrensByName(targetNode.transform, new string[] { "SkillDetailButton" });
            if (iconNode != null)
            {
                iconNode.gameObject.SetActive(!init);
            }
        }

        void initPetDetailUI()
        {
            for (var i = 0; i < PET_COMPOUND_MAX_SLOT_NUM; i++)
            {
                initPetDetailSlot(i);
            }

            initResultSlot(true);
            SetResultButtonInteract(false);
        }

        void SetResultButtonInteract(bool isInteract)
        {
            if (resultBtn != null)
            {
                resultBtn.SetInteractable(isInteract);
            }
        }

        void initResultSlot(bool init)
        {
            if (questionNode != null)
            {
                questionNode.SetActive(init);
            }

            if (resultPortrait != null)
            {
                resultPortrait.gameObject.SetActive(!init);
            }
        }

        void initPetDetailSlot(int index)
        {
            GameObject targetNode = null;

            switch (index)
            {
                case 0:
                    targetNode = firstNode;
                    break;
                case 1:
                    targetNode = secondNode;
                    break;
            }

            initPetName(null, targetNode);
            initCommonSkillIconButton(targetNode);
            initPortrait(null, targetNode);
            initSkillInfoIcon(true, targetNode);
        }

        PetMergeBaseData GetMergeData()
        {
            if(compoundPetTagList == null) 
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
        
            if(array.Count < PET_COMPOUND_MAX_SLOT_NUM) 
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

            var alertConditionCheck = isCompoundAlertCondition();
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
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002118), StringData.GetStringByIndex(100002226),
                () => {
                    //ok
                    RequestPetMerge();
                },
                () => {
                    //cancle
                },
                () => {
                    //x
                }
            );
        }

        bool isCompoundAlertCondition()
        {
            if(compoundPetTagList == null || compoundPetTagList.Length <= 0)
            {
                return false;
            }

            var resultCondition = isResultCondition();
            if(!resultCondition)
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

                NetworkManager.Send("pet/merge", data, ResponseMerge, ResponseFail);
            }
        }

        protected void ResponseMerge(JObject jsonData)
        {
            if (jsonData == null)
            {
                return;
            }
            if ((eApiResCode) jsonData["rs"].Value<int>() != eApiResCode.OK)
            {
                isNetwork = false;
                ResponseMessage(jsonData["rs"]);
                SetResultButtonInteract(true);
                return;
            }

            //새로운 펫 태그 받아야함.

            var newPetTag = jsonData["ptag"].Value<int>();
            if (newPetTag <= 0)
            {
                //ToastManager.On("올바른 서버 응답이 아님 펫 태그: " + newPetTag);
                Debug.Log("올바른 서버 응답이 아님 펫 태그: " + newPetTag);
                SuccessResponseRefreshUI();
                SetResultButtonInteract(true);
                return;
            }

            this.newPetTag = newPetTag;


            SuccessResponseRefreshUI();
            ShowResultPopup(jsonData);
        }

        void ShowResultPopup(JObject jsonData)
        {
            if(jsonData.ContainsKey("ptag"))
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

                var baseData = petData.GetPetDesignData();

                PetPopupData popupData = new PetPopupData(false, true, tag, baseData);
                PopupManager.OpenPopup<PetCompoundResultPopup>(popupData);

                // 시스템 메시지 전송
                if (baseData != null && baseData.GRADE >= (int)eDragonGrade.Unique)
                {
                    eAchieveSystemMessageType messageType = eAchieveSystemMessageType.COMPOUND_PET_U;

                    switch ((eDragonGrade)baseData.GRADE)
                    {
                        case eDragonGrade.Unique:
                            messageType = eAchieveSystemMessageType.COMPOUND_PET_U;
                            break;
                        case eDragonGrade.Legend:
                            messageType = eAchieveSystemMessageType.COMPOUND_PET_L;
                            break;
                    }

                    ChatManager.Instance.SendAchieveSystemMessage(messageType, User.Instance.UserData.UserNick, baseData.KEY);
                }
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

        public void onClickBackButton()
        {
            if (isInitFlag)
            {
                petTabLayer.onClickChangeLayer("0");
            }
            else
            {
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;

                var petTag = 0;
                if (this.petTag > 0)
                {
                    petTag = this.petTag;
                }
                else if (this.newPetTag > 0)
                {//새로운 펫이 생성이 되었으면 새 펫 인덱스로 넘겨야함
                    petTag = this.newPetTag;
                }
                else
                {
                    this.petTabLayer.onClickChangeLayer("0");
                    return;
                }

                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = petTag;
                petTabLayer.onClickChangeLayer("1");
            }
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

