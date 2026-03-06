using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPartIconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        Image slotBg = null;

        [SerializeField]
        Image slotSprite = null;

        [SerializeField]
        GameObject lockObj = null;

        [SerializeField]
        GameObject plusObj = null;

        [SerializeField]
        GameObject slotAnimNode = null;

        [SerializeField]
        GameObject clickNode = null;

        [SerializeField]
        Text levelLabel = null;

        [SerializeField]
        GameObject dimedObject = null;

        [SerializeField]
        Animator partEquipAnim = null;

        [SerializeField]
        Color[] FrameColor = null;
        [SerializeField]
        Image Frame = null;

        [SerializeField]
        List<GameObject> setGradeStepObjectList = new List<GameObject>();
        [SerializeField]
        List<GameObject> setEffectObjectList = new List<GameObject>();

        [SerializeField]
        GameObject FusionIcon = null;

        const string animFlag = "isEquip";//true 일때 slot_mount anim 출력
        const string idleAnimName = "slot_idle";
        const string equipAnimName = "slot_mount";

        int slotPartTag = 0;//고유 번호

        Sequence equipAnimSeq = null;
        
        public int SlotPartTag
        {
            get { return slotPartTag; }
        }

        int slotPartID = 0;//아이템 id
        public int SlotPartID
        {
            get { return slotPartID; }
        }

        public void refreshSlot(bool isUnLock ,int partTag ,int partID )
        {
            slotPartID = partID;
            slotPartTag = partTag;

            InitAnimNode();
            InitObjVisible();

            if (levelLabel != null)
                levelLabel.gameObject.SetActive(false);

            if (isUnLock)
            {
                unLockSlot();
                if (partTag > 0 && partID > 0)
                    refreshSlotSprite(partTag);
            }
            else
                LockSlot();//잠긴 슬롯 표시 리소스 필요 임시로 꺼둠

            SetFrame();

            if (slotSprite != null)
                slotSprite.transform.localScale = Vector3.one;
            if (slotBg != null)
                slotBg.transform.localScale = Vector3.one;
            if (clickNode != null)
                clickNode.transform.localScale = Vector3.one;

            if (FusionIcon != null)
            {
                var partData = User.Instance.PartData.GetPart(slotPartTag);
                if (partData != null)
                {
                    FusionIcon.SetActive(partData.IsFusion);
                }                
            }
        }

        void SetFrame()
        {
            if (Frame == null)
                return;

            SetFrameColor(0);

            var partData = User.Instance.PartData.GetPart(slotPartTag);
            if (partData == null)
            {
                return;
            }

            var partDesignData = partData.GetItemDesignData();
            if (partDesignData != null)
            {
                SetFrameColor(partDesignData.GRADE);
            }
        }

        void SetFrameColor(int index)
        {
            if (FrameColor == null || FrameColor.Length <= index)
            {
                Frame.color = Color.white;
                return;
            }

            Frame.color = FrameColor[index];
        }

        public void refreshSlot(ChampionPart part)
        {
            slotPartID = -1;
            slotPartTag = -1;
            var partGrade = 0;

            if (part != null)
            {
                slotPartID = part.ID;
                slotPartTag = part.Tag;
                partGrade = part.GetItemDesignData().GRADE;
            }

            if (slotSprite != null)
                slotSprite.transform.localScale = Vector3.one;
            if (slotBg != null)
                slotBg.transform.localScale = Vector3.one;

            InitAnimNode();
            InitObjVisible();

            if (Frame != null)
                SetFrameColor(partGrade);

            if (levelLabel != null)
                levelLabel.gameObject.SetActive(false);

            if (true)
            {
                unLockSlot();
                if (slotPartTag >= 0 && slotPartID > 0)
                    refreshSlotSprite(part);
            }
            else
                LockSlot();//잠긴 슬롯 표시 리소스 필요 임시로 꺼둠
        }

        void InitAnimNode()
        {
            if (slotAnimNode != null)
                slotAnimNode.gameObject.SetActive(false);
        }

        void InitObjVisible()
        {
            if (slotSprite != null)
                slotSprite.gameObject.SetActive(false);
            if (slotBg != null)
                slotBg.gameObject.SetActive(false);
            if (plusObj != null)
                plusObj.SetActive(false);
            if (lockObj != null)
                lockObj.SetActive(false);
            if (FusionIcon != null)
                FusionIcon.SetActive(false);
        }

        public void ShowAnimArrowNode()
        {
            if (slotAnimNode != null && slotAnimNode.activeInHierarchy == false)
            {
                slotAnimNode.gameObject.SetActive(true);

                var animComponent = slotAnimNode.GetComponent<Animation>();
                if (animComponent == null)
                    return;

                animComponent.Play();
            }
        }

        public void HideAnimArrowNode()
        {
            if (slotAnimNode != null && slotAnimNode.activeInHierarchy == true)
                slotAnimNode.gameObject.SetActive(false);
        }

        public void ShowClickNode()
        {
            if (clickNode != null)
                clickNode.gameObject.SetActive(true);
        }

        public void HideClickNode()
        {
            if (clickNode != null)
                clickNode.gameObject.SetActive(false);
        }

        void refreshSlotSprite(int partTag)
        {
            var partData = User.Instance.PartData.GetPart(partTag);
            refreshSlotSprite(partData);
        }

        void refreshSlotSprite(UserPart partData)
        {
            if (partData == null)
            {
                Debug.Log("partData is null");
                return;
            }

            var partDesignData = partData.GetItemDesignData();
            if (partDesignData != null)
            {
                slotSprite.sprite = partDesignData.ICON_SPRITE;
            }
            slotSprite.gameObject.SetActive(true);
            slotBg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("bggrade_board_", partData.Grade()));
            slotBg.gameObject.SetActive(true);

            if (plusObj != null)
                plusObj.SetActive(false);

            var partLevel = partData.Reinforce;
            if (levelLabel != null && partLevel > 0)
            {
                levelLabel.gameObject.SetActive(true);
                levelLabel.text = SBFunc.StrBuilder("+", partLevel);
            }

            if(FusionIcon != null)
            {
                FusionIcon.SetActive(partData.IsFusion);
            }
        }

        //각 상태에 따른 슬롯 데이터 및 이미지 갱신
        void unLockSlot()
        {
            slotSprite.gameObject.SetActive(false);
            slotBg.gameObject.SetActive(false);
            lockObj.SetActive(false);
            if (plusObj != null)
                plusObj.SetActive(true);
        }
        void LockSlot()
        {
            slotSprite.gameObject.SetActive(false);
            slotBg.gameObject.SetActive(false);
            lockObj.SetActive(true);
            if (plusObj != null)
                plusObj.SetActive(false);
        }

        public void SetIconDimmed(bool _isDimmed)
        {
            if (dimedObject != null)
                dimedObject.SetActive(_isDimmed);
        }

        public void PlayEquipAnim()
        {
            if (partEquipAnim != null)
            {
                if(equipAnimSeq != null)
                {
                    equipAnimSeq.Kill();
                }

                var equipAnimLength = GetAnimLength(equipAnimName);
                equipAnimSeq = DOTween.Sequence();
                equipAnimSeq.AppendCallback(() =>
                {
                    partEquipAnim.SetBool(animFlag, true);
                }).AppendInterval(2.5f).AppendCallback(() => {
                    InitAnimation();
                });
            }
        }

        public void InitAnimation()
        {
            if (partEquipAnim != null)
            {
                if (equipAnimSeq != null)
                {
                    equipAnimSeq.Kill();
                    equipAnimSeq = null;
                }
                partEquipAnim.SetBool(animFlag, false);
            }
        }
        private float GetAnimLength(string animName)
        {
            float time = 0;
            RuntimeAnimatorController ac = partEquipAnim.runtimeAnimatorController;

            for (int i = 0; i < ac.animationClips.Length; i++)
            {
                if (ac.animationClips[i].name == animName)
                {
                    time = ac.animationClips[i].length;
                }
            }

            return time;
        }

        public void InitEffect()
        {
            foreach (var obj in setGradeStepObjectList)
                if (obj != null)
                    obj.SetActive(false);

            foreach (var obj in setEffectObjectList)
                if (obj != null)
                    obj.SetActive(false);
        }

        public void MakeStepGradeOptionEffect(int _grade)
        {
            var modifyIndex = _grade - 1;

            if (setGradeStepObjectList == null || setGradeStepObjectList.Count <= 0 || modifyIndex < 0 || setGradeStepObjectList.Count < modifyIndex)
                return;

            setGradeStepObjectList[modifyIndex].SetActive(true);
        }
        public void MakeSetOptionEffect(bool _isSixPieceSet)//true 면 3셋, false 면 6셋
        {
            if (setEffectObjectList == null || setEffectObjectList.Count <= 0 || setEffectObjectList.Count < 2)
                return;

            var index = _isSixPieceSet ? 1 : 0;
            setEffectObjectList[index].SetActive(true);
            setEffectObjectList[1 - index].SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotSprite != null)
                slotSprite.transform.DOScale(Vector3.one * 1.5f, 0.3f);
            if (slotBg != null)
                slotBg.transform.DOScale(Vector3.one * 1.5f, 0.3f);
            if (clickNode != null)
                clickNode.transform.DOScale(Vector3.one * 1.5f, 0.3f);
        }

        public void OnPointerExit(PointerEventData eventDate)
        {
            if (slotSprite != null)
                slotSprite.transform.DOScale(Vector3.one, 0.3f);
            if (slotBg != null)
                slotBg.transform.DOScale(Vector3.one, 0.3f);
            if (clickNode != null)
                clickNode.transform.DOScale(Vector3.one, 0.3f);
        }

        public void RefreshPointerAnim()
        {
            if (slotSprite != null)
                slotSprite.transform.localScale = Vector3.one;
            if (slotBg != null)
                slotBg.transform.localScale = Vector3.one;
        }
    }
}

