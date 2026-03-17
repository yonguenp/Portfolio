using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//캡슐 관리 개별 관리 객체 - 하위 ProductionCapsuleSpine가 spine 제어함.
namespace SandboxNetwork
{

    public class ProductionCapsule : MonoBehaviour
    {
        [SerializeField] int capsuleUIIndex = 1;

        [SerializeField] GameObject capsuleEffectPrafab = null;//이펙트 생성 프리펩
        [SerializeField] GameObject capsuleEffectparent = null;//이펙트 생성 타겟

        [SerializeField] ProductionCapsuleSpine capsuleSpine = null;
        [SerializeField] GameObject spineTarget = null;//드래곤, 펫 스파인 생성 위치

        public int CapsuleIndex { get { return capsuleUIIndex; } }

        ProductionCapsuleEffect capsuleEffect = null;//이펙트 데이터
        GachaSpineObject spineObjectData = null;
        
        private bool isCapsuleOpening;//스파인 클릭했는지 - normal 일 때는 자동 오픈 만들어야함. - 캡슐 더블 터치 방지용
        public bool IsCapsuleOpening { get { return isCapsuleOpening; } }//열린 상태인가
        
        private bool isProductionComplete;//캡슐 열리는 연출이 끝났는지 체크용
        public bool IsProductionComplete { get { return isProductionComplete; } }

        private bool isSetIdle = false;//연타 방지 플래그

        private Sequence autoOpenSequence = null;//자동 오픈 트윈 시퀀스

        private bool isSkipPressed = false;


        public void Init()//초기화
        {
            isCapsuleOpening = false;
            isProductionComplete = false;
            isSetIdle = false;
            isSkipPressed = false;
            spineObjectData = null;
            if (spineTarget != null)
                SBFunc.RemoveAllChildrens(spineTarget.transform);
            SetEffect();//이펙트 달기
        }

        void SetEffect()//effect prefab 생성
        {
            if (capsuleEffectparent != null)
                SBFunc.RemoveAllChildrens(capsuleEffectparent.transform);

            var clone = Instantiate(capsuleEffectPrafab, capsuleEffectparent.transform);
            capsuleEffect = clone.GetComponent<ProductionCapsuleEffect>();
            capsuleEffect.InitEffect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_isVisible"></param>
        /// <param name="_spineInitCallback"></param>//현재 오브젝트를 켰을 상황보다 ProductionCapsuleSpine의 start가 뒤에 타기 때문에, init 끝난 뒤에 anim 세팅
        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
        }

        public void SetSpineVisible(bool _isVisible, Action _spineInitCallback = null)
        {
            capsuleSpine.gameObject.SetActive(_isVisible);
            if (_spineInitCallback != null)
            {
                capsuleSpine.SuccessInitCallback = _spineInitCallback;
            }
        }

        public void SetSpineInitCallback(Action _spineInitCallback)
        {
            if (_spineInitCallback != null)
            {
                capsuleSpine.SuccessInitCallback = _spineInitCallback;
            }
        }

        public void SpineAnimationClear()
        {
            if (capsuleSpine != null)
                capsuleSpine.SkeletonAni.Clear();
        }    

        public float GetSpineAnimationLength(eProductionCapsuleSpineState _state)
        {
            if (capsuleSpine != null)
                return capsuleSpine.GetAnimaionTime(_state);
            else
                return 0f;
        }

