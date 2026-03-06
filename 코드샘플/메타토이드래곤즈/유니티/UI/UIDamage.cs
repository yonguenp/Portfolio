using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct UIDamageEvent
    {
        public UIDamageEvent(int damage, eDamageType type, Transform target, Vector3 addedPos, bool isMe, bool shield = false)
        {
            Damage = damage;
            Type = type;
            Target = target;
            AddedPos = addedPos;
			ImHit = isMe;
            IsShield = shield;
		}
        public int Damage { get; private set; }
        public eDamageType Type { get; set; }
        public Transform Target { get; set; }
        public Vector3 AddedPos { get; private set; }

		public bool ImHit { get; private set; }
		public bool IsShield { get; private set; }

        public static void Send(int damage, eDamageType type, Transform target, Vector3 worldPos, bool isMe = false, bool isSkill = true, bool isShield = false)
        {
			if(!isSkill && type != eDamageType.CRITICAL)
			{
				type = eDamageType.ELEMENT_NORMAL;
			}

            EventManager.TriggerEvent(new UIDamageEvent(damage, type, target, worldPos, isMe, isShield));
        }
    }
    public struct UIDamageClearEvent 
    {
        private static readonly UIDamageClearEvent e = default;
        public static void Send()
        {
            EventManager.TriggerEvent(e);
        }
    }
    public class UIDamage : MonoBehaviour, EventListener<UIDamageEvent>, EventListener<UIDamageClearEvent>
    {
        [SerializeField]
        private GameObject damagePrefab = null;
        [SerializeField]
        private GameObject specialPrefab = null;
        [SerializeField]
        private GameObject criShieldPrefab = null;
        [SerializeField]
        private GameObject missPrefab = null;
        [SerializeField]
        private RectTransform canvasRect = null;

        private SBListPool<GameObject> damagePool = null;
        private SBListPool<GameObject> specialPool = null;
        private SBListPool<GameObject> criShieldPool = null;
        private SBListPool<GameObject> missPool = null;
        private Dictionary<SBListPool<GameObject>, List<UIDamageObject>> playingList = null;
        private int damageCount = 5;
        private int specialCount = 5;
        private int missCount = 1;
        private int criShieldCount = 3;

        void Start()
        {
            if(damagePool == null)
            {
                damagePool = new SBListPool<GameObject>((GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(true);
                }, (GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(false);
                });
            }
            if(specialPool == null)
            {
                specialPool = new SBListPool<GameObject>((GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(true);
                }, (GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(false);
                });
            }
            if(missPool == null)
            {
                missPool = new SBListPool<GameObject>((GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(true);
                }, (GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(false);
                });
            }
            if(criShieldPool == null)
            {
                criShieldPool = new SBListPool<GameObject>((GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(true);
                }, (GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(false);
                });
            }

            if (playingList == null)
                playingList = new();
            else
                playingList.Clear();

            SpawnPool(specialPool, specialPrefab, specialCount);
            SpawnPool(damagePool, damagePrefab, damageCount);
            SpawnPool(missPool, missPrefab, missCount);
            SpawnPool(criShieldPool, criShieldPrefab, criShieldCount);
            this.EventStart<UIDamageEvent>();
            this.EventStart<UIDamageClearEvent>();
        }

        private void SpawnPool(SBListPool<GameObject> pool, GameObject prefab, int count)
        {
            if (pool == null || prefab == null)
                return;

            while (pool.Count < count)
            {
                pool.Put(Instantiate(prefab, transform));
            }
        }
        private void OnDestroy()
        {
            this.EventStop<UIDamageEvent>();
            this.EventStop<UIDamageClearEvent>();
        }
        public void OnEvent(UIDamageEvent eventType)
        {
            SBListPool<GameObject> targetPool = null;
            GameObject targetPrefab = null;
            string text = eventType.Damage.ToString();
            if (eventType.Damage <= 0)
                eventType.Type = eDamageType.MISS;
            Color color = Color.white;
            // 2024-02-07 1.5 -> 1.2 변경.
            float fontScale = eventType.ImHit ? 1f : 1.2f;
            switch (eventType.Type)
            {
                case eDamageType.CRITICAL:
                    //if (!eventType.ImHit)
                    //    Camera.main.GetComponent<ProCamera2DShake>().Shake("CriDmgShake");
                    targetPool = specialPool;
                    targetPrefab = specialPool.Get();
                    if (eventType.IsShield)
                    {
                        targetPool = criShieldPool;
                        targetPrefab = criShieldPool.Get();
                    }
                    if (targetPrefab == null)
                    {
                        if (eventType.IsShield)
                            SpawnPool(criShieldPool, criShieldPrefab, criShieldCount);
                        else
                            SpawnPool(specialPool, specialPrefab, specialCount);
                        targetPrefab = targetPool.Get();
                    }
                    text = SBFunc.StrBuilder("!", text);
                    break;
                case eDamageType.MISS:
                    targetPool = missPool;
                    targetPrefab = missPool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(missPool, missPrefab, missCount);
                        targetPrefab = missPool.Get();
                    }
                    text = eventType.ImHit ? "M" : "m";
                    fontScale = 1f;
                    break;
                case eDamageType.ELEMENT_FIRE:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
                        color = new Color(1f, 0.2f, 0.26f);
                    else
                        color = Color.white;
                    //color = new Color(1f, 0.4157f, 0.306f);
                    break;
                case eDamageType.ELEMENT_WATER:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
                        color = new Color(1f, 0.2f, 0.26f);
                    else
                        color = Color.white;
                    //color = new Color(0.333f, 0.706f, 1f);
                    break;
                case eDamageType.ELEMENT_EARTH:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
                        color = new Color(1f, 0.2f, 0.26f);
                    else
                        color = Color.white;
                    //color = new Color(0.753f, 0.486f, 0.09f);
                    break;
                case eDamageType.ELEMENT_WIND:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
                        color = new Color(1f, 0.2f, 0.26f);
                    else
                        color = Color.white;
                    //color = new Color(0.204f, 0.992f, 0.96f);
                    break;
                case eDamageType.ELEMENT_LIGHT:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
                        color = new Color(1f, 0.2f, 0.26f);
                    else
                        color = Color.white;
                    //color = new Color(0.992f, 0.96f, 0.204f);
                    break;
                case eDamageType.ELEMENT_DARK:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
                        color = new Color(1f, 0.2f, 0.26f);
                    else
                        color = Color.white;
                    //color = new Color(0.616f, 0.204f, 0.992f);
                    break;
                case eDamageType.ELEMENT_NORMAL:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    if (eventType.IsShield)
                        color = new Color(0.59f, 0.92f, 1f);
                    else if (eventType.ImHit)
						color = new Color(1f, 0.2f, 0.26f);
					else
						color = Color.white;
					break;
                case eDamageType.SKILL:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    color = new Color(1f, 0.3333f, 0.3333f);
                    break;
                case eDamageType.RECORVERY:
                    targetPool = damagePool;
                    targetPrefab = damagePool.Get();
                    if (targetPrefab == null)
                    {
                        SpawnPool(damagePool, damagePrefab, damageCount);
                        targetPrefab = damagePool.Get();
                    }
                    color = new Color(0.2f, 0.85f, 0.26f);
                    break;
                default:
                    break;
            }

            if (targetPrefab == null)
                return;

            var targetObj = targetPrefab;
            targetObj.transform.localScale = Vector3.one * fontScale;
            var damageObj = targetObj.GetComponent<UIDamageObject>();
            if (damageObj != null)
                damageObj.SetText(text, color);

            if (eventType.Target != null)
            {
                var effectTr = eventType.Target.GetComponent<statusEffect>();
                if (effectTr != null && effectTr.EffectTr != null)
                {
                    eventType.Target = effectTr.EffectTr;
                }
            }

            damageObj.PlayAnim();
            damageObj.StartCoroutine(TargetFollow(targetPool, damageObj, eventType.Target, eventType.AddedPos));
        }

        public void OnEvent(UIDamageClearEvent e)
        {
            if (playingList == null)
                return;

            var it = playingList.GetEnumerator();
            while(it.MoveNext())
            {
                if (it.Current.Key == null || it.Current.Value == null)
                    continue;

                var targetPool = it.Current.Key;
                var targetList = it.Current.Value;
                for (int i = 0, count = targetList.Count; i < count; ++i)
                {
                    if (targetList[i] == null)
                        continue;

                    targetList[i].StopAllCoroutines();
                    targetPool.Put(targetList[i].gameObject);
                }
                targetList.Clear();
            }
        }

        IEnumerator TargetFollow(SBListPool<GameObject> targetPool, UIDamageObject damageObj, Transform target, Vector3 addedPos)
        {
            var mainCam = Camera.main;
            if (canvasRect == null)
            {
                if (targetPool != null && damageObj != null)
                    targetPool.Put(damageObj.gameObject);
                yield break;
            }
            if (!playingList.ContainsKey(targetPool))
                playingList.Add(targetPool, new());

            playingList[targetPool].Add(damageObj);
            while (damageObj != null && damageObj.IsPlaying())
            {
                if(target != null)
                {
                    var position = mainCam.WorldToViewportPoint(target.position + addedPos);

                    Vector2 screenPos = new Vector2(position.x * canvasRect.sizeDelta.x - (canvasRect.sizeDelta.x * 0.5f),
                        position.y * canvasRect.sizeDelta.y - (canvasRect.sizeDelta.y * 0.5f));

                    damageObj.transform.localPosition = screenPos;
                }
                yield return null;
            }

            playingList[targetPool].Remove(damageObj);
            if (targetPool != null && damageObj != null)
                targetPool.Put(damageObj.gameObject);
            yield break;
        }
    }
}