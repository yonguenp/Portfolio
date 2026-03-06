// components/trade/ItemCard.jsx
import styles from "../../styles/Trade.module.css";
import { TAB } from "./Tabs";
import { STAT_OPTIONS } from "./StatOptions";

export default function ItemCard({ item, mode, onAction, compact = false }) {

  const getFusionStatVal = (type, val) => {    
    switch (type) {
      case "HP":
      case "DEF":
      case "ATK":
      case "CRI_DMG":
      case "ADD_ATK_DMG":
      case "BOSS_DMG":
      case "ADD_MAIN_ELEMENT_DMG":
        return "+ " + val;
    }

    return val + " %";
  };

  const user_no = sessionStorage.getItem("user_no");
  const isFusion = item.grade == "Unique_FUSION" || item.grade == "Legendary_FUSION";
  return (
    <div className={styles.card}>
      <div className={styles.name}>
        {isFusion ? (
          <>
            <span className={styles.fusion}>FUSION </span>
            {item.name}
          </>
        ) : (
          item.name
        )}
      </div>

      <div className={styles.imageWrap}>
        <img src={item.image} alt={item.name} />
      </div>

      {/* 강화 */}
      {
        (mode === TAB.GEM_MARKET || mode === TAB.GEM_SELLING || mode === TAB.MY_GEMS || mode === TAB.GEM_HISTORY) && (
          <div className={styles.stats}>
            <div
              key={`${item.id}-reinforce`}
              className={styles.statLine}
            >
              <span className={styles.statLabel}>
                Reinforce
              </span>
              <span className={styles.statValue}>
                {item.reinforce}
              </span>
            </div>
          </div>
        )
      }

      {/* 스탯 영역 */}
      <div className={styles.stats}>
        {(item.stats || [])
          .filter(stat =>
            STAT_OPTIONS.some(s => s.key === stat.type)
          )
          .map((stat, idx, arr) => {
            const statInfo = STAT_OPTIONS.find(
              s => s.key === stat.type
            );

            const isFusionOption = isFusion && idx === arr.length - 1;
            return (
              <div
                key={`${item.id}-${stat.type}-${idx}`}
                className={styles.statLine}
              >
                <span className={styles.statLabel}>
                  {isFusionOption ? <span className={styles.fusion}>{statInfo?.label ?? stat.type}</span> : <span className={ idx === 0 ? styles.mainstat : styles.normalstat}>{statInfo?.label ?? stat.type}</span>}
                </span>
                <span className={styles.statValue}>
                  {isFusionOption ? getFusionStatVal(stat.type, stat.value) : (stat.value + " %")}
                </span>
              </div>
            );
          })}
      </div>
      {
        item.item_type !== "GB" && item.reg_qty && item.reg_qty > 0 && (mode !== TAB.GEM_MARKET && mode !== TAB.GEM_SELLING && mode !== TAB.GEM_HISTORY) && (
          <div className={styles.stats}>
            <div
              key={`${item.id}-amount`}
              className={styles.statLine}
            >
              <span className={styles.statLabel}>
                {
                  (((compact && item.tm_idx) || mode === TAB.ITEM_MARKET || mode === TAB.GEM_SELLING || mode === TAB.ITEM_SELLING) ? 'Remain Amount' : (mode === TAB.ITEM_HISTORY ? 'Sold Amount' : 'Amount'))
                }
              </span>
              <span className={styles.statValue}>
                {
                  (((compact && item.tm_idx) || mode === TAB.ITEM_MARKET || mode === TAB.GEM_SELLING || mode === TAB.ITEM_SELLING) ? (item.remain_qty ?? 0) : (mode === TAB.ITEM_HISTORY ? (item.sold_qty ?? 0) : (item.reg_qty ?? 0))).toLocaleString()
                }
              </span>
            </div>
            {
              ((compact && !item.tm_idx) || (mode === TAB.MY_GEMS || mode === TAB.MY_ITEMS) || item.item_type === "GB") ?
                <></> :
                (
                  <div
                    key={`${item.id}-unit-price`}
                    className={styles.statLine}
                  >
                    <span className={styles.statLabel}>
                      {compact ? 'Price Each' : 'Total Price'}
                    </span>
                    <span className={styles.statValue}>
                      {compact ? item.price.toLocaleString() : ((item.price * (item.reg_qty - item.sold_qty)).toLocaleString())}
                    </span>
                  </div>
                )
            }
          </div>
        )
      }


      {!compact && (mode !== TAB.MY_GEMS && mode !== TAB.MY_ITEMS) && item.price && (
        <div className={styles.price}>
          <img
            src="/images/icon/magnite.png"
            alt="magnite"
            className={styles.priceIcon}
          />
          <span className={styles.priceValue}>
            {item.price.toLocaleString()}
          </span>
        </div>
      )}


      {!compact && (mode === TAB.GEM_MARKET || mode === TAB.ITEM_MARKET) && (
        <div className={`${styles.action} bg-red-500 text-white`}>
          <button disabled={item.user_no == user_no} onClick={() => onAction(item)}>{item.user_no != user_no ? 'Buy' : 'My Registed'}</button>
        </div>
      )}
      {!compact && (mode === TAB.GEM_SELLING || mode === TAB.ITEM_SELLING) && (
        <div className={`${styles.action} bg-orange-500 text-white`}>
          <button onClick={() => onAction(item)}>Cancel</button>
        </div>
      )}
      {!compact && (mode === TAB.MY_GEMS || mode === TAB.MY_ITEMS) && (
        <div className={`${styles.action} bg-green-500 text-white`}>
          <button onClick={() => onAction(item)}>Sell</button>
        </div>
      )}
      {/* {!compact && (mode === TAB.GEM_HISTORY || mode === TAB.ITEM_HISTORY) && (
        <div className="bg-purple-500 text-white flex items-center justify-center">
          {item.closed_at}
        </div>
      )} */}
    </div>
  );
}
