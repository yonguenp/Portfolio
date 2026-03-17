using Com.LuisPedroFonseca.ProCamera2D;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public class BattleWorldBossSpine : BattleMonsterSpine
    {
        enum ScarecrowParts
        {
            LeftTop = 0,
            RightTop,
            LeftBot,
            RightBot,

            MAX
        }
        BattleWorldBossPartsSpine[] Scarecrow = new BattleWorldBossPartsSpine[(int)ScarecrowParts.MAX];

        [Serializable]
        public class WorldBossSerializeParts
        {
            [SerializeField]
            public int key = -1;
            [SerializeField]
            public GameObject part = null;
            [SerializeField]
            public bool preview = false;
        }
        [SerializeField]
        List<WorldBossSerializeParts> SerializeParts = new List<WorldBossSerializeParts>();
        Dictionary<int, BattleWorldBossPartsSpine> Parts = new Dictionary<int, BattleWorldBossPartsSpine>();
        
        public List<BattleWorldBossPartsData> ActiveParts { get; private set; } = new List<BattleWorldBossPartsData>();
        public BattleWorldBossData BossData { get; private set; } = null;
        [SerializeField]
        SkeletonAnimation backSpine = null;
        [SerializeField]
        ParticleSystem DustEffect = null;
        [SerializeField]
        ParticleSystem LevelUpEffect = null;
        List<BattleWorldBossPartsSpine> ReadyAnimParts = new List<BattleWorldBossPartsSpine>();
        public override void InitializeComponent()
        {
            InitParts();
            base.InitializeComponent();            
        }
        public override void Init()
        {
            base.Init();
            SetAnimation(0, "ready", true);
            backSpine.AnimationState.SetAnimation(0, "ready", false);


            BossData = (BattleWorldBossData)Data;
            BossData.SetSpine(this);
        }

        public void OnBossShowAnimation()
        {
            OnLevelUp(1);

            skeletonAni.ClearState();

            var track = SetAnimation(0, "start", false);
            
            track = backSpine.AnimationState.SetAnimation(0, "start", false);
            if (track != null)
                track.Complete += (track) => { 
                    backSpine.AnimationState.SetAnimation(0, "idle", true);
                    UIBattleObjectStartEvent.Send();
                };

            foreach (var part in ReadyAnimParts)
            {
                part.OnStartAnimation();
            }

            SkeletonAni.enabled = true;
        }

        void InitParts()
        {
            ActiveParts.Clear();
            Parts.Clear();
            ReadyAnimParts.Clear();

            foreach (var part in SerializeParts)
            {
                if (part.part != null)
                {
                    part.part.SetActive(part.preview);
                    var spine = part.part.GetComponent<BattleWorldBossPartsSpine>();
                    if (spine != null)
                    {
                        Parts.Add(part.key, spine);

                        if (part.preview)
                        {
                            if(spine.SetReadyAnimation())
                                ReadyAnimParts.Add(spine);
                        }
                    }
                }
            }
        }

        public override void SetDamage(int value, IBattleCharacterData caster)
        {
            if (BossData == null)
            {
                Death();
                return;
            }

            if (BossData.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT) > 0)
            {
                for (int i = 0, count = BossData.Infos.Count; i < count; ++i)
                {
                    if (Data.Infos[i] == null)
                        continue;

                    value = BossData.Infos[i].SetDamage(value);
                    if (value == 0)
                        break;
                }
            }

            BossData.OnDamage(value, caster);
            //if (Data.HP <= 0)
            //{
            //    Data.HP = 0;
            //    Death();
            //}
        }

        public override void Hit()
        {
            if(BossData.LastHitPart != null)
                BossData.LastHitPart.Hit();
        }
        
        public BattleWorldBossPartsSpine GetHitObject(WorldBossBattleDragonData caster)
        {
            if (caster != null)
            {
                int partyIndex = caster.PartyIndex;
                if(partyIndex >= 0 && partyIndex < Scarecrow.Length)
                    return Scarecrow[partyIndex];
            }

            return null;
        }

        public void OnLevelUp(int level)
        {
            if(level <= 1)
                DustEffect.Play();
            else
                LevelUpEffect.Play();
            //UIBattleBossStartEvent.Send(Data);

            foreach (var data in WorldBossPartData.GetLevelData(BossData.MonsterKey, level))
            {
                foreach (var group in WorldBossPartData.GetGroup(data.GROUP))
                {
                    if (data.KEY == group.KEY)
                        continue;

                    Parts[group.KEY]?.gameObject.SetActive(false);
                    var spine = Parts[group.KEY].GetComponent<BattleWorldBossPartsSpine>();
                    if (spine != null)
                    {
                        var battleData = (BattleWorldBossPartsData)spine.Data;
                        //ProCamera2D.Instance.RemoveCameraTarget(spine.transform);

                        var summonData = WorldBossStage.Instance.GetSummonMonsterByPart(battleData);
                        if (summonData != null)
                            WorldBossStage.Instance.DestroySummonMonster(summonData);

                        ActiveParts.Remove(battleData);
                    }
                }

                var partsSpine = Parts[data.KEY];
                if (partsSpine != null)
                {
                    if (partsSpine.SetData(BossData, data))
                    {
                        ActiveParts.Add((BattleWorldBossPartsData)partsSpine.Data);
                        Parts[data.KEY]?.gameObject.SetActive(true);

                        if (partsSpine.IsScarecrow())
                        {
                            var indexs = partsSpine.GetAttackPartyIndexs();
                            for(int i = 0, count = indexs.Count; i < count; ++i)
                            {
                                var index = indexs[i];
                                if (index >= 0 && Scarecrow.Length > index)
                                    Scarecrow[index] = partsSpine;
                            }
                        }
                    }

                    //WorldBossStage.Instance.AddCameraTarget(partsSpine.transform);
                }                
            }

            foreach(var part in ActiveParts)
            {
                part.PartsSpine?.OnLevelUp();
            }
        }

        public override void UpdateStatus(float dt)
        {
            base.UpdateStatus(dt);
             

            for (int i = ActiveParts.Count - 1; i >= 0; i--)
            {
                ActiveParts[i].PartsSpine.UpdateStatus(dt);
            }
        }

        public override TrackEntry SetAnimation(eSpineAnimation anim)
        {
            return null;
        }

        protected override void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            if (!trackEntry.Loop)
            {
                SetAnimation(0, "idle", true);
            }
        }
    }
}