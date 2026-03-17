using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIMonsterSpine : MonoBehaviour
	{
        [SerializeField] SkeletonGraphic skeleton;
		public void SetData(MonsterBaseData data, string ani = null)
        {
            var monsterPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.MonsterClonePath, data.IMAGE);
            SkeletonAnimation anim = monsterPrefab.GetComponentInChildren<SkeletonAnimation>();
            if (anim == null)
            {
                gameObject.SetActive(false);
                return;
            }

            bool loop = true;
            if(string.IsNullOrEmpty(ani))
            {
                ani = "monster_idle";
            }

            skeleton.skeletonDataAsset = anim.SkeletonDataAsset;
            skeleton.startingAnimation = ani;
            skeleton.startingLoop = loop;
            
            skeleton.Initialize(true);

            skeleton.MatchRectTransformWithBounds();
        }

        public void SetColor(Color color)
        {
            skeleton.color = color;
        }
        public void SetScale(Vector2 scale)
        {
            skeleton.transform.localScale = scale;
        }
    }
}
