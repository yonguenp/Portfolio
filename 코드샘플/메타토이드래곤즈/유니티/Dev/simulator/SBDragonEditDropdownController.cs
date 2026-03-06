using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

#if DEBUG

namespace SandboxNetwork
{
    public class SBDragonEditDropdownController : MonoBehaviour
    {
        [SerializeField] private List<TMPro.TMP_Dropdown> dragonDrops;
        private Dictionary<string, CharBaseData> totalBaseData = null;
        private List<string> dropdownOptionList; //key_name 형태 (UI 및 '_' spilt 해서 key 체크)

        private bool isLoaded = false;
        public bool IsLoaded { set { isLoaded = value; }}

        private SimulatorBattleLine battleLine = new SimulatorBattleLine();
        public void init(SimulatorBattleLine battleLine)
        {
            initCharbaseData();
            initDragonDropDown();
            SetBattleLine(battleLine);
            SetDragonDropByTableData();
        }
        
        //데이터 테이블 세팅 끝나면 돌리기 (체크 로직 추가해야함)
        void initCharbaseData()
        {
            if (totalBaseData == null)
            {
                totalBaseData = CharBaseData.GetAllDic();
            }
        }

        void initDragonDropDown()
        {
            if (dragonDrops == null || dragonDrops.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < dragonDrops.Count; i++)
            {
                var drop = dragonDrops[i];
                if (drop == null)
                {
                    continue;
                }

                drop.ClearOptions();
            }
        }

        void SetBattleLine(SimulatorBattleLine _battleLine)
        {
            battleLine = _battleLine;
        }

        void SetDragonDropByTableData()
        {
            if (dragonDrops == null || dragonDrops.Count <= 0)
            {
                return;
            }

            if (totalBaseData == null)
            {
                return;
            }

            var keyStringList = totalBaseData.Keys.Select(i => i.ToString()).ToList();
            var valueList = totalBaseData.Values.ToList();

            if (keyStringList.Count != valueList.Count)
            {
                return;
            }

            if (dropdownOptionList == null)
            {
                dropdownOptionList = new List<string>();
            }
            dropdownOptionList.Clear();

            dropdownOptionList.Add("none");

            for (var i = 0; i < keyStringList.Count; i++)
            {
                var key = keyStringList[i];
                var value = valueList[i];

                if (value == null)
                {
                    continue;
                }

                var nameIndex = value._NAME;
                var name = StringData.GetStringByStrKey(nameIndex);

                dropdownOptionList.Add(SBFunc.StrBuilder(key, "_", name));
            }

            for (var i = 0; i < dragonDrops.Count; i++)
            {
                var drop = dragonDrops[i];
                if (drop == null)
                {
                    continue;
                }

                drop.AddOptions(dropdownOptionList);
            }
        }

