import { sys } from 'cc';

const KEY_BOOK = 'star_sweeper_book';

/**
 * 도감 기록 단일 항목
 */
export interface ConstellationRecord {
    name: string;   // 별자리 이름
    wave: number;   // 완성한 Wave 번호
    date: string;   // 완성 날짜 (ISO 8601 문자열)
}

/**
 * ConstellationBookManager - 별자리 도감 데이터 관리 (신규 — spec_v3 NEW-02)
 * - localStorage 키: star_sweeper_book
 * - 저장 형식: JSON 배열 [{ name, wave, date }, ...]
 * - 동일 별자리 재완성 시 최초 기록만 유지 (중복 등록 방지)
 * - static 메서드 전용 — 인스턴스 불필요 (DataManager 패턴과 동일)
 *
 * 도감 수록 별자리 7종 (spec_v3 기준):
 *   1. 오리온자리    Wave 1
 *   2. 큰곰자리      Wave 2
 *   3. 카시오페이아  Wave 3
 *   4. 사자자리      Wave 4
 *   5. 전갈자리      Wave 5
 *   6. 황소자리      Wave 6
 *   7. 은하의 심연   Wave 7+
 */
export class ConstellationBookManager {

    /**
     * 별자리 완성 기록 등록
     * @param name  별자리 이름 (ConstellationPattern.name)
     * @param wave  완성 당시 Wave 번호
     * @returns true: 신규 등록 성공 / false: 이미 등록된 별자리 (중복 무시)
     */
    static recordCompletion(name: string, wave: number): boolean {
        const records = ConstellationBookManager.getRecords();

        // 중복 등록 방지 — 이미 해금된 별자리는 재등록하지 않음
        const alreadyUnlocked = records.some(r => r.name === name);
        if (alreadyUnlocked) return false;

        const newRecord: ConstellationRecord = {
            name,
            wave,
            date: new Date().toISOString(),
        };
        records.push(newRecord);

        sys.localStorage.setItem(KEY_BOOK, JSON.stringify(records));
        return true;
    }

    /**
     * 전체 도감 기록 반환
     * @returns ConstellationRecord 배열 (등록 순서 유지)
     */
    static getRecords(): ConstellationRecord[] {
        const val = sys.localStorage.getItem(KEY_BOOK);
        if (!val) return [];
        try {
            const parsed = JSON.parse(val);
            if (Array.isArray(parsed)) return parsed as ConstellationRecord[];
            return [];
        } catch {
            return [];
        }
    }

    /**
     * 특정 별자리 해금 여부 확인
     * @param name 별자리 이름
     * @returns true: 해금됨 / false: 미해금
     */
    static isUnlocked(name: string): boolean {
        return ConstellationBookManager.getRecords().some(r => r.name === name);
    }

    /**
     * 도감 데이터 초기화 (디버그용)
     */
    static clearRecords() {
        sys.localStorage.removeItem(KEY_BOOK);
    }
}
