using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonCompoundInfoData
    {
        public bool isNew { get; private set; } = false;
        public bool isSuccess { get; private set; } = false;
        public int dragonID;

        private CharBaseData dragonCharBaseData = null;
        public CharBaseData DragonCharBaseData { get { return dragonCharBaseData; } }

        public DragonCompoundInfoData(int _dragonID , bool _isSuccess, bool _isNew)
        {
            dragonID = _dragonID;
            isSuccess = _isSuccess;
            isNew = _isNew;
            dragonCharBaseData = CharBaseData.Get(_dragonID);
        }
    }

    public class DragonCompoundResultPopupData : PopupData
    {
        private List<DragonCompoundInfoData> resultList = null;
        public List<DragonCompoundInfoData> ResultList { get { return resultList; } }
        public Action Callback { get; private set; } = null;

        public string TitleString;
        public DragonCompoundResultPopupData(List<DragonCompoundInfoData> _infoList, Action cb = null, string titleString = "")
        {
            if (resultList == null)
                resultList = new List<DragonCompoundInfoData>();
            resultList = _infoList.ToList();
            Callback = cb;
            TitleString = titleString;
        }
    }

    public class DragonCompoundResultPopup : Popup<DragonCompoundResultPopupData>
    {
        //가챠 결과 방식으로 변경
        //[Header("prefab")]
        //[SerializeField]
        //private GameObject slotPrefab = null;

        //[SerializeField]
        //private GameObject prefabParent = null;

        //[SerializeField]
        //private List<DragonCompoundResultSlot> slotList = new List<DragonCompoundResultSlot>();


        [SerializeField]
        private GameObject contentParent = null;
        [SerializeField]
        private GameObject portraitPrefab = null;
        [SerializeField]
        private GameObject spinePrefab = null;

        [SerializeField]
        GameObject closeBtn = null;

        [Space(10)]
        [Header("UI")]
        [SerializeField]
        private Text labelTitle = null;

        [SerializeField] GameObject capsultProductionPrefab = null;//캡슐 연출용 프리펩
        [SerializeField] GameObject capsuleProductionParent = null;//캡슐 연출 프리펩 부모 위치
        private CapsuleResultProduction capsuleProduction = null;

        bool isClickAvailable = false;
        Coroutine closeBtnCo = null;


        private void OnDisable()
        {
            if (closeBtnCo != null)
            {
                StopCoroutine(closeBtnCo);
            }
            closeBtnCo = null;
        }

        public override void InitUI()
        {
            var infoDataList = Data.ResultList;
            if (infoDataList == null || infoDataList.Count <= 0) return;

            if(labelTitle != null)
            {
                if (string.IsNullOrEmpty(Data.TitleString))
                    labelTitle.text = StringData.GetStringByIndex(100001136);//합성 결과
                else
                    labelTitle.text = Data.TitleString;
            }

            isClickAvailable = false;

            if (closeBtn != null)
                closeBtn.gameObject.SetActive(false);

            PlayCapsuleProduction();
        }

        void DrawAllData(List<DragonCompoundInfoData> _list)
        {
            SBFunc.RemoveAllChildrens(contentParent.transform);

            var isSingleCompound = _list.Count <= 1;

            for (int i = 0; i < _list.Count; i++)
            {
                var compoundData = _list[i];
                if (compoundData == null)
                    continue;

                var isNew = compoundData.isNew;
                var dTag = compoundData.dragonID;
                var charBaseData = compoundData.DragonCharBaseData;
                if(isNew)
                {
                    var clone = Instantiate(spinePrefab, contentParent.transform);

                    var bottomObject = SBFunc.GetChildrensByName(clone.transform, new string[] { "Bottom" });
                    var gradeObject = SBFunc.GetChildrensByName(clone.transform, new string[] { "Grade" });

                    if (bottomObject != null)
                        bottomObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(8, -109.5f);
                    if(gradeObject != null)
                        gradeObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(8, -122);

                    GachaSpineObject go = clone.GetComponent<GachaSpineObject>();
                    if (go != null)
                    {
                        go.Init(dTag,
                            StringData.GetStringByStrKey(charBaseData._NAME),
                            charBaseData.GRADE,
                            eGachaType.DRAGON,
                            charBaseData.SKIN, false, isNew);

                        if(isSingleCompound)
                        {
                            go.gameObject.SetActive(false);
                            go.gameObject.SetActive(true);
                            go.OnShow();
                        }    
                        else
                            go.SkipCoverAnimation();
                    }
                }
                else
                {
                    var clone = Instantiate(portraitPrefab, contentParent.transform);
                    GachaPortraitObject go = clone.GetComponent<GachaPortraitObject>();
                    if (go != null)
                    {
                        string bgPath = SBFunc.StrBuilder("bggrade_", StringData.GetStringByIndex(CharGradeData.Get(charBaseData.GRADE)._NAME).ToLower());

                        go.Init(StringData.GetStringByStrKey(charBaseData._NAME),
                            charBaseData.GetThumbnail(),
                            ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, charBaseData.BACKGROUND == "NONE" ? bgPath : charBaseData.BACKGROUND),
                            charBaseData.GRADE,
                            ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", SBDefine.ConvertToElementString(charBaseData.ELEMENT))));

                        if (isSingleCompound)
                        {
                            go.gameObject.SetActive(false);
                            go.gameObject.SetActive(true);
                            go.OnShow();
                        }
                        else
                            go.SkipCoverAnimation();
                    }
                }
            }
        }

        void PlayCapsuleProduction()
        {
            SetCapsulePrefab();
            InitCapsuleProduction();

            if (capsuleProduction)//캡슐 연출 넘기기
            {
                var results = ConvertGachaListByCompoundList();
                results.Sort((x, y) => {
                    int ret = ((GachaResultDragonAndPet)y).GRADE.CompareTo(((GachaResultDragonAndPet)x).GRADE);
                    if (ret == 0)
                    {
                        ret = ((GachaResultDragonAndPet)y).IsNew.CompareTo(((GachaResultDragonAndPet)x).IsNew);
                    }
                    return ret;
                });//결과 데이터 오름차순 정렬

                capsuleProduction.SetCapsulePositioningSpine(results, eGachaType.DRAGON, () => {
                    closeBtnCo = StartCoroutine(VisibleOkButtonCo());
                });//캡슐 다발 연출
            }
        }

        List<GachaResult> ConvertGachaListByCompoundList()
        {
            var targetDataList = Data.ResultList;
            List<GachaResult> resultList = new List<GachaResult>();

            foreach(var data in targetDataList)
            {
                if (data == null)
                    continue;

                var dragonID = data.dragonID;
                GachaResult result = new GachaResultDragonSpine(dragonID, spinePrefab, data.isNew);
                resultList.Add(result);
            }

            return resultList;
        }

        void SetCapsulePrefab()
        {
            if (capsuleProductionParent != null)
            {
                SBFunc.RemoveAllChildrens(capsuleProductionParent.transform);//캡슐 연출 프리펩 삭제
            }

            var capsuleProductionClone = Instantiate(capsultProductionPrefab, capsuleProductionParent.transform);
            capsuleProduction = capsuleProductionClone.GetComponent<CapsuleResultProduction>();

            capsuleProduction.gameObject.SetActive(false);
        }

        void InitCapsuleProduction()
        {
            if (capsuleProduction != null)
                capsuleProduction.InitProduction();
        }

        public void OnClickFrame(DragonCardFrame frame, UserDragonCard cardData) { //드래곤 설명
        }

        public void ForceUpdate() 
        {
    
        }
        public override void ClosePopup()
        {
            if (!isClickAvailable)
                return;

            if (Data != null)
            {
                if (Data.Callback != null)
                    Data.Callback.Invoke();
            }

            base.ClosePopup();
        }

        public override void OnClickDimd()
        {
            if (!isClickAvailable)
                return;

            base.OnClickDimd();
        }
        IEnumerator VisibleOkButtonCo()
        {
            yield return SBDefine.GetWaitForSeconds(1f);

            if (closeBtn != null)
                closeBtn.SetActive(true);

            isClickAvailable = true;
        }
    }
}
