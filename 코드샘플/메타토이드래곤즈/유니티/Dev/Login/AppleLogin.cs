using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

namespace SandboxNetwork
{
    public class AppleLogin : MonoBehaviour
    {
		protected LoginData loginResultData = null;

		protected bool isInitLogin = false;
		protected bool isProceedLogin = false;    // 로그인 처리 진행중 여부 체크

		public void OnAppleSignIn()
		{
			Debug.Log("Apple - OnAppleSignIn Enter before");
			if (loginResultData == null || string.IsNullOrWhiteSpace(loginResultData.jwtToken))
			{
				loginResultData.loginResultState = eLoginResult.ERROR_INVALID_TOKEN;
				isProceedLogin = false;
				return;
			}
			Debug.Log("Apple - OnAppleSignIn Enter");
			loginResultData.loginType = eAuthAccount.APPLE;

			var data = new WWWForm();
			data.AddField("id_tok", loginResultData.jwtToken);

			NetworkManager.Send("auth/apple", data, (response) => {
				isProceedLogin = false;
				if (SBFunc.IsJTokenCheck(response["state"]))
				{
					switch (response["state"].Value<int>())
					{
						case (int)eLoginUserState.NONE://new account
							loginResultData.loginResultState = eLoginResult.OK_NEW_ACCOUNT;
							break;
						case (int)eLoginUserState.NORMAL://has account
							loginResultData.loginResultState = eLoginResult.OK_HAS_ACCOUNT;
							UpdateLoginData(response);
							break;

						case (int)eLoginUserState.TO_BE_DELETED:
							loginResultData.loginResultState = eLoginResult.ERROR_TOBE_DELETED_ACCOUNT;
							if (SBFunc.IsJTokenCheck(response["state_update_at"]))
							{
								DateTime time = SBFunc.DateTimeParse(response["state_update_at"].Value<string>()).AddDays(1);
								SystemPopup.OnSystemPopup( StringData.GetStringByStrKey("탈퇴철회"), StringData.GetStringFormatByStrKey("탈퇴철회메시지", time.ToString()), () =>
								{
									WWWForm param = new WWWForm();
									param.AddField("a_identifier", response["a_identifier"].Value<string>());
									param.AddField("a_type", response["a_type"].Value<string>());

									NetworkManager.Send("auth/deletecancel", param, (response) => {
										JObject res = (JObject)response;
										if (res.ContainsKey("rs"))
										{
											if (res["rs"].Value<int>() == 0)
											{
												SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("탈퇴철회"),  StringData.GetStringByStrKey("탈퇴철회완료"), () =>
												{
													LoginEvent.SendLoginResult(loginResultData);
												});
											}
											else
											{
												ToastManager.On(StringData.GetStringByStrKey("탈퇴철회오류발생"));
												LoginEvent.SendLoginResult(loginResultData);
											}
										}
										else
										{
											ToastManager.On(StringData.GetStringByStrKey("탈퇴철회오류발생"));
											LoginEvent.SendLoginResult(loginResultData);
										}
									});
								},
								() => {
									LoginEvent.SendLoginResult(loginResultData);
								});
							}
							return;

						case (int)eLoginUserState.BANNED:
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("account_banned"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200), ()=> {
								loginResultData.loginResultState = eLoginResult.ERROR_BANNED_ACCOUNT;
								LoginEvent.SendLoginResult(loginResultData);
							});

							return;
						case (int)eLoginUserState.DELETED:
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("account_deleted"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200),()=> {
								loginResultData.loginResultState = eLoginResult.ERROR_DELETED_ACCOUNT;
								LoginEvent.SendLoginResult(loginResultData);
							});
							
							return;
						default:
							break;
					}
					Debug.Log($"Apple - OnAppleSignIn - {response["state"].Value<int>()}");					
					LoginEvent.SendLoginResult(loginResultData);
				}
			},
			(error) => {
				loginResultData.loginResultState = eLoginResult.ERROR_UNKNOWN;
				Debug.Log("Apple - OnAppleSignIn - fail");
				isProceedLogin = false;
				LoginEvent.SendLoginResult(loginResultData);
			});
		}

		void UpdateLoginData(JObject jobject)
		{
			if (SBFunc.IsJTokenCheck(jobject["token_bin"]))
			{
				loginResultData.binToken = jobject["token_bin"].Value<string>();
			}
		}
    }
}