using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class StageScene : MonoBehaviour
    {
        [SerializeField] StageSceneUI stageUI = null;
        
        Dictionary<StageDifficult, List<StageInfo>> stageInfos = new Dictionary<StageDifficult, List<StageInfo>>();
        [SerializeField] List<StageInfo> normalStage = new List<StageInfo>();
        [SerializeField] List<StageInfo> hardStage = new List<StageInfo>();
        [SerializeField] List<StageInfo> hellStage = new List<StageInfo>();

        [SerializeField] RectTransform DragonParent;
        [SerializeField] Transform DragonLeftTr;
        [SerializeField] Transform DragonRightTr;

        [Header("Monster")]
        [SerializeField] RectTransform MonsterParent;
        [SerializeField] private GameObject MonsterBubbleObj = null;
        //[SerializeField] private Color[] bossBgColors = null;
        [SerializeField] private GameObject MonsterGroup;

        [Header("back Dimmed")]
        [SerializeField] private Image backDimImg = null;
        [SerializeField] private Color[] backDimColor = null;
        public List<StageInfo> StageInfoNode { get { return stageInfos[(StageDifficult)CacheUserData.GetInt("adventure_difficult", 1)]; } }
        public StageInfo CurInfo { get; private set; } = null;
        public WorldProgress StageInfo { get; private set; } = null;
        private WorldCursor cursor = null;
        private int minWorld = -1;
        private int maxWorld = -1;
        private int lastSelectedWorld = 0;
        private int lastStage = 1;
        private List<AdventureLobbyDragonSpine> dragonSpineList = new List<AdventureLobbyDragonSpine>();
        private Transform dragonMoveTargetTransform = null;
        private int LastDragonTargetWorld = 0;
        private int LastDragonTargetStage = 0;
        private ScrollRect targetScrollRect =   null;
        private List<GameObject> bossBubbles = new List<GameObject>();
        float prevPosX = 0f;

        private void Awake()
        {
            stageInfos.Add(StageDifficult.NORMAL, normalStage);
            stageInfos.Add(StageDifficult.HARD, hardStage);
            stageInfos.Add(StageDifficult.HELL, hellStage);
        }
        private void Start()
        {
            Init();
            Refresh();
        }

        void Init()
        {
            minWorld = 1;
            maxWorld = GameConfigTable.GetLastWorld();
            
            var cursor = StageManager.Instance.AdventureProgressData.WorldCuror;
            var targetWorld = cursor.World;
            var targetStage = cursor.Stage;
            var targetDiff = cursor.Diff;
            
            CacheUserData.SetInt("adventure_difficult", cursor.Diff);

            StageManager.Instance.SetAll(targetWorld, targetStage, targetDiff);//최초 커서 기준 World 및 stage 인덱스 세팅

            var stageInformation = StageManager.Instance.AdventureProgressData.GetWorldInfoData(targetWorld, targetDiff);
            if (stageInformation != null)
            {
                bool isLastStage = stageInformation.Stages.Count == targetStage;
                bool isStageClear = stageInformation.Stages[targetStage - 1] > 0;
                bool isLastWorld = targetWorld == maxWorld;
                if (!isLastWorld && isLastStage && isStageClear)
                {
                    var tempInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(targetWorld + 1, targetDiff);
                    if (tempInfo != null)
                    {
                        stageInformation = tempInfo;
                        targetWorld += 1;
                        targetStage = 1;
                    }
                }
                else if (!isLastStage && isStageClear)
                {
                    targetStage += 1;
                }
            }
            StageInfo = stageInformation;

            if (this.cursor == null)
                this.cursor = new WorldCursor(targetWorld, targetStage, targetDiff);
            else
                this.cursor.SetData(targetWorld, targetStage, targetDiff);

            UIManager.Instance.InitUI(eUIType.Adventure);
            UIManager.Instance.RefreshUI(eUIType.Adventure);
            int lastEnterWorld = StageManager.Instance.LastEnterWorld;
            InitDragonSpines();
            MonsterBubbleObj.transform.SetAsFirstSibling();
            if (lastEnterWorld > 0 && lastEnterWorld <= StageManager.Instance.AdventureProgressData.GetLatestWorld())
            {
                ChangeWorld(lastEnterWorld, 1);
                StageManager.Instance.SetLastEnterWorld(0);
            }
            foreach (var stage in StageInfoNode)
            {
                stage.SetStageClickCallBack(SetDragonToNode, SetDragonMoveToNode, SetDragonMoveToRoad);
            }
            SetBossBubble();
            PopupManager.GetPopup<AdventureReadyPopup>().SetDragonSaveCallBack(DragonChange); // 드래곤 변경 후 팝업 닫았을때 이 씬의 드래곤도 바뀌도록 하는 처리
            PopupManager.GetPopup<AdventureReadyPopup>().SetStageStartCallBack(ClearDragons); 
            lastSelectedWorld = targetWorld;
        }

        void SetDragonMoveToNode(int worldIndex, int stageIndex)
        {
            dragonMoveTargetTransform = StageInfoNode[worldIndex-1].arrStageNode[stageIndex-1].NodeParent;
            if (dragonMoveTargetTransform == null) return;
            if (worldIndex < LastDragonTargetWorld || (worldIndex == LastDragonTargetWorld && stageIndex < LastDragonTargetStage)) // 드래곤이 가장 괜찮게 보이도록 정렬하기 위해 사용
            {
                for (int i = dragonSpineList.Count - 1; i >= 0; --i)
                {
                    dragonSpineList[i].SetTargetTransform(dragonMoveTargetTransform);  
                }
            }
            else
            {
                foreach (var dragon in dragonSpineList)
                {
                    dragon.SetTargetTransform(dragonMoveTargetTransform);
                }
            }
            LastDragonTargetWorld = worldIndex;
            LastDragonTargetStage = stageIndex;
        }
        void SetDragonMoveToRoad(int worldIndex, int stageIndex)
        {
            dragonMoveTargetTransform = StageInfoNode[worldIndex - 1].arrStageNode[stageIndex - 1].RoadParent;
            if (worldIndex < LastDragonTargetWorld || (worldIndex == LastDragonTargetWorld && stageIndex < LastDragonTargetStage)) // 드래곤이 가장 괜찮게 보이도록 정렬하기 위해 사용
            {
                for (int i = dragonSpineList.Count - 1; i >= 0; --i)
                {
                    dragonSpineList[i].SetTargetTransform(dragonMoveTargetTransform);
                }
            }
            else
            {
                foreach (var dragon in dragonSpineList)
                {
                    dragon.SetTargetTransform(dragonMoveTargetTransform);
                }
            }
            LastDragonTargetWorld = worldIndex;
            LastDragonTargetStage = stageIndex;
        }

        void SetDragonToNode(int worldIndex, int stageIndex)
        {
            dragonMoveTargetTransform = StageInfoNode[worldIndex - 1].arrStageNode[stageIndex - 1].NodeParent;
            foreach (var dragon in dragonSpineList)
            {
                dragon.TeleportTargetPos(dragonMoveTargetTransform);
            }
            
            LastDragonTargetWorld = worldIndex;
            LastDragonTargetStage = stageIndex;
        }


        void Refresh()
        {
            int myLastWorld = StageManager.Instance.AdventureProgressData.GetLatestWorld(CacheUserData.GetInt("adventure_difficult", 1));
            if (StageInfo != null)
            {
                int curWorld = StageInfo.World;
                var diffKeys = stageInfos.Keys;
                StageDifficult curDiff = (StageDifficult)CacheUserData.GetInt("adventure_difficult", 1);
                foreach (var key in diffKeys)
                {
                    for (int i = 0; i < stageInfos[key].Count; i++)
                    {
                        if (StageInfoNode[i] == null)
                            continue;

                        bool isTarget = curWorld == (i + 1) && key == curDiff;
                        if (isTarget)
                            CurInfo = stageInfos[key][i];

                        stageInfos[key][i].gameObject.SetActive(false);
                    }
                }

                if (stageUI != null)
                {
                    if (maxWorld == minWorld || minWorld >= myLastWorld) //화살표 다끄기
                    {
                        stageUI.SetVisibleLeftArrow(false);
                        stageUI.SetVisibleRightArrow(false);
                    }
                    else if (curWorld >= myLastWorld || curWorld >= maxWorld) //오른쪽 화살표 끄기
                    {
                        stageUI.SetVisibleLeftArrow(true);
                        stageUI.SetVisibleRightArrow(false);
                    }
                    else if (minWorld >= curWorld) //왼쪽 화살표 끄기
                    {
                        stageUI.SetVisibleLeftArrow(false);
                        stageUI.SetVisibleRightArrow(true);
                    }
                    else//다켜기
                    {
                        stageUI.SetVisibleLeftArrow(true);
                        stageUI.SetVisibleRightArrow(true);
                    }
                }
            }

            if (CurInfo != null)
            {
                CurInfo.gameObject.SetActive(true);
                CurInfo.SetData(StageInfo, cursor);
                targetScrollRect = CurInfo.CurScrollRect;
                
            }

            if (stageUI != null)
            {
                stageUI.SetData(this, StageInfo);

                stageUI.RefreshQuestDirectWorld();
            }
            
        }
        public void OnClickWorldLeft()
        {
            int world = StageInfo.World;
            int myLastWorld = StageManager.Instance.AdventureProgressData.GetLatestWorld();
            if (minWorld == myLastWorld)
            {
                ToastManager.On(100002422);
                return;
            }
            else if (world <= minWorld)
            {
                ChangeWorld(myLastWorld, StageInfo.Diff);
                return;
            }
            ChangeWorld(world - 1, StageInfo.Diff);
            
        }
        public void OnClickWorldRight()
        {
            int world = StageInfo.World;
            int myLastWorld = StageManager.Instance.AdventureProgressData.GetLatestWorld();
            if (minWorld == myLastWorld)
            {
                ToastManager.On(100002422);
                return;
            }
            else if (world >= myLastWorld)
            {
                ChangeWorld(minWorld, StageInfo.Diff);
                return;
            }
            ChangeWorld(world + 1, StageInfo.Diff);
        }

        public bool ChangeWorld(int worldIndex, int diff)
        {
            bool worldAvailableCheck = StageManager.Instance.AdventureProgressData.isAvailableWorld(worldIndex, diff);
            if (worldAvailableCheck == false)
            {
                ToastManager.On(100000628);
                return false;
            }
            StageManager.Instance.SetWorld(worldIndex);//현재 선택 월드 인덱스 저장
            StageInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(worldIndex, diff);
            
            if (worldIndex > lastSelectedWorld)
            {
                SetDefaultDragonPos();
                SetBossBubble(); // 정말로 월드가 바꼈을때 갱신
            }
            else if (worldIndex < lastSelectedWorld)
            {
                SetLastDragonPos();
                SetBossBubble();
            }

            Refresh();
            if (backDimImg != null) {
                backDimImg.color = backDimColor[worldIndex - 1];
            }
            
            lastSelectedWorld = worldIndex;

            ScrollMove();
            CancelInvoke("DragonFocus");
            return true;
        }

        void SetBossBubble()
        {
            foreach (Transform child in MonsterParent)
            {
                if (child == MonsterGroup.transform)
                    continue;

                if (child == MonsterBubbleObj.transform)
                    continue;

                if(child != null)
                    Destroy(child.gameObject);
            }
            bossBubbles.Clear();

            if (MonsterParent == null)
                return;

            MonsterParent.sizeDelta = new Vector2(UICanvas.Instance.GetCanvasRectTransform().sizeDelta.x, 0);

            int world = StageInfo.World;
            lastStage = StageInfo.Stages.Count;

            MonsterBubbleObj.SetActive(true);
            
            for (int i = 1; i <= lastStage; ++i)
            {
                var star = StageInfo.Stages[i - 1];
                GameObject bubblegroup = Instantiate(MonsterGroup, MonsterParent);
                bubblegroup.transform.SetAsFirstSibling();
                var spawnData = StageBaseData.GetByAdventureWorldStage(world, i).SPAWN; 

                var monsters = MonsterSpawnData.GetBySpawnGroup(spawnData);
                List<int> checker = new List<int>();
                foreach (var monster in monsters)
                {
                    if (checker.Contains(monster.MONSTER))
                        continue;
                    
                    if (monster.IS_BOSS == 1)
                    {
                        checker.Add(monster.MONSTER);

                        var bubble = Instantiate(MonsterBubbleObj, bubblegroup.transform);                        
                        bool even = (checker.Count % 2) == 0;
                        bubble.transform.localPosition = new Vector3((checker.Count - 1) * (even ? 50f : -50f), 0f,0f);

                        if(even)
                            bubble.transform.SetAsFirstSibling();
                        else
                            bubble.transform.SetAsLastSibling();

                        var comp = bubble.GetComponent<BossBubble>();
                        if(comp != null)
                        {   
                            //var imageKey = MonsterBaseData.Get(monster.MONSTER.ToString()).THUMBNAIL;
                            //comp.SetData(ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, imageKey), bossBgColors[world - 1]);
                            var monsterData = MonsterBaseData.Get(monster.MONSTER.ToString());
                            if (monsterData != null)
                            {
                                var prefabPath = monsterData.IMAGE;
                                comp.SetData(world, prefabPath, i == lastStage, monster.IS_BOSS > 0, star >= 3);
                            }
                        }
                    }
                }
                if (checker.Count % 2 == 0)
                {
                    foreach(Transform child in bubblegroup.transform)
                    {
                        child.localPosition += new Vector3(25f, 0f, 0f); 
                    }
                }

                bossBubbles.Add(bubblegroup);
            }

            MonsterBubbleObj.SetActive(false);

            ScrollMove();
        }

        void InitDragonSpines()
        {
            int currentTeamPresetNo = CacheUserData.GetInt("presetAdventureDeck", 0);
            var dragons = User.Instance.PrefData.AdventureFormationData.TeamFormation[currentTeamPresetNo];
            if (DragonParent == null) 
                return;

            DragonParent.sizeDelta = new Vector2(UICanvas.Instance.GetCanvasRectTransform().sizeDelta.x, 0); 
            foreach(var dragonTag in dragons)
            {
                if(dragonTag != 0) {
                    var myDragonData = User.Instance.DragonData.GetDragon(dragonTag);                    
                    var baseData = myDragonData.BaseData;
                    var spineObject = Instantiate(baseData.GetUIPrefab(), DragonParent);
                    UIDragonSpine uispine = spineObject.GetComponent<UIDragonSpine>();
                    var spine = spineObject.AddComponent<AdventureLobbyDragonSpine>();
                    spine.SetTranscendParent(uispine.TranscendParent);
                    spine.SetSpineRaycastTargetState(false);
                    spine.transform.localScale = Vector3.one* 1.4f;
                    spine.SetData(myDragonData);
                    spine.SetParentTr(DragonParent.transform);
                    dragonSpineList.Add(spine);
                    spine.Init();

                    Destroy(uispine);
                }
            }
            for (int i = 0, max = dragonSpineList.Count; i < max; ++i)
            {
                dragonSpineList[i].SetOrder(i,max);                
            }

            SetDefaultDragonPos();
        }

        public void ClearDragons()
        {
            if(dragonSpineList != null)
            {
                foreach(var spine in dragonSpineList)
                {
                    Destroy(spine.gameObject);
                }
                dragonSpineList.Clear();
            }
        }

        public void DragonChange()
        {
            ClearDragons();
            InitDragonSpines();
            SetDragonMoveToNode(LastDragonTargetWorld, LastDragonTargetStage);
            if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Adventure))
            {
                TutorialManager.tutorialManagement.NextTutorialStart();
            }
        }

        void SetDefaultDragonPos() //화면상 가장 왼쪽 끝 위치 // 
        {
            foreach (var dragon in dragonSpineList)
            {
                dragon.transform.SetParent(DragonLeftTr);
                dragon.transform.localPosition = Vector3.zero;
            }
        }
        void SetLastDragonPos()  // 2월드에 있던 드래곤들이 1월드로 돌아가는 경우에 오른쪽 끝 부분에서 위치 시작하기 위한 용도
        {
            foreach (var dragon in dragonSpineList)
            {
                dragon.transform.SetParent(DragonRightTr); //스크롤링 연출에 의해 포지션이 변경되지 않도록 부모 변경
                dragon.transform.localPosition = Vector3.zero;
                dragon.SetDragonWaitScrolling(true); //행동은 스크롤링이 끝나고 움직이도록 함 - 자연스러운 연출 용도
            }                                                      
        }                                                          


        public void OnScrollValueChanged()
        {
            CancelInvoke("DragonFocus");
            Invoke("DragonFocus", SBDefine.AdventureLobbyScrollTime);

            ScrollMove();
        }

        void ScrollMove()
        {
            if (targetScrollRect == null)
                return;

            var curPosX = targetScrollRect.content.anchoredPosition.x;
            DragonParent.anchoredPosition += new Vector2(curPosX - prevPosX, 0);
            MonsterParent.anchoredPosition += new Vector2(curPosX - prevPosX, 0);
            prevPosX = curPosX;

            for(int i = 0; i < bossBubbles.Count; i++)
            {
                var bubble = bossBubbles[i];
                if (bubble != null)
                {
                    if(StageInfoNode[StageInfo.World - 1].arrStageNode.Count > i)
                        bubble.transform.position = StageInfoNode[StageInfo.World - 1].arrStageNode[i].NodeParent.position;
                }
            }            
        }

        void DragonFocus()
        {
            CancelInvoke("DragonFocus");

            bool outPos = false;
            bool left = false;
            var uiCam = UIManager.Instance.UICamera;
            foreach (var dragon in dragonSpineList)
            {
                if (dragon.IsMoving())
                {
                    Invoke("DragonFocus", 0.1f);
                    return;
                }
            }

            foreach (var dragon in dragonSpineList)
            { 
                Vector3 pos = uiCam.WorldToViewportPoint(dragon.transform.position);
                if (pos.x < 0f)
                {
                    outPos = true;
                    left = true;
                    break;
                }

                if(pos.x > 1f)
                {
                    outPos = true;
                    left = false;
                    break;
                }
            }

            if (outPos)
            {
                List<StageNode> nodes = new List<StageNode>();
                foreach(var node in CurInfo.arrStageNode)
                {
                    if (!node.Lock)
                        nodes.Add(node);
                }

                if (!left)
                    nodes.Reverse();

                bool targeting = false;
                StageNode target = null;
                foreach (var stage in nodes)
                {
                    if(targeting)
                    {
                        target = stage;
                        break;
                    }

                    RectTransform rt = stage.transform as RectTransform;
                    Vector3 pos = uiCam.WorldToViewportPoint(stage.transform.position);
                    if (pos.x > 0f && pos.x < 1f)
                    {
                        if (!left)
                        {
                            target = stage;
                            break;
                        }

                        targeting = true;
                    }
                }

                if(target != null)
                {
                    SetDragonMoveToNode(target.worldNo, target.stageNo);
                }
            }

        }
    }
}