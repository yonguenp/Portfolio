import { _decorator, Component, Node, Label, Sprite, SpriteFrame, resources } from 'cc';
import { StarColor, GameManager } from './GameManager';
import { AudioManager } from './AudioManager';
import { ConstellationBookManager } from './ConstellationBookManager';
import { HUDController } from './HUDController';
import { UIManager } from './UIManager';
const { ccclass, property } = _decorator;

export interface ConstellationPattern {
    name: string;
    required: Partial<Record<StarColor, number>>;
    totalStars: number;
}

// 별자리 패턴 정의 — spec_v3.md "Wave 색상 설계표" 기준으로 완전 동기화
// Wave 1: RED, BLUE 스폰 → 오리온자리: RED×3, BLUE×2
// Wave 2: RED, BLUE, YELLOW 스폰 → 큰곰자리: BLUE×2, YELLOW×2, RED×1
// Wave 3: RED, BLUE, YELLOW, GREEN 스폰 → 카시오페이아: GREEN×2, RED×2, BLUE×1
// Wave 4: RED, BLUE, YELLOW, GREEN 스폰 → 사자자리: GREEN×2, YELLOW×2, RED×2
// Wave 5: 전체 5색 스폰 → 전갈자리: PURPLE×2, GREEN×2, RED×2
// Wave 6: 전체 5색 스폰 → 황소자리: PURPLE×2, BLUE×2, YELLOW×2, RED×1
// Wave 7+: 5색 중 2~4종 랜덤 선택, 총 6~8개 랜덤 배분 (spec_v3 무작위 생성 규칙)
function buildPattern(wave: number): ConstellationPattern {
    if (wave >= 7) {
        return _buildRandomPattern(wave);
    }

    const patterns: ConstellationPattern[] = [
        { name: '오리온자리',    required: { RED: 3, BLUE: 2 },                       totalStars: 5 },
        { name: '큰곰자리',      required: { BLUE: 2, YELLOW: 2, RED: 1 },             totalStars: 5 },
        { name: '카시오페이아',  required: { GREEN: 2, RED: 2, BLUE: 1 },              totalStars: 5 },
        { name: '사자자리',      required: { GREEN: 2, YELLOW: 2, RED: 2 },            totalStars: 6 },
        { name: '전갈자리',      required: { PURPLE: 2, GREEN: 2, RED: 2 },            totalStars: 6 },
        { name: '황소자리',      required: { PURPLE: 2, BLUE: 2, YELLOW: 2, RED: 1 }, totalStars: 7 },
    ];
    return patterns[wave - 1];
}

/**
 * Wave 7+ 랜덤 패턴 생성 — spec_v3 규칙 적용
 * 1. 사용 가능 색상: 5색 전체
 * 2. 총 요구 별 수: min(6 + Math.floor((wave-7)/2), 10)
 * 3. 2~4종 색상 랜덤 선택 후 배분
 * 4. 각 색상 최소 1개 이상
 * 5. 단일 색상 전체의 50% 초과 배정 불가
 * 6. Math.random() 기반 순수 랜덤 (시드 없음)
 */
function _buildRandomPattern(wave: number): ConstellationPattern {
    const allColors: StarColor[] = [
        StarColor.RED, StarColor.BLUE, StarColor.YELLOW, StarColor.GREEN, StarColor.PURPLE,
    ];
    const totalStars = Math.min(6 + Math.floor((wave - 7) / 2), 10);
    const maxPerColor = Math.floor(totalStars * 0.5); // 50% 초과 배정 불가

    // 2~4종 랜덤 선택
    const colorCount = 2 + Math.floor(Math.random() * 3); // 2, 3, 4
    const shuffled = [...allColors].sort(() => Math.random() - 0.5);
    const chosen = shuffled.slice(0, colorCount);

    // 각 색상에 1개씩 먼저 배분
    const required: Partial<Record<StarColor, number>> = {};
    for (const c of chosen) required[c] = 1;
    let remaining = totalStars - colorCount;

    // 남은 별을 랜덤하게 배분 (50% 초과 방지)
    let attempts = 0;
    while (remaining > 0 && attempts < 1000) {
        const c = chosen[Math.floor(Math.random() * chosen.length)];
        if ((required[c] ?? 0) < maxPerColor) {
            required[c] = (required[c] ?? 0) + 1;
            remaining--;
        }
        attempts++;
    }
    // 배분 실패 잔여분은 첫 번째 색상에 할당 (안전장치)
    if (remaining > 0) {
        required[chosen[0]] = (required[chosen[0]] ?? 0) + remaining;
    }

    return { name: '은하의 심연', required, totalStars };
}

