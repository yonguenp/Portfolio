using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffController
{
    /// <summary>
    /// 버프 타입에 따른 서버 및 클라이언트 약속 정리
    /// 모든 버프 추가와 삭제는 서버에서 알려주며, 클라이언트는 현재 상태를 저장하기만 함
    /// 
    /// 스탯 버프 / 디버프 : 서버에서 스탯을 변경하여 ScBcStatusSync로 보내주면 클라에서 처리
    /// 돌진 / 점멸 : 서버에서 위치를 변경하여 클라로 보내주므로 클라는 할 것이 없음
    /// 데미지 : 서버에서 데미지 값을 계산하여 처리해주므로 클라는 할 것이 없음
    /// 스턴 : 클라와 서버 모두에서 이동을 하지 못하게 막음
    /// 혼란 : 클라에서만 서버로 보내는 이동 방향 값을 반대로 바꿈 (서버에서는 클라 값을 신뢰하여 처리)
    /// 시야방해 / 투명화 : 클라에서만 처리
    /// </summary>

    Dictionary<SkillEffectGameData.EffectStatType, int> statBuffStatus = new Dictionary<SkillEffectGameData.EffectStatType, int>();
    Dictionary<SkillEffectGameData.EffectStatType, int> statDebuffStatus = new Dictionary<SkillEffectGameData.EffectStatType, int>();
    List<SkillEffectGameData.EffectType> currentEffectStatus = new List<SkillEffectGameData.EffectType>();
    CharacterObject parentObject = null;
    BuffStatusContainer _status = new BuffStatusContainer();
    Dictionary<int, GameObject> buffEffectObjects = new Dictionary<int, GameObject>();
    int shieldBuffID = 0;

    public BuffStatusContainer BuffStatus { get { return _status; } }

    public BuffController(CharacterObject obj)
    {
        parentObject = obj;
    }

    ~BuffController()
    {
        statBuffStatus = null;
        statDebuffStatus = null;
        currentEffectStatus = null;
        parentObject = null;
        _status = null;
        shieldBuffID = 0;
    }

    public void ClearAll()
    {
        statBuffStatus?.Clear();
        statDebuffStatus?.Clear();
        currentEffectStatus?.Clear();
    }

    public void CreateBuff(int buffID)
    {
        var buffData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill_effect, buffID) as SkillEffectGameData;
        if (buffData == null)
        {
            SBDebug.LogError($"[Error] CreateBuff : {buffID}");
            return;
        }
        var effectType = buffData.Type;
        var statType = buffData.StatType;

        SBDebug.Log($"CreateBuff : {effectType} / {statType} / {buffID}");

        switch (effectType)
        {
            // 스탯 버프 및 디버프, 같은 스탯에 적용되는 버프와 디버프는 각각 덮어쓴다.
            case SkillEffectGameData.EffectType.StatBuff:
                statBuffStatus[statType] = buffID;
                if (statType == SkillEffectGameData.EffectStatType.Shield)
                    shieldBuffID = buffID;
                break;

            case SkillEffectGameData.EffectType.StatDebuff:
                statDebuffStatus[statType] = buffID;
                break;

            // 상태이상
            case SkillEffectGameData.EffectType.Stun:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                parentObject.OnStun(buffData.ActiveTime);
                _status.SetStatusFlag(ObjectBuffStatus.Stun);
                break;

            case SkillEffectGameData.EffectType.Confuse:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                _status.SetStatusFlag(ObjectBuffStatus.Confuse);
                break;

            case SkillEffectGameData.EffectType.BlockSight:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                _status.SetStatusFlag(ObjectBuffStatus.BlockSight);
                parentObject.BlockSight();
                break;

            case SkillEffectGameData.EffectType.Invisible:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                _status.SetStatusFlag(ObjectBuffStatus.Invisible);
                parentObject.PlayInvisible(false, buffData.ActiveTime);
                break;

            // 이동기
            case SkillEffectGameData.EffectType.Rush:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                //_status.SetStatusFlag(ObjectBuffStatus.Freeze);
                parentObject.Rush(buffData.Value1, buffData.ActiveTime);
                break;
            case SkillEffectGameData.EffectType.Knockback:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                //_status.SetStatusFlag(ObjectBuffStatus.Freeze);
                parentObject.Rush(buffData.Value1, buffData.ActiveTime);
                break;
            case SkillEffectGameData.EffectType.Teleport:
                break;
            
            // 즉발 버프이므로 현재 상태를 저장하지 않는다
            case SkillEffectGameData.EffectType.SkillChange:
                parentObject.ChangeSkill(buffData.Value1, buffData.Value2);
                break;
            case SkillEffectGameData.EffectType.UnlockCoolTime:
                parentObject.UnlockCoolTime(buffData.Value1);
                break;
            case SkillEffectGameData.EffectType.ReduceCoolTime:
                parentObject.ReduceCoolTime(buffData.Value1, buffData.Value2 * 0.001f);
                break;
            case SkillEffectGameData.EffectType.IncreaseCoolTime:
                parentObject.IncreaseCoolTime(buffData.Value1, buffData.Value2 * 0.001f);
                break;
            case SkillEffectGameData.EffectType.Pluck:
                if (!currentEffectStatus.Contains(effectType))
                {
                    currentEffectStatus.Add(effectType);
                }
                _status.SetStatusFlag(ObjectBuffStatus.Pluck);
                parentObject.PlayPluck(buffData);
                break;
            case SkillEffectGameData.EffectType.Location_Tracking:
                if (parentObject.IsMe)
                    parentObject.PlayTracking(buffData);
                break;

            case SkillEffectGameData.EffectType.Location_Exchange:
                break;

            case SkillEffectGameData.EffectType.Damage:
            case SkillEffectGameData.EffectType.Access_Trigger:
                break;
            // 클라이언트에서 할 것이 없는 애들
            case SkillEffectGameData.EffectType.BatteryBuff:
            case SkillEffectGameData.EffectType.Heal:
            case SkillEffectGameData.EffectType.Duration_Increase:
            case SkillEffectGameData.EffectType.Delete_Effect:
            case SkillEffectGameData.EffectType.Location_Create:
                return;
            case SkillEffectGameData.EffectType.Skill_Preview_Effect:
                _status.SetStatusFlag(ObjectBuffStatus.SkillCasting);
                var charDir = parentObject.PosInfo.MoveDir;
                Managers.Effect.PlayEffect(buffData.Value1, parentObject.RootEffect, 0, false, charDir.X, charDir.Y);
                break;
            case SkillEffectGameData.EffectType.Silence:
                _status.SetStatusFlag(ObjectBuffStatus.Silence);
                break;
            case SkillEffectGameData.EffectType.Stuck:
                _status.SetStatusFlag(ObjectBuffStatus.Freeze);
                break;
            case SkillEffectGameData.EffectType.Trap_detect:
                _status.SetStatusFlag(ObjectBuffStatus.Trap);
                break;
            default:
                //throw new Exception($"Wrong SkillEffectGameData : {buffData.GetID()}");
                SBDebug.LogWarning($"없는 이펙트 타입입니다. 만들어야됨! : {buffData.GetID()}");
                break;
        }

        if (buffEffectObjects.ContainsKey(buffID))
        {
            var oldEffectObject = buffEffectObjects[buffID];
            UnityEngine.Object.Destroy(oldEffectObject);
            buffEffectObjects.Remove(buffID);
        }

        if (buffData.ResourceID == 0) return;
        var effectObject = Managers.Effect.PlayEffect(buffData.ResourceID, parentObject.RootEffect, buffData.ActiveTime);
        if (effectObject != null)
            buffEffectObjects.Add(buffID, effectObject);
    }

    public void DeleteBuff(int buffID)
    {
        var buffData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill_effect, buffID) as SkillEffectGameData;
        var effectType = buffData.Type;
        var statType = buffData.StatType;

        SBDebug.Log($"DeleteBuff : {effectType} / {statType} / {buffID}");

        switch (effectType)
        {
            // 스탯 버프 및 디버프
            case SkillEffectGameData.EffectType.StatBuff:
                statBuffStatus.Remove(statType);
                if (statType == SkillEffectGameData.EffectStatType.Shield)
                    shieldBuffID = 0;
                break;

            case SkillEffectGameData.EffectType.StatDebuff:
                statDebuffStatus.Remove(statType);
                break;

            // 상태이상
            case SkillEffectGameData.EffectType.Stun:
                currentEffectStatus.Remove(effectType);
                _status.ClearStatusFlag(ObjectBuffStatus.Stun);
                break;

            case SkillEffectGameData.EffectType.Confuse:
                currentEffectStatus.Remove(effectType);
                _status.ClearStatusFlag(ObjectBuffStatus.Confuse);
                break;

            case SkillEffectGameData.EffectType.BlockSight:
                currentEffectStatus.Remove(effectType);
                _status.ClearStatusFlag(ObjectBuffStatus.BlockSight);
                parentObject.UnblockSight();
                break;

            case SkillEffectGameData.EffectType.Invisible:
                currentEffectStatus.Remove(effectType);
                _status.ClearStatusFlag(ObjectBuffStatus.Invisible);
                parentObject.StopInvisible();
                break;

            // 이동기
            case SkillEffectGameData.EffectType.Rush:
                currentEffectStatus.Remove(effectType);
                _status.ClearStatusFlag(ObjectBuffStatus.Freeze);
                break;
            case SkillEffectGameData.EffectType.Knockback:
                currentEffectStatus.Remove(effectType);
                _status.ClearStatusFlag(ObjectBuffStatus.Freeze);
                break;
            case SkillEffectGameData.EffectType.Teleport:
                break;

            case SkillEffectGameData.EffectType.Pluck:
                _status.ClearStatusFlag(ObjectBuffStatus.Pluck);
                return;

            // 클라이언트에서 할 것이 없는 애들
            case SkillEffectGameData.EffectType.Damage:
            case SkillEffectGameData.EffectType.BatteryBuff:
            case SkillEffectGameData.EffectType.Heal:
            case SkillEffectGameData.EffectType.Duration_Increase:
            case SkillEffectGameData.EffectType.Delete_Effect:
            case SkillEffectGameData.EffectType.SkillChange:
            case SkillEffectGameData.EffectType.Location_Teleport:
            case SkillEffectGameData.EffectType.IncreaseCoolTime:
            case SkillEffectGameData.EffectType.ReduceCoolTime:
                return;
            case SkillEffectGameData.EffectType.Access_Trigger:
                currentEffectStatus.Remove(effectType);
                break;

            case SkillEffectGameData.EffectType.Skill_Preview_Effect:
                _status.ClearStatusFlag(ObjectBuffStatus.SkillCasting);
                break;

            case SkillEffectGameData.EffectType.Location_Exchange:
                return;

            case SkillEffectGameData.EffectType.Silence:
                _status.ClearStatusFlag(ObjectBuffStatus.Silence);
                break;
            case SkillEffectGameData.EffectType.Stuck:
                _status.ClearStatusFlag(ObjectBuffStatus.Freeze);
                break;
            case SkillEffectGameData.EffectType.Location_Tracking:
                if (parentObject.IsMe)
                    parentObject.StopTracking();
                break;
            case SkillEffectGameData.EffectType.Trap_detect:
                _status.ClearStatusFlag(ObjectBuffStatus.Trap);
                break;
            default:
                //throw new Exception($"Wrong SkillEffectGameData : {buffData.GetID()}");
                SBDebug.LogWarning($"없는 이펙트 타입입니다. 만들어야됨!: {buffData.GetID()}");
                break;
        }

        if (buffEffectObjects.ContainsKey(buffID))
        {
            var oldEffectObject = buffEffectObjects[buffID];
            UnityEngine.Object.Destroy(oldEffectObject);
            buffEffectObjects.Remove(buffID);
        }
    }

    public void DeleteShield()
    {
        if (shieldBuffID > 0)
        {
            DeleteBuff(shieldBuffID);
        }
    }
}
