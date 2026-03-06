using SBCommonLib;
using SBSocketSharedLib;
using Spine.Unity;
using UnityEngine;

public class BatteryCreator : PropController
{
    ObjectBatteryCreatorGameData data = null;
    long nextAvailableTimestamp = long.MaxValue;
    bool currentStatus;
    GameObject sandglassIcon = null;

    public bool IsAvailable
    {
        get
        {
            var localTime = SBUtil.GetCurrentMilliSecTimestamp();
            return nextAvailableTimestamp <= localTime;
        }
    }

    public override void Init()
    {
        base.Init();

        GameObjectType = GameObjectType.MapObject;
        data = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_battery_creater, ObjData.sub_obj_uid) as ObjectBatteryCreatorGameData;

        if (sandglassIcon == null)
        {
            sandglassIcon = Managers.Resource.Instantiate("Object/Icon/sandglass_icon");
            sandglassIcon.transform.SetParent(transform);
            sandglassIcon.transform.localPosition = new Vector3(0f, 3f, 0f);
            sandglassIcon.transform.localScale = Vector3.one;

            sandglassIcon.transform.Find("model").gameObject.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "f_play_0", true);
        }

        nextAvailableTimestamp = Game.Instance.GameRoom.GameStartTimestamp + (int)(data.create_starting_cool_time * 1000);

        SetStatus(false);
    }

    void SetStatus(bool isOn)
    {
        currentStatus = isOn;
        if (isOn)
        {
            animState.SetAnimation(0, $"f_on_{data.resource_id}", true);
            sandglassIcon.SetActive(false);
        }
        else
        {
            animState.SetAnimation(0, $"f_off_{data.resource_id}", true);
            sandglassIcon.SetActive(true);
        }
    }

    public void OnGetBattery()
    {
        var localTime = SBUtil.GetCurrentMilliSecTimestamp();
        nextAvailableTimestamp = localTime + (int)(data.create_cool_time * 1000);
        SetStatus(false);
        Game.Instance.PlayerController.UpdateTargetIcon();

        PlayAnim($"f_play_{data.resource_id}", false);
        AddAnimQueue($"f_off_{data.resource_id}", true);
    }

    public override void SetRespawnTimeOnReconnect(long regenTime)
    {
        nextAvailableTimestamp = regenTime;
    }

    private void Update()
    {
        if (Game.Instance.GameRoom.GameStartTimestamp == 0)
            return;
        else
        {
            if (nextAvailableTimestamp == (int)(data.create_starting_cool_time * 1000))
            {
                nextAvailableTimestamp = Game.Instance.GameRoom.GameStartTimestamp + (int)(data.create_starting_cool_time * 1000);
            }
        }
        var localTime = SBUtil.GetCurrentMilliSecTimestamp();
        if (localTime >= nextAvailableTimestamp && !currentStatus)
        {
            SetStatus(true);
            Game.Instance.PlayerController.UpdateTargetIcon();
        }
    }
}
