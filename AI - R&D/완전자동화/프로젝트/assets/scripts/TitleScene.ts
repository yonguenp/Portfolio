import { _decorator, Component, Node, Label, director, tween, UIOpacity } from 'cc';
import { DataManager } from './DataManager';
import { AudioManager } from './AudioManager';
const { ccclass, property } = _decorator;

/**
 * TitleScene - 타이틀 화면 초기화 및 버튼 처리
 * - 최고 점수 표시
 * - 게임 시작 버튼 → GameScene 로드
 * - 별자리 도감 버튼 → ConstellationBookScene 로드 (spec_v3 신규)
 * - BGM 재생
 */
@ccclass('TitleScene')
export class TitleScene extends Component {

    @property({ type: Label })
    bestScoreLabel: Label | null = null;

    @property({ type: Node })
    fadeOverlay: Node | null = null;

    /** 별자리 도감 버튼 노드 — icon_book.svg Sprite 사용 (spec_v3 신규) */
    @property({ type: Node })
    bookButton: Node | null = null;

    start() {
        // 최고 점수 표시
        const best = DataManager.loadBestScore();
        if (this.bestScoreLabel) {
            this.bestScoreLabel.string = `Best: ${best.toLocaleString()}`;
        }

        // BGM 재생
        AudioManager.instance?.playBGMTitle();

        // 페이드 인
        this._fadeIn();
    }

    private _fadeIn() {
        if (!this.fadeOverlay) return;
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) return;
        this.fadeOverlay.active = true;
        opacity.opacity = 255;
        tween(opacity)
            .to(0.4, { opacity: 0 })
            .call(() => { this.fadeOverlay!.active = false; })
            .start();
    } 

    onStartButtonClicked() {
        console.log('on start');
        if (!this.fadeOverlay) {
            director.loadScene('GameScene');
            return;
        }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { director.loadScene('GameScene'); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 0;
        tween(opacity)
            .to(0.4, { opacity: 255 })
            .call(() => { director.loadScene('GameScene'); })
            .start();
    }

    /** 별자리 도감 버튼 클릭 — ConstellationBookScene 로드 (spec_v3 신규) */
    onBookButtonClicked() {
        console.log('on book');
        if (!this.fadeOverlay) {
            director.loadScene('ConstellationBookScene');
            return;
        }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { director.loadScene('ConstellationBookScene'); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 0;
        tween(opacity)
            .to(0.4, { opacity: 255 })
            .call(() => { director.loadScene('ConstellationBookScene'); })
            .start();
    }
}
