import { _decorator, Component } from 'cc';
import { EnemySpawner } from './EnemySpawner';
import { EnemyType } from './Enemy';
import { GameManager } from './GameManager';
const { ccclass, property } = _decorator;

interface Wave {
    count:      number;
    interval:   number;
    hpMult?:    number;
    speedMult?: number;
}

@ccclass('WaveManager')
export class WaveManager extends Component {
    @property waveDelay = 5;

    private readonly _waves: Wave[] = [
        { count:  5, interval: 1.5 },
        { count:  8, interval: 1.2, speedMult: 1.1 },
        { count: 12, interval: 1.0, hpMult: 1.5 },
        { count: 15, interval: 0.8, hpMult: 2.0, speedMult: 1.2 },
        { count: 20, interval: 0.6, hpMult: 2.5, speedMult: 1.3 },
    ];

    private _spawner:      EnemySpawner | null = null;
    private _waveIdx     = 0;
    private _spawnTimer  = 0;
    private _spawned     = 0;
    private _running     = false;
    private _bossSpawned = false;

    protected start(): void {
        this._spawner = this.getComponent(EnemySpawner);
        this.scheduleOnce(() => this._startWave(), 2);
    }

    private _waveForIdx(idx: number): Wave {
        if (idx < this._waves.length) return this._waves[idx];
        const extra = idx - this._waves.length + 1;
        return {
            count:     Math.min(20 + extra * 3, 60),
            interval:  Math.max(0.6 - extra * 0.02, 0.25),
            hpMult:    2.5 + extra * 0.4,
            speedMult: 1.3 + extra * 0.05,
        };
    }

    private _pickEnemyType(waveIdx: number): EnemyType {
        const r = Math.random();
        if (waveIdx < 2) {
            return EnemyType.Basic;
        } else if (waveIdx < 4) {
            return r < 0.70 ? EnemyType.Basic : EnemyType.Speed;
        } else if (waveIdx < 6) {
            if (r < 0.50) return EnemyType.Basic;
            if (r < 0.80) return EnemyType.Speed;
            return EnemyType.Tank;
        } else {
            const extra       = waveIdx - 6;
            const tankChance  = Math.min(0.40, 0.15 + extra * 0.03);
            const speedChance = Math.min(0.35, 0.20 + extra * 0.02);
            if (r < tankChance)               return EnemyType.Tank;
            if (r < tankChance + speedChance) return EnemyType.Speed;
            return EnemyType.Basic;
        }
    }

    private _startWave(): void {
        const gm = GameManager.instance;
        if (gm) gm.wave = this._waveIdx + 1;
        this._spawned     = 0;
        this._spawnTimer  = 0;
        this._running     = true;
        this._bossSpawned = false;
    }

    update(dt: number): void {
        const ts = GameManager.instance?.timeScale ?? 1;
        if (ts === 0 || !this._running) return;
        const edt = dt * ts;

        const w = this._waveForIdx(this._waveIdx);

        this._spawnTimer -= edt;
        if (this._spawnTimer <= 0 && this._spawned < w.count) {
            const type = this._pickEnemyType(this._waveIdx);
            this._spawner?.spawn(w.hpMult ?? 1, w.speedMult ?? 1, type);
            this._spawned++;
            this._spawnTimer = w.interval;
        }

        if (this._spawned >= w.count) {
            if (!this._bossSpawned && (this._waveIdx + 1) % 10 === 0) {
                this._bossSpawned = true;
                const bossHpMult    = 3 * (this._waveIdx + 1) / 10;
                const bossSpeedMult = 1 + this._waveIdx * 0.02;
                this.scheduleOnce(() => {
                    this._spawner?.spawnBoss(bossHpMult, bossSpeedMult);
                }, 1.0);
            }

            this._running = false;
            this._waveIdx++;
            this.scheduleOnce(() => this._startWave(), this.waveDelay);
        }
    }
}
