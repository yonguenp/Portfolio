export enum TowerType {
    Normal = 0,
    Rapid  = 1,
    Splash = 2,
    Slow   = 3,
    Sniper = 4,
}

export interface TowerConfig {
    name:         string;
    desc:         string;
    cost:         number;
    color:        [number, number, number];  // RGB
    size:         number;                    // UITransform size px
    damage:       number;
    fireRate:     number;
    range:        number;
    splashRadius: number;   // 0 = no splash
    slowFactor:   number;   // 0 = no slow; 0.5 = reduce speed to 50%
    slowDuration: number;
    upgDamage:    number;   // multiplier per level (e.g. 1.20)
    upgFireRate:  number;
    upgRange:     number;
    upgSpecial:   number;   // splash: radius mult per level; slow: add per level
}

export interface TowerStats {
    damage:       number;
    fireRate:     number;
    range:        number;
    splashRadius: number;
    slowFactor:   number;
    slowDuration: number;
}

export const TOWER_CONFIGS: Record<TowerType, TowerConfig> = {
    [TowerType.Normal]: {
        name: '기본',   desc: '균형잡힌 타워',
        cost: 50,  color: [60,  120, 220], size: 44,
        damage: 30,  fireRate: 1.0, range: 160,
        splashRadius: 0, slowFactor: 0, slowDuration: 0,
        upgDamage: 1.20, upgFireRate: 1.10, upgRange: 1.05, upgSpecial: 0,
    },
    [TowerType.Rapid]: {
        name: '연사',   desc: '공격력 절반·연사 2배',
        cost: 75,  color: [0,   200, 220], size: 36,
        damage: 15,  fireRate: 2.0, range: 140,
        splashRadius: 0, slowFactor: 0, slowDuration: 0,
        upgDamage: 1.25, upgFireRate: 1.15, upgRange: 1.0, upgSpecial: 0,
    },
    [TowerType.Splash]: {
        name: '스플레시', desc: '광역 피해',
        cost: 80,  color: [220, 120,  20], size: 48,
        damage: 15,  fireRate: 0.5, range: 130,
        splashRadius: 80, slowFactor: 0, slowDuration: 0,
        upgDamage: 1.20, upgFireRate: 1.0, upgRange: 1.0, upgSpecial: 1.12,
    },
    [TowerType.Slow]: {
        name: '둔화',   desc: '피해 없음·속도↓',
        cost: 60,  color: [160,  60, 220], size: 44,
        damage: 0,   fireRate: 1.0, range: 120,
        splashRadius: 0, slowFactor: 0.50, slowDuration: 2.0,
        upgDamage: 1.0, upgFireRate: 1.0, upgRange: 1.0, upgSpecial: 0.04,
    },
    [TowerType.Sniper]: {
        name: '장거리', desc: '공격력 2배·사거리 2배',
        cost: 80,  color: [50,  200,  80], size: 40,
        damage: 60,  fireRate: 0.5, range: 320,
        splashRadius: 0, slowFactor: 0, slowDuration: 0,
        upgDamage: 1.25, upgFireRate: 1.10, upgRange: 1.0, upgSpecial: 0,
    },
};

export function getStats(type: TowerType, level: number): TowerStats {
    const c = TOWER_CONFIGS[type];
    const n = Math.max(0, Math.min(9, level - 1));
    return {
        damage:       c.damage      * Math.pow(c.upgDamage,   n),
        fireRate:     c.fireRate    * Math.pow(c.upgFireRate,  n),
        range:        c.range       * Math.pow(c.upgRange,     n),
        splashRadius: c.splashRadius > 0 ? c.splashRadius * Math.pow(c.upgSpecial, n) : 0,
        slowFactor:   c.slowFactor  > 0 ? Math.min(0.85, c.slowFactor + n * c.upgSpecial) : 0,
        slowDuration: c.slowDuration,
    };
}

export function upgradeCost(type: TowerType, currentLevel: number): number {
    return Math.floor(TOWER_CONFIGS[type].cost * 0.4 * currentLevel);
}

export function sellValue(type: TowerType, level: number): number {
    let total = TOWER_CONFIGS[type].cost;
    for (let l = 1; l < level; l++) total += upgradeCost(type, l);
    return Math.floor(total * 0.5);
}
