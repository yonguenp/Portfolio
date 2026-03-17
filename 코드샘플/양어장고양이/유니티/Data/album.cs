using System;

[Serializable]
public class album : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.ALBUM; }
}

