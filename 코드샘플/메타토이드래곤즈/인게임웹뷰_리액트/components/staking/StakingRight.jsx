import styles from "../../styles/StakingRight.module.css";
import StakingRightTopFilter from "./StakingRightTopFilter";
import StakingRightBottomListData from "./StakingRightBottomListData";
import StakingRightBottomButton from "./StakingRightBottomButton";
import { ReactComponent as QuestShield } from "../../assets/svg/common/quest_shield.svg";
import PullToRefresh from "react-simple-pull-to-refresh";
import { useSelector, useDispatch } from "react-redux";
import { fetchStakingData } from "../../data/stakingSlice";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
export default function StakingRight() {
  const stakingStats = useSelector((state) => state.staking.stakingStats);

  const dispatch = useDispatch();

  const masterRank = () => {
    return stakingStats?.mission?.rank_over_master === true;
  };

  const dailyMission = () => {
    return stakingStats?.mission?.daily_quest_complate === true;
  };

  const handleRefresh = () => {
    return new Promise(async (resolve, reject) => {
      await dispatch(fetchStakingData());
      resolve(true);
    });
  };

  return (
    <>
      <PullToRefresh onRefresh={handleRefresh}>
        <div className={styles.container}>
          {!masterRank() && (
            <div className={styles.warning}>
              <div>
                <QuestShield fill="#F90052" style={{ width: "18px" }} />
              </div>
              <div>
                Claim requires a Master or higher rank in the previous
                season's arena.
              </div>
            </div>
          )}
          {!dailyMission() && (
            <div className={styles.warning}>
              <div>
                <QuestShield fill="#F90052" style={{ width: "18px" }} />
              </div>
              <div>Complete all daily missions to claim</div>
            </div>
          )}
          <StakingRightTopFilter />
          <div style={{ position: "relative", padding: "0 1rem" }}>
            <StakingRightBottomListData />
          </div>
          {/* {
          data.user?.userInfo.wallet_addr 
          && <StakingRightBottomButton data={userData} dummy={data}/>
        } */}
        </div>
      </PullToRefresh>
      <StakingRightBottomButton/>
    </>
  );
}
