import { _decorator, Component, Node, director } from 'cc';
import { AudioManager } from './AudioManager';
import { DataManager } from './DataManager';
const { ccclass, property } = _decorator;

export enum GameState {
    IDLE = 'IDLE',
    PLAYING = 'PLAYING',
    PAUSED = 'PAUSED',
    GAME_OVER = 'GAME_OVER',
}

export enum StarColor {
    RED = 'RED',
    BLUE = 'BLUE',
    YELLOW = 'YELLOW',
    GREEN = 'GREEN',
    PURPLE = 'PURPLE',
    DARK = 'DARK',
}

export const STAR_SCORE: Record<string, number> = {
    RED: 10,
    BLUE: 10,
    YELLOW: 15,
    GREEN: 15,
    PURPLE: 20,
    DARK: 0,
};

export interface WaveConfig {
    wave: number;
    fallSpeed: number;
    spawnInterval: number;
    isBossWave: boolean;
    availableColors: StarColor[];
}

// spec_v2.md "Wave 색상 설계표" 기준으로 완전 동기화
// Wave 1: RED, BLUE 만 스폰 (YELLOW/GREEN/PURPLE 절대 없음)
// Wave 2: RED, BLUE, YELLOW (GREEN/PURPLE 없음)
// Wave 3: RED, BLUE, YELLOW, GREEN — 보스 웨이브 (PURPLE 없음)
// Wave 4: RED, BLUE, YELLOW, GREEN (4색 유지)
// Wave 5: 전체 5색 (RED, BLUE, YELLOW, GREEN, PURPLE)
// Wave 6+: 전체 5색 유지, 보스 웨이브 3Wave마다 반복
const WAVE_CONFIGS: WaveConfig[] = [
    { wave: 1, fallSpeed: 200, spawnInterval: 1.5, isBossWave: false, availableColors: [StarColor.RED, StarColor.BLUE] },
    { wave: 2, fallSpeed: 240, spawnInterval: 1.3, isBossWave: false, availableColors: [StarColor.RED, StarColor.BLUE, StarColor.YELLOW] },
    { wave: 3, fallSpeed: 280, spawnInterval: 1.1, isBossWave: true,  availableColors: [StarColor.RED, StarColor.BLUE, StarColor.YELLOW, StarColor.GREEN] },
    { wave: 4, fallSpeed: 320, spawnInterval: 1.0, isBossWave: false, availableColors: [StarColor.RED, StarColor.BLUE, StarColor.YELLOW, StarColor.GREEN] },
    { wave: 5, fallSpeed: 360, spawnInterval: 0.9, isBossWave: false, availableColors: [StarColor.RED, StarColor.BLUE, StarColor.YELLOW, StarColor.GREEN, StarColor.PURPLE] },
    { wave: 6, fallSpeed: 380, spawnInterval: 0.85, isBossWave: true, availableColors: [StarColor.RED, StarColor.BLUE, StarColor.YELLOW, StarColor.GREEN, StarColor.PURPLE] },
];

/**
 * GameManager - 게임 전체 상태 관리 싱글톤
 * - 점수 / 라이프 / Wave 관리
 * - 콤보 시스템 (연속 3개 수집 시 ×1.5 배수)
 * - 별자리 완성 이벤트 처리
 * - 최고 점수 영속 저장 (sys.localStorage)
 */
@ccclass('GameManager')
export class GameManager extends Component {

    private static _instance: GameManager | null = null;
    public static get instance(): GameManager | null {
        if (!GameManager._instance) {
            console.warn('[GameManager] instance not yet initialized');
            return null;
        }
        return GameManager._instance;
    }

    // --- 게임 상태 ---
    private _state: GameState = GameState.IDLE;
    private _score: number = 0;
    private _bestScore: number = 0;
    private _lives: number = 3;
    private _currentWave: number = 1;
    private _comboCount: number = 0;
    private _constellationsCompleted: number = 0;

    // --- 이벤트 콜백 (배열 기반 - 다수 구독자 지원) ---
    public onScoreChanged: ((score: number) => void) | null = null;
    public onLivesChanged: ((lives: number) => void) | null = null;
    private _onWaveChangedListeners: Array<(wave: number) => void> = [];
    public onGameOver: (() => void) | null = null;
    public onConstellationCompleted: (() => void) | null = null;
    /** 콤보 ×1.5 달성 시 HUDController.showComboEffect() 호출용 콜백 (NEW-01) */
    public onComboActivated: (() => void) | null = null;

    /** onWaveChanged 구독자 등록 */
    public addWaveChangedListener(fn: (wave: number) => void) {
        if (!this._onWaveChangedListeners.includes(fn)) {
            this._onWaveChangedListeners.push(fn);
        }
    }

    /** onWaveChanged 구독자 해제 */
    public removeWaveChangedListener(fn: (wave: number) => void) {
        const idx = this._onWaveChangedListeners.indexOf(fn);
        if (idx !== -1) this._onWaveChangedListeners.splice(idx, 1);
    }

