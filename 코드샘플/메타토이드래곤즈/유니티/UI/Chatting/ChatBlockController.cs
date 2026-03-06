using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 채팅 차단 유저 리스트
 * chatManager에서 차단 목록 데이터 송/수신 만들어야함
 * 
 */
namespace SandboxNetwork
{
    public class ChatBlockController : MonoBehaviour
    {
        int maxUserBlockCount = 0;

        [SerializeField] GameObject chatBlockObject = null;
        [SerializeField] GameObject chatBlockPrefab = null;
        [SerializeField] ScrollRect chatBlockScrollRect = null;
        [SerializeField] Text blockUserCountLabel = null;

        private bool firstInit = false;

        private bool isShow = false;

        List<BlockUserData> blockUserList = new List<BlockUserData>();

        void FirstInit()
        {
            maxUserBlockCount = int.Parse(GameConfigTable.GetConfigValue("BLOCK_USER_COUNT"));
        }

        public void OnShowUI()
        {
            if (chatBlockObject != null)
                chatBlockObject.SetActive(true);

            isShow = true;
            if (firstInit == false)
            {
                FirstInit();
                firstInit = true;
            }

            RefreshBlockScrollData();
        }

        public void OnHideUI()
        {
            if (chatBlockObject != null)
                chatBlockObject.SetActive(false);
            isShow = false;
        }

        public void ToggleUI()
        {
            if (isShow)
                OnHideUI();
            else
                OnShowUI();
        }

        void RefreshBlockScrollData()//chatManager 차단 리스트 갱신해서 다시그리기
        {
            if (chatBlockScrollRect == null)
                return;

            SBFunc.RemoveAllChildrens(chatBlockScrollRect.content);

            blockUserList = ChatManager.Instance.GetBlockUserDataList();
            if (blockUserList == null || blockUserList.Count <= 0)
            {
                blockUserCountLabel.text = string.Format("({0}/{1})", 0, maxUserBlockCount);
                return;
            }

            for (int i = 0; i < blockUserList.Count; i++)
            {
                var userNick = blockUserList[i].Nick;
                if (userNick == "")
                    continue;

                AddScrollItem(userNick, blockUserList[i].UID, blockUserList[i].PortraitIcon);
            }

            RefreshBlockUserCount();
        }

        void RefreshBlockUserCount()//차단 유저 카운트 + chatmanager에서 현재 차단 유저 리스트 카운트 가져오기
        {
            if (blockUserCountLabel == null)
                return;

            blockUserCountLabel.text = string.Format("({0}/{1})", blockUserList.Count, maxUserBlockCount);
        }

        bool AddScrollItem(string _userNick, long id, string icon)
        {
            var userBlockObject = Instantiate(chatBlockPrefab, chatBlockScrollRect.content);
            var blockComp = userBlockObject.GetComponent<ChatBlockUserPrefab>();
            if (blockComp == null)
            {
                Destroy(userBlockObject);
                return false;
            }

            blockComp.SetData(_userNick, id, icon, RemoveScrollItem);
            return true;
        }

        void RemoveScrollItem(long _userID)
        {
            var childrens = SBFunc.GetChildren(chatBlockScrollRect.content.gameObject);
            if (childrens == null || childrens.Length <= 0)
                return;

            GameObject targetObject = null;
            for(int i = 0; i< childrens.Length; i++)
            {
                var child = childrens[i];
                if (child == null)
                    continue;

                var blockComp = child.GetComponent<ChatBlockUserPrefab>();
                if (blockComp == null)
                    continue;

                var UserID = blockComp.UserID;
                if(UserID == _userID)
                {
                    targetObject = child;
                    ToastManager.On(100002144, blockComp.UserNick);
                    break;
                }
            }

            if (targetObject != null)
            {
                ChatManager.Instance.RemoveBlockUser(_userID);
                Destroy(targetObject);                
            }
        }
    }
}
