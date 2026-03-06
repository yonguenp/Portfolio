using Coffee.UISoftMask;
﻿using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
//using Debug = UnityEngine.Debug;

public class HahahaChat : MonoBehaviour
{
    public InputField InputFeild;
    public ScrollRect ScrollRect;
    public GameObject ScrollContainer;

    public GameObject MessagePrefab_Mine;
    public GameObject MessagePrefab;
    public GameObject SamandaSceenCapturePrefab;
    public RectTransform ListViewRectTransform;
    public GameObject iconUP;
    public GameObject iconDOWN;

    public GameObject[] KeyboardOnOffObjects;

    float nContentHeight = 0;
    bool bOnContentSizeFitter = false;
    float scrollY = 0.0f;
    int nChildCount = 0;
    bool isAlphaAction = false;
    bool bForceScrollBottom = false;
    Dictionary<string, string> UserProfile = new Dictionary<string, string>();

    Coroutine KeyboardHeightUpdate = null;
    private Stopwatch mStopwatch = null;
    // 현재 설정된 채팅 UI의 목표 y 좌표.
    private float fKeyboardHeight0 = 0.0f;
    // 키보드 높이
    private float fKeyboardHeight1 = 0.0f;

    //bool bKeyBoardShow = false;
    
    class MessageStruct {
        public string accountNo;
        public string sender;
        public string msg;
        public bool isImage;
        public GameObject UI;
    };
    List<MessageStruct> MessageList = new List<MessageStruct>();

    // chat message object pool
    private Queue<GameObject> qMessagePool = new Queue<GameObject>();
    private Queue<GameObject> qMyMessagePool = new Queue<GameObject>();

    private void OnEnable()
    {
        SoftMask mask = ScrollRect.gameObject.GetComponent<SoftMask>();
        if (mask == null)
        {
            mask = ScrollRect.gameObject.AddComponent<SoftMask>();
            if (mask != null)
            {
                mask.showMaskGraphic = false;
                mask.softness = 0.3f;
            }
        }
    }
    void Start()
    {
        SamandaLauncher.SetSamandaCapturePrefab(SamandaSceenCapturePrefab);
        isAlphaAction = true;
        OnToggleAlphaAction();

        // pre-heat pool
        PrepareMessagePool();

        SamandaLauncher.onKeyboardVisible.AddListener(OnKeyboardVisible);
    }

    private void PrepareMessagePool(int otherSize = 30, int mySize = 5)
    {
        for (int i = 0; otherSize + mySize > i; ++i)
        {
            bool isMine = mySize > i;
            GameObject msgObj = CreateMessageItem(isMine);
            msgObj.SetActive(false);

            (isMine ? qMyMessagePool : qMessagePool).Enqueue(msgObj);
        }
    }

    private GameObject CreateMessageItem(bool isMine)
    {

        GameObject ret = Instantiate(isMine ? MessagePrefab_Mine : MessagePrefab);

        ret.name = "ChatMessage Item" + (isMine? "ME" : "");

        Graphic[] grap = ret.GetComponentsInChildren<Graphic>(true);
        foreach (var p in grap)
        {
            p.gameObject.AddComponent<Coffee.UISoftMask.SoftMaskable>();
        }

        HahahaChatMessage messageComponent = ret.GetComponent<HahahaChatMessage>();
        if (messageComponent)
        {
            messageComponent.SetOpacityForce(1.0f, isAlphaAction);
        }

        ret.transform.SetParent(ScrollContainer.transform);

        return ret;
    }

    // 풀에서 메시지 오브젝트를 꺼냄
    private GameObject DequeueMessageItem(bool isMine)
    {
        Queue<GameObject> q = isMine ? qMyMessagePool : qMessagePool;
        // queue에서 꺼낼 수 있으면 dequeue / 비었다면 생성
        GameObject ret = (0 < q.Count) ? q.Dequeue() : CreateMessageItem(isMine);
        // queue에서 나온 친구는 active하지 않다
        ret.SetActive(true);

        return ret;
    }

