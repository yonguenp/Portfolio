using SBSocketSharedLib;
using System;
using System.Collections.Generic;

public class BaseStateMachine<T> where T : struct, IConvertible
{
    protected Dictionary<T, IState> states;
    protected IState curState;

    public BaseStateMachine(T initType, IState initState)
    {
        states = new Dictionary<T, IState>();
        states.Add(initType, initState);

        curState = initState;
        initState.OnEnter();
    }

    public virtual void OnEnter(T newType)
    {
        var newState = states[newType];

        if (curState == newState) { return; }

        curState.OnExit();
        newState.OnEnter();
        curState = newState;
    }

    public virtual void OnUpdate(float dt)
    {
        curState.OnUpdate(dt);
    }

    public void AddState(T type, IState state)
    {
        if (states == null) states = new Dictionary<T, IState>();

        states.Add(type, state);
    }
}
