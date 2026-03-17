using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class StarReward : MonoBehaviour
    {
        [SerializeField] private Text labelReqStarAmount = null;
        [SerializeField] private GameObject nodeBubble = null;
        [SerializeField] private GameObject nodeRewardBox = null;
        [SerializeField] private Text rewardCntText = null;
        [SerializeField] private Image starIcon = null;

        private int reqStarAmount = 0;
        private int maxStarAmount = 24;
        private int totalLength = -1;

        void Start()
        {
            if(totalLength < 0)
            {
                totalLength = (int)transform.parent.GetComponent<Slider>().maxValue;
            }
           // RefreshPos();
        }

        public void SetStarIcon(Sprite sprite)
        {
            starIcon.sprite = sprite;
        }

        public void SetData(int reqStarNum, int rewardState, int maxStarNum, int rewardCnt)
        {
            reqStarAmount = reqStarNum;
            maxStarAmount = maxStarNum;
            labelReqStarAmount.text = reqStarNum.ToString();
            nodeBubble.SetActive(true);
            switch (rewardState)
            {
                case 0:
                    nodeBubble.SetActive(true);
                    rewardCntText.text = rewardCnt.ToString();
                    break;
                case 1:
                    nodeRewardBox.SetActive(true);
                    rewardCntText.text = rewardCnt.ToString();
                    break;
                case 2:
                    nodeBubble.SetActive(false);
                    break;
            }

            if (totalLength < 0)
            {
                totalLength = (int)transform.parent.GetComponent<Slider>().maxValue;
            }
            
            RefreshPos();
        }

        void RefreshPos()
        {
            var sliderComp = transform.parent.GetComponent<Slider>();
            var sliderSize = sliderComp.GetComponent<RectTransform>().sizeDelta;
            var sliderWidth = sliderSize.x;
            var slideImage = sliderComp.fillRect;

            var left = slideImage.offsetMin.x;
            var right = slideImage.offsetMax.x - 3.0f;

            var width = sliderWidth - (left + right * (-1));

            float value = (reqStarAmount / (float)maxStarAmount) * width;

            float xPos = (float) Math.Round(value);

            transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos - width * 0.5f, 0);
        }
    }
}