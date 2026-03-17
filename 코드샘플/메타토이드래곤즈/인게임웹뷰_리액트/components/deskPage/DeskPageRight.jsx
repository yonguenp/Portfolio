import { useState, useMemo, useEffect } from 'react';
import styles from "../../styles/DeskPageRight.module.css";
import ExchangeRightTop from "../exchange/ExchangeRightTop";
import ExchangeRightBottom from "../exchange/ExchangeRightBottom";
import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";

export default function DeskPageRight({ layoutType, data, refetch }) {
  const { openPopup } = usePopup();

  const printlog = (val) => {
    console.log(val);
  }
  const getOrdinal = (n) => {
    const s = ["th", "st", "nd", "rd"];
    const v = n % 100;

    return n + (s[(v - 20) % 10] || s[v] || s[0]);
  };

  const [items, setItems] = useState([]);
  const [wins, setWins] = useState([]);
  const [openItemDetails, setOpenItemDetails] = useState({});
  const [openDetails, setOpenDetails] = useState({});
  const { userInfo, setUserInfo } = useDAppState();

  const toggleItemDetails = (tournament) => {
    setOpenItemDetails((prev) => ({
      ...prev,
      [tournament]: !prev[tournament],
    }));
  };

  const toggleDetails = (tournament) => {
    setOpenDetails((prev) => ({
      ...prev,
      [tournament]: !prev[tournament],
    }));
  };


  const handleClaimPrize = async (tournament) => {
    const result = await window.DApp.post(`tournament/unifiedclaimprize`, {
      server_tag: sessionStorage.getItem("server_tag"),
      claim_type: 2
    });

    if (result.rs == 0) {
      setUserInfo(prev => ({
        ...prev,
        champoracle: result.oracles,
      }));

      window.DApp.emit("dapp.popup", {
        err: 0,
        title: "Success",
        msg: "You’ve successfully claimed your prize!",
      });

      setItems(prev => prev.map(item => ({ ...item, claimed: 2 })));
    }
  };

  const handleClaimWinning = async (tournament) => {
    const result = await window.DApp.post(`tournament/unifiedclaimprize`, {
      server_tag: sessionStorage.getItem("server_tag"),
      claim_type: 1,
    });

    if (result.rs == 0) {
      setUserInfo(prev => ({
        ...prev,
        champoracle: result.oracles,
      }));

      window.DApp.emit("dapp.popup", {
        err: 0,
        title: "Success",
        msg: "You’ve successfully claimed your winnings!",
      });

      setWins(prev => prev.map(item => ({ ...item, claimed: 2 })));
    }
  };

  useEffect(() => {
    setItems(data["prizeds"].concat(data["ut_prizeds"]));
    setWins(data["wins"].concat(data["ut_wins"]));
  }, [data]);

  const getResultStr = (val) => {
    switch (val) {
      case 8:
        return "Champion";
      case 7:
        return "Finalist";
      case 6:
        return "Semifinalist";
    }

    return "unknown";
  }

  const getRoundStr = (val) => {
    switch (val) {
      case 2:
        return "Round of 16";
      case 3:
        return "Quater Finals";
      case 4:
        return "Semi Finals";
      case 5:
        return "Final";
    }
  }

  const isAWin = (val) => {
    if (val == 2)
      return true;
  }

  const isBWin = (val) => {
    if (val == 3)
      return true;
  }

  return (
    <div className={`${styles.container}`}>
      {layoutType == "claim_prize" && (
        <>
          {
            items?.length > 0 ?
              items.some(item => item.claimed < 1) ? (
                <div className={`${styles.claimAllStickyBox}`}>
                  <div
                    className={`${styles.btnBox} bg-red-400 font-b3-b`}
                    onClick={handleClaimPrize}
                  >
                    Claim All
                  </div>
                </div>
              ) :
                (
                  <div className={`${styles.claimAllStickyBox} ${styles.disabled}`}>
                    <div
                      className={`${styles.btnBox} bg-red-400 font-b3-b`}
                      disabled={true}
                    >
                      All Claimed
                    </div>
                  </div>
                )
              :
              (<div></div>)
          }
          <div className={`${styles.container_prize}`}>
            {items?.length === 0 ? (
              <div className={styles.container_prize}>
                <div className={styles.centeredContent}>
                  There is no prize oracle allocated.
                </div>
              </div>
            ) :
              (items?.map((item, index) => (
                <div key={item.tournament} >
                  <div className={`${styles.containerRightBox} bg-gray-900`}>
                    <div className="font-b3-r">{`The ${getOrdinal(item.tournament)} Tournament`}</div>
                    <div
                      className={`${styles.containerRightBoxInner} bg-gray-900 font-b4-r`}
                    >
                      <div className={styles.rewardBox}>
                        <div className="flex align-center justify-start font-b4-r green-500 mb-4 gap-2">
                          <div>Prize</div>
                        </div>
                        <div className={`${styles.rewardBoxTop} mb-8`}>
                          <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/championoracle.png"} style={{ width: "26px", height: "26px" }} /><div className="font-b1-b">{(Math.floor(item.amount * 100) / 100).toLocaleString()}</div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className={styles.btnBoxWrapper}>
                    <div
                      className={`${styles.containerRightBox2} bg-gray-900`}
                      onClick={() => toggleItemDetails(item.tournament)}
                    >
                      {!openItemDetails[item.tournament] ? (
                        <div className="flex justify-center font-b1-r">Show Details</div>
                      ) : (
                        <div className={`${styles.containerRightBoxInner2} bg-gray-900`}>
                          <div className="font-b3-r"
                            style={{
                              fontWeight: "bold",
                              color:
                                item.result === 8
                                  ? "#FFD700" // 금색
                                  : item.result === 7
                                    ? "#C0C0C0" // 은색
                                    : item.result === 6
                                      ? "#CD7F32" // 동색
                                      : "white", // 기본값 (선택)
                            }}>{getResultStr(item.result)}</div>
                          <div className="font-b4-r">{(Math.floor(item.amount * 100) / 100).toLocaleString()}</div>
                        </div>

                      )}

                    </div>
                  </div>
                </div>
              )))}
          </div>
        </>
      )}

      {layoutType == "claim_win" && (
        <>
          {
            wins?.length > 0 ?
              wins.some(item => item.claimed < 1) ? (
                <div className={`${styles.claimAllStickyBox}`}>
                  <div
                    className={`${styles.btnBox} bg-red-400 font-b3-b`}
                    onClick={handleClaimWinning}
                  >
                    Claim All
                  </div>
                </div>
              ) :
                (
                  <div className={`${styles.claimAllStickyBox} ${styles.disabled}`}>
                    <div
                      className={`${styles.btnBox} bg-red-400 font-b3-b`}
                      disabled={true}
                    >
                      All Claimed
                    </div>
                  </div>
                )
              :(
              <div></div>)
          }
          <div className={`${styles.container_prize}`}>
            {wins && Object.entries(wins).length === 0 ? (
              <div className={styles.container_prize}>
                <div className={styles.centeredContent}>
                  There is no winning oracle allocated.
                </div>
              </div>
            ) : (
              wins && Object.entries(wins).map(([tournament, wins]) => (
                <div key={wins.tournament} >
                  <div className={`${styles.containerRightBox} bg-gray-900`}>
                    <div className="font-b3-r">{`The ${getOrdinal(wins.season_id ?? wins.tournament)} Tournament`}</div>
                    <div
                      className={`${styles.containerRightBoxInner} bg-gray-900 font-b4-r`}
                    >
                      <div className={styles.rewardBox}>
                        <div className="flex align-center justify-start font-b4-r green-500 mb-4 gap-2">
                          <div>Winnings</div>
                        </div>
                        <div className={`${styles.rewardBoxTop} mb-8`}>
                          <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/championoracle.png"} style={{ width: "26px", height: "26px" }} /><div className="font-b1-b">{(Math.floor(wins.total * 100) / 100).toLocaleString()}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div className={styles.btnBoxWrapper}>
                    <div
                      className={`${styles.containerRightBox2} bg-gray-900`}
                      onClick={() => toggleDetails(wins.tournament)}
                    >
                      {!openDetails[wins.tournament] ? (
                        <div className="flex justify-center font-b1-r">Show Details</div>
                      ) : (
                        <div className={`${styles.containerRightBoxInner2} bg-gray-900 font-b4-r`}>
                          {wins.info.map((item, idx) => item.amount > 0 && (
                            <div
                              key={idx}
                            >
                              <div>
                                <div className={styles.rewardBoxTop} style={{ fontWeight: "bold" }}>
                                  Round <div className="font-semibold" style={{ fontWeight: "normal", color: "#FFD700" }}>{getRoundStr(item.round)}</div>
                                </div>

                                <div className={styles.rewardBoxTop} style={{ fontWeight: "bold" }}>
                                  Bet <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/championoracle.png"} style={{ width: "18px", height: "18px", }} /><div className="font-semibold" style={{ fontWeight: "normal", color: "#FFD700" }}>{item.amount.toLocaleString()}</div>
                                </div>
                                {
                                  (isAWin(item.winner) || isBWin(item.winner)) &&
                                  (
                                    <div className={styles.rewardBoxTop} style={{ fontWeight: "bold" }}>
                                      Cut <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/championoracle.png"} style={{ width: "18px", height: "18px", }} /><div className="font-semibold" style={{ fontWeight: "normal", color: "#FFD700" }}>{(Math.floor(item.expected_dividend * 100) / 100).toLocaleString()}</div>
                                    </div>
                                  )
                                }

                                <div className={`${styles.rewardBoxTop} flex justify-center items-center`} >

                                  <span
                                    className={
                                      isAWin(item.winner) ? `${styles.winnerText}` : `${styles.loserText}`
                                    }
                                  >
                                    {item.luser}
                                    {/* {item.winner === "sideA" ? ( "(Win)" ) : ( "(Lose)")} */}
                                  </span>
                                  <span className="mx-1 text-white"> VS </span>
                                  <span
                                    className={
                                      isBWin(item.winner) ? `${styles.winnerText}` : `${styles.loserText}`
                                    }
                                  >
                                    {item.ruser}
                                    {/* {item.winner === "sideB" ? ( "(Win)" ) : ( "(Lose)")} */}
                                  </span>
                                </div>
                              </div>
                              <br />
                            </div>
                          ))}
                        </div>
                      )}

                    </div>
                  </div>
                </div>
              )))}</div>
        </>
      )}
    </div>
  );
}




// <div className={styles.rewardBoxTop}>
// {item.sideA}
// {item.winner === "sideA" && "(승)" }

// <span className="mx-1 text-white">VS</span>
// {item.sideB}
// {item.winner === "sideB" && "(승)" }
// </div>