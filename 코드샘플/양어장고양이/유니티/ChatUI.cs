using DG.Tweening;
using Newtonsoft.Json.Linq;
using SuperBlur;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEngine.UI.Button;

public class ChatUI : MonoBehaviour
{
    public GameObject Chat_Panel;
    public HahahaChat hahahaChat;
    public GameObject ChatOffToggleIcon;
    public GameObject ChatOnToggleIcon;
    public GameObject BottomUI_Panel;
    public GameObject ReturnToOpenChatButton;
    public GameObject FriendAlarm;

    private string roomID = "";
    private bool isOpenChat { get { return string.IsNullOrEmpty(roomID); } }
    public string ChatRoomID { set { roomID = value; OnRoomChange(); } get { return roomID; } }

    private Coroutine chatRefreshCoroutine = null;
    private Coroutine persnalChatRefresh = null;
    private string SendChatMessage = "";
    void Start()
    {
        OnRoomChange();

        OnHideChatUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartChat()
    {
        if (hahahaChat)
        {
            hahahaChat.ClearMsgList();
        }

        if (chatRefreshCoroutine != null)
        {
            StopCoroutine(chatRefreshCoroutine);
            chatRefreshCoroutine = null;
        }

        if (chatRefreshCoroutine == null)
        {
            chatRefreshCoroutine = StartCoroutine(ChatRefreshPull());
        }
        CancelInvoke("InactiveChat");

        Chat_Panel.SetActive(true);
    }

    public void StopChat()
    {
        //SamandaLauncher.OnSamandaShortcut(SamandaLauncher.Samanda_Shorcut.PAGE_HOME, false);
        if(chatRefreshCoroutine != null)
        {
            StopCoroutine(chatRefreshCoroutine);
            chatRefreshCoroutine = null;
        }
        Invoke("InactiveChat", 1.0f);
    }

    public void InactiveChat()
    {
        Chat_Panel.SetActive(false);
    }

    public void OnShowChatUI()
    {
        StartChat();
    }

    public void OnHideChatUI()
    {
        StopChat();

        if (Chat_Panel)
        {
            foreach (DOTweenAnimation dotween in Chat_Panel.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayBackwards();
            }
        }
    }
    
    public void OnEnable()
    {
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.프리플레이)
        {
            if (IsChatUIActive())
            {
                if (chatRefreshCoroutine == null)
                {
                    StartChat();
                }
            }
        }
        //hahahaChat.ClearMsgList();
    }

    public void OnDisable()
    {
        if (chatRefreshCoroutine != null)
        {
            StopCoroutine(chatRefreshCoroutine);
            chatRefreshCoroutine = null;
        }
    }

    public void OnToggleChat()
    {
        bool bUseChat = ChatOffToggleIcon.activeInHierarchy;
        if (bUseChat)
            OnHideChatUI();
        else
            OnShowChatUI();

        //ChatOnToggleIcon.SetActive(bUseChat);
        ChatOffToggleIcon.SetActive(!bUseChat);

        BottomUI_Panel.SetActive(bUseChat);
    }

    public bool IsChatUIActive()
    {
        return ChatOffToggleIcon.activeInHierarchy;
    }

    static Vector3[] colorTable = {
            new Vector3(200,200,200),
            new Vector3(145,216,252),
            new Vector3(144,228,126),
            new Vector3(252,168,78),
            new Vector3(254,124,142),
            new Vector3(208,124,254),
        };
    static public Vector3 GetLevelColorTable(uint level)
    {
        int index = 0;
        if (level <= 3)
        {
            index = 0;
        }
        else if (level < 8)
        {
            index = 1;
        }
        else if (level < 13)
        {
            index = 2;
        }
        else if (level < 18)
        {
            index = 3;
        }
        else if (level < 24)
        {
            index = 4;
        }
        else
        {
            index = 5;
        }

        return colorTable[index];
    }

