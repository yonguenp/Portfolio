using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelupTextInfo : MonoBehaviour
{
    [Header("[Common Info Layer]")]
    public GameObject commonInfoLayer;
    public Text commonInfoText;

    [Header("[Custom Info Layer]")]
    public Color originTextColor;
    public Color customTextColor;

    [Header("[Arrow Info Layer]")]
    public GameObject arrowInfoLayer;
    public Text InfoText;
    public Text beforeLevelInfoText;
    public Text afterLevelInfoText;

    public void SetLevelTextData(LevelupTextData levelTextData)
    {
        if (levelTextData == null) { return; }

        switch (levelTextData.textStyle)
        {
            case LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE:
                commonInfoLayer.SetActive(false);
                arrowInfoLayer.SetActive(true);

                InfoText.text = levelTextData.levelInfoText;
                beforeLevelInfoText.text = levelTextData.beforeLevelText;
                afterLevelInfoText.text = levelTextData.afterLevelText;
                break;
            case LevelupTextData.LEVELUP_TEXT_STYLE.COMMON_STYLE:
                commonInfoLayer.SetActive(true);
                arrowInfoLayer.SetActive(false);

                commonInfoText.text = levelTextData.commonText;
                commonInfoText.color = originTextColor;
                break;
            case LevelupTextData.LEVELUP_TEXT_STYLE.CUSTOM_STYLE:
                commonInfoLayer.SetActive(true);
                arrowInfoLayer.SetActive(false);

                commonInfoText.text = levelTextData.commonText;
                commonInfoText.color = customTextColor;
                break;
        }
    }
}
