
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{
    public struct UISkillCutSceneEvent
    {
        public static UISkillCutSceneEvent e = default;
        public int Key;
        public eSpineAnimation Anim;
        public float EndTime;
        public SBSpine<eSpineAnimation> Target;

        public static void Send(int Key, eSpineAnimation anim, float time, SBSpine<eSpineAnimation> target)
        {
            e.Key = Key;
            e.Anim = anim;
            e.EndTime = time;
            e.Target = target;

            EventManager.TriggerEvent(e);
        }

        public void Reset()
        {
            Key = -1;
            Anim = eSpineAnimation.NONE;
            EndTime = 0f;
            Target = null;
        }
    }

    public struct UISkillCameraZoomInEvent
    {
        public UISkillCameraZoomInEvent(int Key, SBSpine<eSpineAnimation> Spine, SkillDimmed SkillDimmed, float Fade)
        {
            this.Key = Key;
            this.Spine = Spine;
            this.SkillDimmed = SkillDimmed;
            this.Fade = Fade;
        }
        public int Key;
        public SBSpine<eSpineAnimation> Spine;
        public SkillDimmed SkillDimmed;
        public float Fade;

        public static void Send(int Key, SBSpine<eSpineAnimation> Spine, SkillDimmed SkillDimmed, float Fade)
        {
            EventManager.TriggerEvent(new UISkillCameraZoomInEvent(Key, Spine, SkillDimmed, Fade));
        }
    }

    public struct UISkillCameraZoomOutEvent
    {
        public UISkillCameraZoomOutEvent(int Key, SBSpine<eSpineAnimation> Spine, SkillDimmed SkillDimmed)
        {
            this.Key = Key;
            this.Spine = Spine;
            this.SkillDimmed = SkillDimmed;
        }
        public int Key;
        public SBSpine<eSpineAnimation> Spine;
        public SkillDimmed SkillDimmed;

        public static void Send(int Key, SBSpine<eSpineAnimation> Spine, SkillDimmed Dimmed)
        {
            EventManager.TriggerEvent(new UISkillCameraZoomOutEvent(Key, Spine, Dimmed));
        }
    }

    public class UISkillCutScene : MonoBehaviour, EventListener<UISkillCutSceneEvent>, EventListener<UISkillCameraZoomInEvent>, EventListener<UISkillCameraZoomOutEvent>
    {
        //const float posX = -183f;
        //const float posY = -133f;
        readonly int SkillOrder = 201;
        readonly int DefaultOrder = 0;
        const float posX = 0f;
        const float posY = -50f;

        [SerializeField]
        private Animator skillCutSceneAnim = null;

        [SerializeField]
        private Transform moveTransform = null;
        [SerializeField]
        private Transform bgTransform = null;
        [SerializeField]
        private Transform rbTransform = null;
        [SerializeField]
        private UIDragonSpine dragonSpine_1 = null;
        [SerializeField]
        private UIDragonSpine dragonSpine_2 = null;
        [SerializeField]
        private UIDragonSpine legend1DragonSpine = null;
        [SerializeField]
        private UIDragonSpine legend2DragonSpine = null;
        [SerializeField]
        private Transform[] lines;

        private Dictionary<int, SBSpine<eSpineAnimation>> targets = null;

        Dictionary<eSkillDimmedType, Tween> dimmedTweenDic = null;
        Dictionary<Tween, SBSpine<eSpineAnimation>> tweenTargetDic = null;
        private IEnumerator coroutine = null;

        private void Awake()
        {
            SBFunc.SetUIDataAsset(dragonSpine_1, dragonSpine_2, legend1DragonSpine, legend2DragonSpine);
        }

        private void OnEnable()
        {
            if (targets == null)
                targets = new Dictionary<int, SBSpine<eSpineAnimation>>();
            else
                targets.Clear();

            if (dimmedTweenDic == null)
                dimmedTweenDic = new();
            else
                dimmedTweenDic.Clear();

            if (tweenTargetDic == null)
                tweenTargetDic = new();
            else
                tweenTargetDic.Clear();

            OnOut();
            EventManager.AddListener<UISkillCutSceneEvent>(this);
            EventManager.AddListener<UISkillCameraZoomInEvent>(this);
            EventManager.AddListener<UISkillCameraZoomOutEvent>(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<UISkillCutSceneEvent>(this);
            EventManager.RemoveListener<UISkillCameraZoomInEvent>(this);
            EventManager.RemoveListener<UISkillCameraZoomOutEvent>(this);
            ClearTween();
            StopAllCoroutines();
        }


        protected void ClearTween()
        {
            if (tweenTargetDic != null)
            {
                var tweens = new List<Tween>(tweenTargetDic.Keys);
                for (int i = 0, count = tweens.Count; i < count; ++i)
                {
                    if (tweens[i] == null)
                        continue;

                    tweens[i].Kill();
                }

                tweenTargetDic.Clear();
            }

        }

        protected void ClearDragon()
        {
            if (bgTransform != null)
                foreach (Transform child in bgTransform)
                {
                    bool isline = false;
                    foreach (Transform line in lines)
                    {
                        if (line == child)
                        {
                            isline = true;
                            break;
                        }
                    }

                    if (!isline)
                        Destroy(child.gameObject);
                }

            if (rbTransform != null)
                foreach (Transform child in rbTransform)
                {
                    bool isline = false;
                    foreach (Transform line in lines)
                    {
                        if (line == child)
                        {
                            isline = true;
                            break;
                        }
                    }

                    if (!isline)
                        Destroy(child.gameObject);
                }

            if (moveTransform != null)
                foreach (Transform child in moveTransform)
                {
                    bool isline = false;
                    foreach (Transform line in lines)
                    {
                        if (line == child)
                        {
                            isline = true;
                            break;
                        }
                    }

                    if (!isline)
                        Destroy(child.gameObject);
                }
        }

        public void OnOut()
        {
            if (skillCutSceneAnim != null)
            {
                skillCutSceneAnim.SetBool("Out", true);
                skillCutSceneAnim.SetBool("Start", false);
                skillCutSceneAnim.SetBool("In", false);
                skillCutSceneAnim.SetBool("End", false);
            }

            ClearDragon();
        }

        public void OnStart()
        {
            if (skillCutSceneAnim != null)
            {
                if (skillCutSceneAnim.GetBool("In") || skillCutSceneAnim.GetBool("Start"))
                {
                    OnIn();
                    return;
                }

                skillCutSceneAnim.SetBool("Out", false);
                skillCutSceneAnim.SetBool("Start", true);
                skillCutSceneAnim.SetBool("In", false);
                skillCutSceneAnim.SetBool("End", false);
            }
        }

        public void OnIn()
        {
            //Time.timeScale = 0.7f;
            if (skillCutSceneAnim != null)
            {
                skillCutSceneAnim.SetBool("Out", false);
                skillCutSceneAnim.SetBool("Start", false);
                skillCutSceneAnim.SetBool("In", true);
                skillCutSceneAnim.SetBool("End", false);
            }
        }

        public void OnEnd()
        {
            if (skillCutSceneAnim != null)
            {
                skillCutSceneAnim.SetBool("Out", false);
                skillCutSceneAnim.SetBool("Start", false);
                skillCutSceneAnim.SetBool("In", false);
                skillCutSceneAnim.SetBool("End", true);
            }
        }

        public void OnEvent(UISkillCutSceneEvent eventType)
        {
            if (eventType.Key < 0)
            {
                OnEnd();
                ClearCoroutine();
                return;
            }

            ClearCoroutine();

            OnStart();
            coroutine = CreateDragon(eventType.Key, eventType.Anim, eventType.EndTime, eventType.Target);
            StartCoroutine(coroutine);
        }


        IEnumerator CreateDragon(int key, eSpineAnimation anim, float time, SBSpine<eSpineAnimation> target)
        {
            var data = CharBaseData.Get(key.ToString());
            if (data == null)
                yield break;

            var prefabDragon = ResourceManager.GetResource<GameObject>(eResourcePath.UIDragonClonePath, data.IMAGE);
            if (prefabDragon == null)
                yield break;

            var scale = Vector3.one;
            scale.x = scale.x * 0.5f;
            scale.y = scale.y * 0.6667f;
            if (data.BACKGROUND != "NONE" && bgTransform != null)
            {
                var spinePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.SpecialBGPath, data.BACKGROUND);
                if (spinePrefab != null)
                {
                    var bgObj = Instantiate(spinePrefab, bgTransform, false);
                    bgObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(170f, 0f, 0f);
                    var bgScale = scale * 2.2f;
                    bgObj.transform.localScale = bgScale;
                    //bgObj.transform.localRotation = Quaternion.Inverse(ltTransform.localRotation);

                    bgObj = Instantiate(spinePrefab, rbTransform);
                    bgObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(170f, 0f, 0f);
                    bgScale.y = bgScale.y * -1f;
                    bgObj.transform.localScale = bgScale;

                    //bgObj.transform.localRotation = Quaternion.Inverse(rbTransform.localRotation);
                }
            }

            if (moveTransform != null)
            {
                dragonSpine_1.gameObject.SetActive(false);
                dragonSpine_2.gameObject.SetActive(false);
                legend1DragonSpine.gameObject.SetActive(false);
                legend2DragonSpine.gameObject.SetActive(false);
                if ((eDragonGrade)data.GRADE != eDragonGrade.Legend) // 레전더리가 아닌 일반 드래곤에 관한 처리
                {
                    if (dragonSpine_1 != null && dragonSpine_1.GetSkeletonData().FindSkin(key.ToString()) != null)
                    {
                        dragonSpine_1.SetSkin(key.ToString());
                        dragonSpine_1.SetAnimation(eSpineAnimation.ATTACK);
                        yield return SBDefine.GetWaitForEndOfFrame();
                        dragonSpine_1.gameObject.SetActive(true);
                    }
                    else if (dragonSpine_2 != null)
                    {
                        dragonSpine_2.SetSkin(key.ToString());
                        dragonSpine_2.SetAnimation(eSpineAnimation.ATTACK);
                        yield return SBDefine.GetWaitForEndOfFrame();
                        dragonSpine_2.gameObject.SetActive(true);
                    }
                }
                else // 레전더리 스파인 데이터는 스킨에 따라 2종류라서 분리
                {
                    if (legend1DragonSpine != null && legend1DragonSpine.GetSkeletonData().FindSkin(key.ToString()) != null)
                    {
                        legend1DragonSpine.SetSkin(key.ToString());
                        legend1DragonSpine.SetAnimation(eSpineAnimation.ATTACK);
                        yield return SBDefine.GetWaitForEndOfFrame();
                        legend1DragonSpine.gameObject.SetActive(true);
                    }
                    else if (legend2DragonSpine != null)
                    {
                        legend2DragonSpine.SetSkin(key.ToString());
                        legend2DragonSpine.SetAnimation(eSpineAnimation.ATTACK);
                        yield return SBDefine.GetWaitForEndOfFrame();
                        legend2DragonSpine.gameObject.SetActive(true);
                    }
                }
            }

            foreach (Transform line in lines)
            {
                line.SetAsLastSibling();
            }

            if (time > 0f)
                yield return SBDefine.GetWaitForSeconds(time);

            OnEnd();
            coroutine = null;
            yield break;
        }

        void ClearDimmed(UISkillCameraZoomOutEvent eventType)
        {
            if (!targets.ContainsKey(eventType.Key))
                targets.Add(eventType.Key, eventType.Spine);

            var curType = eventType.SkillDimmed.DimmedType;
            var dimmed = eventType.SkillDimmed.DimmedObject;
            if (dimmed != null)
            {
                if (dimmedTweenDic != null)
                {
                    if (dimmedTweenDic.ContainsKey(eventType.SkillDimmed.DimmedType))
                    {
                        var prevTween = dimmedTweenDic[eventType.SkillDimmed.DimmedType];
                        if (prevTween != null)
                        {
                            SetTweenTargetOrder(prevTween, DefaultOrder);
                            tweenTargetDic.Remove(prevTween);
                            prevTween.Kill();
                            dimmedTweenDic[eventType.SkillDimmed.DimmedType] = null;
                        }
                        var tween = dimmed.GetComponent<SpriteRenderer>().DOFade(0f, 0.2f).
                        OnUpdate(() =>
                        {
                            if (targets.ContainsKey(eventType.Key) && targets[eventType.Key] != null)
                                targets[eventType.Key].SetOrder(SkillOrder);
                        }).
                        OnComplete(() =>
                        {
                            if (targets.ContainsKey(eventType.Key) && targets[eventType.Key] != null)
                                targets[eventType.Key].SetOrder(DefaultOrder);
                            tweenTargetDic.Remove(dimmedTweenDic[eventType.SkillDimmed.DimmedType]);
                        });
                        tweenTargetDic.Add(tween, targets[eventType.Key]);
                        dimmedTweenDic[eventType.SkillDimmed.DimmedType] = tween;
                    }
                }
            }
        }

        public void OnEvent(UISkillCameraZoomInEvent eventType)
        {
            if (!targets.ContainsKey(eventType.Key))
                targets.Add(eventType.Key, eventType.Spine);

            var curType = eventType.SkillDimmed.DimmedType;
            var dimmed = eventType.SkillDimmed.DimmedObject;
            if (dimmed != null)
            {
                dimmed.SetActive(true);
                var color = dimmed.GetComponent<SpriteRenderer>().color;
                color.a = 0f;
                dimmed.GetComponent<SpriteRenderer>().color = color;
                if (dimmedTweenDic != null)
                {
                    var tween = dimmed.GetComponent<SpriteRenderer>().DOFade(eventType.Fade, 0.5f).OnUpdate(() =>
                    {
                        if (targets.ContainsKey(eventType.Key) && targets[eventType.Key] != null)
                            targets[eventType.Key].SetOrder(SkillOrder);
                    }).OnComplete(() => { tweenTargetDic.Remove(dimmedTweenDic[curType]); dimmedTweenDic[curType] = null; });
                    if (dimmedTweenDic.ContainsKey(curType))
                    {
                        var prevTween = dimmedTweenDic[curType];
                        if (prevTween != null)
                        {
                            SetTweenTargetOrder(prevTween, DefaultOrder);
                            tweenTargetDic.Remove(prevTween);
                            prevTween.Kill();
                            dimmedTweenDic[curType] = null;
                        }
                        dimmedTweenDic[curType] = tween;
                    }
                    else
                        dimmedTweenDic.Add(curType, tween);

                    tweenTargetDic.Add(tween, targets[eventType.Key]);
                }
            }
        }

        private void SetTweenTargetOrder(Tween tween, int order)
        {
            if (tweenTargetDic.ContainsKey(tween) && tweenTargetDic[tween] != null)
            {
                tweenTargetDic[tween].SetOrder(order);
            }
        }

        public void OnEvent(UISkillCameraZoomOutEvent eventType)
        {
            ClearDimmed(eventType);
        }
        private void ClearCoroutine()
        {
            if (coroutine == null)
                return;

            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}