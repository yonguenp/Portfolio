import { _decorator, Component, Node, view } from 'cc';
import { GameManager, GameState, StarColor } from './GameManager';
import { StarFragment } from './StarFragment';
import { BucketController } from './BucketController';
import { AudioManager } from './AudioManager';
import { ConstellationManager } from './ConstellationManager';
import { ScoreFloater } from './ScoreFloater';
const { ccclass, property } = _decorator;

/**
 * StarSpawner - Wave별 별 조각 스폰 관리 + AABB 충돌 판정
 * - 자체 내장 풀 방식 (ObjectPool @property 의존 제거 — Prefab 없이도 동작)
 * - onLoad()에서 bucketNode 자동 탐색 (씬에서 미연결 시 형제 노드 탐색)
 * - 타이머 기반 스폰, 매 프레임 AABB 충돌 검사
 */
@ccclass('StarSpawner')
export class StarSpawner extends Component {

    @property({ type: Node })
    bucketNode: Node | null = null;

    /** 사전 생성할 풀 크기 */
    private readonly POOL_INITIAL = 15;
    /** 내장 StarFragment 노드 풀 */
    private _pool: Node[] = [];

    private _activeStars: StarFragment[] = [];
    private _timer: number = 0;
    private _spawnInterval: number = 1.5;
    private _fallSpeed: number = 200;
    private _availableColors: StarColor[] = [StarColor.RED, StarColor.BLUE];
    private _isBossWave: boolean = false;

    private _screenHalfW: number = 460;
    private _spawnY: number = 380;
    private _bottomY: number = -380;

    onLoad() {
        const size = view.getVisibleSize();
        this._screenHalfW = size.width / 2 - 20;
        this._spawnY = size.height / 2 + 30;
        this._bottomY = -size.height / 2 - 30;

        // bucketNode가 에디터에서 미연결인 경우 형제 노드에서 자동 탐색
        if (!this.bucketNode && this.node.parent) {
            this.bucketNode = this.node.parent.getChildByName('BucketNode');
        }

        // 풀 사전 생성
        for (let i = 0; i < this.POOL_INITIAL; i++) {
            const n = this._createStarNode();
            n.active = false;
            this._pool.push(n);
        }

        console.log('spawner loaded');
    }

    /** 노드를 새로 생성하고 StarFragment 컴포넌트를 부착 */
    private _createStarNode(): Node {
        const node = new Node('StarFragment');
        node.addComponent(StarFragment);
        this.node.addChild(node);
        return node;
    }

    /** 풀에서 비활성 노드를 꺼내거나 새로 생성 */
    private _getFromPool(): Node {
        let node = this._pool.find(n => !n.active);
        if (!node) {
            node = this._createStarNode();
            this._pool.push(node);
        }
        node.active = true;
        return node;
    }

    /** 풀로 반환 (비활성화만) */
    private _returnToPool(sf: StarFragment) {
        sf.reset();
        this._activeStars = this._activeStars.filter(s => s !== sf);
        sf.node.active = false;
    }

    // Wave 설정 적용
    applyWaveConfig() {
        const gm = GameManager.instance;
        if (!gm) return;
        const cfg = gm.getCurrentWaveConfig();
        this._fallSpeed = cfg.fallSpeed;
        this._spawnInterval = cfg.spawnInterval;
        this._availableColors = cfg.availableColors;
        this._isBossWave = cfg.isBossWave;
        this._timer = 0;
    }

    update(deltaTime: number) {
        // GameState.PAUSED 포함 PLAYING 이 아닌 모든 상태에서 스폰/충돌 로직 정지 (M-04)
        if (GameManager.instance?.state !== GameState.PLAYING) return;
        
        // 스폰 타이머
        this._timer += deltaTime;
        if (this._timer >= this._spawnInterval) {
            this._timer = 0;
            this._spawnStar();
        }

        // AABB 충돌 검사
        this._checkCollisions();
    }

    private _spawnStar() {
        const node = this._getFromPool();
        const sf = node.getComponent(StarFragment);
        if (!sf) {
            node.active = false;
            return;
        }

        const color = this._pickColor();
        const spawnX = (Math.random() * 2 - 1) * this._screenHalfW;

        sf.onMiss = (s) => this._onStarMissed(s);
        sf.onCatch = (s) => this._onStarCaught(s);
        sf.setBottomBound(this._bottomY);
        sf.init(color, spawnX, this._spawnY, this._fallSpeed);

        this._activeStars.push(sf);
    }

    private _pickColor(): StarColor {
        // 보스 웨이브: 30% 확률로 DARK Star 추가
        if (this._isBossWave && Math.random() < 0.3) {
            return StarColor.DARK;
        }
        const idx = Math.floor(Math.random() * this._availableColors.length);
        return this._availableColors[idx];
    }

    private _checkCollisions() {
        if (!this.bucketNode) return;
        const bucket = this.bucketNode.getComponent(BucketController);
        if (!bucket) return;

        const b = bucket.getBounds();

        for (const sf of [...this._activeStars]) {
            if (!sf.isActive) continue;
            const sp = sf.node.getWorldPosition();
            const halfStar = 32; // star 64x64 의 절반
            if (sp.x + halfStar > b.minX && sp.x - halfStar < b.maxX &&
                sp.y + halfStar > b.minY && sp.y - halfStar < b.maxY) {
                sf.catch();
            }
        }
    }

    private _onStarMissed(sf: StarFragment) {
        if (!sf.isDark) {
            GameManager.instance?.loseLife(1);
        }
        GameManager.instance?.resetCombo();
        this._returnToPool(sf);
    }

    private _onStarCaught(sf: StarFragment) {
        if (sf.isDark) {
            // SFX: Dark Star 수집음 (M-02)
            AudioManager.instance?.playDarkCatch();
            GameManager.instance?.loseLife(2);
            GameManager.instance?.resetCombo();
        } else {
            // SFX: 일반 별 수집음 (M-02)
            AudioManager.instance?.playCatch();
            GameManager.instance?.incrementCombo();
            const scoreVal = sf.score;
            GameManager.instance?.addScore(scoreVal);
            // 점수 팝업 연출 (V6-04)
            ScoreFloater.show(sf.node.getWorldPosition(), scoreVal);
            // 별자리 매니저에 전달
            if (this._constellationManager) {
                this._constellationManager.addStar(sf.color);
            }
        }
        this._returnToPool(sf);
    }

    // 별자리 매니저 연결 (GameScene에서 설정)
    private _constellationManager: ConstellationManager | null = null;
    setConstellationManager(cm: ConstellationManager | null) {
        this._constellationManager = cm;
    }

    // 전체 초기화 (씬 재시작 / 씬 언로드 시)
    clearAll() {
        for (const sf of [...this._activeStars]) {
            this._returnToPool(sf);
        }
        this._activeStars = [];
        this._timer = 0;
    }
}
