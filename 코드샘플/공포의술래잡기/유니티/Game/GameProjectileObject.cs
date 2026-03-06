using SBSocketSharedLib;
using Spine.Unity;
using System.Collections;
using UnityEngine;

public partial class Game
{
    private GameObject CloneSpawn(CharacterObject source, float dirX, float dirY, float posX, float posY)
    {
        var go = CharacterGameData.GetCharacterData(source.CharacterType).LoadCharacterObject();
        var clone = go.GetComponent<CharacterObject>();
        clone.SetBaseData(string.Empty,
            new PositionInfo
            {
                MoveDir = new Vec2Float(dirX, dirY),
                MoveStatus = (byte)MoveStatus.Run,
                Pos = new Vec2Float(posX, posY),
                Status = (byte)CreatureStatus.Moving,
            },
            new StatInfo
            {
                MoveSpeed = source.Stat.MoveSpeed,
            });

        clone.SetClone(source);
        clone.SetSource(source.Id);

        StartCoroutine(CloneSpawnCoroutine(clone, dirX, dirY, posX, posY));

        return go;
    }

    private IEnumerator CloneSpawnCoroutine(CharacterObject charObj, float dirX, float dirY, float posX, float posY)
    {
        yield return null;
        charObj.transform.localEulerAngles = Vector3.zero;
        charObj.MoveStart(CreatureStatus.Moving, MoveStatus.Run, dirX, dirY, posX, posY, false);
    }

    public void ProjectileSpawn(SCBcProjectileSpawn packet)
    {
        SkillSummonGameData summonData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill_summon, packet.Skill_SummonId) as SkillSummonGameData;

        SBDebug.Log($"생성된 투사체 ID::{packet.GameObjectId}");

        if (string.IsNullOrEmpty(summonData.ResourcePath))
        {
            SBDebug.LogWarning($"No projectile resource : SkillSummonID {packet.Skill_SummonId}");
        }

        PositionInfo posInfo = new PositionInfo();
        posInfo.MoveDir = packet.SkillDir;
        posInfo.Pos = packet.Position;
        posInfo.Status = (byte)CreatureStatus.Moving;

        var resourcePath = summonData.ResourcePath;
        GameObject go = null;
        var ownerCharacter = Managers.Object.FindCharacterById(packet.OwnerId);

        float addPosY = 0;
        var showType = ProjectileObject.eShowType.All;

