using Coffee.UIExtensions;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct GachaEvent
    {
        public enum GachaEventEnum
        {
            InitProduction,//연출 초기화 - UI외부
            IdleProduction,//연출 기본세팅 - UI 외부

            SendGachaResult,//UI 쪽에서 응답받은 결과를 보냄
            SendGachaTypeDataResult,//UI 쪽에서 응답받은 결과를 보냄

            CheckCapsuleOpenCount,//ProductionCapsule 객체에서 CapsuleResultProduction 으로 캡슐 전부 오픈했는지 체크용도
            CapsuleIndexOpen,//애니메이션이랑 소스 안쪽에서 켜는 것이랑 타이밍 안맞아서 유니티 애니메이션 클립 이벤트로 변경하려함.
            CapsuleOpenAnimationComplete,//animator Controller를 통해서 클립 이벤트 발송 객체명 : ProductionCapsuleCompleteAnimation
            CapsuleAnimationShowEffect,//드래곤 및 펫 스파인 등장 연출 이후에 이펙트 요청 이벤트

            EndProduction,//연출 다끝나고 UI 쪽으로 쏨
            SetUIVisible,//UI 쪽으로 on/off 보냄

            SetGachaTab, // 해당 가챠메뉴 탭으로 이동
        }

        static GachaEvent e;
        public GachaEventEnum Event;
        public bool isUIVisible;
        public eGachaType gachaType;
        public GachaTypeData gachaTypeData;
        public JArray gachaResult;
        public bool isSingleGacha;
        public int capsuleUIIndex;//현재 캡슐UI 인덱스
        public int capsuleDataIndex;//결과 데이터 인덱스
        public eGachaGroupMenu groupMenuType;

        public GachaEvent(GachaEventEnum _Event, eGachaType _type, GachaTypeData _typeData, JArray _result, bool _isUIVisible, bool _isSingleGacha, int _capsuleUIIndex, int _capsuleDataIndex, eGachaGroupMenu _menuType)
        {
            Event = _Event;
            gachaType = _type;
            gachaTypeData = _typeData;
            gachaResult = _result;
            isUIVisible = _isUIVisible;
            isSingleGacha = _isSingleGacha;
            capsuleUIIndex = _capsuleUIIndex;
            capsuleDataIndex = _capsuleDataIndex;
            groupMenuType = _menuType;
        }
        public static void InitProduction(eGachaType _type)
        {
            e.Event = GachaEventEnum.InitProduction;
            e.gachaType = _type;
            EventManager.TriggerEvent(e);
        }
        public static void IdleProduction()
        {
            e.Event = GachaEventEnum.IdleProduction;
            EventManager.TriggerEvent(e);
        }
        public static void SendGachaResult(eGachaType _type, JArray _result)
        {
            e.Event = GachaEventEnum.SendGachaResult;
            e.gachaType = _type;
            e.gachaResult = _result;
            EventManager.TriggerEvent(e);
        }
        public static void SendGachaTypeDataResult(GachaTypeData _typeData, JArray _result)
        {
            e.Event = GachaEventEnum.SendGachaTypeDataResult;
            e.gachaTypeData = _typeData;
            e.gachaResult = _result;
            EventManager.TriggerEvent(e);
        }
        public static void CheckCapsuleOpenCount(bool _isSingleGacha)
        {
            e.Event = GachaEventEnum.CheckCapsuleOpenCount;
            e.isSingleGacha = _isSingleGacha;
            EventManager.TriggerEvent(e);
        }
        public static void CapsuleOpenAnimationComplete()
        {
            e.Event = GachaEventEnum.CapsuleOpenAnimationComplete;
            EventManager.TriggerEvent(e);
        }
        public static void CapsuleIndexOpen(int _capsuleUIIndex, int _capsuleDataIndex)
        {
            e.Event = GachaEventEnum.CapsuleIndexOpen;
            e.capsuleUIIndex = _capsuleUIIndex;
            e.capsuleDataIndex = _capsuleDataIndex;
            EventManager.TriggerEvent(e);
        }
        public static void CapsuleAnimationShowEffect(int _capsuleUIIndex)
        {
            e.Event = GachaEventEnum.CapsuleAnimationShowEffect;
            e.capsuleUIIndex = _capsuleUIIndex;
            EventManager.TriggerEvent(e);
        }
        public static void SetUIVisible(bool _isVisible)
        {
            e.Event = GachaEventEnum.SetUIVisible;
            e.isUIVisible = _isVisible;
            EventManager.TriggerEvent(e);
        }
        public static void EndProduction()
        {
            e.Event = GachaEventEnum.EndProduction;
            EventManager.TriggerEvent(e);
        }

        public static void SetGachaTab(eGachaGroupMenu menuType)
        {
            if (menuType == eGachaGroupMenu.NONE)
                return;

            e.Event = GachaEventEnum.SetGachaTab;
            e.groupMenuType = menuType;
            EventManager.TriggerEvent(e);
        }
    }

    public class GachaProductionController : MonoBehaviour, EventListener<GachaEvent>
    {
        [Header("gacha prefab")]
        [SerializeField] GameObject spineClone;

        [SerializeField] GameObject capsultProductionPrefab = null;//캡슐 연출용 프리펩
        [SerializeField] GameObject capsuleProductionParent = null;//캡슐 연출 프리펩 부모 위치

        [SerializeField] GachaSpine spineController;

        [SerializeField] GameObject skipButton;
        [SerializeField] GameObject dimmedButton;

        [SerializeField] GameObject resultParticle = null;
        [SerializeField] private GameObject[] prev_effects;
        
        private bool skip;
        private CapsuleResultProduction capsuleProduction = null;
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public void OnEvent(GachaEvent eventType)
        {
            switch (eventType.Event)
            {
                case GachaEvent.GachaEventEnum.InitProduction:
                    InitSpineController(eventType.gachaType);
                    InitUI();
                    break;
                case GachaEvent.GachaEventEnum.IdleProduction:
                    InitUI();
                    break;
                case GachaEvent.GachaEventEnum.SendGachaResult:
                    DrawGachaResultProcess(eventType.gachaResult);
                    break;
                case GachaEvent.GachaEventEnum.SendGachaTypeDataResult:
                    DrawGachaResultProcess(eventType.gachaResult);
                    break;
            }
        }

        void InitSpineController(eGachaType _type)
        {
            if (spineController != null)
                spineController.SetType(_type);
        }
        public void InitUI()
        {
            dimmedButton.SetActive(false);
            OnSkipHide();
            SetCapsulePrefab();
            if (capsuleProduction != null)
                capsuleProduction.InitProduction();

            IdleProduction();
        }

        void IdleProduction()
        {
            if (spineController != null)
                spineController.SetState(eGachaSpineState.IDLE);

            if (resultParticle != null)
                resultParticle.SetActive(false);
        }

        private void DrawUISetting()
        {
            UIManager.Instance.InitUI(eUIType.None);
        }
        private void DrawEndUISetting()
        {
            UIManager.Instance.InitUI(eUIType.Gacha);
        }
        void DrawGachaResultProcess(JArray _result)
        {
            eGachaType type = eGachaType.NONE;
            List<GachaResult> resultList = new List<GachaResult>();

            List<Asset> itemRewardList = new List<Asset>();//사실 럭키박스는 여기 오면 안되긴하는데 일단 터지는 것 임시처리

            for (int i = 0; i < _result.Count; i++)//카드로 따로 분리 안한다고함.
            {
                var _type = (eGoodType)_result[i][0].Value<int>();//앞 단의 타입 정의
                int resultID = _result[i][1].Value<int>();
                
                switch (_type)
                {
                    case eGoodType.CHARACTER:
                    case eGoodType.CARD:
                        GachaResult result = new GachaResultDragonSpine(resultID, spineClone, _result[i][2].Value<int>() == 1);
                        resultList.Add(result);
                        type = eGachaType.DRAGON;
                        break;
                    case eGoodType.PET:
                        GachaResultPetSpine pet = new GachaResultPetSpine(resultID, spineClone);
                        resultList.Add(pet);
                        type = eGachaType.PET;
                        break;
                    default:
                        itemRewardList.Add(new Asset(_type, resultID, _result[i][2].Value<int>()));
                        break;
                }
            }

            if(type != eGachaType.NONE && resultList.Count > 0)
                StartCoroutine(GachaDraw(type, resultList));
            else
            {
                if(itemRewardList.Count > 0)
                    SystemRewardPopup.OpenPopup(itemRewardList.ToList());
            }
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
            dimmedButton.SetActive(false);
        }

        public void OnClickDimmed()//캡슐 전부 오픈 시
        {
            if (capsuleProduction != null)
                capsuleProduction.InitProduction();

            InitUI();
            DrawEndUISetting();
            GachaEvent.EndProduction();

            if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.DragonGacha))
            {
                TutorialManager.tutorialManagement.NextTutorialStart();
            }
        }

        private IEnumerator GachaDraw(eGachaType _type, List<GachaResult> results)
        {
            foreach (var effect in prev_effects)
            {
                effect.gameObject.SetActive(false);
            }

            if (resultParticle != null)
            {
                resultParticle.SetActive(false);
            }

            GachaEvent.SetUIVisible(false);
            DrawUISetting();
            SoundManager.Instance.PushBGM("BGM_GACHA_ACTION", false, false);
            //최고 등급 드래곤을 기준으로 1회만 보여줌

            int max = 0;
            int first = ((GachaResultDragonAndPet)results[0]).GRADE;

            for (int i = 0; i < results.Count; i++)
            {
                max = max < ((GachaResultDragonAndPet)results[i]).GRADE ? ((GachaResultDragonAndPet)results[i]).GRADE : max;
            }

            skip = false;
            Invoke("OnSkipShow", 0.5f);

            spineController.SetState(eGachaSpineState.PLAY);

            while (!spineController.GetTrackEntry().IsComplete && !skip)
            {
                yield return new WaitForEndOfFrame();
            }
            SoundManager.Instance.StopBGM();//BGM_GACHA_ACTION sound stop
            OnSkipHide();

            spineController.SetState(eGachaSpineState.HIT_IDLE);

            if (capsuleProduction)//캡슐 연출 넘기기
            {
                results.Sort((x, y) =>
                {
                    int ret = ((GachaResultDragonAndPet)y).GRADE.CompareTo(((GachaResultDragonAndPet)x).GRADE);
                    if(ret == 0)
                    {
                        ret = ((GachaResultDragonAndPet)y).IsNew.CompareTo(((GachaResultDragonAndPet)x).IsNew);
                    }
                    return ret;
                });
                //결과 데이터 오름차순 정렬
                capsuleProduction.SetCapsulePositioningSpine(results, _type, ()=> {
                    Invoke("SetActiveDimmedButton", 0.5f);//가챠 결과 끄는 화면 버튼 추가
                });//캡슐 다발 연출
            }
        }

        void SetActiveDimmedButton()
        {
            if (dimmedButton != null)
                dimmedButton.SetActive(true);
        }

        void OnSkipShow()
        {
            CancelInvoke("OnSkipShow");
            skipButton.SetActive(true);
        }

        void OnSkipHide()
        {
            CancelInvoke("OnSkipShow");
            skipButton.SetActive(false);
        }
        public void OnClickSkip()
        {
            skip = true;
        }
    }
}

