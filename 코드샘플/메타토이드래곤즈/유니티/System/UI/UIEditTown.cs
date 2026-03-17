using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditTown : UIObject
{
	[SerializeField]
	Text editAlertText = null;

    [SerializeField]
    GameObject exitBtnObj = null;
    
    bool isTutorialing = false;
    public override bool RefreshUI(eUIType targetType)
	{
		return curSceneType != targetType;
	}

	public void OnTownEdit()
    {
        bool isConstuctMode = Town.Instance.IsBuildingConstructModeState();
        if (isConstuctMode)
        {
            Town.Instance.SetConstructModeState(!isConstuctMode);
            return;
        }

        bool currentState = Town.Instance.IsBuildingEditModeState();
        Town.Instance.SetBuildingEditModeState(!currentState);
    }

    private void OnEnable()
    {
        if(TutorialManager.tutorialManagement != null) 
            isTutorialing = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Construct) || TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.ConstructUI);
        if (isTutorialing)
        {
            if(exitBtnObj != null)
                exitBtnObj.SetActive(false);
        }
            
    }

    public void OnClickTownEdit()
    {
        if (isTutorialing)
            return;
        OnTownEdit();
    }


	public void SetEditMessege(string message)
    {
        if (editAlertText != null)
        {
			editAlertText.text = message;
            //건설할 위치를 선택해 주세요 <-> 편집모드로 변할때 상자 크기 가변적으로 하기 위해 설정함
			GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 120f);
			LayoutRebuilder.ForceRebuildLayoutImmediate(editAlertText.GetComponent<RectTransform>());
			LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
		}
    }

}
