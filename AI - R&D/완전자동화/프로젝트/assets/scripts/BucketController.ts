import { _decorator, Component, Node, Vec3, input, Input, EventTouch,
         UITransform, view, CCFloat } from 'cc';
import { GameManager, GameState } from './GameManager';
const { ccclass, property } = _decorator;

/**
 * BucketController - 플레이어 버킷 컨트롤러
 * - 터치/드래그 입력으로 좌우 이동
 * - 화면 경계 클램핑
 * - 별 조각과의 충돌 처리 (AABB)
 */
@ccclass('BucketController')
export class BucketController extends Component {

    @property({ type: CCFloat })
    halfWidth: number = 60;   // 버킷 충돌 반폭 (px)

    @property({ type: CCFloat })
    halfHeight: number = 30;  // 버킷 충돌 반높이 (px)

    private _screenHalfW: number = 480;

    onLoad() {
        const size = view.getVisibleSize();
        this._screenHalfW = size.width / 2;
    }

    onEnable() {
        input.on(Input.EventType.TOUCH_MOVE, this._onTouchMove, this);
    }

    onDisable() {
        input.off(Input.EventType.TOUCH_MOVE, this._onTouchMove, this);
    }

    private _onTouchMove(event: EventTouch) {
        if (GameManager.instance?.state !== GameState.PLAYING) return;

        const delta = event.getDeltaX();
        const pos = this.node.getPosition();
        const newX = this._clamp(pos.x + delta, -this._screenHalfW + this.halfWidth, this._screenHalfW - this.halfWidth);
        this.node.setPosition(newX, pos.y, pos.z);
    }

    private _clamp(val: number, min: number, max: number): number {
        return Math.max(min, Math.min(max, val));
    }

    /**
     * 버킷 AABB 경계 반환 (월드 좌표 기준)
     */
    getBounds(): { minX: number; maxX: number; minY: number; maxY: number } {
        const pos = this.node.getWorldPosition();
        return {
            minX: pos.x - this.halfWidth,
            maxX: pos.x + this.halfWidth,
            minY: pos.y - this.halfHeight,
            maxY: pos.y + this.halfHeight,
        };
    }
}
