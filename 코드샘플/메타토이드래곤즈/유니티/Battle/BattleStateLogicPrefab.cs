using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public partial class BattleStateLogic
    {
        protected virtual Vector3 GetAddedPosition(IBattleCharacterData targetData, SkillSummonData data)
        {
            if (targetData == null || data == null)
                return Vector3.zero;

            return new Vector3(targetData.ConvertPos(data.POSITION_X), data.POSITION_Y) * SBDefine.CONVERT_FLOAT;
        }
        protected Vector3 ColliderPos(Vector3 targetPos, Vector3 addedPos, BattleSpine caster, BattleSpine target)
        {
            if (caster == null || target == null)
                return targetPos;

            var cLocalScale = caster.transform.localScale;
            var cOffset = caster.Controller.myCollider.offset;
            float cRadius = caster.Controller.myCollider.radius;

            var tLocalScale = target.transform.localScale;
            var tOffset = target.Controller.myCollider.offset;
            float tRadius = target.Controller.myCollider.radius;

            targetPos.x += ((cRadius + cOffset.x) * Mathf.Abs(cLocalScale.x) + (tRadius + tOffset.x) * Mathf.Abs(tLocalScale.x)) * addedPos.x;
            targetPos.y += ((cRadius + cOffset.y) * Mathf.Abs(cLocalScale.y) + (tRadius + tOffset.y) * Mathf.Abs(tLocalScale.y)) * addedPos.y * 0.5f;

            return targetPos;
        }
        protected virtual Vector3 EffectPos(SkillResourceData data, IBattleCharacterData casterData, IBattleCharacterData targetData, Vector3 pos)
        {
            if (data == null)
                return Vector3.zero;

            //Vector3 cPos;
            //if (casterData.Transform != null && casterData.Transform.localScale.x < 0)
            //    cPos = new Vector3(targetData.Transform.position.x - pos.x, targetData.Transform.position.y + pos.y, 0f);
            //else
            //    cPos = new Vector3(targetData.Transform.position.x + pos.x, targetData.Transform.position.y + pos.y, 0f);
            var cPos = new Vector3(targetData.Transform.position.x + pos.x, targetData.Transform.position.y + pos.y, 0f);

            switch (data.LOCATION)
            {
                case eSkillResourceLocation.TOP:
                {
                    var effectTr = targetData.EffectTransform;
                    if (effectTr != null)
                        cPos = new Vector3(effectTr.position.x + pos.x, effectTr.position.y + pos.y, 0f);
                } break;
                case eSkillResourceLocation.BOTTOM:
                    break;
                case eSkillResourceLocation.COLLIDER:
                {
                    if (casterData != targetData)
                    {
                        Vector2 rv = new Vector2(RandomVal() - 0.5f, RandomVal() - 0.5f).normalized;
                        var collider = targetData.GetCircleCollider();
                        if (collider != null)
                        {
                            Vector3 targetScale = targetData.Transform.localScale;
                            rv *= (collider.radius * targetScale.x) * RandomVal();
                            pos.x += rv.x + (collider.offset.x * targetScale.x);
                            pos.y += rv.y + (collider.offset.y * targetScale.y);
                        }
                    }
                } break;
                default:
#if DEBUG
                    Debug.LogError("EffectPos => data LOCATION Error");
#endif
                    break;
            }
            return cPos;
        }
        protected virtual GameObject CreatePrefab(SkillResourceData data)
        {
            if (data != null)
            {
                var obj = data.GetResourcePrefab();
                if (obj != null)
                    return Object.Instantiate(obj, Stage.Map.EffectBeacon);
            }

            return new GameObject();
        }
        protected virtual GameObject CreatePrefabEffect(SkillResourceData data, IBattleCharacterData casterData, IBattleCharacterData targetData, Vector2 pos, Vector3 scale)
        {
            var target = targetData.Transform;
            if (target == null)
                return null;

            if (targetData == null || targetData.Transform == null || (data.ORDER_TYPE == eSkillResourceOrderType.CHAR))
            {
                return null;//찌꺼기 남음
            }

            Vector3 cPos = EffectPos(data, casterData, targetData, pos);
            GameObject obj = CreatePrefab(data);
            if (data == null)
            {
                obj.transform.position = cPos;
                return obj;
            }

            Vector3 lScale = obj.transform.localScale;
            Vector3 cScale = new Vector3(lScale.x * scale.x, lScale.y * scale.y, lScale.z * scale.z);

            var skillActive = obj.GetComponents<SkillActive>();
            if (skillActive != null)
            {
                for (int i = 0, count = skillActive.Length; i < count; ++i)
                {
                    if (skillActive[i] == null)
                        continue;

                    skillActive[i].Set(casterData, targetData);
                    skillActive[i].Active();
                }
            }

            switch (data.ORDER_TYPE)
            {
                case eSkillResourceOrderType.CHAR:
                    obj.transform.SetParent(targetData.Transform, true);
                    break;
                case eSkillResourceOrderType.WORLD:
                case eSkillResourceOrderType.NONE:
                default:
                    obj.transform.SetParent(Stage.Map.EffectBeacon, true);
                    break;
            }

            var sort = obj.GetComponent<SortingGroup>();
            if (sort == null)
                sort = obj.AddComponent<SortingGroup>();
            switch (data.ORDER)
            {
                case eSkillResourceOrder.BACK:
                    sort.sortingOrder = -1;
                    break;
                case eSkillResourceOrder.FRONT:
                    sort.sortingOrder = 99;
                    break;
                case eSkillResourceOrder.AUTO:
                default:
                    sort.sortingOrder = 0;
                    break;
            }

            switch (data.FOLLOW)
            {
                case eSkillResourceFollow.FOLLOW:
                    var follow = obj.GetComponent<FollowPosition>();
                    if (follow == null)
                        follow = obj.AddComponent<FollowPosition>();

                    follow.Set(targetData.Transform, obj.transform.parent, pos, targetData.IsLeft);
                    break;
                default:
                    break;
            }

            obj.transform.position = cPos;
            obj.transform.localScale = cScale;

            return obj;
        }
        protected virtual GameObject CreatePrefabEffect(SkillResourceData data, IBattleCharacterData casterData, Vector3 position, Vector3 addedPos, Vector3 scale)
        {
            GameObject obj = CreatePrefab(data);
            if(data == null)
            {
                obj.transform.position = position + addedPos;
                return obj;
            }

            Vector3 lScale = obj.transform.localScale;

            Vector3 cPos = position + addedPos;
            Vector3 cScale = new Vector3(lScale.x * scale.x, lScale.y * scale.y, lScale.z * scale.z);

            switch (data.ORDER_TYPE)
            {
                case eSkillResourceOrderType.CHAR:
                    obj.transform.SetParent(casterData.Transform, true);
                    break;
                case eSkillResourceOrderType.WORLD:
                case eSkillResourceOrderType.NONE:
                default:
                    obj.transform.SetParent(Stage.Map.EffectBeacon, true);
                    break;
            }

            switch (data.FOLLOW)
            {
                case eSkillResourceFollow.FOLLOW:
                    var follow = obj.GetComponent<FollowPosition>();
                    if (follow == null)
                        follow = obj.AddComponent<FollowPosition>();

                    follow.Set(casterData.Transform, obj.transform.parent, addedPos, casterData.IsLeft);
                    break;
                default:
                    break;
            }

            var sort = obj.GetComponent<SortingGroup>();
            if (sort == null)
                sort = obj.AddComponent<SortingGroup>();
            switch (data.ORDER)
            {
                case eSkillResourceOrder.BACK:
                    sort.sortingOrder = -1;
                    break;
                case eSkillResourceOrder.FRONT:
                    sort.sortingOrder = 99;
                    break;
                case eSkillResourceOrder.AUTO:
                default:
                    sort.sortingOrder = 0;
                    break;
            }

            obj.transform.position = cPos;
            obj.transform.localScale = cScale;

            return obj;
        }
        protected virtual GameObject CreateFieldEffect(SkillResourceData data, IBattleCharacterData casterData, Vector3 position, Vector2 addedPos, Vector3 scale)
        {
            if (casterData == null)
                return null;

            var effectTile = CreatePrefabEffect(data, casterData, position, addedPos, scale);
            if (effectTile == null)
                return null;

            var fieldObj = new SBFieldObject();
            fieldObj.Set(effectTile);
            AddEvent(fieldObj);
            return effectTile;
        }
        protected virtual GameObject CreateFieldEffect(SkillResourceData data, IBattleCharacterData casterData, IBattleCharacterData targetData, Vector2 addedPos, Vector3 scale)
        {
            if (casterData == null || targetData == null)
                return null;

            var effectTile = CreatePrefabEffect(data, casterData, targetData, addedPos, scale);
            if (effectTile == null)
                return null;

            var fieldObj = new SBFieldObject();
            fieldObj.Set(effectTile);
            AddEvent(fieldObj);
            return effectTile;
        }
        /// <summary>
        /// 수정예정
        /// </summary>
        /// <param name="data"></param>
        /// <param name="effect"></param>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual SBFollowObject CreateFollowEffect(SkillResourceData data, SkillEffectData effect, IBattleCharacterData caster, IBattleCharacterData target, float duration)
        {
            if (data == null)
                return null;

            var ePrefab = data.GetResourcePrefab();
            if (ePrefab != null)
            {
                Transform targetTransform = target.Transform;
                Transform attachTransform = Stage.Map.EffectBeacon;
                switch (effect.TYPE)
                {
                    case eSkillEffectType.POISON:
                    case eSkillEffectType.STUN:
                        targetTransform = target.EffectTransform;
                        attachTransform = targetTransform;
                        break;
                    default:
                        break;
                }
                Vector3 addedPos = Vector3.zero;
                var check = ePrefab.GetComponent<SBEffectPosType>();
                if (check != null)
                {
                    switch (check.EffectPosType)
                    {
                        case eEffectPosType.Top:
                            targetTransform = target.EffectTransform;
                            break;
                        case eEffectPosType.Collider:
                            addedPos = target.GetCircleCollider().offset;
                            break;
                        case eEffectPosType.Bottom:
                        default:
                            break;
                    }
                    switch (check.AttachType)
                    {
                        case eAttachType.Target:
                            attachTransform = targetTransform;
                            break;
                        case eAttachType.Map:
                        default:
                            break;
                    }
                }

                var name = SBFunc.StrBuilder(effect.TYPE.ToString(), effect.NEST_GROUP);
                SBFollowObject eventObject = null;
                if(effect.NEST_GROUP != 0)
                {
                    name = effect.GetSkillName();
                    for (int i = 0, count = events.Count; i < count; ++i)
                    {
                        if (events[i] == null)
                            continue;

                        if (events[i] is SBFollowObject)
                        {
                            var e = events[i] as SBFollowObject;
                            if (e.EffectName == name && e.TargetData == target)
                            {
                                eventObject = e;
                                e.Set(null, targetTransform, target, effect.TYPE.ToString(), duration, 0, null, addedPos);
                                break;
                            }
                        }
                    }
                }

                if (eventObject == null)
                {
                    var obj = Object.Instantiate(ePrefab, attachTransform);

                    if (check != null)
                    {
                        switch (check.SiblingType)
                        {
                            case eSiblingType.First:
                                obj.transform.SetAsFirstSibling();
                                break;
                            case eSiblingType.Last:
                                obj.transform.SetAsLastSibling();
                                break;
                            default:
                                break;
                        }
                    }

                    if (target.IsEnemy)
                    {
                        var scale = obj.transform.localScale;
                        scale.x = -scale.x;
                        obj.transform.localScale = scale;
                    }

                    var pos = target.Transform.position;
                    pos.y -= 0.00001f;
                    obj.transform.position = pos;

                    var skillActive = obj.GetComponents<SkillActive>();
                    if (skillActive != null)
                    {
                        for (int i = 0, count = skillActive.Length; i < count; ++i)
                        {
                            if (skillActive[i] == null)
                                continue;

                            skillActive[i].Set(caster, target);
                            skillActive[i].Active();
                        }
                    }

                    switch (data.ORDER)
                    {
                        case eSkillResourceOrder.BACK:
                        {
                            var sort = obj.GetComponent<SortingGroup>();
                            if (sort == null)
                                sort = obj.AddComponent<SortingGroup>();
                            sort.sortingOrder = -1;
                        } break;
                        case eSkillResourceOrder.FRONT:
                        {
                            var sort = obj.GetComponent<SortingGroup>();
                            if (sort == null)
                                sort = obj.AddComponent<SortingGroup>();
                            sort.sortingOrder = 99;
                        } break;
                        case eSkillResourceOrder.AUTO:
                        default:
                            break;
                    }

                    eventObject = new SBFollowObject();
                    eventObject.Set(obj.transform, targetTransform, target, name, duration, 0, null, addedPos);
                    AddEvent(eventObject);

                    return eventObject;
                }
            }

            return null;
        }
        protected virtual SBFollowObject CreateFollowEffect(SkillResourceData data, SkillPassiveData passive, IBattleCharacterData casterData, IBattleCharacterData targetData, float duration)
        {
            if (data == null)
                return null;

            var ePrefab = data.GetResourcePrefab();
            if (ePrefab != null)
            {
                Transform targetTransform = targetData.Transform;
                Transform attachTransform = Stage.Map.EffectBeacon;

                Vector3 addedPos = Vector3.zero;
                var check = ePrefab.GetComponent<SBEffectPosType>();
                if (check != null)
                {
                    switch (check.EffectPosType)
                    {
                        case eEffectPosType.Top:
                            targetTransform = targetData.EffectTransform;
                            break;
                        case eEffectPosType.Collider:
                            addedPos = targetData.GetCircleCollider().offset;
                            break;
                        case eEffectPosType.Bottom:
                        default:
                            break;
                    }
                    switch (check.AttachType)
                    {
                        case eAttachType.Target:
                            attachTransform = targetTransform;
                            break;
                        case eAttachType.Map:
                        default:
                            break;
                    }
                }

                var name = SBFunc.StrBuilder(passive.PASSIVE_EFFECT.ToString(), passive.NEST_GROUP);
                SBFollowObject eventObject = null;
                if (passive.NEST_GROUP != 0)
                {
                    name = passive.GetPassiveName();
                    for (int i = 0, count = events.Count; i < count; ++i)
                    {
                        if (events[i] == null)
                            continue;

                        if (events[i] is SBFollowObject)
                        {
                            var e = events[i] as SBFollowObject;
                            if (e.EffectName == name && e.TargetData == targetData)
                            {
                                eventObject = e;
                                e.Set(null, targetTransform, targetData, passive.PASSIVE_EFFECT.ToString(), duration, 0, null, addedPos);
                                break;
                            }
                        }
                    }
                }

                if (eventObject == null)
                {
                    if (targetData == null || targetData.Transform == null)
                    {
                        return null;//찌꺼기 남음
                    }

                    var obj = Object.Instantiate(ePrefab, attachTransform);

                    if (check != null)
                    {
                        switch (check.SiblingType)
                        {
                            case eSiblingType.First:
                                obj.transform.SetAsFirstSibling();
                                break;
                            case eSiblingType.Last:
                                obj.transform.SetAsLastSibling();
                                break;
                            default:
                                break;
                        }
                    }

                    if (targetData.IsEnemy)
                    {
                        var scale = obj.transform.localScale;
                        scale.x = -scale.x;
                        obj.transform.localScale = scale;
                    }

                    var pos = targetData.Transform.position;
                    pos.y -= 0.00001f;
                    obj.transform.position = pos;

                    var skillActive = obj.GetComponents<SkillActive>();
                    if (skillActive != null)
                    {
                        for (int i = 0, count = skillActive.Length; i < count; ++i)
                        {
                            if (skillActive[i] == null)
                                continue;

                            skillActive[i].Set(casterData, targetData);
                            skillActive[i].Active();
                        }
                    }

                    switch (data.ORDER)
                    {
                        case eSkillResourceOrder.BACK:
                        {
                            var sort = obj.GetComponent<SortingGroup>();
                            if (sort == null)
                                sort = obj.AddComponent<SortingGroup>();
                            sort.sortingOrder = -1;
                        } break;
                        case eSkillResourceOrder.FRONT:
                        {
                            var sort = obj.GetComponent<SortingGroup>();
                            if (sort == null)
                                sort = obj.AddComponent<SortingGroup>();
                            sort.sortingOrder = 99;
                        } break;
                        case eSkillResourceOrder.AUTO:
                        default:
                            break;
                    }

                    eventObject = new SBFollowObject();
                    eventObject.Set(obj.transform, targetTransform, targetData, name, duration, 0, null, addedPos);
                    AddEvent(eventObject);

                    return eventObject;
                }
            }

            return null;
        }
    }
}