using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChatBlockLayer : TabLayer
    {
        [Space(10f)]
        [Header("Prefab")]
        [SerializeField] ChattingOtherNode clone = null;

        [Header("ScrollView")]
        [SerializeField] ScrollRect blockScrollView = null;
        [Header("Empty")]
        [SerializeField] GameObject emptyNode = null;

        private List<BlockUserData> blocks = null;
        private List<ChattingOtherNode> nodes = new();

        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);

            blocks = ChatManager.Instance.GetBlockUserDataList();
            ScrollCreateItem();
        }
        public override string GetTitleText()
        {
            return string.Format("{0} ({1}/{2})", base.GetTitleText(), blocks == null ? 0 : blocks.Count, GameConfigTable.GetConfigIntValue("BLOCK_USER_COUNT"));
        }
        void ClearNode()
        {
            foreach (var node in nodes)
            {
                node.gameObject.SetActive(false);
            }
            if (emptyNode != null)
                emptyNode.SetActive(false);
        }
        void ScrollCreateItem()
        {
            if (clone == null)
                return;

            ClearNode();
            if (blocks == null || blocks.Count < 1)
            {
                if (emptyNode != null)
                    emptyNode.SetActive(true);
                return;
            }

            int i = 0;
            var it = blocks.GetEnumerator();
            while (it.MoveNext())
            {
                if (nodes.Count <= i)
                {
                    var obj = Instantiate(clone, blockScrollView.content);
                    nodes.Add(obj);
                }

                nodes[i].gameObject.SetActive(true);
                nodes[i].SetReturnTab(LayerIndex);
                nodes[i].Init(it.Current, eOtherNodeType.BLOCK_USER, CancleBlock);
                i++;
            }
        }
        void CancleBlock(ThumbnailUserData data)
        {
            if (data == null)
                return;

            var popup = PopupManager.OpenPopup<ChattingBlockAlarmPopup>();
            popup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002143));
            popup.SetUserNickName(data.Nick);
            popup.SetCallBack(() =>//ok
            {
                ChatManager.Instance.RemoveBlockUser(data.UID);
                blocks = ChatManager.Instance.GetBlockUserDataList();
                ChatEvent.RefreshChatUI();
                ScrollCreateItem();
            },
                () =>
                {//cancle
                },
                () =>
                {//exit
                }
            );
        }
        public void OnClickBack()
        {
            PopupManager.GetPopup<ChattingPopup>().ChangeTab(0);
        }
    }
}