        public void SetSpinePosition(Vector2 _pos)
        {
            if (capsuleSpine != null)
                capsuleSpine.GetComponent<RectTransform>().anchoredPosition = _pos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_state"></param>
        /// <param name="_animationDoneCallback"></param>//해당 anim 이 끝나면 반환 callback
        void SetState(eProductionCapsuleSpineState _state, Action _instancingSpineCallback = null, Action _animationDoneCallback = null)
        {
            if (capsuleSpine != null)
                capsuleSpine.SetState(_state, _instancingSpineCallback, _animationDoneCallback);
        }

        public void SetIdleAnimation(int _grade, bool _isSingle = false)
        {
            if(_isSingle)
            {
                if (isSkipPressed)
                    return;

                capsuleSpine.gameObject.SetActive(true);
                if (capsuleSpine != null)
                {
                    SetState(capsuleSpine.GetIdleAnimationByGrade(_grade));
                    if (capsuleEffect != null)
                        capsuleEffect.ShowIdleEffect(_grade);
                }
                return;
            }

            SetSpineVisible(true, () =>
            {
                if (capsuleSpine != null)
                {
                    var hitAnimLength = GetHitAnimationLength(_grade);
                    var tween = DOTween.Sequence();
                    tween.AppendCallback(() =>{
                        SetState(GetHitStateByGrade(_grade));//일반 캡슐 자동오픈
                    }).AppendInterval(hitAnimLength).AppendCallback(() => {
                        SetState(capsuleSpine.GetIdleAnimationByGrade(_grade));
                        isSetIdle = true;

                        if (capsuleEffect != null)
                            capsuleEffect.ShowIdleEffect(_grade);
                    });
                    tween.Play();
                }
            });
        }

        public void PlayFrontHitCapsuleAnimation(int _grade)//정면 캡슐 애니메이션(결과에서 Hit 넘기는 용도, 단일일때는 anim 이름 바꿔서 계속 사용)
        {
            SpineAnimationClear();
            gameObject.SetActive(true);
            var animationState = GetHitStateByGrade(_grade);
            SetState(animationState);
        }

        public eProductionCapsuleSpineState GetHitStateByGrade(int _grade)
        {
            return _grade > (int)eDragonGrade.Uncommon ? eProductionCapsuleSpineState.HIT_RARE : eProductionCapsuleSpineState.HIT_NORMAL;
        }
        float GetHitAnimationLength(int _grade)
        {
            return GetSpineAnimationLength(GetHitStateByGrade(_grade));
        }

        public void AutoOpenNormalGrade(float _openDelay, GachaResult _data, eGachaType _type, bool _isSingleGacha, bool soundSkip = false)//노말 등급일 때는 자동 오픈 연출
        {
            isCapsuleOpening = true;

            if (_isSingleGacha)
            {
                if((GachaResultSpine)_data != null)
                {
                    if (!soundSkip)
                    {
                        var rs = (GachaResultSpine)_data;
                        switch ((eDragonGrade)rs.GRADE)
                        {
                            case eDragonGrade.Legend:
                                SoundManager.Instance.PlaySFX("sfx_gacha_open_l");//hit sound 추가
                                break;
                            case eDragonGrade.Unique:
                                SoundManager.Instance.PlaySFX("sfx_gacha_open_u");//hit sound 추가
                                break;
                            case eDragonGrade.Rare:
                                SoundManager.Instance.PlaySFX("sfx_gacha_open_r");//hit sound 추가
                                break;
                        }
                    }
                }

                SetState(eProductionCapsuleSpineState.OPEN_NORMAL, ()=> {
                    
                    if (isSkipPressed)
                        return;

                    InstanceGachaSpine(_data, _type);//타겟에 스파인 생성
                },()=> {

                    if (isSkipPressed)
                        return;

                    isProductionComplete = true;
                    GachaEvent.CheckCapsuleOpenCount(_isSingleGacha);//현재 열린 상태를 쏨.
                });
            }
            else
            {
                SetSpineVisible(true, () =>
                {
                    var grade = ((GachaResultSpine)_data).GRADE;
                    if (!soundSkip)
                    {
                        switch ((eDragonGrade)grade)
                        {
                            case eDragonGrade.Legend:
                                SoundManager.Instance.PlaySFX("sfx_gacha_open_l");//hit sound 추가
                                break;
                            case eDragonGrade.Unique:
                                SoundManager.Instance.PlaySFX("sfx_gacha_open_u");//hit sound 추가
                                break;
                            case eDragonGrade.Rare:
                                SoundManager.Instance.PlaySFX("sfx_gacha_open_r");//hit sound 추가
                                break;
                        }
                    }

                    if (autoOpenSequence != null)
                        autoOpenSequence.Kill();

                    autoOpenSequence = DOTween.Sequence();
                    autoOpenSequence.AppendCallback(() =>
                    {
                        SetState(GetHitStateByGrade(grade));
                    }).AppendInterval(_openDelay).AppendCallback(() =>
                    {
                        SetState(eProductionCapsuleSpineState.OPEN_NORMAL, () =>
                        {
                            if (isSkipPressed)
                                return;

                            InstanceGachaSpine(_data, _type);//타겟에 스파인 생성

                        }, () =>
                        {
                            if (isSkipPressed)
                                return;

                            isProductionComplete = true;
                            GachaEvent.CheckCapsuleOpenCount(_isSingleGacha);//현재 열린 상태를 쏨.
                        });//일반 캡슐 자동오픈
                    });
                    autoOpenSequence.Play();
                });
            }
        }

        /// <summary>
        /// 터치를 했을 때, 오픈 연출 - isSetIdle 용도 : 터치로 들어오기 전에 앞단에서 SetIdleAnimation 초기화 해야하는데, 그전에 들어오는 경우(터치 연타 방지)가 있음
        /// </summary>
        /// <param name="_spineClone"></param>
        /// <param name="_data"></param>
        /// <param name="_type"></param>
        /// <param name="_isSingleGacha"></param>
        public void OpenGachaCapsuleProcess(GachaResult _data, eGachaType _type, bool _isSingleGacha, Action _openSuccessCallback = null)//capsueResultProduction 에서 해당 index가지고 세팅
        {
            if (_isSingleGacha)//단차 캡슐은 이전에 세팅을 하고 가서 조건 처리
                isSetIdle = true;

            if (!isSetIdle)
                return;

            if (isCapsuleOpening)
                return;

            isCapsuleOpening = true;

            var grade = ((GachaResultSpine)_data).GRADE;
            switch ((eDragonGrade)grade)
            {
                case eDragonGrade.Legend:
                    SoundManager.Instance.PlaySFX("sfx_gacha_open_l");//hit sound 추가
                    break;
                case eDragonGrade.Unique:
                    SoundManager.Instance.PlaySFX("sfx_gacha_open_u");//hit sound 추가
                    break;
                case eDragonGrade.Rare:
                    SoundManager.Instance.PlaySFX("sfx_gacha_open_r");//hit sound 추가
                    break;

            }
            SetState(capsuleSpine.GetOpenAnimationStateByGrade(grade),()=> {

                if (isSkipPressed)
                    return;

                InstanceGachaSpine(_data, _type);//타겟에 스파인 생성
            },()=> {

                if (isSkipPressed)
                    return;

                if (capsuleSpine == null)
                    return;

                SendGachaSystemMessage(_data, _type == eGachaType.PET);//획득 시스템 메세지 요청
                isProductionComplete = true;
                GachaEvent.CheckCapsuleOpenCount(_isSingleGacha);//현재 열린 상태를 쏨.
                if (_openSuccessCallback != null)
                    _openSuccessCallback();
            });//각 grade 에 따른 spine Anim 출력
        }

        void InstanceGachaSpine(GachaResult _data, eGachaType _type, bool isSkipped = false)//들어온 데이터 기반으로 스파인 만들기
        {
            SBFunc.RemoveAllChildrens(spineTarget.transform);
            GachaResultSpine rs = (GachaResultSpine)_data;
            var clone = Instantiate(((GachaResultDragonAndPet)_data).GetPrefab(), spineTarget.transform);
            GachaSpineObject spine = clone.GetComponent<GachaSpineObject>();
            if (spine != null)
            {
                spine.Init(rs.ID,
                    rs.NAME,
                    rs.GRADE,
                    _type,
                    rs.SKIN,
                    false,rs.IsNew,capsuleUIIndex);
                spine.StopSpineSequence();
            }

            spineObjectData = spine;

            if (spineObjectData != null)
                spineObjectData.SkipCoverAnimation(true, isSkipped);
        }

        public void ShowOpenEffect(int _grade)
        {
            if (capsuleEffect != null)
                capsuleEffect.ShowOpenEffect(_grade);
        }

        public void SendGachaSystemMessage(GachaResult resultData, bool isPet)//유니크 이상 일때 드래곤, 펫에 대한 시스템 메시지
        {
            GachaResultDragonAndPet gachaData = (GachaResultDragonAndPet)resultData;

            if (gachaData == null) return;

            if (GameConfigTable.IsIgnoreSystemMessageTarget(gachaData.ID))
                return;

            int upper_grade = GameConfigTable.GetSystemMessageGrade();
            if (gachaData.GRADE >= upper_grade)
            {
                eAchieveSystemMessageType messageType = eAchieveSystemMessageType.GET_DRAGON_U;

                switch ((eDragonGrade)gachaData.GRADE)
                {
                    case eDragonGrade.Unique:
                        messageType = isPet ? eAchieveSystemMessageType.GET_PET_U : eAchieveSystemMessageType.GET_DRAGON_U;
                        break;
                    case eDragonGrade.Legend:
                        messageType = isPet ? eAchieveSystemMessageType.GET_PET_L : eAchieveSystemMessageType.GET_DRAGON_L;
                        break;
                    default:
                        Debug.LogError("알수없는 grade 타입");
                        return;
                }

                ChatManager.Instance.SendAchieveSystemMessage(messageType, User.Instance.UserData.UserNick, gachaData.ID);
            }
        }

        public void SkipProcess(GachaResult _data, eGachaType _type, bool _isSingleGacha)//캡슐 스킵 버튼 누르면 바로 열리게
        {
            isSkipPressed = true;
            capsuleEffect.gameObject.SetActive(false);//이펙트 끄기
            SetSpineVisible(false);//캡슐 스파인 끄기

            if (autoOpenSequence != null)
                autoOpenSequence.Kill();

            autoOpenSequence = null;

            InstanceGachaSpine(_data, _type, true);//스파인 바로 생성

            SendGachaSystemMessage(_data, _type == eGachaType.PET);//획득 시스템 메세지 요청

            isProductionComplete = true;
            GachaEvent.CheckCapsuleOpenCount(_isSingleGacha);//현재 열린 상태를 쏨.
        }
    }
}
