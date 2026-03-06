public enum SceneType
{
    Unknown,
    Start,
    Lobby,
    Room,
    Game,
    Result,
}

public enum Sound : sbyte
{
    Bgm,
    Effect,
    MaxCount,
}

public enum MoveDir : byte
{
    None,
    Right,
    Up,
    Left,
    Down,

    // 비트값 아닌데 비트연산해서 오히려 에러남 : Left | Up은 2|3이라 그냥 3임
    // RightUP = Right | Up,
    // RightDown = Right | Down,
    // LeftUP = Left | Up,
    // LeftDown = Left | Down,
}
