using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField]
        SpriteRenderer inSide = null;
        [SerializeField]
        SpriteRenderer outSide = null;

        private TownElevatorStateMachine stateMachine = null;

        private SBController controller = null;
        public SBController Controller
        {
            get { return controller; }
            set { controller = value; }
        }
        public TownElevatorData Data
        {
            get 
            {
                if (stateMachine == null)
                    return null;

                return stateMachine.Data; 
            }
        }
        public Transform TopGear { get; private set; } = null;
        public Transform BotGear { get; private set; } = null;
        public SpriteRenderer InSide { get { return inSide; } }
        public SpriteRenderer OutSide { get { return outSide; } }
        public void Init(Transform gearT, Transform gearB)
        {
            TopGear = gearT;
            BotGear = gearB;

            if (stateMachine == null)
                stateMachine = new TownElevatorStateMachine();

            stateMachine.SetData(this, 0);
            stateMachine.SetState();
            stateMachine.ChangeState<TownElevatorIdleState>();

            controller = GetComponent<SBController>();
            if (controller == null)
                controller = gameObject.AddComponent<SBController>();
        }

        public void Push(TownStateData obj)
        {
            if (IsContain(obj))
                return;

            Data.PushContain(obj);
            obj.Dragon.transform.SetParent(transform, true);
        }
        public void Remove(TownStateData obj)
        {
            Data.PopNeed(obj);
            Data.PopContain(obj);
        }
        public void Pop(TownStateData obj)
        {
            if (!IsContain(obj))
                return;

            obj.Dragon.transform.SetParent(Data.GetParent(obj), true);
            Data.PopContain(obj);
        }

        public void PushNeed(TownStateData obj)
        {
            Data.PushNeed(obj);
        }

        public void PopNeed(TownStateData obj)
        {
            Data.PopNeed(obj);
        }

        public bool IsContain(TownStateData obj)
        {
            return Data.IsContain(obj);
        }

        public bool IsState<T>() where T : TownElevatorAIState
        {
            return stateMachine.CurState is T;
        }

        private void Update()
        {
            if (stateMachine == null)
                return;

            stateMachine.Update(SBGameManager.Instance.DTime);
        }
    }
}