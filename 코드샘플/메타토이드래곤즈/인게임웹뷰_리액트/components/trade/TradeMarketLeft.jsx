import { useState, useEffect } from "react";
import styles from "../../styles/Trade.module.css";
import { TAB } from "./Tabs";

export default function TradeMarketLeft({ tab, setTab }) {
  const [openMyInfo, setOpenMyInfo] = useState(true);

  const MY_INFO_TABS = [
    TAB.GEM_SELLING,
    TAB.ITEM_SELLING,
    TAB.MY_GEMS,
    TAB.MY_ITEMS,
    TAB.GEM_HISTORY,
    TAB.ITEM_HISTORY,
  ];

  useEffect(() => {
    if (MY_INFO_TABS.includes(tab)) {
      setOpenMyInfo(true);
    }
  }, [tab]);

  const MenuButton = ({ label, targetTab, indent = false }) => {
    const active = tab === targetTab;

    return (
      <div
        className={`${styles.sideBtn} ${indent ? styles.indent : ""} ${
          active ? styles.activeSideBtn : ""
        }`}
        onClick={() => setTab(targetTab)}
      >
        {label}
      </div>
    );
  };

  return (
    <div className={styles.leftContainer}>

      {/* ===== MARKET ===== */}
      <div className={styles.sideSectionTitle}>MARKET</div>
      <MenuButton label="Gem" targetTab={TAB.GEM_MARKET} />
      <MenuButton label="Item" targetTab={TAB.ITEM_MARKET} />

      {/* ===== MY INFO (Accordion) ===== */}
      <div
        className={`${styles.sideSectionTitle} ${styles.accordionHeader}`}
        onClick={() => setOpenMyInfo((v) => !v)}
      >
        MY ASSETS
      </div>

      {openMyInfo && (
        <div className={styles.subMenu}>

          <div className={styles.subSectionTitle}>Selling</div>
          <MenuButton indent label="Gem" targetTab={TAB.GEM_SELLING} />
          <MenuButton indent label="Item" targetTab={TAB.ITEM_SELLING} />

          <div className={styles.subSectionTitle}>My Inventory</div>
          <MenuButton indent label="Gem" targetTab={TAB.MY_GEMS} />
          <MenuButton indent label="Item" targetTab={TAB.MY_ITEMS} />

          <div className={styles.subSectionTitle}>History</div>
          <MenuButton indent label="Gem" targetTab={TAB.GEM_HISTORY} />
          <MenuButton indent label="Item" targetTab={TAB.ITEM_HISTORY} />

        </div>
      )}
    </div>
  );
}
