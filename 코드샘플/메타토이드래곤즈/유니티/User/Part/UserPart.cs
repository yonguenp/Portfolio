using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserPart
    {
        //itemTag - 고유번호
        public int Tag { get; protected set; } = -1;
        //itemTag - 파츠 아이템 번호 - PartTable과 PartOptionTable의 키
        public int ID { get; private set; } = -1;
        //획득 시간
        public int Obtain { get; private set; } = -1;
        //장착했다면 장비한 드래곤 태그
        public int LinkDragonTag { get; private set; } = -1;
        //장비 레벨
        public int Reinforce { get; private set; } = 0;
        //부가 옵션 리스트
        public List<KeyValuePair<int, float>> SubOptionList { get; private set; } = new ();

        public bool IsFusion { get { return FusionStatKey > 0; } }
        public int FusionStatKey { get; private set; } = -1;
        public float FusionStatValue { get; private set; } = 0.0f;

        protected PartBaseData partDesignData = null;
        protected ItemBaseData itemDesignData = null;

        public UserPart(int tag, int id, int obtain, int linkDragonTag, int level)
        {
            Tag = tag;
            ID = id;
            Obtain = obtain;
            SetLink(linkDragonTag);
            SetLevel(level);
        }

        public void SetLevel(int value)
        {
            Reinforce = value;
        }

        public void SetLink(int value)
        {
            LinkDragonTag = value;
        }

        public void SetPartOption(KeyValuePair<int, float> option, int index)//simulator-add
        {
            if (SubOptionList == null)
                SubOptionList = new();

            if (SubOptionList.Count <= index)
            {
                var goalSize = index - SubOptionList.Count + 1;

                for (var i = 0; i < goalSize; i++)
                {
                    SubOptionList.Add(default);
                }

                SubOptionList[index] = option;
            }
            else
            {
                SubOptionList[index] = option;
            }
        }

        public void DeletePartOption(int index)//simulator-delete
        {
            if (SubOptionList == null)
                return;

            if (SubOptionList.Count <= index)
                return;

            SubOptionList.RemoveAt(index);
        }


        public void SetPartOptionList(JToken partList)
        {
            if (SubOptionList == null)
                SubOptionList = new ();
            else
                SubOptionList.Clear();

            var array = JArray.FromObject(partList);
            for (var i = 0; i < array.Count; i++)
            {
                var partJarrayData = array[i];
                var optionData = JArray.FromObject(partJarrayData);
                if (optionData == null)
                    continue;

                var key = optionData[0].Value<int>();
                var value = optionData[1].Value<float>();

                SubOptionList.Add(new(key, value));
            }
        }

        public void SetFusionStat(int key, float value)
        {
            FusionStatKey = key;
            FusionStatValue = value;
        }

        public PartBaseData GetPartDesignData()
        {
            if (partDesignData == null)
            {
                partDesignData = PartBaseData.Get(ID);
            }

            return partDesignData;
        }

        public ItemBaseData GetItemDesignData()
        {
            if (itemDesignData == null)
            {
                itemDesignData = GetPartDesignData().ITEM;
            }

            return itemDesignData;
        }

        public Sprite ICON_SPRITE
        {
            get
            {
                if (GetItemDesignData() == null)
                {
                    return null;
                }

                return GetItemDesignData().ICON_SPRITE;
            }
        }

        public int Grade()
        {
            if (GetItemDesignData() == null)
            {
                return -1;
            }

            return GetItemDesignData().GRADE;
        }

        public string Name()
        {
            if (GetItemDesignData() == null)
            {
                return "";
            }

            return GetItemDesignData().NAME;
        }

        public string Desc()
        {
            if (GetItemDesignData() == null)
            {
                return "";
            }

            return GetItemDesignData().DESC;
        }

        /*
         * return param
         * HP: number, ATK: number, DEF: number, CRI: number, HP_PER: number, ATK_PER: number, DEF_PER: number, CRI_PER: number
         */
        public PartStatus GetALLStat()
        {
            PartStatus stat = new PartStatus();
            stat.Initialze();
            stat.IncreaseStatus(this);
            stat.IncreaseStatus(SBFunc.GetSubOption(SubOptionList));
            if (IsFusion)
            {
                var data = PartFusionData.Get(FusionStatKey);
                if (data != null)
                {
                    stat.IncreaseStatus(data, FusionStatValue);
                }
            }
            return stat;
        }

        public float GetValue()
        {
            float value = GetPartDesignData().VALUE + (GetPartDesignData().VALUE_GROW * Reinforce);

            if (value == (int)value)
            {
                return value;
            }
            else
            {
                return Mathf.Round(value * 100f) * 0.01f;
            }
        }
    }
}