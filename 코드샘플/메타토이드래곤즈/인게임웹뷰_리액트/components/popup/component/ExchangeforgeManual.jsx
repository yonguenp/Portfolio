import React from "react";
import { ReactComponent as Level } from "../../../assets/svg/exchange/level.svg";
import { ReactComponent as Vip } from "../../../assets/svg/exchange/vip3.svg";
import { ARENA_TIER_STR } from "../../../data/data";

export default function ExchangeforgeManual(props) {
  const { popupData, closePopup } = props;
  const {
    data: {
      infoData: { forge_level_info },
      userState,
    },
  } = popupData;
  const processedLevelInfo = Object.keys(forge_level_info).reduce((res, v) => {
    if (v > 0)
      res.push({
        level: v,
        upgradeCost: forge_level_info[v].cost,
        dailyMax: forge_level_info[v].limit,
        openRank: forge_level_info[v].rank,
      });

    return res;
  }, []);
  //console.log(processedLevelInfo);
  function ManualRank(props) {
    //console.log(props);
    let data = ARENA_TIER_STR[props.openRank];
    // if (props.level >= 11 && props.level <= 13) {
    //   data = `Platinum`;
    // } else if (props.level >= 14 && props.level <= 16) {
    //   data = "Diamond";
    // } else if (props.level >= 17 && props.level <= 19) {
    //   data = "Master";
    // } else if (props.level === 20) {
    //   data = "Master 1";
    // }

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
        <div className="textBox">
          <div className="font-h6">
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "마그나이트 제련소 : 일일 한도 & 업그레이드 정보" 
  : "Magnite Forge : Daily Cap & Upgrade Info"}
          </div>
          <div className="font-b3-r gray-500">
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "마그나이트 제련소 레벨에 따라 일일 마그나이트 한도가 결정됩니다." 
  : "Magnite Forge level determines daily Magnite cap."}
            <br />
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "11레벨 이상 업그레이드에는 이전 아레나 시즌 랭크가 필요합니다." 
  : "Level 11+ requires previous arena season rank for upgrade."}
          </div>
        </div>

        <div className="manualBoxListDataHeader font-b4-r white">
          <div className="theadManual">
            <div className="theadManualDiv">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "단계" 
  : "Level"}</div>
            <div className="theadManualDiv borderLeft">
              <img
                src={
                  process.env.REACT_APP_TEST_PATH +
                  "/images/icon/magnite.png"
                }
                alt="metod"
                style={{ width: "24px", height: "24px" }}
              />
              <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "확장 비용" 
  : "Upgrade Cost"}</div>
            </div>
            <div className="theadManualDiv borderLeft">
              <img
                src={
                  process.env.REACT_APP_TEST_PATH +
                  "/images/icon/magnite.png"
                }
                alt="metod"
                style={{ width: "24px", height: "24px" }}
              />
              <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "일일 최대" 
  : "Daily Max"}</div>
            </div>
            <div className="theadManualDiv borderLeft">
              <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "가능 랭크" 
  : "Open Rank"}</div>
            </div>
          </div>
        </div>

        <div className="manualBox font-b3-b">
          <div className="manualBoxListData font-b4-r white">
            <table className="manualBoxTable">
              <tbody>
                {processedLevelInfo.map((data, index) => (
                  <tr
                    key={index}
                    style={{
                      background:
                        userState.forge_level == data.level
                          ? "var(--Red-400, #f90052)"
                          : "",
                    }}
                  >
                    <td className="level">
                      {data.level > 10 ? <Vip /> : <Level />}
                      Lv.{data.level}
                    </td>
                    <td>{data.upgradeCost}</td>
                    <td>{data.dailyMax}</td>
                    <td className="openRank">
                      {data.level > 10 ? (
                        <ManualRank
                          level={data.level}
                          openRank={data.openRank}
                        />
                      ) : <ManualRank
                      level={data.level}
                      openRank={data.openRank == 0 ? 1 : data.openRank}
                    /> }
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="singleBtn font-b3-b white" onClick={closePopup}>
          Close
        </div>
      </div>
    </div>
  );
}
