using SBSocketSharedLib;
using UnityEngine;
using UnityEngine.UI;

public partial class CharacterObject
{
    // bool isHudShowing;

    // public void SetHudPos(HudPosText hud)
    // {
    //     _hudPos = hud;
    // }

    public void SetHudHp(HudHp hud)
    {
        _hudHp = hud;
    }

    public void SetHudBattery(HudBattery hud)
    {
        _hudBattery = hud;
    }

    // public void SetHudPos(Vector2 pos)
    // {
    //     _hudPos?.SetPos(pos);
    // }

    // public void SetHudPos2(Vector2 pos)
    // {
    //     _hudPos?.SetServerPos(pos);
    // }

    public void ShowAllHuds(bool value)
    {
        if (Escaped == true)
            value = false;

        if (gameObject == null)
            return;

        if (gameObject.activeInHierarchy == false)
            value = false;

        _hudPos?.gameObject.SetActive(value);
        userNameObject?.gameObject.SetActive(value);
        RootEffect?.gameObject.SetActive(value);

        if (State == CreatureStatus.Groggy)
            value = false;
        _hudBattery?.gameObject.SetActive(value);
        _hudHp?.gameObject.SetActive(value);
    }

    void RemoveAllHuds()
    {
        if (_hudBattery) Destroy(_hudBattery.gameObject);
        if (_hudHp) Destroy(_hudHp.gameObject);
        if (_hudPos) Destroy(_hudPos.gameObject);
        if (userNameObject) Destroy(userNameObject.gameObject);

        _hudBattery = null;
        _hudHp = null;
        _hudPos = null;
        userNameObject = null;

        Game.Instance.HudNode?.OnRemoveCharacterHud(this);
    }

    public virtual void SetUserName(HudName userName)
    {
        userNameObject = userName;
    }

    public virtual void SetUserEmotion(HudEmotion userEmotion)
    {
        userEmotionObject = userEmotion;
    }
}
