using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonStateBattle : GemDungeonState // 몬스터는 공격. 드래곤은 1마리 이상은 살아 있고 공격 // 한번씩 몬스터가 죽어야 됨 // 그리고 새 몬스터 나와야 됨
    {
        float nextWaveDelay = 3f;
        float destroyDelay = 2f;
        Coroutine monsterApearCor = null;

        List<int> defenseTags = new List<int>();

        public override bool OnEnter()
        {

            if (base.OnEnter())
            {
                targets.Clear();
                soundDelays.Clear();
                if (defenseTags == null)
                    defenseTags = new();
                defenseTags.Clear();

                monsterApearCor = null;
                DragonSet(eGemDungeonState.BATTLE);
                MonsterSet();
                //foreach(var dat in defenses)
                //{
                //    var data = dat.Data.GetSpine().Data;
                //}

                return true;
            }
            return false;
        }

        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Data.Time += dt;

                //전투 로직
                for (int i = 0, count = offenses.Count; i < count; ++i)
                {
                    var character = offenses[i];
                    if (character == null)
                        continue;
                    if (burnOutDragonList.Contains(character.Data.ID))
                        continue;

                    CharacterAction(character.Data, dt);
                    character.UpdateStatus(dt);
                }
                for (int i = 0, count = defenses.Count; i < count; ++i)
                {
                    var character = defenses[i];
                    if (character == null)
                        continue;
                    if (character.Data == null)
                        continue;

                    CharacterAction(character.Data, dt);
                    character.UpdateStatus(dt);
                    if (character.IsDeath && character.gameObject.activeInHierarchy)
                    {
                        character.gameObject.SetActive(false);
                        UnityEngine.Object.Destroy(character.gameObject, destroyDelay);
                        Data.DefensePos.Remove(character.Data.Position);
                        Data.DefenseDic.Remove(character.Data.Position);
                        defenseTags.Add(character.Data.ID);
                    }
                    //Data.SkillCast(character.Data, eBattleSide.DefenseSide);
                }

                if (defenseTags.Count > 0)
                {
                    // Debug.Log("monster death : " + defenseTags.Count.ToString());
                    if (monsterApearCor == null)
                    {
                        var deepList = defenseTags.ConvertAll(data => data);
                        //Debug.Log("monster event : " + deepList.Count.ToString());
                        GemDungeonUpdateEvent.Send(Floor, defenseTags.Count, deepList);
                        monsterApearCor = Stage.StartCoroutine(MonsterApearCoroutine());
                        defenseTags.Clear();
                    }
                    foreach (var spineList in Stage.DefenseSpines)
                    {
                        spineList.RemoveAll(x => x.IsDeath);
                    }
                    defenses.RemoveAll(x => x.IsDeath);
                }

                //

                UpdateProjectile(dt);//투사채 처리
                UpdateSounds(dt);//사운드 재생
                CharacterSKill();//스킬 처리
                //CheckProjectile();

                return CheckFatigue();
            }
            return false;
        }

        public override bool OnExit()
        {
            if (monsterApearCor != null)
            {
                Stage.StopCoroutine(monsterApearCor);
                monsterApearCor = null;
            }
            if (defenseTags == null)
                defenseTags = new();
            defenseTags.Clear();
            defenseDeath = 0;
            foreach (var character in defenses)
            {
                character.gameObject.SetActive(false);
                character.GetComponent<Collider2D>().enabled = false;
            }
            foreach (var character in offenses)
            {
                character.gameObject.SetActive(false);
                character.GetComponent<Collider2D>().enabled = false;
            }

            //Stage.StartCoroutine(WaitProjectileCor());
            projectiles.Clear();
            RemoveAllProjectile();
            ((GemDungeonStage)Stage).RemoveAllObject(true, true);
            burnOutDragons.Clear();
            defenses.Clear();
            offenses.Clear();
            foreach (var spineList in Stage.DefenseSpines)
            {
                spineList.Clear();
            }
            foreach (var spineList in Stage.OffenseSpines)
            {
                spineList.Clear();
            }
            Stage.OffenseSpines.Clear();
            Stage.DefenseSpines.Clear();

            soundDelays.Clear();
            projectiles.Clear();
            return base.OnExit();
        }

        IEnumerator MonsterApearCoroutine()
        {
            yield return new WaitForSeconds(nextWaveDelay);
            MonsterAdd();
            monsterApearCor = null;
        }
        bool CheckFatigue()
        {
            if (FloorData == null)
                return false;

            if (FloorData.ExpectedState == eGemDungeonState.BATTLE)
                return true;

            return false;
        }


        //    IEnumerator WaitProjectileCor()
        //    {

        //        // 코루틴 동안 defences 에 오브젝트가 넣어질 수 있으니 따로 빼둠
        //        List<GameObject> list = new List<GameObject>();   
        //        foreach(var obj in defenses)
        //        {
        //            list.Add(obj.gameObject);
        //        }
        //        foreach (var obj in offenses)
        //        {
        //            list.Add(obj.gameObject);
        //        }
        //        foreach (var projectile in projectiles)  // 여기에 안 담겨짐 
        //        {
        //            yield return new WaitUntil(()=>projectile.IsEnd);
        //        }
        //        projectiles.Clear();
        //        foreach (var gameObj in list)
        //        {
        //            UnityEngine.Object.Destroy(gameObj);
        //        }

        //    }
    }
}

