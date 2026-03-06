using SBSocketSharedLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterObject
{
    public bool IsPlaying { get; private set; } = true;
    // 상태이상 체크 존
    public bool IsConfused { get { return CheckStatus(ObjectBuffStatus.Confuse); } }
    public bool IsInvisible { get { return CheckStatus(ObjectBuffStatus.Invisible); } }
    public bool IsSightBlocked { get { return CheckStatus(ObjectBuffStatus.BlockSight); } }
    public bool IsMovable { get { return !CheckStatus(ObjectBuffStatus.Freeze) && !CheckStatus(ObjectBuffStatus.Stun) && !CheckStatus(ObjectBuffStatus.SkillCasting); } }
    public bool IsSkillAvailable { get { return !CheckStatus(ObjectBuffStatus.Stun) && !CheckStatus(ObjectBuffStatus.SkillCasting); } }

    public bool IsActiveSkillAvailable { get { return IsSkillAvailable && !CheckStatus(ObjectBuffStatus.Silence); } }
    
    public bool IsDetectTrap { get { return CheckStatus(ObjectBuffStatus.Trap); } }
    // 상태이상 체크 존


    public void AddBuffs(IList<int> buffIDs)
    {
        foreach (var id in buffIDs)
        {
            _buff.CreateBuff(id);
            if (id >= 20210102 && id <= 20210602)
            {
                metamorphosis = true;
                _characterRenderer.SetAddAnimation(PosInfo.MoveDir.X, PosInfo.MoveDir.Y);
            }
        }
    }

    public void DeleteBuffs(IList<int> buffIDs)
    {
        foreach (var id in buffIDs)
        {
            _buff.DeleteBuff(id);
            if (id >= 20210102 && id <= 20210602)
            {
                if (metamorphosis == true)
                {
                    metamorphosis = false;
                    _characterRenderer.ClearTrack(2);
                }
            }
        }
    }

    private void SetMovable()
    {
        _buff.BuffStatus.ClearStatusFlag(ObjectBuffStatus.Freeze);
    }

    private void SetImmovable()
    {
        _buff.BuffStatus.SetStatusFlag(ObjectBuffStatus.Freeze);
    }

    public void OnStun(float time)
    {
        _characterRenderer.PlayStunAnimation(PosInfo.MoveDir.X, PosInfo.MoveDir.Y, IsChaser);
        OnCancelSkillCasting();
        StartCoroutine(PlayStunCoroutine(time));

        if (IsMe)
        {
            Game.Instance.PlayerController.OnMove(Vector2.zero);
            Game.Instance.PlayerController.RemoveSkillRangeGuideUI();
        }
    }

    IEnumerator PlayStunCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        UpdateAnimation();
    }

    public void DisconnectCharacter()
    {
        IsPlaying = false;
        PlayInvisible(true, 0f);
    }

    public void IsReconnectCharacter()
    {
        IsPlaying = true;
        StopInvisible();
    }
    public override void PlayInvisible(bool force = false, float time = 0f)
    {
        if (IsInvisible)
        {
            float alphaValue = 0;
            if (!force)
            {
                // 아군에게는 보여야 한다
                if (IsFriend)
                {
                    alphaValue = GameConfig.Instance.SKILL_TRANSPARENT_ALPHA;
                }
            }
            SetColor(new Color(1, 1, 1, alphaValue));
        }

        if (IsMe)
        {
            Game.Instance.PlayerController.OnSetSkillRemainTime(time);
        }
    }

    public override void StopInvisible()
    {
        // 숨은 상태일때는 컬러를 바꿔주지 말자.
        if (!IsInvisible && State != CreatureStatus.Hiding)
        {
            SetColor(Color.white);
        }

        if (IsMe)
        {
            Game.Instance.PlayerController.OnResetSkillRemainTime();
        }
    }

    public override void BlockSight()
    {
        if (Game.Instance.PlayerController.Character == this)
        {
            Game.Instance.UIGame.SetSightBlockStatus(true);
        }
    }

    public override void UnblockSight()
    {
        if (Game.Instance.PlayerController.Character == this)
        {
            Game.Instance.UIGame.SetSightBlockStatus(false);
        }
    }

    public void Rush(int tileCount, float seconds)
    {
        StartCoroutine(RushActionCoroutine(tileCount, seconds));
    }

    IEnumerator RushActionCoroutine(int tileCount, float seconds)
    {
        if (seconds <= 0f) { seconds = 0.5f; }
        float tile = tileCount * 0.001f;
        float playTime = 0;
        var startPos = transform.position;
        var curStatus = (CreatureStatus)PosInfo.Status;

        var targetPos = new Vector3(PosInfo.Pos.X, PosInfo.Pos.Y, 0);
        var dir = targetPos - transform.position;
        var dirDis = dir.magnitude;
        var dirNor = dir.normalized;
        var minDis = 0.2f * 0.2f;

        tile = dirDis;
        var speed = tile / seconds;

        SBDebug.Log($"target pos ({PosInfo.Pos.X}, {PosInfo.Pos.Y})");

        // 이펙트 애니메이션 방향을 위해 잠시 캐릭터의 이동 방향을 돌진 방향으로 바꾼다
        var cachedDir = PosInfo.MoveDir;
        PosInfo.MoveDir = new Vec2Float(dirNor.x, dirNor.y);

        PosInfo.Status = (byte)CreatureStatus.Moving;
        UpdateAnimation();

        while (true)
        {
            if (seconds < playTime) { break; }
            if (IsMovable) { break; }
            if(State == CreatureStatus.Hiding) { break; }
            if (State == CreatureStatus.Groggy) { yield break; }

            playTime += Time.deltaTime;

            var nextPos = startPos + (dirNor * speed * playTime);
            var nextPosInt = new Vector3Int((int)nextPos.x, (int)nextPos.y, 0);
            if (nextPosInt.x != prevPos.x ||
                nextPosInt.y != prevPos.y)
            {
                prevPos = nextPosInt;
            }
            transform.position = new Vector3(nextPos.x, nextPos.y);

            var subPos = targetPos - transform.position;
            if (minDis > subPos.sqrMagnitude)
                break;

            yield return null;
        }
        //if (seconds > playTime)
        //    yield return new WaitForSeconds(seconds - playTime);

        // 위에서 바꿨던 캐릭터의 이동 방향 복구
        PosInfo.MoveDir = cachedDir;
        PosInfo.Status = (byte)curStatus;

        _characterRenderer.ResetAttackAnim();

        //UpdateAnimation();
    }

    public override void Jump(int tileCount, int playMiliseconds)
    {
        StartCoroutine(JumpActionCoroutine(tileCount, playMiliseconds));
    }

    IEnumerator JumpActionCoroutine(int tileCount, int playMiliseconds)
    {
        float tile = tileCount * 0.001f;
        var speed = tile / (playMiliseconds * 0.001f);
        float playTime = 0;
        var startPos = transform.position;
        var curStatus = (CreatureStatus)PosInfo.Status;

        PosInfo.Status = (byte)CreatureStatus.Moving;
        UpdateAnimation();
        while (true)
        {
            if (playMiliseconds < (playTime * 1000f)) { break; }
            if (IsMovable) { break; }
            if (State == CreatureStatus.Groggy) { yield break; }

            var nextPos = startPos + (new Vector3(PosInfo.MoveDir.X, PosInfo.MoveDir.Y) * speed * Time.deltaTime);
            var nextPosInt = new Vector3Int((int)nextPos.x, (int)nextPos.y, 0);
            if (nextPosInt.x != prevPos.x ||
                nextPosInt.y != prevPos.y)
            {
                prevPos = nextPosInt;
            }

            var timeRate = playTime / playMiliseconds;
            float r = Mathf.PI / 180.0f;
            float radian = 180.0f * timeRate * r;
            float value = Mathf.Sin(radian);
            if (float.IsNaN(value)) value = 0;
            startPos = nextPos;
            transform.position = startPos + new Vector3(0, value);
            playTime += Time.deltaTime;

            yield return null;
        }

        if (curStatus == CreatureStatus.Idle)
        {
            PosInfo.Status = (byte)curStatus;
            UpdateAnimation();
        }
    }

    bool CheckStatus(ObjectBuffStatus type)
    {
        try
        {
            return (_buff.BuffStatus.CurrentStatus & type) == type;
        }
        catch
        {
            return false;
        }
    }

    public void ChangeSkill(int oldID, int newID)
    {
        SBDebug.Log(CharacterType);
        var charData = CharacterGameData.GetCharacterData(CharacterType);
        charData.ChangeSkillData(oldID, newID);
        if (IsMe)
        {
            Game.Instance.PlayerController.ChangeSkillData(oldID, newID);
        }
    }

    public void UnlockCoolTime(int skillID)
    {
        if (IsMe)
        {
            Game.Instance.PlayerController.UnlockCoolTime(skillID);
        }
    }

    public void ReduceCoolTime(int skillID, float seconds)
    {
        if (IsMe)
        {
            Game.Instance.PlayerController.ReduceCurrentCoolTime(skillID, seconds);
        }
    }

    public void IncreaseCoolTime(int skillID, float seconds)
    {
        if (IsMe)
        {
            Game.Instance.PlayerController.IncreaseCurrentCoolTime(skillID, seconds);
        }
    }

    public void PlayPluck(SkillEffectGameData skillEffectGameData)
    {
        StartCoroutine(PluckActionCoroutine(skillEffectGameData));
    }

    IEnumerator PluckActionCoroutine(SkillEffectGameData skillEffectGameData)
    {
        var moveTile = skillEffectGameData.Value1 / 1000;
        var tileBaseMoveSpeed = skillEffectGameData.ActiveTime / moveTile;
        var curVec = new Vector2(transform.position.x, transform.position.y);
        var targetPos = new Vector2(PosInfo.Pos.X, PosInfo.Pos.Y);
        var disVec = targetPos - curVec;
        var disNor = disVec.normalized;
        var distance = disVec.magnitude;
        var moveFrameVec = disNor / tileBaseMoveSpeed;
        var timeRate = distance / moveTile;
        var limitTIme = skillEffectGameData.ActiveTime * timeRate;

        float playTime = 0;

        SBDebug.Log($"PluckActionCoroutine targetPos {targetPos}, tileBaseMoveSpeed {tileBaseMoveSpeed}, curVec : {curVec}, distance {disVec}");
        while (true)
        {
            if (playTime >= limitTIme)
            {
                transform.position = targetPos;
                yield break;
            }
            else
            {
                transform.position = transform.position + (new Vector3(moveFrameVec.x, moveFrameVec.y) * Time.deltaTime);
                playTime += Time.deltaTime;
            }

            yield return null;
        }
    }

    public void PlayTracking(SkillEffectGameData skillEffectGameData)
    {
        var playTime = skillEffectGameData.ActiveTime;

        var characterList = Managers.Object.Objects;

        foreach (var c in characterList)
        {
            var value = c.Value;
            if (value == null) continue;
            CharacterObject co = c.Value.GetComponent<CharacterObject>();
            if (co == null) continue;
            if (co.IsFriend == false)
                Game.Instance.HudNode.CreateHudDetect(co, playTime);
        }
    }

    public void StopTracking()
    {
        Game.Instance.HudNode.ClearHudDetect();
    }
}
