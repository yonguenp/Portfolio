using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 차단 유저 리스트 스크롤뷰 세팅용 아이템 프리펩
 * 닉네임, 버튼
 * 버튼 -> 확인 누르면 ChatBlockController에 refresh요청
 */

namespace SandboxNetwork
{
    public class ChatBlockUserPrefab : MonoBehaviour
    {
        [SerializeField] Text nickNameLabel = null;
        [SerializeField] Button blockReleaseButton = null;
        [SerializeField] UserPortraitFrame portrait = null;
        public string UserNick { get; private set; }
        public long UserID { get; private set; }

        public delegate void func(long _releaseUserID);
        private func clickCallback = null;
        public func ClickCallback { set { clickCallback = value; } }

        public void SetData(string _nickName, long id, string icon, func _successCallback)
        {
            portrait.SetUserPortraitFrame(icon);
            UserID = id;
            SetNickName(_nickName);
            if (_successCallback != null)
                clickCallback = _successCallback;
        }

        void SetNickName(string _nickName)
        {
            if (nickNameLabel != null)
            {
                UserNick = _nickName;
                nickNameLabel.text = _nickName;
            }
        }

        public void OnClickRemoveBlockUser()
        {
            //ChatManager.Instance.RemoveBlockUser()
            clickCallback?.Invoke(UserID);
        }
    }
}
