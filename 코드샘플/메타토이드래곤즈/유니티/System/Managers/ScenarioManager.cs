using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class ScenarioManager : MonoBehaviour, EventListener<QuestEvent>, EventListener<BuildCompleteEvent>
{
    public static ScenarioManager Instance
    {
        get
        {
            return UICanvas.Instance.GetScenarioManager();
        }
    }

    [SerializeField] ScriptController scripter = null;
    [SerializeField] GuideController guider = null;
    /*
     * "트리거 타입
    1. 스테이지 진입 후(웨이브 시작 전)    
    2. 스테이지 클리어 후(결과 화면 이동 전)    
    3. 스테이지 배치 몬스터 등장 
    4. 퀘스트 클리어 후(퀘스트 보상 수령 후)    
    5. 특정 건물 건설 후
    6. 특정 건물 건설 완료 후"
     */
    ScriptTriggerData Cur
    {
        get
        {
            return ScriptTriggerData.GetSeq(User.Instance.UserData.Sequence + 1);
        }
    }

    public bool IsPlaying { get { return scripter.IsPlaying; } }
    private void Awake()
    {
        if (scripter != null)
        {
            scripter.gameObject.SetActive(false);
        }
        if (guider!= null)
        {
            guider.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener<QuestEvent>(this);
        EventManager.AddListener<BuildCompleteEvent>(this);
    }
    private void OnDisable()
    {
        EventManager.RemoveListener<QuestEvent>(this);
        EventManager.RemoveListener<BuildCompleteEvent>(this);
    }

    public void OnScript(ScriptTriggerData trigger, Action exitCallBack = null, Action exitFirstCallBack = null)
    {
        if (trigger.SEQ <= 0)
        {
            string strTrigger = CacheUserData.GetString("SCRIPT_" + trigger.TYPE.ToString());
            List<string> triggers = strTrigger.Split(',').ToList();
            
            CacheUserData.SetString("SCRIPT_" + trigger.TYPE.ToString(), string.Join(",", triggers));
            scripter.SetData(trigger, exitCallBack, exitFirstCallBack);
        }
        else
        {
            User.Instance.UpdateSequence(User.Instance.UserData.Sequence + 1);
            scripter.SetData(trigger, exitCallBack, exitFirstCallBack);
        }
    }

#if UNITY_EDITOR
    public void StopScript()
    {
        scripter.StopScript();
    }
    public void NexstScript()
    {
        scripter.Next();
    }

    public void ExitScript()
    {
        scripter.ExitScript();
    }
#endif

    public void OnEvent(QuestEvent eventType)
    {
        if(eventType.e == QuestEvent.eEvent.QUEST_DONE)
        {
            foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.QUEST_CLEAR))
            {
                if (OnCheckQuestDone(sub, eventType))
                {
                    return;
                }
            }
            OnCheckQuestDone(Cur, eventType);
        }
    }

    public bool OnCheckQuestDone(ScriptTriggerData trigger, QuestEvent eventType)
    {
        if (trigger == null)
            return false;

        if (trigger.TYPE != ScriptTriggerType.QUEST_CLEAR)
            return false;
        if (trigger.TYPE_PARAM != eventType.eventQID)
            return false;
        OnScript(trigger);
        return true;
    }


    public bool OnAdventureEvent(IStateBase e, AdventureMachine machine)
    {
        ScriptTriggerData trigger = Cur;
        if (trigger == null)
            return false;

        switch (e)
        {
            case AdventureStateDragonMove:
            {
                if (machine.Data.Wave > 1)
                    return false;

                if (trigger.TYPE != ScriptTriggerType.STAGE_IN)
                    return false;

                AdventureStateDragonMove data = (AdventureStateDragonMove)e;
                if (machine.BaseData.KEY == trigger.TYPE_PARAM)
                {
                    OnScript(trigger);
                    return true;
                }
            }
            break;
            case AdventureStateEnd:
            {
                if (trigger.TYPE != ScriptTriggerType.STAGE_CLEAR)
                    return false;

                AdventureStateEnd data = (AdventureStateEnd)e;
                if (data.Data.State == eBattleState.Win)
                {
                    if (machine.BaseData.KEY == trigger.TYPE_PARAM)
                    {
                        OnScript(trigger);
                        return true;
                    }
                }
            }
            break;
            case AdventureStateBattle:
            {
                if (trigger.TYPE != ScriptTriggerType.STAGE_MONSTER_SHOW)
                    return false;

                AdventureStateBattle data = (AdventureStateBattle)e;
                foreach (var monster in machine.Data.DefenseDic.Values)
                {
                    if (monster.ID == trigger.TYPE_PARAM)
                    {
                        OnScript(trigger);
                        return true;
                    }
                }
            }
            break;
        }

        return false;
    }

    public void OnEvent(BuildCompleteEvent e)
    {
        if (e.eType == eBuildingState.CONSTRUCTING)
        {
            foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.CONSTRUCT_START))
            {
                if (OnCheckScript(sub, e))
                    return;
            }
        }
        if (e.eType == eBuildingState.NORMAL)
        {
            foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.CONSTRUCT_DONE))
            {
                if (OnCheckScript(sub, e))
                    return;
            }
        }

        OnCheckScript(Cur, e);
    }

    public bool OnCheckScript(ScriptTriggerData trigger, BuildCompleteEvent e)
    {
        if (trigger == null)
            return false;

        if ((trigger.TYPE == ScriptTriggerType.CONSTRUCT_START && e.eType == eBuildingState.CONSTRUCTING) ||
            (trigger.TYPE == ScriptTriggerType.CONSTRUCT_DONE && e.eType == eBuildingState.NORMAL))
            return false;

        BuildingLevelData levelData = BuildingLevelData.Get(trigger.TYPE_PARAM.ToString());
        if (levelData != null && levelData.BUILDING_GROUP == e.building.BName && levelData.LEVEL == e.building.Data.Level)
        {
            OnScript(trigger);
            return true;
        }

        return false;
    }
