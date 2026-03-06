using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork
{
	public enum mainUIObjectType
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
		townedit,
		announcement,
		chat,
		hamburger,
		arenaPoint,
		friendPoint,
		guildPoint,
	}

	public class MainUIObject: UIObject, EventListener<UserStatusEvent>, EventListener<UIObjectEvent>, EventListener<PopupTopUIRefreshEvent>
	{
		[SerializeField] private Animation anim = null;

		[SerializeField] private GameObject hamburgerComponent = null;
		[SerializeField] private CanvasGroup canvasGroup = null;

		public delegate void callback();
		callback xButtonCallBack = null; // x 버튼을 통해서 타운이 아니라 다른 곳으로 보내거나 다른 행동을 하고 싶을때 사용할 것
		bool played = false;
		protected virtual void Start()
		{
			EventManager.AddListener<UserStatusEvent>(this);
			EventManager.AddListener<UIObjectEvent>(this);
			EventManager.AddListener<PopupTopUIRefreshEvent>(this);
		}

		protected virtual void OnDestroy()
		{
			EventManager.RemoveListener<UserStatusEvent>(this);
			EventManager.RemoveListener<UIObjectEvent>(this);
			EventManager.RemoveListener<PopupTopUIRefreshEvent>(this);
		}

		public void OnClickToTownButton()
		{
			//LoadingManager.ImmediatelySceneLoad("Town",true, eUIType.Town);

			if(xButtonCallBack != null)
			{
				xButtonCallBack.Invoke();
				ReleaseTownButtonCallBack();
				return;
			}
			LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
		}
		public void OnOffExitBtn(bool isAble)
        {
			if (isAble)
			{
				gameObject.SetActive(true);
				uiChildrens[(int)mainUIObjectType.gototown].gameObject.SetActive(true);

            }
			else { 
				uiChildrens[(int)mainUIObjectType.gototown].gameObject.SetActive(false);
            }
        }

		public void SetTownButtonCallBack(callback cb) // x 버튼을 통해서 타운이 아니라 다른 곳으로 보내거나 다른 행동을 하고 싶을때 사용할 것
        {
			xButtonCallBack = cb;
		}
		public void ReleaseTownButtonCallBack()
		{
			xButtonCallBack = null;

        }
		public void setArenaTimeCallBack(callback cb)
        {
			if (cb != null)
			{
				uiChildrens[(int)mainUIObjectType.arenaticket]?.GetComponent<UIArenaTicket>().setCallBack(() => { cb?.Invoke(); });
			}
		}
		public void RefreshTicket()
        {
			uiChildrens[(int)mainUIObjectType.arenaticket]?.GetComponent<UIArenaTicket>().RefreshTicketTime();

		}
		public void SetStaminaTimeCallBack(callback cb)
        {
			uiChildrens[(int)mainUIObjectType.energy]?.GetComponent<UIStaminaObject>().setCallBack(() => { cb?.Invoke(); });
		}

		public void RefreshArenaPoint()
		{
			uiChildrens[(int)mainUIObjectType.arenaPoint]?.GetComponent<UIArenaPointObject>().RefreshArenaPointCount();
        }
		
		public void OnClickPostButton()
		{
			PopupManager.OpenPopup<PostListPopup>();
		}

		public void OnClickSettingButton()
		{
			PopupManager.OpenPopup<SettingPopup>();
		}
		public void OnClickChattingButton()
		{
			PopupManager.OpenPopup<ChattingPopup>();
		}
		public void OnClickAnnouncementButton()//공지사항
		{
            PopupManager.OpenPopup<AnnouncePopup>();
        }

		public void OnClickUpdateGuideButton()
		{
			ToastManager.On(StringData.GetStringByStrKey("system_message_update_01"));
		}

		public void OnClickHamburgerBtn()
		{
			if (hamburgerComponent.activeInHierarchy)
			{
                hamburgerComponent.SetActive(false);
            }
			else
			{
                hamburgerComponent.SetActive(true);
            }
        }

		public void SetActiveChildObject(mainUIObjectType _type, bool _isVisible, bool _isRefresh = false)
        {
			if (uiChildrens == null || uiChildrens.Count <= 0)
				return;
			if (uiChildrens.Count <= (int)_type)
				return;

			var obj = uiChildrens[(int)_type];
			if(obj != null)
            {
				obj.gameObject.SetActive(_isVisible);

				if (_isRefresh)
					obj.RefreshUI();
			}
        }

		public void OnClickStore()
		{
            hamburgerComponent.SetActive(false);
            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData());
            //PopupManager.OpenPopup<StorePopup>(new StorePopupData("store", eStoreType.ARENA_POINT, eGoodType.GEMSTONE));
        }
        public void OnClickShop()
        {
            hamburgerComponent.SetActive(false);
            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData());
        }

        public void OnClickAttandence()
		{
			hamburgerComponent.SetActive(false);
			if (false == AttendancePopup.CheckAttendance())
				AttendancePopup.OpenPopup();
		}


		public void OnClickAdvertisement()
		{
			hamburgerComponent.SetActive(false);
			AdvertiseManager.Instance.TryADWithPopup((log) => { ToastManager.On(StringData.GetStringByStrKey("광고시청완료")); }, () => { ToastManager.On(StringData.GetStringByStrKey("ad_empty_alert")); });
		}
		public void OnClickPurchase()
		{
			hamburgerComponent.SetActive(false);
			IAPManager.Instance.TryPurchase(1, (response) => {
				if (((JObject)response).ContainsKey("rs"))
				{
					if (response["rs"].Value<int>() == 0)
					{
						ToastManager.On(StringData.GetStringByStrKey("결제완료"));
					}
					else
						ToastManager.On(StringData.GetStringByStrKey("결제오류"));
				}
				else
				{
					ToastManager.On(StringData.GetStringByStrKey("결제오류"));
				}
			}, (response) => { ToastManager.On(StringData.GetStringByStrKey("구매불가")); });
			
		}

		/// <summary>
		/// 아이템 번호를 넣어서 그 아이템의 카운트를 출력해주는 UI
		/// </summary>
		/// <param name="itemNo"></param>
		public void SetInvenItemUI(int itemNo)
		{
			uiChildrens[(int)mainUIObjectType.invenItemCount].GetComponent<UIInvenItemCountObj>().RefreshItem(itemNo);

        }

        public virtual void OnEvent(UserStatusEvent eventType)
        {
            switch(eventType.Event)
            {
				case UserStatusEvent.eUserStatusEventEnum.GOLD:
					uiChildrens[(int)mainUIObjectType.gold]?.RefreshUI(curUIType);
					break;
				case UserStatusEvent.eUserStatusEventEnum.GEMSTONE:
					uiChildrens[(int)mainUIObjectType.gemstone]?.RefreshUI(curUIType);
					break;
				case UserStatusEvent.eUserStatusEventEnum.MILEAGE:
					uiChildrens[(int)mainUIObjectType.mileage]?.RefreshUI(curUIType);
					break;
				case UserStatusEvent.eUserStatusEventEnum.EXP:
				case UserStatusEvent.eUserStatusEventEnum.ENERGY:
				case UserStatusEvent.eUserStatusEventEnum.ENERGY_TICK:
					uiChildrens[(int)mainUIObjectType.energy]?.RefreshUI(curUIType);
					break;
				case UserStatusEvent.eUserStatusEventEnum.FRIEND_POINT:
					if(uiChildrens.Count > (int)mainUIObjectType.friendPoint)
						uiChildrens[(int)mainUIObjectType.friendPoint]?.RefreshUI(curUIType);
					break;
				case UserStatusEvent.eUserStatusEventEnum.GUILD_POINT:
                    if (uiChildrens.Count > (int)mainUIObjectType.guildPoint)
                        uiChildrens[(int)mainUIObjectType.guildPoint]?.RefreshUI(curUIType);
                    break;

            }
        }

		public void OnEvent(UIObjectEvent eventType)
		{
			switch (eventType.e)
			{
				case UIObjectEvent.eEvent.EVENT_SHOW:
					anim.Stop();
					anim.Play("R_Show");
					break;

				case UIObjectEvent.eEvent.EVENT_HIDE:
					anim.Stop();
					anim.Play("R_Hide");
					break;
			}
		}

		public virtual void OnEvent(PopupTopUIRefreshEvent eventType)
		{
			if (eventType.justPopupTop)
				return;

			if (eventType.bOn && PopupManager.IsPopupOpening())
			{
				if(!played)
                {
					if(canvasGroup.alpha < 1.0f)
						played = true;
				}
				gameObject.SetActive(false);
			}
			else
			{
				gameObject.SetActive(true);
				if(played)
                {
					anim.Stop();
					anim.Play("R_Show");
					played = false;
				}
			}
		}


	}
}