using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class MainMenuObject: UIObject, EventListener<UIObjectEvent>
	{
		[SerializeField] private Animation anim = null;
		[SerializeField] private HotTimeController hottimeObject = null;

		bool isRequestArenaData = false;

		public override void Init()
		{
			base.Init();

			EventManager.AddListener(this);
		}

		public void OnClickGachaButton()
		{
			//SBFunc.MoveScene(null, SBFunc.GACHA_SCENE_NAME);
			SBFunc.MoveGachaScene();
		}
		public void OnClickDragonButton()
		{
			DragonManagePopup.OpenPopup(0);
		}
		public void OnClickPartButton()
		{
			DragonManagePopup.OpenPopup(1);
		}
		public void OnClickPetButton()
		{
			DragonManagePopup.OpenPopup(2);
		}
		public void OnClickQuestIcon()
		{
			PopupManager.OpenPopup<MissionPopup>(new TabTypePopupData(0, 0));
		}
		public void OnClickArenaButton()
        {

			if (User.Instance.DragonData.GetAllUserDragons().Count <= 0)
			{
				ToastManager.On(100000623);
				return;
			}

            if (isRequestArenaData)
            {
				return;
            }

			isRequestArenaData = true;

			//입장전 미리 arena 데이터 요청
			ArenaManager.Instance.ReqArenaData(() =>
			{
				//LoadingManager.ImmediatelySceneLoad("ArenaLobby");
				LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation);
				isRequestArenaData = false;
			}, () =>
			{
				ToastManager.On(100002516);
				isRequestArenaData = false;
				//LoadingManager.ImmediatelySceneLoad("Town", true, eUIType.Town);//임시
			});

			//LoadingManager.ImmediatelySceneLoad("ArenaLobby");//임시
			//LoadingManager.ImmediatelySceneLoad("AdventureReward");//임시
			//LoadingManager.ImmediatelySceneLoad("ArenaResult");//임시
			
		}

		public void OnClickStore()
		{
			PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData());
		}
		public void OnClickDugeonButton()
		{
            if (User.Instance.DragonData.GetAllUserDragons().Count <= 0)
            {
                ToastManager.On(100000623);
                return;
            }
            //LoadingManager.Instance.EffectiveSceneLoad("AdventureStageSelect", eSceneEffectType.LightFadeOut);
            PopupManager.OpenPopup<DungeonSelectPopup>();
        }


        public void OnClickProductManageButton()
		{
			if (User.Instance.GetAllProducesList(true).Count <= 0)
            {
				ToastManager.On(100003229);
				return;
			}

			PopupManager.OpenPopup<ProductManagePopup>();
		}

		public void OnEvent(UIObjectEvent eventType)
		{
			if ((eventType.t & UIObjectEvent.eUITarget.RB) != UIObjectEvent.eUITarget.NONE)
			{
				switch (eventType.e)
				{
					case UIObjectEvent.eEvent.EVENT_SHOW:
						anim.Play("B_Show");
						break;

					case UIObjectEvent.eEvent.EVENT_HIDE:
						anim.Play("B_Hide");
						break;
					case UIObjectEvent.eEvent.REFRESH_COLLECTION_REDDOT:
						RefreshCollectionReddot();
						break;
				}
			}
		}

		void RefreshCollectionReddot()
        {
			((UIDragonIconObject)uiChildrens[1]).CheckButtonStates();
		}
	}
}