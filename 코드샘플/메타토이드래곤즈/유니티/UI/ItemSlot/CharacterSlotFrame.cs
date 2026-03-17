using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class CharacterSlotFrame : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        protected GameObject shadow = null;
        [SerializeField]
		protected GameObject slotAnimNode = null;
        [SerializeField]
		protected GameObject HiddenNode = null;
        [SerializeField]
		protected GameObject HiddenIconNode = null;
        [SerializeField]
		protected Button HiddenRegistButton = null;
        [SerializeField]
		protected Button HiddenReleaseButton = null;
        [SerializeField]
		protected RectTransform SpineParent = null;
        [SerializeField]
        protected GameObject btnNode = null;
        [SerializeField]
        Transform StarParentTr = null;
        [SerializeField]
        GameObject[] TranscendenceStars = null;

        [SerializeField]
        List<UIDragonSpine> dragonSpineList = new List<UIDragonSpine>();

        public int DragonTag { get; private set; } = -1;

        public delegate void func(string CustomEventData);

		protected func clickCallback = null;
		protected func hiddenRegistCallback = null;
		protected func hiddenReleaseCallback = null;

		protected bool clickActive = false;
		protected int line = 0;
        public int Line
        {
            get { return line; }
        }
		protected int index = 0;
        public int Index
        {
            get { return index; }
        }
        protected BattleLine curBattleLine = null;

        Coroutine spineCo = null;

        private void Awake()
        {
            if (dragonSpineList.Count > 3)
                SBFunc.SetUIDataAsset(dragonSpineList[0], dragonSpineList[1], dragonSpineList[2], dragonSpineList[3]);
        }
        protected virtual UserDragon GetDragon(int tag)
        {
            return User.Instance.DragonData.GetDragon(tag);
        }

        private void OnEnable()
        {
            if (DragonTag < 0)
                return;

            var data = GetDragon(DragonTag);
            if (data == null)
                return;

            var targetSpine = GetTargetSpine(data.BaseData);
            if(targetSpine != null)
                targetSpine.gameObject.SetActive(true);
        }
        public virtual void SetDragonData(int dragonTag, bool isShowLevel = false, bool shadowState = true, BattleLine battleLine = null, bool dragEnable = true)
        {
            EnableDrag = dragEnable;

            var element = GetDragon(dragonTag);
            if (element == null)
                return;

            curBattleLine = battleLine;
            if(curBattleLine != null)
            {
                for(int l = 0; l < 3; l++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (curBattleLine.GetDragon(l, i) == dragonTag)
                        {
                            line = l;
                            index = i;
                            break;
                        }
                    }
                }
            }

            if(dragonSpineList != null && dragonSpineList.Count >= 3)
            {
                if (spineCo != null)
                    StopCoroutine(spineCo);

                spineCo = StartCoroutine(SetDragonSpine(element));
            }
            else
            {
                var SpinePrefab = element.BaseData.GetUIPrefab();
                GameObject dragonClone = null;
                if (SpinePrefab != null)
                    dragonClone = Instantiate(SpinePrefab, SpineParent);

                if (dragonClone == null)
                    return;

                SBFunc.SetLayer(dragonClone, "UI");

                dragonClone.GetComponent<RectTransform>().localScale = new Vector3(4, 4, 1);
                var skin = dragonClone.GetComponent<UIDragonSpine>();
                if (skin != null)
                    skin.SetData(element);
                skin.SetSpineRaycastTargetState(false);
            }

            this.DragonTag = dragonTag;

            if (isShowLevel)
                ShowLevel(element.Level, element.Element());
            if (shadow != null)
                shadow.SetActive(shadowState);

            if (isShowLevel)
                SetTranscendenceStar(element.TranscendenceStep, element.Grade());
            else
                SetTranscendenceStar(0, 0);

            HideAnimArrowNode();
        }

        IEnumerator SetDragonSpine(UserDragon data)
        {
            SetVisibleSpineList(false);

            var targetSpine = GetTargetSpine(data.BaseData);
            if (targetSpine != null)
            {
                if (targetSpine.Data == null)
                    targetSpine.SetData(data);
                else
                {
                    targetSpine.SetSkin(data.BaseData.KEY.ToString());
                    targetSpine.SetTranscendEffect(data.TranscendenceStep);
                }

                targetSpine.SetAnimation(eSpineAnimation.IDLE);

                yield return SBDefine.GetWaitForEndOfFrame();
                targetSpine.gameObject.SetActive(true);
            }
        }

        UIDragonSpine GetTargetSpine(CharBaseData _baseData)
        {
            if (_baseData == null)
                return null;

            if (dragonSpineList != null && dragonSpineList.Count == 4)
            {
                switch (_baseData.SPINE_NAME)
                {
                    case "metatoy_1":
                        return dragonSpineList[0];
                    case "metatoy_2":
                        return dragonSpineList[1];
                    case "legendary":
                        return dragonSpineList[2];
                    case "legendary_2":
                        return dragonSpineList[3];
                }
            }

            Debug.LogError("드래곤 스파인 추가됬음? 위에 세팅 해야함!");

            if ((eDragonGrade)_baseData.GRADE != eDragonGrade.Legend) // 레전더리가 아닌 일반 드래곤에 관한 처리
                return dragonSpineList[0];
            else
                return dragonSpineList[1].GetSkeletonData().FindSkin(_baseData.KEY.ToString()) != null ? dragonSpineList[1] : dragonSpineList[2];
        }

        void SetVisibleSpineList(bool _isVisible)
        {
            if (dragonSpineList == null || dragonSpineList.Count <= 0)
                return;

            foreach (var spine in dragonSpineList)
                if (spine != null)
                    spine.gameObject.SetActive(_isVisible);
        }

        public void setEmptyData(int line, int index , bool shadowState =false)
        {
            this.line = line;
            this.index = index;

            SetClear(shadowState);
        }

        public virtual void SetClear(bool shadowState = false)
        {           
            var infoNode = SBFunc.GetChildrensByName(this.gameObject.transform, new string[] { "infopanel" });//레벨 표시 노드 끄기
            if (infoNode != null)
            {
                infoNode.gameObject.SetActive(false);
            }
            if (shadow != null)
            {
                shadow.SetActive(shadowState);
            }
            SetTranscendenceStar(0, 0);
            SetVisibleSpineList(false);
            HideAnimArrowNode();
            DragonTag = 0;
        }

        //사용자 드래곤이 아닌 깡통드래곤 커스텀 세팅
        public void SetCustomData(int dragonTag ,int level, int transcendLevel ,bool isShowLevel = false,bool isHidden  = false, bool shadowState = true)
        {
            CharBaseData CharBaseData = CharBaseData.Get(dragonTag.ToString());
            if (CharBaseData == null)
                return;

            GameObject dragonClone = Instantiate(CharBaseData.GetUIPrefab(), SpineParent);
            if (dragonClone == null)
                return;

            SBFunc.SetLayer(dragonClone, "UI");

            dragonClone.GetComponent<RectTransform>().localScale = new Vector3(4, 4, 1);
            var skin = dragonClone.GetComponent<UIDragonSpine>();
            if (skin != null)
            {
                skin.SetData(CharBaseData);
                skin.SetTranscendEffect(transcendLevel);
            }

            this.DragonTag = dragonTag;
            SetTranscendenceStar(transcendLevel, CharBaseData.GRADE);
            if (isShowLevel)
                ShowLevel(level, CharBaseData.ELEMENT);//깡통 데이터 세팅 시 레벨데이터 받아야함
            if (shadow != null)
                shadow.SetActive(shadowState);

            HideAnimArrowNode();
        }

        public void SetHiddenDragon()
        {
            var dragonClone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.UIDragonClonePath, "Hidden"), SpineParent);//깡통 드래곤 스파인 세팅 임시
            SBFunc.SetLayer(dragonClone, "UI");
            
            dragonClone.transform.localScale = new Vector3(4, 4,1);
            //var skin = dragonClone.GetComponent<UIDragonSpine>();
            //skin.SetSkin("dragon/n_000");

            this.DragonTag = -1;

            var infoNode = SBFunc.GetChildrensByName(this.gameObject.transform, new string[] { "infopanel" });//레벨 표시 노드 끄기
            if (infoNode != null)
            {
                infoNode.gameObject.SetActive(false);
            }
            dragonClone.SetActive(true);
            this.HideAnimArrowNode();
        }

        public void setCallback(func ok_cb)
        {
            if(ok_cb != null)
            {
                clickCallback = ok_cb;
            }
        }

        void ShowLevel(int level,int element)
        {
            var infoNode = SBFunc.GetChildrensByName(gameObject.transform, new string[] { "infopanel" });//레벨 표시 노드 끄기
            var levelNode = SBFunc.GetChildrensByName(infoNode.gameObject.transform, new string[] { "labelLevel" });
            var elementNode = SBFunc.GetChildrensByName(infoNode.gameObject.transform, new string[] { "iconElement" });

            if (infoNode != null)
            {
                infoNode.gameObject.SetActive(true);
            }

            if (levelNode != null)
            {
                var levelLabel = levelNode.GetComponent<Text>();
                if (levelLabel != null)
                {
                    var levelFormat = StringData.GetStringByIndex(100000056);
                    levelLabel.text = string.Format(levelFormat, level);
                }
            }

            if (elementNode != null)
            {
                var spriteComp = elementNode.GetComponent<Image>();
                if (spriteComp != null)
                {
                    spriteComp.sprite = this.GetElementIconSpriteByIndex(element);
                }
            }
        }

        Sprite GetElementIconSpriteByIndex(int e_type)
        {
            var elementStr = "";
            switch (e_type)
            {
                case 1:
                    elementStr = "fire";
                    break;
                case 2:
                    elementStr = "water";
                    break;
                case 3:
                    elementStr = "soil";
                    break;
                case 4:
                    elementStr = "wind";
                    break;
                case 5:
                    elementStr = "light";
                    break;
                case 6:
                    elementStr = "dark";
                    break;
                default:
                    break;
            }

            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", elementStr));
        }

        public virtual void ShowAnimArrowNode(bool shadowState = true)
        {
            if (this.slotAnimNode != null && slotAnimNode.activeInHierarchy == false)
            {
                slotAnimNode.gameObject.SetActive(true);
                if (shadow != null)
                {
                    shadow.SetActive(shadowState);
                }
                this.clickActive = true;
            }
        }

        public void HideAnimArrowNode(bool shadowState = false)
        {
            if (this.slotAnimNode != null && this.slotAnimNode.activeInHierarchy == true)
            {
                this.slotAnimNode.gameObject.SetActive(false);
                if (shadow != null && DragonTag == 0)
                {
                    shadow.SetActive(shadowState);
                }
            }
            this.clickActive = false;
        }

        public bool isHideArrow()
        {
            if (this.slotAnimNode != null)
            {
                return this.slotAnimNode.activeInHierarchy;
            }
            else
            {
                return false;
            }
        }

        public void onClickFrame()
        {
            if (clickCallback != null && clickActive)
            {
                SoundManager.Instance.PlaySFX("FX_BUTTON_OK");
                clickCallback(DragonTag.ToString());
            }
            else if (DragonTag > 0)
            {
                var popup = DragonManagePopup.OpenPopup(0, 1);
                popup.SetExitCallback(()=> {
                    DragonChangedEvent.Refresh();
                });
                
                popup.CurDragonTag = DragonTag;
                popup.ClearDragonInfoList();

                foreach (var tag in curBattleLine.GetList())
                {
                    popup.DragonInfoList.Add(tag);
                }
                popup.ForceUpdate();
            }
        }

        public void ShowHiddenDragonUI(bool isRegist, bool isFull,int indexCount,int totalCount)
        {
            HiddenNode.gameObject.SetActive(true);
            HiddenIconNode.gameObject.SetActive(isRegist);//아이콘 노드 세팅
            HiddenReleaseButton.gameObject.SetActive(isRegist);//해제 버튼 켜기
            HiddenRegistButton.gameObject.SetActive(!isRegist);//등록 버튼 끄기

            if (isRegist)//등록된 상태
            {
                HiddenReleaseButton.gameObject.SetActive(true);//해제 상호작용 켜기
            }
            else
            {
                HiddenRegistButton.gameObject.SetActive(!isFull);//등록 상호작용 끄기
            }
        }

        public void HideHiddenDragonUI()
        {
            if (HiddenNode != null)
            {
                HiddenNode.SetActive(false);
            }
        }
        public void ShowOnlyHideBubble()
        {
            HiddenNode.SetActive(true);
            HiddenIconNode.gameObject.SetActive(true);
            HiddenReleaseButton.gameObject.SetActive(false);
            HiddenRegistButton.gameObject.SetActive(false);
        }



        public void SetHiddenDragonButtonCallback(func regist_callback ,func release_callback)
        {
            if (regist_callback != null)
            {
                this.hiddenRegistCallback = regist_callback;
            }

            if (release_callback != null)
            {
                this.hiddenReleaseCallback = release_callback;
            }
        }

        public void onClickHiddenRegistButton()
        {
            //console.log('onClickHiddenRegistButton');

            if (this.hiddenRegistCallback != null)
            {
                this.hiddenRegistCallback(this.DragonTag.ToString());
            }
        }

        public void onClickHiddenReleaseButton()
        {
            //console.log('onClickHiddenReleaseButton');

            if (this.hiddenReleaseCallback != null)
            {
                this.hiddenReleaseCallback(this.DragonTag.ToString());
            }
        }
        public void HideShadow()
        {
            shadow.SetActive(false);
        }

        public void SetBtnNodeState(bool state)
        {
            btnNode.SetActive(state);
        }

        void SetTranscendenceStar(int starCount,int grade)
        {
            if (StarParentTr == null)
                return;

            int StepMax = CharTranscendenceData.GetStepMax((eDragonGrade)grade);
            StarParentTr.gameObject.SetActive(true);
            for (int i = 0, count = TranscendenceStars.Length; i < count; ++i)
            {
                if (TranscendenceStars[i] != null)
                {
                    TranscendenceStars[i].SetActive(i < starCount && i < StepMax);
                    
                }
                    
            }
        }

        public bool EnableDrag = true;

        bool isDraging = false;
        Vector3 originPos = Vector3.zero;
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!EnableDrag)
                return;

            if (DragonTag <= 0)
                return;

            if (isDraging)
                return;

            isDraging = true;
            originPos = transform.position;

            DragonChangedEvent.MoveStart(DragonTag);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraging)
                return;

            if (DragonTag <= 0)
                return;

            var pos = Camera.main.ScreenToWorldPoint(eventData.position);
            pos.z = transform.position.z;

            transform.position = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraging)
                return;

            isDraging = false;
            transform.position = originPos;

            if (DragonTag <= 0)
                return;

            DragonChangedEvent.MoveDone(DragonTag, eventData.position);
        }
    }
}
