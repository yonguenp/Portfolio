using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerItem : MonoBehaviour
{
    [SerializeField] Text text = null;
    ScriptTriggerData CurData = null;
    TriggerList parent = null;
    public void SetData(ScriptTriggerData data, TriggerList triggerList)
    {
        CurData = data;

        parent = triggerList;
        text.text = "트리거 키 : " + CurData.KEY.ToString() + " / ";
        switch (CurData.TYPE)
        {
            case ScriptTriggerType.STAGE_IN:
            {
                StageBaseData baseData = StageBaseData.Get(CurData.TYPE_PARAM.ToString());
                if (baseData != null)
                {
                    text.text += baseData.WORLD.ToString() + "월드 " + baseData.STAGE.ToString() + " 스테이지 진입 후 발동";
                    return;
                }
            }break;
            case ScriptTriggerType.STAGE_CLEAR:
            {
                StageBaseData baseData = StageBaseData.Get(CurData.TYPE_PARAM.ToString());
                if (baseData != null)
                {
                    text.text += baseData.WORLD.ToString() + "월드 " + baseData.STAGE.ToString() + " 스테이지 클리어 후 발동";
                    return;
                }
            }
            break;
            case ScriptTriggerType.STAGE_MONSTER_SHOW:
            {
                MonsterSpawnData spawnData = MonsterSpawnData.GetKey(CurData.TYPE_PARAM);
                if (spawnData != null)
                {
                    var baseData = MonsterBaseData.Get(spawnData.MONSTER.ToString());
                    if (baseData != null)
                    {
                        text.text += StringData.GetStringByStrKey(baseData._NAME) + " 몬스터 등장 후 발동";
                        return;
                    }
                }
            }
            break;
            case ScriptTriggerType.QUEST_CLEAR:
            {
                QuestData baseData = QuestData.Get(CurData.TYPE_PARAM.ToString());
                if (baseData != null)
                {
                    text.text += baseData.KEY + " 퀘스트 클리어 후 발동";
                    return;
                }
            }
            break;
            case ScriptTriggerType.CONSTRUCT_START:
            {
                BuildingLevelData baseData = BuildingLevelData.Get(CurData.TYPE_PARAM.ToString());
                if (baseData != null)
                {
                    text.text += "Lv." + baseData.LEVEL.ToString() + StringData.GetStringByStrKey("building_base:name:" + baseData.BUILDING_GROUP) + " 건설 시작 후 발동";
                    return;
                }
            }
            break;
            case ScriptTriggerType.CONSTRUCT_DONE:
            {
                BuildingLevelData baseData = BuildingLevelData.Get(CurData.TYPE_PARAM.ToString());
                if (baseData != null)
                {
                    text.text += "Lv." + baseData.LEVEL.ToString() + StringData.GetStringByStrKey("building_base:name:" + baseData.BUILDING_GROUP) + " 건설 완료 후 발동";
                    return;
                }
            }
            break;
            case ScriptTriggerType.FRIST_INIT:
            {
                text.text += "계정 생성후 최초 진입시 발동";
                return;
            }
            break;
            case ScriptTriggerType.ARENA_FIRST_INIT:
            {
                text.text += "아레나 최초 진입시 발동";
                return;
            }
            break;
            case ScriptTriggerType.DAILYDUNGEON_FIRST_INIT:
            {
                text.text += "요일던전 최초 진입시 발동";
                return;
            }
            break;
            case ScriptTriggerType.INTRO:
            {
                text.text += "인트로";
                return;
            }
            break;
        }

        text.text += " 오류 확인 필요";
        text.color = Color.red;
    }

    public void OnClick()
    {
        if (CurData == null)
            return;

        parent.OnSelectTrigger(CurData);
    }
}
