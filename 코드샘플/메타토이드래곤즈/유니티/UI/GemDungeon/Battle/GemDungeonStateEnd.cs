using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonStateEnd : GemDungeonState // 몬스터는 우왕좌왕, 드래곤은 쓰러짐
    {
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                DragonSet(eGemDungeonState.END);
                // 몬스터 세팅
                MonsterSet();
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                MonsterMove(dt);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            DragonRemove();
            return base.OnExit();
        }
        protected void DragonRemove()
        {
            foreach (var character in offenses)
            {
                if (character != null)
                {
                    character.gameObject.SetActive(false);
                    character.GetComponent<Collider2D>().enabled = false;
                }
            }
            burnOutDragons.Clear();
            offenses.Clear();
            foreach (var spineList in Stage.OffenseSpines)
            {
                spineList.Clear();
            }
            Stage.OffenseSpines.Clear();
            ((GemDungeonStage)Stage).RemoveAllObject(true, false);
        }
    }
}
