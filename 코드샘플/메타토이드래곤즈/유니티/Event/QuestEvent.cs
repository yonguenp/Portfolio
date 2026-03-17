using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public struct QuestEvent
	{
		public enum eEvent
		{
			QUEST_UPDATE,
			QUEST_OPEN,
			QUEST_REQUEST_SYNC,
			QUEST_REQUEST_REFRESH,
			QUEST_START,
			QUEST_DONE,
			TUTORIAL_QUEST_CHECK,
			TUTORIAL_QUEST_OPEN,
		}

		public eEvent e;
		public int eventQID;

		public QuestEvent(eEvent _Event, int qids = 0)
		{
			e = _Event;
			eventQID = qids;
		}

		static public void Event(eEvent _event, int qids = 0)
		{
			switch (_event)
			{
				case eEvent.QUEST_UPDATE:
					EventManager.TriggerEvent(new QuestEvent(eEvent.QUEST_UPDATE));
					break;
				case eEvent.QUEST_OPEN:
					EventManager.TriggerEvent(new QuestEvent(eEvent.QUEST_OPEN));
					break;
				case eEvent.QUEST_REQUEST_SYNC:
					EventManager.TriggerEvent(new QuestEvent(eEvent.QUEST_REQUEST_SYNC));
					break;
				case eEvent.QUEST_START:
					EventManager.TriggerEvent(new QuestEvent(eEvent.QUEST_START, qids));
					break;
				case eEvent.QUEST_DONE:
					EventManager.TriggerEvent(new QuestEvent(eEvent.QUEST_DONE, qids));
					break;
				case eEvent.QUEST_REQUEST_REFRESH:
					EventManager.TriggerEvent(new QuestEvent(eEvent.QUEST_REQUEST_REFRESH));
					break;
				case eEvent.TUTORIAL_QUEST_CHECK:
                    EventManager.TriggerEvent(new QuestEvent(eEvent.TUTORIAL_QUEST_CHECK));
                    break;
                case eEvent.TUTORIAL_QUEST_OPEN:
                    EventManager.TriggerEvent(new QuestEvent(eEvent.TUTORIAL_QUEST_OPEN));
                    break;
            }
		}
	}
}
