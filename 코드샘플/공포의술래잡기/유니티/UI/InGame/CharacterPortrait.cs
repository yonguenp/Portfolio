using UnityEngine;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour
{
    public enum PORTRAIT_STATE
    {
        NORMAL,
        GROGGY,
        ESCAPE,
    }

    [SerializeField] UIPortraitCharacter character = null;
    [SerializeField] GameObject objHp = null;
    [SerializeField] Text txtHp = null;
    [SerializeField] GameObject objBattery = null;
    [SerializeField] Text txtBattery = null;
    [SerializeField] Text nick = null;

    [SerializeField] Image rankSymbol = null;
    [SerializeField] GameObject objLaurel = null;
    [SerializeField] Image r_Laurel = null;
    [SerializeField] Image l_Laurel = null;
    [SerializeField] GameObject objEscapeImage = null;
    [SerializeField] GameObject objHpGauge = null;
    [SerializeField] Image hpGauge;

    public SBSocketSharedLib.PlayerObjectInfo userInfo { get; private set; }
    public int CharacterType { get; private set; }
    public int BattryChargeCount { get; private set; }

    PORTRAIT_STATE curState = PORTRAIT_STATE.NORMAL;
    int slotIndex = -1;

    void Clear()
    {
        txtHp.text = string.Empty;
        SetBattery(0);
    }
    public void SetData(int type, SBSocketSharedLib.PlayerObjectInfo data, bool hideDetail)
    {
        userInfo = data;
        slotIndex = Managers.PlayData.GetSlotIndex(data.ObjectId);

        Clear();

        SetImage(type);
        SetHp(data.StatInfo.Hp, data.StatInfo.MaxHp);

        //objHp.SetActive(!hideDetail);
        objHpGauge.SetActive(!hideDetail);
        objBattery.SetActive(!hideDetail);

        objLaurel.SetActive(hideDetail);
        objEscapeImage.SetActive(hideDetail);

        if (rankSymbol != null)
        {
            rankSymbol.gameObject.SetActive(!hideDetail);
            int rankpoint = 0;
            if (data != null && !string.IsNullOrEmpty(data.ObjectId))
                rankpoint = Managers.PlayData.GetRoomPlayer(data.ObjectId).RankPoint;

            rankSymbol.sprite = RankType.GetRankFromPoint(rankpoint).rank_resource;
        }
        SetName(data.Name);
    }

    public void SetData(CharacterPortrait target)
    {
        if (target == null)
            return;

        SetData(target.CharacterType, target.userInfo, true);
    }

    public void SetData(CharacterPortrait target, int count)
    {
        if (target == null)
            return;

        SetData(target.CharacterType, target.userInfo, true);
        Color _color = Color.white;
        switch (count)
        {
            case 1:
                ColorUtility.TryParseHtmlString("#F5D247", out _color);
                break;
            case 2:
                ColorUtility.TryParseHtmlString("#C6C6C6", out _color);
                break;
            case 3:
                ColorUtility.TryParseHtmlString("#A37354", out _color);
                break;
        }
        objEscapeImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/InGame/img_grade_0" + count);
        r_Laurel.color = _color;
        l_Laurel.color = _color;
    }

    public void SetHp(ushort hp, ushort maxHp)
    {
        if (!string.IsNullOrEmpty(txtHp.text) && txtHp.text != hp.ToString())
        {
            if (character != null)
            {
                character.SetColorWithTween(Color.yellow);
            }
        }

        txtHp.text = hp.ToString();

        if (hp != 0 && hp > 0 && maxHp > 0)
            hpGauge.fillAmount = ((float)hp / (float)maxHp);
        else
            hpGauge.fillAmount = 0;
    }

    public void AddBatteryChargeCount(int batteryCharge)
    {
        SetBattery(BattryChargeCount + batteryCharge);
    }

    public void SetBattery(int batteryCnt)
    {
        BattryChargeCount = batteryCnt;
        txtBattery.SetText(BattryChargeCount.ToString(), SHelper.TEXT_TYPE.SURVIVOR_1 + slotIndex);
    }

    private void SetImage(int type)
    {
        CharacterType = type;
        character.SetPortrait(type);
    }

    public void SetState(PORTRAIT_STATE state)
    {
        if (curState == PORTRAIT_STATE.ESCAPE)
        {
            state = PORTRAIT_STATE.ESCAPE;
        }

        curState = state;

        switch (state)
        {
            case PORTRAIT_STATE.NORMAL:
                SetColor(Color.white);
                break;
            case PORTRAIT_STATE.GROGGY:
                SetColor(Color.red);
                break;
            case PORTRAIT_STATE.ESCAPE:
                Managers.Sound.Play("effect/EF_ESCAPE_SUCCESS", Sound.Effect);
                //SetColor(Color.green);
                break;
        }
    }

    private void SetColor(Color color)
    {
        character.SetColor(color);
    }

    private void SetName(string name)
    {
        if (name.Length > 10)
            name = name.Substring(0, 8) + "...";

        nick.SetText(name, SHelper.TEXT_TYPE.SURVIVOR_1 + slotIndex);
    }

}
