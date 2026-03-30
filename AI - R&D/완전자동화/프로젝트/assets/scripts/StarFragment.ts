import { _decorator, Component, Node, Sprite, SpriteFrame,
         resources, CCFloat } from 'cc';
import { StarColor, STAR_SCORE, GameManager, GameState } from './GameManager';
const { ccclass, property } = _decorator;

export type StarMissCallback = (star: StarFragment) => void;
export type StarCatchCallback = (star: StarFragment) => void;

const STAR_RESOURCE_MAP: Record<string, string> = {
    RED:    'star_red',
    BLUE:   'star_blue',
    YELLOW: 'star_yellow',
    GREEN:  'star_green',
    PURPLE: 'star_purple',
    DARK:   'star_dark',
};

/**
 * StarFragment - 낙하하는 별 조각 단일 오브젝트
 * - 색상 타입 보유
 * - 매 프레임 fallSpeed 속도로 아래로 이동
 * - 화면 하단 이탈 시 onMiss 콜백 호출
 * - 버킷 충돌 판정은 StarSpawner 또는 BucketController 에서 수행
 */
@ccclass('StarFragment')
export class StarFragment extends Component {

    @property({ type: CCFloat })
    fallSpeed: number = 200;  // px/s (외부에서 Wave별로 설정됨)

    private _color: StarColor = StarColor.RED;
    private _active: boolean = false;
    private _bottomBound: number = -380; // 화면 하단 이탈 기준 Y

    public onMiss: StarMissCallback | null = null;
    public onCatch: StarCatchCallback | null = null;

    get color(): StarColor { return this._color; }
    get score(): number { return STAR_SCORE[this._color] ?? 0; }
    get isDark(): boolean { return this._color === StarColor.DARK; }
    get isActive(): boolean { return this._active; }

    private _sprite : Sprite = null;
    /**
     * 오브젝트 풀에서 꺼낼 때 초기화
     */
    init(color: StarColor, startX: number, startY: number, speed: number) {
        this._color = color;
        this.fallSpeed = speed;
        this._active = true;
        this.node.setPosition(startX, startY, 0);
        this.node.active = true;
        this._loadSprite(color);
    }

    private _loadSprite(color: StarColor) {
        const resName = STAR_RESOURCE_MAP[color];
        if (!resName) return;
        resources.load(`${resName}/spriteFrame`, SpriteFrame, (err, sf) => {
            if (err || !this.isValid) return;
            if(this._sprite == null)
            {
                this._sprite = this.node.addComponent(Sprite);
            }
            this._sprite.spriteFrame = sf;
        });
    }

    update(deltaTime: number) {
        if (!this._active) return;
        // GameState.PAUSED 시 낙하 이동 및 이탈 판정 정지 (M-04)
        // UI 애니메이션과 무관하게 게임 로직만 정지
        if (GameManager.instance?.state !== GameState.PLAYING) return;

        const pos = this.node.getPosition();
        const newY = pos.y - this.fallSpeed * deltaTime;
        this.node.setPosition(pos.x, newY, 0);

        if (newY < this._bottomBound) {
            this._active = false;
            this.onMiss?.(this);
        }
    }

    /**
     * 버킷에 잡혔을 때 외부에서 호출
     */
    catch() {
        if (!this._active) return;
        this._active = false;
        this.onCatch?.(this);
    }

    /**
     * 풀로 반환 시 초기화
     */
    reset() {
        this._active = false;
        this.node.active = false;
        this.onMiss = null;
        this.onCatch = null;
    }

    setBottomBound(y: number) {
        this._bottomBound = y;
    }
}
