using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using SBCommonLib;
using System;

public class VehicleObjectController : PropController
{
    GameObject despawnIcon = null;
    Transform remainTimeObject = null;
    ObjectVehicleGameData vehicleData = null;
    SpriteRenderer remainTimeSprite = null;

    public override void Init()
    {
        base.Init();
        if (ObjData != null)
        {
            vehicleData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_vehicle, ObjData.sub_obj_uid) as ObjectVehicleGameData;

            despawnIcon = Managers.Resource.Instantiate("Object/Icon/object_respawn_icon");
            despawnIcon.name = "despawnIcon";
            despawnIcon.transform.SetParent(transform);
            despawnIcon.transform.position = transform.position + new Vector3(0, 1, 0);

            remainTimeObject = despawnIcon.transform.Find("TimeRemain");
            remainTimeSprite = remainTimeObject.GetComponent<SpriteRenderer>();

            despawnIcon.gameObject.SetActive(false);
            remainTimeObject.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }
    }

    public override void OnDespawn()
    {
        gameObject.SetActive(true);
        skeletonAnimation.skeleton.SetColor(new Color(0.63f, 0.58f, 0.58f, 0.29f));

        if (vehicleData != null)
        {
            SetRemainTimeUI(vehicleData.respawnTime);
        }

        enabled = false;
    }

    public override void OnRespawn()
    {
        gameObject.SetActive(true);
        skeletonAnimation.skeleton.SetColor(Color.white);
        despawnIcon.gameObject.SetActive(false);

        enabled = true;
    }

    private void SetRemainTimeUI(float time)
    {
        despawnIcon.gameObject.SetActive(true);
        remainTimeObject.gameObject.SetActive(true);

        float respawnTime = vehicleData.respawnTime;
        remainTimeSprite.material.SetFloat("_Arc1", (respawnTime - time) / respawnTime * 360f);
        remainTimeSprite.material.DOFloat(360f, "_Arc1", time);
    }

    public override void SetRespawnTimeOnReconnect(long regenTime)
    {
        var localTime = Game.Instance.GameRoom.GameTime.GetClientTimestamp();
        var remainTime = (float)((regenTime - localTime) * 0.001f);
        float respawnTime = vehicleData.respawnTime;
        if (remainTime > 0f)
        {
            SetRemainTimeUI(remainTime);

            enabled = false;
        }
    }
}