    public void OnSendMessageChat()
    {
        SendChatMessage = hahahaChat.InputFeild.text;
        SendChatMessage = Crosstales.BWF.Manager.BadWordManager.Instance.ReplaceAll(SendChatMessage);
        SendChatMessage = System.Text.RegularExpressions.Regex.Replace(SendChatMessage, "/\xF0[\x90-\xBF][\x80-\xBF]{2}|[\xF1 -\xF3][\x80 -\xBF]{ 3}|\xF4[\x80 -\x8F][\x80 -\xBF]{ 2}/ ", "");

        hahahaChat.InputFeild.text = "";
    }

    public IEnumerator ChatRefreshPull()
    {
        uint level = 0;
        long myUserNo = NetworkManager.GetInstance().UserNo;
        string nick = SamandaLauncher.GetAccountNickName();
        users user = GameDataManager.Instance.GetUserData();
        string queryURL = NetworkManager.CHAT_URL;
        if(!isOpenChat)
        {
            queryURL = NetworkManager.PERSONAL_CHAT_URL;
        }

        int tailSeq = 0;
        while (true)
        {
            if (user != null)
            {
                object obj;
                if (user.data.TryGetValue("profileId", out obj))
                {
                    level = (uint)obj;
                }
            }

            JObject data = new JObject();
            
            if (string.IsNullOrEmpty(SendChatMessage))
            {
                data.Add("OpCode", 2);
            }
            else
            {
                data.Add("OpCode", 1);
                JObject content = new JObject();
                content.Add("SenderAccountNo", NetworkManager.GetInstance().UserNo.ToString());                
                content.Add("Sender", "[e960cdb67f2cb7488f16347705580180" + level + "e960cdb67f2cb7488f16347705580180]" + nick);
                content.Add("Message", SendChatMessage);
                content.Add("ProfileUrl", "");
                data.Add("Content", content);

                SendChatMessage = "";

                hahahaChat.ScrollRect.verticalNormalizedPosition = 0.0f;
            }

            if (!isOpenChat)
            {
                data.Add("FromUserNo", myUserNo);
                data.Add("ToUserNo", ChatRoomID);
                data.Add("Uri", "personalchat");
            }
            else
            {
                data.Add("Uri", "openchat");
            }

            data.Add("Rs", 0);
            data.Add("PId", SamandaLauncher.GetPID());
            data.Add("ReferenceSeq", tailSeq);
           
            string sendstring = data.ToString(Newtonsoft.Json.Formatting.None);

            UnityWebRequest req = UnityWebRequest.Put(queryURL, System.Text.Encoding.UTF8.GetBytes(sendstring));
            req.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            req.timeout = 10;
            yield return req.SendWebRequest();

            if (!req.isNetworkError && !req.isHttpError)
            {   
                string response = req.downloadHandler.text;
                JObject root = JObject.Parse(response);

                if (root.ContainsKey("Contents"))
                {
                    JArray contents = (JArray)root["Contents"];
                    //int count = contents.Count;
                    List<string> blockedUsers = FriendsManager.Instance.GetBlockedAccountNoList();
                    
                    foreach (JToken chat in contents)
                    {
                        if (chat != null && !string.IsNullOrEmpty(chat.ToString()) && chat["SenderAccountNo"] != null && chat["Sequence"] != null && chat["Sender"] != null && chat["Message"] != null)

                        {
                            bool blocked = false;
                            string SenderAccountNo = chat["SenderAccountNo"].Value<string>();
                            foreach (string block in blockedUsers)
                            {
                                if (block == SenderAccountNo)
                                {
                                    blocked = true;
                                    break;
                                }
                            }

                            if (!blocked)
                                hahahaChat.OnChatMessage(chat["SenderAccountNo"].Value<string>(), chat["Sender"].Value<string>(), chat["Message"].Value<string>());

                            int seq = chat["Sequence"].Value<int>();
                            if (seq > tailSeq)
                            {
                                tailSeq = seq;
                            }
                        }
                        //count--;
                        //if (count < 5)
                        //    yield return new WaitForSeconds(0.01f);
                    }
                }
            }

            if (Chat_Panel)
            {
                ChatOffToggleIcon.SetActive(true);
                //ChatOnToggleIcon.SetActive(false);
                foreach (DOTweenAnimation dotween in Chat_Panel.GetComponentsInChildren<DOTweenAnimation>())
                {
                    dotween.DOPlayForward();
                }
            }

            float time = 2.0f;
            while(time > 0 && string.IsNullOrEmpty(SendChatMessage))
            {
                yield return new WaitForEndOfFrame();
                time -= Time.deltaTime;
            }
        }
    }

