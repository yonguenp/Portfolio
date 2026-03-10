
import { _decorator, Component, Node, Vec3, Sprite, Button, tween, v3, UIOpacity, easing, macro, KeyCode } from 'cc';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { CapsuleInfo } from './CapsuleInfo';
import { GachaResult } from './GachaResult';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = GachaResultCapsule
 * DateTime = Tue Mar 15 2022 12:18:50 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaResultCapsule.ts
 * FileBasenameNoExtension = GachaResultCapsule
 * URL = db://assets/Scripts/Gacha/GachaResultCapsule.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('GachaResultCapsule')
export class GachaResultCapsule extends Component 
{
    gachaResultScript: GachaResult = null;
    capsuleInfo: CapsuleInfo = null;

    // 연출 관련
    @property(Sprite)
    capsuleSprite: Sprite = null;

    @property(Node)
    capsuleEffect: Node = null;

    @property(Button)
    capsuleButton: Button = null;

    @property(UIOpacity)
    capsuleOpacity: UIOpacity = null;

    instantiatePos: Vec3 = new Vec3(0, 300, 0);    // 애니메이션 첫 연출 위치

    start () 
    {

    }

    Init(resultinfo: CapsuleInfo, resultScript: GachaResult) 
    {
        if (resultinfo == null || resultScript == null) 
            return;

        this.capsuleInfo = resultinfo;
        this.gachaResultScript = resultScript;
        this.ResetCapusuleState();
        this.StartCapsuleDropAnim();
    }

    StartCapsuleDropAnim() 
    {
        tween(this.node)
        .to(1, { position: v3(0,0,0)}, {easing: easing.bounceOut})
        .delay(0.5)
        .call(() => {
            this.capsuleEffect.active = true;
            this.capsuleButton.interactable = true;
        })
        .start();

        tween(this.node!.getComponent(UIOpacity))
        .to(0.5, { opacity: 255 }, {easing: easing.fade})
        .start();
    }

    ResetCapusuleState() 
    {
        this.node.setPosition(this.instantiatePos);
        this.capsuleOpacity.opacity = 0;
        this.capsuleButton.interactable = false;
        this.capsuleEffect.active = false;
    }

    onClickCapsule() 
    {
        if (this.gachaResultScript == null) 
            return;
    }
}
