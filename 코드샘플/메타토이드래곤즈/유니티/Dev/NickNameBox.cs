using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class NickNameBox : MonoBehaviour
    {
        [Header("LoginEditBox")]
        [SerializeField]
        private StartLoading loader;
        [SerializeField]
        private InputField editBox = null;
        [SerializeField]
        private Button loginBtn = null;
        [SerializeField]
        Button idCheckButton = null;
        [SerializeField]
        GameObject idBeforeCheckObject = null;
        [SerializeField]
        GameObject idAfterCheckObject = null;


        [Space(10)]
        [SerializeField]
        private GameObject systemPopup;

        LoginData signupData = null;
        /*
				 * useBundle = AssetBundleManager.UseBundleAsset
			이전엔 useBundle 를 사용하여 번들 사용 미사용을 체크함,

			과거 번들 버전을 지정하여 사용하는 기능은 삭제함,

			이 코드에서 따로 동작하는 것은 없음.
			이 코드는 로그인 동작을 초기 점화할 뿐 실제 계정 데이터와 로딩은 StartLoading.cs -> Init()에서 이루어짐
			새로 로그인씬이 만들어진다면 이 코드는 필수가 아님

		*/

        public void SetActive(bool active, LoginData data = null)
        {
            gameObject.SetActive(active);
            signupData = data;
        }

        bool checkIDExistState = false;
        public bool CheckIDExistState
        {
            get
            {
                return checkIDExistState;
            }
            set
            {
                checkIDExistState = value;

                idBeforeCheckObject.gameObject.SetActive(!checkIDExistState);
                idAfterCheckObject.gameObject.SetActive(checkIDExistState);
            }
        }

		private void OnEnable()
        {
            systemPopup.SetActive(false);

            if (editBox != null)
			{
                editBox.text = "";// SBGameManager.Instance.UserNickname;
            }

            CheckPrevLoginState();
            loginBtn.interactable = true;
        }

        public void OnClickExistIDCheck()
        {
            string nick = editBox.text.Trim();
            if (string.IsNullOrEmpty(nick))
                return;

            var data = new WWWForm();
            data.AddField("req_nick", nick);
            NetworkManager.Send("auth/checknick", data, (JObject jsonData) => {
                switch (jsonData["rs"].Value<int>())
                {
                    case (int)eApiResCode.OK:
                        AppsFlyerSDK.AppsFlyer.sendEvent("create_nickname", new Dictionary<string, string>());
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002835), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        CheckIDExistState = true;
                        idCheckButton.SetInteractable(false);
                        idCheckButton.SetButtonSpriteState(true);

                        CheckLoginButtonState();

                        return;

                    case (int)eApiResCode.NICKNAME_DUPLICATES:
                    case (int)eApiResCode.ACCOUNT_EXISTS:
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        return;

                    case (int)eApiResCode.ACCOUNT_NOT_EXISTS:
                        ToastManager.On(100002529);
                        return;

                    case (int)eApiResCode.INVALID_NICK_CHAR:
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002918), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        return;
                }
            });
        }

		public void OnClickSignUp()
        {
            if (editBox == null) return;
            
            // 닉네임 관련 체크
            string nick = editBox.text.Trim();
            if (string.IsNullOrEmpty(nick))
                return;

            if (CheckIDExistState == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002836), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                return;
            }

            //if (Crosstales.BWF.BWFManager.Instance.Contains(nick))
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetString(100000248, "알림"), StringData.GetString(100002664, "바르고 고운말을 사용합시다."));
            //    return;
            //}

            if (loginBtn != null)
            {
                loginBtn.interactable = false;
                //loginBtn.SetInteractable(false);                
                loginBtn.SetButtonSpriteState(false);                
            }

            var data = new WWWForm();
            data.AddField("req_nick", nick);
            data.AddField("token_bin", SBGameManager.Instance.UserAccessToken);

            NetworkManager.Send("user/create", data, OnClickLogin);
        }

        private void OnClickLogin(JObject jObject)
        {
			switch (jObject["rs"].Value<int>())
			{
				case (int)eApiResCode.OK: //유저 데이터 채우기 및 씬 이동

                    //User.Instance.SetBase(jObject);
                    loader.UpdateUserLoginPrefData(jObject);

                    loader.OnSelect();
                    return;                    

                case (int)eApiResCode.NICKNAME_DUPLICATES:
                case (int)eApiResCode.ACCOUNT_EXISTS:
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                    return;

                case (int)eApiResCode.ACCOUNT_NOT_EXISTS:
                    ToastManager.On(100002529);
                    return;

                case (int)eApiResCode.INVALID_NICK_CHAR:
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                    return;
            }

			if (loginBtn != null)
			{
                loginBtn.interactable = true;
                //loginBtn.SetInteractable(true);
                loginBtn.SetButtonSpriteState(true);
			}
		}

        // 이전에 접속한 이력이 있는 경우 처리
        void CheckPrevLoginState()
        {
            //if (loader == null) return;

            //string accessToken = PlayerPrefs.GetString("user_access_token", "");

            // test - 일단은 기존 uid 처리로 테스트
            //if (string.IsNullOrWhiteSpace(userAccessToken) == false)
            //{
            //    //var data = new WWWForm();
            //    //data.AddField("access_token", accessToken);
            //    //NetworkManager.Send("dev/signin", data, OnClickLogin);
            //}
            //else
            //{

            //}

            idCheckButton.gameObject.SetActive(true);

            CheckIDExistState = false;
        }

        public void OnChangeIDField()
        {
            if (editBox == null || editBox.text == "")
            {
                if (loginBtn != null)
                {
                    //loginBtn.SetInteractable(false);
                    idCheckButton.SetButtonSpriteState(false);
                }
                    
                if (idCheckButton != null)
                {
                    CheckIDExistState = false;
                    idCheckButton.SetInteractable(false);
                    idCheckButton.SetButtonSpriteState(false);
                }

                return;
            }

            if (loginBtn != null)
            {
                //loginBtn.SetInteractable(false);
                loginBtn.SetButtonSpriteState(false);
            }

            if (idCheckButton != null)
            {
                CheckIDExistState = false;
                idCheckButton.SetInteractable(true);
                idCheckButton.SetButtonSpriteState(true);
            }
        }

        public void CheckLoginButtonState()
        {
            if (loginBtn == null) return;
            
            //loginBtn.SetInteractable(termsCheck && idCheck);
            loginBtn.SetButtonSpriteState(CheckIDExistState);
        }

		public void ResoureInitPopupOpen()
		{
            SystemLoadingPopup popup = systemPopup.GetComponent<SystemLoadingPopup>();
            popup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("리소스초기화"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
            popup.SetCallBack(
                () =>
                {
                    AssetBundleManager.AssetFileRemove();
                }
            );
            popup.SetButtonState(true, true, true);
        }

        public void OnClickShowTermsButton_Service()
        {
            Application.OpenURL(GameConfigTable.GetServiceTermsURL());
        }

        public void OnClickShowTermsButton_Info()
        {
            Application.OpenURL(GameConfigTable.GetPrivateTermsURL());
        }
    }
}