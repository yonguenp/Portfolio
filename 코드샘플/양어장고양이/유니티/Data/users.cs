using System;

[Serializable]
public class users : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USERS; }

    public uint GetMaxCardCount()
    {
        if (data.ContainsKey("max_cards"))
            return (uint)data["max_cards"];

        return 0;
    }
}

