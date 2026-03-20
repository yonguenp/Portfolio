import { _decorator, Component, director } from 'cc';
const { ccclass } = _decorator;

export enum GameState {
    IDLE     = 'IDLE',
    PLAYING  = 'PLAYING',
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

    protected onLoad(): void {
        if (GameManager.instance) { this.destroy(); return; }
        GameManager.instance = this;
        director.addPersistRootNode(this.node);
    }

    protected onDestroy(): void {
        if (GameManager.instance === this) GameManager.instance = null;
    }

    reset(): void {
        this.gold  = 200;
        this.lives = 20;
        this.score = 0;
        this.wave  = 0;
        this.state = GameState.PLAYING;
    }

    spendGold(n: number): boolean {
        if (this.gold < n) return false;
        this.gold -= n;
        return true;
    }

    addGold(n: number): void  { this.gold  += n; }
    addScore(n: number): void { this.score += n; }

    loseLife(): void {
        this.lives--;
        if (this.lives <= 0) {
            this.state = GameState.GAMEOVER;
            director.loadScene('GameOver');
        }
    }
}