    // 사용 종료된 메시지 오브젝트를 큐로 반납
    private void EnqueueMessageItem(GameObject objMsg)
    {
        HahahaChatMessage msgComp = objMsg.GetComponent<HahahaChatMessage>();
        if (null == msgComp)
        {
            // 이상한 오브젝트를 전달받음. assert 필요
            return;
        }

        bool isMine = msgComp.is_mine;

        // parent에서 비활성화
        objMsg.SetActive(false);

        // 타입에 맞는 풀에 넣음
        (isMine ? qMyMessagePool : qMessagePool).Enqueue(objMsg);
    }

    void OnKeyboardVisible()
    {
        fKeyboardHeight1 = SamandaLauncher.GetRelativeKeyboardHeight(GetComponentInParent<Canvas>().GetComponent<RectTransform>(), true);
        //Debug.Log("[KB] OnKeyboardVisible. h0: " + fKeyboardHeight0 + " / h1: " + fKeyboardHeight1);

        if (fKeyboardHeight0 == fKeyboardHeight1)
        {
            //Debug.Log("[KB] fKeyboardHeight0 == fKeyboardHeight1");
            // 변화 없음
            return;
        }
        else if (fKeyboardHeight1 > fKeyboardHeight0)
        {
            // 키보드 높이가 높아졌다면 즉시 쫓아가야 함
            fKeyboardHeight0 = fKeyboardHeight1;
        }

        if (null == KeyboardHeightUpdate)
        {
            //Debug.Log("[KB] Starting coroutine");
            KeyboardHeightUpdate = StartCoroutine(UpdateKeyboardHeight());
        }
        else if (fKeyboardHeight1 > fKeyboardHeight0)
        {
            StopAndRemoveCoroutine();
            KeyboardHeightUpdate = StartCoroutine(UpdateKeyboardHeight());
        }
    }

    public IEnumerator UpdateKeyboardHeight()
    {
        // 키보드 내려가고 다시 올라오나 기다려 볼 시간 ms
        const long KEYBOARD_DESCENDING_DELAY = 200L;
        // 이것보다 가까우면 순간이동하여 도착
        const float VERY_CLOSE_THRESHOLD = 2.0f;
        // 근접할수록 느려지는데 정도 이상 느려지지 않음
        const float MINIMUM_SPEED = 4.0f;
        // 이동 속도 조절용 계수
        const float SPEED_COEFFICIENT = 10.0f;

        if (fKeyboardHeight1 < fKeyboardHeight0)
        {
            //Debug.Log("[KB] keyboard downgoing");
            // 키보드 내려가는 이벤트에는 즉시 반응하지 않는다
            while (true)
            {
                if (null == mStopwatch)
                {
                    //Debug.Log("[KB] descending. start stopwatch");
                    mStopwatch = new Stopwatch();
                    mStopwatch.Start();
                    yield return null;
                }
                else if (mStopwatch.ElapsedMilliseconds < KEYBOARD_DESCENDING_DELAY)
                {
                    yield return null;
                }
                else
                {
                    //Debug.Log("[KB] waited for 300ms. now start descend");
                    break;
                }
            }
        }

        mStopwatch = null;

        // 새로이 가야 할 목표지점
        fKeyboardHeight0 = fKeyboardHeight1;

        if(fKeyboardHeight0 == 0f)
        {
            fKeyboardHeight0 = 35f;
        }

        // 애니메이션
        foreach (GameObject obj in KeyboardOnOffObjects)
        {
            DOTweenAnimation[] anims = obj.GetComponentsInChildren<DOTweenAnimation>();
            foreach (DOTweenAnimation ani in anims)
            {
                ani.DORestart();
                if (50.0f > fKeyboardHeight0)
                {
                    // 키보드 숨김. UI 표시
                    ani.DOPlayForward();
                }
                else
                {
                    ani.DOPlayBackwards();
                }
            }
        }

        // 높이 조정
        RectTransform curRT = transform as RectTransform;
        //int loops = 5;
        //Debug.Log("[KB] pos beginning of coroutine " + curRT.localPosition);
        while (true)
        {
            Vector2 cur = curRT.localPosition;
            // 여기서 diff는 도착지점까지 거리
            float diff = fKeyboardHeight0 - cur.y;

            // animation end
            if (VERY_CLOSE_THRESHOLD > Math.Abs(diff))
            {
                //Debug.Log("[KB] arrived at target position " + fKeyboardHeight0);
                cur.y = fKeyboardHeight0;
                curRT.localPosition = cur;
                StopAndRemoveCoroutine();
                break;
            }

            // 여기서부터 diff는 이번 프레임에 이동할 거리
            if (MINIMUM_SPEED > Math.Abs(diff) * SPEED_COEFFICIENT)
            {
                diff *= MINIMUM_SPEED * Time.deltaTime / Math.Abs(diff);
            }
            else
            {
                diff *= Time.deltaTime * SPEED_COEFFICIENT;
            }

            if (Math.Abs(diff) <= VERY_CLOSE_THRESHOLD)
            {
                // 도착
                cur.y = fKeyboardHeight0;
            }
            else
            {
                // 한 틱 접근
                cur.y += diff;

            }

            //if (0 < loops--)
            //{
            //    Debug.Log("[KB] moving by " + diff);
            //    Debug.Log("[KB] after pos " + cur);
            //}

            curRT.localPosition = cur;

            yield return null;
        }
    }

