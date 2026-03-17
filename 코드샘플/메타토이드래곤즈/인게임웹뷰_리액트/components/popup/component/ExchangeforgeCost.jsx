import React from "react";
import { ARENA_TIER_STR } from "../../../data/data";

export default function ExchangeforgeCost(props) {
  const { popupData, closePopup } = props;
  const {
    data: {
      infoData: { forge_swap_info },
      userState,
    },
  } = popupData;
  const processed_data = Object.keys(forge_swap_info).map((v) => {
    return {
      name: ARENA_TIER_STR[v],
      cost: forge_swap_info[v],
      rank: v,
    };
  });
  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6">
  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
    ? "랭크별 마그나이트 비용" 
    : "Magnite Cost per Rank"}
</div>
<div className="font-b4-r gray-500">
  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
    ? <>이전 아레나 시즌 랭크가 높을수록<br />마그넷 블록이 더 적게 필요합니다.</> 
    : <>Higher previous arena season ranks require<br />fewer Magnet Blocks to smelt Magnite.</>}
</div>
        </div>

        <div className="costBoxListDataHeader font-b4-r white">
          <div className="theadRank">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "이전 아레나 랭크" : "Previous Arena Rank"}</div>
          <div className="theadCost border">
            <div>
              <img
                src={
                  process.env.REACT_APP_TEST_PATH +
                  "/images/icon/magnetblock.png"
                }
                alt=""
                style={{ width: "24px" }}
              />
            </div>
            <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그넷 블록" : "Magnet Block"}</div>
          </div>
          <div className="theadCost">
            <div>
              <img
                src={
                  process.env.REACT_APP_TEST_PATH +
                  "/images/icon/magnite.png"
                }
                alt=""
                style={{ width: "24px" }}
              />
            </div>
            <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트" : "Magnite"}</div>
          </div>
        </div>
        <div className="costBox font-b3-b">
          <div className="costBoxListData font-b4-r white">
            <table className="costBoxTable">
              <tbody>
                {processed_data.map((data, index) => (
                  <tr
                    key={index}
                    style={{
                      background:
                        (userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank) == data.rank
                          ? "var(--Red-400, #f90052)"
                          : "",
                    }}
                  >
                    <td className="rank font-b4-b">
                      <div>
                        <img
                          src={
                            process.env.REACT_APP_TEST_PATH +
                            `/images/rank/${data.rank}.png`
                          }
                          style={{ width: "24px" }}
                          alt=""
                        />
                        {data.name}
                      </div>
                    </td>
                    <td>{data.cost.toLocaleString()}</td>
                    <td>1</td>
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
