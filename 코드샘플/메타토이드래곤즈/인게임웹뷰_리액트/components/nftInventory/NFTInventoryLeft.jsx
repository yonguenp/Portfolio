import styles from "../../styles/NFTInventoryLeft.module.css";
import { useSelector } from "react-redux";
import NFTInventoryLeftDetail from "./NFTInventoryLeftDetail";
import { usePopup } from "../../context/PopupContext";
import { useEffect, useState } from "react";
import { getGradeForItem } from "../../utils/itemUtils";

export default function NFTInventoryLeft(props) {
  const [rewards, setRewards] = useState(null);
  const { fullList, filter } = useSelector((state) => state.nftInventory);
  const selectedItem = useSelector((state) => state.nftInventory.selectedItem);

  const { isOpen, openPopup, closePopup } = usePopup();
  //console.log(selectedData.dataArr)
  const fetchReward = async () => {
      const gradeValue = getGradeForItem(selectedItem);

      console.log('gradeValue');
      console.log(gradeValue);
    const data = await window.DApp.post(
      `inventory/rewards`,
      {
        collection: filter,
        tokenId: selectedItem?.tokenId || 0,
        grade: gradeValue,
        server_tag : sessionStorage.getItem("server_tag"),
        user_no: sessionStorage.getItem("user_no"),
      }
    );
    setRewards(data);
  };
  useEffect(() => {
    if (!selectedItem) return;
    fetchReward();
  }, [selectedItem]);

    let displayDescription = selectedItem?.description || "";

    if (selectedItem) {
        switch (filter) {
            case "gemblock":
                displayDescription = "Players can obtain Gem Block items for use in their in-game equipment.";
                break;
            case "passive":
                displayDescription = "Players can receive materials in the game that can be used to acquire Passive Skill Options.";
                break;
            case "goldbox":
                displayDescription = "The first time you claim the reward, you receive a buff. Upon subsequent claims, you can choose to receive either a Dragon Ticket or a Pet Ticket.";
                break;
            case "petticket":
                displayDescription = "This ticket can be exchanged 1:1 for a PET to be used in MTDZ The Game.";
                break;
            case "mts":
                displayDescription = "This is an in-game currency that grants a random Gem Block.";
                break;
            case "metafigure":
                displayDescription = "An item that can summon a miner is provided.";
                break;
            default:
                displayDescription = selectedItem.description;
                break;
        }
    }

  return (
    <div className={styles.container}>
        <>
          {selectedItem ? (
            <>
              <div className={styles.topInfoBox}>
                <div className={styles.topInfo}>
                  <div className={styles.topInfoImg}>
                    <img
                      className={
                        filter == "metafigure" || filter == "gemblock"
                          ? styles.fullImage
                          : styles.fillImage
                      }
                      src={selectedItem.image}
                      alt="dataList"
                    />
                  </div>
                  <div className={styles.topInfoTitle}>
                    <div className={styles.topInfoWrap}>
                      <div className={styles.topInfoTitleTop}>
                        {selectedItem.name}
                      </div>
                      <div className={styles.topInfoTitleBottom}>
                        {/*{selectedItem.description}*/}
                          {displayDescription}
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  className={styles.topInfoButton}
                  onClick={() => {
                    if (!rewards) return;
                    openPopup({
                      type: "NFTInventory",
                      data: { ...rewards, selectedItem, fullList },
                    });
                  }}
                >
                  Apply
                </div>
                {filter === 3 && (
                  <div
                    className={styles.topInfoButton}
                    onClick={() => openPopup({ type: "NFTInventory", data: 9 })}
                  >
                    다른 유형 팝업
                  </div>
                )}
              </div>
              <NFTInventoryLeftDetail selected="true" rewards={rewards} />
            </>
          ) : (
            <>
              <div className={styles.topInfoBox}>
                <div className={styles.topInfo}>
                  <div className={styles.topInfoTitle_noWallet}>
                    <div className={styles.topInfoTitle_noWallet}>
                      Item not selected
                    </div>
                    <div
                      className={`${styles.topInfoTitleBottom_noWallet} font-b4-r`}
                    >
                      Select an item to view more information.
                    </div>
                  </div>
                </div>
                <div className={` ${styles.topInfoButton_noWallet} opacity-50`}>
                  Apply
                </div>
              </div>
              <NFTInventoryLeftDetail selected="false" />
            </>
          )}
        </>
    </div>
  );
}
