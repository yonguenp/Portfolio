using SBSocketSharedLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatItem : MonoBehaviour, EventListener<NotifyEvent>
{
    public enum STAT_TYPE
    {
        NONE,

        CHARACTER_STAT_MIN = NONE,

        CHARACTER_NAME,
        CHARACTER_LEVEL,
        CHARACTER_ENCHANCEMENT,
        SKILL_LEVEL,

        CHARACTER_STAT_MAX,

        DETAIL_STAT_MIN = CHARACTER_STAT_MAX,
        //common
        CHARACTER_EXP,
        MOVE_SPEED,

        //survivor
        HP,
        QUEST_SPEED,
        //SHIELD,
        MAX_BATTERY,

        //chaser
        ATK,
        COOLTIME,
        ATTACK_COOL_TIME,

        PLAY_TIME,
        //chaser
        KILL_COUNT,
        HIT_COUNT,
        //survivor
        CHARGE_COUNT,
        GET_COUNT,

        WIN_COUNT,
        LOSE_COUNT,
        HIGH_SCORE,

        DETAIL_STAT_MAX,
    };

    [SerializeField]
    protected Text StatName;
    [SerializeField]
    protected Text StatValue;
    [SerializeField]
    Image StatBg;
    [SerializeField]
    Sprite[] FrameSprite;

    protected STAT_TYPE curType = STAT_TYPE.NONE;
    UserCharacterData myData;
    CharacterGameData charData;

    private void OnEnable()
    {
        this.EventStartListening();
    }

    private void OnDisable()
    {
        this.EventStopListening();
    }

    public virtual void SetStat(CharacterGameData characterData, STAT_TYPE statType)
    {
        curType = statType;

        charData = characterData;
        myData = Managers.UserData.GetMyCharacterInfo(charData.GetID());
        Refresh();
    }

    void Refresh()
    {
        CharacterLevelGameData curLevelData = null;

        int level = GameConfig.Instance.MAX_CHARACTER_LEVEL;
        if (myData != null)
            level = myData.lv;

        curLevelData = charData.levelData[level];

        switch (curType)
        {
            case STAT_TYPE.CHARACTER_NAME:
                StatName.text = StringManager.GetString("이름");
                StatValue.text = charData.GetName();
                break;
            case STAT_TYPE.CHARACTER_LEVEL:
                StatName.text = StringManager.GetString("레벨");
                StatValue.text = level.ToString();
                break;
            case STAT_TYPE.CHARACTER_ENCHANCEMENT:
                StatName.text = StringManager.GetString("강화");
                StatValue.text = (myData == null ? 1 : myData.enchant).ToString();
                break;
            case STAT_TYPE.SKILL_LEVEL:
                StatName.text = StringManager.GetString("스킬");
                StatValue.text = (myData == null ? 1 : myData.skillLv).ToString();
                break;

            case STAT_TYPE.CHARACTER_EXP:
                StatName.text = StringManager.GetString("경험치");
                StatValue.text = curLevelData.need_exp.ToString();
                break;
            case STAT_TYPE.MOVE_SPEED:
                StatName.text = StringManager.GetString("속도");
                StatValue.text = (Mathf.RoundToInt(curLevelData.move_speed)).ToString();
                if (StatBg && FrameSprite.Length == 2)
                {
                    StatBg.sprite = FrameSprite[1];
                }
                break;
            case STAT_TYPE.HP:
                StatName.text = StringManager.GetString("체력");
                StatValue.text = (curLevelData.hp / 1000).ToString();
                break;
            case STAT_TYPE.MAX_BATTERY:
                StatName.text = StringManager.GetString("최대소지량");
                StatValue.text = (curLevelData.max_battary * 0.001f).ToString();
                break;
            //case STAT_TYPE.SHIELD:
            //    StatName.text = "SHD";
            //    StatValue.text = curLevelData.shield_hp.ToString();
            //    break;
            case STAT_TYPE.ATK:
                StatName.text = StringManager.GetString("공격력");
                StatValue.text = (curLevelData.atk_point / 1000).ToString();
                break;
            case STAT_TYPE.ATTACK_COOL_TIME:
                StatName.text = StringManager.GetString("char_info_atktime");
                StatValue.text = (curLevelData.attack_cool_time).ToString();
                break;
            case STAT_TYPE.PLAY_TIME:
                StatName.text = StringManager.GetString("ch_play_count");
                StatValue.text = (myData == null ? 0 : myData.playData.play).ToString();
                break;
            //chaser
            case STAT_TYPE.KILL_COUNT:
                StatName.text = StringManager.GetString("ch_survivor_killcount");
                StatValue.text = (myData == null ? 0 : myData.playData.kill).ToString();
                break;
            case STAT_TYPE.HIT_COUNT:
                StatName.text = StringManager.GetString("ch_survivor_attack_count");
                StatValue.text = (myData == null ? 0 : myData.playData.hit).ToString();
                break;

            //survivor
            case STAT_TYPE.CHARGE_COUNT:
                StatName.text = StringManager.GetString("ch_bettery_charge_count");
                StatValue.text = (myData == null ? 0 : myData.playData.charge).ToString();
                break;
            case STAT_TYPE.GET_COUNT:
                StatName.text = StringManager.GetString("ch_bettery_get_count");
                StatValue.text = (myData == null ? 0 : myData.playData.get).ToString();
                break;

            case STAT_TYPE.WIN_COUNT:
                StatName.text = StringManager.GetString("ch_win_count");
                StatValue.text = (myData == null ? 0 : myData.playData.win).ToString();
                break;
            case STAT_TYPE.LOSE_COUNT:
                StatName.text = StringManager.GetString("ch_lose_count");
                StatValue.text = (myData == null ? 0 : myData.playData.lose).ToString();
                break;
            case STAT_TYPE.HIGH_SCORE:
                StatName.text = StringManager.GetString("ch_best_score");
                StatValue.text = (myData == null ? 0 : myData.playData.highscore).ToString();
                break;
        }
    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_CHARACTER_UPDATE:
                {
                    if (curType == STAT_TYPE.CHARACTER_LEVEL)
                    {
                        Refresh();
                    }
                }
                break;
        }
    }
}
