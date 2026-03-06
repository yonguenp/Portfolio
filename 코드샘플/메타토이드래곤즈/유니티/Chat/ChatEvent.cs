namespace SandboxNetwork
{
	public struct ChatEvent
	{
		public enum eChatEventEnum
		{
			SetTotalData,//init
			AddData,//api - push
			RemoveData,//delete


			RefreshUI,
			SendMacro,
			SystemMessage,
			WhisperRes,

			SocketConnected,		// 소켓 연결 성공
			SocketTryConnect,		// 소켓 연결 시도중
			SocketDisconnected,		// 소켓 연결이 끊어진 경우
		}

		public eChatEventEnum Event;
		static ChatEvent e;

		public eChatCommentType type;//refreshUI 요청 시 타입 체크용
		public long TargetUID { get; private set; }
		public string comment;//일단은 매크로 전용

		public ChatEvent(eChatEventEnum _Event, eChatCommentType _type, string _comment)
		{
			Event = _Event;
			type = _type;
			comment = _comment;
			TargetUID = -1;
		}

		public static void RefreshChatUI(int _index)
		{
			e.type = (eChatCommentType)_index;
			e.Event = eChatEventEnum.RefreshUI;
			EventManager.TriggerEvent(e);
		}
		public static void RefreshChatUI()
		{
			e.Event = eChatEventEnum.RefreshUI;
			EventManager.TriggerEvent(e);
		}
		public static void SendChatMacro(string _comment)
		{
			e.Event = eChatEventEnum.SendMacro;
			e.comment = _comment;
			EventManager.TriggerEvent(e);
		}

		public static void SendSystemMessege(string _comment)
		{
			e.Event = eChatEventEnum.SystemMessage;
			e.comment = _comment;
			EventManager.TriggerEvent(e);
		}

		// 소켓 연결 상태와 관련된 이벤트
		public static void SendSocketConnectState(eChatEventEnum state)
        {
			e.Event = state;
			EventManager.TriggerEvent(e);
		}
		public static void SendWhisperRes(long targetUID)
		{
			e.Event = eChatEventEnum.WhisperRes;
			e.TargetUID = targetUID;
			EventManager.TriggerEvent(e);
		}
	}
}