import styles from "../../styles/StakingLeft.module.css";
import { useSelector } from "react-redux";

import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";
import { useState, useEffect } from 'react';
import { usePopup } from "../../context/PopupContext";
import { memo } from "react";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { ARENA_TIER_STR, GRADE_STR } from "../../data/data";

const rules = new Intl.PluralRules('en-US', { type: 'ordinal' });
function formatRank(rank) {
  // null, 0, "no data", undefined 등의 값을 먼저 처리합니다.
  if (!rank || typeof rank !== 'number' || rank <= 0) {
    return 'no data';
  }

  const category = rules.select(rank);
  switch (category) {
    case 'one':
      return `${rank}st`;
    case 'two':
      return `${rank}nd`;
    case 'few':
      return `${rank}rd`;
    default:
      return `${rank}th`;
  }
}

export default function StakingLeft() {
  const { userInfo } = useDAppState();
  const [userRankInfo, setUserRankInfo] = useState(null);
  const [timeVal, setTimeVal] = useState(null);

  const { stakingData, stakingStats, daily_limit, daily_claimed } = useSelector(
    (state) => ({
      ...state.staking,
    }),
    (l, r) => l.stakingStats == r.stakingStats && l.stakingData === r.stakingData,
  );

  const stakingCardCount = stakingData.length;

  const { openPopup } = usePopup();

  const calRemain = (interval) => {
    const now = Math.round((new Date()).getTime() / 1000); 
  
    const refreshTime = new Date();
    refreshTime.setHours(24, 0, 0, 0); 
    const refreshTimestamp = Math.round(refreshTime.getTime() / 1000); 
  
    let remain = refreshTimestamp - now;
    const hours = Math.floor(remain / 3600); 
    const minutes = Math.floor((remain % 3600) / 60);
    const seconds = (remain % 3600) % 60; 
  
    let timeString = (hours < 10 ? '0' + hours : hours) + ':' + (minutes < 10 ? '0' + minutes : minutes) + ':' + (seconds < 10 ? '0' + seconds : seconds);

    if (remain <= 0) {
        timeString = "00:00:00";
        clearInterval(interval);
    }

    setTimeVal(timeString);
  }

  useEffect(() => {
    const userNo = sessionStorage.getItem("user_no");

    // 2. 가드 클로즈: userInfo가 없거나, userNo가 없거나, "null" 문자열이면 즉시 리턴
    //    이것이 NFTInventoryLeft의 if (!selectedItem) return; 과 동일한 역할입니다.
    if (!userInfo || !userNo || userNo === "null" || userNo === "undefined") {
      return;
    }

    const fetchUserRankInfo = async () => {
      try {
        // 새로운 API 엔드포인트를 호출합니다. (URL은 실제 경로에 맞게 수정 필요)
        const rankData = await window.DApp.post(`staking/getrankinfo`, {
          user_no: sessionStorage.getItem("user_no"),
          server_tag: sessionStorage.getItem("server_tag"),
        });

        console.log('rankData');
        console.log(rankData);

        if (rankData && rankData.rs === 0) {
          setUserRankInfo(rankData.list);
        }
      } catch (error) {
        console.error("Failed to fetch user rank info:", error);
      }
    };

    fetchUserRankInfo();

    const interval = setInterval(calRemain, 1000);
    calRemain(interval);

    return () => clearInterval(interval);
  }, [userInfo]);

  //"Pending" 행 데이터 계산 (state: 1 = waiting)
  const pendingCards = stakingData.filter(v => v.state === 1);
  const pendingTotal = pendingCards.length;
  // (Pending은 정의상 'Locked' 상태입니다)
  const pendingLocked = pendingTotal;
  const pendingUnlocked = 0;

  //"Available" 행 데이터 계산 (state: 2 = free)
  const availableCards = stakingData.filter(v => v.state === 0);
  const availableTotal = availableCards.length;
  // (Locked/Unlocked은 'withdrawable' 플래그를 기준으로 계산합니다)
  const availableLocked = availableCards.filter(v => v.withdrawable === false).length;
  const availableUnlocked = availableCards.filter(v => v.withdrawable === true).length;

  return (
    <div style={{}}>
        <div className={`${styles.extractorBox} bg-gray-900`}>
          <div className="mb-8 font-b3-b">Current Reward</div>
          <div className={`${styles.extractorInnerBox} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox}`}>
              <div className="font-b4-r green-500 mb-4">Estimated Rewards</div>
              <div className={`${styles.rewardBoxTop}`}>
                <img
                  src="/images/icon/magnetblock.png"
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
                      ? `+${stakingStats?.expected_curr_base || 0.0}`
                      : 0}
                  </div>
                </div>
                <div className={`${styles.rewardBoxButtons} bg-gray-800`}>
                  <div className="ml-8 gray-500">Bonus</div>
                  <div className="ml-8">
                    {stakingCardCount > 0
                      ? `+${(() => {
                        const diff = Number(stakingStats.expected_curr) - (Number(stakingStats?.expected_curr_base ?? 0.0));
                        return diff === 0 ? 0 : diff.toFixed(0) || 0.0;
                    })()}` : 0}
                  </div>
                </div>
              </div>
              <button
                disabled={stakingStats?.state != "wait_claim" || userRankInfo?.arena_rank < 117 || !userRankInfo?.daily_quest
                }
                className={`${styles.extractorBoxBottom} white bg-red-400 font-b3-b`}
                onClick={() =>
                  openPopup({
                    type: "GenesisClaim",
                    data: stakingStats.expected_curr,
                  })
                  // daily_limit - (daily_claimed + Number(stakingStats?.expected_curr || 0)) > 0 ?
                  // openPopup({
                  //   type: "GenesisClaim",
                  //   data: stakingStats.expected_curr,
                  // })
                  // :
                  // openPopup({
                  //   type: "ForceClaim",
                  //   data:  daily_limit - daily_claimed,
                  // })
                }
              >
                {stakingStats?.state == "earning" && stakingStats?.end_at ? stakingStats.end_at : "Claim" }
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
          <div className="mb-8 font-b3-b">Expected Reward</div>
          {/*<div className={`${styles.statusInnerBox} bg-gray-900 font-b4-r`}>*/}
          {/*  <div className={`${styles.rewardBox} mb-8`}>*/}
          {/*    <div className="font-b4-r green-500 mb-4 flex gap-2 align-center">*/}
          {/*      Daily Vault*/}
          {/*      <QuestionBig*/}
          {/*        onClick={() =>*/}
          {/*          openPopup({ type: "ClaimManual"})*/}
          {/*        }*/}
          {/*      />*/}
          {/*    </div>*/}
          {/*    <section*/}
          {/*        className="flex justify-center align-center"*/}
          {/*        style={{*/}
          {/*          display: "flex",*/}
          {/*          flexDirection: "row",*/}
          {/*          gap: "2px",*/}
          {/*        }}*/}
          {/*      >*/}
          {/*        <ul className={`${styles.rows} `}>*/}
          {/*          <li className={styles.dataRow}>*/}
          {/*            <p className={`${styles.daily_limit} mb-4 gray-500`}>Balance</p>*/}
          {/*            <p className={`${styles.daily_claimed} mb-4 gray-500`}>Claimed</p>*/}
          {/*            <p className={`${styles.daily_refresh} mb-4 gray-500`}>Refresh</p>*/}
          {/*          </li>*/}
          {/*          <li className={styles.dataRow}>*/}
          {/*            <img*/}
          {/*              src="/images/icon/magnetblock.png"*/}
          {/*              alt="metod"*/}
          {/*              style={{ width: "16px", height: "16px" }}*/}
          {/*            />*/}
          {/*            <p className={styles.daily_limit}>{daily_limit} </p> */}
          {/*            <img*/}
          {/*              src="/images/icon/magnetblock.png"*/}
          {/*              alt="metod"*/}
          {/*              style={{ width: "16px", height: "16px" }}*/}
          {/*            />*/}
          {/*            <p className={styles.daily_claimed}>*/}
          {/*              {daily_claimed} */}
          {/*            </p>*/}
          {/*            <p className={styles.daily_refresh}>*/}
          {/*              {timeVal}*/}
          {/*            </p>*/}
          {/*          </li>*/}
          {/*        </ul>*/}
          {/*      </section>*/}
          {/*  </div>*/}
          {/*</div>*/}
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
                <section
                  className="flex justify-center align-center"
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "2px",
                  }}
                >
                  <ul className={`${styles.rows} `}>
                    <li className={styles.dataRow}>
                      <p className={`${styles.grade} mb-4 gray-500`}>Grade</p>
                      <p className={`${styles.qty} mb-4 gray-500`}>Qty.</p>
                      <p className={`${styles.reward} mb-4 gray-500`}>
                        Reward<span className="font-b6-r">(6H)</span>
                      </p>
                    </li>
                    {["L", "U", "S", "R", "N"].map((v) => {
                      if (stakingStats?.expected_detail?.[v]?.count > 0)
                        return (
                          <li key={v} className={styles.dataRow}>
                            <p className={styles.grade}>{GRADE_STR[v]}</p>
                            <p className={styles.qty}>
                              {stakingStats?.expected_detail?.[v].count || 0}
                            </p>
                            <p className={styles.reward}>
                              <img
                                src="/images/icon/magnetblock.png"
                                alt="metod"
                                style={{ width: "16px", height: "16px" }}
                              />
                              {stakingStats?.expected_detail?.[v].amount?.toFixed(0) || 0}
                            </p>
                          </li>
                        );
                    })}
                  </ul>
                </section>
              ) : (
                <div className="font-b4-r">Cards not inserted yet.</div>
              )}
            </div>
          </div>
          <div className={`${styles.statusInnerBox} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox}`}>
              <div className="font-b4-r green-500 mb-4 flex gap-2 align-center">
                Status Cards
              </div>

              {stakingCardCount > 0 ? (
                <section
                  className="flex justify-center align-center"
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "2px",
                  }}
                >
                  <ul className={`${styles.rows} `}>
                    <li className={styles.dataRow}>
                      <p className={`${styles.status} mb-4 gray-500`}>Status</p>
                      <p className={`${styles.total_qty} mb-4 gray-500`}>Total</p>
                      <p className={`${styles.locked_qty} mb-4 gray-500`}>
                        <span style={{ fontSize: '16px' }}>🔒</span>
                      </p>
                      <p className={`${styles.unlocked_qty} mb-4 gray-500`}>
                        <span style={{ fontSize: '16px' }}>🔓</span>
                      </p>
                    </li>
                    {/* 2. "Pending" 행 */}
                    <li className={styles.dataRow}>
                      <p className={styles.status} style={{ fontSize: '10px' }}>Pending</p>
                      <p className={styles.total_qty}>{pendingTotal}</p>
                      <p className={styles.locked_qty}>{pendingLocked}</p>
                      <p className={styles.unlocked_qty}>{pendingUnlocked}</p>
                    </li>

                    {/* 3. "Available" 행 */}
                    <li className={styles.dataRow}>
                      <p className={styles.status} style={{ fontSize: '10px' }}>Available</p>
                      <p className={styles.total_qty}>{availableTotal}</p>
                      <p className={styles.locked_qty}>{availableLocked}</p>
                      <p className={styles.unlocked_qty}>{availableUnlocked}</p>
                    </li>
                  </ul>
                </section>
              ) : (
                <div className="font-b4-r">Cards not inserted yet.</div>
              )}
            </div>
          </div>
          <div className={`${styles.statusInnerBox} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox} mb-8`}>
              <div className="font-b4-r green-500 mb-2 flex gap-2 align-center">
                {`S${userRankInfo?.last_season_no || "-"} Arena Rank Bonus`}
                <QuestionBig
                  onClick={() =>
                    openPopup({ type: "GenesisCardManual", data: "arena" })
                  }
                />
              </div>
              <div className={`${styles.statusBouseBox}`}>
                <div className={`${styles.statusSeasonRank} font-b4-r`}>
                  {userRankInfo?.arena_rank > 100
                    ? `${ARENA_TIER_STR[userRankInfo?.arena_rank - 100]}`
                    : "None"}
                </div>
                <div className={`${styles.metodiumBox} font-b4-r`}>
                  <img
                    src="/images/icon/magnetblock.png"
                    alt="metod"
                    style={{ width: "16px", height: "16px" }}
                  />
                  <div>
                    {stakingCardCount > 0 && stakingStats?.expected_detail?.arena > 0
                      ? stakingStats?.expected_detail?.arena.toFixed(0)
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
                    ? formatRank(userRankInfo?.raid_weekly_ranking)
                    : "no data"}
                </div>
                <div className={`${styles.metodiumBox}`}>
                  <img
                    src="/images/icon/magnetblock.png"
                    alt="metod"
                    style={{ width: "16px", height: "16px" }}
                  />
                  <div>
                    {stakingCardCount > 0
                      ? stakingStats?.expected_detail?.boss ?? 0
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