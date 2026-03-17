using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanInfo_Chat : MonoBehaviour
{
    [SerializeField] GameObject targetChat;
    [SerializeField] GameObject myChat;
    [SerializeField] InputField inputField;
    [SerializeField] Button sendChatBtn;
    [SerializeField] Transform chatContent;
    [SerializeField] Image chatToggleIcon;

    ClanChatData lastChatData = null;
    int bwfCount = 0;

    public void Init()
    {
        ChatClear();
        RefreshChatUI();
    }

    public void SetToggleChatSize(bool fullsize)
    {
        Vector3 rot = Vector3.zero;
        if(fullsize)
        {
            rot.z = 90.0f;
        }
        else
        {
            rot.z = -90.0f;
        }
        chatToggleIcon.transform.eulerAngles = rot;
    }

    public void RefreshChatUI()
    {
        targetChat.SetActive(true);
        myChat.SetActive(true);


        var chats = Managers.ClanCaht.GetAllChat();
        int index = 0;
        if (lastChatData != null)
        {
            for (int i = 0; i < chats.Length; i++)
            {
                if (lastChatData == chats[i])
                {
                    index = i + 1;
                    break;
                }
            }
        }

        for (int i = index; i < chats.Length; i++)
        {
            ClanChatData data = chats[i];

            GameObject chat = new GameObject();
            if (data.user_id != Managers.UserData.MyUserID)
                chat = GameObject.Instantiate(targetChat, chatContent);
            else
                chat = GameObject.Instantiate(myChat, chatContent);

            if (chat.GetComponent<ClanChatItem>() != null)
            {
                var item = chat.GetComponent<ClanChatItem>();

                item.SetInfo(data.user_nick, data.user_point);
                item.SetChatItem(data.message);
                item.SetDate(data.tiem_ms);
            }

            lastChatData = data;
        }


        targetChat.SetActive(false);
        myChat.SetActive(false);

        CancelInvoke("ScrollDown");
        Invoke("ScrollDown", 0.1f);
    }

    public void ScrollDown()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)chatContent.transform);
        chatContent.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }


    public void SendBtn()
    {
        if (inputField.text == string.Empty)
            return;

        //if (Crosstales.BWF.BWFManager.Instance.Contains(inputField.text))
        //{
        //    bwfCount++;
        //    Invoke("ClearCount", 30.0f);
        //}

        //if (bwfCount > 3)
        //{
        //    PopupCanvas.Instance.ShowFadeText("클랜반복욕설", bwfCount);
        //    return;
        //}

        if (inputField.text.Length > 100)
        {
            inputField.text = inputField.text.Substring(0, 100);
        }

        Managers.Network.SendClanChatMessage(inputField.text);
        //SBDebug.Log(inputField.text);
        inputField.text = string.Empty;
    }

    void ClearCount()
    {
        bwfCount--;
        if (bwfCount < 0)
            bwfCount = 0;
    }

    public void ChatClear()
    {
        foreach (Transform chat in chatContent)
        {
            if (chat == targetChat.transform || chat == myChat.transform)
                continue;
            Destroy(chat.gameObject);
        }

        lastChatData = null;
        bwfCount = 0;
    }
}
