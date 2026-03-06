using SBSocketSharedLib;

public class CharacterStateMachine : BaseStateMachine<CreatureStatus>
{
    public CharacterStateMachine(CreatureStatus initType, IState initState) : base(initType, initState) { }

    public override void OnEnter(CreatureStatus newType)
    {
        base.OnEnter(newType);
    }

    public override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);
    }
}

public class CharacterIdleState : IState
{
    public void OnEnter()
    {
        SBDebug.Log(string.Format("{0} : {1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
    }

    public void OnExit()
    {
        SBDebug.Log(string.Format("{0} : {1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
    }

    public void OnUpdate(float dt)
    {
        SBDebug.Log(string.Format("{0} : {1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
    }
}

public class CharacterMoveState : IState
{
    public void OnEnter()
    {
        SBDebug.Log(string.Format("{0} : {1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
    }

    public void OnExit()
    {
        SBDebug.Log(string.Format("{0} : {1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
    }

    public void OnUpdate(float dt)
    {
        SBDebug.Log(string.Format("{0} : {1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
    }
}
