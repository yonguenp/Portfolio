using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

namespace SandboxNetwork
{
    public class BattleMap : MonoBehaviour, IBattleMap, EventListener<BattleMapAmbientEvent>
    {
        static readonly float LimitX = 19.2f;

        public static float LimitLeft { get { return (-LimitX + Camera.main.transform.position.x); } }
        public static float LimitRight { get { return (LimitX + Camera.main.transform.position.x); } }

        public static readonly float SpencingX = 38.4f;
        [SerializeField]
        protected GameObject ColliderGroup = null;
        [SerializeField]
        protected BattleField clouds = null;
        [SerializeField]
        protected List<BattleField> fields = null;
        [SerializeField]
        protected Transform characterTransform = null;
        [SerializeField]
        protected Transform effectTransform = null;
        protected SBController[] field = new SBController[2];
        public Transform Beacon { get { return characterTransform; } }
        public Transform EffectBeacon { get { return effectTransform; } }
        public MonoBehaviour Coroutine => this;
        public SBController OffenseBeacon { get { return field[0]; } }
        public SBController DefenseBeacon { get { return field[1]; } }
        public GameObject Colliders { get { return ColliderGroup; } }
        [SerializeField]
        SpriteRenderer[] backgroundFullscreen;
        [SerializeField]
        Transform backgroundObject;

		protected List<BattleField> topFields = new List<BattleField>();
        protected ProCamera2DZoomToFitTargets proCameraZoom = null;

        protected Dictionary<Transform, Vector3> ScaleInfo = new Dictionary<Transform, Vector3>();

        protected float StageMinY = 0.0f;
        protected float StageMaxY = 0.0f;

        const float farRatio = 0.85f;
        const float nearRatio = 1.15f;

        public void Awake()
        {
            topFields.Clear();
            
            foreach (BattleField field in fields)
            {
                if (field.IsTop())
                {
                    topFields.Add(field);
                }
            }

            var obj0 = new GameObject("0");
            obj0.transform.SetParent(characterTransform, false);
            field[0] = obj0.AddComponent<SBController>();
            field[0].Speed = SBDefine.TownDefaultSpeed;
            var obj1 = new GameObject("1");
            obj1.transform.SetParent(characterTransform, false);
            field[1] = obj1.AddComponent<SBController>();
            field[1].Speed = SBDefine.TownDefaultSpeed;
        }

        protected virtual void OnEnable()
        {
            proCameraZoom = Camera.main.GetComponent<ProCamera2DZoomToFitTargets>();
            EventManager.AddListener(this);

            BoxCollider2D[] col = ColliderGroup.GetComponentsInChildren<BoxCollider2D>();
            if(col.Length >= 2)
            {
                StageMinY = col[0].bounds.max.y;
                StageMaxY = col[1].bounds.min.y;
            }
        }
        private void OnDisable()
        {
            proCameraZoom = null;
            EventManager.RemoveListener(this);
        }

        public void UpdateCloud(float dt)
        {
            if (clouds == null)
                return;

            clouds.UpdateLeft(dt);
        }

        public void UpdateLeft(float dt)
        {
            if (fields == null)
                return;

            for (int i = 0, count = fields.Count; i < count; ++i)
            {
                if (fields[i] == null)
                    continue;

                fields[i].UpdateLeft(dt);
            }
        }

        public void UpdateRight(float dt)
        {
            if (fields == null)
                return;

            for (int i = 0, count = fields.Count; i < count; ++i)
            {
                if (fields[i] == null)
                    continue;

                fields[i].UpdateRight(dt);
            }
        }

        public void OnEvent(BattleMapAmbientEvent eventType)
        {
            return;

            //todo 시간될때 background 환경 연출해주자
        }

        protected virtual void LateUpdate()
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            SetBackGround(cameraPosition);
            UpdateCloud(SBGameManager.Instance.DTime);
            SetFields(cameraPosition);

