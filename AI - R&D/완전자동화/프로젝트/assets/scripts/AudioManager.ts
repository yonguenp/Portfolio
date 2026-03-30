import { _decorator, Component, AudioClip, AudioSource, director,
         resources } from 'cc';
const { ccclass, property } = _decorator;

/**
 * AudioManager - BGM / SFX 재생 관리 싱글톤
 * - BGM: 루프 재생 (TitleScene용, GameScene용)
 * - SFX: 별 수집음, 별자리 완성음, 라이프 감소음, 게임 오버음
 */
@ccclass('AudioManager')
export class AudioManager extends Component {

    private static _instance: AudioManager | null = null;
    public static get instance(): AudioManager | null {
        if (!AudioManager._instance) {
            console.warn('[AudioManager] instance not yet initialized (audio muted)');
            return null;
        }
        return AudioManager._instance;
    }

    @property({ type: AudioSource })
    bgmSource: AudioSource | null = null;

    @property({ type: AudioSource })
    sfxSource: AudioSource | null = null;

    // BGM 클립
    @property({ type: AudioClip })
    bgmTitle: AudioClip | null = null;

    @property({ type: AudioClip })
    bgmGame: AudioClip | null = null;

    // SFX 클립
    @property({ type: AudioClip })
    sfxCatch: AudioClip | null = null;

    @property({ type: AudioClip })
    sfxDarkCatch: AudioClip | null = null;

    @property({ type: AudioClip })
    sfxConstellation: AudioClip | null = null;

    @property({ type: AudioClip })
    sfxLoseLife: AudioClip | null = null;

    @property({ type: AudioClip })
    sfxWaveClear: AudioClip | null = null;

    @property({ type: AudioClip })
    sfxGameOver: AudioClip | null = null;

    @property({ type: AudioClip })
    sfxCombo: AudioClip | null = null;

    /** 별자리 도감 신규 등록 SFX — 밝고 짧은 "띵동" 효과음 (spec_v3 NEW-02) */
    @property({ type: AudioClip })
    sfxBookUnlock: AudioClip | null = null;

    /** Wave 진행도 100% 완료 SFX — 진행도 바 채움 완료 시 재생 (spec_v4 신규) */
    @property({ type: AudioClip })
    sfxProgressComplete: AudioClip | null = null;

    /** 은하의 심연 최초 완성 팡파레 SFX (NEW-04) */
    @property({ type: AudioClip })
    sfxGalaxyFanfare: AudioClip | null = null;

    onLoad() {
        if (AudioManager._instance && AudioManager._instance !== this) {
            this.node.destroy();
            return;
        }
        AudioManager._instance = this;
        director.addPersistRootNode(this.node);
    }

    onDestroy() {
        if (AudioManager._instance === this) AudioManager._instance = null;
    }

    // ===== BGM =====

    playBGMTitle() {
        this._playBGM(this.bgmTitle);
    }

    playBGMGame() {
        this._playBGM(this.bgmGame);
    }

    private _playBGM(clip: AudioClip | null) {
        if (!this.bgmSource || !clip) return;
        if (this.bgmSource.clip === clip && this.bgmSource.playing) return;
        this.bgmSource.clip = clip;
        this.bgmSource.loop = true;
        this.bgmSource.play();
    }

    stopBGM() {
        this.bgmSource?.stop();
    }

    setBGMVolume(vol: number) {
        if (this.bgmSource) this.bgmSource.volume = vol;
    }

    // ===== SFX =====

    playCatch()         { this._playSFX(this.sfxCatch); }
    playDarkCatch()     { this._playSFX(this.sfxDarkCatch); }
    playConstellation() { this._playSFX(this.sfxConstellation); }
    playLoseLife()      { this._playSFX(this.sfxLoseLife); }
    playWaveClear()     { this._playSFX(this.sfxWaveClear); }
    playGameOver()      { this._playSFX(this.sfxGameOver); }
    playCombo()         { this._playSFX(this.sfxCombo); }
    /** 별자리 도감 신규 등록 시 "띵동" SFX (spec_v3 NEW-02) */
    playBookUnlock()    { this._playSFX(this.sfxBookUnlock); }
    /** Wave 진행도 100% 완료 SFX (spec_v4 신규) */
    playProgressComplete() { this._playSFX(this.sfxProgressComplete); }
    /** 은하의 심연 최초 완성 팡파레 SFX (NEW-04) */
    playGalaxyFanfare()    { this._playSFX(this.sfxGalaxyFanfare); }

    private _playSFX(clip: AudioClip | null) {
        if (!this.sfxSource || !clip) return;
        this.sfxSource.playOneShot(clip);
    }

    setSFXVolume(vol: number) {
        if (this.sfxSource) this.sfxSource.volume = vol;
    }
}
