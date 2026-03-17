using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class StageInfo : MonoBehaviour
    {
        public List<StageNode> arrStageNode { get; private set; } = new List<StageNode>();
        List<StageRoad> StageRoad = new List<StageRoad>();
        [SerializeField] GameObject nodePole = null;
        [SerializeField] ScrollRect StageScroll = null;
        [SerializeField] RectTransform stageRootParent = null;
        [SerializeField] GameObject StagePrefab = null;
        [SerializeField] GameObject RoadPrefab = null;
        public ScrollRect CurScrollRect { get { return StageScroll; } }    

        public delegate void Callback(int worldIndex, int stageIndex);

        Callback StageNodeClickCallBack = null;
        Callback StageNodePointDownCallBack = null;
        Callback RoadClickCallBack = null;

        public WorldProgress InfoData { get; private set; } = null;

        bool isTutorial = false;

        public int TotalStar
        {
            get
            {
                return InfoData != null ? InfoData.GetAllStar() : 0;
            }
        }


        public void SetData(WorldProgress data, WorldCursor cursor)
        {
            if (data == null || cursor == null)
                return;
            StageScroll.horizontalNormalizedPosition = 0;
            InfoData = data;
            int targetStage = cursor.Stage;
            int stageCount = InfoData.Stages.Count;
            bool firstZero = false;
            int lastestPlayAbleStage = 0;
            isTutorial = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Adventure);

            for (int i = 0; i < stageCount; ++i)
            {
                int star = InfoData.Stages[i];
                if (i >= arrStageNode.Count)
                {
                    var stage = Instantiate(StagePrefab, stageRootParent);
                    arrStageNode.Add(stage.GetComponent<StageNode>());
                }
                bool isNextRoadOn = true;
                
                if (star > 0)//if (stageArr[i] > 0)
                {
                    arrStageNode[i].SetStage(CurScrollRect,false, false, InfoData.World, i + 1, InfoData.Diff, star);
                    isNextRoadOn = true;
                    lastestPlayAbleStage = i;
                }
                else if (!firstZero)
                {
                    firstZero = true;
                    arrStageNode[i].SetStage(CurScrollRect,true, false, InfoData.World, i + 1, InfoData.Diff, star);
                    isNextRoadOn = true;
                    lastestPlayAbleStage = i;
                }
                else
                {
                    arrStageNode[i].SetStage(CurScrollRect, false, true, InfoData.World, i + 1, InfoData.Diff, star);
                    isNextRoadOn = false;
                }
                if (isTutorial)
                {
                    var world = TutorialManager.tutorialManagement.GetCurTutoPrivateKey(0);
                    var stage = TutorialManager.tutorialManagement.GetCurTutoPrivateKey(1);
                    if(InfoData.World == world && i+1 == stage)
                    {
                        TutorialManager.Instance.SetRecordObject(800101,arrStageNode[i].GetComponent<RectTransform>());
                        TutorialManager.tutorialManagement.NextTutorialStart();
                    }
                }
                
                arrStageNode[i].SetClickCallBack((int worldIndex, int stageIndex) => {

                    foreach (var stage in arrStageNode)
                    {
                        stage.OffHighLight();
                    }
                    StageScroll.FocusOnItem(arrStageNode[stageIndex].GetComponent<RectTransform>(), SBDefine.AdventureLobbyScrollTime);
                    nodePole.SetActive(false);
                    arrStageNode[stageIndex].SetHighLight();
                    if(StageNodeClickCallBack != null)
                    {
                        StageNodeClickCallBack(worldIndex, stageIndex + 1);
                    }
                }, 
                (int worldIndex, int stageIndex) =>
                {
                    if (StageNodePointDownCallBack != null)
                    {
                        StageNodePointDownCallBack(worldIndex, stageIndex + 1);
                    }
                }
                );

                if (i < stageCount)
                {
                    if (StageRoad.Count <= i)
                    {
                        var road = Instantiate(RoadPrefab, arrStageNode[i].RoadParent).GetComponent<StageRoad>();
                        StageRoad.Add(road);
                    }
                    StageRoad[i].SetRoad(isNextRoadOn, InfoData.World, i, stageCount - 1 == i);
                    StageRoad[i].SetRoadClickCallBack((int worldIndex, int stageIndex) =>
                    {
                        if (RoadClickCallBack != null)
                        {
                            RoadClickCallBack(worldIndex, stageIndex + 1);
                        }
                    });
                    if (i ==stageCount - 1)
                    {
                        StageRoad[i].SetLastRoad();
                        
                    }
                }

            }

            int target = targetStage - 1;
            target = Mathf.Clamp(target, 0, arrStageNode.Count-1);
            if (arrStageNode[target] != null)
            {
                bool isTargetWorld = cursor.World == InfoData.World;
                bool isStageCheck = stageCount > 0 && stageCount > target;
                bool isNodeCheck = arrStageNode[target] != null && !arrStageNode[target].Lock;

                bool poleShow = true;
                var dragons = User.Instance.PrefData.AdventureFormationData.TeamFormation[0];
                foreach (var dragonTag in dragons)
                {
                    if (dragonTag != 0)
                    {
                        poleShow = false;
                        break;
                    }
                }
                nodePole.SetActive(isTargetWorld && poleShow);
                if (isTargetWorld && isStageCheck && isNodeCheck)
                {
                    nodePole.transform.SetParent(arrStageNode[target].NodeParent);
                    nodePole.transform.localPosition = Vector3.zero;

                    var targetRect = arrStageNode[target].GetComponent<RectTransform>();
                    if (targetRect != null)
                    {
                        StartCoroutine(ScrollToCenterAfterLayoutRebuild(targetRect));
                    }
                    if(StageNodePointDownCallBack != null)
                    {
                        StageNodePointDownCallBack(cursor.World, target + 1);
                    }
                    arrStageNode[target].SetHighLight();
                }
                else
                {
                    arrStageNode[lastestPlayAbleStage].SetHighLight();
                    StartCoroutine(ScrollToCenterAfterLayoutRebuild(arrStageNode[lastestPlayAbleStage].GetComponent<RectTransform>()));
                    if (StageNodePointDownCallBack != null)
                    {
                        StageNodePointDownCallBack(InfoData.World, lastestPlayAbleStage + 1);
                    }
                }

            }
        }


        public void SetStageClickCallBack(Callback ClickCallBack, Callback pointDownCallBack =  null, Callback roadClickCallBack = null)
        {
            if (ClickCallBack != null)
            {
                StageNodeClickCallBack = ClickCallBack;
            }
            if(pointDownCallBack != null)
            {
                 StageNodePointDownCallBack = pointDownCallBack;
            }
            if(roadClickCallBack != null)
            {
                RoadClickCallBack = roadClickCallBack;
            }
        }

        IEnumerator ScrollToCenterAfterLayoutRebuild(RectTransform targetRect) // 최초 접속시 layoutRebuild가 안되서 RectTransform 사이즈가 0임
        {                                                                       // ForcedImmediately 이후 호출해도 소용이 없어서 갱신부 변경
            yield return new WaitUntil(() => stageRootParent.childCount > 0);
            yield return new WaitUntil(() => targetRect.rect.size.x > 0);
            yield return null;
            StageScroll.FocusOnItem(targetRect, SBDefine.AdventureLobbyScrollTime);
        }



    }
}