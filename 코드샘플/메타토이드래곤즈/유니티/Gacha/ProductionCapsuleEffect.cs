using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 캡슐 마다 이펙트 제어부. 타이밍은 외부에서 세팅한 데이터로.
/// </summary>
namespace SandboxNetwork
{
    public class ProductionCapsuleEffect : MonoBehaviour
    {
        [SerializeField] List<UIParticle> idleEffectList = new List<UIParticle>();//각 등급별로 켜져야함 가챠 일땐 normal과 rare 까지만, 합성일 때는 제한 풀기 (전부 다씀)
        [SerializeField] List<UIParticle> openEffectList = new List<UIParticle>();//일단은 단일로 쓴대서 1개만
        /*
         * 09 -> rare (3)
         * 13 -> unique(4)
         * 14 -> legen(5)
         */

        public void InitEffect()
        {
            InitIdleEffect();
            InitOpenEffect();
        }

        void InitIdleEffect()
        {
            if (idleEffectList == null || idleEffectList.Count <= 0)
                return;

            foreach (var effect in idleEffectList)
                effect?.gameObject.SetActive(false);
        }

        void InitOpenEffect()
        {
            if (openEffectList == null || openEffectList.Count <= 0)
                return;

            foreach (var effect in openEffectList)
                effect?.gameObject.SetActive(false);
        }

        public void ShowIdleEffect(int _grade)
        {
            InitOpenEffect();

            var index = 0;
            switch(_grade)
            {
                case (int)eDragonGrade.Normal:
                case (int)eDragonGrade.Uncommon:
                    index = 0;
                    break;
                case (int)eDragonGrade.Rare:
                case (int)eDragonGrade.Unique:
                case (int)eDragonGrade.Legend:
                    index = 1;// isGacha ? 1 : _grade - 2;
                    break;
            }

            if(index < idleEffectList.Count && index >= 0)
            {
                idleEffectList[index].gameObject.SetActive(true);
                idleEffectList[index].Clear();
                idleEffectList[index].Play();
            }
        }

        public void ShowOpenEffect(int _grade)
        {
            InitIdleEffect();

            var index = 0;
            switch (_grade)
            {
                case (int)eDragonGrade.Normal:
                case (int)eDragonGrade.Uncommon:
                    return;
                case (int)eDragonGrade.Rare:
                    index = 0;
                    break;
                case (int)eDragonGrade.Unique:
                    index = 1;
                    break;
                case (int)eDragonGrade.Legend:
                    index = 2;
                    break;
            }

            if (index < openEffectList.Count && index >= 0)
            {
                openEffectList[index].gameObject.SetActive(true);
                openEffectList[index].Clear();
                openEffectList[index].Play();
            }
        }
    }
}
