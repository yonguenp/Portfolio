using SBSocketSharedLib;
using UnityEngine;
using Spine.Unity;

public class BatteryGenerator : PropController
{
    float currentProgress = 0f;
    GameObject arrowIcon = null;
    int maxBatteryCount;

    public bool IsActived { get; private set; }

    public bool IsFullyCharged { get { return currentProgress >= 1f; } }

    public override void Init()
    {
        base.Init();

        GameObjectType = GameObjectType.MapObject;
        var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_battery_generator, ObjData.sub_obj_uid) as ObjectBatteryGeneratorGameData;
        maxBatteryCount = data.input_battery_max;

        if (arrowIcon == null)
        {
            arrowIcon = Managers.Resource.Instantiate("Object/Icon/arrow_icon");
            arrowIcon.transform.SetParent(transform);
            arrowIcon.transform.localPosition = new Vector3(0f, .5f, 0f);
            arrowIcon.transform.localScale = Vector3.one;

            arrowIcon.transform.Find("model").gameObject.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "f_play_0", true);
        }

        IsActived = false;
        currentProgress = 0f;
        PlayAnim("f_off_0", true);
        arrowIcon.SetActive(false);
        Game.Instance.UIGame.SetBatteryRemainCount(maxBatteryCount);
    }

    public void OnActiveGenerator()
    {
        if (IsActived) return;
        if (currentProgress >= 1f) return;
        IsActived = true;
        PlayAnim("f_on_0", true);
        arrowIcon.SetActive(true);

        if (Game.Instance.PlayerController.Target != null &&
            Game.Instance.PlayerController.Target.gameObject == gameObject)
            Game.Instance.PlayerController.UpdateTargetIcon();
    }

    public void OnDeactivateGenerator()
    {
        if (!IsActived) return;
        if (currentProgress >= 1f) return;
        IsActived = false;
        PlayAnim("f_off_0", true);
        arrowIcon.SetActive(false);

        if (Game.Instance.PlayerController.Target != null &&
            Game.Instance.PlayerController.Target.gameObject == gameObject)
            Game.Instance.PlayerController.UpdateTargetIcon();
    }

    public void OnSetBattery(int batteryCnt, bool isMe)
    {
        float progress = (float)batteryCnt / maxBatteryCount;
        currentProgress = progress;
        if (currentProgress >= 1f)
        {
            currentProgress = 1f;
        }

        Game.Instance.GameRoom.SetBatteryProgress(currentProgress);
        Game.Instance.UIGame.SetBatteryProgress(currentProgress);
        Game.Instance.UIGame.SetBatteryRemainCount(maxBatteryCount - batteryCnt);
        
        if (isMe)
        {
            PlayAnim("f_play_0", false);
            if (currentProgress >= 1f)
            {
                arrowIcon.SetActive(false);
                AddAnimQueue("f_full_0", true);
            }
            else if (IsActived) AddAnimQueue("f_on_0", true);
            else AddAnimQueue("f_off_0", true);
        }
        else
        {
            if (currentProgress >= 1f)
            {
                arrowIcon.SetActive(false);
                PlayAnim("f_full_0", true);
            }
        }
    }
}
