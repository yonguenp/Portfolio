import { _decorator, Component, EventKeyboard, KeyCode, input, Input } from 'cc';
import { EventData } from 'sb';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { TutorialData } from '../Data/TutorialData';
import { TutorialTable } from '../Data/TutorialTable';
import { NetworkManager } from '../NetworkManager';
import { QuestManager } from '../QuestManager';
import { SceneManager } from '../SceneManager';
import { SoundMixer } from '../SoundMixer';
import { EventManager } from '../Tools/EventManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { PopupManager } from '../UI/Common/PopupManager';
import { SystemPopup } from '../UI/SystemPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TutorialManager
 * DateTime = Wed Mar 23 2022 18:06:46 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = TutorialManager.ts
 * FileBasenameNoExtension = TutorialManager
 * URL = db://assets/Scripts/Tutorial/TutorialManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

 export class TutorialEvent implements EventData {
    private static instance: TutorialEvent = null;1
    private jsonData: any = null;
    public get Data(): any {
        return this.jsonData;
    }
    GetID(): string {
        return 'TutorialEvent';
    }

    public static TriggerEvent(jsonData) {
        if(TutorialEvent.instance == null) {
            TutorialEvent.instance = new TutorialEvent();
        }

        TutorialEvent.instance.jsonData = jsonData;
        EventManager.TriggerEvent(TutorialEvent.instance);
    }
    
}

export class TutorialManager extends Component {
    
    // 인스턴스
    static instance: TutorialManager = null;
    static get GetInstance() : Readonly <TutorialManager> {
        if (TutorialManager.instance == null){
            TutorialManager.instance = new TutorialManager();
        }

        return TutorialManager.instance;
    }

    // 튜토리얼 스케줄 갱신 시간
    TUTORIAL_SCHEDULE_TIME: number = 0.5;

    // 현재 진행중인 튜토리얼에 대한 데이터
    curTutorialGroup: number = 0;
    curTutorialSeq: number = 0;
    curTutorialMaxSeq: number = 0;

    isPlayTutorial: boolean = false;        // 현재 튜토리얼 진행중인지 아닌지 체크

    static achiveList : number[] = [];

    Init() {
        input.on(Input.EventType.KEY_DOWN, this.TempTutorialTestCheat, this);
    }

