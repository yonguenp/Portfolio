import { useState, memo, useRef } from "react";
import styles from "../../styles/StakingRightTopFilter.module.css";
import { useSelector, useDispatch } from "react-redux";
import { StakingFilter } from "../../data/data";
import { setFilter, setSelectAll, setIsActive } from "../../data/stakingSlice";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import FilterTab from "./filter/FilterTabs";

function StakingRightTopFilter() {
  const { userInfo } = useDAppState();

  const stakingData = useSelector(
    (state) => state.staking,
    (l, r) =>
      l.stakingData.length == r.stakingData.length &&
      l.selectedStakingData == r.selectedStakingData
  );
  const listDataArray = stakingData.stakingData;

  //const [checkedSelectAll, setCheckedSelectAll] = useState(stakingData.selectedAll);

  const dispatch = useDispatch();

  const handleFilterClick = (index) => {
    if (!userInfo?.addr) return alert("Wallet is Not Connected");
    // dispatch(setSelectAll({ selectAll: false, selectedStakingData: [] }));
    dispatch(setFilter(index));
    //dispatch(setIsActive());
  };

  const handleCheckAll = (arr) => {
    dispatch(setSelectAll());
  };

  return (
    <div className={`${styles.filterTop} ${styles.makeitsticky}`}>
      <div className={styles.filterTopBox}>
        {/* <ul className={styles.filterTopList}>
          {StakingFilter.map((v, index) => (
            <li
              className={`${
                stakingData.filterMenu == StakingFilter.indexOf(v)
                  ? styles.active
                  : ""
              } font-b3-b`}
              key={index}
              onClick={(e) => handleFilterClick(StakingFilter.indexOf(v))}
            >
              {StakingFilter[index]}(
              {
                stakingData.stakingData.filter((data) => {
                  return data.isInserted == StakingFilter.indexOf(v);
                }).length
              }
              )
            </li>
          ))}
        </ul> */}
        <FilterTab />

        {
          <div className={`${styles.filterTopRight}  font-b3-b`}>
            <input
              type="checkBox"
              id="all"
              className={styles.filterTopCheckBox}
              onChange={() => handleCheckAll(listDataArray)}
              checked={
                stakingData.selectedStakingData.length ==
                stakingData.stakingData.filter(
                  (v) => v.isInserted == stakingData.filterMenu
                ).length
                  ? true
                  : false
              }
            />
            <div className={`${styles.filterTopSelectAllText} gray-500`}>
              <label htmlFor="all">Select All</label>
            </div>
          </div>
        }
      </div>
    </div>
  );
}

export default memo(StakingRightTopFilter);
