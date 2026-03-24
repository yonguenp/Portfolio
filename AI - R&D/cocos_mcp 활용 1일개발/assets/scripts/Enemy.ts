import { _decorator, Component, Sprite, UITransform, Color, Label, Node, Graphics } from 'cc';
import { GameManager } from './GameManager';
import {
    grid, SPAWN_CELL,
    gridToCanvas, canvasToGrid,
    findPath, TILE,
} from './PathFinder';
const { ccclass, property } = _decorator;

// ── Enemy type definitions ────────────────────────────────────────────────────

export enum EnemyType {
    Basic = 0,
    Speed = 1,
    Tank  = 2,
}

const ENEMY_TYPE_CFG = {
    [EnemyType.Basic]: { hpMult: 1.0, speedMult: 1.0, r: 220, g:  80, b:  80, scale: 1.0  },
    [EnemyType.Speed]: { hpMult: 0.5, speedMult: 1.5, r:  80, g: 220, b: 220, scale: 0.75 },
    [EnemyType.Tank]:  { hpMult: 1.5, speedMult: 0.5, r: 180, g: 100, b:  30, scale: 1.3  },
};

@ccclass('Enemy')
export class Enemy extends Component {
    static activeEnemies: Enemy[] = [];

    @property maxHp       = 100;
    @property moveSpeed   = 100;
    @property goldReward  = 20;
    @property scoreReward = 10;

    private _hp          = 100;
    private _maxHpValue  = 100;
    private _wpIdx       = 0;
    private _dead        = false;
    private _path: Array<{ x: number; y: number }> = [];
    private _gridVer     = -1;

    private _slowFactor  = 1.0;
    private _slowTimer   = 0;
    private _isBoss      = false;

    private _baseR = 220;
    private _baseG =  80;
    private _baseB =  80;

    private _flashing    = false;
    private _flashTimer  = 0;
    private static readonly FLASH_DUR = 0.08;

    private _hpBarFill: Node | null = null;

    protected onEnable(): void  { Enemy.activeEnemies.push(this); }
    protected onDisable(): void {
        const i = Enemy.activeEnemies.indexOf(this);
        if (i >= 0) Enemy.activeEnemies.splice(i, 1);
    }

    // ── Init ──────────────────────────────────────────────────────────────────────

    init(hpMult = 1, speedMult = 1, isBoss = false, enemyType = EnemyType.Basic): void {
        const typeCfg = ENEMY_TYPE_CFG[enemyType];

        this._maxHpValue = this.maxHp    * hpMult    * typeCfg.hpMult;
        this._hp         = this._maxHpValue;
        this.moveSpeed   = this.moveSpeed * speedMult * typeCfg.speedMult;
        this._dead       = false;
        this._slowFactor = 1.0;
        this._slowTimer  = 0;
        this._isBoss     = isBoss;
        this._flashing   = false;
        this._flashTimer = 0;
        this._hpBarFill  = null;

        const spawn = gridToCanvas(SPAWN_CELL.col, SPAWN_CELL.row);
        this.node.setPosition(spawn.x, spawn.y, 0);
        this._recalcPath();

        if (isBoss) {
            this._baseR = 100; this._baseG = 0; this._baseB = 150;
            this.node.setScale(2, 2, 1);
            const sprite = this.node.getComponent(Sprite);
            if (sprite) sprite.color = new Color(100, 0, 150, 255);

            // BOSS label
            const bossLblNode = new Node('BossLbl');
            bossLblNode.layer = this.node.layer;
            bossLblNode.addComponent(UITransform).setContentSize(80, 22);
            const lbl = bossLblNode.addComponent(Label);
            lbl.string   = 'BOSS';
            lbl.fontSize = 14;
            lbl.color    = new Color(255, 50, 50, 255);
            bossLblNode.setPosition(0, 28, 0);
            bossLblNode.setScale(0.5, 0.5, 1);
            bossLblNode.setParent(this.node);

            this._createHPBar();
        } else {
            this._baseR = typeCfg.r;
            this._baseG = typeCfg.g;
            this._baseB = typeCfg.b;
            this.node.setScale(typeCfg.scale, typeCfg.scale, 1);
            const sprite = this.node.getComponent(Sprite);
            if (sprite) sprite.color = new Color(typeCfg.r, typeCfg.g, typeCfg.b, 255);

            if (enemyType !== EnemyType.Basic) {
                const lblNode = new Node('TypeLbl');
                lblNode.layer = this.node.layer;
                lblNode.addComponent(UITransform).setContentSize(60, 16);
                const typeLbl = lblNode.addComponent(Label);
                typeLbl.string   = enemyType === EnemyType.Speed ? 'SPD' : 'TNK';
                typeLbl.fontSize = 11;
                typeLbl.color    = new Color(255, 255, 255, 200);
                lblNode.setPosition(0, 22, 0);
                lblNode.setScale(1 / typeCfg.scale, 1 / typeCfg.scale, 1);
                lblNode.setParent(this.node);
            }
        }
    }

    // ── Boss HP bar ───────────────────────────────────────────────────────────────