    private void StopAndRemoveCoroutine()
    {
        //Debug.Log("[KB] Stopping Coroutine");
        if (null == KeyboardHeightUpdate)
        {
            return;
        }

        mStopwatch = null;
        StopCoroutine(KeyboardHeightUpdate);
        KeyboardHeightUpdate = null;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (TouchScreenKeyboard.visible && !bKeyBoardShow)
    //    {
    //        bKeyBoardShow = true;
    //        if (ScreenKeyboardCoroutine != null)
    //            StopCoroutine(ScreenKeyboardCoroutine);
    //        ScreenKeyboardCoroutine = StartCoroutine(OnKeyboardShow());
    //    }
    //    else if (!TouchScreenKeyboard.visible && bKeyBoardShow)
    //    {
    //        bKeyBoardShow = false;
    //        OnKeyboardHide();
    //    }
    //}

    //public IEnumerator OnKeyboardShow()
    //{
    //    foreach (GameObject obj in KeyboardOnOffObjects)
    //    {
    //        DOTweenAnimation[] anims = obj.GetComponentsInChildren<DOTweenAnimation>();
    //        foreach (DOTweenAnimation ani in anims)
    //        {
    //            ani.DORestart();
    //            ani.DOPlayBackwards();
    //        }
    //    }

    //    RectTransform curRT = transform as RectTransform;
    //    RectTransform inputField = InputFeild.transform as RectTransform;

    //    while (true)
    //    {
    //        float height = SamandaLauncher.GetRelativeKeyboardHeight(GetComponentInParent<Canvas>().GetComponent<RectTransform>(), true);
    //        Vector2 localPosition = curRT.localPosition;
    //        localPosition.y = height;
    //        curRT.localPosition = localPosition;

    //        yield return new WaitForSeconds(0.1f);
    //    }
    //}

    //public void OnKeyboardHide()
    //{
    //    if (ScreenKeyboardCoroutine != null)
    //        StopCoroutine(ScreenKeyboardCoroutine);

    //    ScreenKeyboardCoroutine = null;

    //    RectTransform curRT = transform as RectTransform;
        
    //    Vector2 localPosition = curRT.localPosition;
    //    localPosition.y = 0;
    //    curRT.localPosition = localPosition;

    //    foreach (GameObject obj in KeyboardOnOffObjects)
    //    {
    //        DOTweenAnimation[] anims = obj.GetComponentsInChildren<DOTweenAnimation>();
    //        foreach (DOTweenAnimation ani in anims)
    //        {
    //            ani.DORestart();
    //            ani.DOPlayForward();
    //        }
    //    }
    //}

    public void OnChatProfile(string nick, string url)
    {
        UserProfile.Add(nick, url);
    }

    public void OnChatRoom(string data)
    {
        switch(data)
        {
            case "openchat":
                break;
            case "1:1chat":
                break;
            case "groupchat":
                break;
            case "error":
                break;
        }

        ClearMsgList();
    }

    public void ClearMsgList()
    {
        foreach(MessageStruct msg in MessageList)
        {
            if(msg.UI)
            {
                EnqueueMessageItem(msg.UI);
                msg.UI = null;
            }
        }

        foreach(Transform child in ScrollContainer.transform)
        {
            child.gameObject.SetActive(false);
        }

        MessageList.Clear();
        ScrollRect.verticalNormalizedPosition = 0.0f;
    }

    public void OnChatMessage(string accountNo, string sender, string msg, bool isImage = false)
    {
        MessageStruct messageStruct = new MessageStruct();
        messageStruct.accountNo = accountNo;
        messageStruct.sender = sender;
        messageStruct.msg = msg;
        messageStruct.isImage = isImage;
        messageStruct.UI = null;
        MessageList.Add(messageStruct);

        if (MessageList.Count > 50)
        {
            for (int i = 0; i < 30; i++)
            {
                EnqueueMessageItem(MessageList[i].UI);
                MessageList[i].UI = null;
            }

            MessageList = MessageList.GetRange(30, MessageList.Count - 30);
            ScrollRect.verticalNormalizedPosition = 0.0f;
        }

        MakeChatMsgUI(MessageList.Count - 1);
    }

    public void MakeChatMsgUI(int index)
    {
        if(MessageList.Count <= index)
        {
            return;
        }

        if (MessageList[index].UI != null)
        {
            return;
        }

        string accountNo = MessageList[index].accountNo;
        string sender = MessageList[index].sender;
        string msg = MessageList[index].msg;
        bool isImage = MessageList[index].isImage;
        
        GameObject msgObject = null;

        if (sender.IndexOf("$") == 0)
        {
            sender = sender.Substring(1, sender.Length - 1);
            msgObject = DequeueMessageItem(true);
        }
        else
        {
            string userNick = sender;
            string[] nick = sender.Split(new string[] { "e960cdb67f2cb7488f16347705580180]" }, StringSplitOptions.None);
            if(nick.Length > 1)
            {
                userNick = nick[1];
            }

            if(SamandaLauncher.GetAccountNickName() == userNick)
            {
                msgObject = DequeueMessageItem(true);
            }
            else
            {
                msgObject = DequeueMessageItem(false);
            }            
        }

        if (msgObject)
        {
            msgObject.name = "ChatMessage Item";

            HahahaChatMessage messageComponent = msgObject.GetComponent<HahahaChatMessage>();
            if (messageComponent)
            {
                messageComponent.SetMessage(accountNo, sender, msg, isImage, "", neco_admin.GetAdmin(uint.Parse(accountNo)));

                messageComponent.SetOpacityForce(1.0f, isAlphaAction);
            }

            if (ScrollContainer)
            {
                MessageList[index].UI = msgObject;
                msgObject.transform.SetParent(ScrollContainer.transform);
                msgObject.GetComponent<RectTransform>().localScale = Vector3.one;
                msgObject.transform.localPosition = Vector3.zero;

                for (int i = 0; i < MessageList.Count; i++)
                {
                    if (MessageList[i].UI != null)
                    {
                        MessageList[i].UI.transform.SetSiblingIndex(i);
                    }
                }

                //메시지가 내꺼면 색상 지정해줘야함. 254,220,130

                foreach (RectTransform rt in msgObject.transform.GetComponentsInChildren<RectTransform>())
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(msgObject.GetComponent<RectTransform>());

                nContentHeight += (msgObject.GetComponent<RectTransform>().rect.height * msgObject.GetComponent<RectTransform>().localScale.y);//버티컬그룹이 호리젠탈 밑에 들어가서 제거함 + msgObject.GetComponent<VerticalLayoutGroup>().spacing;

                if (!bOnContentSizeFitter)
                {
                    if (ScrollContainer.GetComponent<RectTransform>().rect.height < nContentHeight)
                    {
                        bOnContentSizeFitter = true;
                        bForceScrollBottom = true;
                        ScrollContainer.GetComponent<ContentSizeFitter>().enabled = true;
                    }
                }


                if (bOnContentSizeFitter)
                {
                    if (bForceScrollBottom == false && ScrollRect.verticalNormalizedPosition >= 1.0f / ScrollContainer.transform.childCount)
                    {
                        return;
                    }

                    LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollContainer.GetComponent<RectTransform>());

                    ScrollRect.verticalNormalizedPosition = 0.0f;
                    bForceScrollBottom = false;
                }
            }
            else
            {
                EnqueueMessageItem(msgObject);
            }
        }

        //if(iconUP.activeSelf == false && iconDOWN.activeSelf == false && ScrollContainer.GetComponent<RectTransform>().rect.height > 935 / 2)
        //    ToggleViewSize();
    }

