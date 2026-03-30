import { _decorator, Component, Node, Label, tween, UIOpacity } from 'cc';
import { GameManager } from './GameManager';
const { ccclass, property } = _decorator;

/**
 * WaveManager - Wave 전환 연출 및 보스 웨이브 경고 처리
 * - Wave 팝업 텍스트 표시 (0.5초 딜레이 후 사라짐)
 * - 보스 웨이브 경고 연출
 * - GameScene / GameManager와 협력
 */
@ccclass('WaveManager')
export class WaveManager extends Component {

    @property({ type: Label })
    waveAnnouncementLabel: Label | null = null;

    @property({ type: Label })
    bossWarningLabel: Label | null = null;

    @property({ type: Node })
    bossWarningPanel: Node | null = null;

    onLoad() {
        if (this.bossWarningPanel) this.bossWarningPanel.active = false;
    }

    /**
     * GameScene.start() 에서 호출하거나 GameManager.onWaveChanged 콜백으로 연결
     */
    announceWave(wave: number) {
        const cfg = GameManager.instance?.getCurrentWaveConfig();
        const isBoss = cfg?.isBossWave ?? false;

        if (isBoss) {
            this._showBossWarning(wave);
        } else {
            this._showWaveAnnouncement(wave);
        }
    }

    private _showWaveAnnouncement(wave: number) {
        if (!this.waveAnnouncementLabel) return;
        const label = this.waveAnnouncementLabel;
        label.string = `Wave ${wave}`;
        label.node.active = true;

        const opacity = label.node.getComponent(UIOpacity) ?? label.node.addComponent(UIOpacity);
        opacity.opacity = 255;

        tween(opacity)
            .delay(1.0)
            .to(0.5, { opacity: 0 })
            .call(() => { label.node.active = false; })
            .start();
    }

    private _showBossWarning(wave: number) {
        if (this.bossWarningPanel) {
            this.bossWarningPanel.active = true;
            if (this.bossWarningLabel) {
                this.bossWarningLabel.string = `⚠ BOSS WAVE ${wave} ⚠\nDark Stars Incoming!`;
            }

            const opacity = this.bossWarningPanel.getComponent(UIOpacity) ?? this.bossWarningPanel.addComponent(UIOpacity);
            opacity.opacity = 255;

            tween(opacity)
                .delay(1.5)
                .to(0.5, { opacity: 0 })
                .call(() => {
                    if (this.bossWarningPanel) this.bossWarningPanel.active = false;
                })
                .start();
        }
    }
}
