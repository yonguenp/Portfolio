import { _decorator, Component } from 'cc';
import { EnemySpawner } from './EnemySpawner';
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

    private _spawner:     EnemySpawner | null = null;
    private _waveIdx    = 0;
    private _spawnTimer = 0;
    private _spawned    = 0;
    private _running    = false;

    protected start(): void {
        this._spawner = this.getComponent(EnemySpawner);
        this.scheduleOnce(() => this._startWave(), 2);
    }

    private _startWave(): void {
        if (this._waveIdx >= this._waves.length) return;
        const gm = GameManager.instance;
        if (gm) gm.wave = this._waveIdx + 1;
        this._spawned    = 0;
        this._spawnTimer = 0;
        this._running    = true;
    }

    update(dt: number): void {
        if (!this._running) return;
        const w = this._waves[this._waveIdx];
        if (!w) return;

        this._spawnTimer -= dt;
        if (this._spawnTimer <= 0 && this._spawned < w.count) {
            this._spawner?.spawn(w.hpMult ?? 1, w.speedMult ?? 1);
            this._spawned++;
            this._spawnTimer = w.interval;
        }

        if (this._spawned >= w.count) {
            this._running = false;
            this._waveIdx++;
            if (this._waveIdx < this._waves.length) {
                this.scheduleOnce(() => this._startWave(), this.waveDelay);
            }
        }
    }
}
