using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary> 길드 드래곤 행동 관리자 </summary>
    public class GuildDragonMachine : EventListener<GuildEvent>, EventListener<ReddotEvent>
    {
        private Transform Parent { get; set; } = null;
        private Transform CanvasParent { get; set; } = null;
        private SBTypePool<SkeletonDataAsset, GuildDragonSpine> TypePool { get; set; } = null;
        private List<GuildDragonSpine> ActiveDragons { get; set; } = null;
        private SBTypePool<string, NameText> NamePool { get; set; } = null;
        private GameObject NameText { get; set; } = null;
        private List<NameText> ActiveNames { get; set; } = null;

        public void Initialize(Transform parent, Transform canvasParent)
        {
            if (TypePool == null)
            {
                TypePool = new(
                    (item) =>
                    {
                        item.gameObject.SetActive(true);
                    },
                    (item) =>
                    {
                        item.gameObject.SetActive(false);
                    });
            }
            if (NamePool == null)
            {
                NamePool = new(
                    (item) =>
                    {
                        item.gameObject.SetActive(true);
                    },
                    (item) =>
                    {
                        item.gameObject.SetActive(false);
                    });
            }
            Parent = parent;
            CanvasParent = canvasParent;
            TypePool.InitializeTransform(Parent);
            NamePool.InitializeTransform(CanvasParent);
            ActiveDragons = new();
            ActiveNames = new();

            EventManager.AddListener<GuildEvent>(this);
            EventManager.AddListener<ReddotEvent>(this);

            if (NamePool != null)
            {
                if (false == NamePool.IsInitialize(nameof(NameText)))
                {
                    if (NameText == null)
                        NameText = ResourceManager.GetResource<GameObject>(eResourcePath.StaticPrefabUIPath, "NameText");

                    var obj = Object.Instantiate(NameText, CanvasParent);
                    var text = obj.GetComponent<NameText>();
                    if (text == null)
                    {
                        Object.Destroy(obj);
                        return;
                    }

                    NamePool.InitializeType(nameof(NameText), text);
                }
            }
        }
        public void Clears()
        {
            for (int i = 0, count = ActiveDragons.Count; i < count; ++i)
            {
                ActiveDragons[i].SetActive(false);
                TypePool.Put(ActiveDragons[i].DataAsset, ActiveDragons[i]);
            }
            ActiveDragons.Clear();
            for (int i = 0, count = ActiveNames.Count; i < count; ++i)
            {
                ActiveNames[i].gameObject.SetActive(false);
                NamePool.Put(nameof(NameText), ActiveNames[i]);
            }
            ActiveNames.Clear();
        }
        public void RefreshDragons()
        {
            Clears();
            if (GuildManager.Instance.LastGuildLeaveState != eGuildLeaveState.None)
                return;

            bool happy = QuestUIObject.IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.DAILY) ||
                QuestUIObject.IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.WEEKLY) ||
                QuestUIObject.IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.CHAIN);

            var users = new List<GuildUserData>(GuildManager.Instance.MyGuildInfo.GuildUserList);
            users.Sort(SortGuildMember);
            
            var multi = 1f;
            var toyData = JsonConvert.DeserializeObject<JObject>(PlayerPrefs.GetString("Setting_Toy"));
            if (toyData != null)
                multi = toyData["value"].Value<float>();

            int dragonCount = 0;
            var maxCount = Mathf.FloorToInt(GameConfigTable.GetConfigIntValue("GUILD_MAXIMUM_MEMBER", 20) * multi) - 1;//내꺼 위해서 하나뺌
            var it = users.GetEnumerator();
            while (dragonCount < maxCount && it.MoveNext())
            {
                var user = it.Current;

                if (user.PortraitIcon == string.Empty || user.PortraitIcon == "0")
                    continue;

                if (user.UID.ToString() == User.Instance.UserData.UserNick)
                    continue;

                if (false == int.TryParse(user.PortraitIcon, out int dragonNo))
                    continue;

                var baseData = CharBaseData.Get(dragonNo);
                if (baseData == null)
                    continue;

                var asset = baseData.GetSkeletonDataAsset();
                if (false == TypePool.IsInitialize(asset))
                {
                    var obj = baseData.GetDefaultSpine();
                    if (obj == null)
                        continue;

                    var spineObject = Object.Instantiate(obj, Parent);
                    if (false == spineObject.TryGetComponent<GuildDragonSpine>(out var spine))
                        spine = spineObject.AddComponent<GuildDragonSpine>();

                    TypePool.InitializeType(asset, spine);
                }

                var typeObject = TypePool.Get(asset);
                typeObject.SetData(baseData, user, asset);
                typeObject.SetActive(true);
                typeObject.transform.localScale = Vector3.one + (user.Rank <= 0 ? Vector3.zero : ((Vector3.one * 0.5f) * ((float)(users.Count - user.Rank) / users.Count)));
                ActiveDragons.Add(typeObject);
                typeObject.SetShadow(true);

                var effectTr = typeObject.GetComponent<statusEffect>();
                if (effectTr != null)
                    SetName(user, effectTr.EffectTr);

                
                dragonCount++;
            }

            //내꺼
            if (int.TryParse(User.Instance.UserData.UserPortrait, out int no))
            {
                var user = GuildManager.Instance.MyData;
                if (user != null)
                {
                    var baseData = CharBaseData.Get(no);
                    if (baseData != null)
                    {
                        var asset = baseData.GetSkeletonDataAsset();
                        if (false == TypePool.IsInitialize(asset))
                        {
                            var obj = baseData.GetDefaultSpine();
                            if (obj != null)
                            {
                                var spineObject = Object.Instantiate(obj, Parent);
                                if (false == spineObject.TryGetComponent<GuildDragonSpine>(out var spine))
                                    spine = spineObject.AddComponent<GuildDragonSpine>();

                                TypePool.InitializeType(asset, spine);
                            }

                            var typeObject = TypePool.Get(asset);
                            typeObject.SetData(baseData, user, asset);
                            typeObject.SetActive(true);
                            typeObject.SetShadow(true);
                            typeObject.transform.localScale = Vector3.one + (user.Rank <= 0 ? Vector3.zero : ((Vector3.one * 0.5f) * ((float)(users.Count - user.Rank) / users.Count)));
                            ActiveDragons.Add(typeObject);
                            var effectTr = typeObject.GetComponent<statusEffect>();
                            if (effectTr != null)
                                SetName(user, effectTr.EffectTr);
                            dragonCount++;
                        }
                    }

                    if (Town.TownDragonsDic.ContainsKey(no))
                    {
                        Town.TownDragonsDic[no].Data.SetDragonState(eDragonState.Guild);
                        Town.TownDragonsDic[no].OnEvent(new DragonHideEvent());
                    }
                }
            }
        }
        private int SortGuildMember(GuildUserData data1, GuildUserData data2)
        {
            bool isRank1 = data1.Rank != 0;
            bool isRank2 = data2.Rank != 0;
            if (isRank1 && isRank2)
            {
                if (data1.Rank < data2.Rank)
                    return -1;
                else if (data1.Rank > data2.Rank)
                    return 1;
            }
            else if (isRank1)
                return -1;
            else if (isRank2)
                return 1;

            return 0;
        }
        public void Destory()
        {
            TypePool.Destroy();
            EventManager.RemoveListener<GuildEvent>(this);
            EventManager.RemoveListener<ReddotEvent>(this);
        }

        public void OnEvent(GuildEvent eventType)
        {
            switch (eventType.Event)
            {
                case GuildEvent.eGuildEventType.LostGuild:
                {
                    Clears();
                }
                break;
                case GuildEvent.eGuildEventType.GuildRefresh:
                {
                    RefreshDragons();
                } break;
                default:
                    break;
            }
        }
        public void Update(float dt)
        {
            for (int i = 0, count = ActiveDragons.Count; i < count; ++i)
            {
                if (ActiveDragons[i] == null)
                    continue;

                ActiveDragons[i].MachineUpdate(dt);
            }
        }

        public void OnEvent(ReddotEvent e)
        {
            if (e.type == eReddotEvent.GUILD_MISSION
                || e.type == eReddotEvent.GUILD_MISSION_DAILY
                || e.type == eReddotEvent.GUILD_MISSION_WEEKLY
                || e.type == eReddotEvent.GUILD_MISSION_CHAIN)
            {
                Debug.Log("ReddotEvent : " + e.type);

                bool happy = QuestUIObject.IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.DAILY) ||
                QuestUIObject.IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.WEEKLY) ||
                QuestUIObject.IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.CHAIN);

                foreach (var dr in ActiveDragons)
                {
                    dr.SetHappy(happy);
                }
            }
        }
        protected virtual void SetName(GuildUserData data, Transform target)
        {
            if (CanvasParent == null || target == null)
                return;

            var typeObject = NamePool.Get(nameof(NameText));
            typeObject.gameObject.SetActive(true);
            typeObject.SetData(data, target);
            SBFunc.SetLayer(typeObject.transform, "town_dragon");
            ActiveNames.Add(typeObject);
        }
    }
}