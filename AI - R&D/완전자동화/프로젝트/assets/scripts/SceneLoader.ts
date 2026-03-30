import { _decorator, Component, Node, director, tween, UIOpacity, CCFloat } from 'cc';
const { ccclass, property } = _decorator;

/**
 * SceneLoader - 씬 전환 유틸리티 (페이드 인/아웃 포함)
 * - 각 씬에 부착하거나 GameManager 노드에 추가
 * - loadScene(name) 호출 시 페이드 아웃 → 씬 로드 → 페이드 인
 */
@ccclass('SceneLoader')
export class SceneLoader extends Component {

    @property({ type: Node })
    fadeOverlay: Node | null = null;

    @property({ type: CCFloat })
    fadeDuration: number = 0.4;

    private static _instance: SceneLoader | null = null;
    public static get instance(): SceneLoader | null { return SceneLoader._instance; }

    onLoad() {
        SceneLoader._instance = this;
    }

    onDestroy() {
        if (SceneLoader._instance === this) SceneLoader._instance = null;
    }

    loadScene(sceneName: string) {
        this._fadeOut(() => {
            director.loadScene(sceneName, () => {
                // 씬 로드 완료 후 새 씬에서 페이드인은 UIManager.fadeIn() 이 담당
            });
        });
    }

    private _fadeOut(onComplete: () => void) {
        if (!this.fadeOverlay) { onComplete(); return; }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { onComplete(); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 0;
        tween(opacity)
            .to(this.fadeDuration, { opacity: 255 })
            .call(onComplete)
            .start();
    }
}
