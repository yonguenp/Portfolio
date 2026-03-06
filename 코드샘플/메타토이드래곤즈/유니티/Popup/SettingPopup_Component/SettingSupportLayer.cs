using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SettingSupportLayer : MonoBehaviour
    {

        [Header("[Temporary Banner]")]
        [SerializeField] GameObject banner = null;

        [Header("[Coupon]")]
        [SerializeField] GameObject couponLayer = null;
        [SerializeField] InputField couponInputField = null;

        [Header("[UserInfo]")]
        [SerializeField] Text userNumberText = null;

        [Header("[Wallet]")]
        [SerializeField] GameObject walletLayerObject = null;
        [SerializeField] Text walletAddressText = null;

        [Header("[Community]")]
        [SerializeField] GameObject communityLayerObject = null;

        [Header("[MTW]")]
        [SerializeField] GameObject mtwLayerObject = null;

        [Header("Server")]
        [SerializeField] Text server = null;

        [Header("[URL]")]
        [SerializeField] string discordURL = "";
        [SerializeField] string twitterURL = "";
        [SerializeField] string mtwURL = "";        

        string supportURL 
        { 
            get {
                return GameConfigTable.GetSurpportURL();
            } 
        }

        string serviceTermsURL
        {
            get
            {
                return GameConfigTable.GetServiceTermsURL();
            }
        }

        string privacyTermsURL
        {
            get
            {
                return GameConfigTable.GetPrivateTermsURL();
            }
        }

        public void Init()
        {
            couponLayer.SetActive(GameConfigTable.IsRegistedVersion());
            
            userNumberText.text = User.Instance.UserAccountData.UserNumber.ToString();
            walletAddressText.text = User.Instance.UserAccountData.WalletAddress;
            
            UpdateLayerByUserState();
        }

        private void OnEnable()
        {
            couponInputField.text = "";
        }

        public void UpdateLayerByUserState()
        {
            // p2e check
            bool isAvailP2E = User.Instance.ENABLE_P2E;

            walletLayerObject.SetActive(false);
            mtwLayerObject.SetActive(false);
            communityLayerObject.SetActive(isAvailP2E);
            banner.SetActive(true);

            server.text = NetworkManager.ServerName;
        }

        public void OnClickCouponInputButton()
        {
            string coupon_code = couponInputField.text;
            coupon_code = coupon_code.Trim();

            if (string.IsNullOrWhiteSpace(coupon_code))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰입력오류"), true, false, false);
                return;
            }

            couponInputField.text = "";

            WWWForm param = new WWWForm();
            param.AddField("code", coupon_code);

            NetworkManager.Send("system/coupon", param, (response) => {
                JObject res = (JObject)response;
                if (res.ContainsKey("rs"))
                {
                    if (res["rs"].Value<int>() == 0)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰입력완료"), true, false, false);
                    }
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰입력오류"), true, false, false);
                }
            },
            (fail)=> {
                JObject root = null;
                if (!string.IsNullOrEmpty(fail))
                    root = JObject.Parse(fail);
                if (root != null && root.ContainsKey("rs"))
                {
                    switch ((eApiResCode)root["rs"].Value<int>())
                    {
                        case eApiResCode.PACKAGE_NOT_EXISTS:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰입력오류"), true, false, false);
                            break;
                        case eApiResCode.EXPIRED_DATE:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰기간오류"), true, false, false);
                            break;
                        case eApiResCode.OUT_OF_STOCK:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰사용제한초과"), true, false, false);
                            break;
                        default:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("쿠폰"), StringData.GetStringByStrKey("쿠폰입력오류"), true, false, false);
                            break;
                    }
                }
                
            });

        }

        public void OnClickCopyUserNumberButton()
        {
            GUIUtility.systemCopyBuffer = User.Instance.UserAccountData.UserNumber.ToString();

            ToastManager.On(StringData.GetStringByStrKey("회원번호복사하기"));
        }

        public void OnClickCopyWalletAddressButton()
        {
            GUIUtility.systemCopyBuffer = User.Instance.UserAccountData.WalletAddress;

            ToastManager.On(StringData.GetStringByStrKey("지갑주소복사하기"));
        }

        public void OnClickDiscordButton()
        {
            if (string.IsNullOrWhiteSpace(discordURL) == false)
            {
                Application.OpenURL(discordURL);
            }
        }

        public void OnClickTwitterButton()
        {
            if (string.IsNullOrWhiteSpace(twitterURL) == false)
            {
                Application.OpenURL(twitterURL);
            }
        }

        public void OnClickMTWButton()
        {
            if (string.IsNullOrWhiteSpace(mtwURL) == false)
            {
                Application.OpenURL(mtwURL);
            }
        }

        public void OnClickNoticeButton()
        {
            PopupManager.OpenPopup<AnnouncePopup>();
        }

        public void OnClickSupportButton()
        {
            SBFunc.SendSupportURL(supportURL);
        }

        public void OnClickServiceTermsButton()
        {
            if (string.IsNullOrWhiteSpace(serviceTermsURL) == false)
            {
                Application.OpenURL(serviceTermsURL);
            }
        }

        public void OnClickPrivacyTermsButton()
        {
            if (string.IsNullOrWhiteSpace(privacyTermsURL) == false)
            {
                Application.OpenURL(privacyTermsURL);
            }
        }
    }
}