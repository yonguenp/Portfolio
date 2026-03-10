
import { _decorator, Component, Node, math, UITransform, Sprite, Color, UI, tween, Vec3, Tween, easing, Prefab, instantiate } from 'cc';
import { BlockDimmedInfo } from './TutorialSeqController';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TutorialDimmedLayer
 * DateTime = Tue Apr 26 2022 12:28:47 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = TutorialDimmedLayer.ts
 * FileBasenameNoExtension = TutorialDimmedLayer
 * URL = db://assets/Scripts/Tutorial/TutorialDimmedLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

export enum TUTO_ARROW_DIR {
    UP,
    DOWN,
    LEFT,
    RIGHT,
    CUSTOM
}
 
@ccclass('TutorialDimmedLayer')
export class TutorialDimmedLayer extends Component {

    @property(Sprite)
    dimmedSprite: Sprite = null;
    @property(Node)
    highlightNode: Node = null;
    @property(Node)
    arrowImageNode: Node = null;
    @property(Prefab)
    arrowImagePrefab: Prefab = null;

    DEFAULT_ALPHA_VALUE: number = 125;                          // 기본 딤드 투명도
    SUB_ALPHA_VALUE: number = 35;                                // 서브 딤드 투명도
    REVISE_HIGHLIGHT_SIZE: number = 15;                         // 하이라이트 크기 보정치

    curDimmedInfo: BlockDimmedInfo = null;

    dimmedMaskSize: math.Size = new math.Size();

    arrowImageInstance: Node = null;
    arrowTween: Tween<Node> = null;

    onDisable() {
        this.arrowTween?.stop();

        this.arrowImageInstance?.destroy();
    }

    InitDimmedLayer(dimmedInfo: BlockDimmedInfo) {
        if (dimmedInfo == null) {return;}

        this.curDimmedInfo = dimmedInfo;

        this.dimmedMaskSize = this.curDimmedInfo.dimmedMaskSize;

        // 하이라이트 레이어 관련
        if (this.highlightNode != null && this.curDimmedInfo.useDimmedHighlight) {
            let highlightSize = new math.Size(this.dimmedMaskSize.width + this.REVISE_HIGHLIGHT_SIZE, this.dimmedMaskSize.height + this.REVISE_HIGHLIGHT_SIZE);
            this.highlightNode.getComponent(UITransform).contentSize = highlightSize;

            this.highlightNode.active = this.curDimmedInfo.useDimmedHighlight;
        }

        // 딤드 스프라이트 관련
        if (this.dimmedSprite != null) {
            let resultAlphaValue = this.curDimmedInfo.isDefaultDimmedLayer ? this.DEFAULT_ALPHA_VALUE : this.SUB_ALPHA_VALUE;

            this.dimmedSprite.color = new Color(this.dimmedSprite.color.r, this.dimmedSprite.color.g, this.dimmedSprite.color.b, resultAlphaValue);
        }

        // 화살표 이미지 관련
        if (this.arrowImageNode != null) {
            this.arrowImageNode.active = false;
        }
    
        if (this.arrowImageNode != null && this.curDimmedInfo.UseArrowImage) {

            this.arrowImageNode.active = this.curDimmedInfo.UseArrowImage && this.curDimmedInfo.arrowDirection != TUTO_ARROW_DIR.CUSTOM;

            let extraSpace = 10;        // 추가 여분 거리
            let spriteHalfSize = this.arrowImageNode.getComponent(UITransform).contentSize.height / 2;

            // 방향 설정
            let maskRadius = 0;
            let resultPos = 0;
            switch(this.curDimmedInfo.arrowDirection) {
                case TUTO_ARROW_DIR.UP:
                    maskRadius = this.curDimmedInfo.dimmedMaskSize.height / 2;
                    resultPos = maskRadius + spriteHalfSize + extraSpace;

                    this.arrowImageNode.setRotationFromEuler(0, 0, 0);
                    this.arrowImageNode.setPosition(0, resultPos, 0);
                    break;
                case TUTO_ARROW_DIR.DOWN:
                    maskRadius = this.curDimmedInfo.dimmedMaskSize.height / 2;
                    resultPos = -(maskRadius + spriteHalfSize + extraSpace);

                    this.arrowImageNode.setRotationFromEuler(0, 0, 180);
                    this.arrowImageNode.setPosition(0, resultPos, 0);
                    break;
                case TUTO_ARROW_DIR.LEFT:
                    maskRadius = this.curDimmedInfo.dimmedMaskSize.width / 2;
                    resultPos = -(maskRadius + spriteHalfSize + extraSpace);

                    this.arrowImageNode.setRotationFromEuler(0, 0, 90);
                    this.arrowImageNode.setPosition(resultPos, 0, 0);
                    break;
                case TUTO_ARROW_DIR.RIGHT:
                    maskRadius = this.curDimmedInfo.dimmedMaskSize.width / 2;
                    resultPos = maskRadius + spriteHalfSize + extraSpace;

                    this.arrowImageNode.setRotationFromEuler(0, 0, -90);
                    this.arrowImageNode.setPosition(resultPos, 0, 0);
                    break;
                case TUTO_ARROW_DIR.CUSTOM:
                    this.arrowImageInstance = instantiate(this.arrowImagePrefab);
                    this.arrowImageInstance.setParent(this.curDimmedInfo.arrowParentNode);
                    this.arrowImageInstance.setPosition(this.curDimmedInfo.arrowImagePos);

                    if (this.curDimmedInfo.arrowImageSize.width != 0 && this.curDimmedInfo.arrowImageSize.height != 0) {
                        this.arrowImageInstance.getComponent(UITransform).contentSize = this.curDimmedInfo.arrowImageSize;
                    }
                    break;
            }

            // 화살표 트윈 실행
            this.StartArrowTween();
        }
    }

