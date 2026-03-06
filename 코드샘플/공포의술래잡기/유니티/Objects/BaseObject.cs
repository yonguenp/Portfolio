using SBSocketSharedLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : MonoBehaviour
{
    protected Vector3Int prevPos = new Vector3Int(int.MaxValue, int.MaxValue, 0);
    //protected long eventStartTime = 0;
    protected bool _updated = false;
    protected Vector3 _diffPos = Vector3.zero;
    protected float _diffDis = 0;
    protected Animator _animator = null;
    protected SpriteRenderer _sprite = null;

    public GameObjectType GameObjectType { get; set; }
    public string Id { get; private set; }
    public virtual StatInfo Stat { get; protected set; }
    public float Speed { get { return Stat.MoveSpeed; } set { Stat.MoveSpeed = value; } }
    public virtual int Hp { get { return Stat.Hp; } }
    public PositionInfo PosInfo { get; private set; }
    public MoveStatus MoveStatus { get { return (MoveStatus)PosInfo.MoveStatus; } }

    public bool IsShow { get; set; } = true;

    public Vector2 CellPos
    {
        get { return new Vector2(PosInfo.Pos.X, PosInfo.Pos.Y); }
        private set { if (PosInfo.Pos.X != value.x || PosInfo.Pos.Y != value.y) { PosInfo.Pos = new Vec2Float(value.x, value.y); _updated = true; } }
    }
    public Vector2 Dir
    {
        get { return new Vector2(PosInfo.MoveDir.X, PosInfo.MoveDir.Y); }
        private set { if (PosInfo.MoveDir.X != value.x || PosInfo.MoveDir.Y != value.y) { PosInfo.MoveDir = new Vec2Float(value.x, value.y); UpdateAnimation(); _updated = true; } }
    }
    public virtual CreatureStatus State
    {
        get { return PosInfo == null ? CreatureStatus.None : (CreatureStatus)PosInfo.Status; }
        private set { if ((CreatureStatus)PosInfo.Status != value) PosInfo.Status = (byte)value; UpdateAnimation(); UpdateState(); _updated = true; }
    }

    protected BaseObject()
    {
        PosInfo = new PositionInfo();
        Stat = new StatInfo();
    }

    public virtual void SetBaseData(string id, PositionInfo pos = null, StatInfo stat = null)
    {
        Id = id;

        if (pos != null)
            SetPosInfo(pos);
        if (stat != null)
            SetStatsInfo(stat);
    }

    public void SetPosInfo(PositionInfo info)
    {
        if (info == null) return;
        PosInfo = info;

        CellPos = new Vector2(info.Pos.X, info.Pos.Y);
        State = (CreatureStatus)info.Status;
        Dir = new Vector2(info.MoveDir.X, info.MoveDir.Y);

        ApplyPos(info.Pos.X, info.Pos.Y);

        prevPos = new Vector3Int((int)info.Pos.X, (int)info.Pos.Y, 0);
    }

    public void SetStatsInfo(StatInfo info)
    {
        Stat = info;
        if (this as CharacterObject != null)
        {
            (this as CharacterObject).SetSpeed(Stat.MoveSpeed);
        }

        UpdateAnimation();
        _updated = true;
    }

    public void SetState(CreatureStatus state)
    {
        State = state;
    }

    public void SyncPos()
    {
        if (PosInfo != null)
        {
            if (PosInfo.Pos.X == 0 && PosInfo.Pos.Y == 0) return;
            SetPosition(new Vector3(PosInfo.Pos.X, PosInfo.Pos.Y));
        }
    }


    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateController();
        UpdateAfter();
    }

    public virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        SyncPos();

        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureStatus.Idle:
                UpdateIdle();
                break;
            case CreatureStatus.Moving:
                UpdateMoving();
                break;
            case CreatureStatus.Dead:
                UpdateDead();
                break;
        }
    }

    // 스르륵 이동하는 것을 처리
    protected virtual void UpdateMoving()
    {
        if ((CreatureStatus)PosInfo.Status == CreatureStatus.Moving)
        {
            //var speed = PosInfo.MoveStatus == (byte)MoveStatus.Run ? _runSPeed : _normalSpeed;
            var speed = Stat.MoveSpeed;

            float nextX = PosInfo.Pos.X + PosInfo.MoveDir.X * speed * Time.deltaTime;
            float nextY = PosInfo.Pos.Y + PosInfo.MoveDir.Y * speed * Time.deltaTime;

            //float finalX = PosInfo.Pos.X;
            //float finalY = PosInfo.Pos.Y;

            // var nextPosInt = new Vector3Int((int)nextX, (int)nextY, 0);
            // if (nextPosInt.x != prevPos.x ||
            //     nextPosInt.y != prevPos.y)
            // {

            // }

            var addPos = Vector3.zero;
            if (_diffDis != 0)
            {
                addPos = _diffPos * 0.2f;
                var addDis = addPos.magnitude;
                _diffDis -= addDis;
                if (_diffDis < 0) _diffDis = 0;

                //SBDebug.Log($"_diffDis {_diffDis} / addPos[{addPos}]");
            }

            PosInfo.Pos = new Vec2Float(nextX + addPos.x, nextY + addPos.y);
            SetPosition(new Vector3(nextX, nextY));
        }
    }

    public virtual void PlayInvisible(bool force = false, float time = 0f) { }

    public virtual void StopInvisible() { }

    public virtual void BlockSight() { }

    public virtual void UnblockSight() { }

    public virtual void Jump(int tileCount, int playMiliseconds) { }

    public void ApplyPos(float x, float y)
    {
        if (State == CreatureStatus.Hiding)
            return;

        if (PosInfo != null)
            PosInfo.Pos = new Vec2Float(x, y);

        SyncPos();
    }

    public virtual void MoveStart(CreatureStatus state, MoveStatus moveState, float moveDirX, float moveDirY, float posX, float posY, bool isForcePos)
    {
        if (moveState == MoveStatus.Teleport)
        {
            SBDebug.Log("MoveStart MoveState.Teleport");
            CharacterObject charObj = this as CharacterObject;
            if (charObj != null)
            {
                Managers.Effect.PlayEffect(19, charObj.RootEffect);
            }

            ApplyPos(posX, posY);
        }
        else if (moveState == MoveStatus.Jump || moveState == MoveStatus.Pluck || moveState == MoveStatus.Knockback)
        {
            SBDebug.Log("MoveStart MoveState.Jump");
            // var moveVec = new Vector2(posX - PosInfo.Pos.X, posY - PosInfo.Pos.Y);
            // moveVec = moveVec.normalized;
            //PosInfo.MoveDir = new Vec2Float(moveVec.x, moveVec.y);
            PosInfo.Pos = new Vec2Float(posX, posY);
        }
        else if (moveState == MoveStatus.Vent)
        {
            SBDebug.Log("MoveStart MoveState.Vent");
            PosInfo.Pos = new Vec2Float(posX, posY);
            ApplyPos(posX, posY);
            isForcePos = false;
        }
        else
        {
            PosInfo.Status = (byte)state;
            PosInfo.MoveStatus = (byte)moveState;
        }

        if (isForcePos)
        {
            ApplyPos(posX, posY);
        }
    }

    public virtual void MoveEnd(CreatureStatus state, MoveStatus moveState,
        float moveDirX, float moveDirY, float posX, float posY, bool isForcePos)
    {
        PosInfo.Status = (byte)CreatureStatus.Idle;
        PosInfo.MoveStatus = (byte)MoveStatus.None;
        // float subX = posX - PosInfo.Pos.X;
        // float subY = posY - PosInfo.Pos.Y;
        // var subVec = new Vector2(subX, subY);
        // float diffDistance = subVec.magnitude;
        //SBDebug.Log($"MoveEnd Diff Pos Distance {diffDistance}");
        if (isForcePos)
            ApplyPos(posX, posY);
        else
        {
            _diffPos = new Vector3(posX - PosInfo.Pos.X, posY - PosInfo.Pos.Y);
            _diffDis = _diffPos.magnitude;
            //SBDebug.Log($"dif Pos {_diffPos}, dis Dis {_diffDis}");
        }
    }

    protected virtual void UpdateIdle() { }

    protected virtual void MoveToNextPos() { }

    protected virtual void UpdateDead() { }

    public virtual void UpdateAnimation() { }

    public virtual void UpdateState() { }

    public virtual void OnDamage() { }

    public virtual void ShowRenderer(bool isShow)
    {
        this.IsShow = isShow;
    }

    public virtual void Release() { }

    protected virtual void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    protected virtual void UpdateAfter()
    {

    }

    public Vector2 GetOneBaseDir(Vector2 v)
    {
        float x = 0;
        float y = 0;
        if (v.x < 0) x = -1;
        else if (v.x > 0) x = 1;

        if (v.y < 0) y = -1;
        else if (v.y > 0) y = 1;

        return new Vector2(x, y);
    }

}
