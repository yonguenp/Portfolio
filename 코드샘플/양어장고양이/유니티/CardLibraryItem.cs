using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLibraryItem : MonoBehaviour
{    
    card_define myData;
    user_card userData;

    CardLibrary parent;
    CardLibraryDetail libraryDetail;

    public Button button;
    public Image CardImage;
    public GameObject NotCompleteCover;
    public GameObject cursorObject;

    public GameObject PerfectIcon;
    public GameObject MovieIcon;

    public void SetLibraryItem(int state, card_define data, CardLibrary _parent)
    {
        myData = data;
        parent = _parent;

        if (PerfectIcon != null && MovieIcon != null)
        {
            bool isMovie = data != null && data.GetResourceType() == 1;
            PerfectIcon.SetActive((state & 2) != 0 && !isMovie);
            MovieIcon.SetActive(isMovie && state != 0);
        }


        if (state == 0)
        {
            button.enabled = false;
            CardImage.gameObject.SetActive(false);
            NotCompleteCover.SetActive(false);
            return;
        }

        if ((state & 2) != 0)
        {            
            NotCompleteCover.SetActive(false);            
        }


        object obj;
        if (myData != null && myData.data.TryGetValue("cover_img", out obj))
        {
            CardImage.sprite = Resources.Load<Sprite>((string)obj);
        }

        if (CardImage.sprite == null)
        {
            CardImage.sprite = Resources.Load<Sprite>("Sprites/icon/card_default");
        }

        button.enabled = true;
        button.onClick.AddListener(() => {
            parent.OnCardLibraryDetail(myData);
        });

        if (_parent && _parent.LibraryListContainer.transform.childCount == 2)
        {
            if (ContentLocker.GetCurContentSeq() >= 33 && ContentLocker.GetCurContentSeq() <= 35)
            {
                ContentLocker locker = gameObject.AddComponent<ContentLocker>();
                locker.ContentID = 35;
                locker.guidePrefab = Resources.Load<GameObject>("Prefabs/ContentGuide/ContentGuide_Canvas");
                locker.targetUIRectTransform = CardImage.GetComponent<RectTransform>();
                locker.targetSprite = GetComponent<Image>().sprite;//rounding masking target
                //string[] scirpt = { LocalizeData.GetText("CS30-00") };
                //locker.GuideText = scirpt;

                locker.RefreshUnlockStatus();
            }
        }
    }

    public void SetLibraryDetailItem(card_define data, user_card userCard, CardLibraryDetail parent)
    {
        userData = userCard;
        libraryDetail = parent;

        CardImage.sprite = userCard.GetIcon();

        button.enabled = true;
        button.onClick.AddListener(() => {
            libraryDetail.OnCardLibraryDetail(userData);
        });

        if (PerfectIcon != null && MovieIcon != null)
        {
            PerfectIcon.SetActive(false);
            MovieIcon.SetActive(false);

            switch (userData.GetCardType())
            {
                case user_card.CARD_TYPE.PERFECT:
                    PerfectIcon.SetActive(true);
                    break;
                case user_card.CARD_TYPE.MOVIE:
                    MovieIcon.SetActive(true);
                    break;
                case user_card.CARD_TYPE.PIECE:
                default:
                    break;
            }
        }
    }

    public void RefreshCursor(user_card uc)
    {
        cursorObject.SetActive(uc == userData);
    }
}
