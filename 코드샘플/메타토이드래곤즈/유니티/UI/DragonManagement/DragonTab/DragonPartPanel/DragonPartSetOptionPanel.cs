using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//장비 장착 세트 옵션 라벨 데이터 세팅부분
namespace SandboxNetwork
{
    public class DragonPartSetOptionPanel : MonoBehaviour
    {
        [SerializeField]
        Sprite defaultIconImage = null;

        [SerializeField]
        List<Color> partSetOptionFrameColorList = new List<Color>();
        [SerializeField]
        List<Color> partSetOptionColorList = new List<Color>();
        [SerializeField]
        Color NoSetOptionColor = new Color();
        [SerializeField]
        Color NoSetOptionFrameColor = new Color();


        [SerializeField]
        GameObject setTargetfirstNode;
        [SerializeField]
        Text setEffectfirstLabel = null;
        [SerializeField]
        Image setEffectfirstBG = null;
        [SerializeField]
        Image setEffectfirstIcon = null;
        [SerializeField]
        Image setEffectfirstFrame = null;

        [SerializeField]
        GameObject setTargetsecondNode = null;
        [SerializeField]
        Text setEffectSecondLabel = null;
        [SerializeField]
        Image setEffectSecondBG = null;
        [SerializeField]
        Image setEffectSecondIcon = null;
        [SerializeField]
        Image setEffectSecondFrame = null;

        int dragonTag  = 0;
        public void Init()
        {
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag; 
                var dragonData = User.Instance.DragonData;
                if (dragonData == null)
                {
                    Debug.Log("user's dragon Data is null");
                    return;
                }

                var userDragonInfo = dragonData.GetDragon(dragonTag);
                if (userDragonInfo == null)
                {
                    Debug.Log("user Dragon is null");
                    return;
                }

                this.dragonTag = dragonTag;

                SetEffectOption(userDragonInfo.PartsSetList);
            }
        }

        void SetEffectOption(int[] partSetEffectList)
        {
            SetEffectLabel(setEffectfirstLabel, StringData.GetStringByStrKey("세트효과없음"));
            SetEffectLabel(setEffectSecondLabel, StringData.GetStringByStrKey("세트효과없음"));
            setTargetfirstNode.SetActive(false);
            setTargetsecondNode.SetActive(false);

            setEffectfirstBG.color = NoSetOptionColor;
            setEffectSecondBG.color = NoSetOptionColor;

            setEffectfirstFrame.color = NoSetOptionFrameColor;
            setEffectSecondFrame.color = NoSetOptionFrameColor;


            if (partSetEffectList != null && partSetEffectList.Length > 0)
            {
                Text targetLabel = null;
                GameObject targetNode = null;
                Image targetIcon = null;
                Image targetBg  = null;
                Image targetFrame = null;

                for (var i = 0; i < partSetEffectList.Length; i++)
                {
                    var index = partSetEffectList[i];
                    if (index <= 0)
                    {
                        continue;
                    }

                    switch (i)
                    {
                        case 0:
                        {
                            targetLabel = setEffectfirstLabel;

                            targetNode = setTargetfirstNode;
                            targetBg = setEffectfirstBG;
                            targetIcon = setEffectfirstIcon;
                            targetFrame = setEffectfirstFrame;
                        }
                        break;
                        case 1:
                        {
                            targetLabel = setEffectSecondLabel;

                            targetNode = setTargetsecondNode;
                            targetBg = setEffectSecondBG;
                            targetIcon = setEffectSecondIcon;
                            targetFrame = setEffectSecondFrame;
                        }
                        break;
                    }

                    var data = PartSetData.Get(index.ToString());
                    var stat_type = data.STAT_TYPE;
                    var value_type = data.VALUE_TYPE;
                    var value = data.VALUE;
                    var group = data.GROUP;
                    var grade = int.Parse((group / 100).ToString());

                    var str = SBFunc.StrBuilder(StatTypeData.GetDescStringByStatType(stat_type, value_type == "PERCENT") , " +" ,value);

                    if (value_type == "PERCENT")
                        str += "%";

                    if ("CRI_PROC" == stat_type)
                        stat_type = "CRI";

                    var isThreeSetOption = data.NUM <= ePartSetNum.SET_3;
                    var listIndex = isThreeSetOption ? 0 : 1;
                    if(listIndex >= 0 && partSetOptionFrameColorList.Count > 0 && partSetOptionFrameColorList.Count > listIndex)
                        targetFrame.color = partSetOptionFrameColorList[listIndex];
                    if (listIndex >= 0 && partSetOptionColorList.Count > 0 && partSetOptionColorList.Count > listIndex)
                        targetBg.color = partSetOptionColorList[listIndex];

                    SetEffectLabel(targetLabel, str);
                    SetSprite(targetNode, targetIcon, stat_type, grade);
                }
            }
        }

        void SetEffectLabel(Text targetLabel ,string targetString)
        {
            if (targetLabel != null)
                targetLabel.text = targetString;
        }

        void SetSprite(GameObject targetNode ,Image icon ,string stat_type ,int grade)
        {
            if (targetNode.activeInHierarchy == false)
                targetNode.SetActive(true);

            var spriteIcon = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("gem_" + stat_type.ToLower()));
            icon.sprite = spriteIcon != null ? spriteIcon : defaultIconImage;
        }
    }
}
