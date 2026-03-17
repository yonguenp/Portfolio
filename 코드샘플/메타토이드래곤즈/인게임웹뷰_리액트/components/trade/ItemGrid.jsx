// components/trade/ItemGrid.jsx
import ItemCard from "./ItemCard";
import styles from "../../styles/Trade.module.css";

export default function ItemGrid({ items, mode, onAction, page, onPage, maxPage }) {

  const prevPage = () => {
    if (page > 0)
      onPage(page - 1);
  }

  const nextPage = () => {
    if (maxPage > page + 1)
      onPage(page + 1);
  }

  return (
    <div className={styles.gridWrapper}>

      {
        maxPage > 0 && (
          <div className={styles.pageTop}>
            <button
              onClick={prevPage}
              disabled={page === 0}
              className={styles.pageBtn}
            >
              ◀ Prev Page
            </button>
          </div>
        )
      }
      {/* 아이템 그리드 */}
      <div className={styles.grid}>
        {items.map((item, key) => (
          <ItemCard
            key={key}
            item={item}
            mode={mode}
            onAction={onAction}
          />
        ))}
      </div>

      {
        maxPage > 0 && (
          <div className={styles.pageBottom}>
            <button
              onClick={nextPage}
              disabled={maxPage <= page + 1}
              className={styles.pageBtn}
            >
              Next Page ▶
            </button>
          </div>
        )
      }
    </div>
  );
}
