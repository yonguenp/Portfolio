
import { _decorator, Component, Node, Tween, tween, Sprite, Color, UIOpacity, instantiate, Layers, Vec3, sp, Widget, Label, SpriteFrame, Button } from 'cc';
import { EventListener } from 'sb';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { EventManager } from '../Tools/EventManager';
import { ChangeLayer } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { User } from '../User/User';
import { CapsuleInfo } from './CapsuleInfo';
import { GachaEvent } from './GachaEvent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = GachaResult
 * DateTime = Mon Mar 21 2022 12:02:54 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaResult.ts
 * FileBasenameNoExtension = GachaResult
 * URL = db://assets/Scripts/Gacha/GachaResult.ts
 *
 */
 
@ccclass('GachaResult')
export class GachaResult extends Component implements EventListener<GachaEvent>
{
    @property(Node)
    canvas: Node = null;

    @property(Node)
    dragonNode: Node = null;

    @property(UIOpacity)
    dimdUW: UIOpacity = null;

    @property(UIOpacity)
    dimdDW: UIOpacity = null;

    @property(Node)
    nodeNameLabel: Node = null;

    @property(Sprite)
    sprElemental : Sprite = null;

    @property(Label)
    labelName : Label = null;

    @property(Node)
    nodeNew: Node = null;

    @property(Button)
    btnEnd: Button = null;

    @property({group:"labelUp", type : Node})
    nodeResultLabelUp: Node = null;

    @property({group:"labelUp", type : Vec3})
    vecResultLabelUp : Vec3;

    @property({group:"labelDown", type : Node})
    nodeResultLabelDown: Node = null;

    @property({group:"labelDown", type : Vec3})
    vecResultLabelDown: Vec3;

    // 캡슐 정보 리스트
    originCapsuleInfoList: Array<CapsuleInfo> = new Array<CapsuleInfo>();   // 뽑기 결과 원본 데이터

    // 현재 개봉중인 캡슐
    currentCapsuleInfo: CapsuleInfo = null;
    currentCapsuleNode: Node = null;
    ftween : Tween<UIOpacity> = null;

    onLoad()
    {
        EventManager.AddEvent(this);
    }

    GetID(): string 
    {
        return "GachaEvent";
    }
    
    OnEvent(eventType: GachaEvent): void 
    {
        if(eventType.Data['state'] == null)
            return;

        switch(eventType.Data['state'])
        {
            case 'Dragon_Hit' :
                this.Init(eventType.Data.resultObj);
                break;

            case 'Dragon_Show' :
                this.SetDragonThumnail();
                break;

            case 'Dragon_Hit_Idle' :
                break;

            default:
                this.canvas.active = false;
                break;
        }
    }

    Init(resultList: Array<CapsuleInfo>) 
    {
        console.log(resultList)

        if (resultList == null || resultList.length <= 0) 
            return;

        this.canvas.active = true;
        this.originCapsuleInfoList = resultList;

        GachaEvent.TriggerEvent("Dragon_Show");
    }

    SetDragonThumnail()
    {
        //나오기만 함
        this.dragonNode.removeAllChildren();
        if(this.ftween)
            this.ftween.stop()

        this.nodeResultLabelUp.getComponent(UIOpacity).opacity = 0;
        this.nodeResultLabelDown.getComponent(UIOpacity).opacity = 0;
        
        this.btnEnd.interactable = false;        

        let dragonInfo = this.originCapsuleInfoList.shift();
        let element = User.Instance.DragonData.GetDragon(dragonInfo.objectID)
        if(element == null)
            return;
        
        let dragonClone = instantiate(User.Instance.DragonData.GetNameDragonSpine(element.GetDesignData().IMAGE))
        const layer = 1 << Layers.nameToLayer('UI_2D');
        ChangeLayer(dragonClone, layer);
        
        dragonClone.setScale(2,2,1);
        dragonClone.setPosition(0, -60, 0);
        dragonClone.setParent(this.dragonNode);
        dragonClone.getComponentInChildren(sp.Skeleton).color = new Color(255,255,255,0);

        this.sprElemental.spriteFrame = this.GetElementIconSpriteByIndex(element.Element)
        this.labelName.string = dragonInfo.objectName;
        this.nodeNameLabel.active = false
        this.nodeNew.active = false

        tween(dragonClone.getComponentInChildren(sp.Skeleton).color)
            //.to(0, {a : 0})
            .delay(.6)
            .to(.3, {a : 255})
            .delay(.8)
            .call(() => 
            {
                this.nodeNameLabel.active = true;
                this.nodeNew.active = true;
                this.btnEnd.interactable = true;
            }).start();

        tween(this.dimdUW)
            .to(0, {opacity : 0})
            .delay(.6)
            .to(.3, {opacity : 255}).start();

        tween(this.dimdDW)
            .to(0, {opacity : 0})
            .delay(.6)
            .to(.3, {opacity : 255}).start();

        tween(this.nodeResultLabelUp)
            .to(0, {position : this.vecResultLabelUp})
            .delay(.6)
            .call(() =>
            {
                tween(this.nodeResultLabelUp.getComponent(UIOpacity)).to(.3, {opacity : 255}).start();
            })
            .by(.3, {position : new Vec3(0, -30, 0)}).start();

        tween(this.nodeResultLabelDown)
            .to(0, {position : this.vecResultLabelDown})
            .delay(.6)
            .call(() =>
            {
                tween(this.nodeResultLabelDown.getComponent(UIOpacity)).to(.3, {opacity : 255}).start();
            })
            .by(.3, {position : new Vec3(0, 30, 0)})
            .delay(1)
            .call(() => 
            {
                this.ftween = tween(this.nodeResultLabelDown.getComponent(UIOpacity)).repeatForever(tween().to(.8, {opacity : 125}).to(.8, {opacity : 255})).start();
            }).start();
    }

    onClickBackToMain()
    {
        //다음 드래곤이 있으면 가져옴
        //없으면 끝
        if(this.originCapsuleInfoList.length > 0)
        {
            //보여줄 드래곤이 더 있음
            this.SetDragonThumnail();
            return;
        }

        GachaEvent.TriggerEvent("Main_Dragon");

        // 튜토리얼 진행
        TutorialManager.GetInstance.OnTutorialEvent(101, 6);
    }

    onDestroy()
    {
        EventManager.RemoveEvent(this);
    }

    GetElementIconSpriteByIndex(e_type : number) : SpriteFrame
    {
        let elementStr = "";
        switch(e_type)
        {
            case 1 :
                elementStr = "fire";
                break;
            case 2 :
                elementStr = "water";
                break;
            case 3 :
                elementStr = "soil";
                break;
            case 4 :
                elementStr = "wind";
                break;
            case 5 :
                elementStr = "light";
                break;
            case 6 :
                elementStr = "dark";
                break;
            default:
                break;
        }

        return GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ELEMENTICON_SPRITEFRAME)[`type_${elementStr}`]
    }
}
