import styles from "../../styles/ExchangeRightBottom.module.css";
import { useState } from "react";
import BigNumber from "bignumber.js";
import { usePopup } from "../../context/PopupContext";

export default function ExchangeRightBottom({
  userState,
  count,
  setCount,
  disabledSmelt,
  maxAmount,
  per_magnet,
  onRefresh
}) {
  //redux 로 돌리고, 내 현재 메토드, 마그넷 갯수 가져와야함 ,
  //내가 가진 마그넷 갯수 기반으로 하여금 50% 혹은 MAX 처리도 되야함
  //그리고 내가 얼마나 바꿀건지도 바로바로 확인이 되야함
  // Locked, UnLocked Redux 에서 보유하고 있어야지

  /* 
  border: 1px solid var(--Gray-800, #3A3838);
  border: 1px solid var(--Gray-800, #F90052);
   */
  const { openPopup } = usePopup();

  // const [count, setCount] = useState(0);
  const onFocus = (event)=>{
    //console.log('onFocus');
    const inputValue = event.target.value.replace(/,/g, '');
    const regex = /^[0-9\b]+$/;

    if (inputValue === "" || regex.test(inputValue)) {
      setCount(Number(inputValue));
    }
  }

  const onBlur = (event)=>{
    const inputValue = event.target.value.replace(/,/g, '');
    const regex = /^[0-9\b]+$/;

    if (inputValue === "" || regex.test(inputValue)) {
      if (Number(inputValue) < per_magnet)
      {
        setCount(0);
      }
      else
      {
        let val = (((parseInt(Number(inputValue))) - (Number(inputValue) % per_magnet)));
        if((((parseInt(Number(inputValue))) - (Number(inputValue) % per_magnet))) > maxAmount)
          val = maxAmount;

        setCount(val);
      }
    }
  }

  const handleChange = (event) => {
    const inputValue = event.target.value.replace(/,/g, '');
    const regex = /^[0-9\b]+$/;

    if (inputValue === "" || regex.test(inputValue)) {
      setCount(Number(inputValue));
    }
  };

  return (
    <div className={`${styles.container} `}>
      <div className={styles.magnetBox}>
        <div
          className={`${styles.arrowBox} ${
            !disabledSmelt ? " bg-red-400" : "bg-gray-800"
          } `}
        >
          <img
            src={
              process.env.REACT_APP_TEST_PATH +
              `/images/icon/${
                !disabledSmelt ? "arrow-btot-white.png" : "arrow-btot.png"
              }`
            }
            alt="arrow"
          />
        </div>
        <div
          className={`${styles.content} ${
            !disabledSmelt ? "" : styles.contentDisabled
          }`}
        >
          <div className={styles.contentLeft}>
            <img
              src={
                process.env.REACT_APP_TEST_PATH + "/images/icon/magnet_big.png"
              }
              alt=""
              style={{ width: "64px" }}
            />
          </div>
          <div>
            <div className={styles.contentTop}>
              <div className={`${styles.contentTopLeft} font-b4-b`}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그넷 블록" : "Magnet Block" }
              </div>
              <div>
                <img
                  src={
                    process.env.REACT_APP_TEST_PATH +
                    "/images/icon/smelting.png"
                  }
                  alt=""
                  style={{ width: "24px" }}
                />
              </div>
              <div className={`${styles.contentTopRight} font-b4-b`}>
                { navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? `제련 가능 수량 ${Number(
                  userState?.unlocked_magnet || 0
                ).toLocaleString()}` : `Smelting Available ${Number(
                  userState?.unlocked_magnet || 0
                ).toLocaleString()}`}
              </div>
            </div>
            <div className={styles.contentBottom}>
              <div
                className={`${styles.countBox} font-b3-b ${
                  disabledSmelt && count != 0 ? styles.countBox_warning : ""
                }`}
              >
                <button
                  className={styles.countBoxMinus}
                  onClick={() =>
                    setCount((prev) =>
                      parseInt(prev) > per_magnet ? parseInt(prev) - per_magnet : 0
                    )
                  }
                >
                  -
                </button>

                <input
                  type="text"
                  className={` ${styles.countBoxNumber} font-b3-b`}
                  placeholder="10"
                  value={count}
                  onChange={handleChange}
                  onFocus={onFocus}
                  onBlur={onBlur}
                />

                <button
                  className={styles.countBoxPlus}
                  onClick={() =>
                    setCount((prev) => (parseInt(prev) + per_magnet >= maxAmount ? maxAmount : parseInt(prev) + per_magnet))
                  }
                >
                  +
                </button>
              </div>
              <button
                className={`${styles.magnetBtn} bg-gray-800 font-b4-b white`}
                onClick={() => setCount(0)}
              >
                { navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "초기화" : "Reset" }
              </button>
              <button
                className={`${styles.magnetBtn} bg-red-400 font-b4-b white`}
                onClick={() => setCount(maxAmount)}
              >
                { navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "최대" : "Max" }
              </button>
            </div>
            <div className={styles.contentBottom}>
              {/* <button
                className={`${styles.magnetBtn} bg-red-400 font-b4-b white`}
                onClick={() => setCount(maxAmount)}
              >
                Max
              </button> */}
              {/* <button
                className={`${styles.magnetBtn} font-b4-r gray-500`}
                onClick={() => setCount((prev) => 
                  Math.min(maxAmount, (parseInt(prev) + 1000 >= maxAmount ? maxAmount : parseInt(prev) + (1000 * per_magnet))))}
              >
                +1000
              </button>
              <button
                className={`${styles.magnetBtn} font-b4-r gray-500`}
                onClick={() => setCount((prev) => 
                  Math.min(maxAmount, (parseInt(prev) + 100 >= maxAmount ? maxAmount : parseInt(prev) + (100 * per_magnet))))}
              >
                +100
              </button>
              <button
                className={`${styles.magnetBtn} font-b4-r gray-500`}
                onClick={() => setCount((prev) => 
                  Math.min(maxAmount, parseInt(prev) + (10 * per_magnet)))}
              >
                +10
              </button> */}
            </div>
          </div>
        </div>
      </div>

      <button
        disabled={disabledSmelt}
        className={`${disabledSmelt ? styles.disabled : ""} ${
          styles.btnBox
        } bg-red-400 font-b3-b`}
        onClick={() =>
          openPopup({
            type: "SmeltMetodium",
            data: { amount: count, userState },
            onRefresh: onRefresh,
          })
        }
      >
        {/* <PlayWallet style={{ marginTop: "-1px" }} /> */}
        { navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "제련하기" : "Smelt Now" } 
      </button>
    </div>
  );
}