        public void onClickDropDownMenu(int index)
        {
            if (battleLine == null)
            {
                return;
            }

            var checkLabel = dragonDrops[index].captionText.text;
            if(checkLabel == "none")
            {
                SBSimulatorEvent.deleteDragonUI(index);
            }
            else
            {
                var isSwitchingCondition = battleLine.IsChangePossibleCondition(index);
                var isFull = battleLine.IsDeckFull();

                if(isFull && !isSwitchingCondition && !isLoaded)//그냥 덱이 풀 인상태
                {
                    dragonDrops[index].captionText.text = "none";
                    dragonDrops[index].value = 0;

                    ToastManager.On("덱에 모든 드래곤 등록했슴다");
                    return;
                }

                var clickData = checkLabel.Split("_");
                if (clickData != null && clickData.Length > 1)
                {
                    var tag = clickData[0];
                    UserDragon dragonData = null;
                    if (isLoaded)
                    {
                        dragonData = User.Instance.DragonData.GetDragon(int.Parse(tag));
                    }
                    else
                    {
                        var prevDragonCheck = User.Instance.DragonData.GetDragon(int.Parse(tag));
                        if (prevDragonCheck != null)
                        {
                            dragonDrops[index].captionText.text = "none";
                            dragonDrops[index].value = 0;

                            ToastManager.On("덱에 중복 드래곤은 불가능해요");
                            return;
                        }

                        dragonData = SetDefaultDragon(int.Parse(tag));
                    }

                    SBSimulatorEvent.drawDragonUI(dragonData, index);
                }

                //if (isFull && !isLoaded)
                //{
                //    dragonDrops[index].captionText.text = "none";
                //    dragonDrops[index].value = 0;

                //    ToastManager.Set("덱에 모든 드래곤 등록했슴다");
                //}
                //else
                //{
                //    var clickData = checkLabel.Split("_");
                //    if (clickData != null && clickData.Length > 1)
                //    {
                //        var tag = clickData[0];
                //        UserDragon dragonData = null;
                //        if(isLoaded)
                //        {
                //            dragonData = User.Instance.DragonData.GetDragon(int.Parse(tag));
                //        }
                //        else
                //        {
                //            var prevDragonCheck = User.Instance.DragonData.GetDragon(int.Parse(tag));
                //            if (prevDragonCheck != null)
                //            {
                //                dragonDrops[index].captionText.text = "none";
                //                dragonDrops[index].value = 0;

                //                ToastManager.Set("덱에 중복 드래곤은 불가능해요");
                //                return;
                //            }

                //            dragonData = SetDefaultDragon(int.Parse(tag));
                //        }
                        
                //        SBSimulatorEvent.drawDragonUI(dragonData, index);
                //    }
                //}
            }
        }

        UserDragon SetDefaultDragon(int dragonTag)//기본 드래곤 (펫x , 장비 x)데이터 세팅
        {
            var getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
            var userdragonTempData = new UserDragon();
            if (getdragonTempData != null)
            {
                userdragonTempData = getdragonTempData;
            }

            userdragonTempData.SetBaseData(dragonTag, eDragonState.Normal, 0, 1, 1, -1);
            //parts 기준으로 link 세팅
            Array.ForEach(userdragonTempData.Parts, (element) => {
                var partTag = element;
                User.Instance.PartData.SetPartLink(partTag, dragonTag);
            });

            userdragonTempData.SetPartSetEffectOption();//부옵 계산
            User.Instance.PetData.SetPetLink(-1, dragonTag);

            return userdragonTempData;
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
        public void onClickDragonEditButton(int index)
        {
            var checkLabel = dragonDrops[index].captionText.text;
            if (checkLabel == "none")
            {
                ToastManager.On("덱에 드래곤을 먼저 등록하세요");
                return;
            }

            var clickData = checkLabel.Split("_");
            if (clickData != null && clickData.Length > 1)
            {
                var tag = clickData[0];

                PopupManager.OpenPopup<SimulatorDragonEditPopup>(new SimulatorDragonPopupData(tag, index));
            }
        }

        public void RefreshDragonDrops(int slotIndex, int dragonTag)//slotindex = 드래곤 덱 인덱스 = 드롭다운 슬롯 인덱스 , dragonTag == 0 이면 none 처리
        {
            if (dragonDrops == null || dragonDrops.Count <= 0)
            {
                return;
            }

            if (dragonDrops.Count <= slotIndex)
            {
                return;
            }

            var dropText = "";
            var dropValue = 0;

            var dropOptions = dragonDrops[slotIndex].options;
            if (dragonTag == 0)
            {
                dropText = "none";
                dropValue = 0;
            }
            else
            {
                for (var i = 0; i < dropOptions.Count; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    var text = dropOptions[i].text;
                    var splitData = text.Split("_");

                    if(int.Parse(splitData[0]) == dragonTag)
                    {
                        dropText = text;
                        dropValue = i;
                        break;
                    }
                }
            }
            dragonDrops[slotIndex].captionText.text = dropText;
            dragonDrops[slotIndex].value = dropValue;
        }
    }
}


#endif