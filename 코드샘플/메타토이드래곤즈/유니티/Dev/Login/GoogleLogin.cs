using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

using Google;
using System;

namespace SandboxNetwork
{
	abstract public class GoogleLogin : MonoBehaviour
    {
		protected LoginData loginResultData = new LoginData();

		protected bool isInitLogin = false;
		protected bool isProceedLogin = false;    // 로그인 처리 진행중 여부 체크

		abstract public void OnGoogleLogin();
		abstract public void ClearGoogleInstance();
		protected void RequestGoogleLoginToken(string webClientID)
        {
			isProceedLogin = true;


            try
            {
				if (GoogleSignIn.Configuration == null)
				{
					GoogleSignInConfiguration configuration = new GoogleSignInConfiguration
					{
						WebClientId = webClientID,
						RequestIdToken = true,
						RequestEmail = true,
						UseGameSignIn = false,
						RequestAuthCode = true,
					};

					GoogleSignIn.Configuration = configuration;
				}
			}
			catch
            {
				Debug.LogError("GoogleSignIn.Configuration thrown - maybe already inititalized");
            }

			GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task => {
				if (task.IsFaulted)
				{
					isProceedLogin = false;
					using (IEnumerator<System.Exception> enumerator =
							task.Exception.InnerExceptions.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							GoogleSignIn.SignInException error =
									(GoogleSignIn.SignInException)enumerator.Current;
							Debug.Log("Got Error: " + error.Status + " " + error.Message);
						}
						else
						{
							Debug.Log("Got Unexpected Exception?!?" + task.Exception);
						}
					}
				}
				else if (task.IsCanceled)
				{
					Debug.Log("Canceled");
					isProceedLogin = false;
				}
				else
				{
					OnGoogleSignIn(task.Result.IdToken);					
				}
			});
		}

		protected void OnGoogleSignIn(string jwtToken)
		{
			if (!isProceedLogin)
				return;

			if (string.IsNullOrWhiteSpace(jwtToken))
			{
				loginResultData.loginResultState = eLoginResult.ERROR_INVALID_TOKEN;
				isProceedLogin = false;
				return;
			}

			loginResultData.jwtToken = jwtToken;
			loginResultData.loginType = eAuthAccount.GOOGLE;

			var data = new WWWForm();
			data.AddField("id_tok", jwtToken);
			data.AddField("is_pc",
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			 1
#else
			 0
#endif
			 );

			NetworkManager.Send("auth/google", data, (response) => {
				isProceedLogin = false;
				if (SBFunc.IsJTokenCheck(response["rs"]))
                {
					int rs = response["rs"].Value<int>();
					switch (rs)
                    {
						case (int)eApiResCode.OK:
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
															SystemPopup.OnSystemPopup( StringData.GetStringByStrKey("탈퇴철회"), StringData.GetStringByStrKey("탈퇴철회완료"), () =>
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
											()=> {
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
								LoginEvent.SendLoginResult(loginResultData);
							}
							break;
						case (int)eApiResCode.ACCOUNT_IS_BANNED:
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000614), StringData.GetStringByStrKey("account_banned"), () => {
								loginResultData.loginResultState = eLoginResult.ERROR_BANNED_ACCOUNT;
								LoginEvent.SendLoginResult(loginResultData);
							});
							break;
						case (int)eApiResCode.ACCOUNT_IS_DELETED:
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000614), StringData.GetStringByStrKey("account_deleted"), ()=> {
								loginResultData.loginResultState = eLoginResult.ERROR_DELETED_ACCOUNT;
								LoginEvent.SendLoginResult(loginResultData);
							});
							break;
						case (int)eApiResCode.NOT_WHITELIST:
							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000614), StringData.GetStringByStrKey("error_whitelist_login"), () => {
								loginResultData.loginResultState = eLoginResult.ERROR_NOT_WHITE_LIST;
								LoginEvent.SendLoginResult(loginResultData);
							});
							break;
						default:
							//data sync error에 대한 이벤트 통계를 위하여 추가
							LoginManager.Instance.SetFirebaseEvent("auth_error_" + rs.ToString());

							string msg = StringData.GetStringByIndex(100000634);
							if (StringData.IsContainStrKey("errorcode_" + rs.ToString()))
								msg = StringData.GetStringByStrKey("errorcode_" + rs.ToString());

							SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000614), msg, () => {
								loginResultData.loginResultState = eLoginResult.ERROR_NOT_WHITE_LIST;
								LoginEvent.SendLoginResult(loginResultData);
							});
							break;
                    }
				}

				ClearGoogleInstance();
			},
			(error) => {
				loginResultData.loginResultState = eLoginResult.ERROR_UNKNOWN;

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