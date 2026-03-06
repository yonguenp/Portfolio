using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PortraitEtcInfoData
    {
        public Dictionary<ePortraitEtcType , int> Portrait_Info { get; private set; } = new Dictionary<ePortraitEtcType, int>();

        public PortraitEtcInfoData(ePortraitEtcType _type = ePortraitEtcType.RAID, int _value = -1)
        {
            Init();
            Portrait_Info.Add(_type, _value);
        }

        public PortraitEtcInfoData(JToken _data)
        {
            Init();
            Portrait_Info = SBFunc.GetPortraitData(_data);
        }

        public void Init()
        {
            Clear();
        }

        public void Clear()
        {
            if (Portrait_Info == null)
                Portrait_Info = new Dictionary<ePortraitEtcType, int>();

            Portrait_Info.Clear();
        }

        public ePortraitEtcType GetDefaultType()
        {
            if (Portrait_Info == null)
                return ePortraitEtcType.NONE;

            var it = Portrait_Info.GetEnumerator();
            while (it.MoveNext())
            {
                return it.Current.Key;
            }

            return ePortraitEtcType.NONE;
        }
        public int GetValue(ePortraitEtcType _type)
        {
            if (Portrait_Info == null || Portrait_Info.Count <= 0)
                return 0;

            if (false == Portrait_Info.ContainsKey(_type))
                return 0;

            return Portrait_Info[_type];
        }

        public void UpdateInfo(JToken _jsonData)
        {
            if (Portrait_Info == null)
                Init();

            SBFunc.UpdatePortraitData(_jsonData, Portrait_Info);
        }

        public Sprite GetFrameSpriteByType(ePortraitEtcType _type)
        {
            if (Portrait_Info == null)
                return null;

            if (false == Portrait_Info.ContainsKey(_type))
                return null;

            return _type.GetPortraitSprite(Portrait_Info[_type]);
        }
        public void SetPortraitReward(Image userFrame, GameObject addNode, Image topAddImg, Image botAddImg, GameObject crowns = null)
        {
            Sprite _sprite = GetFrameSpriteByType(ePortraitEtcType.RAID);
            int crownCount = GetValue(ePortraitEtcType.CHAMPION);

            if (addNode != null)
            {
                addNode.SetActive(_sprite != null || crownCount > 0);
            }

            if (userFrame != null)
            {
                userFrame.sprite = _sprite;
                userFrame.type = _sprite != null ? Image.Type.Sliced : Image.Type.Simple;
                userFrame.color = _sprite != null ? Color.white : new Color(1, 1, 1, 0);
            }

            var spriteList = ePortraitEtcType.RAID.GetPortraitAddSprite(GetValue(ePortraitEtcType.RAID));
            if (spriteList == null || spriteList.Count != 2)
            {
                if (botAddImg != null)
                    botAddImg.gameObject.SetActive(false);
                if (topAddImg != null)
                    topAddImg.gameObject.SetActive(false);
            }
            else
            {
                if (botAddImg != null)
                {
                    botAddImg.gameObject.SetActive(true);
                    botAddImg.sprite = spriteList[0];
                }
                if (topAddImg != null)
                {
                    topAddImg.gameObject.SetActive(true);
                    topAddImg.sprite = spriteList[1];
                }
            }

            if (crowns != null)
            {
                crowns.SetActive(crownCount > 0);

                foreach (Transform crown in crowns.transform)
                {
                    crown.gameObject.SetActive(crownCount > 0);
                    crownCount--;
                }
            } 
        }
    }
}

