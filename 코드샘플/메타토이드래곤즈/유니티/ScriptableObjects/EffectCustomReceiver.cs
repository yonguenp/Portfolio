using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct EffectReceiverEvent
    {
        public EffectCustomData Data;
        public SBSpine<eSpineAnimation> Spine;
        public EffectCustom Effect;

        public EffectReceiverEvent(EffectCustomData Data, SBSpine<eSpineAnimation> Spine, EffectCustom Effect)
        {
            this.Data = Data;
            this.Spine = Spine;
            this.Effect = Effect;
        }

        public static void Send(EffectCustomData Data, SBSpine<eSpineAnimation> Spine, EffectCustom Effect)
        {
            EventManager.TriggerEvent(new EffectReceiverEvent(Data, Spine, Effect));
        }
    }
    public struct EffectReceiverClearEvent
    {
        public static EffectReceiverClearEvent e;
        public static void Send()
        {
            EventManager.TriggerEvent(e);
        }
    }
    public class EffectCustomReceiver : MonoBehaviour, EventListener<EffectReceiverEvent>, EventListener<EffectReceiverClearEvent>
    {
        [SerializeField]
        private bool isArena = false;//Arena Receiver 인지 확인용도
        [SerializeField]
        private eEffectReceiverOption option = eEffectReceiverOption.ALL;//Arena Receiver 인지 확인용도

        private Dictionary<eEffectCustomType, KeyValuePair<EffectReceiverEvent, IEnumerator>> coroutineDic = null;
        private Dictionary<eEffectCustomType, List<Tween>> tweenDic = null;

        #region Camera Shake Custom

        ProCamera2DShake proCameraShake = null;

        #endregion
        #region Camera FocusZoom Custom

        ProCamera2DZoomToFitTargets proCameraZoomToFit = null;
        ProCamera2DNumericBoundaries proCameraNumericBoundary = null;

        Transform focusObjectTransform = null;
        List<CameraTarget> backupTartgetList = new();
        CameraFocusZoomCustom cameraFocusZoomBackupData = null;

        struct NumericBoundaryValue
        {
            public float originBoundary_left;
            public float originBoundary_right;
            public float originBoundary_top;
            public float originBoundary_bottom;
        }
        NumericBoundaryValue originBoundary;


        #endregion
        // Start is called before the first frame update
        void Start()
        {
            // 카메라 컴포넌트 로드
            var proCamera = Camera.main;
            if (proCamera != null)
            {
                if (proCameraZoomToFit == null)
                {
                    proCameraZoomToFit = proCamera.GetComponent<ProCamera2DZoomToFitTargets>();
                }

                if (proCameraNumericBoundary == null)
                {
                    proCameraNumericBoundary = proCamera.GetComponent<ProCamera2DNumericBoundaries>();
                }

                if (proCameraShake == null)
                {
                    proCameraShake = proCamera.GetComponent<ProCamera2DShake>();
                }
            }

            if (coroutineDic == null)
                coroutineDic = new Dictionary<eEffectCustomType, KeyValuePair<EffectReceiverEvent, IEnumerator>>();
            else
                coroutineDic.Clear();
            if (tweenDic == null)
                tweenDic = new();
            else
                tweenDic.Clear();
        }
        #region EventListener
        private void OnEnable()
        {
            this.EventStart<EffectReceiverEvent>();
            this.EventStart<EffectReceiverClearEvent>();
        }
        private void OnDisable()
        {
            this.EventStop<EffectReceiverEvent>();
            this.EventStop<EffectReceiverClearEvent>();
        }
        public void OnEvent(EffectReceiverClearEvent eventType)
        {
            AllClearCoroutineDic();
        }
        public void OnEvent(EffectReceiverEvent eventType)
        {
            var effect = eventType.Effect;
            IEnumerator coroutine = null;
            switch (effect.effectCustomType)
            {
                case eEffectCustomType.EffectBackground:
                    if (false == option.HasFlag(eEffectReceiverOption.BACKGROUND))
                        return;

                    coroutine = StartBackground(eventType);
                    StartCoroutine(coroutine);
                    return;
                case eEffectCustomType.CameraShake:
                    if (false == option.HasFlag(eEffectReceiverOption.CAMERA_SHAKE))
                        return;

                    coroutine = StartCameraShake(eventType);
                    break;
                case eEffectCustomType.CameraFocusZoom:
                    if (false == option.HasFlag(eEffectReceiverOption.CAMERA_ZOOM))
                        return;

                    coroutine = StartCameraFocusZoom(eventType);
                    break;
                case eEffectCustomType.EffectOutlineScale:
                    if (false == option.HasFlag(eEffectReceiverOption.OUTLINE_SCALE))
                        return;

                    coroutine = StartOutlineScale(eventType);
                    break;
                case eEffectCustomType.EffectTimeScale:
                    if (false == option.HasFlag(eEffectReceiverOption.TIME_SCALE))
                        return;

                    coroutine = StartTimeScale(eventType);
                    break;
                case eEffectCustomType.EffectClear:
                    break;
            }

            AddCoroutineDic(eventType, coroutine);
        }
        #endregion
        #region Coroutine 관리
        void AllClearCoroutineDic()
        {
            if (coroutineDic == null)
                return;

            var keys = new List<eEffectCustomType>(coroutineDic.Keys);
            foreach (var type in keys)
                ClearCoroutineDic(type);
        }

        void AddCoroutineDic(EffectReceiverEvent data, IEnumerator coroutine)
        {
            if (data.Effect == null || coroutine == null || coroutineDic == null)
                return;

            if (coroutineDic.ContainsKey(data.Effect.effectCustomType))
                ClearCoroutineDic(data.Effect.effectCustomType);

            coroutineDic.Add(data.Effect.effectCustomType, new KeyValuePair<EffectReceiverEvent, IEnumerator>(data, coroutine));
            StartCoroutine(coroutine);
        }

        void ClearCoroutineDic(eEffectCustomType type)
        {
            if (coroutineDic == null)
                return;

            if (coroutineDic.ContainsKey(type))
            {
                var pair = coroutineDic[type];
                if (pair.Value != null)
                    StopCoroutine(pair.Value);

                if (tweenDic != null && tweenDic.ContainsKey(type) && tweenDic[type] != null)
                {
                    for (int i = 0, count = tweenDic[type].Count; i < count; ++i)
                    {
                        if (tweenDic[type][i] == null)
                            continue;

                        tweenDic[type][i].Kill();
                    }
                    tweenDic[type].Clear();
                }

                switch (type)
                {
                    case eEffectCustomType.CameraShake:
                    {
                        proCameraShake.StopShaking();
                        proCameraShake.shakeEffectCount = 0;
                    } break;
                    case eEffectCustomType.CameraFocusZoom:
                    {
                        ResetZoomToFit();
                        proCameraZoomToFit.zoomEffectCount = 0;
                    } break;
                    case eEffectCustomType.EffectBackground:
                    {
                        var list = BattleDimmed.DimmedList;
                        if (list != null)
                        {
                            for (int i = 0, count = list.Count; i < count; ++i)
                            {
                                if (list[i] == null)
                                    continue;

                                UISkillCameraZoomOutEvent.Send(pair.Key.Data.Key, pair.Key.Spine, list[i]);
                            }
                        }
                    }
                    break;
                    case eEffectCustomType.EffectOutlineScale:
                    {
                        var prevEffect = pair.Key.Effect as EffectCustomOutlineScale;
                        if (prevEffect != null)
                        {
                            var dragonSpine = pair.Key.Spine;
                            if (dragonSpine == null)
                                return;

                            dragonSpine.SpineTransform.DOKill();
                            dragonSpine.SpineTransform.localScale = prevEffect.startScale;
                            
                            dragonSpine.SetOutline(false);
                        }
                    }
                    break;
                    case eEffectCustomType.EffectTimeScale:
                    {
                        if(isArena)
                            Time.timeScale = ArenaManager.Instance.ColosseumData.Speed;
                        else
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                    }
                    break;
                    default:
                        break;
                }

                coroutineDic.Remove(type);
            }
        }
        #endregion
        #region EventStart
        IEnumerator StartBackground(EffectReceiverEvent eventData)
        {
            var data = eventData.Effect as EffectCustomBackground;
            if (data == null)
                yield break;

            var spine = eventData.Spine;
            var dimmedObject = BattleDimmed.GetSkillDimmed(data.DimmedType);
            if (dimmedObject == null)
                yield break;

            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.delayTime, eventData.Data.SPEED));
            UISkillCameraZoomInEvent.Send(eventData.Data.Key, spine, dimmedObject, data.Alpha);
            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.BackgroundDuration, eventData.Data.SPEED));
            UISkillCameraZoomOutEvent.Send(eventData.Data.Key, spine, dimmedObject);
        }

        IEnumerator StartCameraFocusZoom(EffectReceiverEvent eventData)
        {
            var data = eventData.Effect as CameraFocusZoomCustom;
            if (data == null)
                yield break;

            proCameraZoomToFit.zoomEffectCount++;

            InitZoomEvent(eventData, data);

            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.delayTime, eventData.Data.SPEED));

            ActiveProCameraNumericBoundary(data);
            ActiveProCameraZoomToFitValue(data);

            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.ZoomDuration, eventData.Data.SPEED));

            // 현재 전투중인지 체크
            ResetZoomToFit();
            proCameraZoomToFit.zoomEffectCount--;
        }

        IEnumerator StartOutlineScale(EffectReceiverEvent eventData)
        {
            var data = eventData.Effect as EffectCustomOutlineScale;
            if (data == null)
                yield break;

            var dragonSpine = eventData.Spine;
            if (dragonSpine == null)
                yield break;

            data.startScale = dragonSpine.SpineTransform.localScale;
            if (data.delayTime > 0f)
                yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.delayTime, eventData.Data.SPEED));

            var effectScale = data.changeScale;

            dragonSpine.SpineTransform.DOKill();
            dragonSpine.SetOutline(true);
            if (data.inTime > 0f)
            {
                var inTime = CalcEffectTime(data.inTime, eventData.Data.SPEED);
                if (data.startScale.x != effectScale)
                    dragonSpine.SpineTransform.DOScaleX(effectScale, inTime);
                if (data.startScale.y != effectScale)
                    dragonSpine.SpineTransform.DOScaleY(effectScale, inTime);
                yield return SBDefine.GetWaitForSeconds(inTime);
            }

            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.duration, eventData.Data.SPEED));
            if (data.outTime > 0f)
            {
                var outTime = CalcEffectTime(data.outTime, eventData.Data.SPEED);
                if (data.startScale.x != effectScale)
                    dragonSpine.SpineTransform.DOScaleX(data.startScale.x, outTime);
                if (data.startScale.y != effectScale)
                    dragonSpine.SpineTransform.DOScaleY(data.startScale.y, outTime);
                yield return SBDefine.GetWaitForSeconds(outTime);
            }

            dragonSpine.SetOutline(false);

            yield break;
        }

        IEnumerator StartTimeScale(EffectReceiverEvent eventData)
        {
            var data = eventData.Effect as EffectCustomTimeScale;
            if (data == null)
                yield break;

            if (isArena)
                data.startScale = ArenaManager.Instance.ColosseumData.Speed;
            else
                data.startScale = AdventureManager.Instance.Data.Speed;

            if (data.delayTime > 0f)
                yield return SBDefine.GetWaitForSeconds(CalcEffectTime(data.delayTime, eventData.Data.SPEED));

            var effectScale = data.changeScale;
            if (!tweenDic.ContainsKey(eEffectCustomType.EffectTimeScale))
                tweenDic.Add(eEffectCustomType.EffectTimeScale, new List<Tween>());

            if (data.inTime > 0f)
            {
                var tween = DOTween.To(() => data.startScale, (float v) => Time.timeScale = v, effectScale, CalcEffectTime(data.inTime, eventData.Data.SPEED));
                tweenDic[eEffectCustomType.EffectTimeScale].Add(tween);
                yield return SBDefine.GetWaitForSecondsRealtime(CalcEffectTime(data.inTime, eventData.Data.SPEED));
            }

            yield return SBDefine.GetWaitForSecondsRealtime(CalcEffectTime(data.duration, eventData.Data.SPEED));
            if (data.outTime > 0f)
            {
                var tween = DOTween.To(() => effectScale, (float v) => Time.timeScale = v, data.startScale, CalcEffectTime(data.outTime, eventData.Data.SPEED));
                tweenDic[eEffectCustomType.EffectTimeScale].Add(tween);
                yield return SBDefine.GetWaitForSecondsRealtime(CalcEffectTime(data.outTime, eventData.Data.SPEED));
            }

            yield break;
        }

        IEnumerator StartCameraShake(EffectReceiverEvent eventData)
        {
            var effect = eventData.Effect as CameraShakeCustom;
            if (effect == null)
                yield break;

            InitShakePreset(effect);

            if (proCameraShake == null)
                yield break;

            proCameraShake.shakeEffectCount++;
            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(effect.delayTime, eventData.Data.SPEED));
            for(int i = 0, count = proCameraShake.ShakePresets.Count; i < count; ++i)
            {
                if (proCameraShake.ShakePresets[i] == null)
                    continue;

                if (proCameraShake.ShakePresets[i].name != effect.name)
                    continue;

                proCameraShake.Shake(CalcEffectTime(proCameraShake.ShakePresets[i].Duration, eventData.Data.SPEED),
                    proCameraShake.ShakePresets[i].Strength, proCameraShake.ShakePresets[i].Vibrato,
                    proCameraShake.ShakePresets[i].Randomness, proCameraShake.ShakePresets[i].InitialAngle,
                    proCameraShake.ShakePresets[i].Rotation, proCameraShake.ShakePresets[i].Smoothness, proCameraShake.ShakePresets[i].IgnoreTimeScale);
                break;
            }
            yield return SBDefine.GetWaitForSeconds(CalcEffectTime(effect.Duration, eventData.Data.SPEED));
            proCameraShake.shakeEffectCount--;
        }
        float CalcEffectTime(float time, float speed)
        {
            return time / (SBGameManager.Instance.TimeScale + speed * SBDefine.GlobalAnimSpeedRate);
        }
        #endregion
        #region Shake
        void InitShakePreset(CameraShakeCustom newData)
        {
            if (proCameraShake != null)
            {
                if (proCameraShake.ShakePresets.Exists(element => element.name == newData.name) == false)
                {
                    ShakePreset shakePreset = ScriptableObject.CreateInstance<ShakePreset>();
                    shakePreset.name = newData.name;
                    shakePreset.Strength = newData.Strength;
                    shakePreset.Duration = newData.Duration;
                    shakePreset.Vibrato = newData.Vibrato;
                    shakePreset.Smoothness = newData.Smoothness;
                    shakePreset.Randomness = newData.Randomness;
                    shakePreset.UseRandomInitialAngle = newData.UseRandomInitialAngle;
                    shakePreset.Rotation = newData.Rotation;
                    shakePreset.IgnoreTimeScale = newData.IgnoreTimeScale;

                    proCameraShake.ShakePresets.Add(shakePreset);
                }
            }
        }
        #endregion
        #region Zoom
        void InitZoomEvent(EffectReceiverEvent newData, CameraFocusZoomCustom effect)
        {
            focusObjectTransform = null;
            switch (effect.focusObjectType)
            {
                case eEffectCustomFocusObjectType.None:
                    break;
                case eEffectCustomFocusObjectType.Caster:
                    focusObjectTransform = newData.Spine.transform;
                    break;
                case eEffectCustomFocusObjectType.Target:
                    break;
                case eEffectCustomFocusObjectType.Projectile:
                    break;
            }

            SetProCameraNumericBoundary(effect);

            if (cameraFocusZoomBackupData == null)
                cameraFocusZoomBackupData = ScriptableObject.CreateInstance<CameraFocusZoomCustom>();

            cameraFocusZoomBackupData.ZoomOutBorder = proCameraZoomToFit.ZoomOutBorder;
            cameraFocusZoomBackupData.ZoomInBorder = proCameraZoomToFit.ZoomInBorder;
            cameraFocusZoomBackupData.ZoomInSmoothness = proCameraZoomToFit.ZoomInSmoothness;
            cameraFocusZoomBackupData.ZoomOutSmoothness = proCameraZoomToFit.ZoomOutSmoothness;
            cameraFocusZoomBackupData.MaxZoomInAmount = proCameraZoomToFit.MaxZoomInAmount;
            cameraFocusZoomBackupData.MaxZoomOutAmount = proCameraZoomToFit.MaxZoomOutAmount;
            cameraFocusZoomBackupData.DisableWhenOneTarget = proCameraZoomToFit.DisableWhenOneTarget;
            cameraFocusZoomBackupData.CompensateForCameraPosition = proCameraZoomToFit.CompensateForCameraPosition;

            backupTartgetList = new List<CameraTarget>(proCameraZoomToFit.ProCamera2D.CameraTargets);
            proCameraZoomToFit.ProCamera2D.CameraTargets.Clear();

            proCameraZoomToFit.ProCamera2D.AddCameraTarget(focusObjectTransform != null ? focusObjectTransform : Camera.main.transform, 1, 1, 0, effect.focusPostion);
        }
        void ActiveProCameraZoomToFitValue(CameraFocusZoomCustom zoomCustomData)
        {
            if (proCameraZoomToFit == null) { return; }
            if (zoomCustomData == null) return;

            proCameraZoomToFit.ZoomOutBorder = zoomCustomData.ZoomOutBorder;
            proCameraZoomToFit.ZoomInBorder = zoomCustomData.ZoomInBorder;
            proCameraZoomToFit.ZoomInSmoothness = zoomCustomData.ZoomInSmoothness;
            proCameraZoomToFit.ZoomOutSmoothness = zoomCustomData.ZoomOutSmoothness;
            proCameraZoomToFit.MaxZoomInAmount = zoomCustomData.MaxZoomInAmount;
            proCameraZoomToFit.MaxZoomOutAmount = zoomCustomData.MaxZoomOutAmount;
            proCameraZoomToFit.DisableWhenOneTarget = zoomCustomData.DisableWhenOneTarget;
            proCameraZoomToFit.CompensateForCameraPosition = zoomCustomData.CompensateForCameraPosition;
        }
        void SetProCameraNumericBoundary(CameraFocusZoomCustom zoomCustomData)
        {
            if (proCameraNumericBoundary == null || zoomCustomData == null)
                return;

            originBoundary.originBoundary_left = proCameraNumericBoundary.LeftBoundary;
            originBoundary.originBoundary_right = proCameraNumericBoundary.RightBoundary;
            originBoundary.originBoundary_top = proCameraNumericBoundary.TopBoundary;
            originBoundary.originBoundary_bottom = proCameraNumericBoundary.BottomBoundary;
        }
        void ActiveProCameraNumericBoundary(CameraFocusZoomCustom zoomCustomData)
        {
            if (proCameraNumericBoundary == null || zoomCustomData == null) 
                return;

            if (zoomCustomData.focusPostion.x > 0)
            {
                proCameraNumericBoundary.RightBoundary += zoomCustomData.focusPostion.x;
            }
            else
            {
                proCameraNumericBoundary.LeftBoundary += zoomCustomData.focusPostion.x;
            }

            if (zoomCustomData.focusPostion.y > 0)
            {
                proCameraNumericBoundary.TopBoundary += zoomCustomData.focusPostion.y;
            }
            else
            {
                proCameraNumericBoundary.BottomBoundary += zoomCustomData.focusPostion.y;
            }
        }
        void ResetZoomToFit()
        {
            ResetProCameraZoomToFit();
            ResetProCameraNumericBoundary();
            ActiveProCameraZoomToFitValue(cameraFocusZoomBackupData);
        }
        void ResetProCameraZoomToFit()
        {
            if (proCameraZoomToFit == null) return;
            if (backupTartgetList.Count <= 0) return;

            proCameraZoomToFit.ProCamera2D.CameraTargets.Clear();
            List<CameraTarget> newTargetList = new();

            // 줌연출이 적용되는 동안 Death처리된 타겟 제거
            foreach (CameraTarget target in backupTartgetList)
            {
                if (target.TargetTransform == null) continue;

                if(target.TargetTransform.name.Contains("CameraFocus"))
                {
                    newTargetList.Add(target);
                    continue;
                }

                if (isArena)
                {
                    var spine = target.TargetTransform.GetComponent<ArenaDragonSpine>();
                    if (spine == null)
                        continue;

                    var data = spine.AData;
                    if (data == null)
                        continue;

                    if (data.Death == false)
                        newTargetList.Add(target);
                }
                else
                {
                    if (AdventureStage.Instance.IsState<BattleStateLogic>())
                    {
                        var spine = target.TargetTransform.GetComponent<BattleSpine>();
                        if (spine == null)
                            continue;

                        var data = spine.Data;
                        if (data == null)
                            continue;

                        if (data.Death == false)
                            newTargetList.Add(target);
                    }
                }
            }

            proCameraZoomToFit.ProCamera2D.AddCameraTargets(newTargetList);
        }
        void ResetProCameraNumericBoundary()
        {
            if (proCameraNumericBoundary == null) { return; }

            proCameraNumericBoundary.LeftBoundary = originBoundary.originBoundary_left;
            proCameraNumericBoundary.RightBoundary = originBoundary.originBoundary_right;
            proCameraNumericBoundary.TopBoundary = originBoundary.originBoundary_top;
            proCameraNumericBoundary.BottomBoundary = originBoundary.originBoundary_bottom;
        }
        #endregion
    }
}