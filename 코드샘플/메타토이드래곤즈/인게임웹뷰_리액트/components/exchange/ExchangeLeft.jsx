import styles from "../../styles/ExchangeLeft.module.css";
import { useSelector } from "react-redux";

import { ReactComponent as Craft } from "../../assets/svg/common/craft.svg";
import { ReactComponent as QuestShield } from "../../assets/svg/common/quest_shield.svg";
import { ReactComponent as QuestShieldBlur } from "../../assets/svg/common/quest_shield_blur.svg";
import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";
import { ReactComponent as Arrow } from "../../assets/svg/common/arrow(swap).svg";

import { usePopup } from "../../context/PopupContext";
import { useState, useEffect } from "react";
import { ARENA_TIER_STR } from "../../data/data";
export default function ExchangeLeft({ userState, infoData, onRefresh }) {
  const { openPopup } = usePopup();
  useEffect(() => {
    setQuestLevel(userState.town_level >= 5);
    setQuestForge(userState.forge_level > 0);
  }, [userState]);

  //퀘스트 달성 여부
  const [questLevel, setQuestLevel] = useState(false);
  const [questForge, setQuestForge] = useState(false);
  //console.log("asdf", userState);
  const QuestLevelComp = () => {
    return (
      <div className={styles.topInfoBox}>
        <div className={styles.topInfo}>
          {questLevel ? <QuestShieldBlur /> : <QuestShield fill="white" />}
          <div
            className={`
              ${styles.topInfoTitle} 
              ${questLevel ? "gray-500" : "white"}
              font-b4-r
            `}
          >
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트 포지를 활성화 조건은 타운 레벨이 5 이상이어야 합니다." : "To activate magnite Forge, Town level must be 5 or higher."}                
          </div>
        </div>
        <div className={`${styles.topInfoBoxBottom}`}>
          <div className={`${styles.topInfoBoxBottomTitle} font-b4-r gray-500`}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "타운 레벨" : "Town Level"}                
          </div>
          <div
            className={`${styles.topInfoBoxBottomCount} font-b3-b ${
              questLevel ? "gray-500" : "red-400"
            }`}
          >
            {`${userState.town_level}/5`}
          </div>
        </div>
      </div>
    );
  };

  const QuestForgeComp = () => {
    return (
      <div className={styles.topInfoBox}>
        <div className={styles.topInfo}>
          {questForge ? <QuestShieldBlur /> : <QuestShield fill="white" />}
          <div
            className={`
              ${styles.topInfoTitle}
              ${questForge ? "gray-500" : "white"}
              font-b4-r
            `}
          >
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트를 제련하려면 마그나이트 포지를 건설해야 합니다." : "You need to build a Magnite Forge to smelt Magnite."}                            
          </div>
        </div>
        <div
          className={`
            ${styles.topInfoButton_noWallet2}
            ${questForge ? "opacity50" : ""}
            font-b3-b
            `}
          onClick={() => questLevel && !questForge && openPopup({ type: "Exchange", onRefresh: onRefresh })} 
          style={{ opacity: (questForge || !questLevel) ? 0.5 : 1 }}   
        >
          <Craft /> { questForge ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "건설 완료" : "Build Completed") : (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "건설" : "Build")}
        </div>
      </div>
    );
  };
  
  return (
    <div className={styles.container}>
        {
          //퀘스트를 이행하였는가 ??
          questForge === true && questLevel === true ? (
            <>
              <div className={`${styles.questCompleteBox} bg-gray-900`}>
                <div className="font-b3-b mb-8">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "제련소 정보" : "My Forge"}</div>
                <div className={`${styles.questCompleteInnerBox} bg-gray-900`}>
                  <div className={`${styles.topInfo} font-b4-r green-500`}>
                    <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트 제련소" : "Magnite Forge"}</div>{" "}
                    <QuestionBig
                      onClick={() =>
                        openPopup({
                          type: "ExchangeforgeManual",
                          data: { infoData, userState },
                        })
                      }
                    />
                  </div>
                  <div className={`${styles.myForegInfoBox} font-b2-b`}>
                    <div>{`Lv.${userState.forge_level}`}</div>
                    <div>
                      <div className="font-b4-r gray-500">
                        Best Rank{" "}
                        <span className="white font-b4-b ml-4">
                          {ARENA_TIER_STR[userState.arena_top_rank]}
                        </span>
                      </div>
                      <div className="font-b4-r gray-500">
                        Max Level{" "}
                        <span
                          className="white font-b4-b"
                          style={{ marginLeft: "2px" }}
                        >
                          {Math.max(
                            ...Object.keys(
                              infoData?.forge_level_info || []
                            ).filter(
                              (v) =>
                                infoData?.forge_level_info?.[v]?.rank <=
                                (userState?.arena_top_rank)
                            )
                          )}
                        </span>
                      </div>
                    </div>
                  </div>
                  <button
                    disabled={Math.max(
                      ...Object.keys(
                        infoData?.forge_level_info || []
                      ).filter(
                        (v) =>
                          infoData?.forge_level_info?.[v]?.rank <=
                          (userState?.arena_top_rank)
                      )
                    ) <= userState.forge_level}
                    className={`${styles.topInfoButton} white font-b3-b`}
                    onClick={() =>
                      Math.max(
                        ...Object.keys(
                          infoData?.forge_level_info || []
                        ).filter(
                          (v) =>
                            infoData?.forge_level_info?.[v]?.rank <=
                            (userState?.arena_top_rank)
                        )
                      ) > userState.forge_level ? 
                      openPopup({ type: "Upgrade", data: userState, onRefresh: onRefresh })
                      : openPopup({ type: "MessagePopup", title: "Error", msg: "Please check the conditions.", isRefresher: false })
                    }                    
                  >
                    {
                    Math.max(
                      ...Object.keys(
                        infoData?.forge_level_info || []
                      ).filter(
                        (v) =>
                          infoData?.forge_level_info?.[v]?.rank <=
                          (userState?.arena_top_rank)
                      )
                    ) <= userState.forge_level ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "최대 단계" : "MAX Level") : (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "단계 확장" : "Upgrade")
                    }
                  </button>
                </div>

                <div className={`${styles.questCompleteInnerBox} bg-gray-900`}>
                  <div className={`${styles.topInfo} font-b4-r green-500 mb-8`}>
                    <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "제련 비용" : "Smelting Cost"}</div>{" "}
                    <QuestionBig
                      onClick={() =>
                        openPopup({
                          type: "ExchangeforgeCost",
                          data: { infoData, userState },
                        })
                      }
                    />
                  </div>
                  <div className={`${styles.questInfoTitleBox} font-b2-b`}>
                    <div>
                      <img
                        src={`${process.env.REACT_APP_TEST_PATH}/images/rank/${userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank}.png`}
                        alt="rank"
                        style={{ width: "32px" }}
                      />
                    </div>
                    <div className={styles.verticalGray}></div>
                    <div className="font-b4-r gray-500">
                      <div>{`Season ${userState.last_season_no} Arena Rank`}</div>
                      <div className="white font-b4-b">
                        {ARENA_TIER_STR[userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank]}
                      </div>
                    </div>
                  </div>
                  <div
                    className={`${styles.bottomInfoForgeCost} bg-gray-800 font-b3-b`}
                  >
                    <div className={styles.forgeCostMagnet}>
                      <div>
                        <img
                          src={
                            process.env.REACT_APP_TEST_PATH +
                            "/images/icon/magnetblock.png"
                          }
                          alt="metod"
                        />
                      </div>
                      <div>
                        {Number(
                          infoData?.forge_swap_info?.[userState.arena_rank > 100 ? userState.arena_rank - 100 : userState.arena_rank]
                        ).toLocaleString()}
                      </div>
                    </div>
                    <Arrow fill="#222222" />
                    <div className={styles.forgeCostToken}>
                      <div>
                        <img
                          src={
                            process.env.REACT_APP_TEST_PATH +
                            "/images/icon/magnite.png"
                          }
                          alt="metod"
                        />
                      </div>
                      <div>1</div>
                    </div>
                  </div>
                  {/* <div className={`${styles.bottomInfoRank} font-b4-r green-500`} >
                  Previous Arena Rank
                </div>
                <div className={`${styles.bottomInfoRankDetail}`} >
                  Diamond 3
                </div> */}
                </div>
              </div>
            </>
          ) : (
            <>
              <QuestLevelComp />
              <QuestForgeComp />
            </>
          )
        }
    </div>
  );
}
