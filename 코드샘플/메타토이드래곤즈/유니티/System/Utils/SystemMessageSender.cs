using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace SandboxNetwork
{
    public class SystemMessageSender : EditorWindow
    {
        string sendMessage = "";

        [MenuItem("SB Tools/ Send System Message")]
        static void Init()
        {
            SystemMessageSender senderWindow = (SystemMessageSender)GetWindow(typeof(SystemMessageSender));
            senderWindow.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("[System Message]", EditorStyles.boldLabel);
            GUILayout.Space(20);

            //sendMessage = EditorGUILayout.TextField("Message", sendMessage);
            sendMessage = EditorGUILayout.TextArea(sendMessage, GUILayout.MinHeight(70));

            if (GUILayout.Button("Send"))
            {
                ChatDataInfo testData = new(eChatCommentType.SystemMsg, User.Instance.UserAccountData.UserNumber, User.Instance.UserData.UserNick, User.Instance.UserData.UserPortrait, 1, "", SBFunc.GetDateTimeToTimeStamp(), 0,
                SBFunc.GetDateTimeToTimeStamp(), sendMessage,1,0);

                ChatManager.Instance.SendMessage(testData);
                ToastManager.OnSystem(testData.Comment,5);

                Debug.LogFormat("Send System Message ==> " + sendMessage);
            }
        }
    }
}

#endif
