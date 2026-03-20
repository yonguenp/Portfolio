import { _decorator, Component, director, Button } from 'cc';
import { GameManager } from './GameManager';
const { ccclass } = _decorator;

@ccclass('MainMenuUI')
export class MainMenuUI extends Component {
    protected start(): void {
        // Register button click programmatically — most reliable approach
        const canvas   = this.node.parent;
        const startBtn = canvas?.getChildByName('StartBtn');
        if (startBtn) {
            startBtn.on(Button.EventType.CLICK, this._onStartClick, this);
            console.log('[MainMenuUI] StartBtn click registered');
        } else {
            console.warn('[MainMenuUI] StartBtn not found!');
        }
    }

    private _onStartClick(): void {
        GameManager.instance?.reset();
        director.loadScene('GameScene');
    }

    // Keep this for any scene-based clickEvent that may exist
    onStartClick(): void {
        this._onStartClick();
    }
}
