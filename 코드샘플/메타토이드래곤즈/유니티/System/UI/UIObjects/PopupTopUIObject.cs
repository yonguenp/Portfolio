using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct PopupEvent
    {
        private static PopupEvent obj;
        public bool popupOn;
        public static void Show()
        {
            obj.popupOn = true;
            EventManager.TriggerEvent(obj);
        }

        public static void Close()
        {
            obj.popupOn = false;
            EventManager.TriggerEvent(obj);
        }
    }
    public struct PopupTopUIRefreshEvent
    {
        private static PopupTopUIRefreshEvent obj;
        public bool bOn;
        public bool justPopupTop;//mainUIObject 에도 해당 이벤트를 받기때문에 popupTopUI만 끄고 싶을때 쓸려는 플래그.
        public static void Show(bool _justPopupTop = false)
        {
            obj.bOn = true;
            obj.justPopupTop = _justPopupTop;
            EventManager.TriggerEvent(obj);
        }

        public static void Hide(bool _justPopupTop = false)
        {
            obj.bOn = false;
            obj.justPopupTop = _justPopupTop;
            EventManager.TriggerEvent(obj);
        }
    }
    public enum ePopupTopUIObjectType
    {
        gold,
        cheercoin,
        gemstone,
        energy,
        mail,
        setting,
        gototown,
        arenaticket,
        invenItemCount,
        dragongachaticket,
        petgachaticket,
        mileage,
        arenapoint,
        magnet,
        friendpoint,
        guildpoint,
        magnite,
        sweepticket,
    }
    public class PopupTopUIObject : MainUIObject, EventListener<PopupTopUIRefreshEvent>, EventListener<UserStatusEvent>
    {
        [SerializeField]
        GameObject exitButton = null;
        eUIType targetUIType = eUIType.None;
        protected override void Start()
        {
            EventManager.AddListener<UserStatusEvent>(this);
            EventManager.AddListener<PopupTopUIRefreshEvent>(this);

            PopupTopUIRefreshEvent.Hide();
        }

        protected override void OnDestroy()
        {
            EventManager.RemoveListener<UserStatusEvent>(this);
            EventManager.RemoveListener<PopupTopUIRefreshEvent>(this);
        }

        public override void OnEvent(PopupTopUIRefreshEvent eventType)
        {
            if (eventType.bOn && PopupManager.IsPopupOpening())
            {
                gameObject.SetActive(true);
                SetTopUI();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public override void OnEvent(UserStatusEvent eventType)
        {
            base.OnEvent(eventType);
            switch (eventType.Event)
            {
                case UserStatusEvent.eUserStatusEventEnum.MAGNET:
                    RefreshMagnetUI();
                    break;
                case UserStatusEvent.eUserStatusEventEnum.FRIEND_POINT:
                    RefreshFriendPointUI();
                    break;
                case UserStatusEvent.eUserStatusEventEnum.GUILD_POINT:
                    RefreshGuildPointUI();
                    break;
            }
        }

        public override void InitUI(eUIType targetType)
        {
            targetUIType = targetType;
        }

        public void SetTopUI()
        {
            ReuseAnim();

            var count = uiChildrens.Count;
            for (var i = 0; i < count; ++i)
            {
                if (uiChildrens[i] == null)
                    continue;

                uiChildrens[i].InitUI(targetUIType);
            }

            PopupBase popup = PopupManager.GetFirstPopup();
            if (popup != null)
            {
                exitButton.SetActive(PopupManager.Instance.UsingExitButtonPopups.Contains(popup.GetType()));
            }


            transform.SetAsLastSibling();
        }

        public override bool RefreshUI(eUIType targetType)
        {
            return base.RefreshUI(targetType);
        }

        public void OnClickPopupOff()
        {
            PopupManager.GetFirstPopup().OnClickDimd();
            transform.SetAsLastSibling();
        }

        public void SetStaminaUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.energy);
            if (state)
                uiChildrens[type].InitUI(eUIType.Adventure);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }

        public void SetDiaUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.gemstone);
            if (state)
                uiChildrens[type].InitUI(eUIType.Town);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }
        public void SetGoldUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.gold);
            if (state)
                uiChildrens[type].InitUI(eUIType.Town);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }

        // UIObject 상속기능 안씀
        public void SetMagnetUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.magnet);
            if (state)
                uiChildrens[type].ShowEvent();
            else
                uiChildrens[type].HideEvent();
        }
        public void RefreshMagnetUI()
        {
            UIMagnetObject margetObject = uiChildrens[getUIObjectTypeToInt(ePopupTopUIObjectType.magnet)] as UIMagnetObject;
            margetObject?.RefreshCount();
        }
        // UIObject 상속기능 안씀
        public void SetFriendPointUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.friendpoint);
            if (state)
                uiChildrens[type].ShowEvent();
            else
                uiChildrens[type].HideEvent();
        }
        public void RefreshFriendPointUI()
        {
            var index = getUIObjectTypeToInt(ePopupTopUIObjectType.friendpoint);
            if (uiChildrens.Count < index)
                return;

            (uiChildrens[index] as UIFriendPointObject)?.RefreshCount();
        }
        public void SetGuildPointUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.guildpoint);
            if (state)
                uiChildrens[type].ShowEvent();
            else
                uiChildrens[type].HideEvent();
        }
        public void RefreshGuildPointUI()
        {
            var index = getUIObjectTypeToInt(ePopupTopUIObjectType.guildpoint);
            if (uiChildrens.Count < index)
                return;

            (uiChildrens[index] as UIGuildPointObject)?.RefreshGuildPointCount();
        }
        public void SetMileageUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.mileage);
            if (state)
                uiChildrens[type].InitUI(eUIType.Gacha);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }
        public void SetArenaPointUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.arenapoint);
            if (state)
                uiChildrens[type].InitUI(eUIType.Arena);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }
        public void SetArenaTicketUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.arenaticket);
            if (state)
                uiChildrens[type].InitUI(eUIType.Arena);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }
        public void SetCheerCoinUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.cheercoin);
            if (state)
                uiChildrens[type].InitUI(eUIType.ChampionBattle);
            else
                uiChildrens[type].InitUI(eUIType.None);
        }

        int getUIObjectTypeToInt(ePopupTopUIObjectType _type)
        {
            return (int)_type;
        }

        public void SetMagniteUI(bool state)
        {
            var type = getUIObjectTypeToInt(ePopupTopUIObjectType.magnite);
            if (state)
                uiChildrens[type].ShowEvent();
            else
                uiChildrens[type].HideEvent();
        }
    }
}