    public void OnSendScreenShot()
    {
        SamandaLauncher.ClickScreenShot();
    }

    public void OnScrollControll(Vector2 vec)
    {
        if (isAlphaAction == false)
            return;

        if (vec.y < 0)
            return;

        if (ScrollContainer.transform.childCount != nChildCount)
        {
            nChildCount = ScrollContainer.transform.childCount;
            scrollY = vec.y;
            return;
        }

        if (Math.Abs(scrollY - vec.y) > (1.0f / nChildCount))
        {
            scrollY = vec.y;

            HahahaChatMessage[] msgCompo = ScrollContainer.transform.GetComponentsInChildren<HahahaChatMessage>();            
            for (int i = msgCompo.Length - 1; i >= 0; i--)
            {
                msgCompo[i].SetOpacityForce(1.0f, isAlphaAction);

                //iTween.Stop(msgCompo[i].gameObject);
                //iTween.ValueTo(msgCompo[i].gameObject, iTween.Hash("from", 1.0f, "to", 0, "easetype", iTween.EaseType.easeInQuad, "time", 5.0f, "onupdate", "SetOpacity", "onupdatetarget", msgCompo[i].gameObject));
            }
        }
    }

    public void OnToggleAlphaAction()
    {
        isAlphaAction = !isAlphaAction;

        Transform NotificationBell = gameObject.transform.Find("Notification Bell");
        if(NotificationBell)
        {
            Transform Bell = NotificationBell.Find("Bell");
            if(Bell)
            {
                Transform Filled = Bell.Find("Filled");
                if(Filled)
                {
                    Filled.gameObject.SetActive(!isAlphaAction);
                }
            }
        }

        HahahaChatMessage[] msgCompo = ScrollContainer.transform.GetComponentsInChildren<HahahaChatMessage>();
        for (int i = msgCompo.Length - 1; i >= 0; i--)
        {
            msgCompo[i].SetOpacityForce(isAlphaAction ? 0.0f : 1.0f, false);            
            //iTween.Stop(msgCompo[i].gameObject);
            //iTween.ValueTo(msgCompo[i].gameObject, iTween.Hash("from", msgCompo[i].user_name.color.a, "to", isAlphaAction ? 0.0f : 1.0f, "easetype", iTween.EaseType.easeInQuad, "time", 0.5f, "onupdate", "SetOpacity", "onupdatetarget", msgCompo[i].gameObject));           
        }
    }

    public void ToggleViewSize()
    {
        Vector2 size = ListViewRectTransform.sizeDelta;
        if (size.y == 935)
        {
            size.y = 935 / 2;
            ListViewRectTransform.sizeDelta = size;
            iconUP.SetActive(true);
            iconDOWN.SetActive(false);
        }
        else
        {
            size.y = 935;
            ListViewRectTransform.sizeDelta = size;
            iconUP.SetActive(false);
            iconDOWN.SetActive(true);
        }

        ScrollRect.verticalNormalizedPosition = 0.0f;
    }
}
