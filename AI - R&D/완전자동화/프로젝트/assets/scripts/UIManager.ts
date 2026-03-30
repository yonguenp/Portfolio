import { _decorator, Component, Node, Label, director, tween, Vec3,
         UIOpacity, CCFloat } from 'cc';
import { GameManager } from './GameManager';
const { ccclass, property } = _decorator;

/**
 * UIManager - 화면 전환 연출(페이드 인/아웃) + Wave 팝업 전담 (M-03 역할 분리)
 * - HUD 갱신(점수/라이프/Wave 표시)은 HUDController 단독 담당
 * - 이 컴포넌트는 씬 전환 페이드, Wave 팝업 텍스트, 게임 오버 전환만 처리
 * - scoreLabel / waveLabel / lifeIconsRoot 중복 프로퍼티 제거 완료
 */
@ccclass('UIManager')
export class UIManager extends Component {

    private static _instance: UIManager | null = null;
    public static get instance(): UIManager | null {
        return UIManager._instance;
    }

    // 페이드 오버레이 노드 (UIOpacity 컴포넌트 보유)
    @property({ type: Node })
    fadeOverlay: Node | null = null;

    // Wave 전환 팝업 텍스트 (일시적으로 표시)
    @property({ type: Label })
    wavePopupLabel: Label | null = null;

    @property({ type: CCFloat })
    fadeDuration: number = 0.4;

    // 은하의 심연 최초 완성 연출 노드 (NEW-04)
    @property({ type: Node })
    galaxyEffectNode: Node | null = null;

    // 콜백 참조 보관 (onDestroy에서 해제용)
    private _onWaveCb:     ((wave: number) => void) | null = null;
    private _onGameOverCb: (() => void) | null = null;

    onLoad() {
        UIManager._instance = this;
        const gm = GameManager.instance;
        if (!gm) return;

        // M-03: UIManager는 Wave 팝업 연출과 게임 오버 전환만 구독
        // HUD(점수/라이프/Wave 라벨) 갱신은 HUDController에서 처리
        this._onWaveCb     = (wave) => this._onWaveChanged(wave);
        this._onGameOverCb = () => this._onGameOver();

        gm.addWaveChangedListener(this._onWaveCb);
        gm.onGameOver = this._onGameOverCb;
    }

    onDestroy() {
        if (UIManager._instance === this) UIManager._instance = null;
        const gm = GameManager.instance;
        if (!gm) return;

        if (this._onWaveCb) gm.removeWaveChangedListener(this._onWaveCb);
        if (gm.onGameOver === this._onGameOverCb) gm.onGameOver = null;
    }

    start() {
        // UIManager는 HUD 초기값 갱신 없음 — HUDController.start()에서 처리
    }

    // ===== Wave 팝업 =====

    private _onWaveChanged(wave: number) {
        this._showWavePopup(wave);
    }

    private _showWavePopup(wave: number) {
        if (!this.wavePopupLabel) return;
        this.wavePopupLabel.string = `Wave ${wave} Start!`;
        this.wavePopupLabel.node.active = true;

        const opacity = this.wavePopupLabel.node.getComponent(UIOpacity);
        if (opacity) 
            opacity.opacity = 255;

        console.log('wave panel show');
        tween(this.wavePopupLabel.node)
            .delay(0.8)
            .call(() => {
                if (opacity) {
                    tween(opacity)
                        .to(0.4, { opacity: 0 })
                        .call(() => { 
                            console.log('wave panel hide');
                            this.wavePopupLabel.node.active = false; 
                        })
                        .start();
                }
                else
                {
                    console.log('wave panel hide');
                    this.wavePopupLabel.node.active = false; 
                }
            })
            .start();
    }

    private _onGameOver() {
        this.fadeOut(() => {
            director.loadScene('ResultScene');
        });
    }

    // ===== 씬 전환 페이드 =====

    fadeIn(onComplete?: () => void) {
        if (!this.fadeOverlay) { onComplete?.(); return; }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { onComplete?.(); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 255;
        tween(opacity)
            .to(this.fadeDuration, { opacity: 0 })
            .call(() => {
                this.fadeOverlay!.active = false;
                onComplete?.();
            })
            .start();
    }

    fadeOut(onComplete?: () => void) {
        if (!this.fadeOverlay) { onComplete?.(); return; }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { onComplete?.(); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 0;
        tween(opacity)
            .to(this.fadeDuration, { opacity: 255 })
            .call(() => { onComplete?.(); })
            .start();
    }

    // ===== 은하의 심연 최초 완성 연출 (NEW-04) =====

    /**
     * 은하 이펙트 노드 Fade-In → 유지 → Fade-Out (총 2.5초)
     * @returns Promise (2.5초 후 resolve)
     */
    showGalaxyEffect(): Promise<void> {
        return new Promise<void>((resolve) => {
            const node = this.galaxyEffectNode;
            if (!node) {
                this.scheduleOnce(() => resolve(), 2.5);
                return;
            }
            node.active = true;
            const opacity = node.getComponent(UIOpacity);
            if (opacity) opacity.opacity = 0;

            // 0.5초 Fade-In
            if (opacity) {
                tween(opacity)
                    .to(0.5, { opacity: 255 })
                    .delay(1.0)
                    .to(1.0, { opacity: 0 })
                    .call(() => {
                        node.active = false;
                        resolve();
                    })
                    .start();
            } else {
                this.scheduleOnce(() => {
                    node.active = false;
                    resolve();
                }, 2.5);
            }
        });
    }
}