    // 튜토리얼 시작 요청
    OnTutorialEvent(tutoGroup: number  = 0, tutoSeq: number  = 0) {
        if (this.isPlayTutorial == false && tutoSeq != 1) {return;}                 // 튜토리얼의 첫 시작요청인지 체크 (중간부터는 시작불가)
        if (this.isPlayTutorial && tutoGroup != this.curTutorialGroup) {return;}    // 튜토리얼이 진행중인데 다른 튜토요청이 왔는지 체크
        if (tutoSeq <= this.curTutorialSeq) {return;}                               // 같은 튜토리얼의 이전 단계 요청 예외처리

        if (tutoSeq == 1) {
            if (TutorialManager.achiveList.indexOf(tutoGroup) > -1) {return;}            // 이미 완료한 튜토리얼인지 체크
            
            this.isPlayTutorial = true;
            this.curTutorialGroup = tutoGroup;
            this.curTutorialMaxSeq = TableManager.GetTable<TutorialTable>(TutorialTable.Name).GetMaxSequence(this.curTutorialGroup);

            PopupManager.AllClosePopup();
        }

        this.curTutorialSeq = tutoSeq;

        TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});
        
        // 5.20 - 튜토가 시작 되면 체크
        let params = {
            tutorial: this.curTutorialGroup
        }

        NetworkManager.Send("tutorial/advance", params, (jsonObj) => {
            if (ObjectCheck(jsonObj, "rs") && jsonObj.rs == 0) {
                
                // 추후 튜토 완료에 대한 처리 진행
                // ...
                
                // 튜토리얼 데이터 정리
                //this.ClearTutorialData();

                // 데이터 클리어 후 0 / 0 이벤트 발송 (종료)
                //TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});
            }
            else {
                // 테스트 중엔 임시 off
                // let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                // popup.setMessage(StringTable.GetString(100000615, "튜토리얼 에러"), StringTable.GetString(100000614, "에러"));
            }
        });
    }

    // 퀘스트 상태 기반 튜토리얼 시작 요청
    OnTutorialEventWithCurrentQuest(questKey: number) {
        let resultTutoData = TableManager.GetTable<TutorialTable>(TutorialTable.Name).GetTutorialByQuestKey(questKey);

        if (resultTutoData != null) {
            this.OnTutorialEvent(resultTutoData.GROUP, resultTutoData.SEQUENCE);

            // 탐험 씬 대비 방어코드 
            // let checkSceneName = SceneManager.Instance.GetSceneTargetName();
            // if (checkSceneName != "game") {
            //     SceneManager.SceneChange("game", () => {
            //         QuestManager.AutoCheck = true;
            //     });
            // }
            // else {
            //     this.OnTutorialEvent(resultTutoData.GROUP, resultTutoData.SEQUENCE);
            // }
        }
    }

    // 이벤트 리스너의 튜토리얼 수신 응답
    ResponseTutorialEvent(tutoGroup: number, tutoSeq: number) {
        // 첫 시퀀스만 체크
        if (tutoGroup == this.curTutorialGroup && tutoSeq == 1) {
            QuestManager.AutoCheck = false;
        }
    }

    // 다음 튜토리얼 체크 및 진행
    CheckNextTutorialSeq() {
        this.curTutorialSeq++;

        // 튜토리얼 끝일 경우
        if (this.isPlayTutorial) {
            if (this.curTutorialSeq > this.curTutorialMaxSeq) {
                this.OffTutorialEvent();
            }
            else {
                TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});
            }
        }
    }

    CheckTutorialStateByQuestKey(questKey: number) {
        let resultTutoData = TableManager.GetTable<TutorialTable>(TutorialTable.Name).GetTutorialByQuestKey(questKey);

        return resultTutoData != null && TutorialManager.achiveList.indexOf(resultTutoData.GROUP) < 0;
    }

    // 튜토리얼 데이터 정리
    ClearTutorialData() {
        this.isPlayTutorial = false;
        this.curTutorialGroup = 0;
        this.curTutorialSeq = 0;
        this.curTutorialMaxSeq = 0;
    }

    // 튜토리얼 종료 시 사용
    OffTutorialEvent() {

        // 5.20 - 튜토리얼 체크 기준이 끝이 아닌 처음 시작으로 변경

        // 마지막 튜토리얼 종료 특별 체크
        if (this.curTutorialGroup == 118) {
            PopupManager.OpenPopup('TutorialFinishPopup');
        }

        // 튜토리얼 데이터 정리
        this.ClearTutorialData();

        QuestManager.AutoCheck = true;

        // 데이터 클리어 후 0 / 0 이벤트 발송 (종료)
        TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});

        // 튜토리얼 끝부분에 체크할 경우 아래 코드 주석 해제
        // let params = {
        //     tutorial: this.curTutorialGroup
        // }

        // NetworkManager.Send("tutorial/advance", params, (jsonObj) => {
        //     if (ObjectCheck(jsonObj, "rs") && jsonObj.rs == 0) {
                
        //         // 추후 튜토 완료에 대한 처리 진행
        //         // ...
                
        //         // 튜토리얼 데이터 정리
        //         this.ClearTutorialData();

        //         // 데이터 클리어 후 0 / 0 이벤트 발송 (종료)
        //         TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});
        //     }
        //     else {
                
        //         let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        //         popup.setMessage(StringTable.GetString(100000615, "튜토리얼 에러"), StringTable.GetString(100000614, "에러"));
        //     }
        // });
    }

    // 특정 조건에 의하여 튜토리얼 강제 종료
    ForceOffTutorialEvent() {
        // 5.20 - 튜토리얼 체크 기준이 끝이 아닌 처음 시작으로 변경

        let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000616, "튜토리얼 완료"), StringTable.GetString(100000617, "이미 해당 튜토리얼을 완료했습니다."));
        
        // 튜토리얼 데이터 정리
        this.ClearTutorialData();

        QuestManager.AutoCheck = true;
        
        // 데이터 클리어 후 0 / 0 이벤트 발송 (종료)
        TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});

        // let params = {
        //     tutorial: this.curTutorialGroup
        // }

        // NetworkManager.Send("tutorial/advance", params, (jsonObj) => {
        //     if (ObjectCheck(jsonObj, "rs") && jsonObj.rs == 0) {
                
        //         let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        //         popup.setMessage(StringTable.GetString(100000616, "튜토리얼 완료"), StringTable.GetString(100000617, "이미 해당 튜토리얼을 완료했습니다."));
                
        //         // 튜토리얼 데이터 정리
        //         this.ClearTutorialData();

        //         // 데이터 클리어 후 0 / 0 이벤트 발송 (종료)
        //         TutorialEvent.TriggerEvent({group: this.curTutorialGroup, seq: this.curTutorialSeq});
        //     }
        //     else {
                
        //         let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        //         popup.setMessage(StringTable.GetString(100000615, "튜토리얼 에러"), StringTable.GetString(100000614, "에러"));
        //     }
        // });
    }

    IsNowPlayingTutorial(): Boolean {
        return this.isPlayTutorial;
    }

    IsNowPlayingTutorialByGroup(group: number): Boolean {
        return this.isPlayTutorial && this.curTutorialGroup == group;
    }

    GetCurTutodata(): TutorialData {
        let curLoadData = new TutorialData();
        curLoadData = TableManager.GetTable<TutorialTable>(TutorialTable.Name).GetByGroupSeq(this.curTutorialGroup, this.curTutorialSeq);

        return curLoadData;
    }

    GetCurTutoMaxSequence(): Number {
        let result = TableManager.GetTable<TutorialTable>(TutorialTable.Name).GetMaxSequence(this.curTutorialGroup);

        return result;
    }

    // DataManager에서 사용할 키 형식을 리턴
    GetTutorialDataPropertyKey(group:number, seq:number): string {
        let resultKey: string = `tutorial_${group}_${seq}`;

        return resultKey;
    }

    // todo..추후 삭제 예정(튜토리얼 이벤트 강제 발생)
    TempTutorialTestCheat(event: EventKeyboard) {
        switch (event.keyCode) {
            case KeyCode.F1:
                // 2. 브릭공장 건설 튜토
                this.OnTutorialEvent(102, 1);
                break;
            case KeyCode.F2:
                // 3. 건설 즉시 완료 튜토리얼 발생
                this.OnTutorialEvent(103, 1);
                break;
            case KeyCode.F3:
                // 4. 브릭 생산 튜토리얼 발생
                this.OnTutorialEvent(104, 1);
                break;
            case KeyCode.F4:
                // 5. 뽑기 튜토리얼 발생
                this.OnTutorialEvent(105, 1);
                break;
            case KeyCode.F5:
                // 6. 탐험 튜토리얼 (미완성)
                this.OnTutorialEvent(106, 1);
                break;
            case KeyCode.F6:
                // 7. 제작한 브릭받기 튜토리얼
                this.OnTutorialEvent(108, 1);
                break;
            case KeyCode.F7:
                // 8. 드래곤 레벨업 튜토리얼 발생
                this.OnTutorialEvent(109, 1);
                break;
            case KeyCode.F8:
                // 9. 건물 외형 튜토리얼
                this.OnTutorialEvent(111, 1);
                break;
            case KeyCode.F9:
                // 10. 코인 도저 건설 튜토리얼
                this.OnTutorialEvent(112, 1);
                break;
            case KeyCode.F10:
                // 11. 건전지 공장 업그레이드 튜토리얼
                this.OnTutorialEvent(113, 1);
                break;
            case KeyCode.F11:
                // 12. 드래곤 장비 튜토리얼
                this.OnTutorialEvent(114, 1);
                break;
            case KeyCode.F12:
                // 13. 드래곤 스킬 레벨업
                this.OnTutorialEvent(116, 1);
                break;
        }
    }
}