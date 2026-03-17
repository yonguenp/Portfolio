using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetCompoundResultPopup : Popup<PetPopupData>
    {
        [Header("Data")]
        [SerializeField]
        private GameObject dataContainer = null;

        [Space(10)]
        [Header("UI")]
        [SerializeField]
        private Text labelTitle = null;
        [SerializeField]
        private HorizontalLayoutGroup nameLayout = null;
        [SerializeField]
        private Text labelName = null;
        [SerializeField]
        private GameObject nodeNew = null;
        [SerializeField]
        private GameObject[] gradeNodes = null;
        [SerializeField]
        private GameObject[] elementNodes = null;
        [SerializeField]
        private GameObject petPortraitPrefab = null;

        [SerializeField]
        private GameObject petSpinePrefab = null;

        [Space(10)]
        [Header("StatInfoNode")]
        [SerializeField]
        private GameObject statNode = null;
        [SerializeField]
        private Text[] statNameText = null;
        [SerializeField]
        private Text[] statValueText =  null;

        [SerializeField]
        List<DragonPartStatSlot> statSlotList = new List<DragonPartStatSlot>();

        [SerializeField]
        List<GameObject> effectNodeList = new List<GameObject>();

        [SerializeField]
        GameObject closeBtn = null;

        bool isAvailableClose = false;
        public override void InitUI()
        {
            isAvailableClose = false;
            if (closeBtn != null)
                closeBtn.SetActive(false);

            if (Data.IsNew)
            {
                if (Data.IsNew)
                {
                    if (labelTitle != null)
                    {
                        var str = StringData.GetStringByIndex(100001136);//100002081
                        labelTitle.text = str;
                    }
                    if (nameLayout != null)
                    {
                        nameLayout.spacing = -10;
                    }
                }
                else
                {
                    if (labelTitle != null)
                    {
                        var str = StringData.GetStringByIndex(100001136);//100002080
                        labelTitle.text = str;
                    }
                    if (nameLayout != null)
                    {
                        nameLayout.spacing = 5;
                    }
                }
                if (nodeNew != null)
                {
                    nodeNew.SetActive(Data.IsNew);
                }
            }

            PetBaseData petBaseData = Data.PetData;

            if (gradeNodes != null)
            {
                var count = gradeNodes.Length;
                for (var i = 0; i < count; ++i)
                {
                    var curNode = gradeNodes[i];
                    if (curNode == null)
                    {
                        continue;
                    }

                    curNode.SetActive(false);
                }
            }

            if (labelName != null)
            {
                labelName.text = StringData.GetStringByStrKey(petBaseData._NAME);
            }
            foreach(var node in gradeNodes)
            {
                node.SetActive(false);
            }
            foreach (var node in elementNodes)
            {
                node.SetActive(false);
            }
            gradeNodes[petBaseData.GRADE - 1].SetActive(true);
            elementNodes[petBaseData.ELEMENT - 1].SetActive(true);

            if (dataContainer != null)
                SBFunc.RemoveAllChildrens(dataContainer.transform);

            if (petBaseData != null)
            {
                var clone = Instantiate(petSpinePrefab, dataContainer.transform);
                GachaSpineObject go = clone.GetComponent<GachaSpineObject>();
                if (go != null)
                {
                    go.Init(petBaseData.KEY,
                        StringData.GetStringByStrKey(petBaseData._NAME),
                        petBaseData.GRADE,
                        eGachaType.PET,
                        petBaseData.SKIN, false);

                    go.gameObject.SetActive(false);
                    go.gameObject.SetActive(true);
                    go.OnShow();
                    //go.SkipCoverAnimation();
                }

                SetStatData();

                foreach (var statNode in statSlotList)
                {
                    if (statNode == null)
                        continue;

                    statNode?.gameObject.SetActive(false);
                    statNode.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }

                foreach (var effectnode in effectNodeList)
                    effectnode?.SetActive(false);

                var tempSeq = DOTween.Sequence();
                tempSeq.AppendCallback(()=> {
                    foreach (var effectnode in effectNodeList)
                        effectnode?.SetActive(true);
                }).AppendInterval(1f).AppendCallback(()=> {
                    go.gameObject.GetComponent<RectTransform>().DOAnchorPosY(160f, 0.3f).SetEase(Ease.InOutQuad).Play();
                })
               .AppendCallback(() => {
                   var tempSeq2 = DOTween.Sequence();
                   tempSeq2.AppendCallback(() =>
                   {
                       statSlotList[0].GetComponent<CanvasGroup>().alpha = 0;
                       statSlotList[0].gameObject.SetActive(true);
                       statSlotList[0].GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
                   }).AppendInterval(0.3f)
                   .AppendCallback(() =>
                   {
                       statSlotList[1].GetComponent<CanvasGroup>().alpha = 0;
                       statSlotList[1].gameObject.SetActive(true);
                       statSlotList[1].GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
                       statSlotList[1].GetComponent<RectTransform>().DOAnchorPosY(-100, 0.5f);
                   }).AppendInterval(0.3f)
                   .AppendCallback(() =>
                   {
                       statSlotList[2].GetComponent<CanvasGroup>().alpha = 0;
                       statSlotList[2].gameObject.SetActive(true);
                       statSlotList[2].GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
                       statSlotList[2].GetComponent<RectTransform>().DOAnchorPosY(-200, 0.5f);
                   }).AppendInterval(0.3f)
                   .AppendCallback(() =>
                   {
                       statSlotList[3].GetComponent<CanvasGroup>().alpha = 0;
                       statSlotList[3].gameObject.SetActive(true);
                       statSlotList[3].GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
                       statSlotList[3].GetComponent<RectTransform>().DOAnchorPosY(-300, 0.5f);
                   }).AppendInterval(0.5f).AppendCallback(()=> {

                       isAvailableClose = true;
                       if (closeBtn != null)
                           closeBtn.SetActive(true);
                   });
                   tempSeq2.Play();

               }).Play();
            }
        }

        void SetStatData()
        {
            var petTag = Data.PetTag;
            var petInfo = User.Instance.PetData.GetPet(petTag);
            if (petInfo != null)
            {
                int petReinforce = petInfo.Reinforce;
                var petLv = petInfo.Level;

                foreach (var statNode in statSlotList)
                {
                    if (statNode == null)
                        continue;

                    statNode.GetComponent<DragonPartStatSlot>().SetData("", 0, false, true);
                }

                if (petInfo.Stats != null)
                {
                    var statCount = petInfo.Stats.Count;

                    for (int i = 0; i < statCount; ++i)
                    {
                        var stat = petInfo.Stats[i];
                        if (stat == null)
                            continue;

                        string statKey = stat.Key.ToString();
                        PetStatData data = PetStatData.Get(statKey);
                        bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;

                        var statValue = PetStatData.GetStatValue(statKey, petLv, petReinforce, stat.IsStatus1);

                        statSlotList[i].GetComponent<DragonPartStatSlot>().SetData(StatTypeData.GetDescStringByStatType(data.STAT_TYPE, isPercent), statValue, isPercent, false ,true);
                    }
                }
            }
        }


        void SetBaseStatInfo(int index, string statType, float optionValue, bool isOptionValuePercent = false)
        {
            if (statNameText.Length > index && index >= 0)
            {
                statNameText[index].gameObject.SetActive(true);
                statValueText[index].gameObject.SetActive(true);
                statNameText[index].text = StatTypeData.GetDescStringByStatType(statType, isOptionValuePercent);
                string optionValueString = "+" + optionValue.ToString();
                if (isOptionValuePercent) optionValueString += "%";
                statValueText[index].text = optionValueString;

            }
        }
        GameObject MakePetSpine()
        {
            GameObject node = null;
            var petBaseData = Data.PetData;
            if(petBaseData == null)
            {
                return node; 
            }
            var petName = petBaseData.IMAGE;
            var pet = SBFunc.GetPetSpineByName(petName, eSpineType.UI);
            if (pet != null)
            {
                node = Instantiate(pet, this.dataContainer.transform);
                node.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
                node.GetComponent<RectTransform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);

                var petSpine = node.GetComponent<UIPetSpine>();
                if (petSpine != null)
                {
                    petSpine.Init();
                    if (petBaseData.SKIN != "NONE")
                    {
                        petSpine.SetSkin(petBaseData.SKIN);
                    }
                }
            }
            return node;
        }

        GameObject MakePetPortrait()
        {
            var petTag =Data.PetTag;
            var node = Instantiate(petPortraitPrefab,dataContainer.transform);
            node.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            var frame = node.GetComponent<PetPortraitFrame>();
            if (frame != null)
            {
                var petInfo = User.Instance.PetData.GetPet(petTag);
                if (petInfo != null)
                {
                    frame.SetPetPortraitFrame(petInfo);
                }
            }
            return node;
        }

        public override void OnClickDimd()
        {
            if (!isAvailableClose)
                return;

            base.OnClickDimd();
        }

        public override void ClosePopup()
        {
            if (!isAvailableClose)
                return;

            base.ClosePopup();
        }

    }
}

