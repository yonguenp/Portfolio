using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultSlot : MonoBehaviour
{
    [SerializeField] Sprite[] sprite;
    [SerializeField] Image icon;
    [SerializeField] Toggle toggle;
    private void Start()
    {
        var difficult = transform.GetSiblingIndex(); //0번은 clone 타겟
        
        toggle.interactable = IsSelectableDifficult(difficult);
        if(toggle.interactable)
        {
            icon.sprite = sprite[difficult];
        }
        else
        {
            icon.sprite = sprite[0];
        }
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
}
