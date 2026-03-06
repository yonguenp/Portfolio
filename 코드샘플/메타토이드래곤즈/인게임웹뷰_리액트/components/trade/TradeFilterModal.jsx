import styles from "../../styles/Trade.module.css";
import { STAT_OPTIONS, CATEGORY_OPTIONS, GRADE_OPTIONS, TYPE_OPTIONS } from "./StatOptions";
import { useState, useEffect } from "react";
import { TAB } from "./Tabs";

export default function TradeFilterModal({
  categoryFilters_origin,
  setCategoryFilters_origin,
  statFilters_origin,
  setStatFilters_origin,
  gradeFilters_origin,
  setGradeFilters_origin,
  typeFilters_origin,
  setTypeFilters_origin,
  tab,
  onClose,
}) {
  const [categoryFilters, setCategoryFilters] = useState(categoryFilters_origin);
  const [statFilters, setStatFilters] = useState(statFilters_origin); // ← 배열
  const [gradeFilters, setGradeFilters] = useState(gradeFilters_origin); // ← 배열
  const [typeFilters, setTypeFilters] = useState(typeFilters_origin); // ← 배열

  useEffect(() => {
    console.log("--===---");
    setCategoryFilters(categoryFilters_origin);
    setGradeFilters(gradeFilters_origin);
    setStatFilters(statFilters_origin);
    setTypeFilters(typeFilters_origin);
  }, [categoryFilters_origin, gradeFilters_origin, statFilters_origin, typeFilters_origin]);

  const ALL_CATEGORY_KEYS = CATEGORY_OPTIONS.map(c => c.key);
  const ALL_GRADE_KEYS = GRADE_OPTIONS.map(g => g.key);
  const ALL_STAT_KEYS = STAT_OPTIONS.map(s => s.key);
  const ALL_TYPE_KEYS = TYPE_OPTIONS.map(t => t.key);

  const isAllSelected = tab === TAB.ITEM_MARKET ?
    typeFilters.length === ALL_TYPE_KEYS.length
    :
    categoryFilters.length === ALL_CATEGORY_KEYS.length &&
    gradeFilters.length === ALL_GRADE_KEYS.length &&
    statFilters.length === ALL_STAT_KEYS.length;

  const toggleAll = () => {
    if (isAllSelected) {
      if (tab === TAB.ITEM_MARKET) {
        setTypeFilters([]);
      }
      else {
        setCategoryFilters([]);
        setGradeFilters([]);
        setStatFilters([]);
      }
    } else {
      if (tab === TAB.ITEM_MARKET) {
        setTypeFilters(ALL_TYPE_KEYS);
      }
      else {
        setCategoryFilters(ALL_CATEGORY_KEYS);
        setGradeFilters(ALL_GRADE_KEYS);
        setStatFilters(ALL_STAT_KEYS);
      }
    }
  };

  const toggle = (value, setFn) => {
    setFn((prev) =>
      prev.includes(value)
        ? prev.filter((v) => v !== value)
        : [...prev, value]
    );
  };

  return (
    <div className={styles.overlay}>
      <div className={tab === TAB.ITEM_MARKET ? styles.narrowModal : styles.wideModal}>
        <div className={styles.modalHeader}>
          <button
            className={`${styles.allBtn} ${isAllSelected ? styles.active : ""}`}
            onClick={toggleAll}
          >
            {isAllSelected ? "Cancel All" : "Select All"}
          </button>
        </div>
        <div className={styles.filterColumns}>

          {/* 카테고리 */}
          {
            tab === TAB.ITEM_MARKET &&
            (
              <div className={styles.filterColumn}>
                <h3 className={styles.sectionTitle}>Item Type</h3>
                <div className={styles.columnScroll}>
                  <div className={styles.chipGrid}>
                    {TYPE_OPTIONS.map((c) => (
                      <label
                        key={c.key}
                        className={`${styles.chip} ${typeFilters.includes(c.key) ? styles.active_category : ""
                          }`}
                      >
                        <input
                          type="checkbox"
                          checked={typeFilters.includes(c.key)}
                          onChange={() => toggle(c.key, setTypeFilters)}
                        />
                        {c.key}
                      </label>
                    ))}
                  </div>
                </div>
              </div>
            )
          }
          {
            tab !== TAB.ITEM_MARKET &&
            <div className={styles.filterColumn}>
              <h3 className={styles.sectionTitle}>Gem Type</h3>
              <div className={styles.columnScroll}>
                <div className={styles.chipGrid}>
                  {CATEGORY_OPTIONS.map((c) => (
                    <label
                      key={c.key}
                      className={`${styles.chip} ${categoryFilters.includes(c.key) ? styles.active_category : ""
                        }`}
                    >
                      <input
                        type="checkbox"
                        checked={categoryFilters.includes(c.key)}
                        onChange={() => toggle(c.key, setCategoryFilters)}
                      />
                      {c.key}
                    </label>
                  ))}
                </div>
              </div>
            </div>
          }
          {
            tab !== TAB.ITEM_MARKET &&
            <div className={styles.filterColumn}>
              <h3 className={styles.sectionTitle}>Grade</h3>
              <div className={styles.columnScroll}>
                <div className={styles.chipGrid}>
                  {GRADE_OPTIONS.map((g) => (
                    <label
                      key={g.key}
                      className={`${styles.chip} ${gradeFilters.includes(g.key) ? styles.active_grade : ""
                        }`}
                    >
                      <input
                        type="checkbox"
                        checked={gradeFilters.includes(g.key)}
                        onChange={() => toggle(g.key, setGradeFilters)}
                      />
                      {g.label}
                    </label>
                  ))}
                </div>
              </div>
            </div>
          }
          {
            tab !== TAB.ITEM_MARKET &&
            <div className={styles.filterColumn}>
              <h3 className={styles.sectionTitle}>Stat Option</h3>
              <div className={styles.columnScroll}>
                <div className={styles.chipGrid}>
                  {STAT_OPTIONS.map((s) => (
                    <label
                      key={s.key}
                      className={`${styles.chip} ${statFilters.includes(s.key) ? styles.active_stat : ""
                        }`}
                    >
                      <input
                        type="checkbox"
                        checked={statFilters.includes(s.key)}
                        onChange={() => toggle(s.key, setStatFilters)}
                      />
                      {s.label}
                    </label>
                  ))}
                </div>
              </div>
            </div>
          }
        </div>

        {/* 하단 액션 */}
        <div className={styles.actions}>
          <button
            className={styles.cancel}
            onClick={() => {
              if (tab === TAB.ITEM_MARKET) {
                setTypeFilters_origin(ALL_TYPE_KEYS);
              }
              else {
                setCategoryFilters_origin(ALL_CATEGORY_KEYS);
                setGradeFilters_origin(ALL_GRADE_KEYS);
                setStatFilters_origin(ALL_STAT_KEYS);
              }
              onClose();
            }}
          >
            Reset
          </button>
          <button className={styles.confirm} onClick={() => {
            if (tab === TAB.ITEM_MARKET) {
              if (typeFilters.length == 0) {
                setTypeFilters_origin(ALL_TYPE_KEYS);
              }
              else {
                setTypeFilters_origin(typeFilters);
              }
            }
            else {
              if (categoryFilters.length == 0) {
                setCategoryFilters_origin(ALL_CATEGORY_KEYS);
              }
              else {
                setCategoryFilters_origin(categoryFilters);
              }

              if (gradeFilters.length == 0) {
                setGradeFilters_origin(ALL_GRADE_KEYS);
              }
              else {
                setGradeFilters_origin(gradeFilters);
              }

              if (statFilters.length == 0) {
                setStatFilters_origin(ALL_STAT_KEYS);
              }
              else {
                setStatFilters_origin(statFilters);
              }
            }
            onClose();
          }}>
            Confirm
          </button>
        </div>
      </div>
    </div>
  );
}
