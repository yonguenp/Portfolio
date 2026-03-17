using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class BattleStage : MonoBehaviour, IBattleStage
    {
        public virtual IBattleMap Map { get; protected set; } = null;
        public List<List<BattleSpine>> OffenseSpines { get; protected set; } = new();
        public List<List<BattleSpine>> DefenseSpines { get; protected set; } = new();
        public abstract List<List<BattleSpine>> PrevSpines { get; protected set; }

        [SerializeField]
        protected Canvas hpBarCanvas = null;
        [SerializeField]
        protected List<GameObject> maps = null;
        [SerializeField]
        protected GameObject hpBarGreenPrefab = null;
        [SerializeField]
        protected GameObject hpBarRedPrefab = null;
        [SerializeField]
        protected List<SkillDimmed> dimmed = null;
        [SerializeField]
        protected Transform[] cameraPosXFollows;

        public float BattleStartY { get { return 0.6f; } }
        public float BattleMinY { get { return 0f; } }
        public float BattleMaxY { get { return 1.2f; } }
        public float DragonStartX { get { return 1f; } }
        public float DragonSpancingX { get { return 0.7f; } }
        public float MonsterStartX { get { return 6f; } }
        public float MonsterSpancingX { get { return 0.6f; } }
        protected virtual SimpleStateMachine<BattleState> StateMachine { get; set; } = null;
        private Dictionary<IBattleCharacterData, UIHpBar> HpBars = new Dictionary<IBattleCharacterData, UIHpBar>();
        protected virtual void Start()
        {
            Initialize();
        }
        protected virtual void OnDestroy()
        {
            Destroy();
        }
        protected virtual void Initialize()
        {
            if (OffenseSpines == null)
                OffenseSpines = new List<List<BattleSpine>>();
            else
                OffenseSpines.Clear();
            if (DefenseSpines == null)
                DefenseSpines = new List<List<BattleSpine>>();
            else
                DefenseSpines.Clear();
            if (PrevSpines == null)
                PrevSpines = new List<List<BattleSpine>>();
            else
                PrevSpines.Clear();

            if (dimmed != null)
            {
                for (int i = 0, count = dimmed.Count; i < count; ++i)
                {
                    if (dimmed[i] == null || dimmed[i].DimmedObject == null)
                        return;

                    dimmed[i].DimmedObject.SetActive(false);
                }
                BattleDimmed.SetDimmed(dimmed);
            }

            StartCoroutine(nameof(UpdateCO));

            SBGameManager.Instance.SetFixedDeltaTime(false);
        }

        protected virtual void Destroy()
        {
            ClearBuff();
            StateMachine?.Destroy();
            StateMachine = null;
            Map = null;
            if (OffenseSpines != null)
                OffenseSpines.Clear();
            OffenseSpines = null;
            if (DefenseSpines != null)
                DefenseSpines.Clear();
            DefenseSpines = null;
            if (PrevSpines != null)
                PrevSpines.Clear();
            PrevSpines = null;
            BattleDimmed.SetDimmed(null);
        }

        public abstract BattleSpine GetOffenseSpine(IBattleCharacterData data);
        public abstract BattleSpine GetDefenseSpine(IBattleCharacterData data);
        public abstract void SetGreenHpBar(IBattleCharacterData data);
        public abstract void SetRedHpBar(IBattleCharacterData data);
        protected virtual void SetHpBar(Canvas canvas, GameObject prefab, IBattleCharacterData data)
        {
            if (canvas == null || prefab == null || data == null)
                return;

            var hpBar = Instantiate(prefab, canvas.transform);
            var uiHpBar = hpBar.GetComponent<UIHpBar>();
            if (uiHpBar == null)
            {
                Destroy(hpBar);
            }

            hpBar.transform.localScale = Vector3.one * 0.33f;
            uiHpBar.SetData(this, data);

            HpBars.Add(data, uiHpBar);
        }

        public virtual void RemoveHpBar(IBattleCharacterData data)
        {
            if (HpBars.ContainsKey(data))
            {
                var taget = HpBars[data];
                HpBars.Remove(data);

                if (taget != null)
                    taget.DestroyObject();
            }
        }
        

        protected abstract IEnumerator UpdateCO();
        protected abstract void BattleEnd();
        protected abstract void OnPausePopup();
        
        protected virtual void OnPause()
        {
            OnPausePopup();
        }
        protected virtual void ClearBuff()
        {
            if (OffenseSpines != null)
            {
                for (int x = 0, xCount = OffenseSpines.Count; x < xCount; ++x)
                {
                    var xSpines = OffenseSpines[x];
                    if (xSpines == null)
                        continue;

                    for (int y = 0, yCount = xSpines.Count; y < yCount; ++y)
                    {
                        if (xSpines[y] == null)
                            continue;

                        xSpines[y].ClearBuffStat();
                    }
                }
            }
            if (DefenseSpines != null)
            {
                for (int x = 0, xCount = DefenseSpines.Count; x < xCount; ++x)
                {
                    var xSpines = DefenseSpines[x];
                    if (xSpines == null)
                        continue;

                    for (int y = 0, yCount = xSpines.Count; y < yCount; ++y)
                    {
                        if (xSpines[y] == null)
                            continue;

                        xSpines[y].ClearBuffStat();
                    }
                }
            }
        }
        protected virtual void LateUpdate()
        {
            if (cameraPosXFollows != null)
            {
                Vector3 cameraPosition = Camera.main.transform.position;
                foreach (Transform follow in cameraPosXFollows)
                {
                    if (follow == null)
                        continue;

                    follow.position = new Vector3(cameraPosition.x, follow.position.y, follow.position.z);
                }
            }
        }

        float blurTime = 0;
        private void OnApplicationPause(bool isFocus)
        {
#if !UNITY_EDITOR
            if (!isFocus)
            {
                blurTime = Time.time;
            }
            else
            {
                if (blurTime != 0)
                {
                    if (Time.time - blurTime > 1.0f)
                    {
                        OnPause();
                    }
                }

                blurTime = 0;
            }
#endif
        }

        public virtual Vector3 GetDragonStartPosition(int x, int y, int maxY)
        {
            return new Vector3(-DragonStartX - (DragonSpancingX * x), GetBattlePosY(y, maxY), 0f);
        }
        public virtual Vector3 GetDragonEndPosition(int x, int y, int maxY)
        {
            return new Vector3(-(DragonSpancingX * x), GetBattlePosY(y, maxY), 0f);
        }

        public virtual Vector3 GetOffenseStartFieldPosition(float offset)
        {
            return new Vector3(DragonStartX + offset, 0f, 0f);
        }
        public virtual Vector3 GetOffenseEndFieldPosition(float offset)
        {
            return new Vector3(-DragonStartX + offset, 0f, 0f);
        }
        public virtual Vector3 GetMonsterStartPosition(int x, int y, int maxY, float xOffset, float extraOffset = 0)
        {
            return new Vector3(MonsterSpancingX * x, GetBattlePosY(y, maxY), 0f);
        }
        public Vector3 GetMonsterEndPosition(int x, int y, int maxY)
        {
            return new Vector3(MonsterSpancingX * x, GetBattlePosY(y, maxY), 0f);
        }
        public Vector3 GetBattleMonsterPosition(int x, int y, int maxY)
        {
            return new Vector3(MonsterSpancingX * x, GetBattlePosY(y, maxY), 0f);
        }
        public Vector3 GetBattleDragonPosition(int x, int y, int maxY)
        {
            return new Vector3(-(DragonSpancingX * x), GetBattlePosY(y, maxY), 0f);
        }

        public virtual Vector3 GetMonsterStartFieldPosition(float xOffset, float extraOffset = 0)
        {
            return new Vector3(MonsterStartX + xOffset + extraOffset, 0f, 0f);
        }
        
        public virtual Vector3 GetMonsterEndFieldPosition(float offset)
        {
            return new Vector3(DragonStartX + offset, 0f, 0f);
        }

        public float GetDragonPositionY(int x, int y, int maxY)
        {
            float yPos;
            switch (maxY)
            {
                case 1:
                {
                    yPos = BattleStartY;
                }
                break;
                case 2:
                {
                    yPos = BattleStartY * (y + 1) - BattleStartY * 0.5f - BattleStartY;
                    yPos = yPos + (yPos * 0.5f * x) + BattleStartY;
                }
                break;
                case 3:
                default:
                {
                    yPos = BattleMaxY / (maxY - 1) * y;
                }
                break;
            }

            if (yPos > BattleMaxY)
                yPos = BattleMaxY;
            else if (yPos < BattleMinY)
                yPos = BattleMinY;

            return yPos;
        }

        public float GetBattlePosY(int y, int maxY)
        {
            //1 = 0.6f
            //2 = max / maxY * (y + 1) - max / maxY / maxY;
            //3이상 = max / (maxY - 1) * y
            float yPos;
            switch (maxY)
            {
                case 1:
                {
                    yPos = BattleStartY;
                }
                break;
                case 2:
                {
                    yPos = BattleStartY * (y + 1) - BattleStartY * 0.5f;
                }
                break;
                case 3:
                default:
                {
                    yPos = BattleMaxY / (maxY - 1) * y;
                }
                break;
            }

            if (yPos > BattleMaxY)
                yPos = BattleMaxY;
            else if (yPos < BattleMinY)
                yPos = BattleMinY;

            return yPos;
        }

        public virtual BattleSpine MakeSummonMonsterData(IBattleCharacterData casterData, int id)
        {
            if (StateMachine.BattleState != null)
            {
                var enemyData = MakeSummonMonsterData(id);
                var spine = GetDefenseSpine(enemyData);
                if (spine != null)
                {
                    DefenseSpines[0].Add(spine);
                    BattleStateLogic battleState = StateMachine.BattleState as BattleStateLogic;
                    if (battleState != null)
                    {
                        battleState.defenses.Add(spine);

                        SetRedHpBar(enemyData);
                        return spine;
                    }
                }
            }

            return null;
        }

        public virtual BattleMonsterData MakeSummonMonsterData(int id)
        {
            var enemyData = new BattleMonsterData(eStageType.ADVENTURE);
            enemyData.SetData(0, id);
            return enemyData;
        }
    }
}