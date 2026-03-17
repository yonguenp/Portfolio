import { useState, memo } from "react";
import styles from "../../styles/NFTInventoryRightTopFilter.module.css";
import { useSelector, useDispatch } from "react-redux";
import { NFTFilter, NFTFilters } from "../../data/data";
import { setFilter, thunkSetFilter } from "../../data/nftSlice";
import { setIsActive } from "../../data/nftSlice";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

function NFTInventoryRightTopFilter() {
  //필터별 갯수 각 몇개인지 받아와야함

  //스테이킹과 마찬가지로 불필요한 state는 제거.
  const [clickedFilter, setClickedFilter] = useState([0]);
  const { userInfo } = useDAppState();
  const dispatch = useDispatch();
  const invenFilter = useSelector((state) => state.nftInventory.filter);
  const fullList = useSelector((state) => state.nftInventory.fullList);

  const handleFilterClick = (index) => {
    //if (!clickedFilter.includes(index)) {
    //   setClickedFilter([]);
    //   setClickedFilter((clickedFilter) => {
    //     return [...clickedFilter, index];
    //   });

    dispatch(thunkSetFilter(index));

    //불필요한 dispatch 삭제하여 복잡성 제거
    //  dispatch(setIsActive(null));
    //}
  };

  //console.log(isActive);

  return (
    <div className={styles.filterTop}>
      <ul className={styles.filterTopList}>
        {/**
         * 코드 단축 하기 위해 변경
         */
        /* userInfo?.addr
          ? NFTFilter.map((_, index) => (
              <li
                className={`${
                  invenFilter === index ? styles.active : ""
                } font-b3-b`}
                key={index}
                onClick={(e) => handleFilterClick(index)}
              >
                {NFTFilter[index]}(999)
              </li>
            ))
          : NFTFilter.map((_, index) => (
              <li
                className={`${index === 0 ? styles.active : ""}  font-b3-b`}
                key={index}
                onClick={() => alert("Wallet is Not Connected")}
              >
                {NFTFilter[index]}(000)
              </li>
            ))} */}
        {NFTFilters.map((item, index) => (
          <li
            className={`${
              item.key === invenFilter ? styles.active : ""
            }  font-b3-b`}
            key={index}
            onClick={(e) => handleFilterClick(item.key)}
          >
            {`${item.name} (${fullList?.items[item.key]?.count || 0})`}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default memo(NFTInventoryRightTopFilter);
