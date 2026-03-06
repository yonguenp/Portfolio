import { useDispatch, useSelector } from "react-redux";
import styles from "../../../styles/StakingRightBottomListData.module.css";
import { setIsActive } from "../../../data/stakingSlice";
import { ReactComponent as Locked } from "../../../assets/svg/staking/lock.svg";
import { useEffect } from "react";
export default function WaitingNFTCard({ item }) {
  const date = new Date(item?.deposit_at);
  return (
    <div className={`${styles.listBottomListDetail}`} onClick={() => {}}>
      <img
        src={item.image}
        style={{
          width: "68px",
        }}
        alt="dataList"
      />

      <div
        className={`${styles.stakingDetail} ${styles.blur} white`}
        style={{
          backdropFilter: "blur(3px)",
        }}
      >
        <div className={styles.stakingDetailContent}>
          <div
            className={`${styles.stakingDetailNumber} font-b4-b mb-16 flex align-center justify-start gap-4`}
          >
            {/*<div>{item.name}</div>*/}
            <div>{item.name.slice(4)}</div>
            <div>
              <svg
                className={styles.spinner1}
                width="10"
                height="10"
                viewBox="0 0 10 10"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M5 1C7.20916 1 9 2.79086 9 5C9 7.20916 7.20916 9 5 9C2.79086 9 1 7.20916 1 5C1 2.79086 2.79086 1 5 1Z"
                  stroke="white"
                  strokeOpacity="0.2"
                  strokeWidth="2"
                  strokeLinecap="round"
                />
                <path
                  d="M5 1C7.20916 1 9 2.79086 9 5"
                  stroke="white"
                  strokeWidth="2"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          </div>
          <div className={`${styles.stakingDetailDate} text-start font-b6-r`}>
            {`${date.getFullYear()}.${date.getMonth() + 1}.${date.getDate()}`}
          </div>
          <div className={`${styles.stakingDetailTime} text-start font-b6-r`}>
            {item.deposit_at.split(" ")[1]}
          </div>
        </div>
      </div>
    </div>
  );
}
