using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{

    

    /// <summary> 길드 드래곤 Spine </summary>
    public class GuildDragonSpine : SBSpine<eSpineAnimation>, EventListener<PortraitChangeEvent>
    {
        /// <summary> 해당 드래곤의 길드유저 정보 </summary>
        public GuildUserData UserData { get; private set; } = null;
        public int DragonNo { get; set; } = -1;
        public CharBaseData BaseData { get; private set; } = null;
        /// <summary> 드래곤 SpineData </summary>
        public SkeletonDataAsset DataAsset { get; private set; } = null;
        public SBController Controller { get; private set; } = null;
        private GuildDragonStateMachine Machine { get; set; } = null;

        public override void InitializeTypeFunc()
        {
            GetTypeToName = GetAnimName;
            GetTypeToLoop = SBFunc.IsTypeToLoop;
            GetNameToType = SBDefine.GetDragonAnimNameToType;
            GetTypeToSkip = SBFunc.IsAnimSkip;
        }
        protected string GetAnimName(eSpineAnimation anim)
        {
            return anim switch
            {
                eSpineAnimation.ATTACK => SBDefine.GetDragonAnimTypeToName(anim, 1),
                eSpineAnimation.SKILL => SBDefine.GetDragonAnimTypeToName(anim, BaseData.SKILL1.ANI),//skill_type추가 필요
                _ => SBDefine.GetDragonAnimTypeToName(anim, 1),
            };
        }
        public override void Init()
        {
            base.Init();

            SetData(BaseData);
            SetSkin(BaseData.SKIN);
            SetAnimation(eSpineAnimation.IDLE);
            if (skeletonAni.loop == false)
                skeletonAni.loop = true;

            Controller = gameObject.GetComponent<SBController>();
            if (Controller == null)
                Controller = gameObject.AddComponent<SBController>();

            var body = GetComponent<Rigidbody2D>();
            if (body != null)
                body.gravityScale = 0f;

            var Collider = GetComponent<Collider2D>();
            if (Collider != null)
                Collider.enabled = false;

            if (Controller.myCollider != null && Controller.myCollider.attachedRigidbody != null)
            {
                Controller.myCollider.attachedRigidbody.simulated = false;
            }

            SetHappy(true);

            RandomBatch();

            SBFunc.SetLayer(gameObject, "town_dragon");
        }

        void MachineInit()
        {
            if (Machine == null)
            {
                Machine = new();
                Machine.Initialize(this);
                Machine.StateInit();
            }
        }
        public override void InitializeComponent()
        {
            base.InitializeComponent();
        }
        public void SetData(CharBaseData baseData, GuildUserData userData, SkeletonDataAsset dataAsset)
        {
            if (UserData != null && UserData.UID == User.Instance.UserAccountData.UserNumber)
            {
                EventManager.RemoveListener(this);
            }

            DragonNo = baseData.KEY;
            BaseData = baseData;
            UserData = userData;
            if (UserData.UID == User.Instance.UserAccountData.UserNumber)
            {
                EventManager.AddListener(this);
            }

            if (Machine != null)
            {                
                Machine.StateInit();
            }

            DataAsset = dataAsset;
        }

        private void OnDestroy()
        {
            if (UserData.UID == User.Instance.UserAccountData.UserNumber)
            {
                EventManager.RemoveListener(this);
            }
        }
        public void RandomBatch()
        {
            transform.localPosition = new Vector3(SBFunc.Random(-SBDefine.GuildBuildingSizeFormX, SBDefine.GuildBuildingSizeFormX), -0.24f, 0);
        }

        public void MachineUpdate(float dt)
        {
            if(IsActive())
                Machine?.Update(dt);
        }
        public void SetSpeed(int speed)
        {
            if (speed <= 0)
                return;

            if (Controller != null)
                Controller.Speed = speed;

            SetSpeed((float)speed / SBDefine.TownDefaultSpeed);
        }

        public void SetSpeed(float scale)
        {
            if (skeletonAni == null)
                return;

            skeletonAni.timeScale = scale;
        }

        void EventListener<PortraitChangeEvent>.OnEvent(PortraitChangeEvent eventType)
        {
            if(eventType.portraitNum == 0)
            {
                SetActive(false);
            }
            else
            {
                SetActive(true);
                if(DragonNo != eventType.portraitNum) 
                { 
                    var baseData = CharBaseData.Get(eventType.portraitNum);
                    var asset = baseData.GetSkeletonDataAsset();
                    SetData(baseData,GuildManager.Instance.MyData,asset);
                    Init();
                    Machine.ChangeState<GuildDragonIdle>();
                }
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }

        public void SetHappy(bool happy)
        {
            MachineInit();
            Machine.SetHappy(happy);
        }
    }
}