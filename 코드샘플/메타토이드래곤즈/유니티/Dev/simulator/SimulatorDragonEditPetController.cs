using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorDragonEditPetController : MonoBehaviour
    {
        [SerializeField] private List<SimulatorDragonEditPetSkillSlot> petSkillSlot = null;
        [SerializeField] private SBSimulatorDropDown dragonPetDrop = null;//펫 정보 리스트 드롭
        [SerializeField] private SBSimulatorDropDown petSkillDrop = null;//펫 정보 리스트 드롭
        [SerializeField] private SBSimulatorDropDown petLevelDrop = null;//펫 레벨 리스트 드롭

        [SerializeField] private PetPortraitFrame petFrame = null;
        [SerializeField] private Button petPresetButton = null;
        [SerializeField] private SimulatorPetPresetPanel presetPanel = null;

        int petTag = -1;

        PetTable petTable = null;
        PetGradeTable petGradeTable = null;
        PetExpTable petExpTable = null;
        PetSkillNormalTable petSkillTable = null;
        UserPet petData = null;
        public UserPet PetData { get { return petData; } }

        UserPet prevPet = null;//이전 유저 펫 데이터 백업용
        public UserPet PrevPet { get { return prevPet; } }

        int clickSkillSlotIndex = -1;//펫 스킬 등록할 때 클릭하는 인덱스
        void SetPetTag(int tag)
        {
            petTag = tag;
        }

        void initSavePrevPet()//유저 펫 백업
        {
            var userPetData = User.Instance.PetData.GetPet(petTag);
            if (userPetData != null)
            {
                UserPet p = new UserPet(userPetData.Tag, userPetData.ID, userPetData.Level, userPetData.Exp, userPetData.Reinforce, userPetData.Obtain);
                p.SetUniqueSkillID(userPetData.UniqueSkillID);
                p.SetSkillsID(userPetData.SkillsID);

                prevPet = p;
            }
            else
            {
                prevPet = null;
            }
        }

        public void dumpPrevPet()
        {
            if(prevPet != null)//이전 펫 데이터 없으면 현재 펫 지우기
            {
                User.Instance.PetData.AddPet(prevPet);
            }
            else
            {
                if (petTag > 0)
                {
                    User.Instance.PetData.DeleteUserPet(petTag);
                }
            }
        }
        public void initPrevPet()
        {
            if (prevPet != null)
                prevPet = null;
        }

        void SetTable()
        {
            if (petTable == null)
            {
                petTable = TableManager.GetTable<PetTable>();
            }

            if(petGradeTable == null)
            {
                petGradeTable = TableManager.GetTable<PetGradeTable>();
            }

            if(petSkillTable == null)
            {
                petSkillTable = TableManager.GetTable<PetSkillNormalTable>();
            }

            if(petExpTable == null)
            {
                petExpTable = TableManager.GetTable<PetExpTable>();
            }
        }

        public void init(int tag)
        {
            SetTable();
            SetPetTag(tag);
            initSavePrevPet();//이전 펫 데이터 백업
            RefreshPetData();
            SetPetDropData();
            RefreshDragonPetDropOption();

            SetVisiblePetSkillDrop(false);//펫 스킬 드롭 다운 끄기

            RefreshPetLevelDropUI();//펫 레벨 드롭 다운 세팅
            SetDropDownHideEvent();//펫 스킬 드롭 다운 이벤트 추가

            RefreshPetSkillSlot();//스킬 슬롯 세팅

            initPetPresetPanel();//프리셋 패널 켜져있음 끄기

            //기존 펫 등록하고 한번 refresh
            //팝업 ForceUpdate 쳐서 능력치 갱신 추가
            PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
        }

        void RefreshPetData()//클릭 시 갱신해줘야할 데이터
        {
            SetPetData();
            RefreshPetIcon();
        }

        void SetPetData()
        {
            var userPetData = User.Instance.PetData.GetPet(petTag);
            if(userPetData != null)
            {
                UserPet p = new UserPet(userPetData.Tag, userPetData.ID, userPetData.Level, userPetData.Exp, userPetData.Reinforce, userPetData.Obtain);
                p.SetUniqueSkillID(userPetData.UniqueSkillID);
                p.SetSkillsID(userPetData.SkillsID);

                petData = p;
            }
        }

        void RefreshPetIcon()
        {
            var isData = petData != null;
            petFrame.gameObject.SetActive(isData);

            if (isData)
            {
                petFrame.SetPetPortraitFrame(petData,false,false);
            }
        }

        void SetPetDropData()
        {
            if (dragonPetDrop == null)
            {
                return;
            }

            dragonPetDrop.ClearOptions();


            List<string> levelList = new List<string>();

            levelList.Add("none");

            var partList = petTable.GetAllList();

            for (int i = 0, count = partList.Count; i < count; ++i)
            {
                var petBaseData = partList[i];
                if (petBaseData == null)
                    continue;

                var key = petBaseData.KEY;
                if (key == 0)
                    continue;

                var grade = petBaseData.GRADE.ToString();

                var element = petBaseData.ELEMENT;

                string elementStr = "";
                switch (element)
                {
                    case 1:
                        elementStr = "불";
                        break;
                    case 2:
                        elementStr = "물";
                        break;
                    case 3:
                        elementStr = "땅";
                        break;
                    case 4:
                        elementStr = "바람";
                        break;
                    case 5:
                        elementStr = "빛";
                        break;
                    case 6:
                        elementStr = "어둠";
                        break;
                }

                string petName = StringData.GetStringByStrKey(petBaseData._NAME);


                levelList.Add(SBFunc.StrBuilder(key, "_", grade, "_", elementStr, "_", petName));
            }

            dragonPetDrop.AddOptions(levelList);
        }

        void RefreshDragonPetDropOption()
        {
            if (petTag <= 0)
            {
                return;
            }
            
            string dropText = "";
            int dropValue = 0;
            if (petData != null)
            {
                var petID = petData.ID;

                if(dragonPetDrop == null) { return; }

                for(var i = 0; i< dragonPetDrop.options.Count; i++)
                {
                    var option = dragonPetDrop.options[i];

                    var optionText = option.text;

                    if(optionText == "none")
                    {
                        continue;
                    }

                    var splitData = optionText.Split("_");
                    if( int.Parse(splitData[0]) == petID)
                    {
                        dropText = optionText;
                        dropValue = i;
                        break;
                    }
                }
            }
            else
            {
                dropText = "none";
                dropValue = 0;
            }
            dragonPetDrop.captionText.text = dropText;
            dragonPetDrop.value = dropValue;
        }

        public void onClickDragonPetDropDown()//펫 선택 시
        {
            var selectedText = dragonPetDrop.captionText.text;
            if (selectedText == "none")//해당 tag 의 pet Data 삭제
            {
                User.Instance.PetData.DeleteUserPet(petTag);

                petData = null;
            }
            else
            {
                var id = int.Parse(selectedText.Split("_")[0]);

                if(petData != null && petData.ID == id)//이미 같은 데이터를 들고있음
                {
                    return;
                }

                //펫 기본형 생성 및 삽입
                var newPet = SetDefaultPet(petTag, id);
                User.Instance.PetData.AddPet(newPet);
            }

            RefreshPetData();//새로운 펫 데이터 갱신
            RefreshDragonPetDropOption();
            RefreshPetSkillSlot();
            RefreshPetLevelDropUI();// 펫 데이터가 있으면 레벨 드롭 박스 세팅 및 켜기

            //팝업 ForceUpdate 쳐서 능력치 갱신 추가
            PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
        }

        UserPet SetDefaultPet(int petTag, int petID)//기본 드래곤 (펫x , 장비 x)데이터 세팅
        {
            var userPetTempData = new UserPet(petTag, petID, 1, 0, 0, -1);
            userPetTempData.SetUniqueSkillID(-1);
            int size = GetNormalSkillSize(petID);
            userPetTempData.SetSkillsID(new int[size]);//일단 생성

            return userPetTempData;
        }
        int GetNormalSkillSize(int petID)
        {
            var slotCount = 0;

            var petBaseData = petTable.Get(petID);
            if(petBaseData == null)
            {
                return slotCount;
            }

            var grade = petBaseData.GRADE;
            slotCount = petGradeTable.Get(grade).START_STAT_NUM;

            return slotCount;
        }

        void RefreshPetSkillSlot()
        {
            if(petSkillSlot == null || petSkillSlot.Count <= 0)
            {
                return;
            }

            if(petData == null)
            {
                for(var i = 0; i< petSkillSlot.Count; i++)
                {
                    petSkillSlot[i].SetVisibleSkillSlot(false);
                }
                return;
            }

            var petID = petData.ID;
            var petLevel = petData.Level;
            var SkillSlotCount = GetNormalSkillSize(petID);

            var petSkillData = petData.SkillsID;
            var petCurrentSkillCount = petSkillData.Length;

            for(var i = 0; i < petSkillSlot.Count; i++)
            {
                if(SkillSlotCount > i)
                {
                    petSkillSlot[i].SetVisibleSkillSlot(true);
                    petSkillSlot[i].initSkillSlot();
                    if (petCurrentSkillCount > i)
                    {
                        petSkillSlot[i].RefreshPetSkillIcon(petLevel, petSkillData[i]);
                    }
                }
                else
                {
                    petSkillSlot[i].SetVisibleSkillSlot(false);
                }
            }
        }

        public void onClickPetSkillButton(int _slotIndex)//펫 스킬 세팅
        {
            clickSkillSlotIndex = _slotIndex;

            SetPetSkillDropData();
            RefreshPetSkillDrop(_slotIndex);
            SetVisiblePetSkillDrop(true);
        }

        void SetVisiblePetSkillDrop(bool _isVisible)
        {
            petSkillDrop.gameObject.SetActive(_isVisible);
        }

        void SetPetSkillDropData()//펫마다 등급이 달라서 클릭 시 마다 갱신
        {
            if(petData == null)
            {
                return;
            }

            if(petSkillDrop == null)
            {
                return;
            }

            var groupID = petData.Grade();
            var groupList = petSkillTable.GetSkillDataByGrade(groupID);
            if(groupList == null || groupList.Count <= 0)
            {
                return;
            }

            List<string> stringList = new List<string>();
            stringList.Add("none");

            for(var i = 0; i< groupList.Count; i++)
            {
                var petSkillData = groupList[i];
                if(petSkillData == null)
                {
                    continue;
                }

                var dataKey = petSkillData.KEY.ToString();
                var statType = petSkillData.stat;
                var value = petSkillData.value.ToString();
                var statStr = "";
                switch(statType)
                {
                    case "ATK":
                        statStr = "공";
                        break;
                    case "DEF":
                        statStr = "방";
                        break;
                    case "HP":
                        statStr = "체";
                        break;
                    case "CRI_RATE":
                        statStr = "크";
                        break;
                }

                stringList.Add(SBFunc.StrBuilder(dataKey , "_", statStr , "_", value , "%"));
            }

            petSkillDrop.ClearOptions();
            petSkillDrop.AddOptions(stringList);
        }

        void RefreshPetSkillDrop(int _skillSlotIndex)
        {
            if(petData == null)
            {
                return;
            }

            var skills = petData.SkillsID;

            if(petSkillDrop == null)
            {
                return;
            }

            if(skills == null || skills.Length <= _skillSlotIndex)
            {
                return;
            }

            var skillID = skills[_skillSlotIndex];

            int dropValue = 0;
            string dropText = "";
            if(skillID <= 0)
            {
                dropValue = 0;
                dropText = "none";
            }
            else
            {
                var currentOptions = petSkillDrop.options;

                if(currentOptions != null && currentOptions.Count > 0)
                {
                    for(var i = 0; i < currentOptions.Count; i++)
                    {
                        var option = currentOptions[i];

                        var optionText = option.text;
                        if(optionText == "none")
                        {
                            continue;
                        }

                        var splitData = optionText.Split("_");
                        if(int.Parse(splitData[0]) == skillID)
                        {
                            dropValue = i;
                            dropText = optionText;
                            break;
                        }
                    }
                }
            }
            petSkillDrop.captionText.text = dropText;
            petSkillDrop.value = dropValue;
        }

        void onPetSkillDropDownHideEvent()//장비 선택 드롭다운이 사라질때 현재 선택 인덱스 초기화용 item을 선택하면 onClickPartInfoDropDown 먼저 타고 그 다음에 옴
        {
            clickSkillSlotIndex = -1;

            if (petSkillDrop != null)
                SetVisiblePetSkillDrop(false);
        }

        void SetDropDownHideEvent()
        {
            if (petSkillDrop != null)
            {
                petSkillDrop.DestroyBlock = onPetSkillDropDownHideEvent;
            }

            if(petLevelDrop != null)
            {
                petLevelDrop.DestroyBlock = onPetLevelDropDownHideEvent;
            }
        }

        public void onClickPetSkillDropDown()//펫의 스킬 결정 시 - 기존 스킬과 겹칠 경우 return, 아니면 펫 데이터 갱신
        {
            if(petData == null)//petData가 없는데 클릭이 들어온다 - 버그
            {
                return;
            }

            var selectedText = petSkillDrop.captionText.text;
            if (selectedText == "none")//해당 tag 의 skill id값 삭제
            {
                petData.SkillsID[clickSkillSlotIndex] = 0;
            }
            else
            {
                var selectSkillID = int.Parse(selectedText.Split("_")[0]);
                var currentSkillId = petData.SkillsID[clickSkillSlotIndex];

                if (currentSkillId == selectSkillID)//이미 같은 데이터를 들고있음
                {
                    return;
                }

                petData.SkillsID[clickSkillSlotIndex] = selectSkillID;
            }

            User.Instance.PetData.AddPet(petData);//user Data에 반영

            RefreshPetData();//새로운 펫 데이터 갱신
            RefreshPetSkillSlot();

            //팝업 ForceUpdate 쳐서 능력치 갱신 추가
            PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
        }

        void RefreshPetLevelDropUI()
        {
            if(petData != null)
            {
                onShowLevelDrop();
            }
            else
            {
                SetVisiblePetLevelDrop(false);
            }
        }

        void onShowLevelDrop()//펫 레벨 드롭 다운 켜질 때 세팅해야할 것
        {
            SetPetLevelDropData();
            RefreshPetLevelDrop();
            SetVisiblePetLevelDrop(true);
        }

        void SetVisiblePetLevelDrop(bool _isVisible)
        {
            petLevelDrop.gameObject.SetActive(_isVisible);

            if(petPresetButton != null)//레벨 드롭 다운과 프리셋 저장 버튼은 종속 관계
            {
                petPresetButton.gameObject.SetActive(_isVisible);
            }
        }

        void SetPetLevelDropData()
        {
            if (petLevelDrop == null)
                return;

            List<string> stringList = new List<string>();
            var maxPetLevel = GameConfigTable.GetPetLevelMax(petData.Grade());
            for(var i = 1; i <= maxPetLevel; i++)
            {
                stringList.Add(i.ToString());
            }

            petLevelDrop.ClearOptions();
            petLevelDrop.AddOptions(stringList);
        }

        void RefreshPetLevelDrop()//"none"은 있을 수 없음
        {
            if(petData == null)
            {
                return;
            }

            var currentLevel = petData.Level;

            petLevelDrop.captionText.text = currentLevel.ToString();
            petLevelDrop.value = currentLevel - 1;
        }
        
        void onPetLevelDropDownHideEvent()
        {
            
        }

        public void onClickPetLevelDropDown()//펫의 레벨 결정 시 - 기존 스킬 세팅한 라벨 갱신해야함
        {
            if(petData == null)
            {
                return;
            }

            var selectIndex = petLevelDrop.captionText.text;
            var selectLevel = int.Parse(selectIndex);

            petData.SetLevel(selectLevel);
            petData.SetExp(petExpTable.GetCurrentAccumulateLevelExp(selectLevel, petData.Grade()));
            User.Instance.PetData.AddPet(petData);//user Data에 반영
            RefreshPetData();//펫 초상화 및 아이콘, userPet 갱신
            RefreshPetSkillSlot();//슬롯 라벨 변경 수치값 갱신

            //팝업 ForceUpdate 쳐서 능력치 갱신 추가
            PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
        }

        public void CustomPresetLoad(PresetPet _data)//프리셋 등록으로 받은 데이터 등록하기
        {
            if (_data == null)
            {
                ToastManager.On("유효하지 않은 프리셋입니다.");
                return;
            }
            //펫 생성 및 삽입
            SetPartPresetData(_data);

            RefreshPetData();//새로운 펫 데이터 갱신
            RefreshDragonPetDropOption();
            RefreshPetSkillSlot();
            RefreshPetLevelDropUI();// 펫 데이터가 있으면 레벨 드롭 박스 세팅 및 켜기

            //팝업 ForceUpdate 쳐서 능력치 갱신 추가
            PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
        }

        void SetPartPresetData(PresetPet petData)
        {
            var exp = petData.Exp;
            var skills = petData.Passives;
            var id = petData.PetID;
            var petBaseData = petTable.Get(id);
            var levelAndExpData = petExpTable.GetLevelAndExpByTotalExp(exp, petBaseData.GRADE);
            
            UserPet p = new UserPet(petTag, id, levelAndExpData["finallevel"], levelAndExpData[levelAndExpData["finallevel"].ToString()], 0, -1);
            p.SetSkillsID(skills.ToArray());

            User.Instance.PetData.AddPet(p);
        }
        void initPetPresetPanel()
        {
            if (presetPanel != null)
            {
                presetPanel.onClickClose();
            }
        }

        public void onClickShowPetPreset()//프리셋 세팅 팝업 불러오기
        {
            if (presetPanel != null)
            {
                presetPanel.onShowPanel(this);
            }
        }
        public void onClickSavePetPreset()//펫 프리셋 저장
        {
            if(petData == null)
            {
                ToastManager.On("펫을 먼저 세팅해주세요.");
                return;
            }

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002666), "해당 펫을 프리셋으로 저장할까요?",
                () => {
                    //ok
                    SavePresetData();//펫 프리셋 저장하기

                    ToastManager.On("프리셋 등록 완료!");
                }, 
                () => {
                    //cancel
                },
                () => {
                    //x
                }
            );

        }

        void SavePresetData()
        {
            if (petData == null)
            {
                return;
            }

            int pid = SimulatorPreset.GetPresetIDMax(ePreset.PET) + 1;
            var presetPet = new PresetPet(pid, "프리셋" + pid.ToString(), petData.ID, petData.Exp, petData.SkillsID);

            SimulatorPreset.ReadMyDocument();
            SimulatorPreset.ApplyPreset(presetPet);
            SimulatorPreset.SavePreset();
        }
    }
}

#endif