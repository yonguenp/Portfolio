// components/trade/TradeFilterBar.jsx
import styles from "../../styles/Trade.module.css";
import { STAT_OPTIONS, CATEGORY_OPTIONS, GRADE_OPTIONS, TYPE_OPTIONS } from "./StatOptions";
import { TAB } from "./Tabs";


export default function TradeFilterBar({
  tab,
  sortType,
  setSortType,
  statFilters,
  categoryFilters,
  gradeFilters,
  typeFilters,
  onOpenFilter,
}) {
  const ALL_CATEGORY_KEYS = CATEGORY_OPTIONS.map(c => c.key);
  const ALL_GRADE_KEYS = GRADE_OPTIONS.map(g => g.key);
  const ALL_STAT_KEYS = STAT_OPTIONS.map(s => s.key);
  const ALL_TYPE_KEYS = TYPE_OPTIONS.map(t => t.key);

  const isCategoryAllSelected = categoryFilters.length === ALL_CATEGORY_KEYS.length;
  const isGradeAllSelected = gradeFilters.length === ALL_GRADE_KEYS.length;
  const isStatAllSelected = statFilters.length === ALL_STAT_KEYS.length;
  const isTypeAllSelected = typeFilters.length === ALL_TYPE_KEYS.length;
  const isAllSelected = isCategoryAllSelected && isGradeAllSelected && isStatAllSelected;

  const STAT_LABEL_MAP = Object.fromEntries(
    STAT_OPTIONS.map(s => [s.key, s.label])
  );

  const CATEGORY_LABEL_MAP = Object.fromEntries(
    CATEGORY_OPTIONS.map(c => [c.key, c.key])
  );

  const GRADE_LABEL_MAP = Object.fromEntries(
    GRADE_OPTIONS.map(g => [g.key, g.label])
  );

  const TYPE_LABEL_MAP = Object.fromEntries(
    TYPE_OPTIONS.map(t => [t.key, t.key])
  );

  const categoryText =
    isCategoryAllSelected
      ? "All Category"
      : categoryFilters.map(k => CATEGORY_LABEL_MAP[k]).join(" · ");

  const gradeText =
    isGradeAllSelected
      ? "All Grade"
      : gradeFilters.map(k => GRADE_LABEL_MAP[k]).join(" · ");

  const statText =
    isStatAllSelected
      ? "All Stat"
      : statFilters.map(k => STAT_LABEL_MAP[k]).join(" · ");

  const typeText =
    isTypeAllSelected
      ? "All Type"
      : typeFilters.map(k => TYPE_LABEL_MAP[k]).join(" · ");
      
  return (
    <div className={styles.bar}>
      {/* 정렬 */}
      {
        (tab === TAB.GEM_MARKET &&
          <div className={styles.sortGroup}>
            <span>Sorts</span>
            <select
              value={sortType}
              onChange={(e) => setSortType(e.target.value)}
            >
              <option value="6">Recent</option>              
              {/* <option value="3">Grade Asc</option> */}
              <option value="4">Grade</option>              
              {/* <option value="5">REG_DATE ASC</option> */}
              {/* <option value="6">REG_DATE DESCc</option> */}
              {/* <option value="7">Reinforce Asc</option> */}
              <option value="8">Reinforce</option>
              <option value="1">Price Asc</option>
              <option value="2">Price Desc</option>
            </select>
          </div>
        )
      }
      {
        /* 스탯 필터 */
        tab === TAB.GEM_MARKET &&
        (
          <button
            className={styles.filterBtn}
            onClick={onOpenFilter}
          >
            Filters
            <span className={styles.filterSummary}>
              {categoryText + " / "}
              {gradeText + " / "}
              {statText}
            </span>
          </button>
        )}
      {
        /* 종류 필터 */
        tab === TAB.ITEM_MARKET &&
        (
          <button
            className={styles.filterBtn}
            onClick={onOpenFilter}
          >
            Filters
            <span className={styles.filterSummary}>
              {typeText}
            </span>
          </button>
        )}
    </div>
  );
}