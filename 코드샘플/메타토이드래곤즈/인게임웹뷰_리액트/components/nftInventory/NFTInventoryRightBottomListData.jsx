import styles from "../../styles/NFTInventoryRightBottomListData.module.css";
import { useSelector, useDispatch } from "react-redux";
import { setIsActive } from "../../data/nftSlice";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { NFTFilters } from "../../data/data";
import { useEffect, useState, useRef } from "react";
export default function NFTInventoryRightBottomListData(props) {
  const dummyArray = Array(20).fill(null);
  const dispatch = useDispatch();
  const { userInfo } = useDAppState();
  const selectedItem = useSelector((state) => state.nftInventory.selectedItem);
  const fullList = useSelector((state) => state.nftInventory.fullList);
  const NFTFilter = useSelector((state) => state.nftInventory.filter);
  const [gap, setGap] = useState(6);
  const ref = useRef(null);

  const handleClick = (index) => {
    dispatch(setIsActive(index));
  };
  useEffect(() => {
    if (ref)
      setGap(
        (
          (ref?.current?.clientWidth % 68) /
          (Math.floor(ref?.current?.clientWidth / 68) - 1)
        ).toFixed(2)
      );
  }, [NFTFilter, fullList]);
  return (
    <>
      {dummyArray.length > 0 ? (
        <div className={styles.listBottom}>
          <div
            className={styles.listContainer}
            ref={ref}
            style={{ gap: `${gap}px` }}
          >
            {true ? (
              (fullList?.items?.[NFTFilter]?.list || []).map((item, index) => (
                <div
                  className={`
                    ${styles.listBottomListDetail}                  
                    ${
                      selectedItem?.iv_id === item.iv_id
                        ? styles.active
                        : ""
                    }
                  `}
                  key={index + 1}
                  onClick={() => {
                    handleClick(item);
                  }}
                >
                  <img
                    src={item.image}
                    alt="dataList"
                    style={{
                      width: "56px",
                    }}
                  />
                </div>
              ))
            ) : (
              <div className={`${styles.listBottomNoData} font-b3-r gray-800`}>
                No information to display.
              </div>
            )}
          </div>
        </div>
      ) : (
        <div className={`${styles.listBottomNoData} font-b3-r gray-800`}>
          No information to display.
        </div>
      )}
    </>
  );
}
