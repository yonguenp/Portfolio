using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class QuestUIObject : UIObject, EventListener<QuestEvent>, EventListener<UIObjectEvent>
    {
        readonly string KEY_DROPDOWN = "QuestDropdown";
        //readonly int CLONE_HEIGHT = 64;
        readonly int MAX_VIEWHEIGHT = 500;

        [SerializeField] private GameObject qIconClone;
        [SerializeField] private Animation anim = null;

        private ScrollRect qIconScrollView;
        private List<GameObject> listQuestIcon;

        private Coroutine routine = null;
        private bool highlighted = false;

        private bool isFolded = false;
        public int dropdown { get { return 1;/*return PlayerPrefs.GetInt(KEY_DROPDOWN, 1);*/ } }
        public GameObject TopQuestIcon { get { return listQuestIcon != null && listQuestIcon.Count > 0 ? listQuestIcon[0] : null; } }
        private bool isInit = false;

        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);

            if (curUIType == targetType)
            {
                Init();
            }
        }

        public override void Init()
        {
            base.Init();

            qIconScrollView = GetComponentInChildren<ScrollRect>(true);
            //qIconScrollView.gameObject.SetActive(dropdown == 1);
            if (listQuestIcon == null)
            {
                listQuestIcon = new List<GameObject>();
            }

            EventManager.AddListener<QuestEvent>(this);
            EventManager.AddListener<UIObjectEvent>(this);

            highlighted = false;
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (routine == null && curUIType == curSceneType)
            {
                routine = StartCoroutine(QuestHighlight());
            }

            if (qIconClone == null)
            {
                qIconClone = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "questIcon");
            }

            if (!isInit)
            {
                isInit = true;
                RefreshIcon(curUIType);
            }
        }

        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshIcon(targetType);
                CheckReddot();
            }

            return curSceneType != targetType;
        }

        public override void ShowEvent()
        {
            base.ShowEvent();
            RefreshIcon(curSceneType);
            CheckReddot();

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (routine == null && curUIType == curSceneType)
            {
                routine = StartCoroutine(QuestHighlight());
            }
        }

        public override void HideEvent()
        {
            base.HideEvent();

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }


        private void RefreshIcon(eUIType targetType, bool doneEvent = false)
        {
            if (!isInit || curUIType != targetType)
                return;

            List<int> marked = QuestManager.Instance.GetQuestMarked();
            int needCall = marked.Count;

            //23.1 까지만 사용.
            while (listQuestIcon.Count != needCall)
            {
                if (listQuestIcon.Count > needCall)
                {
                    //파괴
                    Destroy(listQuestIcon[listQuestIcon.Count - 1]);
                    listQuestIcon.RemoveAt(listQuestIcon.Count - 1);
                }
                else
                {
                    //생성
                    GameObject clone = Instantiate(qIconClone, qIconScrollView.content);
                    listQuestIcon.Add(clone);
                }
            }

            if (needCall > 0)
            {
                for (int i = 0; i < needCall; i++)
                {
                    QuestIcon qIcon = listQuestIcon[i].GetComponent<QuestIcon>();
                    qIcon.Init(marked[i]);
                    qIcon.gameObject.SetActive(true);

                    QuestData qData = QuestData.Get(marked[i].ToString());
                    Quest quest = QuestManager.Instance.GetQuest(marked[i]);

                    if (qData.IS_START_POPUP && quest.IsNewQuest() && doneEvent)
                    {
                        if (ScenarioManager.Instance.IsPlaying)
                            continue;

                        qIcon.OnClickQuestIcon(true);
                    }
                }
            }

            //Vector2 size = new Vector2(qIconScrollView.GetComponent<RectTransform>().sizeDelta.x, Mathf.Min(MIN_VIEWHEIGHT + (CLONE_HEIGHT * needCall), MAX_VIEWHEIGHT));
            //Vector2 size = new Vector2(qIconScrollView.GetComponent<RectTransform>().sizeDelta.x, MAX_VIEWHEIGHT);
            //qIconScrollView.viewport.sizeDelta = size;
            qIconScrollView.verticalNormalizedPosition = 1;
            qIconScrollView.vertical = qIconScrollView.content.sizeDelta.y >= qIconScrollView.viewport.sizeDelta.y;
        }

        public void OnClickQuestDropdown()
        {
            //dropdown = Mathf.Abs((dropdown - 1));
            isFolded = dropdown == 0;
            //qIconScrollView.gameObject.SetActive(dropdown == 1);
            var x = qIconScrollView.GetComponent<RectTransform>().sizeDelta.x;
            qIconScrollView.viewport.DOSizeDelta(dropdown == 1 ? new Vector2(x, MAX_VIEWHEIGHT) : new Vector2(x, -.5f), 0.5f);
            PlayerPrefs.SetInt(KEY_DROPDOWN, dropdown);
        }

        public void OnClickQuestIcon()
        {
            PopupManager.OpenPopup<MissionPopup>(new TabTypePopupData(0, 0));
        }

        public void OnEvent(QuestEvent eventType)
        {
            switch (eventType.e)
            {
                case QuestEvent.eEvent.QUEST_DONE:
                case QuestEvent.eEvent.QUEST_UPDATE:
                    RefreshIcon(curSceneType, eventType.e == QuestEvent.eEvent.QUEST_DONE);
                    CheckReddot();
                    break;

                case QuestEvent.eEvent.QUEST_OPEN:
                    if (listQuestIcon != null && listQuestIcon.Count > 0)
                    {
                        TopQuestIcon?.GetComponent<QuestIcon>().OnClickQuestIcon();
                    }
                    break;
                case QuestEvent.eEvent.QUEST_REQUEST_REFRESH:
                    CheckReddot();
                    break;
                case QuestEvent.eEvent.TUTORIAL_QUEST_OPEN:
                    if (listQuestIcon != null && listQuestIcon.Count > 0)
                    {
                        List<int> marked = QuestManager.Instance.GetQuestMarked();
                        Quest quest = QuestManager.Instance.GetQuest(marked[0]);
                        if (quest.IsNewQuest())
                            TopQuestIcon?.GetComponent<QuestIcon>().OnClickQuestIcon(true);
                    }
                    break;
            }
        }

        public void OnEvent(UIObjectEvent eventType)
        {
            if ((eventType.t & UIObjectEvent.eUITarget.RM) != UIObjectEvent.eUITarget.NONE)
            {
                switch (eventType.e)
                {
                    case UIObjectEvent.eEvent.EVENT_SHOW:
                        anim.Play("R_Show");
                        break;

                    case UIObjectEvent.eEvent.EVENT_HIDE:
                        anim.Play("R_Hide");
                        break;
                }
            }
        }

        public void OnHighlightImmediate()
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(QuestHighlight(true));
        }

        IEnumerator QuestHighlight(bool Immediate = false)
        {
            float delay = 3.0f;
            float time = delay;
            highlighted = false;

            if (TutorialManager.tutorialManagement.IsPlayingTutorial)
            {
                if (listQuestIcon.Count > 0 && TopQuestIcon != null)
                    TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Idle");
                yield break;
            }


            while (true)
            {
                if (listQuestIcon == null || listQuestIcon.Count <= 0 || TopQuestIcon == null)
                {
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                if (highlighted == true && qIconScrollView.verticalNormalizedPosition != 1.0f && !Immediate)
                {
                    time = delay;
                    TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Idle");
                    //TopQuestIcon.transform.DOKill();
                    //TopQuestIcon.transform.localScale = Vector3.one;
                }
                else if ((!Input.anyKey && !isFolded) || Immediate)
                {
                    time -= SBGameManager.Instance.DTime;

                    if ((time < 0f && !highlighted) || Immediate)
                    {
                        QuestIcon qIcon = TopQuestIcon.GetComponent<QuestIcon>();

                        var marked = QuestManager.Instance.GetQuestMarked();
                        if (marked == null || marked.Count <= 0)
                        {
                            time = delay;
                            TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Idle");
                            continue;
                        }

                        QuestData qData = QuestData.Get(marked[0].ToString());
                        if (qData == null
                            || qData.TYPE != eQuestType.MAIN
                            || qData.KEY == CacheUserData.GetInt("LastOpenedMainQuest", 0))
                        {
                            time = delay;
                            TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Idle");
                            continue;
                        }

                        highlighted = true;
                        qIconScrollView.verticalNormalizedPosition = 1.0f;
                        TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Highlight");
                        //TopQuestIcon.transform.DOKill();
                        //TopQuestIcon.transform.localScale = Vector3.one;
                        //TopQuestIcon.transform.DOScale(Vector3.one * 1.1f, 0.75f).SetLoops(-1, LoopType.Yoyo);

                        yield return new WaitUntil(() => Input.anyKey && !Immediate);
                    }
                }
                else
                {
                    time = delay;
                    TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Idle");
                    //TopQuestIcon.transform.DOKill();
                    //TopQuestIcon.transform.localScale = Vector3.one;
                }

                highlighted = false;
                yield return new WaitForFixedUpdate();
            }
        }

        static public void CheckReddot()
        {
            ReddotManager.Set(eReddotEvent.DAILY_QUEST, IsQuestReddotCondition(eQuestGroup.Normal, eQuestType.DAILY));
            ReddotManager.Set(eReddotEvent.BATTLE_PASS, IsQuestReddotCondition(eQuestGroup.Normal, eQuestType.BATTLE_PASS));
            ReddotManager.Set(eReddotEvent.HOLDER_PASS, IsQuestReddotCondition(eQuestGroup.Normal, eQuestType.HOLDER_PASS));
            ReddotManager.Set(eReddotEvent.GUILD_MISSION, IsGuildQuestReddotCondition());
            ReddotManager.Set(eReddotEvent.GUILD_MISSION_DAILY, IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.DAILY));
            ReddotManager.Set(eReddotEvent.GUILD_MISSION_WEEKLY, IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.WEEKLY));
            ReddotManager.Set(eReddotEvent.GUILD_MISSION_CHAIN, IsQuestReddotCondition(eQuestGroup.Guild, eQuestType.CHAIN));
        }
        static bool IsGuildQuestReddotCondition()
        {
            var ProceedQuest = QuestManager.Instance.GetTotalQuestDataByGroup(eQuestGroup.Guild);
            if (ProceedQuest.Count <= 0)
                return false;

            foreach (var quest in ProceedQuest)
            {
                if (quest == null)
                    continue;
                if (IsGetRewardCondition(quest))
                    return true;
            }
            return false;
        }

        public static bool IsQuestReddotCondition(eQuestGroup questGroup, eQuestType questType)
        {
            var dailyProceedQuest = QuestManager.Instance.GetProceedUIData(questType, questGroup);
            if (dailyProceedQuest.Count <= 0)
                return false;

            foreach (var quest in dailyProceedQuest)
            {
                if (quest == null)
                    continue;
                if (IsGetRewardCondition(quest))
                    return true;
            }
            return false;
        }

        static bool IsGetRewardCondition(Quest _quest)//보상을 받을 수 있는 상태인가?
        {
            if (_quest == null)
                return false;

            if (_quest.State == eQuestState.TERMINATE || _quest.State == eQuestState.PROCESS_DONE)//이미 완료한 퀘스트
                return false;

            if (_quest.IsQuestClear())//진행 중인 퀘스트만 체크
                return true;

            return false;
        }

        public void SetQuestHighlight(bool state)
        {
            if (gameObject.activeInHierarchy == false)
                return;
            if (state)
            {
                if (routine != null)
                    StopCoroutine(routine);
                routine = StartCoroutine(QuestHighlight());
            }
            else
            {
                if (routine != null)
                    StopCoroutine(routine);

                if (listQuestIcon != null && listQuestIcon.Count > 0)
                    TopQuestIcon.GetComponent<Animation>().Play("QuestIcon_Idle");
            }

        }
    }
}