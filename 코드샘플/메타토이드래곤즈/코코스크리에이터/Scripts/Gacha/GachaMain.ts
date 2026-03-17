
import { _decorator, Component, Node, Color, Label, ToggleContainer, Sprite, SpriteFrame, Toggle, Canvas } from 'cc';
import { GachaEffect } from './GachaEffect';
import { GachaResult } from './GachaResult';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { SystemPopup } from '../UI/SystemPopup';
import { CapsuleInfo } from './CapsuleInfo';
import { NetworkManager } from '../NetworkManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { User, UserDragon } from '../User/User';
import { ItemBaseTable } from '../Data/ItemTable';
import { TableManager } from '../Data/TableManager';
import { ItemBaseData } from '../Data/ItemData';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { GachaShopTable } from '../Data/GachaTable';
import { GachaShopData } from '../Data/GachaData';
import { SceneManager } from '../SceneManager';
import { PopupManager } from '../UI/Common/PopupManager';
import { StringTable } from '../Data/StringTable';
import { eGachaSpineState, GachaSpineController } from './GachaSpineController';
import { EventListener } from 'sb';
import { GachaEvent } from './GachaEvent';
import { EventManager } from '../Tools/EventManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Gacha
 * DateTime = Fri Mar 11 2022 17:37:38 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = Gacha.ts
 * FileBasenameNoExtension = Gacha
 * URL = db://assets/Scripts/Gacha.ts
 *
 */

// 좌측 탭 종류
 enum TabType {
    DRAGON = 1,
    EQUIPMENT,
    SPIRIT
}

@ccclass('GachaMain')
export class GachaMain extends Component implements EventListener<GachaEvent>
{
    TICKET_1IIME_COST: number = 1;
    TICKET_10IIME_COST: number = 10;

    // 현재 선택한 탭
    curTabType: TabType = TabType.DRAGON;
    private curShopData: GachaShopData = null;

    // 캡슐 관련 (결과로 나온 캡슐의 정보를 저장)
    capsuleInfoList: Array<CapsuleInfo> = new Array<CapsuleInfo>();

    @property({ group: { name: 'Property'}, type: Color })
    normalLabelColor: Color = new Color();
    @property({ group: { name: 'Property'}, type: Color })
    shortageLabelColor: Color = new Color();

    @property({ group: { name: 'Property'}, type: Label })
    gacha1TimeCostLabel: Label = null;
    @property({ group: { name: 'Property'}, type: Sprite })
    gacha1TimeCostSprite: Sprite = null;
    @property({ group: { name: 'Property'}, type: Label })
    gacha10TimeCostLabel: Label = null;
    @property({ group: { name: 'Property'}, type: Sprite })
    gacha10TimeCostSprite: Sprite = null;
    @property({ group: { name: 'Property'}, type: Node })
    canvas : Node = null;

    GetID(): string 
    {
        return "GachaEvent";
    }

    onLoad()
    {
        EventManager.AddEvent(this);
    }

    start()
    {
        GachaEvent.TriggerEvent("Main_Dragon");
    }

    OnEvent(eventType: GachaEvent): void 
    {
        if(eventType.Data['state'] == null)
            return;

        let active = true;

        switch(eventType.Data['state'])
        {
            case "Main_Dragon" :
                this.curTabType = TabType.DRAGON;
                this.RefreshDataByTabState();
                break;

            case "Main_Part" :
                this.curTabType = TabType.EQUIPMENT;
                this.RefreshDataByTabState();
                break;

            case "Main_Elemental" :
                this.curTabType = TabType.SPIRIT;
                this.RefreshDataByTabState();
                break;

            default : 
                active = false;
                break;
        }

        this.canvas.active = active;
    }

    onClickTabButton(event, state : string)
    {
        GachaEvent.TriggerEvent(state);
    }

    // 탭 상태를 기준으로 해당 페이지 데이터 갱신
    private RefreshDataByTabState()
    {
        // 탭에 따른 가챠 필요 데이터 로드
        this.curShopData = TableManager.GetTable<GachaShopTable>(GachaShopTable.Name).GetDataBySort(this.curTabType);
        if (this.curShopData == null)
            return;

        // 티켓 보유 여부 확인
        let ticketAmount = 0;
        ticketAmount = User.Instance.GetAllItems().find(value => value.ItemNo == this.curShopData.TICKET)?.Count;

        // 유저 재화 보유량
        let userTotalCost = ticketAmount > 0 ? ticketAmount : User.Instance.GOLD;


        // 티켓이 있을 경우 처리
        if (ticketAmount > 0) 
        {
            let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(this.curShopData.TICKET);
            if (itemInfo == null) 
                return;

            this.gacha1TimeCostSprite.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON];
            this.gacha10TimeCostSprite.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON];

            this.gacha1TimeCostLabel.string = this.TICKET_1IIME_COST.toString();
            this.gacha10TimeCostLabel.string = this.TICKET_10IIME_COST.toString();

