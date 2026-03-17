using SBSocketSharedLib;
using UnityEngine;

public partial class CharacterObject
{
    protected float _acc = 0;
    public override void MoveStart(CreatureStatus state, MoveStatus moveState, float moveDirX, float moveDirY, float posX, float posY, bool isForcePos)
    {
        //차후 각도에 대한 라디안 값 캐싱으로 삭제할 수 있다.
        if (float.IsNaN(moveDirX))
            moveDirX = 0;
        if (float.IsNaN(moveDirY))
            moveDirY = 0;

        var prevMoveDir = PosInfo.MoveDir;
        PosInfo.MoveDir = new Vec2Float(moveDirX, moveDirY);
        if ((CreatureStatus)PosInfo.Status == CreatureStatus.Moving && moveState != MoveStatus.Jump && moveState != MoveStatus.Knockback)
        {
            _diffPos = new Vector3(posX - PosInfo.Pos.X, posY - PosInfo.Pos.Y);
            _diffDis = _diffPos.magnitude;
        }

        if (moveState != MoveStatus.Run)
            _characterRenderer.StopMoveDust();
        base.MoveStart(state, moveState, moveDirX, moveDirY, posX, posY, isForcePos);

        if (moveState == MoveStatus.Teleport)
        {
            PosInfo.MoveDir = prevMoveDir;
            _diffPos = Vector3.zero;
            _diffDis = 0;
            moveAfterEvent?.Invoke();
        }
        else if (moveState == MoveStatus.Vent)
        {
            _diffPos = Vector3.zero;
            _diffDis = 0;
            moveAfterEvent?.Invoke();

            if (IsChaser)
            {
                if (PosInfo.Status == (byte)CreatureStatus.Idle)
                {
                    _characterRenderer.PlayVentUpAnimation(moveDirX, moveDirY);
                    soundController.Play("effect/EF_VENT_IN_OUT", SoundController.PlayType.Broadcast);
                }
            }
        }
        else if (moveState == MoveStatus.Pluck)
        {
            _diffPos = Vector3.zero;
            _diffDis = 0;
            PosInfo.MoveDir = prevMoveDir;

            moveAfterEvent?.Invoke();
        }
        else
        {
            if (IsVehicle == true)
                _characterRenderer.SetAnimationVehicle(characterVehicleData.Index, moveDirX, moveDirY, state, moveState, true);
            else
                _characterRenderer.SetAnimation(moveDirX, moveDirY, state, moveState, true, metamorphosis);
        }

        // if (IsMe)
        //     SBDebug.Log($"MoveStart state {state}, moveState {moveState}, moveDirX {moveDirX}, moveDirY {moveDirY}, posX {posX}, posY {posY}");

        //if (GameManager.Instance.DEBUG)
        //{
        //    SetHudPos2(new Vector2(posX, posY));
        //}
    }

    public override void MoveEnd(CreatureStatus state, MoveStatus moveState, float moveDirX, float moveDirY, float posX, float posY, bool isForcePos)
    {
        // if (IsMe)
        //     SBDebug.Log($"==========moveend state {(CreatureStatus)state}, movestate {(MoveStatus)moveState}, movedir({moveDirX}, {moveDirY}), pos ({posX}, {posY})");

        base.MoveEnd(state, moveState, moveDirX, moveDirY, posX, posY, isForcePos);
        if (IsVehicle == false)
            _characterRenderer.SetAnimation(moveDirX, moveDirY, state, moveState, true);
        //SBDebug.Log($"MoveEnd state {state}, moveState {moveState}, moveDirX {moveDirX}, moveDirY {moveDirY}, posX {posX}, posY {posY}");
        //if (GameManager.Instance.DEBUG)
        //{
        //    SetHudPos2(new Vector2(posX, posY));
        //}

        if (moveState == MoveStatus.Vent)
        {
            soundController.Play("effect/EF_VENT_IN_OUT", SoundController.PlayType.Broadcast);
        }

        _acc = 0;
    }

    public bool CanMove(float dirX, float dirY)
    {
        var speed = _normalSpeed;

        float nextX = PosInfo.Pos.X + dirX * speed * Time.deltaTime;
        float nextY = PosInfo.Pos.Y + dirY * speed * Time.deltaTime;

        var nextPosInt = new Vector3Int((int)nextX, (int)nextY, 0);
        if (nextPosInt.x != prevPos.x ||
            nextPosInt.y != prevPos.y)
        {
            if (Managers.Object.CanGoObject(nextX, nextY) == false)
            {
                Game.Instance.PlayerController.OnMove(Vector2.zero);
                return false;
            }
        }

        return true;
    }

    bool CanMoveWithTile(int x, int y)
    {
        var tileInfo = Game.Instance.GetTileInfo(x, y);
        if (IsChaser)
        {
            if (tileInfo != 0 && tileInfo != 3) return true;
        }
        else
        {
            if (tileInfo != 0 && tileInfo != 2) return true;
        }

        return false;
    }

