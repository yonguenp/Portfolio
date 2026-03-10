
import { _decorator, Component, Node, Label, Sprite} from 'cc';
import { EventListener } from 'sb';
import { EventManager } from '../Tools/EventManager';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { CabsuleGrade, CapsuleInfo } from './CapsuleInfo';
import { GachaEvent } from './GachaEvent';
import { GachaResult } from './GachaResult';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = GachaResultPopup
 * DateTime = Tue Mar 15 2022 18:10:32 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaResultPopup.ts
 * FileBasenameNoExtension = GachaResultPopup
 * URL = db://assets/Scripts/Gacha/GachaResultPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('GachaResultPopup')
export class GachaResultPopup extends Component implements EventListener<GachaEvent>
{
    @property(Node)
    canvas: Node = null;

    @property(Sprite)
    iconSprite: Sprite = null;

    @property(Label)
    gradeLabel: Label = null;

    @property(Label)
    nameLabel: Label = null;

    resultCapsuleInfo: CapsuleInfo = null;

    onLoad()
    {
        EventManager.AddEvent(this);
    }

    Init(resultInfo: CapsuleInfo) 
    {
        if (resultInfo == null) 
            return;

        this.resultCapsuleInfo = resultInfo;
        this.SetResultData();
        this.node.active = true;
    }

    GetID(): string 
    {
        return "GachaEvent";
    }

    OnEvent(eventType: GachaEvent): void 
    {
        if(eventType.Data['stats'] == null)
            return;

        switch(eventType.Data['stats'])
        {
            
            case "Dragon_Hit" :
                this.SetResultData() ;
                break;

            default :
                this.canvas.active = false;
        }
    }

    SetResultData() 
    {
        switch (this.resultCapsuleInfo.capsuleGrade) 
        {
            case CabsuleGrade.NORMAL:
                this.gradeLabel.string = "NORMAL";
                break;

            case CabsuleGrade.RARE:
                this.gradeLabel.string = "RARE";
                break;

            case CabsuleGrade.SUPER_RARE:
                this.gradeLabel.string = "SUPER_RARE";
                break;
        }

        this.nameLabel.string =  this.resultCapsuleInfo.objectName;
        this.SetResultDragonThumbnail();
    }

    onClickExitButton() 
    {
        this.node.active = false;
        GachaEvent.TriggerEvent("Dragon_Main");

        // 튜토리얼 진행
        TutorialManager.GetInstance.OnTutorialEvent(101, 6);
    }

    SetResultDragonThumbnail()
    {
        let dragonTag = this.resultCapsuleInfo.objectID;
        let icon = this.resultCapsuleInfo.objectIconFrame;

        if(dragonTag > 0 && icon != null)
        {
            this.iconSprite.spriteFrame = icon;
        }
    }
}