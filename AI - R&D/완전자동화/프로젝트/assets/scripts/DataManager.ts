import { _decorator, Component, sys } from 'cc';
const { ccclass } = _decorator;

const KEY_BEST_SCORE = 'star_sweeper_best';
const KEY_SETTINGS   = 'star_sweeper_settings';

export interface GameSettings {
    bgmVolume: number;
    sfxVolume: number;
}

const DEFAULT_SETTINGS: GameSettings = {
    bgmVolume: 0.8,
    sfxVolume: 1.0,
};

/**
 * DataManager - 최고 점수 및 설정 영속 저장/불러오기
 * - cc.sys.localStorage 래퍼
 * - 싱글톤 (선택적 - static 메서드로만 사용 가능)
 */
@ccclass('DataManager')
export class DataManager extends Component {

    private static _instance: DataManager | null = null;
    public static get instance(): DataManager | null { return DataManager._instance; }

    onLoad() {
        DataManager._instance = this;
    }

    onDestroy() {
        if (DataManager._instance === this) DataManager._instance = null;
    }

    // ===== 최고 점수 =====

    static saveBestScore(score: number) {
        const current = DataManager.loadBestScore();
        if (score > current) {
            sys.localStorage.setItem(KEY_BEST_SCORE, String(score));
        }
    }

    static loadBestScore(): number {
        const val = sys.localStorage.getItem(KEY_BEST_SCORE);
        return val ? parseInt(val, 10) : 0;
    }

    static clearBestScore() {
        sys.localStorage.removeItem(KEY_BEST_SCORE);
    }

    // ===== 설정 =====

    static saveSettings(settings: GameSettings) {
        sys.localStorage.setItem(KEY_SETTINGS, JSON.stringify(settings));
    }

    static loadSettings(): GameSettings {
        const val = sys.localStorage.getItem(KEY_SETTINGS);
        if (!val) return { ...DEFAULT_SETTINGS };
        try {
            return { ...DEFAULT_SETTINGS, ...JSON.parse(val) };
        } catch {
            return { ...DEFAULT_SETTINGS };
        }
    }
}
