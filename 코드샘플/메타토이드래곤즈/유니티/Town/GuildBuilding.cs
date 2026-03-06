using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class GuildBuilding : LandmarkBuilding, EventListener<GuildEvent>
    {
        [SerializeField]
        SkeletonAnimation GuildFrontSpine;
        [SerializeField]
        SkeletonAnimation GuildBackSpine;
        [SerializeField]
        GameObject guildDestroyingAlarmBox = null;
        [SerializeField]
        Transform dragonForm = null;
        [SerializeField]
        Transform canvas = null;

        bool isGuildUpdateAble = false;
        GuildDragonMachine machine = null;
        protected override void Start()
        {
            base.Start();
            SetData();
            guildDestroyingAlarmBox.SetActive(false);

            if (machine == null)
                machine = new();

            machine.Initialize(dragonForm != null ? dragonForm : transform, canvas);
            RefreshState();
        }
        protected override void OnDestroy()
        {
            machine.Destory();
            base.OnDestroy();
        }

        void SetData()
        {
            WWWForm form = new WWWForm();
            GuildManager.Instance.NetworkSend("guild/state", form);


            //친구 요청 최초 1회 호출 - 길드원들 상태 친구 체크
            if (false == FriendManager.IsLoaded)
                FriendManager.Instance.FriendList();
        }

        public void RefreshState()
        {

            GuildFrontSpine.AnimationState.SetAnimation(0, "closed", true);
            GuildBackSpine.AnimationState.SetAnimation(0,"closed",true);
            //guildObj.SetActive(false);
            //guildDisableObj.SetActive(true);
            locked.SetActive(true);
            int conditionLv = GameConfigTable.GetConfigIntValue("GUILD_OPEN_USER_LEVEL", 10);
            if (GuildManager.Instance.GuildWorkAble == false)
            {
                return;
            }
            if (User.Instance.UserData.Level >= conditionLv)
            {
                locked.SetActive(false);
                if (GuildManager.Instance.IsNoneGuild || GuildManager.Instance.LastGuildLeaveState != eGuildLeaveState.None)
                {
                    GuildFrontSpine.AnimationState.SetAnimation(0, "open", true);
                    GuildBackSpine.AnimationState.SetAnimation(0, "closed", true);
                    //Data.SetState(eBuildingState.NOT_BUILT);
                    guildDestroyingAlarmBox.SetActive(false);
                }
                else
                {
                    GuildFrontSpine.AnimationState.SetAnimation(0, "open", true);
                    GuildBackSpine.AnimationState.SetAnimation(0, "open", true);
                    // Data.SetState(eBuildingState.NORMAL);
                    guildDestroyingAlarmBox.SetActive(GuildManager.Instance.IsDestroying);
                }
            }
        }

        
        public override void OnClickLandmark()
        {
            if (GuildManager.Instance.GuildWorkAble == false)
            {
                ToastManager.On(StringData.GetStringByStrKey("system_message_update_01"));
                return;
            }
            int conditionLv = GameConfigTable.GetConfigIntValue("GUILD_OPEN_USER_LEVEL", 10);
            if (User.Instance.UserData.Level < conditionLv)
            {
                ToastManager.On(StringData.GetStringFormatByStrKey("guild_desc:103", conditionLv));
                return;
            }
            //if (Data != null)
            //{
            //    switch (Data.State)
            //    {
            //        case eBuildingState.CONSTRUCT_FINISHED:
            //            BuildCompleteEvent.Send(this, Data.State);
            //            return;

            //        case eBuildingState.LOCKED:
            //            //ToastManager.On("임시 길드 가입 불가");
            //            //return;
            //        case eBuildingState.NONE:
            //        case eBuildingState.NOT_BUILT:
            //        case eBuildingState.NORMAL:
            //        default:
            //            break;
            //    }
            //}
            GuildManager.Instance.OpenGuild();
        }
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        protected override void OnDisable()
        {
            EventManager.RemoveListener(this);
            base.OnDisable();
        }
        public void OnEvent(GuildEvent eventType)
        {
            RefreshState();
        }
        protected override void Update()
        {
            base.Update();
            machine?.Update(SBGameManager.Instance.DTime);
        }
        public void RefreshTownDragon()
        {
            if (machine == null)
                return;

            machine.RefreshDragons();
        }
    }
}