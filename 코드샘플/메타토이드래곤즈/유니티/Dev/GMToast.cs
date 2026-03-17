using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMToast : MonoBehaviour
{
    enum CHAT_SERVER
    {
        DEV = 0,
        MAIN = 1,
        CR = 2,
        CM = 3,
        MAX = 4
    }

    const string LIVE_MAIN_WEB_SERVER = "https://saga-api.meta-toy.world/main/api/";
    const string LIVE_MAIN_CHAT_SERVER = "MTDZ-SAGA-MAIN-BACK-SOCK-fa0be19e3f38ec99.elb.ap-northeast-2.amazonaws.com";

    const string LIVE_CM_WEB_SERVER = "https://saga-api.meta-toy.world/cm/api/";
    const string LIVE_CM_CHAT_SERVER = "MTDZ-SAGA-CM-BACK-SOCK-6ad9e6b9cdb17847.elb.ap-northeast-2.amazonaws.com";

    const string LIVE_CR_WEB_SERVER = "https://saga-api.meta-toy.world/cr/api/";
    const string LIVE_CR_CHAT_SERVER = "MTDZ-SAGA-CR-BACK-SOCK-f4fc0954fe57f1dd.elb.ap-northeast-2.amazonaws.com";

    CChatServer[] SERVERS = new CChatServer[(int)CHAT_SERVER.MAX];
    string[] SERVER_DOMAIN = new string[4] { NetworkManager.DEV_CHAT_SERVER, LIVE_MAIN_CHAT_SERVER, LIVE_CR_CHAT_SERVER, LIVE_CM_CHAT_SERVER };
    int[] SERVER_PORT = new int[4] { NetworkManager.DEV_SERVER_PORT, 3000, 3000, 3000 };

    [SerializeField]
    InputField NoticeInput;
    [SerializeField]
    Text[] ServerStatus = new Text[4];
    [SerializeField]
    Transform toastRoot = null;
    [SerializeField]
    GameObject toastPrefab = null;

    private void Start()
    {
        for(int i = 0; i < (int)CHAT_SERVER.MAX; i++)
        {
            SERVERS[i] = new CChatServer();
            SERVERS[i].Initialize(SERVER_DOMAIN[i], SERVER_PORT[i], true);
            SERVERS[i].InitConnectChatServer(this);
        }
    }

    private void Update()
    {
        for (int i = 0; i < (int)CHAT_SERVER.MAX; i++)
        {
            bool connected = SERVERS[i].Update(Time.deltaTime);
            ServerStatus[i].text = connected ? "연결" : "해제";
            ServerStatus[i].color = connected ? Color.green : Color.red;
        }
    }

    public void SendMessage()
    {
        ChatDataInfo data = new(eChatCommentType.SystemMsg, 0, "", "", 0, "", SBFunc.GetDateTimeToTimeStamp(), 0,
                        SBFunc.GetDateTimeToTimeStamp(), NoticeInput.text, 1, 0);

        for (int i = 0; i < (int)CHAT_SERVER.MAX; i++)
        {
            if (!SERVERS[i].IsSocketConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat CCConnectSocket => SendAchieveSystemMessage In false == IsSocketConnect"));
#endif                
                return;
            }
            SERVERS[i].SendMessage(data);
        }

        NoticeInput.text = "";
    }

    public void CheckMessage()
    {
        Set(NoticeInput.text);
    }

    private Queue<ToastManager.ToastData> toastQueue = new Queue<ToastManager.ToastData>();
    private Coroutine curUICoroutine = null;

    public void Set(string str, bool isSystem = false, int lifetime = 3, int diffyLevel = 0, bool isEffect = false)
    {
        if (toastQueue.Count > 0 && toastQueue.Peek().Str == str)
        {
            return;
        }

        toastQueue.Enqueue(new ToastManager.ToastData(str, isSystem, lifetime, diffyLevel, isEffect));

        if (curUICoroutine != null)
        {
            return;
        }

        curUICoroutine = StartCoroutine(Open());
    }

    private IEnumerator Open()
    {
        ToastMessage prevToast = null;
        while (toastQueue.TryDequeue(out ToastManager.ToastData curData))
        {
            var curToast = GameObject.Instantiate(toastPrefab, toastRoot);

            var toastMessage = curToast.GetComponent<ToastMessage>();
            toastMessage.SetData(curData.Str, curData.IsSystemToast, curData.LifeTime, curData.IsEffect, prevToast);
            prevToast = toastMessage;

            yield return SBDefine.GetWaitForSeconds(1.5f);
        }

        curUICoroutine = null;
        yield break;
    }
}
