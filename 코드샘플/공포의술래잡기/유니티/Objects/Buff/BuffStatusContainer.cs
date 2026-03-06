public enum ObjectBuffStatus
{
    None = 0,
    Freeze = 1 << 0,
    Confuse = 1 << 1,
    Invisible = 1 << 2,
    BlockSight = 1 << 3,
    Stun = 1 << 4,
    SkillCasting = 1 << 5,
    Pluck = 1 << 6,
    Silence = 1 << 7,
    Trap = 1 << 8,
}

public class BuffStatusContainer
{
    public ObjectBuffStatus CurrentStatus { get; private set; }

    public BuffStatusContainer()
    {
        CurrentStatus = ObjectBuffStatus.None;
    }

    ~BuffStatusContainer() { }

    public void SetStatusFlag(ObjectBuffStatus flag)
    {
        CurrentStatus |= flag;
    }

    public void ClearStatusFlag(ObjectBuffStatus flag)
    {
        CurrentStatus &= ~flag;
    }
}
