using System.Collections;
using System.Collections.Generic;
using SandboxNetwork;
using UnityEngine;

public class HotTimeController : MonoBehaviour
{
    [SerializeField] HotTimeDescType type = HotTimeDescType.NONE;
    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool enable = false;
        switch (type)
        {
            case HotTimeDescType.ADVENTURE:
                enable = GameConfigTable.IsAdventureHotTime;
                break;
            case HotTimeDescType.WORLDBOSS:
                enable = GameConfigTable.IsRaidHotTime;
                break;
            case HotTimeDescType.DAILYDUNGEON:
                enable = GameConfigTable.IsDailyDungeonHotTime;
                break;
            case HotTimeDescType.GEMDUNGEON:
                enable = GameConfigTable.IsGemDungeonHotTime;
                break;
            case HotTimeDescType.DUNGEON_LIST:
                enable = GameConfigTable.IsAdventureHotTime || GameConfigTable.IsRaidHotTime || GameConfigTable.IsDailyDungeonHotTime;
                break;
            default:
                enable = false;
                break;
        }

        gameObject.SetActive(enable);
    }
}