/**
 * ConstellationManager - 별자리 목표 패턴 관리
 * - 현재 목표 별자리 패턴 보유
 * - 수집된 별 현황 추적
 * - 완성 조건 달성 시 GameManager.onConstellationDone() 호출
 * - UI 슬롯 갱신: 색상별 SVG Sprite 슬롯 방식 (m-01 — 텍스트 '★' 방식 폐기)
 *   미수집 슬롯 → slot_empty.svg / 수집 완료 슬롯 → slot_[color].svg
 */
@ccclass('ConstellationManager')
export class ConstellationManager extends Component {

    @property({ type: Node })
    constellationUIRoot: Node | null = null;

    @property({ type: Label })
    constellationNameLabel: Label | null = null;

    /** 슬롯 Sprite 노드 배열 — 에디터에서 ConstellationUI 하위 슬롯 노드들을 순서대로 할당 (m-01) */
    @property({ type: [Node] })
    slotNodes: Node[] = [];

    /** HUDController 참조 — Wave 진행도 바 갱신 (M-WP-01) */
    @property({ type: HUDController })
    hudController: HUDController = null!;

    private _pattern: ConstellationPattern | null = null;
    private _collected: Partial<Record<StarColor, number>> = {};

    // 슬롯 SVG 리소스 캐시
    private _emptyFrame: SpriteFrame | null = null;
    private _colorFrames: Partial<Record<StarColor, SpriteFrame>> = {};

    onLoad() {
        this._preloadSlotFrames();
        this._loadNextPattern();
    }

    /** slot_empty 및 slot_[color] SpriteFrame 사전 로드 */
    private _preloadSlotFrames() {
        resources.load('slot_empty/spriteFrame', SpriteFrame, (err, sf) => {
            if (!err) this._emptyFrame = sf;
        });
        const colorNames: Array<[StarColor, string]> = [
            [StarColor.RED,    'slot_red'],
            [StarColor.BLUE,   'slot_blue'],
            [StarColor.YELLOW, 'slot_yellow'],
            [StarColor.GREEN,  'slot_green'],
            [StarColor.PURPLE, 'slot_purple'],
        ];
        for (const [color, path] of colorNames) {
            resources.load(`${path}/spriteFrame`, SpriteFrame, (err, sf) => {
                if (!err) this._colorFrames[color] = sf;
            });
        }
    }

    private _loadNextPattern() {
        const wave = GameManager.instance?.currentWave ?? 1;
        this._pattern = buildPattern(wave);
        this._collected = {};
        this._updateUI();
    }

    /**
     * 별 하나 수집 - StarSpawner 에서 호출
     * 패턴 외 색상 수집 시 조용히 무시 — 점수는 GameManager.addScore()로 부여되며
     * ConstellationUI 슬롯에 아무런 반응 없음 (spec_v3 m-05 의도된 설계)
     */
    addStar(color: StarColor) {
        if (!this._pattern) return;
        if (color === StarColor.DARK) return;

        // 이 패턴에서 필요한 색상인지 확인
        const required = this._pattern.required[color];
        if (!required) return; // 패턴 외 색상 — 조용히 무시 (슬롯 반응 없음)

        const current = this._collected[color] ?? 0;
        if (current >= required) return; // 이미 충족

        this._collected[color] = current + 1;
        this._updateUI();

        // Wave 7+ 진행도 바 갱신 (M-WP-01)
        const wave = GameManager.instance?.currentWave ?? 1;
        if (wave >= 7 && this._pattern) {
            const totalRequired = this._pattern.totalStars;
            const currentCount = Object.values(this._collected).reduce((sum, n) => sum + (n ?? 0), 0);
            this.hudController?.updateWaveProgress(currentCount, totalRequired);
        }

        this._checkCompletion();
    }

