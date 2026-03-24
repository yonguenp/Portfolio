import { _decorator, Component, Sprite, Color } from 'cc';
import { Enemy } from './Enemy';
import { GameManager } from './GameManager';
const { ccclass } = _decorator;

@ccclass('Projectile')
export class Projectile extends Component {
    private _targetEnemy: Enemy | null = null;
    private _targetX     = 0;
    private _targetY     = 0;
    private _speed       = 700;
    private _damage      = 0;
    private _splashRadius = 0;
    private _slowFactor  = 0;
    private _slowDuration = 0;

    // Arc (splash tower)
    private _useArc      = false;
    private _startX      = 0;
    private _startY      = 0;
    private _arcDuration = 0;
    private _arcElapsed  = 0;
    private _arcHeight   = 0;

    init(
        target: Enemy,
        damage: number,
        splashRadius = 0,
        slowFactor   = 0,
        slowDuration = 0,
        useArc       = false,
    ): void {
        this._targetEnemy  = target;
        this._damage       = damage;
        this._splashRadius = splashRadius;
        this._slowFactor   = slowFactor;
        this._slowDuration = slowDuration;
        this._useArc       = useArc;

        // Capture target world position at fire time
        const tp = target.node.getWorldPosition();
        this._targetX = tp.x;
        this._targetY = tp.y;

        if (useArc) {
            const sp = this.node.getWorldPosition();
            this._startX = sp.x;
            this._startY = sp.y;
            const dist = Math.sqrt(
                (this._targetX - sp.x) ** 2 + (this._targetY - sp.y) ** 2,
            );
            this._arcDuration = Math.max(0.3, dist / this._speed);
            this._arcElapsed  = 0;
            this._arcHeight   = Math.min(160, dist * 0.45);
        }
    }

    update(dt: number): void {
        const ts = GameManager.instance?.timeScale ?? 1;
        if (ts === 0) return;
        const edt = dt * ts;

        if (this._useArc) {
            this._updateArc(edt);
        } else {
            this._updateStraight(edt);
        }
    }

    private _updateStraight(edt: number): void {
        // Non-splash: abort if target already dead
        if (this._splashRadius === 0) {
            const t = this._targetEnemy;
            if (!t || !t.isValid || t.isDead) { this.node.destroy(); return; }
        }

        const mp  = this.node.getWorldPosition();
        const dx  = this._targetX - mp.x;
        const dy  = this._targetY - mp.y;
        const len = Math.sqrt(dx * dx + dy * dy);

        if (len < 10) {
            this._onHit();
            this.node.destroy();
            return;
        }

        const s = this._speed * edt / len;
        this.node.setWorldPosition(mp.x + dx * s, mp.y + dy * s, 0);
    }

    private _updateArc(edt: number): void {
        this._arcElapsed += edt;
        const t  = Math.min(this._arcElapsed / this._arcDuration, 1);
        const ix = this._startX + (this._targetX - this._startX) * t;
        const iy = this._startY + (this._targetY - this._startY) * t;
        const arc = this._arcHeight * 4 * t * (1 - t);   // parabola: 0 at both ends
        this.node.setWorldPosition(ix, iy + arc, 0);

        if (t >= 1) {
            this._onHit();
            this.node.destroy();
        }
    }

    private _onHit(): void {
        if (this._splashRadius > 0) {
            for (const e of Enemy.activeEnemies.slice()) {
                if (!e || !e.isValid || e.isDead) continue;
                const ep = e.node.getWorldPosition();
                const dx = ep.x - this._targetX;
                const dy = ep.y - this._targetY;
                if (Math.sqrt(dx * dx + dy * dy) <= this._splashRadius) {
                    if (this._damage > 0)     e.takeDamage(this._damage);
                    if (this._slowFactor > 0) e.applySlow(this._slowFactor, this._slowDuration);
                }
            }
        } else {
            const t = this._targetEnemy;
            if (t && t.isValid && !t.isDead) {
                if (this._damage > 0)     t.takeDamage(this._damage);
                if (this._slowFactor > 0) t.applySlow(this._slowFactor, this._slowDuration);
            }
        }
    }
}
