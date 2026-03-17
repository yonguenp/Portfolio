
import { _decorator, Component, Node, math, Prefab, EventHandler, instantiate, Vec3, UITransform, Label, Button, Enum, ScrollView, ResolutionPolicy, view } from 'cc';
import { EventListener } from 'sb';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { TutorialData } from '../Data/TutorialData';
import { TutorialTable } from '../Data/TutorialTable';
import { SceneManager } from '../SceneManager';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { DataManager } from '../Tools/DataManager';
import { EventManager } from '../Tools/EventManager';
import { UICanvas } from '../UI/UICanvas';
import { TUTORIAL_EVENT_TYPE } from './TutorialBaseData';
import { TutorialDimmedLayer, TUTO_ARROW_DIR } from './TutorialDimmedLayer';
import { TutorialGuideBox } from './TutorialGuideBox';
import { TutorialEvent, TutorialManager } from './TutorialManager';
const { ccclass, property, executionOrder } = _decorator;

/**
 * Predefined variables
 * Name = TutorialSeqController
 * DateTime = Tue Apr 19 2022 21:57:53 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = TutorialSeqController.ts
 * FileBasenameNoExtension = TutorialSeqController
 * URL = db://assets/Scripts/Tutorial/TutorialSeqController.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

// 버튼 레이어 외 추가 딤드용
@ccclass('BlockDimmedInfo')
export class BlockDimmedInfo {
    @property(Node)
    parentNode: Node = null;                            // 딤드를 생성 시킬 부모노드
    @property(Vec3)
    dimmedMaskPos: Vec3 = new Vec3();                   // 딤드 마스크 위치
    @property(math.Size)
    dimmedMaskSize: math.Size = new math.Size();        // 딤드 가운데 버튼 허용 마스크 사이즈
    @property(math.Size)
    dimmedLayerSize: math.Size = new math.Size();       // 딤드 레이어 사이즈
    @property(Boolean)
    useDimmedHighlight: boolean = false;                // 딤드 강조 레이어 사용 여부
    @property(Boolean)
    isDefaultDimmedLayer: boolean = false;              // 기본 알파 딤드 여부
    @property(Boolean)
    UseArrowImage: boolean = false;                   // 화살표 이미지 사용 여부
    @property({type:Enum(TUTO_ARROW_DIR)})
    arrowDirection: TUTO_ARROW_DIR = TUTO_ARROW_DIR.UP; // 화살표 방향
    @property(Node)
    arrowParentNode: Node = null;                       // 화살표 생성 부모 노드
    @property(Vec3)
    arrowImagePos: Vec3 = new Vec3();                   // 화살표 생성 위치
    @property(math.Size)
    arrowImageSize: math.Size = new math.Size();        // 화살표 크기
}

@ccclass('TutorialSeqController')
@executionOrder(-1)
export class TutorialSeqController extends Component implements EventListener<TutorialEvent> {
    
    // 튜토리얼 기본 데이터
    @property(Number)
    tutorialGroup: number = 0;      // 튜토리얼 구분 값
    @property(Number)
    tutorialSeq: number = 0;         // 튜토리얼 시퀀스 값

    @property(Boolean)
    autoNextSeq: boolean = false;   // 튜토 시퀀스를 자동으로 넘길 지 여부

    // 동작 관련 제어
    @property({group: {name:'Control Layer'}, type:Button})
    tutorialButton: Button = null;                                   // 튜토 버튼 - 값이 없으면 touch 형식이거나 버튼을 동적으로 생성하는 타입임
    @property({group: {name:'Control Layer'}, type:ScrollView})
    lockScrollView: ScrollView = null;                               // 스크롤뷰 Lock - null이 아닐경우 해당 튜토리얼 진행 시 스크롤 lock
    @property({group: {name:'Control Layer'}, type:Number})
    waitingTime: number = 0;                                        // waitingTime 만큼 대기후 자동진행(0이면 무시)

    // 딤드 관련 레이어 제어
    @property({group: {name:'Dimmed Layer'}, type:Prefab})
    dimmedMaskPrefab: Prefab = null;
    @property({group: {name:'Dimmed Layer'}, type:BlockDimmedInfo})
    blockDimmedList: Array<BlockDimmedInfo> = new Array<BlockDimmedInfo>();

    // 튜토 가이드 텍스트 레이어 제어
    @property({group: {name:'Guide Layer'}, type:Prefab})
    tutorialGuidePrefab: Prefab = null;
    @property({group: {name:'Guide Layer'}, type:Node})
    tutorialGuideParent: Node = null;
    @property({group: {name:'Guide Layer'}, type:Vec3})
    tutorialGuidePos: Vec3 = new Vec3();
    @property({group: {name:'Guide Layer'}, type:math.Size})
    tutorialGuideSize: math.Size = new math.Size();

    // 현재 튜토리얼 데이터
    tutorialData: TutorialData = null;
    tutoEventHandler: EventHandler = null;
    findTutorialKey: string = '';

    // blockDimmedList 데이터를 기반으로 생성한 DimmedLayer 리스트
    instantiatedBlockDimmedList: Array<Node> = new Array<Node>();
    // 생성한 튜토리얼 가이드 박스
    instantiateGuideBox: Node = null;

    onLoad() {
        this.InitData();
    }

    onEnable() {
        // 튜토리얼 이벤트 리스너 등록
        EventManager.AddEvent(this);
    }

    onDisable() {
        // 튜토리얼 이벤트 리스너 해제
        EventManager.RemoveEvent(this);
    }

    InitData() {
        let curLoadData = null;
        curLoadData = TableManager.GetTable<TutorialTable>(TutorialTable.Name).GetByGroupSeq(this.tutorialGroup, this.tutorialSeq);
        if (curLoadData == null) {
            console.log("Tutorial Data is null");
            return;
        }

        this.tutorialData = curLoadData;
    }

    GetID(): string {
        return 'TutorialEvent';
    }

    OnEvent(eventType: TutorialEvent): void {
        if (eventType.Data['group'] == undefined || eventType.Data['seq'] == undefined) {
            return;
        }

        // 0 / 0 인자로 올 경우는 튜토리얼 종료 이벤트로 간주
        if (eventType.Data['group'] == 0 || eventType.Data['seq'] == 0) {
            this.ClearTutorial();
            return;
        }

        this.ClearTutorial();

        let tutoGroup = eventType.Data['group'];
        let tutoSeq = eventType.Data['seq'];

        console.log(`outter tuto event IN --> ${tutoGroup}_${tutoSeq}`);

        // 요청한 이벤트가 맞는지 검사
        if (this.tutorialGroup != tutoGroup || this.tutorialSeq != tutoSeq) {return;}

        console.log(`inner tuto event IN --> ${tutoGroup}_${tutoSeq}`);

        // 튜토리얼 seq 시작
        this.PreStartTutorialSeq();
    }

    // 딜레이를 위한 preStart
    PreStartTutorialSeq() {

        // 튜토리얼 수신 응답
        TutorialManager.GetInstance.ResponseTutorialEvent(this.tutorialGroup, this.tutorialSeq);

        this.StartTutorialSeq();

        // 스크롤은 타이밍 문제로 스케쥴 딜레이로 다시조절
        this.scheduleOnce(() => { this.SetScrollViewLockState(false)}, this.tutorialData.DELAY);

        // 효과음 추가
        SoundMixer.PlaySoundFX(SOUND_TYPE.FX_TUTORIAL_START);
    }

    // 본 튜토리얼 기능 작동
    StartTutorialSeq() {
        if (this.tutorialData == null) {return;}

        this.SetScrollViewLockState(false);

        this.SetTutorialButtonEvent();

        this.SetTutorialTimerEvent();

        this.CreateBlockDimmedLayer();

        this.CreateTutorialGuideBox();
    }

    // 튜토리얼 종료
    EndTutorialSeq(event:Event, customEventData: string) {
        console.log(customEventData);
        console.log(this.tutorialSeq);

        this.ClearTutorial();

        if (this.autoNextSeq) {
            TutorialManager.GetInstance.CheckNextTutorialSeq();
        }
    }

    // 스크롤 뷰 lock 설정
    SetScrollViewLockState(isOn: boolean) {
        if (this.lockScrollView == null) {return;}

        this.lockScrollView.enabled = isOn;
    }

    // 튜토리얼(버튼타입) 버튼에 다음 시퀀스 처리 관련 이벤트 삽입
    SetTutorialButtonEvent() {
        if (this.tutorialData?.EVENT_TYPE != TUTORIAL_EVENT_TYPE.BUTTON) {return;}

        // 버튼 데이터가 있을 경우 그대로 추가
        if (this.tutorialButton != null) {
            this.AddTutorialClickEvent(this.tutorialButton);
        }
        // 버튼 데이터가 없을 경우 버튼이 동적 생성이므로 DataManager에서 조회
        else {
            let findkey = TutorialManager.GetInstance.GetTutorialDataPropertyKey(this.tutorialGroup, this.tutorialSeq);
            let tutorialButtonData = DataManager.GetData(findkey) as any;

            if (tutorialButtonData != null) {
                this.tutorialButton = tutorialButtonData;

                this.AddTutorialClickEvent(this.tutorialButton);
            }
        }
    }

    // 타이머형 튜토리얼 세팅
    SetTutorialTimerEvent() {
        if (this.waitingTime <= 0) {return;}

        this.scheduleOnce(this.EndTutorialSeq, this.waitingTime);
    }

    // 블럭 딤드 생성
    CreateBlockDimmedLayer() {
        if (this.dimmedMaskPrefab == null) {return;}
        if (this.blockDimmedList == null || this.blockDimmedList.length <= 0) {return;}

        for(let dimmedObject of this.blockDimmedList) {
            let dimmedlayer = instantiate(this.dimmedMaskPrefab);
            dimmedlayer.setParent(dimmedObject.parentNode);
            dimmedlayer.setPosition(dimmedObject.dimmedMaskPos);
            dimmedlayer.getComponent(UITransform).contentSize = dimmedObject.dimmedMaskSize;
            dimmedlayer.getChildByName('DimmedBG').getComponent(UITransform).contentSize = dimmedObject.dimmedLayerSize;
            dimmedlayer.getComponent(TutorialDimmedLayer).InitDimmedLayer(dimmedObject);

            // 터치 타입 이벤트의 경우에만 딤드에 다음시퀀스 진행 버튼 기능을 추가
            if (this.tutorialData?.EVENT_TYPE == TUTORIAL_EVENT_TYPE.TOUCH) {
                this.AddTutorialClickEvent(dimmedlayer.getChildByName('DimmedBG').getComponent(Button));
            }

            this.instantiatedBlockDimmedList.push(dimmedlayer);
        }
    }

    // 생성한 딤드 레이어 제거
    DestroyDimmedLayer() {
        if (this.instantiatedBlockDimmedList == null || this.instantiatedBlockDimmedList.length <= 0) {return;}

        for (let dimmedObject of this.instantiatedBlockDimmedList) {
            dimmedObject.destroy();
        }

        this.instantiatedBlockDimmedList = null;
    }

    // 가이드 텍스트박스 생성
    CreateTutorialGuideBox() {
        if (this.tutorialGuidePrefab == null) {return;}
        if (this.tutorialData._DESC == 0) {return;}     // 문구 없는 튜토리얼

        this.instantiateGuideBox = instantiate(this.tutorialGuidePrefab);
        this.instantiateGuideBox.setParent(this.tutorialGuideParent);
        this.instantiateGuideBox.setPosition(this.tutorialGuidePos);

        // 사이즈는 0으로 설정되어있을 경우 디폴트사이즈, 값이 있을 경우 커스텀 사이즈 적용
        if (this.tutorialGuideSize.width > 0 && this.tutorialGuideSize.height > 0) {
            this.instantiateGuideBox.getComponent(UITransform).contentSize = this.tutorialGuideSize;
        }

        this.instantiateGuideBox.getComponentInChildren(Label).string = StringTable.GetString(this.tutorialData._DESC);

        this.instantiateGuideBox.getComponent(TutorialGuideBox).InitGuideBox();
        
        // if (this.tutorialData.EVENT_TYPE == 1) {
        //     this.instantiateGuideBox.getComponent(TutorialGuideBox).InitGuideBox();
        // }
        // else {
        //     // 텍스트 박스 사이즈 최적화
        //     if (PopupManager.GetPopupStackCount() == 0) {
        //         this.OrthoRefresh(this.instantiateGuideBox);
        //     }
        // }
    }

    // 화면 비율 대비 사이즈 최적화
    OrthoRefresh(orthoNodeObject: Node) {           
        //임시처리 - game Scene에서 카메라 축소/확대 상태가 다른 씬UI 에도 전이되는 버그
        let checkSceneName = SceneManager.Instance.GetSceneTargetName();
        if(checkSceneName != "game"){return;}
        
        const orthoY = UICanvas.globalMainCamRef.orthoHeight * 2; 
        const orthoScope = orthoY / 9;
        const orthoX = orthoScope * 16;

        if(orthoNodeObject != null) {
            orthoNodeObject.setScale(orthoX / 1280, orthoY / 720); 
        }
    }

    // 가이드 텍스트박스 제거
    DestroyTutorialGuideBox() {
        if (this.instantiateGuideBox != null) {
            this.instantiateGuideBox.destroy();

            this.instantiateGuideBox = null;
        }
    }

    // 버튼에 튜토리얼 감지 이벤트 삽입
    AddTutorialClickEvent(buttonNode: Button) {
        if (buttonNode == null) {return;}

        this.tutoEventHandler = new EventHandler();
        this.tutoEventHandler.target = this.node;
        this.tutoEventHandler.component = 'TutorialSeqController';
        this.tutoEventHandler.handler = 'EndTutorialSeq';
        this.tutoEventHandler.customEventData = 'Tutorial End';

        buttonNode.clickEvents.push(this.tutoEventHandler);

        // this.buttonDimmedObject.buttonComponent.node.on(Button.EventType.CLICK, this.EndTutorialSeq, this);
    }

    // 버튼에 삽입한 이벤트 제거
    RemoveTutorialClickEvent() {
        if (this.tutorialButton == null) {return;}

        let eventIndex = this.tutorialButton.clickEvents.indexOf(this.tutoEventHandler);
        if (eventIndex > -1) {
            this.tutorialButton.clickEvents.splice(eventIndex, 1);
        }
    }

    ClearTutorial() {
        this.DestroyDimmedLayer();
        this.DestroyTutorialGuideBox();
        this.RemoveTutorialClickEvent();
        this.SetScrollViewLockState(true);

        // DataManager의 데이터 정리
        let findkey = TutorialManager.GetInstance.GetTutorialDataPropertyKey(this.tutorialGroup, this.tutorialSeq);
        DataManager.DelData(findkey);
    }
}