            this.gacha1TimeCostLabel.color = userTotalCost < this.TICKET_1IIME_COST ? this.shortageLabelColor : this.normalLabelColor;
            this.gacha10TimeCostLabel.color = userTotalCost < this.TICKET_10IIME_COST ? this.shortageLabelColor : this.normalLabelColor;
        }
        // 티켓이 없는 경우 처리
        else {
            let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(10000001);
            if (itemInfo == null)
                return;

            this.gacha1TimeCostSprite.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON];
            this.gacha10TimeCostSprite.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON];

            this.gacha1TimeCostLabel.string = this.curShopData.COST_NUM.toString();
            this.gacha10TimeCostLabel.string = (this.curShopData.COST_NUM * 10).toString();

            this.gacha1TimeCostLabel.color = userTotalCost < this.curShopData.COST_NUM ? this.shortageLabelColor : this.normalLabelColor;
            this.gacha10TimeCostLabel.color = userTotalCost < (this.curShopData.COST_NUM * 10) ? this.shortageLabelColor : this.normalLabelColor;
        }

        // 탭별 데이터 갱신
        switch(this.curTabType) 
        {
            case TabType.DRAGON:
                break;

            case TabType.EQUIPMENT:
                break;

            case TabType.SPIRIT:
                break;
        }
    }

    OnClickGachaButton_1Time() 
    {
        if (this.curShopData == null)
            return;

        // 티켓 보유 여부 확인
        let ticketAmount = 0;
        ticketAmount = User.Instance.GetAllItems().find(value => value.ItemNo == this.curShopData.TICKET)?.Count;

        // 유저 재화 보유량
        let userTotalCost = ticketAmount > 0 ? ticketAmount : User.Instance.GOLD;
        let gachaCost = ticketAmount > 0? this.TICKET_1IIME_COST : this.curShopData.COST_NUM;   

        // 뽑기 가능 여부 체크
        if (userTotalCost < gachaCost) 
        {
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;

            popup.setMessage(StringTable.GetString(100000612, "뽑기 실패"), StringTable.GetString(100000330, "재화가 부족합니다."));
            return;
        }

        // 뽑기 진행
        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;

        popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000613, "뽑기를 진행하시겠습니까?"));
        popup.setCallback(()=>
        {
            this.capsuleInfoList = [];

            let params = 
            {
                // 아직은 없음
            }
            // 서버로 부터 가챠 결과 리턴
            NetworkManager.Send("gacha/dragon", params, (jsonObj) => 
            {
                if (ObjectCheck(jsonObj, "result") && jsonObj.result > 0) 
                {
                    console.log("check gacah -->" + jsonObj.result);
                    let dragonID = jsonObj.result;
                    let info = User.Instance.DragonData.GetDragon(dragonID);
                    let gachaResult: CapsuleInfo = new CapsuleInfo();

                    gachaResult.objectID = info.Tag as number;
                    gachaResult.capsuleGrade = info.Grade;
                    gachaResult.objectName = info.Name;
                    gachaResult.objectIconFrame = this.GetDragonSpriteFrame(info);
        
                    this.capsuleInfoList.push(gachaResult);
        
                    // todo.. 현재는 탭기준으로 넘기지만 서버기반의 확실한 구분자가 필요함
                    // 가챠 연출 페이지 전환
                    // todo.. 추후 gacha effect에서 빛나는 연출 시 특정 등급을 확인 후 인자로 넘기는 것이 필요할 수 있음 -> init에 해당 파라미터 전달
                    //this.gachaConroller?.SwitchCanvasState(GachaCanvasType.Effect, true);
                    GachaEvent.TriggerEvent('Dragon_Play');

                    GachaEvent.SetResultData(this.capsuleInfoList);      // 가챠 결과 페이지 데이터 초기화
                    //this.RefreshDataByTabState();       // 페이지 갱신
                }
                else 
                {
                    let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;

                    popup.setMessage(StringTable.GetString(100000612, "뽑기 실패"), StringTable.GetString(100000614, "에러"));
                }
            })

            popup.ClosePopup();
        },
        () =>
        {
            popup.ClosePopup();
        });
    }

    OnClickGachaButton_10Time() 
    {
        if (this.curShopData == null) 
            return;

        let ticketAmount = 0;
        ticketAmount = User.Instance.GetAllItems().find(value => value.ItemNo == this.curShopData.TICKET)?.Count;

        let gachaCost_10Time = 10;        

        let userTotalCost = ticketAmount;

        // 뽑기 가능 여부 체크
        // todo.. 추후 유저 재화 데이터 기반 체크 필요
        if (userTotalCost < gachaCost_10Time) 
        {
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;

            popup.setMessage(StringTable.GetString(100000612, "뽑기 실패"), StringTable.GetString(100000330, "재화가 부족합니다."));
            return;
        }

        // 뽑기 진행
        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;

        popup.setMessage(StringTable.GetString(100000248, "알람"), StringTable.GetString(100000613, "뽑기를 진행하시겠습니까?"));
        popup.setCallback(()=>
        {
            // this.capsuleInfoList= [];

            // // todo..서버로 부터 가챠 결과 리턴
            // let gachaResult: CapsuleInfo = new CapsuleInfo();
            // gachaResult.objectID = 1;
            // gachaResult.capsuleGrade = CabsuleGrade.SUPER_RARE;
            // gachaResult.objectName = 'G-Dragon'

            // for (let num = 0 ; num < 10; ++num) {
            //     this.capsuleInfoList.push(gachaResult);
            // }

            // // todo.. 현재는 탭기준으로 넘기지만 서버기반의 확실한 구분자가 필요함
            // this.gachaEffectController.Init(this.capsuleInfoList);

            // this.gachaEffectController.node.active = true;
            popup.ClosePopup();
        },
        () =>
        {
            popup.ClosePopup();
        });
    }

    onExitButton()
    {
        SoundMixer.DestroyAllClip();
        SceneManager.SceneChange("game");
    }

    GetDragonSpriteFrame(dragonInfo : UserDragon) : SpriteFrame
    {
        return GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)[dragonInfo.GetDesignData().THUMBNAIL]
    }

    onDestroy()
    {
        EventManager.RemoveEvent(this);
    }
}
