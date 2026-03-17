using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class DailyRewardObject : MonoBehaviour
    {
        [SerializeField]
        private Text dayText = null;
        [SerializeField]
        private GameObject[] dungeonRewardObjs = null;
        [SerializeField]
        private Image[] dungeonRewardIcons = null;
        [SerializeField]
        private Text[] dungeonRewardTexts = null;
        [SerializeField]
        private Image backImage = null;
        [SerializeField]
        private Sprite noSelectSprite = null;
        [SerializeField]
        private Sprite selectSprite = null;

        private bool isInit = false;
        public void SetSelectState(bool selectState)
        {
            backImage.sprite = (selectState ? selectSprite : noSelectSprite);
        }
        
        public void Init(eDailyType dayType, bool isSelect=false)
        {
            SetSelectState(isSelect);

            if (isInit)
                return;

            var dungeonData = DailyStageData.GetByDay(dayType);
            dayText.text=DailyManager.Instance.GetDailyString(dayType);
            foreach (var reward in dungeonRewardObjs)
            {
                reward.SetActive(false);
            }
            if (dungeonData != null) {

                for (int i = 0, count = Mathf.Min(dungeonRewardObjs.Length, dungeonData.Count); i < count; ++i)
                {
                    dungeonRewardObjs[i].SetActive(true);
                    if(User.Instance.IS_HOLDER){
                        if (dungeonRewardIcons[i] != null)
                        {
                            var iconPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, dungeonData[i].HOLDER_REWARD_ICON);
                            if (iconPrefab != null)
                            {
                                dungeonRewardIcons[i].enabled = false;
                                var objectPrefab = Instantiate(iconPrefab, dungeonRewardIcons[i].transform);
                                var rect = objectPrefab.GetComponent<RectTransform>();
                                if(rect != null)
                                    rect.sizeDelta = Vector2.one * 120;
                            }
                            else
                                dungeonRewardIcons[i].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, dungeonData[i].HOLDER_REWARD_ICON);
                        }
                        dungeonRewardTexts[i].text = StringData.GetStringFormatByStrKey(dungeonData[i].HOLDER_REWARD_DESC);
                    }
                    else
                    {
                        if (dungeonRewardIcons[i] != null)
                        {
                            var iconPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, dungeonData[i].REWARD_ICON);
                            if (iconPrefab != null)
                            {
                                dungeonRewardIcons[i].enabled = false;
                                var objectPrefab = Instantiate(iconPrefab, dungeonRewardIcons[i].transform);
                                var rect = objectPrefab.GetComponent<RectTransform>();
                                if (rect != null)
                                    rect.sizeDelta = Vector2.one * 120;
                            }
                            else
                                dungeonRewardIcons[i].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, dungeonData[i].REWARD_ICON);
                        }
                        dungeonRewardTexts[i].text = StringData.GetStringFormatByStrKey(dungeonData[i].REWARD_DESC);
                    }
                }
            }
            isInit = true;
        }

    }
}

