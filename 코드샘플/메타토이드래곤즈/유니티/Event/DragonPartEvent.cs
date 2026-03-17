namespace SandboxNetwork
{
    public struct DragonPartEvent
    {
        public enum DragonPartEventEnum
        {
            RefreshList,//장비 정보 리스트 갱신

            RefreshLayer,//장비레이어 갱신
            RefreshInfoPanel,//장비 정보 패널 갱신
            HideInfoPanel,//상세 정보 보기 끌 때 쏘는 용도

            ShowEaterNode,//eater 노드 컴포넌트 켜기 (장비 교체 시 암막커튼)

            ShowReinforcePanel,//강화 패널 오픈
            HideReinforcePanel,
            ReinforcePartSlotSelect,//강화 패널 오픈시 클릭 - 강화 패널 갱신용
            SuccessReinforce,//강화 성공 장비 데이터 리스트 갱신
            DeleteReinforce,//강화 실패 터져버린 장비 리스트 삭제

            ShowCompoundPanel,//합성 패널 오픈
            HideCompoundPanel,//합성 패널 끄기
            CompoundAutoSet,//장비 합성 자동 삽입 기능 

            ShowDecomposePanel,//분해 패널 오픈
            HideDecomposePanel,//분해 패널 끄기
            DecomposeAllSelect,//분해 전체 선택

            SetListType,//장비패널에 따라 뿌려주는 리스트 타입을 결정

            PlayEquipPartAnim,//장비 장착 애니메이션 플레이
        }

        public int listTypeIndex;//현재 열린 타입의 상태를 들고있기(PartListViewType 넘기는 용도)
        public int partTag;//장비 태그
        public int equipIndex;
        public bool isRight;//정보 상세창 위치(true)
        public UnityEngine.GameObject obj;//dragonEquipButtonSlotComponent 이거나, dragonEquipChangeEaterComponent 이거 래핑된 오브젝트 getcomponent

        public DragonPartEventEnum Event;
        static DragonPartEvent e;
        public DragonPartEvent(DragonPartEventEnum _Event, int _partTag, bool _isRight,int _listTypeIndex, UnityEngine.GameObject _obj, int equipindex = -1)
        {
            Event = _Event;
            partTag = _partTag;
            listTypeIndex = _listTypeIndex;
            isRight = _isRight;
            obj = _obj;
            equipIndex = equipindex;
        }
        public static void RefreshList()
        {
            e.Event = DragonPartEventEnum.RefreshList;
            EventManager.TriggerEvent(e);
        }
        public static void RefreshLayer()
        {
            e.Event = DragonPartEventEnum.RefreshLayer;
            EventManager.TriggerEvent(e);
        }
        public static void RefreshInfoPanel(int _partTag, bool _isRight, UnityEngine.GameObject buttonSlotObj, int equipindex = -1)
        {
            e.Event = DragonPartEventEnum.RefreshInfoPanel;
            e.partTag = _partTag;
            e.isRight = _isRight;
            e.obj = buttonSlotObj;
            e.equipIndex = equipindex;

            EventManager.TriggerEvent(e);
        }
        public static void HideInfoPanel()
        {
            e.Event = DragonPartEventEnum.HideInfoPanel;
            EventManager.TriggerEvent(e);
        }
        
        public static void ShowEaterNode(int _partTag, UnityEngine.GameObject eaterObj)
        {
            e.Event = DragonPartEventEnum.ShowEaterNode;
            e.partTag = _partTag;
            e.obj = eaterObj;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void SetListType(int _index)
        {
            e.Event = DragonPartEventEnum.SetListType;
            e.listTypeIndex = _index;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void ShowReinforcePanel()
        {
            e.Event = DragonPartEventEnum.ShowReinforcePanel;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void HideReinforcePanel()
        {
            e.Event = DragonPartEventEnum.HideReinforcePanel;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void SuccessReinforcePart(int _partTag)
        {
            e.Event = DragonPartEventEnum.SuccessReinforce;
            e.partTag = _partTag;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void DeleteReinforcePart(int _partTag)
        {
            e.Event = DragonPartEventEnum.DeleteReinforce;
            e.partTag = _partTag;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void ReinforcePartSlotSelect(int _partTag)
        {
            e.Event = DragonPartEventEnum.ReinforcePartSlotSelect;
            e.partTag = _partTag;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void ShowCompoundPanel()
        {
            e.Event = DragonPartEventEnum.ShowCompoundPanel;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void HideCompoundPanel()
        {
            e.Event = DragonPartEventEnum.HideCompoundPanel;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void CompoundAutoSet()
        {
            e.Event = DragonPartEventEnum.CompoundAutoSet;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }

        public static void ShowDecomposePanel()
        {
            e.Event = DragonPartEventEnum.ShowDecomposePanel;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void HideDecomposePanel()
        {
            e.Event = DragonPartEventEnum.HideDecomposePanel;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void DecomposeAllSelect()
        {
            e.Event = DragonPartEventEnum.DecomposeAllSelect;
            e.equipIndex = -1;

            EventManager.TriggerEvent(e);
        }
        public static void PlayEquipPartAnim(int _partTag)
        {
            e.Event = DragonPartEventEnum.PlayEquipPartAnim;
            e.equipIndex = -1;
            e.partTag = _partTag;
            EventManager.TriggerEvent(e);
        }
    }
}