    private _checkCompletion() {
        if (!this._pattern) return;
        for (const [colorKey, needed] of Object.entries(this._pattern.required)) {
            const color = colorKey as StarColor;
            const have = this._collected[color] ?? 0;
            if (have < needed) return; // 미충족
        }
        // 완성! SFX 재생 후 GameManager 통보
        AudioManager.instance?.playConstellation();

        const wave = GameManager.instance?.currentWave ?? 1;

        if (wave >= 7) {
            // Wave 7+: 진행도 바 100% 표시 (M-WP-01)
            const total = this._pattern.totalStars;
            this.hudController?.updateWaveProgress(total, total);

            // n-05: 이미 잠금 해제된 경우 recordCompletion 스킵
            const alreadyUnlocked = ConstellationBookManager.isUnlocked('은하의 심연');
            if (!alreadyUnlocked) {
                // NEW-04: 최초 완성 시 은하 이펙트 + 팡파레 → 2.5초 후 recordCompletion + 일반 클리어
                UIManager.instance?.showGalaxyEffect();
                AudioManager.instance?.playGalaxyFanfare();
                this.scheduleOnce(() => {
                    if (this._pattern) {
                        ConstellationBookManager.recordCompletion(this._pattern.name, wave);
                    }
                    GameManager.instance?.onConstellationDone();
                    this._scheduleNextPattern();
                }, 2.5);
                return;
            }
        } else {
            // Wave 1~6: 완성 직후 진행도 바 100% 표시
            this.hudController?.updateWaveProgress(1, 1);

            // 도감 기록 (NEW-02 — 신규 별자리 최초 완성 시 playBookUnlock 호출)
            if (this._pattern) {
                const isNew = ConstellationBookManager.recordCompletion(this._pattern.name, wave);
                if (isNew) {
                    AudioManager.instance?.playBookUnlock();
                }
            }
        }

        GameManager.instance?.onConstellationDone();
        this._scheduleNextPattern();
    }

    private _scheduleNextPattern() {
        // 0.5초 딜레이 후 다음 패턴 로드 (Wave 전환 연출 대기)
        this.scheduleOnce(() => {
            this._loadNextPattern();
        }, 0.5);
    }

    /**
     * UI 슬롯 갱신 — 색상별 SVG Sprite 방식 (m-01)
     * slotNodes 배열의 각 노드에 Sprite를 설정:
     *   수집 완료 → slot_[color].svg SpriteFrame
     *   미수집    → slot_empty.svg SpriteFrame
     */
    private _updateUI() {
        if (!this._pattern) return;

        if (this.constellationNameLabel) {
            this.constellationNameLabel.string = this._pattern.name;
        }

        // 슬롯 노드 배열 기반 갱신
        let slotIdx = 0;
        for (const [colorKey, needed] of Object.entries(this._pattern.required)) {
            const color = colorKey as StarColor;
            const have = this._collected[color] ?? 0;
            for (let i = 0; i < needed; i++) {
                if (slotIdx >= this.slotNodes.length) break;
                const slotNode = this.slotNodes[slotIdx];
                const sprite = slotNode?.getComponent(Sprite);
                if (sprite) {
                    if (i < have) {
                        // 수집 완료: 색상 슬롯 SVG
                        const frame = this._colorFrames[color];
                        if (frame) sprite.spriteFrame = frame;
                    } else {
                        // 미수집: 빈 슬롯 SVG
                        if (this._emptyFrame) sprite.spriteFrame = this._emptyFrame;
                    }
                }
                slotIdx++;
            }
        }
        // 사용되지 않는 잔여 슬롯 비활성화
        for (let i = slotIdx; i < this.slotNodes.length; i++) {
            if (this.slotNodes[i]) this.slotNodes[i].active = false;
        }
    }

    getCurrentPattern(): ConstellationPattern | null {
        return this._pattern;
    }

    getCollected(): Partial<Record<StarColor, number>> {
        return { ...this._collected };
    }
}