    float _radius = 0.3f;

    protected override void UpdateMoving()
    {
        if (!IsMovable) return;
        if (CheckStatus(ObjectBuffStatus.Freeze) || CheckStatus(ObjectBuffStatus.Pluck))
        {
            return;
        }

        if ((CreatureStatus)PosInfo.Status == CreatureStatus.Moving)
        {
            var speed = _normalSpeed;
            if (IsVehicle)
            {
                speed = characterVehicleData.Speed;
            }

            var oneDir = GetOneBaseDir(new Vector2(PosInfo.MoveDir.X, PosInfo.MoveDir.Y));

            var moveDis = speed * Time.deltaTime * _acc;
            float nextX = PosInfo.Pos.X + PosInfo.MoveDir.X * moveDis;
            float nextY = PosInfo.Pos.Y + PosInfo.MoveDir.Y * moveDis;
            float nextXOneDir = PosInfo.Pos.X + oneDir.x * moveDis;
            float nextYOneDir = PosInfo.Pos.Y + oneDir.y * moveDis;
            //SBDebug.Log($"acc ({_acc}), moveDis ({moveDis})");
            _acc += Time.deltaTime * 20f;
            if (_acc > 1)
                _acc = 1;

            bool canMoveX = false;
            bool canMoveY = false;

            float fixedNextX = nextX;
            float fixedNextY = nextY;

            var dir = new Vector2(PosInfo.MoveDir.X, PosInfo.MoveDir.Y);
            var dirNor = dir * _radius;


            canMoveX = CanMoveWithTile((int)(nextXOneDir + dirNor.x), prevPos.y);
            if (canMoveX == false)
            {
                fixedNextX = PosInfo.Pos.X;
            }

            canMoveY = CanMoveWithTile(prevPos.x, (int)(nextYOneDir + dirNor.y));
            if (canMoveY == false)
            {
                fixedNextY = PosInfo.Pos.Y;
            }

            bool isNextTile = false;

            var nextPosInt = new Vector3Int((int)fixedNextX, (int)fixedNextY, 0);
            if (nextPosInt.x != prevPos.x ||
                nextPosInt.y != prevPos.y)
            {
                isNextTile = true;
                prevPos = nextPosInt;
            }

            PosInfo.Pos = new Vec2Float(fixedNextX, fixedNextY);
            transform.position = new Vector3(fixedNextX, fixedNextY);
            if (isNextTile) moveAfterEvent.Invoke();
            //SBDebug.Log($"MoveUpdate {finalPos}");
            //if (isClone) CheckOwnerIsInvisible();
        }
    }

    int CurX = 0;
    int CurY = 0;

    protected override void UpdateAfter()
    {
        var addPos = Vector3.zero;
        if (_diffDis != 0)
        {
            addPos = _diffPos * (Time.deltaTime * 5);
            var addDis = addPos.magnitude;
            _diffDis -= addDis;
            if (_diffDis < 0)
            {
                _diffDis = 0;
                return;
            }
            //SBDebug.Log($"_diffDis {_diffDis} / addPos[{addPos}]");

            var nextPos = new Vec2Float(transform.position.x + addPos.x, transform.position.y + addPos.y);
            PosInfo.Pos = nextPos;
            transform.position += addPos;

            // var oneDir = GetOneBaseDir(new Vector2(PosInfo.MoveDir.X, PosInfo.MoveDir.Y));

            // float nextX = PosInfo.Pos.X + addPos.x;
            // float nextY = PosInfo.Pos.Y + addPos.y;

            // bool canMoveX = false;
            // bool canMoveY = false;

            // float fixedNextX = nextX;
            // float fixedNextY = nextY;

            // var dir = new Vector2(PosInfo.MoveDir.X, PosInfo.MoveDir.Y);
            // var dirCol = dir * _radius;


            // canMoveX = CanMoveWithTile((int)(nextX + dirCol.x), prevPos.y);
            // if(canMoveX == false)
            // {
            //     Debug.Log("cant move x");
            //     fixedNextX = PosInfo.Pos.X;
            // }

            // canMoveY = CanMoveWithTile(prevPos.x,(int)(nextY + dirCol.y));
            // if(canMoveY == false)
            // {
            //     Debug.Log("cant move y");
            //     fixedNextY = PosInfo.Pos.Y;
            // }

            // var nextPosInt = new Vector3Int((int)fixedNextX, (int)fixedNextY, 0);
            // if (nextPosInt.x != prevPos.x ||
            //     nextPosInt.y != prevPos.y)
            // {
            //     prevPos = nextPosInt;
            // }

            // PosInfo.Pos = new Vec2Float(fixedNextX, fixedNextY);
            // transform.position = new Vector3(fixedNextX, fixedNextY);
        }
    }
}
