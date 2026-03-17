import styles from "../../styles/StakingLeft.module.css";
import { useSelector } from "react-redux";

import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";
import { ReactComponent as Arrow } from "../../assets/svg/common/arrow(swap).svg";

import { usePopup } from "../../context/PopupContext";
import { memo } from "react";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { ARENA_TIER_STR } from "../../data/data";

function StakingLeft() {
  const { userInfo } = useDAppState();
  const { stakingData, stakingStats } = useSelector(
    (state) => state.staking,
    (l, r) => l.stakingStats == r.stakingStats
  );

  const stakingCardCount = stakingData.length;

  const { openPopup } = usePopup();

  return (
    <div style={{}}>
        <div className={`${styles.extractorBox} bg-gray-900`}>
          <div className="mb-8 font-b3-b">My Reward</div>
          <div className={`${styles.extractorInnerBox} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox}`}>
              <div className="font-b4-r green-500 mb-4">Estimated Rewards</div>
              <div className={`${styles.rewardBoxTop}`}>
                <img
                  src="/images/icon/magnite.png"
                  alt="metod"
                  style={{ width: "24px", height: "24px" }}
                />
                <div className="font-b1-b">
                  {stakingCardCount > 0 ? stakingStats.expected_curr : 0}
                </div>
              </div>
              <div className={`${styles.rewardBoxBottom} font-b4-r`}>
                <div className={`${styles.rewardBoxButtons} bg-gray-800`}>
                  <div className="ml-8 gray-500">Base</div>
                  <div className="ml-8">
                    {stakingCardCount > 0
                      ? `+${stakingStats?.expected_detail?.base || 0.0}`
                      : 0}
                  </div>
                </div>
                <div className={`${styles.rewardBoxButtons} bg-gray-800`}>
                  <div className="ml-8 gray-500">Bonus</div>
                  <div className="ml-8">
                    {stakingCardCount > 0
                      ? `+${stakingStats?.expected_detail?.bonus || 0.0}`
                      : 0}
                  </div>
                </div>
              </div>
              <button
                disabled={stakingStats?.state != "wait_claim"}
                className={`${styles.extractorBoxBottom} white bg-red-400 font-b3-b`}
              >
                {stakingStats?.state == "earning" ? "Claim" : "Claim"}
              </button>
            </div>
          </div>
          {/* <div className={`${styles.extractorBoxCenter} bg-gray-900`}>
                  <div className={`${styles.extractorBoxTitle} font-b4-r green-500`}>Estimated Extraction</div>
                  <div className={`${styles.extractorBoxContent} font-b2-b`}>                    
                    <img src="/images/icon/magnite.png" alt="metod" style={{width:'24px', height:'24px'}}/>                      
                    <div>30.568</div>
                  </div>
                </div> */}
        </div>

        <div className={`${styles.extractorBox} bg-gray-900`}>
          <div className="mb-8 font-b3-b">Status</div>
          <div className={`${styles.statusInnerBox} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox}`}>
              <div className="font-b4-r green-500 mb-4 flex gap-2 align-center">
                Inserted Cards
                <QuestionBig
                  onClick={() =>
                    openPopup({ type: "GenesisCardManual", data: "card" })
                  }
                />
              </div>

              {stakingCardCount > 0 ? (
                <div className="flex justify-center align-center">
                  <div
                    className={`${styles.stakingTopBoxGrade} ${styles.stakingDetail} white`}
                  >
                    <div className="mb-4 gray-500">Grade</div>
                    <div className="mb-4 ">Legendary</div>
                    <div className="mb-4 ">Unique</div>
                    <div className="mb-4 ">Rare</div>
                    <div className="mb-4 ">Uncommon</div>
                    <div className="">Common</div>
                  </div>
                  <div
                    className={`${styles.stakingTopBoxQty} ${styles.stakingDetail} text-start`}
                  >
                    <div className="mb-4 gray-500">Qty.</div>
                    <div className="mb-4">
                      {stakingStats?.expected_detail?.["L"].count || 0}
                    </div>
                    <div className="mb-4">
                      {stakingStats?.expected_detail?.["U"].count || 0}
                    </div>
                    <div className="mb-4">
                      {stakingStats?.expected_detail?.["S"].count || 0}
                    </div>
                    <div className="mb-4">
                      {stakingStats?.expected_detail?.["R"].count || 0}
                    </div>
                    <div className="">
                      {stakingStats?.expected_detail?.["N"].count || 0}
                    </div>
                  </div>
                  <div
                    className={`${styles.stakingTopBoxReward} ${styles.stakingDetail} text-start`}
                  >
                    <div className="mb-4 gray-500">
                      Reward<span className="font-b6-r">(6H)</span>
                    </div>
                    <div className={`mb-4 ${styles.metodiumBox} text-end`}>
                      <img
                        src="/images/icon/magnite.png"
                        alt="metod"
                        style={{ width: "16px", height: "16px" }}
                      />
                      <div>
                        {stakingStats?.expected_detail?.["L"].amount || 0}
                      </div>
                    </div>
                    <div className={`mb-4 ${styles.metodiumBox} text-end`}>
                      <img
                        src="/images/icon/magnite.png"
                        alt="metod"
                        style={{ width: "16px", height: "16px" }}
                      />
                      <div>
                        {stakingStats?.expected_detail?.["U"].amount || 0}
                      </div>
                    </div>
                    <div className={`mb-4 ${styles.metodiumBox} text-end`}>
                      <img
                        src="/images/icon/magnite.png"
                        alt="metod"
                        style={{ width: "16px", height: "16px" }}
                      />
                      <div>
                        {stakingStats?.expected_detail?.["S"].amount || 0}
                      </div>
                    </div>
                    <div className={`mb-4 ${styles.metodiumBox} text-end`}>
                      <img
                        src="/images/icon/magnite.png"
                        alt="metod"
                        style={{ width: "16px", height: "16px" }}
                      />
                      <div>
                        {stakingStats?.expected_detail?.["R"].amount || 0}
                      </div>
                    </div>
                    <div className={`${styles.metodiumBox} text-end`}>
                      <img
                        src="/images/icon/magnite.png"
                        alt="metod"
                        style={{ width: "16px", height: "16px" }}
                      />
                      <div>
                        {stakingStats?.expected_detail?.["N"].amount || 0}
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="font-b4-r">Cards not inserted yet.</div>
              )}
            </div>
          </div>
          <div className={`${styles.statusInnerBox} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox} mb-8`}>
              <div className="font-b4-r green-500 mb-2 flex gap-2 align-center">
                {`Season ${
                  userInfo?.user?.arena?.season || "-"
                } Arena Rank Bonus`}
                <QuestionBig
                  onClick={() =>
                    openPopup({ type: "GenesisCardManual", data: "arena" })
                  }
                />
              </div>
              <div className={`${styles.statusBouseBox}`}>
                <div className={`${styles.statusSeasonRank} font-b4-r`}>
                  {userInfo?.user?.arena?.rank < 100
                    ? `${ARENA_TIER_STR[userInfo?.user?.arena?.rank]}`
                    : "None"}
                </div>
                <div className={`${styles.metodiumBox} font-b4-r`}>
                  <img
                    src="/images/icon/magnite.png"
                    alt="metod"
                    style={{ width: "16px", height: "16px" }}
                  />
                  <div>
                    {stakingCardCount > 0
                      ? stakingStats?.expected_detail?.arena
                      : "0"}
                  </div>
                </div>
              </div>
            </div>

            <div className={`${styles.rewardBox}`}>
              <div className="font-b4-r green-500 mb-2  flex gap-2 align-center">
                Boss Raid Rank Bonus
                <QuestionBig
                  onClick={() =>
                    openPopup({ type: "GenesisCardManual", data: "boss" })
                  }
                />
              </div>
              <div className={`${styles.statusBouseBox}`}>
                <div className={`${styles.statusBossRank} font-b4-r`}>
                  {stakingCardCount > 0
                    ? `${userInfo?.user?.boss?.rank}`
                    : "None"}
                </div>
                <div className={`${styles.metodiumBox}`}>
                  <img
                    src="/images/icon/magnite.png"
                    alt="metod"
                    style={{ width: "16px", height: "16px" }}
                  />
                  <div>
                    {stakingCardCount > 0
                      ? stakingStats?.expected_detail?.boss
                      : "0"}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
    </div>
  );
}

export default memo(StakingLeft);
