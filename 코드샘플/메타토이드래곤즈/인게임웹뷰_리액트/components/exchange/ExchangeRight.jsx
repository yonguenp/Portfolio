import { useSelector } from "react-redux";
import styles from "../../styles/ExchangeRight.module.css";
import ExchangeRightTop from "./ExchangeRightTop";
import ExchangeRightBottom from "./ExchangeRightBottom";
import { ReactComponent as QuestShield } from "../../assets/svg/common/quest_shield.svg";
import { useEffect, useState } from "react";
import BigNumber from "bignumber.js";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function ExchangeRight({ userState, infoData, onRefresh }) {
  const { userInfo } = useDAppState();
  const [count, setCount] = useState(0);
  const [expectedAmount, setExpectedAmount] = useState(0);
  const [disabledSmelt, setDisabledSmelt] = useState(false);

  const per_magnet = parseInt(infoData?.forge_swap_info?.[userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank]);

  const maxAmount = Math.max(new BigNumber(userState?.curr_level_info?.limit || 0)
    .minus(userState.today_exchange)
    .multipliedBy(
      new BigNumber(infoData?.forge_swap_info?.[userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank])
    )
    .decimalPlaces(0)
    .toNumber(), 0);
    
  const maxAvail =
    maxAmount < Math.floor(userState?.unlocked_magnet || 0)
      ? maxAmount
      : userState?.unlocked_magnet - (userState?.unlocked_magnet % per_magnet);

  useEffect(()=> {
    setCount(0);
  }, [userInfo])

  useEffect(() => {
    setDisabledSmelt(
      userInfo?.magnet?.unlocked < count ||
        !userState?.daily_quest ||
        (count < 10 && count > 0) ||
        count > maxAmount
    );

    let tmp_count = new BigNumber(count);
    tmp_count.dividedBy(
      new BigNumber(infoData?.forge_swap_info?.[userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank])
    );
    setExpectedAmount(
      Number(
        tmp_count
          .dividedBy(
            new BigNumber(infoData?.forge_swap_info?.[userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank])
          )
          .decimalPlaces(0, BigNumber.ROUND_DOWN)
      )
    );
  }, [count]);
  
  return (
    <>
      {
      !userState?.daily_quest ? (
        <div className={styles.warning}>
          <div>
            <QuestShield fill="#F90052" style={{ width: "18px" }} />
          </div>
          <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "모든 일일 임무를 완료하면 제련이 잠금 해제됩니다." : "Complete all daily missions to unlock smelting."}</div>
        </div>
      ) : 
      per_magnet > 0 && (count % per_magnet) != 0 && (
        <div className={styles.warning}>
          <div>
            <QuestShield fill="#F90052" style={{ width: "18px" }} />
          </div>
          <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "올바른 값을 입력해 주세요." : "Please enter the correct value."}</div>
        </div>
      )
      }
      <div className={styles.container}>
        <ExchangeRightTop
          userState={userState}
          disabledSmelt={disabledSmelt || count == 0}
          expectedAmount={expectedAmount}
        />
        <ExchangeRightBottom
          userState={userState}
          count={count}
          setCount={setCount}
          disabledSmelt={disabledSmelt || count == 0 || userInfo?.magnet?.unlocked < count ||
            !userState?.daily_quest ||
            (count < 10 && count > 0) ||
            count > maxAvail
          }
          maxAmount={maxAvail}
          per_magnet={per_magnet}
          onRefresh={onRefresh}
        />
      </div>
    </>
  );
}
