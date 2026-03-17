namespace SandboxNetwork
{
	public class LoginData
	{
		public eLoginResult loginResultState = eLoginResult.NONE;
		public eAuthAccount loginType = eAuthAccount.NONE;
		public string jwtToken = "";

		public string nick = "";
		public string binToken = "";
	}

	public struct LoginEvent
	{
		static LoginEvent e;

		// 기타 추가 정보가 있다면
		public LoginData loginData;

		public LoginEvent(eLoginResult _resultType, string _jwtToken, string _nick, string _binToken)
		{
			loginData = new LoginData();
			loginData.loginResultState = _resultType;
			loginData.jwtToken = _jwtToken;
			loginData.nick = _nick;
			loginData.binToken = _binToken;
		}

		public static void SendLoginResult(LoginData resultData)
        {
			e.loginData = resultData;
			EventManager.TriggerEvent(e);
		}
	}
}