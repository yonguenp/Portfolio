using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ArenaColosseum : MonoBehaviour, IBattleMap
    {
        [SerializeField]
        private Transform beacon = null;
        [SerializeField]
        protected GameObject ColliderGroup = null;
        public Transform Beacon
        {
            get { return beacon; }
        }
        public Transform EffectBeacon
        {
            get { return beacon; }
        }
        public MonoBehaviour Coroutine => this;
        protected SBController[] field = new SBController[2];
        public SBController OffenseBeacon { get { return field[0]; } }
        public SBController DefenseBeacon { get { return field[1]; } }
        public GameObject Colliders { get { return ColliderGroup; } }

        [SerializeField]
        private GameObject dimmed = null;
        public GameObject Dimmed { get { return dimmed; } }

        private Transform characterPlatform = null;

        [SerializeField]
        private List<Animator> startAnim = null;
        private int curAnim = 0;

        private Dictionary<char, ArenaDragonSpine> dragonSpineDic = null;

        // Start is called before the first frame update
        void Start()
        {
            var fieldBeacon = beacon;
            if (Beacon != null)
            {
                characterPlatform = Beacon.Find("CharacterPlatform");
                if (characterPlatform != null)
                    fieldBeacon = characterPlatform;
            }

            if (beacon == null)
                beacon = transform;

            if (dragonSpineDic == null)
                dragonSpineDic = new Dictionary<char, ArenaDragonSpine>();
            else
                dragonSpineDic.Clear();


            var obj0 = new GameObject("0");
            obj0.transform.SetParent(fieldBeacon, false);
            field[0] = obj0.AddComponent<SBController>();
            field[0].Speed = SBDefine.TownDefaultSpeed;
            var obj1 = new GameObject("1");
            obj1.transform.SetParent(fieldBeacon, false);
            field[1] = obj1.AddComponent<SBController>();
            field[1].Speed = SBDefine.TownDefaultSpeed;
        }
        public void AnimationEnd()
        {
            if (startAnim == null)
                return;

            int count = startAnim.Count;
            if (count <= curAnim || 0 <= count)
                return;

            startAnim[curAnim].speed = 0f;
            curAnim++;
            startAnim[curAnim].Play("anim");
        }

        #region 미사용
        public void UpdateCloud(float dt) { }
        public void UpdateLeft(float dt) { }
        public void UpdateRight(float dt) { }
        #endregion
    }
}