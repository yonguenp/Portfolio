using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eSystemMessageType
    {
        NORMAL,
        WISPER,
        GUILD
    }
    public class SystemMessage : MonoBehaviour
    {
        const int MAX_MESSAGE_QUEUE_COUNT = 10;
        const float MSG_DELAY = 0.45f;

        [SerializeField]
        Transform layoutParent = null;
        [SerializeField]
        SystemMessageSlot SlotPrefab = null;

        static SystemMessage Instance { get; set; } = null;
        SBListPool<SystemMessageSlot> MessageSlot { get; set; } = null;
        Queue<Tuple<eSystemMessageType, string, Action>> Queue { get; set; } = null;
        Coroutine coroutine = null;
        List<SystemMessageSlot> ActiveSlot { get; set; } = null;
        bool IsLoading { get; set; } = false;
        public static int Count => Instance == null ? 0 : Instance.Queue.Count;

        public void Init()
        {
            if (Instance != null)
                return;

            Instance = this;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            if (MessageSlot == null)
            {
                MessageSlot = new((slot) =>
                {
                    slot.gameObject.SetActive(true);
                }, (slot) =>
                {
                    slot.gameObject.SetActive(false);
                });
                MessageSlot.Put(SlotPrefab);
            }

            if (Queue == null)
                Queue = new();
            else
                Queue.Clear();

            if (ActiveSlot == null)
                ActiveSlot = new();

            SlotSpawn(3);
            IsLoading = true;
        }
        public static void SetLoadEnd()
        {
            if (Instance == null)
                return;

            Instance.IsLoading = false;
        }
        public void SlotSpawn(int count = 1)
        {
            while(MessageSlot.Count < count)
            {
                var slot = Instantiate(SlotPrefab, layoutParent);
                MessageSlot.Put(slot);
            }
        }
        public static void MessageEnd(SystemMessageSlot slot)
        {
            if (Instance == null || slot == null)
                return;

            Instance.ActiveSlot.Remove(slot);
            Instance.MessageSlot.Put(slot);
        }
        public static void PushMsg(string msg, Action action = null)
        {
            PushMsg(eSystemMessageType.NORMAL, msg, action);
        }
        public static void PushWisperMsg(ChatDataInfo msg)
        {
            PushMsg(eSystemMessageType.WISPER, SBFunc.ObjectsConvertLimitString(40, msg.SendNickname, " : ", msg.Comment), () =>
            {
                int returnTab;
                if (FriendManager.FriendIdList.ContainsKey(msg.SendUID))
                    returnTab = 2;
                else
                    returnTab = 3;
                PopupManager.OpenPopup<ChattingPopup>().SetDirectChat(msg, returnTab);
            });
        }
        public static void PushGuildMsg(ChatDataInfo msg)
        {
            PushMsg(eSystemMessageType.GUILD, SBFunc.ObjectsConvertLimitString(40, msg.SendNickname, " : ", msg.Comment), () =>
            {
                PopupManager.OpenPopup<ChattingPopup>().SetDirectGuildChatLayer();
            });
        }
        private static void PushMsg(eSystemMessageType eType, string msg, Action action = null)
        {
            if (Instance.IsLoading)
                return;

            if (Instance.Queue.Count > MAX_MESSAGE_QUEUE_COUNT)
                Instance.Queue.Dequeue();

            Instance.Queue.Enqueue(new(eType, msg, action));
            if (Instance.coroutine == null)
            {
                Instance.coroutine = Instance.StartCoroutine(Instance.MessageCoroutine());
            }
        }
        private IEnumerator MessageCoroutine()
        {
            while (Instance.Queue.Count > 0)
            {
                var data = Instance.Queue.Dequeue();
                SlotSpawn();
                var UI = MessageSlot.Get();
                UI.OnMessage(data.Item1, data.Item2, data.Item3);
                UI.transform.SetAsLastSibling();
                ActiveSlot.Add(UI);

                yield return SBDefine.GetWaitForSeconds(MSG_DELAY);
            }
            coroutine = null;
            yield break;
        }
        private void OnDisable()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            if(ActiveSlot != null)
            {
                for (int i = 0, count = ActiveSlot.Count; i < count; ++i)
                {
                    MessageEnd(ActiveSlot[i]);
                }
                ActiveSlot.Clear();
            }
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}