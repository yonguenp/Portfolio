import { useDispatch, useSelector } from "react-redux";
import { StakingFilter } from "../../../data/data";
import styles from "../../../styles/StakingRightTopFilter.module.css";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";
import { setFilter } from "../../../data/stakingSlice";
export default function FilterTab() {
  const dispatch = useDispatch();
  const filterMenu = useSelector((state) => state.staking.filterMenu);
  const count = useSelector((state) => state.staking.count);
  //l.filterMenu == r.filterMenu
  const { userInfo } = useDAppState();
  const handleFilterClick = (index) => {
    if (!userInfo?.user_no) return alert("Wallet is Not Connected");
    // dispatch(setSelectAll({ selectAll: false, selectedStakingData: [] }));
    dispatch(setFilter(index));
    //dispatch(setIsActive());
  };
  return (
    <ul className={styles.filterTopList}>
      {StakingFilter.map((v, index) => (
        <li
          className={`${
            filterMenu == StakingFilter.indexOf(v) ? styles.active : ""
          } font-b3-b`}
          key={index}
          onClick={(e) => handleFilterClick(StakingFilter.indexOf(v))}
        >
          {StakingFilter[index]}({count[index]})
        </li>
      ))}
    </ul>
  );
}
