
import { _decorator, Component, Node, Tween, tween, Vec3, easing } from 'cc';
import { SceneManager } from '../SceneManager';
import { PopupManager } from '../UI/Common/PopupManager';
import { UICanvas } from '../UI/UICanvas';
const { ccclass, property } = _decorator;

@ccclass('TutorialGuideBox')
export class TutorialGuideBox extends Component {
    
    guideboxTween: Tween<Node> = null;

    onDisable() {
        this.guideboxTween?.stop();
    }

    InitGuideBox() {
        this.StartGuideBoxTween();
    }

    StartGuideBoxTween() {
        this.node.setScale(new Vec3(0,0,0));

        this.guideboxTween = tween(this.node)
        .to(0.5, { scale: new Vec3(1, 1, 1)} , {easing: easing.elasticOut})
        .call(() => {
            this.OrthoRefresh(this.node);
        })
        .start();
    }

    // 화면 비율 대비 사이즈 최적화
    OrthoRefresh(orthoNodeObject: Node) {           
        //임시처리 - game Scene에서 카메라 축소/확대 상태가 다른 씬UI 에도 전이되는 버그
        if (PopupManager.GetPopupStackCount() > 0) {return;}    // 팝업이 켜진 상태면 실행하지 않음

        let checkSceneName = SceneManager.Instance.GetSceneTargetName();
        if(checkSceneName != "game"){return;}
        
        const orthoY = UICanvas.globalMainCamRef.orthoHeight * 2; 
        const orthoScope = orthoY / 9;
        const orthoX = orthoScope * 16;

        if(orthoNodeObject != null) {
            orthoNodeObject.setScale(orthoX / 1280, orthoY / 720); 
        }
    }
}