import React from "react";
import { ReactComponent as Level } from "../../../assets/svg/exchange/level.svg";
import { ReactComponent as Vip } from "../../../assets/svg/exchange/vip3.svg";
import {
  GenesisInsertedCards,
  GenesisArenaRankBonus,
  GenesisBossRaidRankBonus,
  ARENA_TIER_STR,
} from "../../../data/data";

export default function GenesisCardManual(props) {
  const { popupData, closePopup } = props;
  const tmp_info_data = sessionStorage.getItem("genesis.info");
  const info_data = JSON.parse(tmp_info_data);
  console.log(popupData);

  function ManualRank(props) {
    let data = "";

    if (props.level >= 11 && props.level <= 13) {
      data = `Platinum`;
    } else if (props.level >= 14 && props.level <= 16) {
      data = "Diamond";
    } else if (props.level >= 17 && props.level <= 19) {
      data = "Master";
    } else if (props.level === 20) {
      data = "Master 1";
    }

    return (
      <>
        <div>
          <img
            src={
              process.env.REACT_APP_TEST_PATH +
              `/images/rank/${props.openRank}.png`
            }
            alt="metod"
            style={{ width: "24px", height: "24px" }}
          />
        </div>
        <div>{data}</div>
      </>
    );
  }

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        {popupData.data === "card" ? (
          <>
            <div className="textBox">
              <div className="font-h6">Inserted Cards</div>
              <div className="font-b3-r gray-500">
                Magnetblock can be claimed every 6 hours,
                <br />
                with rewards based on inserted grade.
              </div>
            </div>
            <div className="genesisManualBoxListDataHeader font-b4-r white">
              <div className="theadManual">
                <div className="theadManualDiv">Grade</div>
                <div className="theadManualDiv borderLeft">
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnetblock.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div>Rewards(6H)</div>
                </div>
              </div>
            </div>
            <div className="manualBox font-b3-b">
              <div className="manualBoxListData font-b4-r white">
                <table className="genesisManualBoxTable">
                  <tbody>
                    {GenesisInsertedCards.reverse().map((data, index) => (
                      <tr key={index}>
                        <td className="level font-b4-b">{data.grade}</td>
                        <td className="font-b4-b">{data.rewards}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        ) : popupData.data === "arena" ? (
          <>
            <div className="textBox">
              <div className="font-h6">Arena Rank Bonus</div>
              <div className="font-b3-r gray-500">
                Bonus rewards are obtained based on
                <br />
                the previous season's Arena rank.
              </div>
            </div>
            <div className="genesisManualBoxListDataHeader font-b4-r white">
              <div className="theadManual">
                <div className="theadManualDiv borderRight">
                  Previous Arena Rank
                </div>
                <div className="theadManualDiv borderLeft">
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnetblock.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div>Rewards(6H)</div>
                </div>
              </div>
            </div>
            <div className="manualBox font-b3-b">
              <div className="manualBoxListData font-b4-r white">
                <table className="genesisManualBoxTable">
                  <tbody>
                    {Object.keys(info_data.arena_info).reverse().map((data, index) => (
                      <tr key={index}>
                        <td className="level justify-start">
                          <div className="ml-16 flex justify-center align-center">
                            <img
                              src={
                                process.env.REACT_APP_TEST_PATH +
                                `/images/rank/${data}.png`
                              }
                              alt="metod"
                              style={{ width: "24px", height: "24px" }}
                            />
                          </div>
                          <div className="font-b4-b">
                            {ARENA_TIER_STR[data]}
                          </div>
                        </td>
                        <td className="font-b4-b">
                          {info_data.arena_info[data]}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        ) : popupData.data === "boss" ? (
          <>
            <div className="textBox">
              <div className="font-h6">Boss Raid Ranking Bonus</div>
              <div className="font-b3-r gray-500">
                Bonus rewards are obtained based on weekly ranking.
              </div>
            </div>
            <div className="genesisManualBoxListDataHeader font-b4-r white">
              <div className="theadManual">
                <div className="theadManualDiv borderRight">
                  Previous Ranking
                </div>
                <div className="theadManualDiv borderLeft">
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnetblock.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div>Rewards(6H)</div>
                </div>
              </div>
            </div>
            <div className="manualBox font-b3-b">
              <div className="manualBoxListData font-b4-r white">
                <table className="genesisManualBoxTable">
                  <tbody>
                    {Object.keys(info_data.boss_info).map((data, index) => (
                      <tr key={index}>
                        <td className="level">{data}</td>
                        <td className="font-b4-b">
                          {info_data.boss_info[data]}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        ) : (
          <>잘못된 데이터</>
        )}
        <div className="singleBtn font-b3-b white" onClick={closePopup}>
          Close
        </div>
      </div>
    </div>
  );
}