    public IEnumerator PersnalChatRefresh()
    {
        long myUserNo = NetworkManager.GetInstance().UserNo;
        string queryURL = NetworkManager.PERSONAL_CHAT_URL;
        uint refrenceTime = 0;

        while (true)
        {
            JObject data = new JObject();
            data.Add("Uri", "personalchat");
            data.Add("OpCode", 4);
            data.Add("FromUserNo", myUserNo);
            data.Add("ReferenceTime", refrenceTime);
            data.Add("Rs", 0);
            data.Add("PId", SamandaLauncher.GetPID());

            string sendstring = data.ToString(Newtonsoft.Json.Formatting.None);

            UnityWebRequest req = UnityWebRequest.Put(queryURL, System.Text.Encoding.UTF8.GetBytes(sendstring));
            req.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            req.timeout = 10;
            yield return req.SendWebRequest();

            if (!req.isNetworkError && !req.isHttpError)
            {
                string response = req.downloadHandler.text;
                JObject root = JObject.Parse(response);

                if (root.ContainsKey("Contents"))
                {
                    List<UserProfile> myFriends = FriendsManager.Instance.GetFriendList();
                    JObject contents = (JObject)root["Contents"];

                    uint chatID = string.IsNullOrEmpty(ChatRoomID) ? 0 : Convert.ToUInt32(ChatRoomID);
                    foreach (JProperty contentProp in contents.Properties())
                    {
                        uint uno = Convert.ToUInt32(contentProp.Name);
                        foreach(UserProfile user in myFriends)
                        {
                            if(user.uno == uno)
                            {
                                JObject content = (JObject)contents[contentProp.Name];
                                
                                if(user.last_update < content["SendTime"].Value<uint>())
                                {
                                    user.lastMessage = content["Message"].Value<string>();
                                    user.last_update = content["SendTime"].Value<uint>();

                                    if (!FriendAlarm.activeInHierarchy)                                        
                                    {
                                        if (user.uno != chatID)
                                        {
                                            FriendAlarm.SetActive(true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                refrenceTime = Convert.ToUInt32(NecoCanvas.GetCurTime());
            }

            float time = 3.0f;
            while (time > 0 && string.IsNullOrEmpty(SendChatMessage))
            {
                yield return new WaitForEndOfFrame();
                time -= Time.deltaTime;
            }
        }
    }

    public void OnFriendsButton()
    {
        FriendAlarm.SetActive(false);
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.FRIEND_INFO_POPUP);
    }

    public void OnMyTitleButton()
    {
        NecoCanvas.GetPopupCanvas().ShowProfilePopup();
    }

    public void OnAllChatButton()
    {
        uint chatID = string.IsNullOrEmpty(ChatRoomID) ? 0 : Convert.ToUInt32(ChatRoomID);
        

        if (chatID != 0)
        {
            List<UserProfile> myFriends = FriendsManager.Instance.GetFriendList();
            foreach (UserProfile user in myFriends)
            {
                if (user.uno != chatID)
                {
                    user.UIShown(Convert.ToUInt32(NecoCanvas.GetCurTime()));
                }
            }
        }

        ChatRoomID = "";
    }

    public void OnSendCardInfo(string cardInfo)
    {
        SendChatMessage = "[e960cdb67f2cb7488f16347705580180" + cardInfo + "e960cdb67f2cb7488f16347705580180]";
    }

    private void OnRoomChange()
    {
        ReturnToOpenChatButton.SetActive(!isOpenChat);
        StopChat();
        StartChat();
    }

    public void OnFriendAlarmCheck()
    {
        if (!FriendAlarm.activeInHierarchy)
        {
            if(FriendsManager.Instance.GetNewFriendCount() > 0 ||  FriendsManager.Instance.GetNewRecivedCount() > 0)
                FriendAlarm.SetActive(true);
        }
    }
}
