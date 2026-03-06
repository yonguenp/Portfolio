using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct PartDataEvent
    {
        public enum PartEvent
        {
            LOCK_STATE,
        }
        private static PartDataEvent obj;
        public PartEvent type;
        public int target_tag;

        public static void Send(PartEvent type, int tag)
        {
            obj.type = type;
            obj.target_tag = tag;

            EventManager.TriggerEvent(obj);
        }
    }
    public class PartSlotFrame : MonoBehaviour, EventListener<PartDataEvent>
    {
        [SerializeField]
        Image partIcon = null;
        [SerializeField]
        protected Text partlevelLabel = null;
        [SerializeField]
        protected GameObject dragonThumbnNailNode = null;
    
        [SerializeField]
        GameObject clickNode = null;

        [SerializeField]
        GameObject partSelectNode = null;

        [SerializeField]
        Image dragonIcon = null;

        [SerializeField]
        SlotFrameController frame  = null;

        [SerializeField]
        Image gradeBg = null;//등급마다 보드이미지가 따로 있음 (레고모양)

        [SerializeField]
        protected GameObject lockIcon = null;
        [SerializeField]
        protected GameObject fusionIcon = null;

        public delegate void func(string CustomEventData);

        protected func clickCallback = null;

        public int PartTag { get; protected set; }

        public bool isBelonged = false;
        public bool isSelected = false;
        public bool isClicked = false;

        void OnEnable()
        {
            EventManager.AddListener<PartDataEvent>(this);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<PartDataEvent>(this);
        }

        /**
         * @param partTag //파츠 tag (고유번호)
         * @param level //파츠 강화 상태 수준 (0이면 미표시)
         */
        public void SetPartSlotFrame(int partTag ,int level = 0, bool setBelongCharacter = true)
        {
            var dragonTag = GetLinkDragonTag(partTag);

            var isDragonTag = dragonTag > 0;
            dragonThumbnNailNode.SetActive(isDragonTag && setBelongCharacter);
            if (isDragonTag && setBelongCharacter)
            {
                DrawDragonIcon(dragonTag);//드래곤 초상화 세팅
            }
            isBelonged = isDragonTag;

            DrawPartIcon(partTag);//파츠 이미지 세팅

            var isPartLevelUpperZero = level > 0;

            if (isPartLevelUpperZero)
            {
                partlevelLabel.text = ('+' + level.ToString());//파츠 강화 수치 세팅
            }
            
            partlevelLabel.gameObject.SetActive(isPartLevelUpperZero);
            PartTag = partTag;

            SetBg();
            if (lockIcon != null)
                lockIcon.SetActive(IsLock(partTag));

            if (fusionIcon != null)
            {
                var partData = User.Instance.PartData.GetPart(PartTag);
                if(partData != null)
                    fusionIcon.SetActive(partData.IsFusion);
            }
        }

        public void SetPartSlotFrame(ChampionPart part)
        {
            dragonThumbnNailNode.SetActive(false);

            DrawPartIcon(part);//파츠 이미지 세팅

            var isPartLevelUpperZero = part.Reinforce > 0;

            if (isPartLevelUpperZero)
            {
                partlevelLabel.text = ('+' + part.Reinforce.ToString());//파츠 강화 수치 세팅
            }

            partlevelLabel.gameObject.SetActive(isPartLevelUpperZero);
            PartTag = part.ID;

            SetBg(part);
            if (lockIcon != null)
                lockIcon.SetActive(false);

            if (fusionIcon != null)
            {
                fusionIcon.SetActive(false);
            }
        }

        public void SetONLYPartSlotFrame(int partTag,int level)
        {
            var itemDesignData = ItemBaseData.Get(partTag);
            var image = itemDesignData.ICON_SPRITE;

            this.partIcon.sprite = image;

            var isPartLevelUpperZero = level > 0;

            if (isPartLevelUpperZero)
            {
                this.partlevelLabel.text = ('+' + level.ToString());//파츠 강화 수치 세팅
            }
            partlevelLabel.gameObject.SetActive(isPartLevelUpperZero);
            PartTag = partTag;

            SetBg();
        }


        protected void SetBg()
        {
            var partInfo = User.Instance.PartData.GetPart(PartTag);
            SetBg(partInfo);
        }

        protected virtual void SetBg(UserPart partInfo)
        {
            if (frame != null && gradeBg != null)
            {                
                if (partInfo != null)
                {
                    var grade = partInfo.Grade();
                    frame.SetColor(grade);
                    gradeBg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("bggrade_board_", grade));
                }

            }
        }


        public void SetVisibleClickNode(bool isVisible)
        {
            if (this.clickNode != null)
            {
                this.clickNode.SetActive(isVisible);
            }
            this.isClicked = isVisible;
        }

        public void SetCallback(func ok_cb)
        {
            if(ok_cb != null)
            {
                this.clickCallback = ok_cb;
            }
        }

        public void SetVisibleSelectedNode(bool isVisible)
        {
            if (this.partSelectNode != null)
            {
                if (this.partSelectNode.activeInHierarchy == !isVisible)
                {
                    this.partSelectNode.SetActive(isVisible);
                }
                this.isSelected = isVisible;
            }
        }

        //현재 파츠가 드래곤에 링크되있으면 드래곤 태그 리턴
        protected virtual int GetLinkDragonTag(int partTag)
        {
            return User.Instance.PartData.GetPartLink(partTag);
        }

        protected virtual bool IsLock(int partTag)
        {
            return User.Instance.Lock.IsLockPart(PartTag);
        }

        void DrawDragonIcon(int dragonTag)
        {
            var dragonInfo = CharBaseData.Get(dragonTag.ToString());
            if (dragonInfo != null)
                dragonIcon.sprite = dragonInfo.GetThumbnail();
        }

        protected void DrawPartIcon(int partTag)
        {
            var partData = User.Instance.PartData.GetPart(partTag);
            DrawPartIcon(partData);            
        }

        protected void DrawPartIcon(UserPart partData)
        {
            Sprite image = null;
            if (partData != null)
            {
                var designData = partData.GetItemDesignData();
                if (designData != null)
                {
                    image = designData.ICON_SPRITE;
                }
            }
            partIcon.sprite = image;
        }

        public virtual void onClickFrame()
        {
            if (clickCallback != null)
            {
                clickCallback(PartTag.ToString());//버튼 클릭시 태그값 던짐
            }
        }

        public void OnEvent(PartDataEvent eventData)
        {
            if (PartTag != eventData.target_tag)
                return;

            switch (eventData.type)
            {
                case PartDataEvent.PartEvent.LOCK_STATE:
                    if (lockIcon != null)
                        lockIcon.SetActive(User.Instance.Lock.IsLockPart(PartTag));
                    break;
            }
        }
    }
}

