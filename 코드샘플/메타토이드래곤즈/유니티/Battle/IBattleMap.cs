using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public interface IBattleMap
    {
        public Transform Beacon { get; }
        public Transform EffectBeacon { get; }
        public GameObject Colliders { get; }
        public MonoBehaviour Coroutine { get; }
        public SBController OffenseBeacon { get; }
        public SBController DefenseBeacon { get; }
        public void UpdateCloud(float dt);
        public void UpdateLeft(float dt);
        public void UpdateRight(float dt);
    }
}