import { useEffect, useState } from "react";
import styles from "../../styles/NFTInventoryLeftDetail.module.css";
import { useSelector } from "react-redux";
import { MTS_RANK_STR } from "../../data/data";

export default function NFTInventoryLeftDetail({ selected, rewards }) {
  const selectedData = useSelector((state) => state.nftInventory.selectedItem);
  const collection = useSelector((state) => state.nftInventory.filter);
  // // api 던져줄때 옵션 제목값도 같이 넘어오면 훨씬 수월해짐, 이건 일단 대기
  // if (selectedData.filter === 1) {
  //   // Gem Block
  //   const detailList = ["Item Name", "Rank", "Base Stat", "Optional Stat"];
  // } else if (selectedData.filter === 3) {
  //   // GoldBox
  //   const detailList = ["Reward1", "Reward2", "Reward3"];
  // } else if (selectedData.filter === 4) {
  //   // Ticket
  //   const detailList = ["Rank", "Reward"];
  // } else if (selectedData.filter === 5) {
  //   const detailList = ["Level", "Reward"];
  // }
  // const fetchRewards = async () => {
  //   const data = await window.DApp.post(
  //     "inventory/inventory_get_item_rewards",
  //     {
  //       collection: collection,
  //       tokenId: selectedData.tokenId,
  //       grade: selectedData?.grade || null,
  //     }
  //   );
  // };
  // useEffect(() => {
  //   if (!selectedData) return;
  //   console.log(selectedData);
  // }, [selectedData]);

  return (
    <div className={styles.bottomDetailBox}>
      <div className={styles.bottomDetail}>
        <div
          className={`${styles.bottomDetailTitle} text-start mb-8 font-b3-b`}
        >
          Details
        </div>
        {selected === "true" ? (
          <>
            {(selectedData?.attributes ?? []).map((v, idx) => {
              let value = v.value;
              if (collection == "mts" && v.trait_type == "Rank") {
                value = MTS_RANK_STR[v.value];
              }
              return (
                <div className={styles.bottomDetailInfo} key={idx}>
                  <div className="font-b4-r">{v.trait_type}</div>
                  <div
                    className="font-b3-b"
                    style={{ display: "flex", gap: "8px" }}
                  >
                    {v?.opt_value && (
                      <span className="font-b3-r green-500">
                        {v?.opt_value}
                      </span>
                    )}
                    <span>{value}</span>
                  </div>
                </div>
              );
            })}
            {rewards &&
              rewards.rewards.map((v, idx) => (
                <div className={styles.bottomDetailInfo} key={idx}>
                  <div className="font-b4-r">{v.trait_type}</div>
                  <div
                    className="font-b3-b"
                    style={{ display: "flex", gap: "8px" }}
                  >
                    {v?.opt_value && (
                      <span className="font-b3-r green-500">
                        {v?.opt_value}
                      </span>
                    )}
                    {v?.value}
                  </div>
                </div>
              ))}
            {/* {selectedData.filter === 1 ? (
              <>
                <div className={styles.bottomDetailInfo}>
                  <div className="font-b4-r">NFT Rairity</div>
                  <div className="font-b3-b">Unique </div>
                </div>

                <div className={styles.bottomDetailInfo}>
                  <div className="font-b4-r">Base Stat</div>
                  <div className="font-b3-b">Dark DMG RES + 50%</div>
                </div>

                <div className={styles.bottomDetailInfo}>
                  <div className="font-b4-r">Base Stat</div>
                  <div className="font-b3-b">Dark DMG RES + 50%</div>
                </div>
              </>
            ) : (
              <>
                <div className={styles.bottomDetailInfo}>
                  <div className="font-b4-r">Reward1</div>
                  <div className="font-b3-b">Global Buff</div>
                </div>

                <div className={styles.bottomDetailInfo}>
                  <div className="font-b4-r">Reward2</div>
                  <div className="font-b3-b">Gold Pass</div>
                </div>

                <div className={styles.bottomDetailInfo}>
                  <div className="font-b4-r">Reward3</div>
                  <div className="font-b3-b">
                    <span
                      className={styles.optionColor}
                      style={{ paddingRight: "5px" }}
                    >
                      254%
                    </span>
                    Mining Boost Ticket
                  </div>
                </div>
              </>
            )} */}
          </>
        ) : (
          <>
            <div className={`${styles.bottomDetailInfo_noData} font-b4-r`}>
              <div>No item to display</div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
