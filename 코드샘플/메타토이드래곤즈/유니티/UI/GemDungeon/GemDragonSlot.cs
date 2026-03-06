using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GemDragonSlot : MonoBehaviour
    {
        [SerializeField] Text fatigueGageText;
        [SerializeField] Slider fatigueSlider;
        [SerializeField] Sprite sliderFillEnoughSprite;
        [SerializeField] Sprite sliderFillLeakSprite;
        [SerializeField] GameObject noFatigueMark;
        [SerializeField] TimeObject fatigueTimeObj;

        [Header("lock slot")]
        [SerializeField] Button LockSlotBtn;

        [Header("add Slot")]
        [SerializeField] Image AddSlotNeedItemIcon;
        [SerializeField] Text AddSlotNeedItemAmountText;
        [SerializeField] Button AddSlotBtn;

        [Header("layers")]
        [SerializeField] GameObject dragonlayer;
        [SerializeField] GameObject slotAddLayer;
        [SerializeField] GameObject emptyLayer;
        [SerializeField] GameObject lockLayer;

        [Header("Dragon layer")]
        
        [SerializeField] Image dragonImg;
        [SerializeField] Image dragonBack;
        [SerializeField] Image elementIcon;
        [SerializeField] Image classIcon;
        [SerializeField] Text lvText;
        [SerializeField] SlotFrameController frame;
        [SerializeField] Text battlePointText;
        [SerializeField] GameObject dimmedObj;
        [SerializeField] CanvasGroup healEffect;
        [SerializeField] Image healEffectProgressBarImg;
        [SerializeField] Sprite[] customGradeBGList;
        [SerializeField] Sprite defaultCustomImage;
        [SerializeField] GameObject[] transcendStar = null;

        [Space(10)]
        [SerializeField] Image SliderFillImg;


        LandmarkGemDungeonDragon Data = null;

        Tween sliderSizeTween = null;
        Tween healTween = null;
        public eGemDungeonSlotState CurState { get; private set; } = eGemDungeonSlotState.Empty;

        public int DragonTag { get; private set; } = 0;

        private int lastDragonTag = 0;

        LandmarkGemDungeonFloor FloorData { get; set; } = null;

        bool isLowFatigue = false;

        private readonly int lowFatigueStandard = 20;
        private int lastestFatigue = -1;
        public void Init(LandmarkGemDungeonFloor floor, eGemDungeonSlotState state = eGemDungeonSlotState.Empty, LandmarkGemDungeonDragon data = null, int curMaxSlotNum=0)
        {
            SetAllLayerOff();
            FloorData = floor;
            CurState = state;
            fatigueSlider.gameObject.SetActive(false);
            noFatigueMark.SetActive(false);
            switch (state)
            {
                case eGemDungeonSlotState.Empty:
                    emptyLayer.SetActive(true);
                    fatigueSlider.value = 0f;
                    fatigueSlider.maxValue = 1f;
                    lastestFatigue = -1;
                    lastDragonTag = 0;
                    break;
                case eGemDungeonSlotState.DragonExist:
                    Data = data;
                    SetDragonInfo();
                    break;
                case eGemDungeonSlotState.AddSlot:
                    SetAddSlotLayer(curMaxSlotNum);
                    lastestFatigue = -1;
                    lastDragonTag = 0;
                    break;
                case eGemDungeonSlotState.Lock:
                default:
                    SetLockSlotLayer(curMaxSlotNum);
                    lastestFatigue = -1;
                    lastDragonTag = 0;
                    break;
            }
        }

        private void OnDisable()
        {
            KillTween();
        }

        void SetAllLayerOff()
        {
            dragonlayer.SetActive(false);
            slotAddLayer.SetActive(false);
            emptyLayer.SetActive(false);
            lockLayer.SetActive(false);
            KillTween();
        }

        void SetLockSlotLayer(int curMaxSlot)
        {
            lockLayer.SetActive(true);
            fatigueSlider.value = 0f;
            fatigueSlider.maxValue = 1f;
            int maxSlot = BuildingBaseData.Get("gemdungeon").MAX_SLOT;
            int startSlot = BuildingBaseData.Get("gemdungeon").START_SLOT;
            SlotCostData slotCostInfo = SlotCostData.GetByType(eSlotCostInfoType.GemDungeonDragon, (curMaxSlot - startSlot) + 1);

            if (curMaxSlot < maxSlot && slotCostInfo != null)
            {
                int price = slotCostInfo.COST_NUM;
                AddSlotNeedItemAmountText.text = price.ToString();
                ePriceDataFlag priceDataFlag = ePriceDataFlag.CloseBtn | ePriceDataFlag.ContentBG;
                switch (slotCostInfo.COST_TYPE.ToUpper())
                {
                    case "GOLD":
                        AddSlotNeedItemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                        priceDataFlag |= ePriceDataFlag.Gold;
                        break;
                    case "GEMSTONE":
                        AddSlotNeedItemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                        priceDataFlag |= ePriceDataFlag.GemStone;
                        break;
                }
                LockSlotBtn.onClick.RemoveAllListeners();
                LockSlotBtn.onClick.AddListener(() => PricePopup.OpenPopup(StringData.GetStringByStrKey("슬롯확장팝업제목"), string.Empty, StringData.GetStringByStrKey("슬롯확장팝업내용"), price, priceDataFlag, AddSlot));
            }
        }
        void SetAddSlotLayer(int curSlotNum)
        {
            slotAddLayer.SetActive(true);
            fatigueSlider.value = 0f;
            fatigueSlider.maxValue = 1f;
            int maxSlot = BuildingBaseData.Get("gemdungeon").MAX_SLOT;
            int startSlot = BuildingBaseData.Get("gemdungeon").START_SLOT;
            SlotCostData slotCostInfo = SlotCostData.GetByType(eSlotCostInfoType.GemDungeonDragon, (curSlotNum - startSlot) + 1);
            
            if (curSlotNum < maxSlot && slotCostInfo != null)
            {
                int price = slotCostInfo.COST_NUM;
                AddSlotNeedItemAmountText.text = price.ToString();
                ePriceDataFlag priceDataFlag = ePriceDataFlag.CloseBtn | ePriceDataFlag.ContentBG;
                switch (slotCostInfo.COST_TYPE.ToUpper())
                {
                    case "GOLD":
                        AddSlotNeedItemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                        priceDataFlag |= ePriceDataFlag.Gold;
                        break;
                    case "GEMSTONE":
                        AddSlotNeedItemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                        priceDataFlag |= ePriceDataFlag.GemStone;
                        break;
                }
                AddSlotBtn.onClick.RemoveAllListeners();
                AddSlotBtn.onClick.AddListener(() => PricePopup.OpenPopup(StringData.GetStringByStrKey("슬롯확장팝업제목"), string.Empty, StringData.GetStringByStrKey("슬롯확장팝업내용"), price, priceDataFlag, AddSlot));
            }
            //AddSlotNeedItemIcon
            //AddSlotNeedItemAmountText
        }

        void AddSlot()
        {
            WWWForm param = new WWWForm();
            param.AddField("floor", FloorData.Floor);
            PopupManager.ClosePopup<PricePopup>();
            NetworkManager.Send("gemdungeon/slotopen", param, (JObject jsonData) =>
            {
                PopupManager.GetPopup<GemDungeonPopup>().Refresh();
            });
        }

        void SetDragonInfo()
        {
            if (Data == null)
                return;

            int maxFatigue = GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX");
            int dragonTag = Data.DragonNo;
            if (lastestFatigue > -1 && lastestFatigue < maxFatigue && Data.ExpectedFatigue == maxFatigue && dragonTag != 0 && lastDragonTag == dragonTag) // 회복 후 연출
            {
                if(healTween != null)
                {
                    healTween.Kill();
                }
                var seq = DOTween.Sequence();
                healEffect.gameObject.SetActive(true);
                healEffect.alpha = 0;
                healEffectProgressBarImg.gameObject.SetActive(true);
                healEffectProgressBarImg.color = new Color(255, 255, 255, 0);
                seq.Append(healEffect.DOFade(1f, 0.2f))
                    .Join(healEffectProgressBarImg.DOFade(1f, 0.2f))
                    .Append(healEffect.DOFade(0f, 0.4f))
                    .Join(healEffectProgressBarImg.DOFade(0f, 0.4f))
                    .AppendCallback(() => {
                    healEffectProgressBarImg.gameObject.SetActive(false);
                    healEffect.gameObject.SetActive(false);
                    }
                );
                healTween = seq.Play();
            }
            lastDragonTag = dragonTag;

            DragonTag = Data.DragonNo;
            UserDragon userDragon = User.Instance.DragonData.GetDragon(DragonTag);
            if (userDragon == null)
                return;
            fatigueSlider.gameObject.SetActive(true);
            lvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", userDragon.Level);
            for(int i = 0; i < transcendStar.Length; ++i)
            {
                transcendStar[i].SetActive(i<userDragon.TranscendenceStep);
            }
            var charData = CharBaseData.Get(DragonTag);
            int grade = charData.GRADE;
            dragonImg.sprite = charData.GetThumbnail();
            dragonBack.sprite = GetCustomGradeBG(charData.GRADE);
            frame.SetColor(grade);
            SetClassIcon(charData);
            //SetElementSpr(charData != null ? charData.ELEMENT : 0);
            elementIcon.gameObject.SetActive(false);
            dragonlayer.SetActive(true);
            
            fatigueSlider.maxValue = maxFatigue;
            battlePointText.text = "0";
            switch (FloorData.State)
            {
                case eGemDungeonState.BATTLE:
                {
                    fatigueSlider.value = Data.ExpectedFatigue;
                    battlePointText.text = FloorData.GetSpecificBattlePoint(DragonTag).ToString();
                    fatigueGageText.text = (Data.ExpectedFatigue / (float)maxFatigue).ToString("P0");
                    if (Data.ExpectedFatigue > 0)
                    {
                        isLowFatigue = Data.ExpectedFatigue <= lowFatigueStandard;
                        SliderFillImg.sprite = isLowFatigue ? sliderFillLeakSprite : sliderFillEnoughSprite;
                        sliderSizeTween = fatigueSlider.transform.DOScale(Vector3.one * 1.05f, 0.5f).SetDelay(0.7f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                        SetFatigueState(true);
                        fatigueTimeObj.Refresh = () => {
                            int curFatigue = Data.ExpectedFatigue;
                            fatigueSlider.value = curFatigue;
                            fatigueGageText.text = (curFatigue/ (float)maxFatigue).ToString("P0");
                            lastestFatigue = curFatigue;
                            if (isLowFatigue == false && curFatigue <= lowFatigueStandard)
                            {
                                isLowFatigue = true;
                            }
                            if (curFatigue <= 0)
                            {
                                KillTween();
                                lastestFatigue = 0;
                                fatigueTimeObj.Refresh = null;
                                PopupManager.GetPopup<GemDungeonPopup>().Refresh();
                                SetFatigueState(false);
                            }
                        };
                    }
                    else
                    {
                        SetFatigueState(false);
                        lastestFatigue = 0;
                    }
                } break;
                default:
                {
                    fatigueSlider.value = Data.Fatigue;
                    fatigueGageText.text = (Data.Fatigue / (float)maxFatigue).ToString("P0");
                    SetFatigueState(Data.Fatigue > 0);
                } break;
            }
        }

        void KillTween()
        {
            if(sliderSizeTween != null)
            {
                sliderSizeTween.Kill();
                sliderSizeTween = null;
                fatigueSlider.transform.localScale = Vector3.one;
            }
            if(healTween != null)
            {
                healTween.Kill();
                healEffect.gameObject.SetActive(false);
                healEffectProgressBarImg.gameObject.SetActive(false);
            }
        }

        void SetElementSpr(int _elementType)
        {
            if (elementIcon != null)
            {
                if (_elementType <= 0)
                {
                    elementIcon.gameObject.SetActive(false);
                }
                else
                {
                    elementIcon.gameObject.SetActive(true);
                }
                var elementFrame = GetElementIconSpriteByIndex(_elementType);
                elementIcon.sprite = elementFrame;
            }
        }
        private Sprite GetElementIconSpriteByIndex(int e_type)
        {
            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("icon_property_", SBDefine.ConvertToElementString(e_type)));
        }
        void SetClassIcon(CharBaseData dragonInfo)
        {
            if (classIcon == null) return;

            if (dragonInfo == null)
            {
                classIcon.gameObject.SetActive(false);
                return;
            }
            classIcon.sprite = dragonInfo.GetClassIcon();
        }

        Sprite GetCustomGradeBG(int grade)
        {
            if (grade > 0 && grade - 1 < customGradeBGList.Length)
            {
                return customGradeBGList[grade - 1];
            }
            else
                return defaultCustomImage;
        }
        void SetFatigueState(bool isRemainFatigue)
        {
            noFatigueMark.SetActive(!isRemainFatigue);
            dimmedObj.SetActive(!isRemainFatigue);
        }

        public void OnClickDragonLayer() 
        {
            int maxFatigue = GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX");
            if (Mathf.CeilToInt(Data.ExpectedFatigue / (float)maxFatigue * SBDefine.BASE_FLOAT) == SBDefine.BASE_FLOAT)
            {
                ToastManager.On(StringData.GetStringByStrKey("스태미나최대"));
                return;
            }
            if (FloorData.IsFullReward)
            {
                ToastManager.On(StringData.GetStringByStrKey("잼던전보상수령경고"));
                return;
            }
            PopupManager.OpenPopup<GemDungeonHealUsePopup>(new GemDungeonHealPopupData(DragonTag));
            
        }
        public void OnClickEmptyLayer()
        {
            if (FloorData.IsFullReward)
            {
                ToastManager.On(StringData.GetStringByStrKey("잼던전보상수령경고"));
                return;
            }
            PopupManager.GetPopup<GemDungeonPopup>().OnClickTeamSet();
        }

        public void OnClickBurnMark()
        {
            PopupManager.OpenPopup<GemDungeonHealUsePopup>(new GemDungeonHealPopupData(DragonTag));
        }
    }
}