            if(ColliderGroup != null)
            {
                ColliderGroup.transform.position = new Vector3(cameraPosition.x, ColliderGroup.transform.position.y, ColliderGroup.transform.position.z);
            }

            foreach(Transform child in Beacon)
            {
                if (OffenseBeacon.transform == child || DefenseBeacon.transform == child)
                    continue;

                SetScaleByY(child);
            }

            foreach (Transform child in OffenseBeacon.transform)
            {
                SetScaleByY(child);
            }

            foreach (Transform child in DefenseBeacon.transform)
            {
                SetScaleByY(child);
            }
        }

        protected virtual void SetFields(Vector3 cameraPosition)
        {
            for (int i = 0, count = fields.Count; i < count; ++i)
            {
                if (fields[i] == null)
                    continue;

                fields[i].OffsetCheck();
            }

            foreach (BattleField field in topFields)
            {
                float val = SBGameManager.Instance.DTime * 2.0f;//0.5초만에 투명화
                if (proCameraZoom != null && proCameraZoom.zoomEffectCount > 0)
                    field.SubOpacity(val);
                else
                    field.AddOpacity(val);

                field.SetTopPosition(Camera.main.orthographicSize + cameraPosition.y);
            }
        }

        protected virtual void SetBackGround(Vector3 cameraPosition)
        {
            SetBackGroundImage(cameraPosition);
            SetBackGroundObject(cameraPosition);
        }

        protected virtual void SetBackGroundImage(Vector3 cameraPosition)
        {
            float cameraHeight = Camera.main.orthographicSize * 2;
            Vector2 cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);

            cameraPosition.z = 0.0f;

            foreach (SpriteRenderer spriteRenderer in backgroundFullscreen)
            {
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                Vector2 scale = new Vector2(cameraSize.x / spriteSize.x, cameraSize.y / spriteSize.y);

                spriteRenderer.transform.transform.position = cameraPosition;
                spriteRenderer.transform.transform.localScale = scale;
            }
        }

        protected virtual void SetBackGroundObject(Vector3 cameraPosition)
        {
            if (backgroundObject != null)
            {
                backgroundObject.position = new Vector3(cameraPosition.x, backgroundObject.position.y, 0.0f);
                backgroundObject.rotation = Camera.main.transform.rotation;
                backgroundObject.transform.transform.localScale = Vector3.one * Camera.main.orthographicSize / (ProCamera2D.Instance.StartScreenSizeInWorldCoordinates.y / 2);
            }
        }

        protected virtual void SetScaleByY(Transform child)
        {
            if (!ScaleInfo.ContainsKey(child))
            {
                ScaleInfo[child] = child.localScale;
            }

            float len = StageMaxY - StageMinY;
            float cur = StageMaxY - child.position.y;

            child.transform.localScale = ScaleInfo[child] * (farRatio + ((nearRatio - farRatio) * (cur/len)));
        }

#if false//UNITY_EDITOR
        [ContextMenu("Auto Cloud Position")]
        void AutoMapPosition()
        {
            Transform map = transform.Find("MAP");
            if (map == null)
                return;

            Transform BackGround_Static = map.Find("BackGround_Static");
            Transform Clouds = BackGround_Static.Find("Cloud");
            Transform[] Cloud = { Clouds.Find("Cloud1"), Clouds.Find("Cloud2"), Clouds.Find("Cloud3") };

            float[] yPosArray = { 0f, 0.1f, 0.3f, -0.1f, 0.1f, 0.2f, 0.4f, -0.1f };
            
            int index = 0;
            foreach(Transform target in Cloud)
            {
                foreach (Transform child in target)
                {
                    Vector3 pos = child.localPosition;
                    pos.y = yPosArray[(index++) % yPosArray.Length];
                    child.localPosition = pos;
                }
            }
        }
#endif
#if UNITY_EDITOR
        [ContextMenu("CheckMapPosition")]
        void CheckMapPosition()
        {
            UpdateRight(12.8f);
            LateUpdate();
        }

#endif

    }
}