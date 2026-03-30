import { _decorator, Component, Node, director } from 'cc';
import { GameManager, GameState } from './GameManager';
import { StarSpawner } from './StarSpawner';
import { ConstellationManager } from './ConstellationManager';
import { UIManager } from './UIManager';
import { WaveManager } from './WaveManager';
const { ccclass, property } = _decorator;

/**
 * GameScene - GameScene 씬 초기화 및 조율 스크립트
 * - 씬 진입 시 GameManager.startGame() 호출
 * - 각 시스템 간 연결 (StarSpawner <-> ConstellationManager)
 * - 일시정지 버튼 처리
 */
@ccclass('GameScene')
export class GameScene extends Component {

    @property({ type: Node })
    starSpawnerNode: Node | null = null;

    @property({ type: Node })
    constellationManagerNode: Node | null = null;

    @property({ type: Node })
    uiManagerNode: Node | null = null;

    @property({ type: Node })
    waveManagerNode: Node | null = null;

    @property({ type: Node })
    pausePanel: Node | null = null;

    private _spawner: StarSpawner | null = null;
    private _constellationMgr: ConstellationManager | null = null;
    private _uiManager: UIManager | null = null;
    private _waveManager: WaveManager | null = null;

    private _onWaveChangedCb: ((wave: number) => void) | null = null;

    onLoad() {
        // 컴포넌트 참조 확보
        if (this.starSpawnerNode) {
            this._spawner = this.starSpawnerNode.getComponent(StarSpawner);
        }
        if (this.constellationManagerNode) {
            this._constellationMgr = this.constellationManagerNode.getComponent(ConstellationManager);
        }
        if (this.uiManagerNode) {
            this._uiManager = this.uiManagerNode.getComponent(UIManager);
        }
        if (this.waveManagerNode) {
            this._waveManager = this.waveManagerNode.getComponent(WaveManager);
        }

        // StarSpawner에 ConstellationManager 연결
        if (this._spawner && this._constellationMgr) {
            this._spawner.setConstellationManager(this._constellationMgr);
        }

        // 일시정지 패널 초기 비활성화
        if (this.pausePanel) this.pausePanel.active = false;
    }

    start() {
        // GameManager 초기화 및 게임 시작
        const gm = GameManager.instance;
        if (gm) {
            gm.startGame();
            // Wave 변경 시 StarSpawner와 WaveManager 모두 구독 (배열 방식 - 덮어쓰기 없음)
            this._onWaveChangedCb = (wave: number) => {
                this._spawner?.applyWaveConfig();
                this._waveManager?.announceWave(wave);
            };
            gm.addWaveChangedListener(this._onWaveChangedCb);
        }

        // 스폰 초기 Wave 설정
        this._spawner?.applyWaveConfig();

        // 페이드 인
        this._uiManager?.fadeIn();
    }

    onDestroy() {
        // GameManager persist 상태 대응 - 씬 언로드 시 콜백 해제
        if (this._onWaveChangedCb) {
            GameManager.instance?.removeWaveChangedListener(this._onWaveChangedCb);
            this._onWaveChangedCb = null;
        }
    }

    // ===== 일시정지 버튼 =====

    onPauseButtonClicked() {
        const gm = GameManager.instance;
        if (!gm) return;
        if (gm.state === GameState.PLAYING) {
            gm.pauseGame();
            if (this.pausePanel) this.pausePanel.active = true;
        }
    }

    onResumeButtonClicked() {
        const gm = GameManager.instance;
        if (!gm) return;
        if (gm.state === GameState.PAUSED) {
            gm.resumeGame();
            if (this.pausePanel) this.pausePanel.active = false;
        }
    }

    onTitleButtonClicked() {
        this._spawner?.clearAll();
        this._uiManager?.fadeOut(() => {
            director.loadScene('TitleScene');
        });
    }
}
