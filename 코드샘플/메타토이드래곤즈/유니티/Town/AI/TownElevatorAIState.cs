using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork 
{
    public abstract class TownElevatorAIState : StateBase
    {
        protected TownElevatorData data = null;
        protected bool IsData { get { return data != null; } }
        public virtual void Set(TownElevatorData data)
        {
            this.data = data;
        }
        public override bool OnEnter()
        {
            return base.OnEnter() && IsData;
        }
        public override bool OnExit()
        {
            return base.OnExit() && IsData;
        }
        public override bool Update(float dt)
        {
            return base.Update(dt) && IsData;
        }
    }
    public class TownElevatorIdleState : TownElevatorAIState
    {
        private float time = 0f;
        private float delay = 0.2f;
        public override bool OnEnter()
        {
            if(base.OnEnter())
            {
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                time -= dt;
                if (time <= 0f)
                {
                    time = delay;
                    return false;
                }
                return true;
            }
            return false;
        }
    }
    public class TownElevatorOpenState : TownElevatorAIState
    {
        private float time = 0f;
        private float delay = 0f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                time = delay;
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                time = delay;
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                time -= dt;
                return time > 0;
            }
            return false;
        }
    }
    public class TownElevatorOutIdleState : TownElevatorAIState
    {
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return data.IsExitDragon();
            }
            return false;
        }
    }
    public class TownElevatorInIdleState : TownElevatorAIState
    {
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return data.IsInDragon();
            }
            return false;
        }
    }
    public class TownElevatorCloseState : TownElevatorAIState
    {
        private float time = 0f;
        private float delay = 0f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                time = delay;
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                time = delay;
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                time -= dt;
                return time > 0;
            }
            return false;
        }
    }
    public class TownElevatorMoveUpState : TownElevatorAIState
    {
        public Vector3 targetPos = Vector3.zero;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                targetPos = Vector3.zero;
                targetPos.y = TownMap.GetElevatorContainerPosY(data.CurFloor + 1);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                targetPos = Vector3.zero;
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                targetPos.x = data.Elevator.transform.localPosition.x;
                targetPos.z = data.Elevator.transform.localPosition.z;
                if (data.Controller.UpdateLocalTarget(dt, targetPos, 0, false, 150f))
                    return true;

                data.CurFloor++;
                return false;
            }
            return false;
        }
    }
    public class TownElevatorMoveDownState : TownElevatorAIState
    {
        public Vector3 targetPos = Vector3.zero;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                targetPos = Vector3.zero;
                targetPos.y = TownMap.GetElevatorContainerPosY(data.CurFloor - 1);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                targetPos = Vector3.zero;
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                targetPos.x = data.Elevator.transform.localPosition.x;
                targetPos.z = data.Elevator.transform.localPosition.z;
                if (data.Controller.UpdateLocalTarget(dt, targetPos, 0, false, 150f))
                    return true;

                data.CurFloor--;
                return false;
            }
            return false;
        }
    }
}
