using Com.LuisPedroFonseca.ProCamera2D;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public class ArenaDragonSpine : BattleDragonSpine
    {
        public ArenaDragonData AData
        {
            get { return Data as ArenaDragonData; }
            protected set { Data = value; }
        }
        public bool IsOffense
        {
            get
            {
                if (AData != null)
                    return !AData.IsEnemy;

                return false;
            }
        }
        public override void Init()
        {
            base.Init();
            if (IsOffense)
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        public override void SetData(IBattleCharacterData data)
        {
            base.SetData(data);
            AData = data as ArenaDragonData;
        }
        public override Vector3 GetAddDeathPos(Vector3 lastPos, float pos)
        {
            Vector3 ret = new Vector3(lastPos.x, lastPos.y + SBDefine.DeathY1, lastPos.z);
            if (transform.localScale.x < 0)
            {
                ret.x += Data.ConvertPos(pos);
            }
            else
            {
                ret.x -= Data.ConvertPos(pos);
            }

            return ret;
        }
    }
}