using SandboxPlatform.SAMANDA;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EscButton : MonoBehaviour
{
    [SerializeField] Popup popup;

    [SerializeField] UnityEvent cb = null;

    private void Update()
    {
        if (popup != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && SAMANDA.Instance.UI.gameObject.activeInHierarchy)
            {
                SAMANDA.Instance.UI.gameObject.SetActive(false);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) && popup.gameObject.activeSelf)
            {
                if (PopupCanvas.Instance.PopupEscList.LastOrDefault() == popup)
                {
                    if (popup.GetPopupType() == PopupCanvas.POPUP_TYPE.CHARACTER_POPUP)
                    {
                        var characterpopup = popup as CharacterPopup;
                        if (characterpopup.GetUI().GetSubPopupObject() != null)
                        {
                            characterpopup.GetUI().GetSubPopupObject().SetActive(false);
                            return;
                        }
                    }
                    else if (popup.GetPopupType() == PopupCanvas.POPUP_TYPE.GACHA_POPUP)
                    {
                        var gachapopup = popup as GachaPopup;
                        if (gachapopup.bSkipAnimation)
                            return;
                    }
                    else if (popup.GetPopupType() == PopupCanvas.POPUP_TYPE.RANK_POPUP)
                    {
                        var rankpopup = popup as RankPopup;
                        if (rankpopup.GetSubPopup())
                        {
                            rankpopup.OnSubPopup(false);
                            return;
                        }
                    }
                    else if (popup.GetPopupType() == PopupCanvas.POPUP_TYPE.SHOP_POPUP)
                    {
                        var shoppopup = popup as ShopPopup;
                        if (shoppopup.RefreshCheck())
                            return;
                    }
                    else if( popup.GetPopupType() == PopupCanvas.POPUP_TYPE.CLAN_POPUP)
                    {
                        var clanpopup = popup as ClanPopup;
                        if (clanpopup.EscSubPopup())
                        {
                            for (ClanPopup.SubPopupType i = 0; i < ClanPopup.SubPopupType.LENGTH; i++)
                            {
                                clanpopup.ClanSubPopupSetActive(i);
                            }
                            return;
                        }
                    }

                    popup.Close();
                    if (cb.GetPersistentEventCount() > 0)
                    {
                        cb?.Invoke();
                    }
                    if (!popup.IsOpening())
                        PopupCanvas.Instance.PopupEscList.Remove(popup);
                }
            }
        }
    }

}
