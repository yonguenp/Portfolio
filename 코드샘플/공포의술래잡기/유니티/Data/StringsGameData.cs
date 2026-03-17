using System.Collections.Generic;

public class StringsGameData : GameData
{
    public string key { get; private set; }
    public string korean { get; private set; }
    public string english { get; private set; }
    public string japanese { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        key = data["key"];
        korean = data["ko"];
        english = data["en"];
        japanese = data["jp"];
    }

    static public StringsGameData GetStringData(string key)
    {
        List<GameData> data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.@string);
        if(data == null)
        {
            SBDebug.LogError("데이터 로드 실패!");
        }
        foreach (StringsGameData row in data)//검색방법은 차차 생각해보자
        {
            if (row.key == key)
            {
                return row;
            }
        }

        return null;
    }
}

