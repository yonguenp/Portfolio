/**
 * Predefined variables
 * Name = sb
 * DateTime = Fri Feb 18 2022 13:56:17 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = .d.ts
 * FileBasenameNoExtension = sb.d
 * URL = db://assets/Scripts/sb.d.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

declare module 'sb' {
    export interface IManagerBase {
        GetManagerName(): string;
        Update(deltaTime: number): void;
    }

    export interface ITableBase {
        Init(): void;
        DataClear(): void;
        Get(index: PropertyKey): any;
        Add(data: any): void;
        SetTable(jsonData: any): void;
    }
    
    export interface IStateData {
        GetID(): string;
        OnEnter(): boolean;
        OnExit(): boolean;
        Update(dt: number): void;
    }
    
    export interface IStateMachine {
        AddState(state): boolean;
        GetState(type): IStateData;
        StateInit();
    }
 
    export interface ITimeObject {
        curTime: number; //목표에 해당되는 시간
        Init(); //해당 오브젝트를 TimeManager에 등록하는 과정.
        Destroy(); //해당 오브젝트를 TimeManager에서 제거하는 과정.
        Refresh(); //시간 갱신 시 필요한 작동방식 (UI 갱신 과정)//TimeManager에서 시간초가 바뀔때 마다 호출
    }

    export interface IWaveBase {
        Start(): void;
        End(): void;
    }
 
    export interface ICharacter {
        Update(dt: number);
    }

    export interface EventData {
        GetID(): string;//리스너에 매칭되는 데이터 ID는 일치해야함
    }
   
    export interface EventListener<T extends EventData> {
        GetID(): string;
        OnEvent(eventType: T): void;
    }

    export interface IPopup {
        Init(data?: any): void;
        ForceUpdate(data?: any): void;
    }

    export interface IPopupExtension {
        Init(): void;
        ForceUpdate(): void;
    }

    export interface IBGMChange {
        Change(): void;
    }
}