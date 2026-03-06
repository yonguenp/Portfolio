using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 유저 기본 초상화 구성
    /// </summary>
    public class UserPortraitFrame : MonoBehaviour
    {
        [SerializeField] Image gradeBGImage = null;
        [SerializeField] Image characterIconImage = null;
        [SerializeField] Sprite defaultIcon = null;
        [SerializeField] Sprite defaultCustomImage = null;
        [SerializeField] GameObject levelNode = null;
        [SerializeField] Text levelLabel = null;
        [SerializeField] Image frame = null;
        [SerializeField] Image userFrame = null;//꾸미기 (보상용) 프레임
        [SerializeField] GameObject addNode = null;//꾸미기 (보상용) 프레임 추가 노드
        [SerializeField] Image topAddImg = null;
        [SerializeField] Image botAddImg = null;
        [SerializeField] GameObject crowns = null;

        protected CharBaseData baseData = null;
        public virtual void SetUserPortraitFrame(string dragonID, int _level = 0, bool _justLevel = true, PortraitEtcInfoData _portraitData = null)
        {
            SetData(dragonID);
            SetIcon(baseData);
            SetBG();
            SetLevel(_level, _justLevel);
            if (_portraitData != null)
                _portraitData.SetPortraitReward(userFrame, addNode, topAddImg, botAddImg, crowns);
            else
                DefaultPortraitReward();
        }
        public virtual void SetUserPortraitFrame(ThumbnailUserData data, bool _justLevel = true)
        {
            if (data == null) return;
            SetUserPortraitFrame(data.PortraitIcon, data.Level, _justLevel, data.EtcInfo);
        }
        protected virtual void SetBG()
        {
            if (gradeBGImage != null)
                gradeBGImage.sprite = defaultCustomImage;
        }

        protected void SetData(string _dragonID)
        {
            if(string.IsNullOrEmpty(_dragonID))
                baseData = null;
            else
            {
                if (int.TryParse(_dragonID, out int parseIntDragonID))//WJ - "mtdz_cap" 터짐 이슈 수정
                    baseData = CharBaseData.Get(parseIntDragonID);
                else
                    baseData = null;
            }
        }

        protected void DefaultPortraitReward()
        {
            if (userFrame != null)
            {
                userFrame.sprite = null;
                userFrame.type = Image.Type.Simple;
                userFrame.color = new Color(1, 1, 1, 0);
            }

            if (addNode != null)
                addNode.SetActive(false);
        }

        protected virtual void SetIcon(CharBaseData _baseData)
        {
            if (characterIconImage == null)
                return;

            if (baseData != null)
                characterIconImage.sprite = baseData.GetThumbnail();
            else
                characterIconImage.sprite = defaultIcon;
        }

        protected virtual void SetLevel(int _level, bool _justLevel = true)
        {
            if (levelNode != null)
                levelNode.SetActive(_level > 0);
            if(levelLabel != null)
                levelLabel.text = _justLevel ? _level.ToString() : string.Format("Lv. {0}", _level);
        }


        public void InitPortrait(bool _isOwn = false)
        {
            if(_isOwn)
            {
                var userData = User.Instance.UserData;
                SetUserPortraitFrame(userData.UserPortrait, userData.Level, false, userData.UserPortraitFrameInfo);
            }
            else
                SetUserPortraitFrame(_isOwn ? User.Instance.UserData.UserPortrait : "");
        }

        //유저 월드레이드 보상세팅용 프레임.
        //public void SetPortraitReward(PortraitInfoData _infoData)
        //{
        //    Sprite _sprite = null;
        //    if(_infoData != null)
        //        _sprite = _infoData.GetFrameSpriteByType(ePortraitType.RAID);

        //    var hasData = _sprite != null;
        //    if (userFrame != null)
        //    {
        //        userFrame.sprite = _sprite;
        //        userFrame.type = hasData ? Image.Type.Sliced : Image.Type.Simple;
        //        userFrame.color = hasData ? Color.white : new Color(1, 1, 1, 0);
        //    }

        //    if (addNode != null)
        //    {
        //        addNode.SetActive(hasData);

        //        if(hasData)
        //        {
        //            var spriteList = ePortraitType.RAID.GetPortraitAddSprite(_infoData.GetValue(ePortraitType.RAID));
        //            if (spriteList == null || spriteList.Count <= 0)
        //                return;

        //            if(spriteList.Count == 2)
        //            {
        //                if (botAddImg != null)
        //                    botAddImg.sprite = spriteList[0];
        //                if (topAddImg != null)
        //                    topAddImg.sprite = spriteList[1];
        //            }
        //        }
        //    }
        //}
    }
}