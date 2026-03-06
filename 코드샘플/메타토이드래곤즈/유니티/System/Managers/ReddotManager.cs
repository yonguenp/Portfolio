using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eReddotEvent
    {
        NONE = -1,
        POST_MAIL,
        SHOP,
        GACHA,
        COLLECTION,
        ACHIVEMENT,
        INVENTORY_NOTUSE,
        TOWN,
        CONSTRUCTION,
        PRODUCT,
        MINING,
        DAILY_QUEST,
        MAGIC_SHOWCASE,
        BATTLE_PASS,
        HOLDER_PASS,
        FRIEND,
        GUILD_MISSION,
        GUILD_WISPER,
        GUILD_CHAT,
        GUILD_MISSION_DAILY,
        GUILD_MISSION_WEEKLY,
        GUILD_MISSION_CHAIN
    }

    public class ReddotManager
    {
        static ReddotManager instance = null;
        public static ReddotManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ReddotManager();
                    instance.InitManager();
                }
                return instance;
            }
        }

        Dictionary<eReddotEvent, bool> dicState = new Dictionary<eReddotEvent, bool>();
        Dictionary<eReddotEvent, Action> dicCheckFunc = new Dictionary<eReddotEvent, Action>();
        void InitManager()
        {
            Clear();
        }

        public static void Set(eReddotEvent type, bool state, bool isSendEvent = true)
        {
            Instance.SetState(type, state, isSendEvent);
        }
        private void SetState(eReddotEvent type, bool state, bool isSendEvent)
        {
            if (dicState.ContainsKey(type))
            {
                dicState[type] = state;
            }
            else
            {
                dicState.Add(type, state);
            }

            if (isSendEvent)
            {
                ReddotEvent.SendReddotEvent(type, state);
            }
        }

        public static bool IsOn(eReddotEvent reddotTypeKey)
        {
            return Instance.GetState(reddotTypeKey);
        }

        private bool GetState(eReddotEvent reddotTypeKey)
        {
            if (!dicState.ContainsKey(reddotTypeKey))
                return false;

            return dicState[reddotTypeKey];
        }

        void Clear()
        {
            dicState.Clear();
        }
    }
}