    private _createHPBar(): void {
        const barW = 120, barH = 10;

        const container = new Node('HPBarContainer');
        container.layer = this.node.layer;
        container.addComponent(UITransform).setContentSize(barW, barH);
        container.setPosition(0, 46, 0);
        container.setScale(0.5, 0.5, 1);   // counter-scale boss 2×

        const bg = new Node('BG');
        bg.layer = this.node.layer;
        bg.addComponent(UITransform).setContentSize(barW, barH);
        const bgG = bg.addComponent(Graphics);
        bgG.fillColor = new Color(40, 0, 0, 220);
        bgG.rect(-barW / 2, -barH / 2, barW, barH);
        bgG.fill();
        bg.setParent(container);

        const fill = new Node('Fill');
        fill.layer = this.node.layer;
        fill.addComponent(UITransform).setContentSize(barW, barH);
        this._hpBarFill = fill;
        fill.setParent(container);

        container.setParent(this.node);
        this._updateHPBar();
    }

    private _updateHPBar(): void {
        if (!this._hpBarFill || !this._hpBarFill.isValid) return;
        const ratio = Math.max(0, this._hp / this._maxHpValue);
        const barW  = 120, barH = 10;
        const fillW = barW * ratio;

        let g = this._hpBarFill.getComponent(Graphics);
        if (!g) g = this._hpBarFill.addComponent(Graphics);
        g.clear();
        // Color: green → yellow → red as HP drops
        const r = ratio > 0.5 ? Math.floor(255 * (1 - ratio) * 2) : 255;
        const gn = ratio > 0.5 ? 200 : Math.floor(200 * ratio * 2);
        g.fillColor = new Color(r, gn, 0, 255);
        g.rect(-barW / 2, -barH / 2, fillW, barH);
        g.fill();
    }

    // ── Slow & color ──────────────────────────────────────────────────────────────

    applySlow(toFactor: number, duration: number): void {
        const wasSlowed = this._slowTimer > 0;
        this._slowFactor = Math.min(this._slowFactor, toFactor);
        this._slowTimer  = Math.max(this._slowTimer, duration);
        if (!wasSlowed && !this._flashing) {
            this._applyColor();
        }
    }

    /** Set sprite color based on current state (slow / normal), skip during flash */
    private _applyColor(): void {
        if (this._isBoss) return;
        const sprite = this.node.getComponent(Sprite);
        if (!sprite) return;
        if (this._slowTimer > 0) {
            // Blue-tinted — set once when slow starts, cleared when slow ends
            sprite.color = new Color(
                Math.floor(this._baseR * 0.4),
                Math.floor(Math.min(255, this._baseG * 0.4 + 100)),
                Math.min(255, Math.floor(this._baseB * 0.4 + 160)),
                255,
            );
        } else {
            sprite.color = new Color(this._baseR, this._baseG, this._baseB, 255);
        }
    }

    // ── Damage ────────────────────────────────────────────────────────────────────

    takeDamage(dmg: number): void {
        if (this._dead) return;
        this._hp -= dmg;
        // White flash
        const sprite = this.node.getComponent(Sprite);
        if (sprite) sprite.color = new Color(255, 255, 255, 255);
        this._flashing   = true;
        this._flashTimer = Enemy.FLASH_DUR;
        this._updateHPBar();
        if (this._hp <= 0) this._die();
    }

    private _die(): void {
        if (this._dead) return;
        this._dead = true;
        GameManager.instance?.addGold(this.goldReward);
        GameManager.instance?.addScore(this.scoreReward);
        this.node.destroy();
    }

    // ── Pathfinding ───────────────────────────────────────────────────────────────

    get waypointProgress(): number { return this._wpIdx; }

    private _recalcPath(): void {
        const pos          = this.node.getPosition();
        const { col, row } = canvasToGrid(pos.x, pos.y);
        const newPath      = findPath(col, row, grid.blocked);
        if (newPath && newPath.length > 0) {
            this._path  = newPath;
            this._wpIdx = 0;
            if (newPath.length > 1) {
                const wp0 = newPath[0];
                const dx  = wp0.x - pos.x;
                const dy  = wp0.y - pos.y;
                if (dx * dx + dy * dy < (TILE * 0.5) ** 2) this._wpIdx = 1;
            }
        }
        this._gridVer = grid.version;
    }

    // ── Update ────────────────────────────────────────────────────────────────────

    update(dt: number): void {
        if (this._dead) return;
        const ts = GameManager.instance?.timeScale ?? 1;
        if (ts === 0) return;
        const edt = dt * ts;

        // Hit flash
        if (this._flashing) {
            this._flashTimer -= edt;
            if (this._flashTimer <= 0) {
                this._flashing = false;
                this._applyColor();
            }
        }

        // Slow timer
        if (this._slowTimer > 0) {
            this._slowTimer -= edt;
            if (this._slowTimer <= 0) {
                this._slowTimer  = 0;
                this._slowFactor = 1.0;
                if (!this._flashing) this._applyColor();
            }
        }

        if (this._gridVer !== grid.version) this._recalcPath();

        if (this._wpIdx >= this._path.length) {
            GameManager.instance?.loseLife();
            this._dead = true;
            this.node.destroy();
            return;
        }

        const wp  = this._path[this._wpIdx];
        const pos = this.node.getPosition();
        const dx  = wp.x - pos.x;
        const dy  = wp.y - pos.y;
        const len = Math.sqrt(dx * dx + dy * dy);
        if (len < 5) { this._wpIdx++; return; }

        const spd = this.moveSpeed * this._slowFactor * edt / len;
        this.node.setPosition(pos.x + dx * spd, pos.y + dy * spd, 0);
    }

    get isDead(): boolean { return this._dead; }
}