    /** 하위 호환: 단일 콜백 직접 할당도 지원 (내부적으로 배열에 추가) */
    public set onWaveChanged(fn: ((wave: number) => void) | null) {
        // 기존 단일-콜백 슬롯 교체 대신 배열에 추가
        if (fn) this.addWaveChangedListener(fn);
    }

    private _fireWaveChanged(wave: number) {
        this._onWaveChangedListeners.forEach(fn => fn(wave));
    }

    // --- 접근자 ---
    get state(): GameState { return this._state; }
    get score(): number { return this._score; }
    get bestScore(): number { return this._bestScore; }
    get lives(): number { return this._lives; }
    get currentWave(): number { return this._currentWave; }
    get comboCount(): number { return this._comboCount; }

    onLoad() {
        if (GameManager._instance && GameManager._instance !== this) {
            this.node.destroy();
            return;
        }
        GameManager._instance = this;
        director.addPersistRootNode(this.node);
        // m-03: 최고 점수 로드도 DataManager 통해 일원화
        this._bestScore = DataManager.loadBestScore();
    }

    onDestroy() {
        if (GameManager._instance === this) {
            GameManager._instance = null;
        }
    }

    // ===== 게임 흐름 =====

    startGame() {
        this._score = 0;
        this._lives = 3;
        this._currentWave = 1;
        this._comboCount = 0;
        this._constellationsCompleted = 0;
        this._state = GameState.PLAYING;

        this.onScoreChanged?.(this._score);
        this.onLivesChanged?.(this._lives);
        this._fireWaveChanged(this._currentWave);
    }

    pauseGame() {
        if (this._state !== GameState.PLAYING) return;
        // director.pause() 대신 GameState.PAUSED 플래그 방식 사용 (M-04)
        // StarSpawner/StarFragment update()에서 이 상태를 직접 체크하여 로직만 정지
        // UIManager tween, WaveManager 팝업 페이드 등 UI 애니메이션은 계속 동작
        this._state = GameState.PAUSED;
    }

    resumeGame() {
        if (this._state !== GameState.PAUSED) return;
        this._state = GameState.PLAYING;
    }

    triggerGameOver() {
        if (this._state === GameState.GAME_OVER) return;
        this._state = GameState.GAME_OVER;
        // m-03: DataManager 단독 저장 — GameManager._saveBestScore() 완전 제거
        DataManager.saveBestScore(this._score);
        this.onGameOver?.();
    }

    // ===== 점수 & 라이프 =====

    addScore(base: number) {
        const multiplier = this._comboCount >= 3 ? 1.5 : 1.0;
        const earned = Math.floor(base * multiplier);
        this._score += earned;
        this.onScoreChanged?.(this._score);
    }

    loseLife(amount: number = 1) {
        this._lives = Math.max(0, this._lives - amount);
        this._comboCount = 0;
        // SFX: 라이프 감소음 (M-02)
        AudioManager.instance?.playLoseLife();
        this.onLivesChanged?.(this._lives);
        if (this._lives <= 0) {
            this.triggerGameOver();
        }
    }

    incrementCombo() {
        this._comboCount++;
        // 콤보 3 달성 순간에만 이벤트 발화 (×1.5 활성 전환 시점)
        if (this._comboCount === 3) {
            AudioManager.instance?.playCombo();
            this.onComboActivated?.();
        }
    }

    resetCombo() {
        this._comboCount = 0;
    }

    // ===== 별자리 완성 =====

    onConstellationDone() {
        this._constellationsCompleted++;
        this.addScore(200);
        this.onConstellationCompleted?.();
        // Wave 완료 조건: 매 3개 별자리마다 wave 상승
        if (this._constellationsCompleted % 3 === 0) {
            this._advanceWave();
        }
    }

    private _advanceWave() {
        this._currentWave++;
        // SFX: Wave 클리어음 (spec_v2.md M-02)
        AudioManager.instance?.playWaveClear();
        this._fireWaveChanged(this._currentWave);
    }

    // ===== Wave 설정 반환 =====

    getCurrentWaveConfig(): WaveConfig {
        if (this._currentWave <= WAVE_CONFIGS.length) {
            return WAVE_CONFIGS[this._currentWave - 1];
        }
        const extra = this._currentWave - WAVE_CONFIGS.length;
        const last = WAVE_CONFIGS[WAVE_CONFIGS.length - 1];
        return {
            wave: this._currentWave,
            fallSpeed: last.fallSpeed + extra * 20,
            spawnInterval: Math.max(0.4, last.spawnInterval - extra * 0.05),
            isBossWave: this._currentWave % 3 === 0,
            availableColors: [...last.availableColors],
        };
    }

    // ===== 데이터 영속 =====
    // m-03: _loadBestScore() / _saveBestScore() 완전 제거
    // 최고 점수 저장/로드는 DataManager 단독 처리 (sys.localStorage 직접 접근 코드 제거)

    getBestScore(): number {
        return this._bestScore;
    }
}
