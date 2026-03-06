using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildInfoPopup : Popup<TabTypePopupData>, EventListener<GuildEvent>
    {
        bool isModless = true;

        [SerializeField]
        TabController tabController;
        [SerializeField]
        GameObject Deco;
        [SerializeField]
        GameObject Body;

        Vector3 originPos = Vector3.zero;
        float originSize = 0.0f;
        public void OnTownControll(bool on)
        {
            if (UIManager.Instance.CurrentUIType == eUIType.Town)
            {
                var camera = Camera.main;
                if (camera != null)
                {
                    var proCameraNumericBoundaries = camera.GetComponent<ProCamera2DNumericBoundaries>();
                    if (proCameraNumericBoundaries != null && proCameraNumericBoundaries.UseNumericBoundaries)
                    {
                        if (on)
                        {
                            Town.Instance.head.SetActive(false);
                            Town.Instance.guildTopLFlag.SetActive(false);
                            Town.Instance.guildTopRFlag.SetActive(false);
                            originSize = Camera.main.orthographicSize;
                            //화면에 풍경이 비치는 비율과 위치
                            Town.Instance.ZoomToTarget(Town.Instance.head.transform, 0.3f, 0.15f, Vector3.up * 5.5f);                            
                        }
                        else
                        {
                            Town.Instance.head.SetActive(true);
                            Town.Instance.guildTopLFlag.SetActive(true);
                            Town.Instance.guildTopRFlag.SetActive(true);

                            Town.Instance.ZoomBackToLastestView(0.5f);
                        }
                    }
                }
            }
        }
        public override void InitUI()
        {
            OnTownControll(true);

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            PopupManager.Instance.Top.SetGuildPointUI(true);
            InitTab();
        }

        private void OnEnable()
        {
            EventManager.AddListener<GuildEvent>(this);
            float bottomY = (transform as RectTransform).rect.min.y;
            Deco.transform.localPosition = new Vector3(0f, bottomY - 200f, 0f);
            Deco.transform.localScale = new Vector3(1f, 0f, 1f);

            float topY = (transform as RectTransform).rect.max.y;
            Body.transform.localPosition = new Vector3(0f, topY - 200f, 0f);

            Invoke("OnDecoTween", 0.1f);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<GuildEvent>(this);
        }

        private void OnDecoTween()
        {
            CancelInvoke("OnDecoTween");

            float bottomY = (transform as RectTransform).rect.min.y;

            Deco.transform.DOLocalMoveY(bottomY, 1.0f);
            Deco.transform.DOScaleY(1.0f, 1.0f).SetEase(Ease.OutBack);

            Body.transform.DOLocalMoveY(-10f, 0.5f);
        }

        void InitTab()
        {
            if (tabController == null)
                return;
            int tabIndex = 0;
            int subIndex = 0;
            if (Data != null)
            {
                tabIndex = Data.TabIndex;
                if (Data.SubIndex != -1)
                    subIndex = Data.SubIndex;
            }
            if (tabIndex < 0)
            {
                tabIndex = 0;
            }
            if(tabController.CurTab == tabIndex)
            {
                tabController.RefreshTab();
            }
            else
            {
                tabController.InitTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
            }            
        }

        public void SetModless(bool _isModless)
        {
            isModless = _isModless;
        }

        public override bool IsModeless()
        {
            return isModless;
        }

        public override void ClosePopup()
        {
            base.ClosePopup();

            PopupManager.Instance.Top.SetGuildPointUI(false);
            OnTownControll(false);
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
        }

        public void OnEvent(GuildEvent eventType)
        {
            switch (eventType.Event)
            {
                case GuildEvent.eGuildEventType.GuildRefresh:
                    break;
                case GuildEvent.eGuildEventType.LostGuild:
                    if (this.enabled)
                    {
                        ClosePopup();
                    }
                    break;
            }

        }
    }

}
