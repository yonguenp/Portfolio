import { useDispatch, useSelector } from "react-redux";
import styles from "../../../styles/StakingRightBottomListData.module.css";
import { setIsActive } from "../../../data/stakingSlice";
import { ReactComponent as Locked } from "../../../assets/svg/staking/lock.svg";
import { useEffect } from "react";
export default function NFTCard({ item }) {
  const dispatch = useDispatch();
  const selectedItems = useSelector(
    (state) => state.staking.selectedStakingData
  );

  const handleClick = () => {
    if (item.selectable) dispatch(setIsActive(item.iv_id));
  };
  const date = new Date(item?.deposit_at);
  return (
    <div
      className={`${styles.listBottomListDetail}`}
      onClick={() => {
        handleClick();
      }}
    >
      <img
        src={item.image}
        style={{
          width: "68px",
        }}
        alt="dataList"
      />

      {item?.isInserted == 1 ? (
        <div
          className={`${styles.stakingDetail} white ${
            selectedItems.includes(item.iv_id) ? styles.active : ""
          }`}
        >
          <div className={styles.stakingDetailContent}>
            <div
              className={`${styles.stakingDetailNumber} font-b4-b flex align-center justify-between gap-2`}
            >
              {/*<div>{item.name}</div>*/}
              <div>{item.name.slice(4)}</div>
              <div>{!item.withdrawable && <Locked fill="white" />}</div>
            </div>
            <div>
              <div
                className={`${styles.stakingDetailDate} text-start font-b6-r`}
              >
                {`${date.getFullYear()}.${date.getMonth() + 1}.${date.getDate()}`}
              </div>
              <div
                className={`${styles.stakingDetailTime} text-start font-b6-r`}
              >
                {item.deposit_at.split(" ")[1]}
              </div>
            </div>
          </div>
        </div>
      ) : (
        <div
          className={`${styles.stakingDetail} ${
            selectedItems.includes(item.iv_id)
              ? styles.active
              : styles.noOpacityBg
          }`}
        ></div>
      )}
    </div>
  );
}
