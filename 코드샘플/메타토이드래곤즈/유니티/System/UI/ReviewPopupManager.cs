using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    //public struct ReviewPopupEvent
    //{
    //	public enum eEvent
    //	{
    //		SHOW_REVIEW_POPUP,
    //	}

    //	public eEvent e;
    //  public bool condition;

    //	public ReviewPopupEvent(eEvent _Event, bool _condition)
    //	{
    //		e = _Event;
    //      condition = _condition;
    //	}

    //	static public void Event(eEvent _event, bool _condition = false)
    //	{
    //		switch (_event)
    //		{
    //			case eEvent.SHOW_REVIEW_POPUP:
    //				EventManager.TriggerEvent(new ReviewPopupEvent(eEvent.SHOW_REVIEW_POPUP, _condition));
    //				break;
    //		}
    //	}
    //}

    public class ReviewPopupManager
    {
        const string REVIEW_LOCAL_KEY = "review_state";
        string REVIEW_URL {
            get
            {
                return GameConfigTable.GetStoreURL();
            }
        }
        string SUPPORT_URL
        {
            get
            {
                return GameConfigTable.GetSurpportURL();
            }
        }

        const int TOWN_REVIEW_SHOW_LEVEL = 2;

        private static ReviewPopupManager instance;
        public static ReviewPopupManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ReviewPopupManager();
                return instance;
            }
            set { instance = value; }
        }

		void ShowReviewPopup(bool _isForce)
        {
            if(!_isForce)
            {
                var isReviewed = GetReviewState();
                if (isReviewed)
                    return;
            }

            ShowDefaultPopup();            
        }

		void SetReviewState(bool _isReviewed)//0 : false // 1 : true
        {
			PlayerPrefs.SetInt(REVIEW_LOCAL_KEY, Convert.ToInt32(_isReviewed));
        }

		bool GetReviewState()
        {
			return Convert.ToBoolean(PlayerPrefs.GetInt(REVIEW_LOCAL_KEY, 0));
        }

        public void TryShowTownLevelCondition(bool _isForce = false)
        {
            if (User.Instance.ExteriorData.ExteriorLevel == TOWN_REVIEW_SHOW_LEVEL)
                instance.ShowReviewPopup(_isForce);
        }

        void ShowDefaultPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("리뷰팝업제목"), StringData.GetStringByStrKey("리뷰팝업내용1"), StringData.GetStringByStrKey("잼있어요"), StringData.GetStringByStrKey("별로에요"),
                () => {
                    RequestReviewPopup();
                },
                () => {
                    //cancel
                    RequestSupportPopup();
                },
                () => {
                    //x
                    SetReviewState(false);
                }
            );
        }

        void RequestSupportPopup()
        {
            ReviewPopup.OnReviewPopup(StringData.GetStringByStrKey("리뷰팝업제목"), StringData.GetStringByStrKey("리뷰팝업내용2"), StringData.GetStringByStrKey("의견남기기"), StringData.GetStringByStrKey("다음에요"),
                () => {
                    SBFunc.SendSupportURL();
                    SetReviewState(false);
                },
                () => {
                    //cancel
                    SetReviewState(false);
                },
                () => {
                    //x
                    SetReviewState(false);
                }
            );
        }

        void RequestReviewPopup()
        {
            ReviewPopup.OnReviewPopup(StringData.GetStringByStrKey("리뷰팝업제목"), StringData.GetStringByStrKey("리뷰팝업내용3"), StringData.GetStringByStrKey("리뷰남기기"), StringData.GetStringByStrKey("다음에요"),
                () => {
                    Application.OpenURL(REVIEW_URL);
                    SetReviewState(true);
                },
                () => {
                    //cancel
                    SetReviewState(false);
                },
                () => {
                    //x
                    SetReviewState(false);
                }
            );
        }
    }
}
