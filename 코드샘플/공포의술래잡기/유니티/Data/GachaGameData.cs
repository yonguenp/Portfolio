using System;
using System.Collections.Generic;
using UnityEngine;

public class GachaGameData : GameData
{
    public enum ASSET_TYPE
    {
        NONE = 0,
        GOLD,
        DIA,
        //ITEM,
        //CHARACTER,
        CASH = 5,
        MILEAGE = 6
    }

    public DateTime startTime { get; private set; }
    public DateTime endTime { get; private set; }

    public int onceTypeID { get; private set; }
    public int repeatTypeID { get; private set; }
    public int adTypeID { get; private set; }
    public int list_weight { get; private set; }
    public List<int> target_char { get; private set; } = new List<int>();

    public string menuResourcePath { get; private set; }
    public string bannerResourcePath { get; private set; }

    public GachaTypesGameData onceInfo { get; private set; } = null;
    public GachaTypesGameData repeatInfo { get; private set; } = null;
    public GachaTypesGameData adInfo { get; private set; } = null;
    static public List<GachaGameData> GetValidGacha()
    {
        List<GachaGameData> ret = new List<GachaGameData>();
        DateTime now = SBCommonLib.SBUtil.KoreanTime;
        foreach (GachaGameData gacha in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.gacha_base))
        {
            if (gacha.startTime < now && gacha.endTime > now)
            {
                ret.Add(gacha);
            }
        }

        return ret;
    }

    public GameObject GetMenuGameObject()
    {
        return Managers.Resource.InstantiateFromBundle("Menu/" + menuResourcePath);
    }
    public GameObject GetBannerGameObject()
    {
        return Managers.Resource.InstantiateFromBundle("Banner/" + bannerResourcePath);
    }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        if (data["start_time"] == "always")
            startTime = DateTime.MinValue;
        else
            startTime = DateTime.Parse(data["start_time"]);

        if (data["start_time"] == "always")
            endTime = DateTime.MaxValue;
        else
            endTime = DateTime.Parse(data["end_time"]);

        menuResourcePath = data["gacha_menu_resource"];//Managers.Resource.InstantiateFromBundle("Menu/" + );//Resources.Load<GameObject>("Prefabs/UI/UI_Gacha/Menu/" + data["gacha_menu_resource"]);
        bannerResourcePath = data["gacha_pickup_resource"];//Managers.Resource.InstantiateFromBundle("Banner/" + );//Resources.Load<GameObject>("Prefabs/UI/UI_Gacha/Banner/" + data["gacha_pickup_resource"]);
        onceTypeID = Int(data["gacha_type_1"]);
        repeatTypeID = Int(data["gacha_type_11"]);
        adTypeID = Int(data["gacha_type_ad"]);
        list_weight = Int(data["list_weight"]);

        target_char.Clear();
        if (data.ContainsKey("target_char"))
        {
            foreach (string sp in (data["target_char"]).Split('/'))
            {
                target_char.Add(Int(sp));
            }
        }
    }

    public void SetOnceTypeData(GachaTypesGameData data)
    {
        if (onceInfo != null)
        {
            SBDebug.LogError("이미 뭔가 있는데?");
        }

        onceInfo = data;
    }

    public void SetRepeatTypeData(GachaTypesGameData data)
    {
        if (repeatInfo != null)
        {
            SBDebug.LogError("이미 뭔가 있는데?");
        }

        repeatInfo = data;
    }

    public void SetAdvertisementTypeData(GachaTypesGameData data)
    {
        if (adInfo != null)
        {
            SBDebug.LogError("이미 뭔가 있는데?");
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        var isPC = packageManager.Call<bool>("hasSystemFeature", "com.google.android.play.feature.HPE_EXPERIENCE");
        if (isPC)
        {
            return;
        }
#endif

        adInfo = data;
    }
}

public class GachaTypesGameData : GameData
{
    public ShopItem priceInfo { get; private set; } = null;

    public int repeats { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        priceInfo = new ShopItem(Int(data["price_type"]), Int(data["price_uid"]), Int(data["price_value"]));

        repeats = Int(data["repeats"]);

        List<GameData> baseDatas = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.gacha_base);
        foreach (GachaGameData baseData in baseDatas)
        {
            if (baseData.onceTypeID == GetID())
            {
                baseData.SetOnceTypeData(this);
            }

            if (baseData.repeatTypeID == GetID())
            {
                baseData.SetRepeatTypeData(this);
            }

            if (baseData.adTypeID == GetID())
            {
                baseData.SetAdvertisementTypeData(this);
            }
        }
    }
}
