using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuGroup : MonoBehaviour
{
    public enum MAINMENU_UI_TYPE
    {
        CAT_INFO,
        COOKING,
        CRAFT,
        ALBUM,
    }

    public GameObject[] MainMenuObjects;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);

        Refresh();
    }

    public void Refresh()
    {
        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();

        //if (neco_data.ClientDEBUG_Seq.COOK_BUTTON_OPEN == seq)
        //{
        //    CookButton.transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        //}
        //else
        //{
        //    CookButton.transform.DOKill();
        //    CookButton.transform.localScale = Vector3.one;
        //}
    }

    public void ToggleButtonInteractable(bool state)
    {
        foreach (GameObject mainmenu in MainMenuObjects)
        {
            if (mainmenu == null) { return; }

            mainmenu.GetComponent<Button>().interactable = state;
        }
    }

    private void OnDisable()
    {
        ToggleButtonInteractable(false);
    }
}
