import {
    _decorator, Component, Prefab, instantiate,
    Sprite, UITransform, Color, Label, Node, Vec3, Graphics,
} from 'cc';
import { TowerType, TowerConfig, TowerStats, TOWER_CONFIGS, getStats } from './TowerData';
import { Enemy } from './Enemy';
import { Projectile } from './Projectile';
import { GameManager } from './GameManager';

const { ccclass, property } = _decorator;

// Towers that deal damage instantly (no visible projectile)
const DIRECT_HIT_TYPES = new Set([TowerType.Normal, TowerType.Rapid, TowerType.Sniper]);

@ccclass('Tower')
export class Tower extends Component {
    @property(Prefab) projectilePrefab: Prefab | null = null;

    towerType: TowerType = TowerType.Normal;
    level: number = 1;

    private _stats: TowerStats = {
        damage: 30, fireRate: 1.0, range: 160,
        splashRadius: 0, slowFactor: 0, slowDuration: 0,
    };
    private _cooldown    = 0;
    private _levelLabel: Label | null = null;
    private _barrel:     Node  | null = null;
    private _rangeCircle: Node | null = null;

    // ── Public API ────────────────────────────────────────────────────────────────

    init(type: TowerType): void {
        this.towerType = type;
        this.level     = 1;
        this._refresh();
        this._updateVisual();
        this._createBarrel();
    }

    upgrade(): boolean {
        if (this.level >= 10) return false;
        this.level++;
        this._refresh();
        this._updateVisual();
        return true;
    }

    showRangeCircle(): void {
        this.hideRangeCircle();
        const n = new Node('RangeCircle');
        n.layer = this.node.layer;
        n.addComponent(UITransform).setContentSize(
            this._stats.range * 2,
            this._stats.range * 2,
        );
        const g = n.addComponent(Graphics);
        const [r, gb, b] = TOWER_CONFIGS[this.towerType].color;
        g.strokeColor = new Color(r, gb, b, 120);
        g.lineWidth   = 2;
        g.circle(0, 0, this._stats.range);
        g.stroke();
        g.fillColor = new Color(r, gb, b, 20);
        g.circle(0, 0, this._stats.range);
        g.fill();
        n.setParent(this.node);
        this._rangeCircle = n;
    }

