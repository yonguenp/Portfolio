import { _decorator, Component, Node, Label, director, tween, UIOpacity } from 'cc';
import { GameManager } from './GameManager';
import { DataManager } from './DataManager';
import { AudioManager } from './AudioManager';
const { ccclass, property } = _decorator;

/**
 * ResultScene - 게임 오버 결과 화면
 * - 이번 점수 / 최고 점수 표시
 * - 재시작 / 타이틀로 버튼 처리
 */
@ccclass('ResultScene')
export class ResultScene extends Component {

    @property({ type: Label })
    currentScoreLabel: Label | null = null;

    @property({ type: Label })
    bestScoreLabel: Label | null = null;

    @property({ type: Node })
    fadeOverlay: Node | null = null;

    start() {
        const gm = GameManager.instance;
        const score   = gm?.score ?? 0;
        const best    = DataManager.loadBestScore();

        if (this.currentScoreLabel) {
            this.currentScoreLabel.string = `Score: ${score.toLocaleString()}`;
        }
        if (this.bestScoreLabel) {
            this.bestScoreLabel.string = `Best: ${best.toLocaleString()}`;
        }

        // 게임 오버 SFX
        AudioManager.instance?.playGameOver();

        // 페이드 인
        this._fadeIn();
    }

    private _fadeIn() {
        if (!this.fadeOverlay) return;
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) return;
        console.log('fade start');
        this.fadeOverlay.active = true;
        opacity.opacity = 255;
        tween(opacity)
            .to(0.4, { opacity: 0 })
            .call(() => { 
                console.log('fade done');
                this.fadeOverlay.active = false; 
            })
            .start();
    }

    onRestartButtonClicked() {
        console.log('on Restart');
        this._fadeOutThen('GameScene');
    }

    onTitleButtonClicked() {
        console.log('on Title');
        this._fadeOutThen('TitleScene');
    }

    private _fadeOutThen(sceneName: string) {
        if (!this.fadeOverlay) { director.loadScene(sceneName); return; }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { director.loadScene(sceneName); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 0;
        tween(opacity)
            .to(0.4, { opacity: 255 })
            .call(() => { director.loadScene(sceneName); })
            .start();
    }
}
