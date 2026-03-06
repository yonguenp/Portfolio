using SandboxNetwork.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    
    public interface IStateBase
    {
        bool OnEnter();
        bool OnExit();
        bool OnPause();
        bool OnResume();
        bool Update(float dt);
        bool Destroy();
    }
    public abstract class StateBase : IStateBase
    {
        protected bool isPause = false;
        public bool IsEnter { get; protected set; } = false;
        public bool IsPlaying { get { return IsEnter && !isPause; } }
        public virtual bool OnEnter()
        {
            if (IsEnter == true) return false;
            IsEnter = true;
            return true;
        }
        public virtual bool OnExit()
        {
            if (IsEnter == false) return false;
            IsEnter = false;
            return true;
        }
        public virtual bool OnPause()
        {
            if (isPause == true) return false;
            isPause = true;
            return true;
        }
        public virtual bool OnResume()
        {
            if (isPause == false) return false;
            isPause = false;
            return true;
        }
        public virtual bool Update(float dt)
        {
            return IsPlaying;
        }
        public virtual bool Destroy()
        {
            return true;
        }
    }

    interface IStateMachine<T> where T : class, IStateBase
    {
        bool AddState(T state);
        T GetState(Type type);
        bool StateInit();
    }
    public abstract class SimpleStateMachine<T> : IStateMachine<T> where T : class, IStateBase
    {
        public T CurState { get; protected set; }
        protected Dictionary<Type, T> states = new Dictionary<Type, T>();

        public abstract void SetState();
        public virtual T BattleState { get; protected set; }

        public virtual bool AddState(T state)
        {
            Type stateType = state.GetType();
            if (states.ContainsKey(stateType))
            {
                return false;
            }

            states.Add(stateType, state);
            return true;
        }

        public virtual bool ChangeState<TYPE>() where TYPE : class, IStateBase
        {
            return ChangeState(typeof(TYPE));
        }

        public virtual bool ChangeState(T state)
        {
            if (state == null)
                return false;

            if (CurState == null)
            {
                if (state.OnEnter())
                {
                    CurState = state;
                   
                    return true;
                }
            }
            else
            {
                if (CurState.OnExit() && state.OnEnter())
                {
                    CurState = state;

                    return true;
                }
            }

            return false;
        }

        public bool ChangeState(Type state)
        {
            return ChangeState(GetState(state));
        }

        public virtual T GetState(Type type)
        {
            if (states.ContainsKey(type))
                return states[type] as T;

            return null;
        }

        public virtual TYPE GetState<TYPE>() where TYPE : class, IStateBase
        {
            var type = typeof(TYPE);
            if (states.ContainsKey(type))
                return states[type] as TYPE;

            return null;
        }

        public virtual bool StateInit()
        {
            CurStateClear();
            states.Clear();
            SetState();
            return true;
        }
        public virtual void StateDataClear()
        {
            
        }

        public virtual void CurStateClear()
        {
            if (CurState != null)
            {
                CurState.OnExit();
                CurState = null;
            }
        }
        public virtual void Destroy()
        {
            CurStateClear();
            var it = states.GetEnumerator();
            while(it.MoveNext())
            {
                if (it.Current.Value == null)
                    continue;

                it.Current.Value.Destroy();
            }
            states.Clear();
        }

        public virtual bool Update(float dt)
        {
            if (CurState == null)
                return false;

            return CurState.Update(dt);
        }

        public virtual bool IsState<TYPE>() where TYPE : StateBase
        {
            if (CurState == null)
                return false;

            return CurState is TYPE;
        }
    }
    //public abstract class StackStateMachine<T> : IStateMachine<T> where T : class, IStateBase
    //{
    //    protected Stack<T> stacks = new Stack<T>();
    //    protected Dictionary<Type, T> states = new Dictionary<Type, T>();

    //    public abstract void SetState();
    //    public bool AddState(T state)
    //    {
    //        Type stateType = state.GetType();
    //        if (states.ContainsKey(stateType))
    //        {
    //            return false;
    //        }

    //        states.Add(stateType, state);
    //        return true;
    //    }

    //    public virtual bool StateInit()
    //    {
    //        ClearStackState();
    //        states.Clear();
    //        SetState();
    //        return true;
    //    }

    //    public virtual T GetPeekState()
    //    {
    //        if (stacks.Count <= 0)
    //            return null;

    //        return stacks.Peek();
    //    }

    //    public virtual bool PushState<TYPE>() where TYPE : class, IStateBase
    //    {
    //        return PushState(typeof(TYPE));
    //    }

    //    public virtual bool PushState(Type type)
    //    {
    //        return PushState(GetState(type));
    //    }

    //    public virtual bool PushState(T state)
    //    {
    //        if (state == null)
    //            return false;

    //        var curState = GetPeekState();
    //        if (curState != null)
    //            curState.OnPause();

    //        stacks.Push(state);
    //        state.OnEnter();

    //        return true;
    //    }

    //    public virtual T PopState()
    //    {
    //        if (stacks.Count <= 0)
    //            return null;

    //        var curState = stacks.Pop();
    //        if (curState != null)
    //            curState.OnExit();

    //        var prevState = GetPeekState();
    //        if (prevState != null)
    //            prevState.OnResume();

    //        return curState;
    //    }

    //    public virtual bool PopState<TYPE>() where TYPE : class, IStateBase
    //    {
    //        bool result = false;
    //        if (stacks.Count <= 0)
    //            return result;

    //        var curState = stacks.Peek();
    //        if (curState != null && curState.GetType() == typeof(TYPE))
    //        {
    //            stacks.Pop();
    //            curState.OnExit();

    //            var prevState = GetPeekState();
    //            if (prevState != null)
    //                prevState.OnResume();

    //            result = true;
    //            return result;
    //        }

    //        var tempStack = new Stack<T>();
    //        while (stacks.Count > 0)
    //        {
    //            var tempState = stacks.Pop();
    //            if (tempState != null && tempState.GetType() == typeof(TYPE))
    //            {
    //                tempState.OnExit();
    //                result = true;
    //                break;
    //            }
    //            tempStack.Push(tempState);
    //        }

    //        while (tempStack.Count > 0)
    //        {
    //            var tempState = tempStack.Pop();
    //            if (tempState == null)
    //                continue;

    //            stacks.Push(tempState);
    //        }

    //        return result;
    //    }

    //    public virtual bool PopState(Type type)
    //    {
    //        bool result = false;
    //        if (stacks.Count <= 0)
    //            return result;

    //        var curState = stacks.Peek();
    //        if (curState != null && curState.GetType() == type)
    //        {
    //            stacks.Pop();
    //            curState.OnExit();

    //            var prevState = GetPeekState();
    //            if (prevState != null)
    //                prevState.OnResume();

    //            result = true;
    //            return result;
    //        }

    //        var tempStack = new Stack<T>();
    //        while (stacks.Count > 0)
    //        {
    //            var tempState = stacks.Pop();
    //            if (tempState != null && tempState.GetType() == type)
    //            {
    //                tempState.OnExit();
    //                result = true;
    //                break;
    //            }
    //            tempStack.Push(tempState);
    //        }

    //        while (tempStack.Count > 0)
    //        {
    //            var tempState = tempStack.Pop();
    //            if (tempState == null)
    //                continue;

    //            stacks.Push(tempState);
    //        }

    //        return result;
    //    }

    //    public virtual void ClearStackState()
    //    {
    //        while (stacks.Count > 0)
    //        {
    //            var curState = stacks.Pop();
    //            if (curState != null)
    //                curState.OnExit();
    //        }
    //    }

    //    public virtual T GetState(Type type)
    //    {
    //        if (states.ContainsKey(type))
    //            return states[type] as T;

    //        return null;
    //    }

    //    public virtual TYPE GetState<TYPE>() where TYPE : class, IStateBase
    //    {
    //        var type = typeof(TYPE);
    //        if (states.ContainsKey(type))
    //            return states[type] as TYPE;

    //        return null;
    //    }

    //    public virtual void Update(float dt)
    //    {
    //        var curState = GetPeekState();
    //        if (curState == null)
    //            return;

    //        curState.Update(dt);
    //    }

    //    public virtual int Count()
    //    {
    //        if (stacks == null)
    //            return 0;
    //        return stacks.Count;
    //    }
    //}
    //public abstract class DictionaryStateMachine<T> : IStateMachine<T> where T : class, IStateBase
    //{
    //    protected Dictionary<Type, T> curStates = new Dictionary<Type, T>();
    //    public Dictionary<Type, T> CurStates { get { return curStates; } set { curStates = value; } }
    //    protected Dictionary<Type, T> states = new Dictionary<Type, T>();

    //    public abstract void SetState();

    //    public virtual bool AddState(T state)
    //    {
    //        Type stateType = state.GetType();
    //        if (states.ContainsKey(stateType))
    //        {
    //            return false;
    //        }

    //        states.Add(stateType, state);
    //        return true;
    //    }

    //    public bool PlusState(Type state)
    //    {
    //        return PlusState(GetState(state));
    //    }

    //    public virtual bool PlusState<TYPE>() where TYPE : class, IStateBase
    //    {
    //        return PlusState(typeof(TYPE));
    //    }

    //    public virtual bool PlusState(T state)
    //    {
    //        if (state == null)
    //            return false;

    //        if (curStates == null || curStates.ContainsKey(state.GetType()))
    //            return false;

    //        if (state.OnEnter())
    //        {
    //            curStates.Add(state.GetType(), state);
    //            return true;
    //        }

    //        return false;
    //    }

    //    public bool MinusState(Type state)
    //    {
    //        return MinusState(GetState(state));
    //    }

    //    public virtual bool MinusState<TYPE>() where TYPE : class, IStateBase
    //    {
    //        return MinusState(typeof(TYPE));
    //    }

    //    public virtual bool MinusState(T state)
    //    {
    //        if (state == null)
    //            return false;

    //        if (curStates == null || !curStates.ContainsKey(state.GetType()))
    //            return false;

    //        if (state.OnExit())
    //        {
    //            curStates.Remove(state.GetType());
    //            return true;
    //        }

    //        return false;
    //    }

    //    public virtual T GetState(Type type)
    //    {
    //        if (states.ContainsKey(type))
    //            return states[type] as T;

    //        return null;
    //    }

    //    public virtual TYPE GetState<TYPE>() where TYPE : class, IStateBase
    //    {
    //        var type = typeof(TYPE);
    //        if (states.ContainsKey(type))
    //            return states[type] as TYPE;

    //        return null;
    //    }

    //    public virtual bool StateInit()
    //    {
    //        CurStateClear();
    //        states.Clear();
    //        SetState();
    //        return true;
    //    }

    //    public virtual void CurStateClear()
    //    {
    //        if (curStates != null)
    //        {
    //            var it = curStates.GetEnumerator();

    //            while (it.MoveNext())
    //            {
    //                var cur = it.Current;

    //                if (cur.Value == null)
    //                    continue;

    //                cur.Value.OnExit();
    //            }

    //            curStates.Clear();
    //        }
    //    }
    //}
}