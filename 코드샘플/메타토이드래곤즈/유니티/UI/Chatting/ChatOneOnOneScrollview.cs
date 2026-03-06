using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChatOneOnOneScrollview : ChatScrollview
    {
        public void SetTargetUno(long uno)
        {
            if (uno > 0)
            {
                targetUID = uno;
            }
        }

        //public override void DataRefresh(eChatCommentType type)
        //{
        //    curType = type;
        //    var curScrollPos = GetCurrentScrollPosition();
        //    Queue<ChatDataInfo> chattingData = CurQueue;

        //    ClearScrollView();
        //    switch (type)
        //    {
        //        case eChatCommentType.World:
        //        case eChatCommentType.Guild:
        //        case eChatCommentType.Whisper:
        //            var warningItem = MakeFirstAddWarningItem();
        //            warningItem.SetUI(AddChatScrollItem(warningItem));
        //            ChatDataList.Add(warningItem);
        //            break;
        //        default:
        //            break;
        //    }

        //    if (chattingData == null)
        //        return;

        //    if (targetUID > 0)
        //    {
        //        if (ChatManager.Instance.GetBlockUserData(targetUID) != null)
        //        {
        //            return;
        //        }
        //    }

        //    foreach (var item in chattingData)
        //    {
        //        ChatUIItem chatUI = new ChatUIItem();
        //        chatUI.SetData(item);
        //        ChatDataList.Add(chatUI);
        //        chatUI.SetUI(AddChatScrollItem(chatUI));

        //        lastData = item;
        //    }

        //    if (ChatDataList == null || ChatDataList.Count <= 0)
        //    {
        //        lastData = null;
        //        return;
        //    }

        //    if (ChatDataList.Count > ChatManager.CHAT_QUEUE_MAX_SIZE)
        //    {
        //        int delCount = ChatDataList.Count - ChatManager.CHAT_QUEUE_MAX_SIZE;
        //        for (int i = 0; i < delCount; i++)
        //        {
        //            if (false == TypePool.Put(ChatDataList[i].chatUIType, ChatDataList[i].ui.gameObject))
        //                Destroy(ChatDataList[i].ui.gameObject);
        //        }
        //        ChatDataList.RemoveRange(0, delCount);
        //    }

        //    LayoutRebuilder.ForceRebuildLayoutImmediate(scrollviewContent.GetComponent<RectTransform>());
        //    if (curScrollPos < NewMessageCheckValue)
        //    {
        //        GotoScrollBottom();
        //    }
        //}
    }
}
