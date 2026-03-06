import { useState, memo } from "react";
import styles from "../../styles/MetodiumShopRightTopFilter.module.css";
import { useSelector, useDispatch } from "react-redux";
import { MetodiumShopFilter, MetodiumShopFilterKR } from "../../data/data";
import { setFilter } from "../../data/shopSlice";
import { setIsActive } from "../../data/shopSlice";

function MetodiumShopRightTopFilter(props) {
  //필터별 갯수 각 몇개인지 받아와야함
  const [clickedFilter, setClickedFilter] = useState([0]);
  const dispatch = useDispatch();

  const handleFilterClick = (index) => {
    if (!clickedFilter.includes(index)) {
      setClickedFilter([]);
      setClickedFilter((clickedFilter) => {
        return [...clickedFilter, index];
      });
      dispatch(setFilter(index));
      dispatch(setIsActive(null));
    }
  };

  //console.log(isActive);

  return (
    <div className={styles.filterTop}>
      <ul className={styles.filterTopList}>
        { (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? MetodiumShopFilterKR : MetodiumShopFilter).map((_, index) => (
          <li
            className={`${
              clickedFilter.includes(index) ? styles.active : ""
            }  font-b3-b`}
            key={index}
            onClick={(e) => handleFilterClick(index)}
          >
            { navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? MetodiumShopFilterKR[index] : MetodiumShopFilter[index] }
          </li>
        ))}
      </ul>
    </div>
  );
}

export default memo(MetodiumShopRightTopFilter);