    StartArrowTween() {
        switch(this.curDimmedInfo.arrowDirection) {
            case TUTO_ARROW_DIR.UP:
                this.arrowTween = tween(this.arrowImageNode).repeatForever(
                    tween()
                    .by(0.5, {position: new Vec3(0, 15, 0)}, {easing: easing.quartOut})
                    .by(0.5, {position: new Vec3(0, -15, 0)}, {easing: easing.quartIn})

                ).start();
                break;
            case TUTO_ARROW_DIR.DOWN:
                this.arrowTween = tween(this.arrowImageNode).repeatForever(
                    tween()
                    .by(0.5, {position: new Vec3(0, -15, 0)}, {easing: easing.quartOut})
                    .by(0.5, {position: new Vec3(0, 15, 0)}, {easing: easing.quartIn})

                ).start();
                break;
            case TUTO_ARROW_DIR.LEFT:
                this.arrowTween = tween(this.arrowImageNode).repeatForever(
                    tween()
                    .by(0.5, {position: new Vec3(-15, 0, 0)}, {easing: easing.quartOut})
                    .by(0.5, {position: new Vec3(15, 0, 0)}, {easing: easing.quartIn})

                ).start();
                break;
            case TUTO_ARROW_DIR.RIGHT:
                this.arrowTween = tween(this.arrowImageNode).repeatForever(
                    tween()
                    .by(0.5, {position: new Vec3(15, 0, 0)}, {easing: easing.quartOut})
                    .by(0.5, {position: new Vec3(-15, 0, 0)}, {easing: easing.quartIn})

                ).start();
                break;
            case TUTO_ARROW_DIR.CUSTOM:
                this.arrowTween = tween(this.arrowImageInstance).repeatForever(
                    tween()
                    .by(0.5, {position: new Vec3(0, 15, 0)}, {easing: easing.quartOut})
                    .by(0.5, {position: new Vec3(0, -15, 0)}, {easing: easing.quartIn})

                ).start();
                break;
        }
    }
}