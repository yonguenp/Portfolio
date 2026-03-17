using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorDragonEditPartController : MonoBehaviour
    {
        [SerializeField] private List<SimulatorDragonEditPartSlot> partSlotList = null;
        [SerializeField] private SBSimulatorDropDown partInfoDrop = null;//장비 기본 정보 드롭
        [SerializeField] private SBSimulatorDropDown partLevelDrop = null;//장비 레벨 드롭
        [SerializeField] private SBSimulatorDropDown partSubOptionDrop = null;//장비 부옵 드롭

        [SerializeField] private DragonPartTotalStatPanel statPanel = null;//장비 세트 효과 표시 팝업

        [SerializeField] private SimulatorPartPresetPanel presetPanel = null;//장비 프리셋 표시 패널

        private List<PartBaseData> totalPartData = null;
        private List<string> dropdownPartInfoList; //key_name 형태 (UI 및 '_' spilt 해서 key 체크)

        int currentDragonTag = -1;
        int currentPartSlotIndex = -1;//현재 클릭한 장비 슬롯 인덱스
        int currentPartSubOptionIndex = -1;//현재 클릭한 부옵 인덱스
        int currentDeckIndex = -1;//몇번째 드래곤을 선택했냐

        PartTable partTable = null;
        SubOptionTable partOptionTable = null;
        public void init(int dragonTag, int index)
        {
            if (partSlotList == null || partSlotList.Count <= 0)
            {
                return;
            }

            if (partTable == null)
            {
                partTable = TableManager.GetTable<PartTable>();
            }

            if(partOptionTable == null)
            {
                partOptionTable = TableManager.GetTable<SubOptionTable>();
            }

            SetPartInfoData();//장비 드롭다운 데이터 생성
            SetVisiblePartInfoDrop(false);//장비 드롭다운 끄기
            SetVisiblePartLevelDrop(false);//레벨 드롭다운 끄기
            SetVisiblePartSubOptionDrop(false);//부옵 드롭다운 끄기
            SetDropDownHideEvent();//드롭다운 hide event
            currentDragonTag = dragonTag;
            currentDeckIndex = index;

            initPartSlot();
            initPartPresetPanel();//프리셋 패널 켜져있음 끄기
            RefreshPartOptionDetailPanel();//상세창 갱신
        }

        void initPartSlot()
        {
            if(partSlotList == null || partSlotList.Count <= 0)
            {
                return;
            }

            var dragonData = User.Instance.DragonData.GetDragon(currentDragonTag);
            if(dragonData == null)
            {
                return;
            }

            var partList = dragonData.Parts.ToList();

            if(partList.Count != partSlotList.Count)
            {
                return;
            }

            for(var i = 0; i< partSlotList.Count; i++)
            {
                var slot = partSlotList[i];
                if(slot == null)
                {
                    continue;
                }
                slot.initSavePrevPart(partList[i]);//이전 장비 데이터 임시 저장
                slot.RefreshPartInfo(partList[i]);
            }
        }

        void RefreshPartOptionDetailPanel()
        {
            if(statPanel != null)
            {
                //partSlotList 리스트 기준 tag값으로 partData 찾기
                UserPart[] arrParts = new UserPart[6];

                for(var i = 0; i < partSlotList.Count; i++)
                {
                    var partTag = partSlotList[i].PartTag;

                    if(partTag<= 0)
                    {
                        continue;
                    }

                    var PartData = User.Instance.PartData.GetPart(partTag);
                    if(PartData != null)
                    {
                        arrParts[i] = PartData;
                    }
                }

                //팝업 ForceUpdate 쳐서 능력치 갱신 추가
                PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
            }
        }

        void SetDropDownHideEvent()
        {
            if(partInfoDrop != null)
            {
                partInfoDrop.DestroyBlock = onPartInfoDropDownHideEvent;
            }

            if (partLevelDrop != null)
            {
                partLevelDrop.DestroyBlock = onPartLevelDropDownHideEvent;
            }

            if(partSubOptionDrop != null)
            {
                partSubOptionDrop.DestroyBlock = onPartSubOptionDropDownHideEvent;
            }
        }

        void SetPartInfoData()
        {
            if(partTable != null && partInfoDrop != null)
            {
                totalPartData = partTable.GetAllList();

                if (dropdownPartInfoList == null)
                {
                    dropdownPartInfoList = new List<string>();
                }
                dropdownPartInfoList.Clear();

                dropdownPartInfoList.Add("none");

                for (int i = 0, count = totalPartData.Count; i < count; ++i)
                {
                    var value = totalPartData[i];
                    if (value == null)
                        continue;

                    var key = value.KEY;
                    var stat = value.STAT_TYPE.ToString();
                    var valueType = value.VALUE_TYPE.ToString();
                    var valueString = valueType == "PERCENT" ? "%" : "+"; 
                    var partValue = value.VALUE.ToString();
                    var partGrade = value.GRADE;

                    dropdownPartInfoList.Add(SBFunc.StrBuilder(key, "_", stat, "_", valueString, "_", partValue, "_",partGrade.ToString()));
                }
                partInfoDrop.ClearOptions();
                partInfoDrop.AddOptions(dropdownPartInfoList);
            }

        }

        void SetVisiblePartInfoDrop(bool _isVisible)
        {
            partInfoDrop.gameObject.SetActive(_isVisible);
        }

        void SetPartLevelData(int _index)//각 장비의 맥렙 값이 다르기때문에, 열 때마다 갱신 해줘야함
        {
            if(partLevelDrop == null)
            {
                return;
            }

            partLevelDrop.ClearOptions();

            if(partSlotList == null || partSlotList.Count <= 0)
            {
                return;
            }

            if (currentPartSlotIndex >= partSlotList.Count)
            {
                return;
            }

            var partTag = partSlotList[_index].PartTag;
            if(partTag <= 0)
            {
                return;
            }

            var partData = User.Instance.PartData.GetPart(partTag);
            if(partData == null)
            {
                return;
            }

            var partID = partData.ID;
            var partMaxLevel = partTable.GetMaxReinforceCount(partID);

            List<string> levelList = new List<string>();

            for(var i = 0; i <= partMaxLevel; i++)
            {
                levelList.Add(i.ToString());
            }

            partLevelDrop.AddOptions(levelList);
        }

        void SetPartSubOptionData(int _SubOptionindex)//각 장비의 그룹 (부옵 슬롯 넘버)에 따른 옵션이 다르게 나와야함
        {
            if (partSubOptionDrop == null)
            {
                return;
            }

            var subOptionList = partOptionTable.GetOptionByGroup(_SubOptionindex);

            List<string> subList = new List<string>();
            subList.Add("none");

            for (var i = 0; i < subOptionList.Count; i++)
            {
                var value = subOptionList[i];

                if (value == null)
                {
                    continue;
                }

                var key = value.KEY.ToString();
                var stat = value.STAT_TYPE.ToString();
                var valueType = value.VALUE_TYPE.ToString();
                var valueString = valueType == "PERCENT" ? "%" : "+";
                var partValue = value.VALUE_MAX.ToString();

                subList.Add(SBFunc.StrBuilder(key, "_", stat, "_", valueString, "_", partValue));
            }

            partSubOptionDrop.ClearOptions();
            partSubOptionDrop.AddOptions(subList);
        }

        void SetVisiblePartLevelDrop(bool _isVisible)
        {
            partLevelDrop.gameObject.SetActive(_isVisible);
        }
        void SetVisiblePartSubOptionDrop(bool _isVisible)
        {
            partSubOptionDrop.gameObject.SetActive(_isVisible);
        }

        public void onClickShowPartInfoDropDown(int _index)
        {
            currentPartSlotIndex = _index;
            RefreshPartInfoDropOption(_index);
            SetVisiblePartLevelDrop(false);
            SetVisiblePartSubOptionDrop(false);
            SetVisiblePartInfoDrop(true);
        }

        void RefreshPartInfoDropOption(int _index)//이전 옵션을 지우고 새로 그리는 상태라 이전 partData info값 따로 갱신
        {
            var partTag = partSlotList[_index].PartTag;

            string text = "";
            int value = 0;
            if(partTag <= 0)//미등록
            {
                text = "none";
                value = 0;
            }
            else
            {
                var partData = User.Instance.PartData.GetPart(partTag);
                var currentID = partData.ID;
                for(var i = 0; i< dropdownPartInfoList.Count; i++)
                {
                    var key = dropdownPartInfoList[i];
                    if(key == "none")
                    {
                        continue;
                    }

                    var splitList = key.Split("_");
                    
                    if(int.Parse(splitList[0]) == currentID)
                    {
                        value = i;
                        text = key;
                        break;
                    }
                }
            }

            partInfoDrop.captionText.text = text;
            partInfoDrop.value = value;
        }

        public void onClickShowPartLevelDropDown(int _index)
        {
            currentPartSlotIndex = _index;
            SetPartLevelData(_index);
            RefreshPartLevelDropOption(_index);
            SetVisiblePartInfoDrop(false);
            SetVisiblePartSubOptionDrop(false);
            SetVisiblePartLevelDrop(true);
        }

        void RefreshPartLevelDropOption(int _index)//이전 옵션을 지우고 새로 그리는 상태라 이전 partData level값 따로 갱신
        {
            var partTag = partSlotList[_index].PartTag;
            var partData = User.Instance.PartData.GetPart(partTag);
            var currentLevel = partData.Reinforce;

            partLevelDrop.captionText.text = currentLevel.ToString();
            partLevelDrop.value = currentLevel;
        }

        public void onClickShowPartSubOptionDropDown(string _slotIndex)//(현재 선택한 슬롯)_(부옵 슬롯 인덱스) string 형태
        {
            var splitData = _slotIndex.Split("_");

            currentPartSlotIndex = int.Parse(splitData[0]);
            currentPartSubOptionIndex = int.Parse(splitData[1]);

            SetPartSubOptionData(currentPartSubOptionIndex);

            RefreshPartSubOptionDropOption(currentPartSlotIndex, currentPartSubOptionIndex);

            SetVisiblePartInfoDrop(false);
            SetVisiblePartLevelDrop(false);
            SetVisiblePartSubOptionDrop(true);
        }

        void RefreshPartSubOptionDropOption(int _slotIndex , int _subOptionIndex)
        {
            var partTag = partSlotList[_slotIndex].PartTag;
            var partData = User.Instance.PartData.GetPart(partTag);

            var optionList = partData.SubOptionList;

            var optionIndex = _subOptionIndex - 1;//버튼 그룹 인덱스로 들어오기 때문에 1 줄여야함
            if(optionList == null || optionList.Count <= optionIndex)
            {
                return;
            }

            var suboptionData = optionList[optionIndex];

            int key = suboptionData.Key;
            float value = suboptionData.Value;

            string dropText = "";
            int dropValue = 0;
            if(partSubOptionDrop != null)
            {
                var dropDownOptionList = partSubOptionDrop.options;
                if(dropDownOptionList != null && dropDownOptionList.Count > 0)
                {
                    for(var i = 0; i < dropDownOptionList.Count;i++)
                    {
                        var option = dropDownOptionList[i];

                        if(option.text == "none")
                        {
                            continue;
                        }

                        var splitData = option.text.Split("_");

                        if(key == int.Parse(splitData[0]))
                        {
                            dropText = option.text;
                            dropValue = i;
                            break;
                        }
                    }
                }
            }

            partSubOptionDrop.captionText.text = dropText;
            partSubOptionDrop.value = dropValue;
        }

        void onPartInfoDropDownHideEvent()//장비 선택 드롭다운이 사라질때 현재 선택 인덱스 초기화용 item을 선택하면 onClickPartInfoDropDown 먼저 타고 그 다음에 옴
        {
            currentPartSlotIndex = -1;

            if (partInfoDrop != null)
                SetVisiblePartInfoDrop(false);
        }

        void onPartLevelDropDownHideEvent()
        {
            currentPartSlotIndex = -1;

            if (partLevelDrop != null)
                SetVisiblePartLevelDrop(false);
        }

        void onPartSubOptionDropDownHideEvent()
        {
            currentPartSlotIndex = -1;
            currentPartSubOptionIndex = -1;

            if (partSubOptionDrop != null)
                SetVisiblePartSubOptionDrop(false);
        }


        //선택한 드래곤 순서 (장비tag 값은 고유 번호라 강제로 넣어야해서, 임시적으로 드래곤 순서로 장비 값 세팅)
        /*
        * 장비 태그
        * 1번 드래곤 1~6 
        * 2번 드래곤 7~12
        * 3번 드래곤 13~18
        * 4번 드래곤 19~24
        * 5번 드래곤 25~30
        * 6번 드래곤 31~36
        */
        public void onClickPartInfoDropDown()//장비를 생성해서 넣어줘야함, tag(고유 생성번호), id가 table key
        {
            var selectedText = partInfoDrop.captionText.text;
            
            if(partSlotList == null || partSlotList.Count <= 0)
            {
                return;
            }

            if(currentPartSlotIndex >= partSlotList.Count)
            {
                return;
            }

            if (selectedText == "none")//장비 비우기
            {
                partSlotList[currentPartSlotIndex].RefreshPartInfo(0);//0 넣고 초기화
                var tag = 6 * currentDeckIndex + currentPartSlotIndex + 1;

                User.Instance.PartData.DeleteUserPart(tag);//등록한 파츠 지우기
            }
            else
            {
                var textSplit = selectedText.Split("_");
                if(textSplit.Length > 1)
                {
                    var partId = int.Parse(textSplit[0]);
                    var tag = 6 * currentDeckIndex + currentPartSlotIndex + 1;

                    var partData = User.Instance.PartData.GetPart(tag);
                    if(partData != null && partData.ID == partId)
                    {
                        return;
                    }

                    SetPartDefaultData(tag, partId);//새로운 partData 생성

                    partSlotList[currentPartSlotIndex].RefreshPartInfo(tag);//장비 갱신
                }
            }
            RefreshPartOptionDetailPanel();
        }

        void SetPartDefaultData(int tag, int id)//userPart 기본 데이터 생성하기
        {
            JObject newPartData = new JObject();
            newPartData.Add("equip_tag", tag);
            newPartData.Add("equip_id", id);
            newPartData.Add("level", 0);
            newPartData.Add("obtain", -1);

            User.Instance.PartData.AddUserPart(newPartData);
        }
        public void onClickPartLevelDropDown()//레벨 선택 시
        {
            var selectedText = partLevelDrop.captionText.text;
            var selectLevel = int.Parse(selectedText);
            var tag = 6 * currentDeckIndex + currentPartSlotIndex + 1;//현재 드래곤 태그값
            var partData = User.Instance.PartData.GetPart(tag);

            if(partData == null)
                return;

            partData.SetLevel(selectLevel);

            partSlotList[currentPartSlotIndex].RefreshPartInfo(tag);//0 넣고 초기화

            RefreshPartOptionDetailPanel();
        }

        public void onClickPartSubOptionDropDown()//subOption 선택 시 - 장비 데이터 안에 부옵 데이터 세팅
        {
            if (partSlotList == null || partSlotList.Count <= 0)
            {
                return;
            }

            if (currentPartSlotIndex >= partSlotList.Count)
            {
                return;
            }

            var tag = 6 * currentDeckIndex + currentPartSlotIndex + 1;//현재 드래곤 태그값
            var partData = User.Instance.PartData.GetPart(tag);

            if (partData == null)
            {
                return;
            }

            var selectedText = partSubOptionDrop.captionText.text;
            if (selectedText == "none")//해당 부옵 지우기
            {
                partData.DeletePartOption(currentPartSubOptionIndex - 1);
            }
            else//일단은 value max값으로 집어넣음
            {
                var splitStr = selectedText.Split("_");
                var key = int.Parse(splitStr[0]);

                var value = SubOptionData.Get(key).VALUE_MAX;
                
                partData.SetPartOption(new(key, value), currentPartSubOptionIndex - 1);//옵션 추가
            }

            partSlotList[currentPartSlotIndex].RefreshPartInfo(tag);//슬롯 갱신
            RefreshPartOptionDetailPanel();//상세정보 갱신
        }

        public List<UserPart> GetUserPartList()//현재 들고 있는 장비 데이터 가져오기
        {
            List<UserPart> tempPart = new List<UserPart>();
            tempPart.Clear();

            if(partSlotList == null || partSlotList.Count <= 0)
            {
                return tempPart;
            }

            for(var i = 0; i < partSlotList.Count; i++)
            {
                var userPartSlot = partSlotList[i];
                if(userPartSlot == null)
                {
                    continue;
                }

                var userPart = userPartSlot.Part;
                tempPart.Add(userPart);
            }

            return tempPart;
        }

        public int[] GetUserPartTagList()//현재 들고 있는 장비 tag리스트 가져오기
        {
            int[] tempPart = new int[6];

            if (partSlotList == null || partSlotList.Count <= 0)
            {
                return tempPart;
            }

            for (var i = 0; i < partSlotList.Count; i++)
            {
                var userPartSlot = partSlotList[i];
                if (userPartSlot == null)
                {
                    continue;
                }

                var userPart = userPartSlot.Part;
                if(userPart == null)
                {
                    continue;
                }

                var tag = userPart.Tag;
                tempPart[i] = tag;
            }

            return tempPart;
        }

        public int[] GetDumpAllPrevPart()
        {
            int[] tempPart = new int[6];
            if (partSlotList == null || partSlotList.Count <= 0)
            {
                return tempPart;
            }

            for (var i = 0; i < partSlotList.Count; i++)
            {
                var userPartSlot = partSlotList[i];
                if (userPartSlot == null)
                {
                    continue;
                }

                tempPart[i] = userPartSlot.dumpPrevPart();
            }
            return tempPart;
        }

        void initPartPresetPanel()
        {
            if (presetPanel != null)
            {
                presetPanel.onClickClose();
            }
        }

        public void onClickShowPartPreset()//프리셋 세팅 팝업 불러오기
        {
            if(presetPanel != null)
            {
                presetPanel.onShowPanel(this);
            }
        }

        public void CustomPresetLoad(PresetPart _data, int slotIndex)//프리셋 등록으로 받은 데이터 등록하기
        {
            if(_data == null)
            {
                ToastManager.On("유효하지 않은 프리셋입니다.");
                return;
            }

            var tag = 6 * currentDeckIndex + slotIndex;

            SetPartPresetData(tag, _data);//새로운 partPreset 생성

            partSlotList[slotIndex - 1].RefreshPartInfo(tag);//장비 갱신

            RefreshPartOptionDetailPanel();
        }

        void SetPartPresetData(int tag, PresetPart partData)
        {
            var level = partData.Level;
            var subs = partData.Subs;
            var id = partData.PartID;

            UserPart p = new UserPart(tag, id, -1, -1, level);

            if(subs != null && subs.Length > 0)
            {
                for (var i = 0; i < subs.Length; i++)
                {
                    var key = subs[i];
                    var value = partOptionTable.Get(key).VALUE_MAX;

                    p.SetPartOption(new(key, value), i);//옵션 추가
                }
            }

            User.Instance.PartData.AddUserPart(p);
        }

        public void onClickSavePreset(int _slotIndex)//현재 슬롯 데이터 가져와서 프리셋 저장 기능 만들기
        {
            if(partSlotList == null || partSlotList.Count <= 0)
            {
                return;
            }

            if(partSlotList.Count <= _slotIndex)
            {
                return;
            }

            var slotPartData = partSlotList[_slotIndex].Part;
            if(slotPartData == null)
            {
                return;
            }

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002666), SBFunc.StrBuilder("해당 " ,_slotIndex + 1, "번 슬롯의 내용을 프리셋으로 저장할까요?"),
                () => {
                    //ok
                    SavePresetData(slotPartData);

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

        void SavePresetData(UserPart _partData)
        {
            var partSubs = _partData.SubOptionList;

            int[] partOptionList = new int[partSubs.Count];

            for (var i = 0; i < partSubs.Count; i++)
            {
                partOptionList[i] = partSubs[i].Key;
            }

            int pid = SimulatorPreset.GetPresetIDMax(ePreset.PART) + 1;
            var presetPart = new PresetPart(pid, "프리셋" + pid.ToString(),_partData.ID,_partData.Reinforce, partOptionList);

            SimulatorPreset.ReadMyDocument();
            SimulatorPreset.ApplyPreset(presetPart);
            SimulatorPreset.SavePreset();
        }
    }
}


#endif