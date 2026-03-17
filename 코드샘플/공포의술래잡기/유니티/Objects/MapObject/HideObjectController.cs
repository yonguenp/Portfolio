using SBCommonLib;
using System.Collections;
using UnityEngine;

public class HideObjectController : PropController
{
    enum eHideObjectType
    {
        None,
        Box,
        Trash,
        Cabinet,
    }

    Coroutine IdleAnimationCoritine = null;
    eHideObjectType _hideObjectType = eHideObjectType.None;
    ObjectHideGameData _hideObjectData = null;

    private bool isCharacter = false;

    public override void Init()
    {
        base.Init();

        animState.SetAnimation(0, "f_idle_0", false);
        if (IdleAnimationCoritine != null)
        {
            StopCoroutine(IdleAnimationCoritine);
        }
        IdleAnimationCoritine = StartCoroutine(UpdateOneSecCO(((Game.Instance.GameRoom.GameStartTimestamp + ObjectId) % 50) * 0.1f));
        _hideObjectData = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_hide, ObjData.sub_obj_uid) as ObjectHideGameData;
        _hideObjectType = (eHideObjectType)_hideObjectData.hideObjectType;
    }

    public void SetInTheCharacter(bool isCharacter)
    {
        this.isCharacter = isCharacter;
    }

    IEnumerator UpdateOneSecCO(float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);            //주준형:: 상자 스폰 시 랜덤5초 각각의 상자 딜레이
        float idleAnimationTIme = GameConfig.Instance.HIDE_OBJECT_DELAY_TIME; //주준형:: 나중에 기획팀 설정에 맞춰서 할 수 있도록 설정
        
        while (true)
        {
            if (isCharacter)
                yield return new WaitForSeconds(3f);                    //주준형:: 하이딩되어있는 오브젝트이면 2초

            float playTime = 0.0f;
            if(!isCharacter)
            {
                while (playTime < idleAnimationTIme)
                {
                    playTime += Time.deltaTime;
                    yield return new WaitForEndOfFrame();

                    if (isCharacter)
                        break;
                }
            }

            if (IsBroken)
                yield break;

            animState.SetAnimation(0, "f_idle_1", false);
        }
    }

    public override void OnDamage()
    {
        if (animState == null) return;

        StopCoroutine(IdleAnimationCoritine);

        // 한 번 흔들고 부서지는것을 보여준다
        animState.SetAnimation(0, "f_broken_0", false);
        animState.AddAnimation(0, "f_broken_1", false, 0f);
        IsBroken = true;

        soundController.Play(_hideObjectData.DestroySoundPath);
        Managers.Effect.PlayEffect(_hideObjectData.DestroyEffectId, transform);
        SetInTheCharacter(false);
    }

    public override void OnDespawn()
    {

    }

    public override void OnRespawn()
    {
        IsBroken = false;

        animState.SetAnimation(0, "f_idle_0", false);
        if (IdleAnimationCoritine != null)
        {
            StopCoroutine(IdleAnimationCoritine);
        }
        IdleAnimationCoritine = StartCoroutine(UpdateOneSecCO());
    }

    public override void SetRespawnTimeOnReconnect(long regenTime)
    {
        var localTime = SBUtil.GetCurrentMilliSecTimestamp();
        // 리젠이 미래에 되는 상황이면 이건 부서진 오브젝트이다
        if (regenTime > localTime)
        {
            IsBroken = true;
            animState.SetAnimation(0, "f_broken_1", false);
        }
    }
}
