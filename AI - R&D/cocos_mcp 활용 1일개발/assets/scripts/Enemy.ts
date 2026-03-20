import { _decorator, Component } from 'cc';
import { GameManager } from './GameManager';
import {
    grid, SPAWN_CELL, EXIT_CELL,
    gridToCanvas, canvasToGrid,
    findPath, TILE,
} from './PathFinder';
const { ccclass, property } = _decorator;

@ccclass('Enemy')
export class Enemy extends Component {
    static activeEnemies: Enemy[] = [];

    @property maxHp       = 100;
    @property moveSpeed   = 100;
    @property goldReward  = 20;
    @property scoreReward = 10;

    private _hp          = 100;
    private _wpIdx       = 0;
    private _dead        = false;
    private _path: Array<{ x: number; y: number }> = [];
    private _gridVer     = -1; // last grid version when path was calculated

    protected onEnable(): void  { Enemy.activeEnemies.push(this); }
    protected onDisable(): void {
        const i = Enemy.activeEnemies.indexOf(this);
        if (i >= 0) Enemy.activeEnemies.splice(i, 1);
    }

    init(hpMult = 1, speedMult = 1): void {
        this._hp       = this.maxHp * hpMult;
        this.moveSpeed = this.moveSpeed * speedMult;
        this._dead     = false;

        const spawn = gridToCanvas(SPAWN_CELL.col, SPAWN_CELL.row);
        this.node.setPosition(spawn.x, spawn.y, 0);
        this._recalcPath();
    }

    private _recalcPath(): void {
        const pos         = this.node.getPosition();
        const { col, row } = canvasToGrid(pos.x, pos.y);
        const newPath     = findPath(col, row, grid.blocked);

        if (newPath && newPath.length > 0) {
            this._path   = newPath;
            this._wpIdx  = 0;
            // Skip first waypoint if already close to it
            if (newPath.length > 1) {
                const wp0 = newPath[0];
                const dx  = wp0.x - pos.x;
                const dy  = wp0.y - pos.y;
                if (dx * dx + dy * dy < (TILE * 0.5) ** 2) {
                    this._wpIdx = 1;
                }
            }
        }
        this._gridVer = grid.version;
    }

    takeDamage(dmg: number): void {
        if (this._dead) return;
        this._hp -= dmg;
        if (this._hp <= 0) this._die();
    }

    private _die(): void {
        if (this._dead) return;
        this._dead = true;
        GameManager.instance?.addGold(this.goldReward);
        GameManager.instance?.addScore(this.scoreReward);
        this.node.destroy();
    }

    update(dt: number): void {
        if (this._dead) return;

        // Recalculate path whenever grid changes (tower placed)
        if (this._gridVer !== grid.version) {
            this._recalcPath();
        }

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
        const spd = this.moveSpeed * dt / len;
        this.node.setPosition(pos.x + dx * spd, pos.y + dy * spd, 0);
    }

    get isDead(): boolean { return this._dead; }
}
