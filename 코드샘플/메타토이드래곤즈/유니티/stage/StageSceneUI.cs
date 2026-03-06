using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class StageSceneUI : MonoBehaviour
    {
        [SerializeField] private Text worldNameLabel = null;
        [SerializeField] private Slider progStarReward = null;
        [SerializeField] private List<StarReward> starRewards = new List<StarReward>();
        [SerializeField] private GameObject worldSelectPopup;
		[SerializeField] private GameObject btnReawrd;
        [SerializeField] private GameObject leftArrow;
        [SerializeField] private GameObject rightArrow;
        [SerializeField] private Transform[] BoxesTrs;
        [SerializeField] private GameObject[] boxRewardEffectObjs;
        [SerializeField] private Dropdown difficultDropdown;
        [SerializeField] private Image difficultImage;
        [SerializeField] private Sprite[] difficultIcon;
        
        [SerializeField] private Image difficultSliderImage;
        [SerializeField] private Sprite[] difficultSliderSprite;

        [SerializeField] private GameObject[] difficultyButtonObjs = null; // 구체적으로 나오지 않아 임시 코드 
        private GameObject SelectedDiffObj = null;
        List<Tween> BoxTween =  null;
        private StageScene stageScene = null;
        private int maxStarAmount = 24;
        private int curStarAmount = 0;
        public WorldProgress StageInfo { get; private set; } = null;
        private string WorldIdx { get { return StageInfo != null ? (1000 + StageInfo.World).ToString() : "1"; } }

        bool isDifficultShowState = false;

        private Coroutine RewardPopupOpenCor = null;

        private bool isNetworkState = false;
        public void SetData (StageScene StageScene, WorldProgress stagedata)
        {
            StageInfo = stagedata;
            stageScene = StageScene;
            Refresh();
            isNetworkState = false;
        }
        public void Refresh()
        {
            worldSelectPopup.SetActive(false);
            RefreshWorldStarUI();
            RefreshWorldName();
        }
        private void Start()
        {
            UIManager.Instance.InitUI(eUIType.Adventure);
            UIManager.Instance.RefreshUI(eUIType.Adventure);
            ClearBoxEffectObj();
            if (difficultyButtonObjs != null && difficultyButtonObjs.Length>0)
            {
                SelectedDiffObj = difficultyButtonObjs[0];
            }
            for(int i = 0; i < difficultyButtonObjs.Length; i++)
            {
                if (difficultyButtonObjs[i] == SelectedDiffObj)
                {
                    SelectedDiffObj.SetActive(true);
                }
                else { 
                    difficultyButtonObjs[i].SetActive(false);
                }
            }            
        }
        void RefreshWorldStarUI()
        {
            var options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData(StringData.GetStringByStrKey("보통난이도")));
            options.Add(new Dropdown.OptionData(StringData.GetStringByStrKey("어려움난이도")));
            options.Add(new Dropdown.OptionData(StringData.GetStringByStrKey("지옥난이도")));

            var difficult = CacheUserData.GetInt("adventure_difficult", 1);
            difficultDropdown.options = options;
            difficultDropdown.value = difficult - 1;
            difficultImage.sprite = difficultIcon[difficult];
            difficultSliderImage.sprite = difficultSliderSprite[difficult - 1];

            var targetData = WorldData.Get(WorldIdx);
            if (targetData == null) return;

            maxStarAmount = StageBaseData.GetByAdventureWorld(StageInfo.World).Count *3;

            curStarAmount = 0;

            if(StageInfo.Stages != null)
            {
                if (StageInfo.Stages.Count > 0)
                {
                    foreach(int elem in StageInfo.Stages)
                    {
                        curStarAmount += elem;
                    }
                }
            }
            progStarReward.maxValue = maxStarAmount;
            progStarReward.value = curStarAmount;
            

            List<int> worldStarReward = targetData.STAR;

			bool canReceive = false;
            if(BoxTween == null)
            {
                BoxTween = new List<Tween>();
            }
            else
            {
                foreach(var elem in BoxTween)
                {
                    if (elem == null) continue;
                    elem.Kill();
                }
                foreach(var tr in BoxesTrs)
                {
                    tr.localScale = Vector3.one;
                }
            }

            for( int i = 0; i < starRewards.Count; ++i)
            {
                if (starRewards[i] == null) continue;
                var rewarded = StageInfo.Rewarded[i];
                starRewards[i].SetData(worldStarReward[i], rewarded, maxStarAmount, targetData.STAR_REWARD[i]);

                starRewards[i].SetStarIcon(difficultIcon[difficult]);

                canReceive |= rewarded == 1;
                if(rewarded == 1) //  받을수 있는 보상이 있다면
                {
                    BoxTween.Add(BoxesTrs[i].DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1,LoopType.Yoyo));
                }

            }

			btnReawrd.SetActive(canReceive);
        }
        
        public void OnDifficultChange()
        {
            if (stageScene == null)
                return;

            int difficult = difficultDropdown.value + 1;
            if (IsSelectableDifficult(difficult))
            {
                CacheUserData.SetInt("adventure_difficult", difficult);
                int world = StageManager.Instance.AdventureProgressData.GetLatestWorld(difficult);        
                stageScene.ChangeWorld(world, difficult);
            }
            else
            {
                difficultDropdown.value = CacheUserData.GetInt("adventure_difficult", 1) - 1;
                ToastManager.On(100000628);
            }

            difficultImage.sprite = difficultIcon[difficult];
        }

        bool IsSelectableDifficult(int diff)
        {
            //유저가 선택가능한 diff인지 확인 추가할것
            switch (diff)
            {
                case 1:
                    return true;

                case 2:
                {
                    var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(8, 1);
                    if (worldInfo != null && worldInfo.GetLastestClearStage() > 0)
                    {
                        return worldInfo.IsWorldClear();
                    }
                    return false;
                }
                case 3:
                {
                    var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(8, 2);
                    if (worldInfo != null && worldInfo.GetLastestClearStage() > 0)
                    {
                        return worldInfo.IsWorldClear();
                    }
                    return false;
                }
                default:
                    return false;
            }
        }
        void RefreshWorldName()
        {
            if(worldNameLabel != null)
            {
                WorldData worldData = WorldData.Get(WorldIdx);
                if(worldData != null)
                {
                    worldNameLabel.text = StringData.GetStringByIndex(worldData._NAME);
                }
            }
        }


        public void OnClickStarReward()
        {
            if (worldSelectPopup.activeInHierarchy)
            {
                return;
            }
            WWWForm param = new WWWForm();
            //param.AddField("step", index);
            param.AddField("world", StageInfo.World);
            param.AddField("diff", CacheUserData.GetInt("adventure_difficult", 1));
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("adventure/reward", param, (JObject jsonData) =>
              {
                  isNetworkState = false;
                  var info = StageManager.Instance.AdventureProgressData.GetWorldInfoData(StageInfo.World, StageInfo.Diff);
                  if(info != null)
                      StageInfo = info;

                  //시스템 리워드 팝업
                  if (SBFunc.IsJTokenCheck(jsonData["rewards"]))
                  {
                      if(RewardPopupOpenCor != null)
                      {
                          StopCoroutine(RewardPopupOpenCor);
                      }
                      RewardPopupOpenCor = StartCoroutine(RewardPopupOpenDelay(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["rewards"])),0.2f));
                  }
                  //event.currentTarget.parent.getComponent(StarReward).YellowDot();
                  RefreshWorldStarUI();
                  ClearBoxEffectObj();
              }, (string arg) =>
              {
                  isNetworkState = false;
              });
		}

        IEnumerator RewardPopupOpenDelay(List<Asset> rewards, float delayTime)
        {
            yield return SBDefine.GetWaitForSeconds(delayTime);
            SystemRewardPopup.OpenPopup(rewards);
        }


        void ClearBoxEffectObj()
        {
            foreach (var obj in boxRewardEffectObjs)
            {
                if(obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

		public void RefreshQuestDirectWorld()
		{
            if (QuestManager.Instance.CheckQuestMove == false) return;

            QuestManager.Instance.CheckQuestMove = false;

            int world = StageManager.Instance.Quest_World;
            int stage = StageManager.Instance.Quest_Stage;
            int diff = StageManager.Instance.Quest_Diff;
            bool worldAvailableCheck = StageManager.Instance.AdventureProgressData.isAvailableWorld(world);

			if (worldAvailableCheck == false)
			{
				ToastManager.On(100000628);
				return;
			}

			bool isLock = IsLockWorldStage(world, stage);

			if (!isLock)//잠금 해제 상태 - 진입 가능
			{
				List<int> worldStagesList = StageManager.Instance.AdventureProgressData.GetWorldStages(world, diff);

				if (worldStagesList == null || worldStagesList.Count < 1 || worldStagesList.Count < stage)
				{
					ToastManager.On(100000629);
					return;
				}

                StageInfoPopupData newPopupData = new StageInfoPopupData(world, stage, diff, worldStagesList[stage - 1]);

                stageScene.ChangeWorld(world, diff);
                if(worldSelectPopup.activeInHierarchy == false) { 
                    PopupManager.OpenPopup<AdventureReadyPopup>(newPopupData);
                }
            }
			else//진입 불가능
			{
				ToastManager.On(100000629);
			}
		}

		bool IsLockWorldStage(int worldIndex, int stageIndex)
		{
			bool isAvailableWorld = StageManager.Instance.AdventureProgressData.isAvailableWorld(worldIndex);

			if (isAvailableWorld == false)
			{
				ToastManager.On(100000628);
				return true;
			}

			if (stageIndex == 1)
			{
				return false;
			}

			int targetIndex = stageIndex - 2;
			List<int> stages = StageManager.Instance.AdventureProgressData.GetWorldStages(worldIndex);

			if (stages == null || stages.Count <= targetIndex)
			{
				return true;
			}

			return stages[targetIndex] < 1;
		}

		public void OnClickWorld()
        {
            //LoadingManager.ImmediatelySceneLoad("WorldSelect");
            var currentWorldIndex = 1;
            if(StageInfo != null)
                currentWorldIndex = StageInfo.World;

            worldSelectPopup.SetActive(true);
            worldSelectPopup.GetComponent<WorldSelectPopup>().Init(currentWorldIndex);
        }
        public void OnClickTown()
        {
            //   LoadingManager.ImmediatelySceneLoad("town",true, eUIType.Town);
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
        }


        public void SetVisibleLeftArrow(bool isActive)
        {
            if (leftArrow != null)
                leftArrow.SetActive(isActive);
        }
        public void SetVisibleRightArrow(bool isActive)
        {
            if (rightArrow != null)
                rightArrow.SetActive(isActive);
        }


        // 구체적으로 나오지 않아 임시 코드 - 추후 변경 필요
        public void OnClickDifficultyShow()
        {
            if(isDifficultShowState == false) { 
                foreach(var obj in difficultyButtonObjs)
                {
                    obj.SetActive(true);
                }
                isDifficultShowState = true;
            }
            else
            {
                foreach (var obj in difficultyButtonObjs)
                {
                    obj.SetActive(false);
                }
                SelectedDiffObj.SetActive(true);
                isDifficultShowState = false;
            }
        }
        public void OnClickDifficult(int index)
        {
            if(isDifficultShowState == false)
            {
                foreach (var obj in difficultyButtonObjs)
                {
                    obj.SetActive(true);
                }
                isDifficultShowState = true;
            }
            else
            {
                if (index > 0) return;
                SelectedDiffObj = difficultyButtonObjs[index];
                for (int i = 0; i < difficultyButtonObjs.Length; ++i)
                {
                    difficultyButtonObjs[i].SetActive(i == index);
                }
            }
        }
        public void OnClickWaitUpdate()
        {
            ToastManager.On(100000326);
        }
        
            
    }
}