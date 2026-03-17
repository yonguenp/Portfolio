using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork { 
    public class WorldSelectPopup : MonoBehaviour
    {
        [SerializeField] ScrollRect targetScroll = null;
        [SerializeField] List<WorldSelectSlot> worldList = new List<WorldSelectSlot>();
        [SerializeField] private Sprite[] worldImgs;
        [SerializeField] private StageScene StageScene = null;
        private RectTransform contentRect = null;
        IEnumerator moveCo = null;
        [SerializeField] Text DiffcultTitle;

        [SerializeField] Sprite Selected;
        [SerializeField] Sprite Normal;
        [SerializeField] Sprite Disable;

        [SerializeField] Button NormalButton;
        [SerializeField] Button HardButton;
        [SerializeField] Button HellButton;

        [SerializeField] GameObject NormalBG;
        [SerializeField] GameObject HardBG;
        [SerializeField] GameObject HellBG;

        StageDifficult difficult = StageDifficult.NONE;
        public void Init(int currentWorldIndex = 1)
        {
            PopupManager.AllClosePopup(); // UIType 이 하나라도 보이면 안됨
            UIManager.Instance.InitUI(eUIType.None);

            difficult = (StageDifficult)CacheUserData.GetInt("adventure_difficult", 1);
            RefreshDifficult(true);

            InitWorldList();

            if (contentRect == null)
                contentRect = targetScroll.content;

            if (moveCo != null)
            {
                StopCoroutine(moveCo);
                moveCo = null;
            }

            moveCo = ScrollMoveCo(currentWorldIndex);
            StartCoroutine(moveCo);
        }

        void InitWorldList()
        {
            for (int i = 0; i < worldList.Count; i++)
            {
                var worldSelectComp = worldList[i];
                if (worldSelectComp == null)
                    continue;
                worldSelectComp.SetWorldSprite(worldImgs[i]);
                worldSelectComp.Init(i + 1);
                worldSelectComp.RefreshWorldInfoSlot();
            }
        }


        public void SelectOff()
        {
            UIManager.Instance.InitUI(eUIType.Adventure);
            gameObject.SetActive(false);
        }
     
        void TweenScrollItemCenter(int worldIndex)
        {
            if (contentRect == null || targetScroll == null)
                return;

            var children = SBFunc.GetChildren(contentRect.gameObject);
            if (children == null || children.Length <= 0)
                return;

            RectTransform tempRect = null;
            for(int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null)
                    continue;

                var worldSlot = child.GetComponent<WorldSelectSlot>();
                if (worldSlot == null)
                    continue;

                var slotIndex = worldSlot.WorldIndex;
                if(worldIndex == slotIndex)
                {
                    tempRect = worldSlot.gameObject.GetComponent<RectTransform>();
                    break;
                }
            }

            if (tempRect != null && targetScroll != null)
                targetScroll.FocusOnItem(tempRect, 0.2f);
        }

        IEnumerator ScrollMoveCo(int worldIndex)
        {
            while(true)
            {
                yield return SBDefine.GetWaitForSeconds(0.1f);
                TweenScrollItemCenter(worldIndex);
                yield break;
            }
        }

        public void OnSelectDifficult(int diff)
        {
            if (!IsSelectableDifficult(diff))
            {
                ToastManager.On(StringData.GetStringByStrKey("난이도선택오류"));
                return;
            }

            CacheUserData.SetInt("adventure_difficult", diff);
            difficult = (StageDifficult)diff;

            RefreshDifficult();
        }

        void RefreshDifficult(bool init = false)
        {
            if (!IsSelectableDifficult((int)difficult))
            {
                CacheUserData.SetInt("adventure_difficult", 1);
                difficult = StageDifficult.NORMAL;
            }

            switch (difficult)
            {
                case StageDifficult.HARD:
                    (NormalButton.targetGraphic as Image).sprite = Normal;
                    (HardButton.targetGraphic as Image).sprite = Selected;
                    (HellButton.targetGraphic as Image).sprite = IsSelectableDifficult(3) ? Normal : Disable;
                    NormalBG.SetActive(false);
                    HardBG.SetActive(true);
                    HellBG.SetActive(false);
                    DiffcultTitle.text = StringData.GetStringByStrKey("어려움난이도");
                    break;
                case StageDifficult.HELL:
                    (NormalButton.targetGraphic as Image).sprite = Normal;
                    (HardButton.targetGraphic as Image).sprite = IsSelectableDifficult(2) ? Normal : Disable;
                    (HellButton.targetGraphic as Image).sprite = Selected;
                    NormalBG.SetActive(false);
                    HardBG.SetActive(false);
                    HellBG.SetActive(true);
                    DiffcultTitle.text = StringData.GetStringByStrKey("지옥난이도");
                    break;
                default:
                case StageDifficult.NORMAL:
                    (NormalButton.targetGraphic as Image).sprite = Selected;
                    (HardButton.targetGraphic as Image).sprite = IsSelectableDifficult(2) ? Normal : Disable;
                    (HellButton.targetGraphic as Image).sprite = IsSelectableDifficult(3) ? Normal : Disable;
                    NormalBG.SetActive(true);
                    HardBG.SetActive(false);
                    HellBG.SetActive(false);
                    DiffcultTitle.text = StringData.GetStringByStrKey("보통난이도");
                    break;
            }

            if (!init)
            {
                int world = StageManager.Instance.AdventureProgressData.GetLatestWorld((int)difficult);
                StageManager.Instance.SetWorld(world);
                StageScene.ChangeWorld(world, (int)difficult);
                InitWorldList();
                gameObject.SetActive(true);

                TweenScrollItemCenter(world);
            }
        }

        bool IsSelectableDifficult(int diff)
        {
            //유저가 선택가능한 diff인지 확인 추가할것
            switch(diff)
            {
                case 1:
                    return true;

                case 2:
                {
                    var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(8, 1);
                    if (worldInfo != null && worldInfo.GetLastestClearStage() > 0)
                    {
                        return worldInfo.IsWorldClear();
                    }
                    return false;
                }
                case 3:
                {
                    var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(8, 2);
                    if (worldInfo != null && worldInfo.GetLastestClearStage() > 0)
                    {
                        return worldInfo.IsWorldClear();
                    }
                    return false;
                }
                default:
                    return false;
            }
        }
    }
}
