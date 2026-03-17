using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class SBObjectEffect : MonoBehaviour
    {
        public bool IsInit { get; protected set; } = false;
        public IBattleCharacterData Data { get; protected set; } = null;
        protected virtual void Awake()
        {
            AwakeInitialize();
        }

        protected abstract bool AwakeInitialize();//하위 오브젝트의 구성요소 불러오기
        protected abstract bool InitializeData();//입력된 데이터로 Effect 동작 구성
        protected virtual void EffectDestory()
        {
            Destroy(gameObject);
        }
        public virtual void SetCharacter(IBattleCharacterData data)
        {
            Data = data;
            InitializeData();
        }
    }
}