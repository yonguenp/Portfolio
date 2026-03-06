using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SettingAccountLayer : MonoBehaviour, EventListener<LoginEvent>
    {
        [Header("[Account]")]
        [SerializeField] GameObject accountConnectLayer = null;
        [SerializeField] Text accountType = null;
        [SerializeField] Button google = null;
        [SerializeField] Button apple = null;
        [SerializeField] Button imx = null;
        [SerializeField] GameObject accountDelete = null;
        [SerializeField] GameObject accountDeleteDim = null;

        [Header("[Location]")]
        [SerializeField] Text locationInfoText = null;

        [Header("[Gemstone]")]
        [SerializeField] Text cashGemstoneAmountText = null;
        [SerializeField] Text freeGemstoneAmountText = null;

        GoogleLoginController googleLogin = null;
        AppleLoginController appleLogin = null;
        IMXLoginController imxLogin = null;

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public void Init()
        {
            // Login Controller는 Mono 상속 / Update를 써야하기때문에 Addcomponent로 인스턴스를 생성해줘야지 new로 하면안됨.
            googleLogin = gameObject.GetComponent<GoogleLoginController>();
            appleLogin = gameObject.GetComponent<AppleLoginController>();
            if (googleLogin == null)
                googleLogin = gameObject.AddComponent<GoogleLoginController>();
            if (appleLogin == null)
                appleLogin = gameObject.AddComponent<AppleLoginController>();
            if (imxLogin == null)
                imxLogin = gameObject.AddComponent<IMXLoginController>();

            locationInfoText.text = StringData.GetStringByStrKey(User.Instance.ENABLE_P2E ? "GLOBAL" : "KR");

            cashGemstoneAmountText.text = User.Instance.UserData.CashGemstone.ToString();
            freeGemstoneAmountText.text = User.Instance.UserData.Gemstone.ToString();

            RefreshLayer();
        }

        bool IsLinkAble()
        {
            switch(User.Instance.UserAccountData.AuthAccountType)
            {
                case eAuthAccount.NONE:
                case eAuthAccount.GUEST:
                    return true;
            }

            return false;
        }

        string AccountType
        {
            get
            {
                switch (User.Instance.UserAccountData.AuthAccountType)
                {
                    case eAuthAccount.NONE:
                        return eAuthAccount.GUEST.ToString();
                    default:
                        return User.Instance.UserAccountData.AuthAccountType.ToString();
                }
            }
        }
        public void RefreshLayer()
        {
            // 플랫폼 로그인 상태 관련
            bool isGuestLogin = IsLinkAble();
            accountConnectLayer.SetActive(isGuestLogin);

            
            byte login_flag = (byte)PlayerPrefs.GetInt("login_flag", 0);
            byte login_imx = 1 << 3;  //8
            imx.gameObject.SetActive(
#if UNITY_EDITOR
                                    true ||
#endif
                                (login_flag & login_imx) > 0);

            accountType.text = StringData.GetStringByStrKey(AccountType);

            accountDelete.SetActive(!isGuestLogin);
            accountDeleteDim.SetActive(isGuestLogin);
        }

        public void OnClickGoogleLoginButton()
        {
            if (User.Instance.UserAccountData.AuthAccountType != eAuthAccount.GUEST)
                return;
            //게스트만 할수있음
            googleLogin.OnGoogleLogin();
        }

        public void OnClickAppleLoginButton()
        {
            if (User.Instance.UserAccountData.AuthAccountType != eAuthAccount.GUEST)
                return;
            //게스트만 할수있음
            appleLogin.OnAppleLogin();
        }

        public void OnClickIMXLoginButton()
        {
            if (User.Instance.UserAccountData.AuthAccountType != eAuthAccount.GUEST)
                return;
            //게스트만 할수있음
            imxLogin.OnIMXLogin();
        }

        public void OnClickPurchaseRecoveryButton()
        {
            IAPManager.Instance.CheckPendingProducts();
            ToastManager.On(StringData.GetStringByStrKey("구매복원완료"));
        }

        public void OnClickAccountDeleteButton()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("계정탈퇴"), StringData.GetStringByStrKey("계정탈퇴경고문구"),
                    ()=> {
                        NetworkManager.Send("auth/delete", null, (response) => {
                            JObject res = (JObject)response;
                            if (res.ContainsKey("rs"))
                            {
                                if (res["rs"].Value<int>() == 0)
                                {
                                    OnLogout();
                                }
                            }
                            else
                            {
                                ToastManager.On(StringData.GetStringByStrKey("계정탈퇴오류발생"));
                            }
                        });
                    },
                    () =>
                    {
                        //cancel
                    },
                    () =>
                    {
                        //x
                    }
                );
        }

        public void OnClickAccountDeleteButtonWhenGuest()
        {
            ToastManager.On(StringData.GetStringByStrKey("게스트탈퇴불가"));
        }

        public void OnClickLogoutButton()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("로그아웃"), StringData.GetStringByStrKey("logout_guide_warning"),
                    OnLogout,
                    () =>
                    {
                        //cancel
                    },
                    () =>
                    {
                        //x
                    }
                );
        }

        void OnLogout()
        {
            User.Instance.UserAccountData.Clear();
            NotificationManager.Instance.Clear();
            SBGameManager.Instance.UserAccessToken = "";
            SBGameManager.Instance.UserNickname = "";
            DataBase.Destroy();
            if (Town.Instance != null)
            {
                Town.Instance.OnLogout();
            }

            PopupManager.ClosePopup<SettingPopup>();

            SBGameManager.Instance.BackToLoginScene(true);
        }

        public void OnLinkAccount(LoginData data)
        {
            WWWForm param = new WWWForm();
            param.AddField("id_tok", data.jwtToken);
            param.AddField("a_type", (int)data.loginType);
            param.AddField("is_pc",
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
             1
#else
			 0
#endif
             );

            NetworkManager.Send("auth/link", param, (response)=> {
                JObject res = (JObject)response;
                if (res.ContainsKey("rs"))
                {
                    if (res["rs"].Value<int>() == 0)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("계정연동"), StringData.GetStringByStrKey("계정연동완료"), () =>
                        {
                            UIManager.Instance.InitUI(eUIType.None);
                            LoadingManager.Instance.LoadStartScene();
                            PopupManager.AllClosePopup();
                        });
                    }
                }
            });
        }

        public void OnEvent(LoginEvent eventType)
        {
            switch (eventType.loginData.loginResultState)
            {
                case eLoginResult.OK_NEW_ACCOUNT:
                    OnLinkAccount(eventType.loginData);
                    break;
                case eLoginResult.OK_HAS_ACCOUNT:
                    ToastManager.On(StringData.GetStringByStrKey("연동불가_이미연동계정"));
                    break;
                default:
                    ToastManager.On(StringData.GetStringByStrKey("연동불가_오류발생"));
                    break;
            }
        }
    }
}
