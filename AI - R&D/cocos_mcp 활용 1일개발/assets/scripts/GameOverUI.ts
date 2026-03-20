import { _decorator, Component, Label, director, Button } from 'cc';
import { GameManager } from './GameManager';
const { ccclass, property } = _decorator;

@ccclass('GameOverUI')
export class GameOverUI extends Component {
    @property(Label) finalScoreLabel: Label | null = null;

    protected start(): void {
        // Show score
        const gm = GameManager.instance;
        if (this.finalScoreLabel && gm) {
            this.finalScoreLabel.string = `Final Score: ${gm.score}`;
        }

        // Register buttons programmatically
        const canvas     = this.node.parent;
        const restartBtn = canvas?.getChildByName('RestartBtn');
        const menuBtn    = canvas?.getChildByName('MainMenuBtn');

        if (restartBtn) {
            restartBtn.on(Button.EventType.CLICK, this._onRestartClick, this);
            console.log('[GameOverUI] RestartBtn registered');
        }
        if (menuBtn) {
            menuBtn.on(Button.EventType.CLICK, this._onMainMenuClick, this);
            console.log('[GameOverUI] MainMenuBtn registered');
        }
    }

    private _onRestartClick(): void {
        GameManager.instance?.reset();
        director.loadScene('GameScene');
    }

    private _onMainMenuClick(): void {
        director.loadScene('MainMenu');
    }

    // Keep for scene-based clickEvents
    onRestartClick(): void  { this._onRestartClick(); }
    onMainMenuClick(): void { this._onMainMenuClick(); }
}
