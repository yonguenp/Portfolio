
import { _decorator, Component, Node } from 'cc';
import { EventListener } from 'sb';
import { EventManager } from '../Tools/EventManager';
import { TUTORIAL_EVENT_TYPE } from './TutorialBaseData';
import { TutorialLayerController } from './TutorialLayerController';
import { TutorialManager, TutorialEvent } from './TutorialManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TutorialController
 * DateTime = Wed Mar 23 2022 11:34:23 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = TutorialController.ts
 * FileBasenameNoExtension = TutorialController
 * URL = db://assets/Scripts/Tutorial/TutorialController.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('TutorialController')
export class TutorialController extends Component /*implements EventListener<TutorialEvent>*/ {
    
    // @property(Node)
    // dimmedMaskNode: Node = null;

    // @property(Number)
    // tutorialKey: number = 0;        // 튜토리얼 키 값
    // @property(Number)
    // tutorialGroup: number = 0;      // 튜토리얼 구분 값

    // @property({group: {name: 'Tutorial Layer'}, type: Node})
    // tutorialLayerNodeList: Array<Node> = new Array<Node>();     // 각 튜토리얼의 레이어 리스트

    // currentSeq: number = 0;         // 현재 진행중인 튜토리얼 시퀀스

    // // 테스트용 임시 데이터
    // testTutoDatatList: Array<TempTutoData_Base> = null;

    // onLoad() {
    //     console.log("111")

    //     this.InitTutorialData();
    // }

    // onEnable() {
    //     // 튜토리얼 이벤트 리스너 등록
    //     EventManager.AddEvent(this);
    //     console.log("111- 1")
    // }

    // onDisable() {
    //     // 튜토리얼 이벤트 리스너 해제
    //     EventManager.RemoveEvent(this);
    // }

    // start () {
    //     // this.InitTutorialData();
    // }

    // // 테이블에서 해당 튜토리얼의 정보 조회 및 데이터 초기화
    // InitTutorialData () {

    //     this.currentSeq = 0;

    //     // 테스트용 임시 데이터
    //     // 원래대로라면 table data에서 list 형식으로 해당 key값의 데이터를 로드하고 초기화하는 과정이 진행될 것.(tutorialKey) 값으로 조회

    //     this.testTutoDatatList = [];
    //     this.testTutoDatatList = new Array<TempTutoData_Base>();

    //     let testTutoData1 = new TempTutoData_Base();
    //     testTutoData1.key = 10000;
    //     testTutoData1.group = 1;
    //     testTutoData1.seq = 0;
    //     testTutoData1.eventType = TUTORIAL_EVENT_TYPE.BUTTON;
    //     testTutoData1.dalay = 0;
    //     testTutoData1.desc = "test tuto sequence 1";

    //     let testTutoData2 = new TempTutoData_Base();
    //     testTutoData2.key = 10000;
    //     testTutoData2.group = 1;
    //     testTutoData2.seq = 1;
    //     testTutoData2.eventType = TUTORIAL_EVENT_TYPE.BUTTON;
    //     testTutoData2.dalay = 3;
    //     testTutoData2.desc = "test tuto sequence 2";

    //     let testTutoData3 = new TempTutoData_Base();
    //     testTutoData3.key = 10000;
    //     testTutoData3.group = 1;
    //     testTutoData3.seq = 2;
    //     testTutoData3.eventType = TUTORIAL_EVENT_TYPE.BUTTON;
    //     testTutoData3.dalay = 2;
    //     testTutoData3.desc = "test tuto sequence 3";

    //     this.testTutoDatatList.push(testTutoData1, testTutoData2, testTutoData3);

    //     // 튜토리얼 레이어 데이터 초기화
    //     // if (this.testTutoDatatList.length == this.tutorialLayerNodeList.length){
    //     //     for (let i = 0; i < this.testTutoDatatList.length; ++i) {
    //     //         this.tutorialLayerNodeList[i].getComponent(TutorialLayerController).InitData(this.testTutoDatatList[i]);
    //     //     }
    //     // }
    // }

    // // 튜토리얼 실행
    // StartTutorialSequence() {
    //     if (this.testTutoDatatList == null || this.testTutoDatatList.length <= 0) {return;}
    //     if (this.tutorialLayerNodeList == null || this.tutorialLayerNodeList.length <= 0) {return;}

    //     console.log("tuto event!!!");

    //     // 튜토리얼 시퀀스 체크
    //     if (this.currentSeq >= this.testTutoDatatList.length) {
    //         // 시퀀스 도달하면 튜토리얼 종료
    //         TutorialManager.GetInstance.OffTutorialEvent();

    //         this.currentSeq = 0;

    //         console.log("tutorial finish!!!");

    //         this.node.active = false;
    //     }
    //     else {
    //         this.tutorialLayerNodeList[this.currentSeq].getComponent(TutorialLayerController).PreStartTutorial();
            
    //         this.currentSeq++;
    //     }
    // }

    // GetID(): string {
    //     return 'TutorialEvent';
    // }

    // OnEvent(eventType: TutorialEvent): void {
    //     if(eventType.Data['key'] == undefined || eventType.Data['group'] == undefined) {
    //         return;
    //     }

    //     let tutoKey = eventType.Data['key'];
    //     let tutoGroup = eventType.Data['group'];

    //     // 요청한 이벤트가 맞는지 검사
    //     if (this.tutorialKey != tutoKey || this.tutorialGroup != tutoGroup) {return;}

    //     this.ResetTutorialLayer();

    //     // 첫 튜토리얼일 경우 이벤트 리스너 등록 여부 체크
    //     // if (this.currentSeq <= 0) {
    //     //     if (EventManager.IsListnerAdded(this) == false) {
    //     //         // 등록이 안되어있을경우 등록
    //     //         EventManager.AddEvent(this);
    //     //     }
    //     // }

    //     // 튜토리얼 이벤트 실행
    //     this.StartTutorialSequence();
    // }

    // ResetTutorialLayer() {
    //     for(var layer of this.tutorialLayerNodeList) {
    //         layer.active = false;
    //     }

    //     this.dimmedMaskNode.active = false;
    // }

    // // update (deltaTime: number) {
    // //     // [4]
    // // }
}