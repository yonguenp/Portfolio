using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	[ExecuteAlways]
	public static class EventManager
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void InitPlayMode()
		{
			if (_subscribersList != null)
			{
				_subscribersList.Clear();
			}
		}

		private static Dictionary<Type, List<EventListenerBase>> _subscribersList;

		static EventManager()
		{
			_subscribersList = new Dictionary<Type, List<EventListenerBase>>();
		}

		public static void AddListener<T>(EventListener<T> listener) where T : struct
		{
			Type eventType = typeof(T);

			if (!_subscribersList.ContainsKey(eventType))
				_subscribersList[eventType] = new List<EventListenerBase>();

			if (!SubscriptionExists(eventType, listener))
				_subscribersList[eventType].Add(listener);
		}

		public static void RemoveListener<T>(EventListener<T> listener) where T : struct
		{
			Type eventType = typeof(T);

			if (!_subscribersList.ContainsKey(eventType))
			{
#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
#else
				return;
#endif
			}

			List<EventListenerBase> subscriberList = _subscribersList[eventType];
			bool listenerFound;
			listenerFound = false;

			if (listenerFound)
			{

			}

			for (int i = 0; i < subscriberList.Count; i++)
			{
				if (subscriberList[i] == listener)
				{
					subscriberList.Remove(subscriberList[i]);
					listenerFound = true;

					if (subscriberList.Count == 0)
						_subscribersList.Remove(eventType);

					return;
				}
			}

#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
#endif
		}

		public static void TriggerEvent<T>(T newEvent) where T : struct
		{
			List<EventListenerBase> list;
			if (!_subscribersList.TryGetValue(typeof(T), out list))
#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( Event ).ToString() ) );
#else
				return;
#endif

			for (int i = 0; i < list.Count; i++)
			{
				(list[i] as EventListener<T>).OnEvent(newEvent);
			}
		}

		private static bool SubscriptionExists(Type type, EventListenerBase receiver)
		{
			List<EventListenerBase> receivers;

			if (!_subscribersList.TryGetValue(type, out receivers)) return false;

			bool exists = false;

			for (int i = 0; i < receivers.Count; i++)
			{
				if (receivers[i] == receiver)
				{
					exists = true;
					break;
				}
			}

			return exists;
		}
	}
	public static class EventRegister
	{
		public delegate void Delegate<T>(T eventType);

		public static void EventStart<EventType>(this EventListener<EventType> caller) where EventType : struct
		{
			EventManager.AddListener(caller);
		}

		public static void EventStop<EventType>(this EventListener<EventType> caller) where EventType : struct
		{
			EventManager.RemoveListener(caller);
		}
	}
}