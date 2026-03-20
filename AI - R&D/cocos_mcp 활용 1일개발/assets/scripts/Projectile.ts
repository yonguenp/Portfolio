import { _decorator, Component } from 'cc';
import { Enemy } from './Enemy';
const { ccclass, property } = _decorator;

@ccclass('Projectile')
export class Projectile extends Component {
    @property speed  = 350;
    @property damage = 30;

    private _target: Enemy | null = null;

    init(target: Enemy): void { this._target = target; }

    update(dt: number): void {
        const t = this._target;
        if (!t || t.isDead || !t.isValid) { this.node.destroy(); return; }
        const tp  = t.node.getWorldPosition();
        const mp  = this.node.getWorldPosition();
        const dx  = tp.x - mp.x;
        const dy  = tp.y - mp.y;
        const len = Math.sqrt(dx * dx + dy * dy);
        if (len < 12) {
            t.takeDamage(this.damage);
            this.node.destroy();
            return;
        }
        const s = this.speed * dt / len;
        this.node.setWorldPosition(mp.x + dx * s, mp.y + dy * s, 0);
    }
}
