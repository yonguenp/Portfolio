using SBSocketSharedLib;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MatchInfoPopup : Popup
{
    [SerializeField]
    RectTransform backgroundUI;
    [SerializeField]
    Text matchingText;
    [SerializeField]
    Text MatchingCountText;
    [SerializeField]
    Button cancelButton;
    [SerializeField]
    GameObject subBoard;

    [Header("플레이어 리스트")]
    [SerializeField]
    GameObject prefab;
    [SerializeField]
    List<UIRankItem> playerList = new List<UIRankItem>();
    [SerializeField] Transform content;
    [SerializeField] Image cancelListBtn;

    [SerializeField] Color myColor;
    [SerializeField] Color onLineColor;
    [SerializeField] Color offLineColor;

    [Header("Backgrounds")]
    [SerializeField] RectTransform outBG;
    [SerializeField] RectTransform inBG;

    [SerializeField] float normalSize;
    [SerializeField] float miniSize;
    [SerializeField] float diffSize;

    [SerializeField] GameObject popupOpen_Btn;

    bool normalMode = true;
    public bool IsRankMode { get; private set; }
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);

        //cancelButton.interactable = false;

        SetCount(0, 0);
        ActiveDetailPlayerList(false);
        CancelInvoke("NetworkError");

        cancelButton.interactable = false;
        CancelInvoke("ReleaseCancelLock");
        Invoke("ReleaseCancelLock", 5.0f);
    }

    public void SetPracticeMode()
    {
        matchingText.text = StringManager.GetString("연습게임매칭중");
        popupOpen_Btn.SetActive(true);
        IsRankMode = false;
    }

    public void SetRankMode()
    {
        matchingText.text = StringManager.GetString("랭크게임매칭중");
        popupOpen_Btn.SetActive(false);
        IsRankMode = true;
    }

    public override void Close()
    {
        CancelInvoke("NetworkError");
        base.Close();
        subBoard.SetActive(false);
    }

    public void SetCount(int curCount, int maxCount)
    {
        if (curCount + maxCount <= 0)
        {
            MatchingCountText.text = $"[-/-]";
        }
        else
        {
            MatchingCountText.text = $"[{curCount}/{maxCount}]";
            if (curCount < maxCount)
                cancelButton.interactable = true;
        }
    }

    public void OnCancelMatch()
    {
        CancelInvoke("NetworkError");
        Managers.Network.SendGameMatchCancel();
        Invoke("NetworkError", 20.0f);
    }

    public void NetworkError()
    {
        CancelInvoke("NetworkError");
        PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("네트워크오류"), () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }
    public void ToggleDetailPlayerList()
    {
        ActiveDetailPlayerList(!subBoard.activeInHierarchy);
    }
    public void ActiveDetailPlayerList(bool isOn)
    {
        subBoard.SetActive(isOn);
        backgroundUI.gameObject.SetActive(isOn);
        if (isOn)
        {
            cancelListBtn.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270));
            backgroundUI.position = (PopupCanvas.Instance.transform as RectTransform).position;
            backgroundUI.sizeDelta = (PopupCanvas.Instance.transform as RectTransform).sizeDelta;
        }
        else
        {
            cancelListBtn.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
    }


    public void SetRoomPlayer(IList<MatchPlayerInfo> playerinfoList)
    {
        int targetSlotIndex = 0;
        playerList[0].Init(targetSlotIndex++, Managers.UserData.MyPoint, Managers.UserData.MyName);
        playerList[0].SetPlayerListUI(myColor, true);
        cancelButton.interactable = true;

        //유저가 있는경우
        for (int i = 0; i < playerinfoList.Count; i++)
        {
            if (playerinfoList[i].UserId == Managers.UserData.MyUserID)
                continue;

            playerList[targetSlotIndex].Init(-1, playerinfoList[i].RankPoint, playerinfoList[i].UserName);
            playerList[targetSlotIndex++].SetPlayerListUI(onLineColor, true);
        }

        //유저가 없는경우
        while (targetSlotIndex < playerList.Count)
        {
            playerList[targetSlotIndex].Init(-1, 0, StringManager.GetString("입장대기중"));
            playerList[targetSlotIndex++].SetPlayerListUI(offLineColor);
        }

        CancelInvoke("ReleaseCancelLock");
        if (playerinfoList.Count == 6)
        {
            cancelButton.interactable = false;
            Invoke("ReleaseCancelLock", 5.0f);
        }
    }

    public void ReleaseCancelLock()
    {
        CancelInvoke("ReleaseCancelLock");
        cancelButton.interactable = true;
    }

    public void CheckMode()
    {
        foreach (Popup popup in PopupCanvas.Instance.GetOpeningPopups())
        {
            if (popup.IsOpening())
            {
                switch (popup.GetPopupType())
                {
                    case PopupCanvas.POPUP_TYPE.TOUCH_EFFECT:
                    case PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP:
                        continue;
                    default:
                        SetNormalMode(false);
                        return;
                }
            }
        }

        SetNormalMode(true);
    }
    public void SetNormalMode(bool isNormal)
    {
        normalMode = isNormal;
        float diff = diffSize;
        if (normalMode)
        {
            inBG.sizeDelta = new Vector2(inBG.sizeDelta.x, normalSize);
            cancelButton.gameObject.SetActive(true);
        }
        else
        {
            inBG.sizeDelta = new Vector2(inBG.sizeDelta.x, miniSize);
            cancelButton.gameObject.SetActive(false);
        }

        outBG.sizeDelta = new Vector2(outBG.sizeDelta.x, inBG.sizeDelta.y + diffSize);
    }
}
