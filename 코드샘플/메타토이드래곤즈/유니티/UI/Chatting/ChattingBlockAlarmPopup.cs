using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChattingBlockAlarmPopup : SystemPopup
    {
        [SerializeField] Text nickNameLabel = null;

        public override void InitUI()//data안에 닉네임도 세팅
        {
            
        }

        public void SetUserNickName(string _nick)
        {
            if (nickNameLabel != null)
            {
                nickNameLabel.text = _nick;
            }
        }
    }
}