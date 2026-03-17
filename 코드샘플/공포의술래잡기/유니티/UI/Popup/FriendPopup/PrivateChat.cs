using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChat : FriendsUIObject
{
    UserProfile userProfile = null;
    [SerializeField]
    FriendItem uiFriendItem = null;

    [SerializeField]
    InputField inputField = null;

    [SerializeField]
    GameObject sendButton = null;

    [SerializeField]
    ChatItem myChatItem = null;

    [SerializeField]
    ChatItem targetChatItem = null;

    [SerializeField]
    ScrollRect scrollview = null;

    string privateText;
    int bwfCount = 0;
    bool enableChat = true;
    void OnEnable()
    {
        
    }

    void OnDisable()
    {
       
    }
 
    public void SetFriend(UserProfile userData)
    {
        userProfile = userData;
        uiFriendItem.SetChatUI(userProfile);
        
        Managers.Chat.CurChatTarget = userData;
        Managers.Chat.ReadMessage(userData.uno);

        RefreshUI();
    }

    override public void RefreshUI()
    {
        Managers.Chat.SetAddMessageCallback(typeof(PrivateChat), AddMessageAndRead);
        InitChat();
    }

    public override void ClearItems()
    {
         foreach(Transform obj in itemContainer)
        {
            if(myChatItem.transform != obj && targetChatItem.transform != obj)
                Destroy(obj.gameObject);
        }

        myChatItem.gameObject.SetActive(false);
        targetChatItem.gameObject.SetActive(false);
    }

    public void InitChat()
    {
        ClearItems();
       var items = Managers.Chat.GetChatDatas(userProfile.uno);
       foreach(var item in items)
           AddMessage(item);

        HideSendButton();
        privateText = "";
    }


    override public bool IsNewFlag()
    {
        return Managers.FriendData.GetNewRecivedCount() > 0;
    }

    override public void NewFlagDone()
    {
        Managers.FriendData.SetNewRecivedCount(0);
    }



    public void OnSendChat()
    {
        if(userProfile == null) 
            return;

        if (enableChat == false)
            return;

        if(Crosstales.BWF.BWFManager.Instance.Contains(inputField.text))
        {
            bwfCount++;
            if(bwfCount > 3)
            {
                PopupCanvas.Instance.ShowFadeText(string.Format("반복된 욕설감지로 인하여 채팅이 {0}분 동안 제한됩니다.", bwfCount));
                enableChat = false;
                inputField.text = "";
                privateText = "";
                HideSendButton();
                Invoke("ChatEnable", bwfCount * 60.0f);
                return;
            }
        }

        Managers.Network.SendChatMessage(userProfile.uno, userProfile.nick, inputField.text);
        //AddMyMessage(inputField.text);
        inputField.text = "";
        privateText = "";
        HideSendButton();
    }

    void ChatEnable()
    {
        CancelInvoke("ChatEnable");

        enableChat = true;
        OnCheckValue(inputField.text);
    }

    Coroutine scrollviewMoveCO = null;

    IEnumerator ScrollviewMoveBottum()
    {
        yield return null;
        scrollview.verticalNormalizedPosition = 0;
    }

    public void AddMessageAndRead(sChatData chatData)
    {
        if(chatData.ChatId != userProfile.uno) 
            return;
            
        if(chatData.Type == eChatType.Receive)
        {
            AddTargetMessage(chatData);
        }
        else
        {
            AddMyMessage(chatData);
        }
        
        Managers.Chat.ReadMessage(chatData.ChatId);
    }

    public void AddMessage(sChatData chatData)
    {
        if(chatData.ChatId != userProfile.uno) 
            return;
            
        if(chatData.Type == eChatType.Receive)
        {
            AddTargetMessage(chatData);
        }
        else
        {
            AddMyMessage(chatData);
        }
    }

    public void AddMyMessage(sChatData chatData)
    {
        var item = myChatItem.CloneItem(this) as ChatItem;
        if(item == null) return;
        item.SetChatItem(chatData.Message);
        item.SetDate(chatData.Time);
        
        if(gameObject.activeSelf == false) return;
        if(scrollviewMoveCO != null)
        {
            StopCoroutine(scrollviewMoveCO);
        }

        scrollviewMoveCO = StartCoroutine(ScrollviewMoveBottum());
    }

    public void AddTargetMessage(sChatData chatData)
    {
        var item = targetChatItem.CloneItem(this) as ChatItem;
         if(item == null) return;
        item.SetChatItem(chatData.Message);
        item.SetDate(chatData.Time);
        
        if(gameObject.activeSelf == false) return;
         if(scrollviewMoveCO != null)
         {
            StopCoroutine(scrollviewMoveCO);
         }
           
        scrollviewMoveCO = StartCoroutine(ScrollviewMoveBottum());
    }

    public bool IsChatTarget(long targetId)
    {
        if(userProfile == null)
            return false;
        return userProfile.uno == targetId;
    }

    public void ClearData()
    {
        StopAllCoroutines();
        Managers.Chat.RemoveAddMessageCallback(typeof(PrivateChat));
        Managers.Chat.CurChatTarget = null;
        userProfile = null;
    }

    public override void HideHI()
    {
    }

    void ShowSendButton()
    {
        sendButton.SetActive(true);
    }

    void HideSendButton()
    {
        sendButton.SetActive(false);
    }

    public void OnCheckValue(string inputStr)
    {
        int len = inputField.text.Length;
        if(len <= 100)
            privateText = inputField.text;
        else
            inputField.text = privateText;

        if (enableChat == false)
        {
            HideSendButton();
            return;
        }

        for(int i =0 ; i < len ; ++i)
        {
            var s = inputField.text[i];
            if(s.CompareTo(' ') != 0)
            {
                ShowSendButton();
                return;
            }
        }
        HideSendButton();
    }
}
