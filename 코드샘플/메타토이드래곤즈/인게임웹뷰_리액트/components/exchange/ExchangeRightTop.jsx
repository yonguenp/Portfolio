import styles from "../../styles/ExchangeRightTop.module.css";

export default function ExchangeRightTop({
  userState,
  disabledSmelt,
  expectedAmount,
}) {
  const dailyMax = Number(userState?.curr_level_info?.limit || 0);
  const remain = Math.max(dailyMax - Number(userState.today_exchange || 0), 0);
  
  return (
    /*
      background: linear-gradient(0deg, #3A3838 0%, rgba(54, 54, 54, 0.00) 100%);
      background: linear-gradient(0deg, #F90052 0%, rgba(54, 54, 54, 0) 100%);
    */
    <div
      className={`${styles.container} ${disabledSmelt ? styles.active : " "}`}
    >
      <div className={styles.imgBox}>
        <img
          src={process.env.REACT_APP_TEST_PATH + "/images/icon/magnite.png"}
          alt=""
          style={{ width: "64px", height: "64px" }}
        />
      </div>
      <div className={styles.infoBox}>
        <div className={`${styles.title} font-b4-b`}>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트" : "Magnite"}</div>
        <div className={`${styles.count} font-h2  ${styles.typo}`}>
          {userState?.forge_level == 0 ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "미건설" : "Not Built") : (remain == 0 ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "일 최대" : "Daily Max") : expectedAmount || 0)}
        </div>
        <div className={styles.available}>
          <div className={styles.availableTop}>
            <div className={`font-b4-b green-500`}>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? `가능 : ${Number(
              remain.toFixed(4)
            )}` : `Available : ${Number(
              remain.toFixed(4)
            )}`}</div>
            <div className={`font-b4-b gray-500`}>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? `일 최대 : ${Number(
              dailyMax || 0
            )}` : `Daily Max : ${Number(
              dailyMax || 0
            )}`}</div>
          </div>
          <div className={styles.dailyBar}>
            <div
              className={`${styles.dailyProgressBar} bg-green-500`}
              style={{
                width: `${Math.floor(((dailyMax - remain) / dailyMax) * 100)}%`,
                height: "8px",
                borderRadius: "4px",
              }}
            ></div>
          </div>
        </div>
      </div>
    </div>
  );
}
