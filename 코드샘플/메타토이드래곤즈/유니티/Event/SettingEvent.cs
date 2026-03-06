using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
	public struct SettingEvent
	{
		public enum eSettingEventEnum
		{
			REFRESH_STRING,
		}

		public eSettingEventEnum Event;
		static SettingEvent e;

		public SettingEvent(eSettingEventEnum _Event)
		{
			Event = _Event;
		}
		public static void RefreshString()
		{
			StringData.Clear();
			e.Event = eSettingEventEnum.REFRESH_STRING;
			EventManager.TriggerEvent(e);
		}
	}
}