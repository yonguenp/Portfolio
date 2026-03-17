using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Immutable.Passport;
using Newtonsoft.Json.Linq;
using System;

namespace SandboxNetwork
{
    public partial class IMXLoginController : MonoBehaviour
    {
        protected LoginData loginResultData = new LoginData();
		private Passport passport = null;

		async public void OnIMXLogin(Action loginStartCB = null, Action loginExitCB = null) 
        {
			loginResultData.loginType = eAuthAccount.IMX;

			string environment = Immutable.Passport.Model.Environment.PRODUCTION;
			string clientId = "qEi0r3mtreisQZnOu9ZxTRfG2Fcdp6Qw";

			if (!NetworkManager.IsLiveServer)
            {
				environment = Immutable.Passport.Model.Environment.SANDBOX;
				clientId = "eex3X8b5aSw5TrDTtwqraq2UIEsAqVio";
			}			

			string redirectUri = "metatoy://callback";
            string logoutUri = "metatoy://logout";


			loginStartCB?.Invoke();

			passport = await Passport.Init(clientId, environment, redirectUri, logoutUri);

			
			bool logined = await passport.HasCredentialsSaved();
			try
			{
				logined = await passport.Login(logined);
			}
			catch (OperationCanceledException ex)
			{
				Debug.LogError($"Failed to login: cancelled {ex.Message}\\n{ex.StackTrace}");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to login: {ex.Message}");
			}


			string id_token = "";

			try
			{
				await passport.ConnectEvm();

				var account_list = await passport.ZkEvmRequestAccounts();
				if (account_list.Count > 0)
				{
					string wallet_address = account_list[0];

					try
					{
						id_token = await passport.GetIdToken();
					}
					catch
					{
						Debug.LogError("Failed to GetIdToken");
					}
				}
			}
			catch
			{
				Debug.LogError("Failed to ZkEvmRequestAccounts");
			}

			loginExitCB?.Invoke();

			if (string.IsNullOrEmpty(id_token))
			{
				return;
			}

			var data = new WWWForm();
			//data.AddField("wallet_address", wallet_address);
			data.AddField("id_tok", id_token);
			//data.AddField("acc_token", acc_token);

			NetworkManager.Send("auth/imx", data, (response) => {
				loginResultData.jwtToken = id_token;

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
								SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("탈퇴철회"), StringData.GetStringFormatByStrKey("탈퇴철회메시지", time.ToString()), () =>
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
												SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("탈퇴철회"), StringData.GetStringByStrKey("탈퇴철회완료"), () =>
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
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("account_banned"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200), () => {
								loginResultData.loginResultState = eLoginResult.ERROR_BANNED_ACCOUNT;
								LoginEvent.SendLoginResult(loginResultData);
							});

							return;
						case (int)eLoginUserState.DELETED:
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("account_deleted"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200), () => {
								loginResultData.loginResultState = eLoginResult.ERROR_DELETED_ACCOUNT;
								LoginEvent.SendLoginResult(loginResultData);
							});

							return;
						default:
							break;
					}
					Debug.Log($"IMX - OnIMXSignIn - {response["state"].Value<int>()}");
					LoginEvent.SendLoginResult(loginResultData);
				}
			},
			(error) => {
				loginResultData.loginResultState = eLoginResult.ERROR_UNKNOWN;
				Debug.Log("IMX - OnIMXSignIn - fail");
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