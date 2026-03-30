import { _decorator, Component, Node, Label, Sprite, SpriteFrame,
         resources, CCInteger, UIOpacity, UITransform, tween, Tween, Size } from 'cc';
import { GameManager } from './GameManager';
import { AudioManager } from './AudioManager';
const { ccclass, property } = _decorator;

/**
 * HUDController - GameScene 내 HUD 요소 단독 제어 (M-03 역할 분리)
 * - 점수 / Wave / 라이프 아이콘 표시 단일 책임
 * - UIManager 는 씬 전환 페이드, 팝업 연출만 담당; HUD 갱신 콜백은 이 컴포넌트가 직접 등록
 * - 콤보 ×1.5 활성 시 showComboEffect() 팝업 (NEW-01)
 */
@ccclass('HUDController')
export class HUDController extends Component {

    @property({ type: Label })
    scoreLabel: Label | null = null;

    @property({ type: Label })
    waveLabel: Label | null = null;

    @property({ type: CCInteger })
    maxLives: number = 3;

    @property({ type: [Node] })
    lifeIcons: Node[] = [];

    /** 콤보 팝업 노드 — ui_combo_popup.svg Sprite 또는 Label 노드 (NEW-01) */
    @property({ type: Node })
    comboPopupNode: Node | null = null;

    /** Wave 진행도 바 배경 노드 (ui_progress_bg.svg) — spec_v4 신규 */
    @property({ type: Node })
    waveProgressNode: Node | null = null;

    /** Wave 진행도 채움 Sprite (ui_progress_fill.svg) — spec_v4 신규 */
    @property({ type: Sprite })
    waveProgressFill: Sprite | null = null;

    // 콜백 참조 (onDestroy 해제용)
    private _onScoreCb:   ((score: number) => void) | null = null;
    private _onLivesCb:   ((lives: number) => void) | null = null;
    private _onWaveCb:    ((wave: number) => void) | null = null;
    private _onComboCb:   (() => void) | null = null;

    onLoad() {
        // onLoad에서는 GameManager 접근 하지 않음 — start()에서 처리
        // (CC3.x: 모든 onLoad() 완료 후 start() 실행 보장)
    }

    onDestroy() {
        const gm = GameManager.instance;
        if (!gm) return;
        if (gm.onScoreChanged  === this._onScoreCb)  gm.onScoreChanged  = null;
        if (gm.onLivesChanged  === this._onLivesCb)  gm.onLivesChanged  = null;
        if (this._onWaveCb) gm.removeWaveChangedListener(this._onWaveCb);
        if (gm.onComboActivated === this._onComboCb) gm.onComboActivated = null;
    }

    start() {
        // M-03: start()에서 GameManager 콜백 등록 — 모든 onLoad() 완료 후 실행 보장
        const gm = GameManager.instance;
        if (gm) {
            this._onScoreCb = (score) => this.updateScore(score);
            this._onLivesCb = (lives) => this.updateLives(lives);
            this._onWaveCb  = (wave)  => this.updateWave(wave);
            this._onComboCb = ()      => this.showComboEffect();

            gm.onScoreChanged   = this._onScoreCb;
            gm.onLivesChanged   = this._onLivesCb;
            gm.addWaveChangedListener(this._onWaveCb);
            gm.onComboActivated = this._onComboCb;

            // 초기 HUD 값 반영 (startGame() 이후 상태)
            this.updateScore(gm.score);
            this.updateLives(gm.lives);
            this.updateWave(gm.currentWave);
        }
        this._loadLifeIconSprite();
        // 콤보 팝업 초기 비활성화
        if (this.comboPopupNode) this.comboPopupNode.active = false;
        // 진행도 바 초기화
        this._initWaveProgress();
    }

    private _loadLifeIconSprite() {
        resources.load('icon_life/spriteFrame', SpriteFrame, (err, sf) => {
            if (err) return;
            for (const icon of this.lifeIcons) {
                const sp = icon.getComponent(Sprite);
                if (sp) sp.spriteFrame = sf;
            }
        });
    }

