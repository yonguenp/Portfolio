using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserPetData
    {
        private Dictionary<int, UserPet> pet = new Dictionary<int, UserPet>();

        /**
         * @param tag 고유 번호
         * @returns 
         * "{ "pet_tag": 12, "pet_id": 5040004, "exp": 0, "level": 1, "reinforce": 0, "unique_skill": 0, "normal_skills": [ 5040005, 5040004, 5040005, 5040029, 5040013 ], "obtain_at": 1680059478 }"
         */

        public int[] NewOptNeedReinforceVal 
        {
            get
            {
                if (newOptionNeedReinforceValue == null)
                {
                    newOptionNeedReinforceValue= new int[4] { GameConfigTable.GetConfigIntValue("PET_OPTION1_REINFORCE"), GameConfigTable.GetConfigIntValue("PET_OPTION2_REINFORCE"), GameConfigTable.GetConfigIntValue("PET_OPTION3_REINFORCE"), GameConfigTable.GetConfigIntValue("PET_OPTION4_REINFORCE") };
                }
                return newOptionNeedReinforceValue;
            }
        }
        private int[] newOptionNeedReinforceValue= null;
            
            
        readonly private string[] petDataKey =
        {
            "pet_tag", "pet_id", "exp", "level", "reinforce", "stats", "obtain_at"
        };

        int count_lv30 = 0;
        int count_lv40 = 0;
        int count_lv50 = 0;

        public void RefreshUserPet(JToken jsonToken)
        {
            if (jsonToken.Type != JTokenType.Object)
                return;

            JObject jsonData = jsonToken.ToObject<JObject>();
            UserPet p = null;

            var isContainKey = petDataContainKeyCheck(jsonData, petDataKey);

            if (jsonData.Type == JTokenType.Object)
            {
                if (isContainKey)
                {
                    int tag = jsonData["pet_tag"].Value<int>();
                    int petID = jsonData["pet_id"].Value<int>();
                    int petEXP = jsonData["exp"].Value<int>();
                    int petLevel = jsonData["level"].Value<int>();
                    int petReinforce = jsonData["reinforce"].Value<int>();
                    int petReinforceFalseCount = 0; // 테스트
                    if (SBFunc.IsJTokenCheck(jsonData["reinforce_failed"]))
                        petReinforceFalseCount = jsonData["reinforce_failed"].Value<int>();
                    JArray[] petStats = jsonData["stats"].Select(jv => (JArray)jv).ToArray();
                    JArray petSubs = null;
                    if(SBFunc.IsJTokenCheck(jsonData["subs"]))
                        petSubs = JArray.FromObject(jsonData["subs"]);

                    int petObtains = jsonData["obtain_at"].Value<int>();

                    if (!pet.ContainsKey(tag))
                    {
                        p = new UserPet(tag, petID, petLevel, petEXP, petReinforce, petObtains, petReinforceFalseCount);
                        pet.Add(tag, p);

                        if (petLevel >= 30)
                        {
                            count_lv30++;
                        }
                        if (petLevel >= 40)
                        {
                            count_lv40++;
                        }
                        if (petLevel >= 50)
                        {
                            count_lv50++;
                        }
                    }
                    else
                    {
                        p = pet[tag];
                        if (p.Level < 30 && petLevel >= 30)
                        {
                            count_lv30++;
                        }
                        if (p.Level < 30 && petLevel >= 40)
                        {
                            count_lv40++;
                        }
                        if (p.Level < 30 && petLevel >= 50)
                        {
                            count_lv50++;
                        }

                        p.SetBaseData(tag, petID, petLevel, petEXP, petReinforce, petObtains, petReinforceFalseCount);
                    }

                    p.SetStats(petStats, petSubs);
                }
            }
        }

        bool petDataContainKeyCheck(JObject _data, string[] _keyList)
        {
            foreach(string key in _keyList)
            {
                if (!_data.ContainsKey(key))
                    return false;
            }

            return true;
        }

        public void AddPet(UserPet _petData)//simulator - add
        {
            int tag = _petData.Tag;
            if (!pet.ContainsKey(tag))
            {
                pet.Add(tag, _petData);
            }
            else
            {
                pet[tag].SetBaseData(_petData.Tag, _petData.ID, _petData.Level, _petData.Exp, _petData.Reinforce, _petData.Obtain, _petData.ReinforceFalseCount);
            }

            pet[tag].SetUniqueSkillID(_petData.UniqueSkillID);
            pet[tag].SetSkillsID(_petData.SkillsID);
        }



        public void DeleteUserPet(int tag)
        {
            if (pet.ContainsKey(tag))
            {
                var p = pet[tag];
                if (p != null)
                {
                    if (p.Level >= 30)
                    {
                        count_lv30--;
                    }
                    if (p.Level >= 40)
                    {
                        count_lv40--;
                    }
                    if (p.Level >= 50)
                    {
                        count_lv50--;
                    }
                }
            }
            pet.Remove(tag);
        }

        /**
         * 
         * @param tag 입력 형식이 기본 타입, 숫자 태그 배열로 전달 가능
         * @returns 검색 결과에 만족하는 형식으로 리턴 (단일 또는 배열)
         */
        public UserPet GetPet(int tag)
        {
            if (pet.ContainsKey(tag))
            {
                return pet[tag];
            }

            return null;
        }

        public List<UserPet> GetAllUserPets()
        {
            Dictionary<int, UserPet> petDataList = pet;
            List<int> keys = new List<int>(pet.Keys);
            List<UserPet> array = new List<UserPet>();

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                array.Add(petDataList[key]);
            }

            return array;
        }

        public void SetPetLink(int petTag, int dragonTag)
        {
            if (!pet.ContainsKey(petTag))
            {
                if (petTag <= 0)
                {
                    return;
                }
                //태그 매치가 이루어지지 않음 = 심각한 문제
                Debug.Log("[심각] 태그매치 불가능");

                return;
            }
            pet[petTag].SetLinkDragonTag(dragonTag);
        }

        //현재 파츠 태그로 링크된 드래곤 태그값 가져옴
        public int GetPetLink(int petTag)
        {
            if (pet.ContainsKey(petTag))
            {
                return pet[petTag].LinkDragonTag;
            }
            return -1;
        }

        public void ClearData()
        {
            pet.Clear();
        }
    }
}