        switch (summonData.Type)
        {
            case SkillSummonGameData.SummonType.CharacterClone:
                var charType = ownerCharacter.CharacterType;
                var cloneObject = CloneSpawn(ownerCharacter, packet.SkillDir.X, packet.SkillDir.Y, packet.Position.X, packet.Position.Y);
                var cloneController = cloneObject.AddComponent<ProjectileObject>();
                cloneController.SetBaseData(packet.GameObjectId, posInfo);
                cloneController.Speed = ownerCharacter.Stat.MoveSpeed;
                cloneController.SetOwnerID(packet.OwnerId);
                cloneController.SetSummonData(summonData);
                Managers.Object.AddProjectile(cloneController);
                return;
            case SkillSummonGameData.SummonType.Trap:
            case SkillSummonGameData.SummonType.InvisibleObject:
                {
                    // 22.03.10 기현 : 덫은 같은 진영만 보인다

                    go = Managers.Resource.Instantiate(resourcePath);
                    // 22.03.10 기현 : 용준님 요청, 덫의 경우 리소스 크기 그대로 보여달라고 했음
                    // var s = summonData.RangeDistance * 0.001f;
                    // // 원형이면 "반경"이기 때문에 사이즈 두 배 해줘야 함
                    // if (summonData.RangeType == SkillRangeType.Circle)
                    // {
                    //     s *= 2;
                    // }
                    if (go == null)
                        return;
                    go.transform.localScale = Vector3.one;
                    posInfo.MoveDir = new Vec2Float(0, 0);
                    if (summonData.Type == SkillSummonGameData.SummonType.Trap)
                        showType = ProjectileObject.eShowType.Conditional;
                    else
                        showType = ProjectileObject.eShowType.Friend;

                    Transform model = go.transform.Find("model");
                    if (model != null)
                    {
                        SkeletonAnimation anim = model.GetComponent<SkeletonAnimation>();
                        if (anim != null)
                        {
                            var animData = anim.skeletonDataAsset.GetSkeletonData(false);
                            if (animData != null)
                            {
                                var animationObject = animData.FindAnimation("f_start_0");
                                if (animationObject != null)
                                {
                                    anim.loop = false;
                                    anim.AnimationName = "f_start_0";

                                    anim.AnimationState.Complete += (entry) =>
                                    {
                                        if (entry.Animation.Name == "f_start_0")
                                        {
                                            anim.loop = true;
                                            anim.AnimationName = "f_idle_0";
                                        }
                                    };
                                }
                                else
                                {
                                    animationObject = animData.FindAnimation("f_idle_0");
                                    if (animationObject != null)
                                    {
                                        anim.loop = true;
                                        anim.AnimationName = "f_idle_0";
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            case SkillSummonGameData.SummonType.AreaOfEffect:
            case SkillSummonGameData.SummonType.Block:
                {
                    go = Managers.Resource.Instantiate(resourcePath);
                    // 22.03.10 기현 : 용준님 요청, 덫의 경우 리소스 크기 그대로 보여달라고 했음
                    // var s = summonData.RangeDistance * 0.001f;
                    // // 원형이면 "반경"이기 때문에 사이즈 두 배 해줘야 함
                    // if (summonData.RangeType == SkillRangeType.Circle)
                    // {
                    //     s *= 2;
                    // }
                    if (go == null)
                        return;
                    go.transform.localScale = Vector3.one;

                    // go = new GameObject();
                    // // 더미이펙트
                    // var effect = Managers.Effect.PlayEffect(99, go.transform, summonData.Lifetime).GetComponent<EffectSP>();
                    // var scale = summonData.RangeDistance * 0.001f;
                    // // 원형이면 "반경"이기 때문에 사이즈 두 배 해줘야 함
                    // if (summonData.RangeType == SkillRangeType.Circle)
                    // {
                    //     scale *= 2;
                    // }
                    // go.transform.localScale = new Vector3(scale, scale, 1);
                    // effect.gameObject.transform.localPosition = Vector3.zero;

                    // var sk = effect.SkeletonAnimation;
                    // if (sk != null)
                    // {
                    //     var color = new Color(1f, 0f, 0f, 0.5f);
                    //     sk.skeleton.SetColor(color);
                    // }

                    posInfo.MoveDir = new Vec2Float(0, 0);
                    Transform model = go.transform.Find("model");
                    if (model != null)
                    {
                        SkeletonAnimation anim = model.GetComponent<SkeletonAnimation>();
                        if (anim != null)
                        {
                            var animData = anim.skeletonDataAsset.GetSkeletonData(false);
                            if (animData != null)
                            {
                                var animationObject = animData.FindAnimation("f_start_0");
                                if (animationObject != null)
                                {
                                    anim.loop = false;
                                    anim.AnimationName = "f_start_0";

                                    anim.AnimationState.Complete += (entry) =>
                                    {
                                        if (entry.Animation.Name == "f_start_0")
                                        {
                                            anim.loop = true;
                                            anim.AnimationName = "f_idle_0";
                                        }
                                    };
                                }
                                else
                                {
                                    animationObject = animData.FindAnimation("f_idle_0");
                                    if (animationObject != null)
                                    {
                                        anim.loop = true;
                                        anim.AnimationName = "f_idle_0";
                                    }
                                }
                            }
                        }
                    }
                }
                break;

            case SkillSummonGameData.SummonType.Projectile:
                go = Managers.Resource.Instantiate(resourcePath);
                addPosY = 0.5f;
                break;

            default:
                SBDebug.LogError($"Wrong projectile type : {summonData.Type}");
                break;
        }

        if (go == null) return;

        var controller = go.AddComponent<ProjectileObject>();
        controller.AddPosY = addPosY;
        controller.SetBaseData(packet.GameObjectId, posInfo);
        controller.Speed = summonData.Speed * 0.001f;
        controller.SetOwnerID(packet.OwnerId);
        controller.SetSummonData(summonData);
        controller.Init();

        //showType
        controller.ShowType = showType;
        controller.ShowRenderer(true);

        Managers.Object.AddProjectile(controller);
        return;
    }

    public void ProjectileDespawn(string objectId, Vec2Float despawnPos)
    {
        CharacterObject cloneObject = null;
        SBDebug.Log($"삭제된 투사체 ID::{objectId}");
        BaseObject baseObject = Managers.Object.FindBaseObjectById(objectId);

        if (baseObject == null)
            return;

        cloneObject = baseObject.GetComponent<CharacterObject>();

        if (cloneObject != null)
        {
            cloneObject.SetSpeed(0);
            var projectileObject = cloneObject.GetComponent<ProjectileObject>();
            projectileObject.Speed = 0f;
            float playTime = GameConfig.Instance.CLONE_PROJECTILE_FADEOUT_TIME * 0.001f;
            cloneObject.OnDespawnClone(playTime, projectileObject.summonData);
            Managers.Object.Remove(objectId, playTime);
        }
        else
        {
            float removeDeleyTime = 1.0f;
            bool isRemove = true;
            var projectileObject = baseObject.GetComponent<ProjectileObject>();
            if (projectileObject)
            {
                if (projectileObject.SummonType == SkillSummonGameData.SummonType.InvisibleObject || projectileObject.SummonType == SkillSummonGameData.SummonType.Trap)
                {
                    isRemove = false;
                    projectileObject.SetReadyRemove();
                }
                else if (projectileObject.SummonType == SkillSummonGameData.SummonType.Projectile)
                {
                    projectileObject.SetRemove(despawnPos, projectileObject);
                }
                else if (projectileObject.SummonType == SkillSummonGameData.SummonType.Block)
                {
                    projectileObject.SetRemove();
                }
            }

            if (isRemove) Managers.Object.Remove(objectId, removeDeleyTime);
        }
    }
}
