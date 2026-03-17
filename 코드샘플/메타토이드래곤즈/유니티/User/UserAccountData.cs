using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserAccountData
    {
        public string SessionToken { get; private set; } = "";
        public string AccessToken { get; private set; } = "";

        public long UserNumber { get; private set; } = -1;
        public string Nickname { get; private set; } = "";

        public string WalletAddress { get; private set; } = "";
        public eAuthAccount AuthAccountType { get; private set; } = eAuthAccount.NONE;

        public void Set(JObject jsonData)   // 추후 클래스 형식 구조 대비
        {
            if (SBFunc.IsJTokenCheck(jsonData["sid"]))
            {
                UpdateSessionID(jsonData["sid"].Value<string>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["token_bin"]))
            {
                UpdateAccessToken(jsonData["token_bin"].Value<string>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["uno"]))
            {
                UpdateUserNumber(jsonData["uno"].Value<long>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["nick"]))
            {
                UpdateNickname(jsonData["nick"].Value<string>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["wallet_addr"]))
            {
                UpdateWalletAddress(jsonData["wallet_addr"].Value<string>());
            }
            if(SBFunc.IsJTokenCheck(jsonData["auth_type"]))
            {
                UpdateAccountType(jsonData["auth_type"].Value<int>());
            }
        }

        public void UpdateSessionID(string value)
        {
            SessionToken = value;
        }
        public void UpdateAccessToken(string value)
        {
            AccessToken = value;
        }
        public void UpdateUserNumber(long value)
        {
            UserNumber = value;
        }
        public void UpdateNickname(string value)
        {
            Nickname = value;
        }
        public void UpdateWalletAddress(string value)
        {
            WalletAddress = value;
        }

        public void UpdateAccountType(int auth_type)
        {
            AuthAccountType = (eAuthAccount)auth_type;
        }

        public void Clear()
        {
            //SessionToken = "";
            AccessToken = "";
            UserNumber = 0;
            Nickname = "";
            WalletAddress = "";
            AuthAccountType = eAuthAccount.NONE;
        }
    }
}