import { _decorator, Component, director } from 'cc';
const { ccclass } = _decorator;

export enum GameState {
    IDLE     = 'IDLE',
    PLAYING  = 'PLAYING',
    PAUSED   = 'PAUSED',
    GAMEOVER = 'GAMEOVER',
}

@ccclass('GameManager')
export class GameManager extends Component {
    static instance: GameManager | null = null;

    gold  = 200;
    lives = 20;
    score = 0;
    wave  = 0;
    state: GameState = GameState.IDLE;
    speed     = 1;       // 1 or 2
    timeScale = 1;       // 0=paused, 1=normal, 2=2x  — read by all update() loops

    protected onLoad(): void {
        if (GameManager.instance) { this.destroy(); return; }
        GameManager.instance = this;
        director.addPersistRootNode(this.node);
    }

    protected onDestroy(): void {
        if (GameManager.instance === this) GameManager.instance = null;
    }

    reset(): void {
        this.gold      = 200;
        this.lives     = 20;
        this.score     = 0;
        this.wave      = 0;
        this.speed     = 1;
        this.timeScale = 1;
        this.state     = GameState.PLAYING;
    }

    spendGold(n: number): boolean {
        if (this.gold < n) return false;
        this.gold -= n;
        return true;
    }

    addGold(n: number): void  { this.gold  += n; }
    addScore(n: number): void { this.score += n; }

    pause(): void {
        if (this.state !== GameState.PLAYING) return;
        this.state     = GameState.PAUSED;
        this.timeScale = 0;
    }

    resume(): void {
        if (this.state !== GameState.PAUSED) return;
        this.state     = GameState.PLAYING;
        this.timeScale = this.speed;
    }

    setSpeed(s: number): void {
        this.speed = s;
        if (this.state === GameState.PLAYING) {
            this.timeScale = s;
        }
    }

    loseLife(): void {
        this.lives--;
        if (this.lives <= 0) {
            this.state     = GameState.GAMEOVER;
            this.timeScale = 1;
            director.loadScene('GameOver');
        }
    }
}
