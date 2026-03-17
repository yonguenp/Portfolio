using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_pass_forever : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_PASS_FOREVER; }

    public static uint[] GetSeasonPhotos(int season)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PASS_FOREVER);
        if (necoData == null)
            return null;

        int lenth = necoData.Count;
        object obj;

        game_data passfData = necoData[season % lenth];
        uint[] photos = new uint[4];

        for(int i = 1; i <= 4; i++)
        {
            if (passfData.data.TryGetValue(string.Format("photo_{0}", i), out obj))
            {
                photos[i-1] = (uint)obj;
            }
        }

        return photos;
    }
}