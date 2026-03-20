import { _decorator, Component, Prefab, instantiate } from 'cc';
import { Enemy } from './Enemy';
import { Projectile } from './Projectile';
const { ccclass, property } = _decorator;

@ccclass('Tower')
export class Tower extends Component {
    @property range    = 160;
    @property fireRate = 1;   // shots per second

    @property(Prefab) projectilePrefab: Prefab | null = null;

    private _cooldown = 0;

    update(dt: number): void {
        this._cooldown -= dt;
        if (this._cooldown > 0) return;

        const target = this._findTarget();
        if (!target) return;

        this._shoot(target);
        this._cooldown = 1 / this.fireRate;
    }

    private _findTarget(): Enemy | null {
        const pos = this.node.getWorldPosition();
        for (const e of Enemy.activeEnemies) {
            if (!e || e.isDead || !e.isValid) continue;
            const ep = e.node.getWorldPosition();
            const dx = ep.x - pos.x;
            const dy = ep.y - pos.y;
            if (Math.sqrt(dx * dx + dy * dy) <= this.range) return e;
        }
        return null;
    }

    private _shoot(target: Enemy): void {
        if (!this.projectilePrefab) return;
        const node = instantiate(this.projectilePrefab);
        this.node.parent?.addChild(node);
        node.setWorldPosition(this.node.getWorldPosition());
        node.getComponent(Projectile)?.init(target);
    }
}
