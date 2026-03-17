using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SandboxNetwork
{
    public enum eFriendEventType
    {
        REGIST,
        REMOVE,
        REFRESH,
        MAX
    }
    public struct FriendEvent
    {
        public static FriendEvent e = default;
        public eFriendEventType eType { get; set; }
        public List<FriendUserData> FriendData { get; set; }
        public long UserUID { get; set; }
        public static void SendRegist(long registUserUID)
        {
            e.eType = eFriendEventType.REGIST;
            e.FriendData = null;
            e.UserUID = registUserUID;

            EventManager.TriggerEvent(e);
        }
        public static void SendRefersh(List<FriendUserData> friendData)
        {
            e.eType = eFriendEventType.REFRESH;
            e.FriendData = friendData;
            e.UserUID = -1;

            EventManager.TriggerEvent(e);
        }
        public static void SendRemove(long removeUserUID)
        {
            e.eType = eFriendEventType.REMOVE;
            e.FriendData = null;
            e.UserUID = removeUserUID;

            EventManager.TriggerEvent(e);
        }
    }
    public class FriendManager : IManagerBase
    {
        private static FriendManager instance = null;
        public static FriendManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FriendManager();
                }
                return instance;
            }
        }

        public static bool IsLoaded { get; private set; } = false;
        public static Dictionary<long, FriendUserData> FriendIdList { get; private set; } = new Dictionary<long, FriendUserData>();

        public delegate void FriendCallback(List<FriendUserData> infodatas);
        public delegate void FriendRemoveCB(long id);

        public int TodaySendCount { get; set; } = 0;//오늘 하루 보낸 카운트

        public void Initialize()
        {

        }
        public void Update(float dt)
        {

        }
        public void FriendSearch(string user_nick = null, FriendCallback callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            if (string.IsNullOrEmpty(user_nick))
            {
                ToastManager.On(100002528);
                return;
            }

            WWWForm param = new WWWForm();
            param.AddField("req_nick", user_nick);//요청 닉네임
            NetworkManager.Send("friend/find", param, (res) =>
            {
                if (res["rs"].Value<int>() == 0)
                {
                    JToken jToken = res["user_info"];
                    Debug.Log(jToken);
                    List<FriendUserData> datas = new List<FriendUserData>();

                    foreach (var item in jToken)
                    {
                        datas.Add(GetData(item));
                    }
                    callback?.Invoke(datas);
                }
                else
                {
                    failCallBack?.Invoke("");
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            }, failCallBack);

        }
        public FriendUserData GetData(JToken friend)
        {
            return new FriendUserData(friend);
        }
        public FriendUserData AddList(JToken friend)
        {
            var uid = SBFunc.IsJTokenCheck(friend["user_no"]) ? friend["user_no"].Value<long>() : 0;
            if (false == FriendIdList.TryGetValue(uid, out var data))
                FriendIdList.Add(uid, GetData(friend));
            else
                data.SetData(friend);

            return data;
        }
        public void FriendList(FriendCallback callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            NetworkManager.Send("friend/list", null, (res) =>
            {
                List<FriendUserData> datas = new List<FriendUserData>();

                if (res["rs"].Value<int>() == 0)
                {
                    JToken jToken = res["list"];

                    foreach (var item in jToken)
                    {
                        datas.Add(AddList(item));
                    }
                    if (false == IsLoaded)
                        IsLoaded = true;
                    callback?.Invoke(datas);
                }
                else
                {
                    failCallBack?.Invoke("");
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            }, failCallBack);
        }
        public void FriendReceiveList(FriendCallback callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            NetworkManager.Send("friend/requestlist", null, (res) =>
            {
                List<FriendUserData> datas = new List<FriendUserData>();

                if (res["rs"].Value<int>() == 0)
                {
                    JToken jToken = res["list"];
                    Debug.Log(jToken);

                    foreach (var item in jToken)
                    {
                        datas.Add(GetData(item));
                    }
                    callback?.Invoke(datas);
                }
                else
                {
                    failCallBack?.Invoke("");
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            }, failCallBack);
        }

        public void FriendRecommendList(FriendCallback callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            NetworkManager.Send("friend/recommendlist", null, (res) =>
            {
                List<FriendUserData> datas = new List<FriendUserData>();

                if (res["rs"].Value<int>() == 0)
                {
                    JToken jToken = res["list"];
                    Debug.Log(jToken);

                    foreach (var item in jToken)
                    {
                        datas.Add(GetData(item));
                    }
                    callback?.Invoke(datas);
                }
                else
                {
                    failCallBack?.Invoke("");
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            }, failCallBack);
        }

        public void FriendReFreshRecommendList(FriendCallback callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            NetworkManager.Send("friend/refreshrecommendlist", null, (res) =>
            {
                var rs = (eApiResCode)res["rs"].Value<int>();
                switch (rs)
                {
                    case eApiResCode.OK:
                    {
                        List<FriendUserData> datas = new List<FriendUserData>();

                        JToken jToken = res["list"];
                        foreach (var item in jToken)
                        {
                            datas.Add(GetData(item));
                        }
                        callback?.Invoke(datas);
                        ToastManager.On(100002526);
                    } 
                    break;
                    case eApiResCode.RECOMMEND_LIST_EMPTY:
                    {
                        ToastManager.On(StringData.GetStringByStrKey("errorcode_1417"));
                    } break;
                    case eApiResCode.CANNOT_REFRESH_RECOMMEND_LIST:
                    {
                        ToastManager.On(StringData.GetStringByStrKey("errorcode_1418"));
                    } break;
                    default:
                        failCallBack?.Invoke("");
                        break;
                }
            }, failCallBack);
        }

        public void SendFriendInvite(FriendUserData info, FriendCallback callback = null, NetworkManager.FailCallback failCallback = null)
        {
            WWWForm param = new WWWForm();
            param.AddField("req_uno", info.UID.ToString());

            NetworkManager.Send("friend/requestsend", param, (res) =>
            {
                if (res["rs"].Value<int>() == 0)
                {
                    FriendEvent.SendRegist(info.UID);
                    callback?.Invoke(null);
                    ToastManager.On(100002527);
                }
                else
                {
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                    failCallback?.Invoke("");
                }
            }, failCallback);
        }
        public void SendFriendInvite(long user_no, FriendCallback callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            WWWForm param = new WWWForm();
            param.AddField("req_uno", user_no.ToString());

            NetworkManager.Send("friend/requestsend", param, (res) =>
            {
                if (res["rs"].Value<int>() == 0)
                {
                    FriendEvent.SendRegist(user_no);
                    callback?.Invoke(null);
                    ToastManager.On(100002527);
                }
                else
                {
                    failCallBack?.Invoke("");
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            }, failCallBack);
        }

        public void DeleteFriend(long user_no, FriendRemoveCB callback = null, NetworkManager.FailCallback failCallBack = null)
        {
            WWWForm param = new WWWForm();
            param.AddField("f_uno", user_no.ToString());
            ///따로 푸쉬 안옴
            NetworkManager.Send("friend/remove", param, (res) =>
            {
                if (res["rs"].Value<int>() == 0)
                {
                    ToastManager.On(StringData.GetStringByStrKey("친구삭제"));
                    FriendIdList.Remove(user_no);

                    FriendEvent.SendRemove(user_no);
                }
                else
                {
                    failCallBack?.Invoke("");
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            }, failCallBack);
        }
        /// <summary> 친구요청 수락  </summary>
        public void AcceptInvite(long UID, NetworkManager.SuccessCallback success = null, NetworkManager.FailCallback fail = null)
        {
            WWWForm param = new WWWForm();
            param.AddField("req_uno", UID.ToString());
            ///푸쉬로 날아옴
            NetworkManager.Send("friend/requestaccept", param, success, fail);
        }
        public void RejectInvite(long UID, NetworkManager.SuccessCallback success = null, NetworkManager.FailCallback fail = null)
        {
            WWWForm param = new WWWForm();
            param.AddField("req_uno", UID.ToString());

            NetworkManager.Send("friend/requestreject", param, success, fail);
        }
        /// <summary> 하트 전체 받기 & 하트 전체 보내기 </summary>
        /// <param name="op_code">보내기 1, 받기 2</param>
        //public static void SendFriendlyPoint(int op_code, NetworkManager.SuccessCallback callback = null, long userUID = -1, VoidDelegate failCallBack = null)//하트 전체 받기 & 하트 전체 보내기
        public static void SendFriendlyPoint(int op_code, NetworkManager.SuccessCallback callback = null, NetworkManager.FailCallback failCallBack = null)//하트 전체 받기 & 하트 전체 보내기
        {
            WWWForm param = new WWWForm();
            /// 서버 코드 확인하니 전체 보내기만 남아있음
            //if (userUID > 0)
            //    param.AddField("f_uno", userUID.ToString());
            param.AddField("op_code", op_code);

            NetworkManager.Send("friend/friendlypoint", param, callback, failCallBack);
        }

        public void AddFriendList(long user_no, FriendUserData info)
        {
            if (FriendIdList.ContainsKey(user_no))
            {
                return;
            }
            FriendIdList[user_no] = info;
        }

        public List<FriendUserData> GetFriendDataToList()
        {
            if (FriendIdList == null || FriendIdList.Count <= 0)
                return new List<FriendUserData>() { };
            else
                return FriendIdList.Values.ToList();
        }

        public FriendUserData GetFriendInfoData(long _userNo)
        {
            if (FriendIdList.ContainsKey(_userNo))
                return FriendIdList[_userNo];
            else
                return null;
        }
        public void OnPractice(long targetUID, Action failCallback = null)
        {
            if (FriendIdList.TryGetValue(targetUID, out var data))
            {
                WWWForm param = new WWWForm();
                var friendUserNo = targetUID;
                param.AddField("req_uno", friendUserNo.ToString());
                NetworkManager.Send("friend/deck", param, ((res) =>
                {
                    if (res["rs"].Value<int>() == 0 && res.ContainsKey("user_deck") && SBFunc.IsJTokenType(res["user_deck"], JTokenType.Object))
                    {
                        ArenaTeamData friendArenaData = new ArenaTeamData();
                        friendArenaData.InitData();

                        var userDeckData = (JObject)res["user_deck"];
                        List<DragonInfo> tempDefList = new List<DragonInfo>();
                        if (userDeckData.ContainsKey("defDeck"))
                        {
                            foreach (var dragonDataSet in userDeckData["defDeck"])
                            {
                                if (dragonDataSet == null || dragonDataSet.Type != JTokenType.Array)
                                    continue;

                                JArray dragonInfo = (JArray)dragonDataSet;
                                DragonInfo tempDataSet = new DragonInfo((int)dragonInfo[0], (int)dragonInfo[1], dragonInfo.Count > 2 ? (int)dragonInfo[2] : 0);
                                friendArenaData.DefDeck.Add(tempDataSet);
                            }
                        }

                        if (userDeckData.ContainsKey("defBp"))
                            friendArenaData.DefBattlePoint = userDeckData["defBp"].Value<int>();

                        if (userDeckData.ContainsKey("icon"))
                            friendArenaData.PortraitIcon = userDeckData["icon"].Value<string>();

                        if (userDeckData.ContainsKey("rank_grade"))
                            friendArenaData.RankGrade = (eArenaRankGrade)userDeckData["rank_grade"].Value<int>();

                        friendArenaData.UID = data.UID;
                        friendArenaData.Level = data.Level;
                        friendArenaData.Nick = data.Nick;
                        friendArenaData.EtcInfo = data.EtcInfo;

                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("친선전팝업알림2"),
                        () =>
                        {
                            if (friendArenaData != null)
                            {
                                ArenaManager.Instance.SetFriendTeamDataSet(friendArenaData);
                                LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);
                            }
                        },
                        () => { },
                        () => { },
                        true, true, true);
                    }
                    else//등록한 덱 정보가 없을 때
                    {
                        ShowSetDeckEmptyPopup();
                    }
                }), (s) => failCallback?.Invoke());
                return;
            }
            failCallback?.Invoke();
        }
        void ShowSetDeckEmptyPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("친선전팝업알림1"),
                () => { },
                null,
                () => { }
            , true, false, true);
        }
    }
}
