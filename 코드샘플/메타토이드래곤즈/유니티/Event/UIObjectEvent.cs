using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public struct UIObjectEvent
	{
		[Flags]
		public enum eUITarget
		{
			NONE = 0,
			LT = 1,
			MT = 2,
			RT = 4,
			LM = 8,
			M = 16,
			RM = 32,
			LB = 64,
			MB = 128,
			RB = 256,
			HAMBURGER = 512,
			ALL = LT | MT | RT | LM | M | RM | LB | MB | RB
        }

		public enum eEvent
		{
			EVENT_SHOW,
			EVENT_HIDE,
			PRODUCT_DONE,
			ITEM_GET,
			ITEM_CHECK,
			ITEM_USE,

			REFRESH_COLLECTION_REDDOT,
			REFRESH_ACHIEVEMENT_REDDOT,
			REFRESH_BADGE,

			REFRESH_STAMINA
		}

		public eEvent e;
		public eUITarget t;

		public UIObjectEvent(eEvent _event, eUITarget _target)
		{
			e = _event;
			t = _target;
		}

		static public void Event(eEvent _event, eUITarget _target = eUITarget.ALL)
		{
			switch (_event)
			{
				case eEvent.EVENT_SHOW:
					EventManager.TriggerEvent(new UIObjectEvent(eEvent.EVENT_SHOW, _target));
					break;
				case eEvent.EVENT_HIDE:
					EventManager.TriggerEvent(new UIObjectEvent(eEvent.EVENT_HIDE, _target));
					break;
				case eEvent.ITEM_GET:
                    EventManager.TriggerEvent(new UIObjectEvent(eEvent.ITEM_GET, _target));
					break;
                case eEvent.ITEM_CHECK:
                    EventManager.TriggerEvent(new UIObjectEvent(eEvent.ITEM_CHECK, _target));
                    break;
                case eEvent.ITEM_USE:
                    EventManager.TriggerEvent(new UIObjectEvent(eEvent.ITEM_USE, _target));
                    break;
				case eEvent.REFRESH_COLLECTION_REDDOT:
					EventManager.TriggerEvent(new UIObjectEvent(eEvent.REFRESH_COLLECTION_REDDOT, _target));
					break;
				case eEvent.REFRESH_ACHIEVEMENT_REDDOT:
					EventManager.TriggerEvent(new UIObjectEvent(eEvent.REFRESH_ACHIEVEMENT_REDDOT, _target));
					break;
				case eEvent.REFRESH_BADGE:
                    EventManager.TriggerEvent(new UIObjectEvent(eEvent.REFRESH_BADGE, _target));
                    break;
				case eEvent.REFRESH_STAMINA:
                    EventManager.TriggerEvent(new UIObjectEvent(eEvent.REFRESH_STAMINA, _target));
                    break;

            }
		}
	}
}
