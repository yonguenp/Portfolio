using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserPartData
    {
        protected Dictionary<int, UserPart> parts = new Dictionary<int, UserPart>();

        /**
         * @param tag 고유 번호
         * @returns 
         */
        public UserPart AddUserPart(JObject jsonData)
        {
            var tag = SBFunc.IsJTokenType(jsonData["equip_tag"], JTokenType.Integer) ? jsonData["equip_tag"].Value<int>() : -1;
            if (tag > 0)
            {
                var id = SBFunc.IsJTokenType(jsonData["equip_id"], JTokenType.Integer) ? jsonData["equip_id"].Value<int>() : -1;
                var obtain = SBFunc.IsJTokenType(jsonData["obtain"], JTokenType.Integer) ? jsonData["obtain"].Value<int>() : -1;
                var reinforce = SBFunc.IsJTokenType(jsonData["reinforce"], JTokenType.Integer) ? jsonData["reinforce"].Value<int>() : -1;
                var belongingDragonTag = -1;
                if (parts.ContainsKey(tag) && parts[tag] != null)
                    belongingDragonTag = parts[tag].LinkDragonTag;

                UserPart newPart = new UserPart(tag, id, obtain, belongingDragonTag, reinforce);
                if (SBFunc.IsJArray(jsonData["subs"]))
                    newPart.SetPartOptionList(jsonData["subs"]);

                if(SBFunc.IsJArray(jsonData["fusions"]))
                {
                    var array = JArray.FromObject(jsonData["fusions"]);
                    for (var i = 0; i < array.Count; i++)
                    {
                        var partJarrayData = array[i];
                        var optionData = JArray.FromObject(partJarrayData);
                        if (optionData == null)
                            continue;

                        var key = optionData[0].Value<int>();
                        var value = optionData[1].Value<float>();

                        newPart.SetFusionStat(key, value);
                    }
                }

                if (parts.ContainsKey(newPart.Tag))
                    parts[newPart.Tag] = newPart;
                else
                    parts.Add(newPart.Tag, newPart);

                return newPart;
            }

            return null;
        }

        public UserPart AddUserPart(UserPart partData)
        {
            if (partData == null || partData.Tag < 0)
                return null;

            var belongingDragonTag = -1;
            if (parts.ContainsKey(partData.Tag) && parts[partData.Tag] != null)
                belongingDragonTag = parts[partData.Tag].LinkDragonTag;

            UserPart newPart = new UserPart(partData.Tag, partData.ID, partData.Obtain, belongingDragonTag, partData.Reinforce);

            var partOptionList = partData.SubOptionList.ToList();
            for (var i = 0; i < partOptionList.Count; i++)
            {
                newPart.SetPartOption(partOptionList[i], i);
            }

            if (parts.ContainsKey(newPart.Tag))
                parts[newPart.Tag] = newPart;
            else
                parts.Add(newPart.Tag, newPart);

            return newPart;
        }

        public void DeleteUserPart(int tag)
        {
            parts.Remove(tag);
        }

        /**
         * 
         * @param tag 입력 형식이 기본 타입, 숫자 태그 배열로 전달 가능
         * @returns 검색 결과에 만족하는 형식으로 리턴 (단일 또는 배열)
         */
        public UserPart GetPart(int tag)
        {
            if (parts.ContainsKey(tag))
            {
                return parts[tag];
            }
            return null;
        }
        //id로 동일 종류 아이템 탐색
        public List<UserPart> GetPartListByID(int id)
        {
            var partsDataList = parts;
            List<int> keys = new List<int>(parts.Keys);
            List<UserPart> array = new List<UserPart>();

            keys.ForEach((element) =>
            {
                var userPart = partsDataList[element];
                if (userPart == null)
                {
                    return;
                }
                var userID = userPart.ID;
                if (userID == id)
                {
                    array.Add(userPart);
                }
            });
            return array;
        }

        public List<UserPart> GetAllUserParts()
        {
            var partsDataList = parts;
            List<int> keys = new List<int>(parts.Keys);
            List<UserPart> array = new List<UserPart>();

            keys.ForEach((element) =>
            {
                array.Add(partsDataList[element]);
            });
            return array;
        }

        public void SetPartLink(int partTag, int dragonTag)
        {
            if (!parts.ContainsKey(partTag))
            {
                if (partTag <= 0)
                {
                    return;
                }
                //태그 매치가 이루어지지 않음 = 심각한 문제
                // SBLog("[심각] 태그매치 불가능")
                return;
            }
            parts[partTag].SetLink(dragonTag);
        }

        //현재 파츠 태그로 링크된 드래곤 태그값 가져옴
        public int GetPartLink(int partTag)
        {
            return parts[partTag].LinkDragonTag;
        }

        public void ClearData()
        {
            parts.Clear();
        }
    }
}