    updateScore(score: number) {
        if (this.scoreLabel) {
            this.scoreLabel.string = score.toLocaleString();
        }
    }

    updateWave(wave: number) {
        if (this.waveLabel) {
            this.waveLabel.string = `Wave ${wave}`;
        }
    }

    updateLives(lives: number) {
        for (let i = 0; i < this.lifeIcons.length; i++) {
            this.lifeIcons[i].active = i < lives;
        }
    }

    /**
     * 콤보 ×1.5 활성 시 "COMBO ×1.5!" 팝업 표시 (NEW-01)
     * - comboPopupNode 활성화 후 UIOpacity tween으로 1.5초 후 페이드 아웃
     * - UI tween이므로 GameState.PAUSED 여부와 무관하게 재생됨 (M-04 정책 준수)
     * - m-04: 팝업이 이미 활성 상태이면 기존 tween 중단 후 opacity 초기화 및 재실행
     */
    showComboEffect() {
        if (!this.comboPopupNode) return;

        const popup = this.comboPopupNode;

        // m-04: tween 중첩 방지 — 이미 활성화 중이면 기존 tween 중단 후 재시작
        if (popup.active) {
            tween(popup).stop();
            const existingOpacity = popup.getComponent(UIOpacity);
            if (existingOpacity) tween(existingOpacity).stop();
        }

        popup.active = true;

        const opacity = popup.getComponent(UIOpacity);
        if (opacity) opacity.opacity = 255;

        // tween: 1.5초 유지 후 0.3초 페이드 아웃
        tween(popup)
            .delay(1.5)
            .call(() => {
                if (opacity) {
                    tween(opacity)
                        .to(0.3, { opacity: 0 })
                        .call(() => { popup.active = false; })
                        .start();
                } else {
                    popup.active = false;
                }
            })
            .start();
    }

    // ===== Wave 진행도 바 (spec_v4 신규) =====

    /** 진행도 바 초기화 — start() 에서 호출 */
    private _initWaveProgress() {
        if (!this.waveProgressFill) return;
        const fill = this.waveProgressFill;
        const uiTransform = fill.node.getComponent(UITransform);
        if (uiTransform) uiTransform.setContentSize(0, 12);
    }

    /**
     * Wave 진행도 바 갱신 (spec_v4 메카닉 4)
     * @param current 현재까지 완성/수집한 수
     * @param total   이번 Wave 총 목표 수
     *
     * - Wave 클리어 순간(current >= total): 0.3초 tween으로 100% 채움 후
     *   0.5초 대기 → 진행도 초기화 + AudioManager.playProgressComplete()
     * - 일반 갱신: fillNode 너비를 (current/total)*120 으로 즉시 설정
     */
    updateWaveProgress(current: number, total: number) {
        if (!this.waveProgressFill) return;

        const fill = this.waveProgressFill;
        const uiTransform = fill.node.getComponent(UITransform);
        if (!uiTransform) return;

        const ratio = Math.min(current / Math.max(total, 1), 1);
        const targetWidth = Math.round(ratio * 120);

        if (current >= total) {
            // Wave 클리어: 100%로 빠르게 채움 후 초기화 (n-06: tween 중첩 방지)
            Tween.stopAllByTarget(uiTransform);
            const height = uiTransform.contentSize.height;
            // tween 대상을 wrapper 객체로 안전하게 처리
            const widthProxy = { w: uiTransform.contentSize.width };
            tween(widthProxy)
                .to(0.3, { w: 120 }, {
                    onUpdate: () => {
                        uiTransform.contentSize = new Size(widthProxy.w, height);
                    }
                })
                .call(() => {
                    AudioManager.instance?.playProgressComplete();
                })
                .delay(0.5)
                .call(() => {
                    uiTransform.contentSize = new Size(0, height);
                })
                .start();
        } else {
            // 일반 갱신: 즉시 너비 설정
            uiTransform.setContentSize(targetWidth, uiTransform.contentSize.height);
        }
    }
}
