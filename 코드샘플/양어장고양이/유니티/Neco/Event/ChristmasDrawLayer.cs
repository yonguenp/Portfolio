using Crosstales;
using DG.Tweening.Core;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasDrawLayer : MonoBehaviour
{
    #region Draw Info
    
    public class DrawInfo
    {
        public enum ItemType { NONE, CASH, ITEM, BOX, MEMORY };
        public ItemType type;
        public uint id;
        public uint amount;
        public DrawInfo(uint _type, uint _id, uint _amount)
        {
            type = (ItemType)_type;
            id = _id;
            amount = _amount;
        }

        public DrawInfo(string _type, uint _id, uint _amount)
        {
            switch (_type)
            {
                case "gold":
                case "catnip":
                case "point":
                    type = ItemType.CASH;
                    break;
                case "item":
                    type = ItemType.ITEM;
                    break;
                case "box":
                    type = ItemType.BOX;
                    break;
                case "memory":
                    type = ItemType.MEMORY;
                    break;
            }
            id = _id;
            amount = _amount;
        }

        public RewardData ToRewardData()
        {
            RewardData reward = new RewardData();

            switch (type)
            {
                case ItemType.CASH:
                    if (id == 0)
                    {
                        reward.gold = amount;
                    }
                    else if (id == 1)
                    {
                        reward.catnip = amount;
                    }
                    else
                    {
                        reward.point = amount;
                    }
                    break;
                case ItemType.ITEM:
                    reward.itemData = items.GetItem(id);
                    reward.count = amount;
                    break;
                case ItemType.BOX:
                    reward.itemData = items.GetItem(id);
                    reward.count = amount;
                    break;
                case ItemType.MEMORY:
                    break;
            }

            return reward;
        }
    }

    List<DrawInfo> drawsList = null;        //드롭된 아이템 최대 13개
    List<GameObject> drawListObj = null;    //드롭된 아이템 오브젝트
    DrawInfo drawinfo;  

    #endregion

    const int MAX_BOX_LEVEL = 13;
    enum DRAW_BUTTONSTATE
    {
        DRAWWAIT,       //애니메이션을 위한 WAIT
        DRAWSET,        //시작 안함
        DRAWPAUSE,      //서버에 상자열던 기록이 있었음
        DRAWED_SUCCESS, //받기
        DRAWED_FAIL,    //돌아가기
        DRAW_SELECTION  //그만하기, 한번 더
    }

    enum DRAW_STATE
    {
        NONE = 0,         // 첫 게임 시작 전
        RUNNING = 1,      // 게임 진행중
        FAILED = 2,       // 상자 터짐
    }

    [Header("Objects")]
    public RewardInfo drawRewardInfo;
    
    public Button drawStartBtn;
    public Button drawSuccessBtn;
    public Button drawFailBtn;
    public GameObject drawResultSet;
    public GameObject drawGoBtn;

    public GameObject drawsClone;
    public RectTransform drawsContentRt;

    public Text boxCountText;

    [Header("Animations")]
    public Animator bulbAnimator;
    public Animation itemAnimation;
    public SkeletonGraphic cboxskeleton;
    public ParticleSystem itemParticle;
    public Sprite failIcon;

    Coroutine drawRoutine = null;

    [Header("Properties")]
    uint boxcount = 0;
    public float boxAniTime = 1.5f;
    public float itemParticleTime = 0.5f;
    
    public bool staticResult = true;

    public uint currentLevel = 0;

    private void OnEnable()
    {
        GetBoxInfo();
    }

    void GetBoxInfo()
    {
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 10);

        NetworkManager.GetInstance().SendApiRequest("event", 10, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            currentLevel = 0;
                            JObject info = (JObject)row["info"];
                            Init();

                            if(info["state"].Value<uint>() != 2 && info["rewards"] != null && info["rewards"].Type == JTokenType.Array && ((JArray)info["rewards"]).Count > 0)
                            {
                                currentLevel = info["level"].Value<uint>();

                                JArray rewards = (JArray)info["rewards"];
                                foreach (JToken r in rewards)
                                {
                                    JArray cur_reward = (JArray)r;
                                    string type = cur_reward[0].Value<string>();
                                    uint id = cur_reward[1].Value<uint>();

                                    if (type == "dia")
                                    {
                                        type = "catnip";
                                        id = 1;
                                    }
                                    else if (type == "point")
                                    {
                                        id = 2;
                                    }

                                    drawsList.Add(new DrawInfo(type, id, cur_reward[2].Value<uint>()));
                                }
                                RefreshDrawsList();
                                SetButtonState(DRAW_BUTTONSTATE.DRAW_SELECTION);
                            }

                            //NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("이벤트종료안내"), ()=> { NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHRISTMAS_EVENT)});
                        }
                        else
                        {
                            ChristmasEventPopup.ChristmasEventWWWErrorCode(rs, true);
                        }
                    }
                }
            }
        });
    }

    void Init()
    {
        if (drawRoutine != null)
        {
            StopCoroutine(drawRoutine);
            drawRoutine = null;
        }

        InitDraw();
        DrawIdle();
    }

    void InitDraw()
    {
        if (drawsList == null)
            drawsList = new List<DrawInfo>();

        if (drawListObj == null)
        {
            drawListObj = new List<GameObject>();

            for(uint i = 0; i < MAX_BOX_LEVEL; i++)
            {
                GameObject obj = Instantiate(drawsClone, drawsContentRt.transform);
                obj.GetComponent<ChristmasDrawListInfo>().Init(i+1);
                obj.SetActive(true);
                drawListObj.Add(obj);
            }
        }
        
        drawListObj.ForEach((obj) => obj.GetComponent<ChristmasDrawListInfo>().Deactive());
        drawsList.Clear();

        boxcount = user_items.GetUserItemAmount(159);
        boxCountText.text = boxcount.ToString();
        cboxskeleton.AnimationState.SetAnimation(0, "animation3", false);
        SetButtonState(DRAW_BUTTONSTATE.DRAWSET);
        itemAnimation.Play("Cidle_ani");
        RefreshDrawsList(true);
    }

    public void SetDraw(int op = 11)
    {
        if (drawRoutine != null)
        {
            StopCoroutine(drawRoutine);
            drawRoutine = null;
        }
        //WWW통신
        //스타트 또는 고

        if(currentLevel == MAX_BOX_LEVEL)
        {
            //Err 상자 레벨이 MAX 입니다. 더이상 열 수 없습니다.
            string msg = "";
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), msg);

            return;
        }

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", op);

        NetworkManager.GetInstance().SendApiRequest("event", op, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            JObject info = (JObject)row["info"];
                            currentLevel = info["level"].Value<uint>();
                            switch ((DRAW_STATE)info["state"].Value<uint>())
                            {
                                case DRAW_STATE.RUNNING:
                                    staticResult = true;
                                    
                                    JArray rewards = (JArray)info["rewards"];
                                    int index = rewards.Count - 1;
                                    JArray cur_reward = (JArray)rewards[index];

                                    string type = cur_reward[0].Value<string>();
                                    uint id = cur_reward[1].Value<uint>();

                                    if (type == "dia")
                                    {
                                        type = "catnip";
                                        id = 1;
                                    }
                                    else if(type == "point")
                                    {
                                        id = 2;
                                    }

                                    drawinfo = new DrawInfo( type, id, cur_reward[2].Value<uint>());
                                    SwapDrawImage(drawinfo);
                                    drawsList.Add(drawinfo);
                                    break;

                                case DRAW_STATE.FAILED:
                                    staticResult = false;
                                    break;
                            }
                            
                            SetButtonState(DRAW_BUTTONSTATE.DRAWWAIT);
                            drawRoutine = StartCoroutine(Draw());
                            //NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("이벤트종료안내"), ()=> { NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHRISTMAS_EVENT)});
                        }
                        else
                        {
                            ChristmasEventPopup.ChristmasEventWWWErrorCode(rs, true);
                        }
                    }
                }
            }
        });
    }

    public void ExitDraw()
    {
        if (drawRoutine != null)
        {
            StopCoroutine(drawRoutine);
            drawRoutine = null;
        }

        //WWW통신
        //상자깡 중단

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 13);

        NetworkManager.GetInstance().SendApiRequest("event", 13, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            currentLevel = 0;
                            List<RewardData> ret = new List<RewardData>();

                            if (row.ContainsKey("rew"))
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.gold = income["gold"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("catnip"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.catnip = income["catnip"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("point"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.point = income["point"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                        reward.count = rw["amount"].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }

                                if (income.ContainsKey("memory"))
                                {
                                    JArray memory = (JArray)income["memory"];

                                    Dictionary<string, uint> memoryDic = new Dictionary<string, uint>();
                                    memoryDic.Add("point", 0);
                                    foreach (JArray rw in memory)
                                    {
                                        neco_cat_memory catMemory = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                        if (catMemory == null) continue;

                                        if (rw[1].Value<uint>() > 0)
                                        {
                                            memoryDic["point"] += rw[1].Value<uint>();  // 포인트로 합산
                                        }
                                        else if (memoryDic.ContainsKey(catMemory.GetNecoMemoryID().ToString()) == false)
                                        {
                                            memoryDic.Add(catMemory.GetNecoMemoryID().ToString(), 1);
                                        }
                                    }

                                    foreach (var memoryPair in memoryDic)
                                    {
                                        RewardData reward = new RewardData();

                                        if (memoryPair.Key == "point")
                                        {
                                            reward.point = memoryPair.Value;
                                        }
                                        else
                                        {
                                            reward.memoryData = neco_cat_memory.GetNecoMemory(uint.Parse(memoryPair.Key));
                                        }

                                        ret.Add(reward);
                                    }
                                }
                            }

                            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_233"), LocalizeData.GetText("Xmasbox_openReward"), ret, ()=> {
                                InitDraw();
                                DrawIdle();
                            });
                            //NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("이벤트종료안내"), ()=> { NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHRISTMAS_EVENT)});
                        }
                        else
                        {
                            ChristmasEventPopup.ChristmasEventWWWErrorCode(rs, true);
                        }
                    }
                }
            }
        });
    }

    IEnumerator Draw()
    {
        cboxskeleton.AnimationState.ClearTrack(0);

        bulbAnimator.SetTrigger("Ing");
        yield return new WaitForSeconds(Random.Range(0.6f, 2f));

        cboxskeleton.AnimationState.SetAnimation(0, staticResult ? "animation" : "animation2", false);
        yield return new WaitForSeconds(boxAniTime);
        
        Debug.Log("등장!");
        bulbAnimator.SetTrigger("Success");
        
        yield return new WaitForSeconds(itemParticleTime);
        itemParticle.Play();

        if (staticResult)
        {
            //성공
            itemAnimation.Play("Creward_ani");


            //SwapDrawImage(rewards[rand]);
            //drawsList.Add(rewards[rand]);
            RefreshDrawsList();

            yield return new WaitForFixedUpdate();
            SetButtonState(DRAW_BUTTONSTATE.DRAWED_SUCCESS);
        }
        else
        {
            //실패
            drawRewardInfo.rewardIcon.sprite = failIcon;
            drawRewardInfo.rewardCountText.text = "";
            drawRewardInfo.rewardItemNameText.text = "";

            itemAnimation.Play("Cfail_ani");
            SetButtonState(DRAW_BUTTONSTATE.DRAWED_FAIL);
        }

        yield return null;
    }

    void DrawIdle()
    {
        bulbAnimator.SetTrigger("Idle");
        itemAnimation.Play("Cidle_ani");
    }

    void SwapDrawImage(DrawInfo drawInfo)
    {
        drawRewardInfo.SetRewardInfoData(drawInfo.ToRewardData());
    }

    void RefreshDrawsList(bool clear = false)
    {
        if(clear)
        {
            drawListObj.ForEach((obj) => obj.GetComponent<ChristmasDrawListInfo>().Deactive());
            drawsList.Clear();
            LayoutRebuilder.ForceRebuildLayoutImmediate(drawsContentRt);
            return;
        }

        if (drawsList.Count < 1)
            return;

        for(int i = 0; i < drawsList.Count; i++)
        {
            drawListObj[i].GetComponent<ChristmasDrawListInfo>().SetReward(drawsList[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(drawsContentRt);
    }

    void SetButtonState(DRAW_BUTTONSTATE state)
    {
        drawStartBtn.gameObject.SetActive(false);
        drawSuccessBtn.gameObject.SetActive(false);
        drawFailBtn.gameObject.SetActive(false);
        drawResultSet.SetActive(false);
        drawGoBtn.SetActive(false);

        switch (state)
        {
            case DRAW_BUTTONSTATE.DRAWWAIT: return;
            case DRAW_BUTTONSTATE.DRAWSET:
                drawStartBtn.interactable = boxcount > 0;
                drawStartBtn.gameObject.SetActive(true);
                break;
            case DRAW_BUTTONSTATE.DRAWPAUSE:
                drawResultSet.SetActive(true);
                break;
            case DRAW_BUTTONSTATE.DRAWED_SUCCESS:
                drawSuccessBtn.gameObject.SetActive(true);
                break;
            case DRAW_BUTTONSTATE.DRAWED_FAIL:
                drawFailBtn.gameObject.SetActive(true);
                break;
            case DRAW_BUTTONSTATE.DRAW_SELECTION:
                drawResultSet.SetActive(true);
                if(currentLevel < 13)
                    drawGoBtn.SetActive(true);
                else
                    drawGoBtn.SetActive(false);
                break;
        }
    }

    public void OnClickSuccessBtn()
    {
        //받은 아이템 담음
        cboxskeleton.AnimationState.SetAnimation(0, "animation3", false);
        itemAnimation.Play("Cidle_ani");
        SetButtonState(DRAW_BUTTONSTATE.DRAW_SELECTION);
    }

    public void OnClickFailBtn()
    {
        //상자 초기화
        InitDraw();
    }
}
