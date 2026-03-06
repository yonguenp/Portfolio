//using SBSocketSharedLib;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//public class PosRender : BaseObject
//{
//    public CharacterObject _character = null;

//    SpriteRenderer model;
//    private void Start()
//    {
//        model = GetComponent<SpriteRenderer>();
//        SetColor(new Color(0, 0, 255, 128));
//    }

//    public void SetColor(Color color)
//    {
//        model.color = color;
//    }

//    float _normalSpeed = 0;
//    float _runSPeed = 0;

//    public void SetSpeed(float norSpeed, float runSpeed)
//    {
//        _normalSpeed = norSpeed;
//        _runSPeed = runSpeed;
//    }

//    protected override void UpdateController()
//    {
//        switch (State)
//        {
//            case CreatureStatus.Idle:
//                UpdateIdle();
//                break;
//            case CreatureStatus.Moving:
//                UpdateMoving();
//                ApplyDiffPos();
//                break;
//            case CreatureStatus.Skill:
//                UpdateSkill();
//                break;
//            case CreatureStatus.Dead:
//                UpdateDead();
//                break;
//        }
//    }

//    private void ApplyDiffPos()
//    {

//    }

//    protected override void UpdateMoving()
//    {
//        if (IsMovable == false) return;

//        if ((CreatureStatus)PosInfo.Status == CreatureStatus.Moving)
//        {
//            var speed = PosInfo.MoveStatus == (byte)MoveStatus.Run ? _runSPeed : _normalSpeed;

//            float nextX = PosInfo.Pos.X + PosInfo.MoveDir.X * speed * Time.deltaTime;
//            float nextY = PosInfo.Pos.Y + PosInfo.MoveDir.Y * speed * Time.deltaTime;

//            float finalX = PosInfo.Pos.X;
//            float finalY = PosInfo.Pos.Y;

//            var nextPosInt = new Vector3Int((int)nextX, (int)nextY, 0);
//            if (nextPosInt.x != prevPos.x ||
//                nextPosInt.y != prevPos.y)
//            {
//                bool isCanMoveX = Managers.Map.CanGo(new Vector3Int((int)nextX, (int)finalY, 0));
//                if (isCanMoveX)
//                {
//                    finalX = nextX;
//                    prevPos.x = nextPosInt.x;
//                }
//                bool isCanMoveY = Managers.Map.CanGo(new Vector3Int((int)finalX, (int)nextY, 0));
//                if (isCanMoveY)
//                {
//                    finalY = nextY;
//                    prevPos.y = nextPosInt.y;
//                }

//                if (Managers.Object.CanGoObject(nextX, nextY) == false)
//                {
//                    Game.Instance.PlayerController.OnMove(Vector2.zero);
//                    return;
//                }
//            }
//            else
//            {
//                finalX = nextX;
//                finalY = nextY;
//            }

//            var addPos = Vector3.zero;
//            //if (_diffDis != 0)
//            //{
//            //  addPos = _diffPos * 0.2f;
//            //  var addDis = addPos.magnitude;
//            //  _diffDis -= addDis;
//            //  if (_diffDis < 0) _diffDis = 0;
//            //}

//            var finalPos = new Vector3(finalX, finalY) + addPos;
//            ApplyPos(finalPos.x, finalPos.y);
//        }
//    }

//    public override void MoveStart(CreatureStatus state, MoveStatus moveState, float moveDirX, float moveDirY, float posX, float posY, bool isForcePos)
//    {
//        PosInfo.MoveDir = new Vec2Float(moveDirX, moveDirY);
//        if ((CreatureStatus)PosInfo.Status == CreatureStatus.Moving && moveState != MoveStatus.Jump)
//        {
//            ApplyPos(posX, posY);
//        }

//        if (moveState == MoveStatus.Teleport)
//        {
//            SBDebug.Log("MoveStart MoveState.TELEPORT");
//            GameObject effect = Managers.Resource.Instantiate("Particle/teleport");
//            effect.transform.position = new Vector3(PosInfo.Pos.X, PosInfo.Pos.Y, 0);
//            GameObject.Destroy(effect, 2.0f);
//            ApplyPos(posX, posY);
//        }
//        else if (moveState == MoveStatus.Jump)
//        {
//            eventStartTime = DateTime.Now.Ticks;
//            PosInfo.Pos = new Vec2Float(posX, posY);
//            IsMovable = false;
//            StartCoroutine(JumpActionCoroutine());
//        }
//        else
//        {
//            PosInfo.Status = (byte)state;
//            PosInfo.MoveStatus = (byte)moveState;
//        }
//    }

//    public override void MoveEnd(CreatureStatus state, MoveStatus moveState, float moveDirX, float moveDirY, float posX, float posY, bool isForcePos)
//    {
//        PosInfo.Status = (byte)CreatureStatus.Idle;
//        PosInfo.MoveStatus = (byte)MoveStatus.None;
//        //float subX = posX - PosInfo.Pos.X;
//        //float subY = posY - PosInfo.Pos.Y;
//        //var subVec = new Vector2(subX, subY);
//        //float diffDistance = subVec.sqrMagnitude;
//        //SBDebug.Log($"MoveEnd Diff Pos Distance {diffDistance}");
//        //if (diffDistance > 0.2f)
//        ApplyPos(posX, posY);
//    }
//}
