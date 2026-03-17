using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class TravelWorldSelectSlot : MonoBehaviour
    {
        public GameObject blockLayerObject = null;

        public Image mapBGImage = null;

        public Text worldNameText = null;
        public Text timeCostText = null;
        public Text costDragonText = null;
        public Text blockText = null;

        public GameObject checkLayerObject = null;
        public GameObject selectedLayerObject = null;

        [SerializeField] private Sprite SelectedImg = null;
        [SerializeField] private Sprite NonSelectedImg = null;
        [SerializeField] private Image nameBg = null;

        public Color originBGColor = new Color();
        public Color selectBGColor = new Color();


        TravelData curTravelData = null;

        public delegate void func(int _selectWorldIndex);

        public func clickWorldIndexCallback = null;

        public void Init(TravelData travelData, int _currentSelectWorldIndex, func _clickWorldIndexCallback)
        {
            if (travelData == null) { return; }

            

            curTravelData = travelData;

            if (_clickWorldIndexCallback != null)
                clickWorldIndexCallback = _clickWorldIndexCallback;

            worldNameText.text = string.Format("{0} - {1}", curTravelData.WORLD, StringData.GetStringByStrKey(curTravelData._NAME));
            timeCostText.text = SBFunc.TimeString(curTravelData.TIME);
            costDragonText.text = SBFunc.StrBuilder("x", curTravelData.CHAR_NUM);

            bool isLastStageClear = false;
            if (blockText != null)
            {
                var StageCount = StageTable.GetWorldStageCount(travelData.WORLD);
                blockText.text = StringData.GetStringFormatByStrKey("특정월드클리어", travelData.WORLD);//StringData.GetString(100001732
                var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(travelData.WORLD, 1);
                if (worldInfo != null)
                {
                    List<int> stageList = worldInfo.Stages;
                    if (stageList != null && stageList.Count > 0)
                    {
                        isLastStageClear = stageList[StageCount - 1] > 0;
                    }
                }
            }

            bool isSelected = (curTravelData.WORLD == _currentSelectWorldIndex);

            checkLayerObject.SetActive(isSelected);
            selectedLayerObject.SetActive(isSelected);
            nameBg.sprite = isSelected ? SelectedImg : NonSelectedImg;
            mapBGImage.color = isSelected ? selectBGColor : originBGColor;

            mapBGImage.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.WorldSelectImgPath, string.Format("worldmap_{0:D2}", curTravelData.WORLD));

            blockLayerObject.SetActive(!isLastStageClear);
        }

        public void OnClickWorldInfoLayer()
        {
            if(clickWorldIndexCallback != null)
            {
                clickWorldIndexCallback(curTravelData.WORLD);
            }
        }

        public bool IsEqualWorldIndex(int _selectIndex)
        {
            if (curTravelData == null)
                return false;

            return curTravelData.WORLD == _selectIndex;
        }

        public void SwitchCurrentSelectedFrame(bool isOn)
        {
            checkLayerObject.SetActive(isOn);
            selectedLayerObject.SetActive(isOn);
            nameBg.sprite = isOn ? SelectedImg : NonSelectedImg;
            mapBGImage.color = isOn ? selectBGColor : originBGColor;
        }
    }
}
