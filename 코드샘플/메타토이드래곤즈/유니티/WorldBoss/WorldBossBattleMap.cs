using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// 기존에 박혀있던 
    /// </summary>
    public class WorldBossBattleMap : BattleMap
    {
        [SerializeField]
        List<WorldBossDragonPos> DragonsPosComponent = new List<WorldBossDragonPos>();

        [SerializeField]
        Transform bossTransform = null;

        [SerializeField]
        List<WorldBossSpace> Space = new List<WorldBossSpace>();

        
        public WorldBossDragonPos[] GetDragonPos()
        {
            return DragonsPosComponent.ToArray();
        }

        public Vector3 GetWorldBossPosition()
        {
            return bossTransform.position;
        }

        public WorldBossSpace[] GetSpace()
        {
            return Space.ToArray();
        }
        /// <summary>
        /// backGround 이미지 영역은 카메라를 통한 조절로 인해 세팅 안함.
        /// </summary>
        /// <param name="cameraPosition"></param>
        protected override void SetBackGround(Vector3 cameraPosition)
        {
            base.SetBackGroundObject(cameraPosition);
        }
    }
}

