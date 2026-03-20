import { _decorator, Component, Label } from 'cc';
import { GameManager } from './GameManager';
const { ccclass, property } = _decorator;

@ccclass('UIManager')
export class UIManager extends Component {
    @property(Label) goldLabel:  Label | null = null;
    @property(Label) livesLabel: Label | null = null;
    @property(Label) waveLabel:  Label | null = null;
    @property(Label) scoreLabel: Label | null = null;

    update(): void {
        const gm = GameManager.instance;
        if (!gm) return;
        if (this.goldLabel)  this.goldLabel.string  = `Gold: ${gm.gold}`;
        if (this.livesLabel) this.livesLabel.string = `Lives: ${gm.lives}`;
        if (this.waveLabel)  this.waveLabel.string  = `Wave: ${gm.wave}`;
        if (this.scoreLabel) this.scoreLabel.string = `Score: ${gm.score}`;
    }
}
