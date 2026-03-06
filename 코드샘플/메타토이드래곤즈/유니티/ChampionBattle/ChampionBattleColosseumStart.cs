using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleColosseumStart : ChampionBattleColosseumState, EventListener<UIBattleObjectEndEvent>
    {
        bool animEnd = false;
        Dictionary<int, BattleSpine> animDragons = null;
        public override bool OnEnter()
        {
            if (base.OnEnter()) //처음 시간설정, UI, 연출 딜레이 등
            {
                ChampionManager.Instance.LogInit();
                
                if (animDragons == null)
                    animDragons = new();
                else
                    animDragons.Clear();

                var offenseDragons = Data.OffenseDic;
                if (offenseDragons != null)
                {
                    foreach (var it in offenseDragons)
                    {
                        if (it.Value == null)
                            continue;

                        var champDragon = Stage.GetOffenseSpine(it.Value);
                        if (champDragon == null)
                            continue;

                        ProCamera2D.Instance.AddCameraTarget(champDragon.transform, 0, 0);
                        Stage.SetGreenHpBar(it.Value);
                    }
                }

                var defenseDragons = Data.DefenseDic;
                if (defenseDragons != null)
                {
                    foreach (var it in defenseDragons)
                    {
                        if (it.Value == null)
                            continue;

                        var champDragon = Stage.GetDefenseSpine(it.Value);
                        if (champDragon == null)
                            continue;

                        ProCamera2D.Instance.AddCameraTarget(champDragon.transform, 0, 0);
                        Stage.SetRedHpBar(it.Value);
                    }
                }

                ChampionManager.Instance.ChampionData.InitializeCharacterPos();
                if (Data.Characters != null)
                {
                    var it = Data.Characters.GetEnumerator();
                    while (it.MoveNext())
                    {
                        Stage.Map.Coroutine.StartCoroutine(DropDragon(it.Current));
                    }

                    Stage.Map.Coroutine.StartCoroutine(CameraShake());
                }
                animEnd = false;
                EventManager.AddListener(this);

              
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                animEnd = false;
                EventManager.RemoveListener(this);
                return true;
            }
            return false;
        }
        public override bool OnPause()
        {
            if (base.OnPause()) //일시 정지
            {
                return true;
            }
            return false;
        }
        public override bool OnResume()
        {
            if (base.OnResume()) //복구
            {
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return (animDragons != null && animDragons.Count > 0) || !animEnd;
            }
            return false;
        }
        public IEnumerator DropDragon(IBattleCharacterData data)
        {
            if (data == null)
                yield break;

            var spine = data.GetSpine();
            if (spine == null || spine.Data == null)
                yield break;

            animDragons.Add(spine.Data.Position, spine);
            var startPosition = new Vector3(0f, 12f, 0f);
            spine.SpineTransform.localPosition = startPosition;
            var time = 0.8f;
            var maxTime = 0.8f;
            while (time > 0f)
            {
                spine.SpineTransform.localPosition = new Vector3(0, SBFunc.BezierCurveSpeed(startPosition.y, 0f, maxTime - time, maxTime, new Vector4(0f, 0f, 0.9f, -1f)), 0);
                if (spine.ShadowTransform != null)
                    spine.ShadowTransform.localScale = new Vector3(SBFunc.BezierCurveSpeed(0f, 1.2f, maxTime - time, maxTime, new Vector4(1f, 0f, 1f, 0f)), SBFunc.BezierCurveSpeed(0f, 1f, maxTime - time, maxTime, new Vector4(1f, 0f, 1f, 0f)), 1f);
                yield return null;
                time -= SBGameManager.Instance.DTime;
                spine.SetAnimation(eSpineAnimation.LOSE);
            }
            if (spine.ShadowTransform != null)
                spine.ShadowTransform.localScale = Vector3.one * 1.2f;
            spine.SpineTransform.localPosition = new Vector3(0f, 0f, 0f);
            spine.MixAnim(SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), SBDefine.GetDragonAnimTypeToName(eSpineAnimation.LOSE), 0.1f, 0.6f, 0.8f);
            yield return SBDefine.GetWaitForSeconds(1.5f);
            spine.SetAnimation(eSpineAnimation.IDLE);
            animDragons.Remove(spine.Data.Position);
            yield break;
        }
        public IEnumerator CameraShake()
        {
            var proCamera = GameObject.FindGameObjectWithTag("MainCamera");
            yield return SBDefine.GetWaitForSeconds(0.7f);
            UIBattleObjectStartEvent.Send();
            yield return SBDefine.GetWaitForSeconds(0.05f);
            if (proCamera != null)
            {
                var shake = proCamera.GetComponent<ProCamera2DShake>();
                if (shake != null)
                    shake.Shake(0);
            }
            SoundManager.Instance.PlaySFX("sfx_dragon_landing_1");
            //착지스파인 추가
            //
            yield return SBDefine.GetWaitForSeconds(0.45f);
            yield break;
        }
        public void OnEvent(UIBattleObjectEndEvent eventType)
        {
            animEnd = true;
        }
    }

    public class ChampionPracticeColosseumStart : ChampionBattleColosseumStart
    {

    }

}