    hideRangeCircle(): void {
        this._rangeCircle?.destroy();
        this._rangeCircle = null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────────

    private _refresh(): void {
        this._stats = getStats(this.towerType, this.level);
    }

    private _updateVisual(): void {
        const cfg = TOWER_CONFIGS[this.towerType];
        const [r, g, b] = cfg.color;

        const sprite = this.node.getComponent(Sprite);
        if (sprite) sprite.color = new Color(r, g, b, 255);

        const tr = this.node.getComponent(UITransform);
        if (tr) tr.setContentSize(cfg.size, cfg.size);

        if (!this._levelLabel) {
            const labelNode = new Node('LevelLabel');
            labelNode.layer = this.node.layer;
            labelNode.addComponent(UITransform).setContentSize(60, 20);
            const lbl = labelNode.addComponent(Label);
            lbl.fontSize = 14;
            lbl.color    = new Color(255, 255, 255, 255);
            this._levelLabel = lbl;
            labelNode.setPosition(new Vec3(0, cfg.size * 0.5 + 10, 0));
            labelNode.setParent(this.node);
        } else {
            this._levelLabel.node.setPosition(new Vec3(0, cfg.size * 0.5 + 10, 0));
        }
        this._levelLabel.string = `Lv.${this.level}`;
    }

    /** Rotating barrel pointing right by default */
    private _createBarrel(): void {
        if (this._barrel) { this._barrel.destroy(); }

        const cfg    = TOWER_CONFIGS[this.towerType];
        const bLen   = cfg.size * 0.65;
        const bWidth = 6;

        const n = new Node('Barrel');
        n.layer = this.node.layer;
        n.addComponent(UITransform).setContentSize(bLen, bWidth);

        const g = n.addComponent(Graphics);
        const [r, gb, b] = cfg.color;
        g.fillColor = new Color(
            Math.max(0, r - 50),
            Math.max(0, gb - 50),
            Math.max(0, b - 50),
            255,
        );
        // Barrel starts at origin, extends rightward
        g.rect(0, -bWidth / 2, bLen, bWidth);
        g.fill();

        // Darker end cap
        g.fillColor = new Color(
            Math.max(0, r - 80),
            Math.max(0, gb - 80),
            Math.max(0, b - 80),
            255,
        );
        g.rect(bLen - 4, -bWidth / 2, 4, bWidth);
        g.fill();

        n.setPosition(Vec3.ZERO);
        n.setParent(this.node);
        this._barrel = n;
    }

    private _findTarget(): Enemy | null {
        const pos = this.node.getWorldPosition();
        let best: Enemy | null = null;
        let bestProgress = -1;

        for (const e of Enemy.activeEnemies) {
            if (!e || !e.isValid || e.isDead) continue;
            const ep = e.node.getWorldPosition();
            const dx = ep.x - pos.x;
            const dy = ep.y - pos.y;
            if (Math.sqrt(dx * dx + dy * dy) <= this._stats.range) {
                const p = e.waypointProgress;
                if (p > bestProgress) { bestProgress = p; best = e; }
            }
        }
        return best;
    }

    private _shoot(target: Enemy): void {
        const cfg = TOWER_CONFIGS[this.towerType];
        const [r, gb, b] = cfg.color;

        if (DIRECT_HIT_TYPES.has(this.towerType)) {
            // ── Instant hit: apply damage + hit flash at enemy position ──────────
            if (this._stats.damage > 0) target.takeDamage(this._stats.damage);
            if (this._stats.slowFactor > 0) target.applySlow(this._stats.slowFactor, this._stats.slowDuration);
            this._spawnHitFlash(target, r, gb, b);
            return;
        }

        // ── Projectile-based towers ───────────────────────────────────────────────
        if (!this.projectilePrefab) return;
        const node = instantiate(this.projectilePrefab);
        this.node.parent?.addChild(node);
        node.setWorldPosition(this.node.getWorldPosition());

        const proj = node.getComponent(Projectile);
        if (proj) {
            proj.init(
                target,
                this._stats.damage,
                this._stats.splashRadius,
                this._stats.slowFactor,
                this._stats.slowDuration,
                this.towerType === TowerType.Splash,   // arc for splash
            );
        }

        const sprite = node.getComponent(Sprite);
        if (sprite) sprite.color = new Color(r, gb, b, 255);
    }

    /** Tiny colored circle flash at the enemy's position */
    private _spawnHitFlash(target: Enemy, r: number, gb: number, b: number): void {
        const flash = new Node('HitFlash');
        flash.layer = this.node.layer;
        flash.addComponent(UITransform).setContentSize(20, 20);
        flash.setWorldPosition(target.node.getWorldPosition());

        const g = flash.addComponent(Graphics);
        g.fillColor = new Color(r, gb, b, 200);
        g.circle(0, 0, 9);
        g.fill();
        g.strokeColor = new Color(255, 255, 255, 180);
        g.lineWidth   = 2;
        g.circle(0, 0, 9);
        g.stroke();

        this.node.parent?.addChild(flash);
        this.scheduleOnce(() => { if (flash.isValid) flash.destroy(); }, 0.07);
    }

    // ── Update ────────────────────────────────────────────────────────────────────

    update(dt: number): void {
        const ts = GameManager.instance?.timeScale ?? 1;
        if (ts === 0) return;
        const edt = dt * ts;

        this._cooldown -= edt;

        const target = this._findTarget();

        // Aim barrel at target (or keep last angle if no target)
        if (target && this._barrel) {
            const tp = target.node.getWorldPosition();
            const mp = this.node.getWorldPosition();
            const angle = Math.atan2(tp.y - mp.y, tp.x - mp.x) * (180 / Math.PI);
            this._barrel.angle = angle;
        }

        if (this._cooldown > 0 || !target) return;
        this._shoot(target);
        this._cooldown = 1 / this._stats.fireRate;
    }
}
