using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class DailyDungeonSelectObj : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private Text dungeonNameText;
        [SerializeField]
        private Image skillBookImg;
        [SerializeField]
        private Text skillBookText;
        [SerializeField]
        private Transform spineParent;
        [SerializeField]
        private GameObject enterBtnObj;
        [SerializeField]
        private GameObject defaultLayer;
        [SerializeField]
        private GameObject itemInfoLayer;
        [SerializeField]
        private Transform itemParent;

        public delegate void funcInt(int i);
        
        private bool isInit = false;
        private eDailyType curDay = eDailyType.None;
        public void Init(eDailyType day, funcInt cb, int index)
        {
            if (curDay != day)
                isInit = false;

            var data = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex;

            defaultLayer.SetActive(true);
            itemInfoLayer.SetActive(false);
            enterBtnObj.SetActive(true);
            int curWorld = data[index];
            var worldData = WorldData.GetByWorldNumber(curWorld);
            dungeonNameText.text = StringData.GetStringByIndex(worldData._NAME);
            enterBtnObj.GetComponent<Button>().onClick.RemoveAllListeners();
            enterBtnObj.GetComponent<Button>().onClick.AddListener(delegate { cb(index); });
            //skillBookImg.sprite = ResourceManager.GetResource<Sprite>(SBDefine.ResourcePath(eResourcePath.ItemIconPath, (User.Instance.IS_HOLDER) ? data[index].HOLDER_REWARD_ICON : data[index].REWARD_ICON));
            //Instantiate(ResourceManager.GetResource<GameObject>(SBDefine.ResourcePath(,data[index].STAGE_IMAGE)), spineParent);

            if (isInit)
                return;

            isInit = true;
            var dailyStageData = DailyStageData.GetByWorld(curWorld);
            if (spineParent != null)
            {
                var spinePrefab = dailyStageData.GetDailySpinePrefab();  // data[index].GetDailySpinePrefab();
                if (spinePrefab != null)
                {
                    SBFunc.RemoveAllChildrens(spineParent);
                    Instantiate(spinePrefab, spineParent);
                }
            }

            if (skillBookImg != null)
            {
                var iconPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, (User.Instance.IS_HOLDER) ? dailyStageData.HOLDER_REWARD_ICON : dailyStageData.REWARD_ICON);
                if(iconPrefab != null)
                {
                    skillBookImg.enabled = false;
                    SBFunc.RemoveAllChildrens(skillBookImg.transform);
                    Instantiate(iconPrefab, skillBookImg.transform);
                }
            }
            if (skillBookText != null) 
                skillBookText.text = StringData.GetStringByStrKey((User.Instance.IS_HOLDER) ? dailyStageData.HOLDER_REWARD_DESC : dailyStageData.REWARD_DESC);

        }

        public void SetItemInfoLayer(int world, int stage)
        {
            defaultLayer.SetActive(false);
            itemInfoLayer.SetActive(true);
            enterBtnObj.SetActive(false);

            var stageInfo = StageBaseData.GetByWorldStage(world, stage);
            var rewards = stageInfo.REWARD_ITEMS;
            SBFunc.RemoveAllChildrens(itemParent);

            //MIn, Max 가 유저의 클리어 달성도(Star)에 의해 달라지기 때문에 추가된 로직
            var itemReward = new Dictionary<int, KeyValuePair<Asset, Asset>>();
            foreach (var item in rewards)
            {
                if (item != null)
                {
                    if(itemReward.TryGetValue(item.ItemNo, out var value))
                    {
                        var minReward = itemReward[item.ItemNo].Key;
                        var maxReward = itemReward[item.ItemNo].Value;
                        if (minReward == null || minReward.Amount > item.Amount)
                            minReward = item;
                        if (maxReward == null || maxReward.Amount < item.Amount)
                            maxReward = item;

                        itemReward[item.ItemNo] = new(minReward, maxReward);
                    }
                    else
                    {
                        itemReward.Add(item.ItemNo, new(item, item));
                    }
                }
            }

            var targetItem = -1;
            var it = itemReward.GetEnumerator();
            while(it.MoveNext())
            {
                var item = it.Current;
                var minItem = item.Value.Key;
                var maxItem = item.Value.Value;

                var itemObj = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), itemParent).GetComponent<ItemFrame>();
                itemObj.SetFrameItem(maxItem);
                itemObj.GetComponent<ItemFrame>().SetItemBgOff(false);
                itemObj.SetAmountTextSizeMultiful(1.5f);
                itemObj.SetTextAlignment(TextAnchor.MiddleCenter);
                if (minItem.Amount != maxItem.Amount) //minCount == maxCount 일반 출력 아니라면 ~로 구분
                    itemObj.SetMinMaxText(minItem, maxItem);

                if(targetItem < 0)
                {
                    var itemData = ItemBaseData.Get(item.Key);
                    if (itemData != null &&
                        (minItem.GoodType == eGoodType.MAGNET
                        || itemData.KIND == eItemKind.SKILL_UP
                        || itemData.KIND == eItemKind.SHOWCASE))
                    {
                        targetItem = item.Key;
                    }
                }
            }
            if (targetItem > 0)
                UIManager.Instance.MainUI.SetInvenItemUI(targetItem);
            else
            {
                UIManager.Instance.RefreshUI(eUIType.None);
                UIManager.Instance.MainUI.OnOffExitBtn(true);
            }
            //

            int gold = stageInfo.REWARD_GOLD;
            if(gold > 0)
            {
                var itemGold = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), itemParent).GetComponent<ItemFrame>();
                itemGold.setFrameCashInfo((int)eGoodType.GOLD, gold);
                itemGold.SetItemBgOff(false);
                itemGold.SetAmountTextSizeMultiful(1.5f);
                itemGold.SetTextAlignment(TextAnchor.MiddleCenter);
            }
            int acc_exp = stageInfo.REWARD_ACCOUNT_EXP;
            if (acc_exp > 0)
            {
                var itemAccExp = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), itemParent).GetComponent<ItemFrame>();
                itemAccExp.SetFrameItem(-1, acc_exp, (int)eGoodType.ACCOUNT_EXP);
                itemAccExp.SetItemBgOff(false);
                itemAccExp.SetAmountTextSizeMultiful(1.5f);
                itemAccExp.SetTextAlignment(TextAnchor.MiddleCenter);
            }


        }

    }
}