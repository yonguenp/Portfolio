using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoCatSpotContainer : MonoBehaviour
{
    public uint curSlotIndex;
    public BoneFollowerGraphic EventIcon;
    public GameObject[] CatObjects = new GameObject[13];
    

    private neco_spot curSpot;
    private neco_cat curCatData;

    bool isMovieInteraction = false;
    List<RewardData> rewards;

   [ContextMenu("auto apply")]
    public void Auto_Apply()
    {
        EventIcon = GetComponentInChildren<BoneFollowerGraphic>(true);        
    }

    public void Awake()
    {
        if(curSlotIndex <= 0)
            Debug.LogError("need curSpot Slot Index !");

        if (EventIcon != null)
        {
            EventIcon.gameObject.SetActive(false);
            Button button = EventIcon.GetComponent<Button>();
            if(button != null)
            {                
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnCatInfo);
            }
        }

        foreach(GameObject catObject in CatObjects)
        {
            if(catObject != null)
            {
                Button buttons = catObject.GetComponentInChildren<Button>(true);
                if (buttons != null)
                {
                    buttons.onClick.RemoveAllListeners();
                    buttons.onClick.AddListener(OnCatInfo);
                }
            }
        }
    }
    public virtual void RefreshCat(neco_spot spot)
    {
        curSpot = spot;
        
        neco_cat cat = curSpot.GetCurSpotCat(curSlotIndex - 1);
        if(cat == null)
        {
            curCatData = null;
            ClearCatObject();
            return;
        }

        if (curCatData == cat)
        {
            if(curCatData.GetVisitParam() == 0)
                return;
        }

        curCatData = cat;
        ClearCatObject();

        if (EventIcon != null)
        {
            EventIcon.SkeletonGraphic = null;
            EventIcon.gameObject.SetActive(false);            
        }

        GameObject targetCatObject = CatObjects[cat.GetCatID()];
        if (targetCatObject != null)
        {
            targetCatObject.SetActive(true);

            if (EventIcon != null)
            {
                bool bAction = curCatData.GetSuddenType() != neco_cat.CAT_SUDDEN_STATE.NONE && spot.GetSpotType() != neco_spot.SPOT_TYPE.FOOD_SPOT;
                
                if (bAction)
                {
                    SkeletonGraphic[] skeletons = targetCatObject.GetComponentsInChildren<SkeletonGraphic>();
                    foreach (SkeletonGraphic sk in skeletons)
                    {
                        Bone bone = sk.Skeleton.FindBone("pivot");
                        if (bone != null)
                        {
                            RectTransform rt = (EventIcon.transform as RectTransform);
                            rt.localRotation = Quaternion.identity;
                            rt.localScale = Vector3.zero;

                            EventIcon.gameObject.SetActive(true);
                            EventIcon.SkeletonGraphic = sk;
                            EventIcon.SetBone("pivot");
                            EventIcon.followBoneRotation = false;
                            EventIcon.followXYPosition = true;
                            EventIcon.followZPosition = false;
                            EventIcon.followLocalScale = false;
                            EventIcon.followSkeletonFlip = false;
                        }

                        sk.color = new Color(1f, 1f, 1f, 0f);
                        sk.DOColor(Color.white, 1.0f);
                    }

                    if (EventIcon.gameObject.activeInHierarchy)
                    {
                        if (EventIcon.gameObject.transform.childCount > 1)
                        {
                            Transform IconTransfrom = EventIcon.gameObject.transform.GetChild(1);
                            if (IconTransfrom != null)
                            {
                                // 돌발 아이콘 관련 처리
                                Image eventIconImage = IconTransfrom.GetComponent<Image>();
                                if (eventIconImage != null)
                                {
                                    switch (curCatData.GetSuddenType())
                                    {
                                        case neco_cat.CAT_SUDDEN_STATE.NONE:
                                            eventIconImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_feelmark");
                                            break;
                                        case neco_cat.CAT_SUDDEN_STATE.TOUCH:
                                            eventIconImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_hand");
                                            break;
                                        case neco_cat.CAT_SUDDEN_STATE.WATCH:
                                            eventIconImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_watch");
                                            break;
                                        case neco_cat.CAT_SUDDEN_STATE.PHOTO:
                                            eventIconImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_picture");
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("eventIconImage notfount");
                            }
                        }
                        else
                        {
                            Debug.Log("EventIcon.gameObject.transform.childCount <= 1");
                        }
                    }
                    else
                    {
                        Debug.Log("EventIcon active == false");
                    }
                }
            }
        }

        MapObjectController controller = NecoCanvas.GetGameCanvas().GetCurMapController();
        if(controller)
        {
            controller.RefreshCat(curCatData);
        }

        StartCoroutine(CatLeaveOut());
    }

    public IEnumerator CatLeaveOut()
    {
        uint leaveTime = curCatData.GetLeaveTime();
        if (leaveTime > 0)
        {
            if (leaveTime > NecoCanvas.GetCurTime())
            {
                uint remain = leaveTime - NecoCanvas.GetCurTime();
                Debug.Log("Cat Leave Remain Time : " + remain);
                yield return new WaitForSeconds(remain);
            }
        }

        Debug.Log("Cat Leave now !");

        neco_cat cat = curSpot.GetCurSpotCat(curSlotIndex - 1);
        if(cat == curCatData)
        {
            curSpot.GoneSpotCat(curSlotIndex - 1);
        }
    }

    public void ClearCatObject()
    {
        foreach(GameObject catObject in CatObjects)
        {
            if(catObject)
            {
                catObject.SetActive(false);
            }
        }

        if (EventIcon != null)
        {
            EventIcon.SkeletonGraphic = null;
            EventIcon.gameObject.SetActive(false);
        }
        StopAllCoroutines();
    }

    public void OnCatSuddenAction()
    {
        neco_cat curCat = curCatData;
        if (curCat != null)
        {
            //if (curSpot.GetSpotID() == 1) 
            //{
            //    //어미냥이
            //    if (curCat.GetCatID() == 1 && neco_data.DebugSeq == 0)
            //    {
            //        neco_data.ShownFlag.Add(1003);
            //        NecoCanvas.GetVideoCanvas().OnVideoPlay(1003, ()=> {
            //            if (EventIcon != null)
            //            {
            //                EventIcon.gameObject.SetActive(false);
            //            }

            //            NecoCanvas.GetGameCanvas().LoadMap(2);
            //        });

            //        neco_data.DebugSeq = 1;
            //        RefreshCat(curSpot);
            //        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();
            //        //if (neco_data.ClientDEBUG_Seq.MOTHER_ACTION == seq)
            //        //{
            //        //    neco_data.SetDebugSeq(neco_data.ClientDEBUG_Seq.FOOD_TOUCH_1ST);
            //        //}

            //        return;
            //    }
            //    //길막
            //    if (curCat.GetCatID() == 2 && neco_data.DebugSeq == 1)
            //    {
            //        neco_data.ShownFlag.Add(2003);
            //        NecoCanvas.GetVideoCanvas().OnVideoPlay(2003, () => {
            //            if (EventIcon != null)
            //            {
            //                EventIcon.gameObject.SetActive(false);
            //            }
            //        });

            //        RefreshCat(curSpot);
            //        return;
            //    }
            //    //삼색
            //    if (curCat.GetCatID() == 3 && neco_data.DebugSeq == 2)
            //    {
            //        neco_data.ShownFlag.Add(3003);
            //        NecoCanvas.GetVideoCanvas().OnVideoPlay(3003, () => {
            //            if (EventIcon != null)
            //            {
            //                EventIcon.gameObject.SetActive(false);
            //            }                                                
            //        });

            //        neco_data.DebugSeq = 3;
            //        RefreshCat(curSpot);
            //        return;
            //    }
            //}

            //if (curSpot.GetSpotID() == 2)
            //{
            //    if (curCat.GetCatID() == 1 && neco_data.DebugSeq == 1)
            //    {
            //        neco_data.ShownFlag.Add(1101);
            //        NecoCanvas.GetVideoCanvas().OnVideoPlay(1101, () => {
            //            if (EventIcon != null)
            //            {
            //                EventIcon.gameObject.SetActive(false);
            //            }
            //        });

            //        RefreshCat(curSpot);
            //        return;
            //    }
            //}

            switch (curCat.GetSuddenType())
            {
                case neco_cat.CAT_SUDDEN_STATE.PHOTO:
                    WWWForm data = new WWWForm();
                    data.AddField("api", "object");
                    data.AddField("op", 4);
                    data.AddField("oid", curSpot.GetSpotID().ToString());
                    data.AddField("slot", curSlotIndex.ToString());

                    NetworkManager.GetInstance().SendApiRequest("object", 4, data, (response) =>
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
                            if (uri == "object")
                            {
                                JToken resultCode = row["rs"];

                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        //List<RewardData> ret = new List<RewardData>();
                                        if (row.ContainsKey("rew"))
                                        {
                                            JObject income = (JObject)row["rew"];
                                            //if (income.ContainsKey("gold"))
                                            //{
                                            //    RewardData reward = new RewardData();
                                            //    reward.gold = income["gold"].Value<uint>();
                                            //    ret.Add(reward);
                                            //}

                                            //if (income.ContainsKey("catnip"))
                                            //{
                                            //    RewardData reward = new RewardData();
                                            //    reward.catnip = income["catnip"].Value<uint>();
                                            //    ret.Add(reward);
                                            //}

                                            //if (income.ContainsKey("item"))
                                            //{
                                            //    JArray item = (JArray)income["item"];
                                            //    foreach (JObject rw in item)
                                            //    {
                                            //        RewardData reward = new RewardData();
                                            //        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                            //        reward.count = rw["amount"].Value<uint>();
                                            //        ret.Add(reward);
                                            //    }
                                            //}

                                            if (income.ContainsKey("memory"))
                                            {
                                                JArray memory = (JArray)income["memory"];
                                                foreach (JArray rw in memory)
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.memoryData = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                                    reward.point = rw[1].Value<uint>();
                                                    GetPhotoPopup(reward);
                                                    return;
                                                }
                                            }
                                        }

                                        return;
                                    }
                                    else
                                    {
                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_339"));
                                    }
                                }
                            }
                        }

                        if (curCatData != null)
                        {
                            //서버 작업이 되었다면 여길타면 안됨.
                            RewardData tmpReward = new RewardData();
                            tmpReward.memoryData = neco_cat_memory.GetNecoMemory(curCatData.GetVisitParam());
                            GetPhotoPopup(tmpReward);
                        }
                    });
                    break;
                case neco_cat.CAT_SUDDEN_STATE.TOUCH:
                    NecoCanvas.GetPopupCanvas().OnShowCatTouchPopup(neco_cat.CAT_SUDDEN_STATE.TOUCH, curCat, PlayVideo);
                    break;
                case neco_cat.CAT_SUDDEN_STATE.WATCH:
                    NecoCanvas.GetPopupCanvas().OnShowCatTouchPopup(neco_cat.CAT_SUDDEN_STATE.WATCH, curCat, PlayVideo);
                    break;
                default:
                    break;

            }
        }
    }

    public void PlayVideo()
    {
        if (curCatData != null)
        {
            if (curCatData.GetSuddenType() == neco_cat.CAT_SUDDEN_STATE.TOUCH || curCatData.GetSuddenType() == neco_cat.CAT_SUDDEN_STATE.WATCH)
            {
                if (curCatData.GetVisitParam() != 0)
                {
                    NecoCanvas.GetVideoCanvas().OnVideoPlay(curCatData.GetVisitParam(), OnSuddenVideoDone);
                    curCatData.ClearSuddenEvent();
                    return;
                }
            }
        }

        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_460"));
    }

    public void GetPhotoPopup(RewardData reward)
    {
        isMovieInteraction = false;
        NecoCanvas.GetPopupCanvas().OnShowGetCatPhotoPopup(curCatData, reward.memoryData, reward.point <= 0, ()=> { PhotoPopupDone(reward); });
        curCatData.ClearSuddenEvent();
    }

    public void PhotoPopupDone(RewardData reward)
    {
        if (reward.point > 0)
        {
            reward.memoryData = null;
            NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_242"), LocalizeData.GetText("LOCALIZE_243"), reward, RewardDone);
        }
        else
        {
            RewardDone();
        }
    }

    public void OnCatInfo()
    {
        if (curCatData == null)
            return;

        if(EventIcon != null)
        {
            if (EventIcon.gameObject.activeSelf)
            {
                OnCatSuddenAction();
                EventIcon.gameObject.SetActive(false);
                return;
            }
        }

        if (curCatData.GetSuddenType() != neco_cat.CAT_SUDDEN_STATE.NONE)
            return;

        if (NecoCanvas.GetVideoCanvas().gameObject.activeSelf)
            return;

        //NecoCanvas.GetPopupCanvas().OnCatDetailPopupShow(curCatData);
    }

    public void OnSuddenVideoDone()
    {
        isMovieInteraction = true;

        WWWForm data = new WWWForm();
        data.AddField("api", "object");
        data.AddField("op", 4);
        data.AddField("oid", curSpot.GetSpotID().ToString());
        data.AddField("slot", curSlotIndex.ToString());

        NetworkManager.GetInstance().SendApiRequest("object", 4, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            rewards = new List<RewardData>();
            bool newMemory = false;
            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "object")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        switch (op)
                        {
                            case 4: 
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        if (row.ContainsKey("rew"))
                                        {
                                            if (row["rew"].HasValues)
                                            {
                                                JObject income = (JObject)row["rew"];
                                                if (income.ContainsKey("gold"))
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.gold = income["gold"].Value<uint>();
                                                    rewards.Add(reward);
                                                }


                                                if (income.ContainsKey("item"))
                                                {
                                                    JArray item = (JArray)income["item"];
                                                    foreach (JObject rw in item)
                                                    {
                                                        RewardData reward = new RewardData();
                                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                                        reward.count = rw["amount"].Value<uint>();
                                                        rewards.Add(reward);
                                                    }
                                                }

                                                if (income.ContainsKey("memory"))
                                                {
                                                    JArray memory = (JArray)income["memory"];
                                                    foreach (JArray rw in memory)
                                                    {
                                                        RewardData reward = new RewardData();
                                                        reward.memoryData = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                                        reward.point = rw[1].Value<uint>();
                                                        rewards.Add(reward);

                                                        if (reward.point == 0)
                                                        {
                                                            NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_MEMORIES, rw[0].Value<uint>(), 0, RewardShow);
                                                            newMemory = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //uint type = 0;
                                        //if (row.ContainsKey("type"))
                                        //    type = row["type"].Value<uint>();

                                        //if (row.ContainsKey("clip"))
                                        //{
                                        //    if(type == 2)
                                        //    {
                                        //        NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_MEMORIES, row["clip"].Value<uint>(), 0, RewardShow);
                                        //    }
                                        //    else
                                        //    {
                                        //        RewardShow();
                                        //    }
                                        //    return;
                                        //}
                                    }
                                    else
                                    {
                                        string msg = rs.ToString();
                                        switch (rs)
                                        {
                                            case 1: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                            case 2: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                            case 3: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                            case 4: msg = LocalizeData.GetText("LOCALIZE_504"); break;
                                        }

                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if(!newMemory)
                RewardShow();

            if (neco_data.PrologueSeq.길막이만지기돌발발생 == neco_data.GetPrologueSeq())
            {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                {
                    mapController.SendMessage("길막이낚시만지기돌발체크", SendMessageOptions.DontRequireReceiver);
                }
            }
        });
    }

    public void RewardShow()
    {
        if (rewards.Count > 0)
        {
            if (rewards.Count == 1 && rewards[0].point > 0)
            {
                rewards[0].memoryData = null;                
                NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_242"), LocalizeData.GetText("LOCALIZE_243"), rewards[0], RewardDone);
            }
            else
            {
                NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_242"), LocalizeData.GetText("LOCALIZE_244"), rewards, RewardDone);
            }
        }
        else
        {
            RewardDone();
        }
    }

    public void RewardDone()
    {
        MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (mapController != null)
        {
            switch (neco_data.GetPrologueSeq())
            {
                case neco_data.PrologueSeq.길막이낚시돌발완료:
                    mapController.SendMessage("길막이낚시돌발완료", SendMessageOptions.DontRequireReceiver);
                    break;                
                case neco_data.PrologueSeq.돌발아이콘터치:
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.돌발아이콘터치, SendMessageOptions.DontRequireReceiver);
                    break;
                //case neco_data.PrologueSeq.보온캣하우스삼색사진:
                //    if (curCatData.GetCatID() == 3 && curSpot.GetSpotID() == 8 && !isMovieInteraction)
                //        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.보온캣하우스삼색사진, SendMessageOptions.DontRequireReceiver);
                //    break;
            }
        }
    }
}