#if INTRO_FORCE
    bool intro_shown = false;
#endif
    public bool OnEventCheckFirstIntroStart(Action exitCB, Action exitFirstCB)
    {
        if (User.Instance.UserData.Exp <= 0
#if INTRO_FORCE
            || GameConfigTable.INTRO_FORCE
#endif
            )
        {
            if (!CacheUserData.GetBoolean("intro_show", false)
#if INTRO_FORCE
                || (GameConfigTable.INTRO_FORCE && !intro_shown)
#endif
                )
            {
#if INTRO_FORCE
                intro_shown = true;
#endif
                CacheUserData.SetBoolean("intro_show", true);

                foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.INTRO))
                {
                    OnScript(sub, exitCB, exitFirstCB);
                    return true;
                }                
            }
        }
        return false;
    }
    public bool OnEventCheckFirstStart(Action cb)
    {
        if(User.Instance.UserData.Exp <= 0)
        {
            if (!CacheUserData.GetBoolean("first_show", false))
            {
                foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.FRIST_INIT))
                {
                    CacheUserData.SetBoolean("first_show", true);
                    OnScript(sub, cb);
                    return true;
                }
            }
        }
        return false;
    }
    public bool OnEventCheckFirstArena(Action cb)
    {
        if (!CacheUserData.GetBoolean("arena_show", false))
        {
            foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.ARENA_FIRST_INIT))
            {
                CacheUserData.SetBoolean("arena_show", true);
                OnScript(sub, cb);
                return true;
            }
        }
        cb?.Invoke();
        return false;
    }

    public bool OnEventCheckFirstDailyDungeon(Action cb)
    {
        if (!CacheUserData.GetBoolean("dailydungeon_show", false))
        {
            foreach (var sub in ScriptTriggerData.GetTriggerList(ScriptTriggerType.DAILYDUNGEON_FIRST_INIT))
            {
                CacheUserData.SetBoolean("dailydungeon_show", true);
                OnScript(sub, cb);
                return true;
            }
        }
        cb?.Invoke();
        return false;
    }

    public void OnGuide(RectTransform guideArea, string text, Action callback = null)
    {
        if (guider != null)
            guider.OnGuide(guideArea, text, callback);
    }

    public void OnCloseGuide()
    {
        if (guider != null)
            guider.OnClose();
    }
}
