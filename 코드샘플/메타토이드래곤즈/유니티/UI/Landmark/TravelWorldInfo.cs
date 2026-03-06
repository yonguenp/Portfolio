using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork 
{ 
    public class TravelWorldInfo : MonoBehaviour
    {
        public GameObject blockLayerObject = null;

        public Image mapBGImage = null;

        public Text worldNameText = null;
        public Text timeCostText = null;
        public Text blockText = null;

        public GameObject checkLayerObject = null;
        public GameObject selectedLayerObject = null;

        public Color originBGColor = new Color();
        public Color selectBGColor = new Color();

        [Header("Map Sprites")]
        public Sprite[] worldSprite = null;

        [SerializeField]
        Vector2 originSize = Vector2.zero;
        [SerializeField]
        Vector2 scaleSize = Vector2.zero;
        [SerializeField]
        float childrenScale = 1.1f;

        WorldTravelWorldSelectLayer curWorldLayer = null;
        TravelData curTravelData = null;

        public void Init(TravelData travelData, WorldTravelWorldSelectLayer layerData)
        {
            if (travelData == null || layerData == null) { return; }

            ClearLayer();

            curWorldLayer = layerData;
            curTravelData = travelData;

            worldNameText.text = StringData.GetStringByStrKey(curTravelData._NAME);
            timeCostText.text = SBFunc.TimeString(curTravelData.TIME);

            bool isLastStageClear = false;
            if (blockText != null)
            {
                var StageCount = StageTable.GetWorldStageCount(travelData.WORLD);
               blockText.text = string.Format(StringData.GetStringByIndex(100001732), travelData.WORLD, StageCount);
                var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(travelData.WORLD, 1);
                if(worldInfo != null)
                {
                    List<int> stageList = worldInfo.Stages;
                    if (stageList != null && stageList.Count > 0)
                    {
                        isLastStageClear = stageList[StageCount - 1] > 0;
                    }
                }
            }

            bool isSelected = (curTravelData.WORLD == curWorldLayer.GetCurrentSelectedWorld());

            checkLayerObject.SetActive(isSelected);
            selectedLayerObject.SetActive(isSelected);
            SelectedChangeNode(isSelected);

            mapBGImage.color = isSelected ? selectBGColor : originBGColor;

            if (worldSprite != null && worldSprite[curTravelData.WORLD - 1] != null)
            {
                mapBGImage.sprite = worldSprite[curTravelData.WORLD - 1];
            }

            blockLayerObject.SetActive(!isLastStageClear);
        }

        void SelectedChangeNode(bool _isSelected)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = _isSelected ? scaleSize : originSize;
            gameObject.GetComponent<RectTransform>().localScale = Vector3.one * (_isSelected ? childrenScale : 1f);
        }

        public void OnClickWorldInfoLayer()
        {
            curWorldLayer.RefreshWorldInfoButtonState(gameObject);
        }

        public void SwitchCurrentSelectedFrame(bool isOn)
        {
            checkLayerObject.SetActive(isOn);
            selectedLayerObject.SetActive(isOn);
            SelectedChangeNode(isOn);
            mapBGImage.color = isOn ? selectBGColor : originBGColor;
        }

        public TravelData GetTravelData()
        {
            return curTravelData;
        }

        void ClearLayer()
        {

        }
    }
}

