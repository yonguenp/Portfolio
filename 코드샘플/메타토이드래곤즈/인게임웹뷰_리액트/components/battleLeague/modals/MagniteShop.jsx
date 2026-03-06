import React, { useEffect, useState } from "react";
import styles from "../../../styles/MetodiumShopPopup.module.css";
import exStyles from "../../../styles/ExchangeRightBottom.module.css";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";

export default function MagniteShop(props) {
  const { popupData, closePopup } = props;

  const [title, setTitle] = useState("Purchase");
  const [popupType, setPopupType] = useState("");
  const [goldBoxChoose, setGoldBoxChoose] = useState(null);
  const [count, setCount] = useState(1);
  const { userInfo, setUserInfo } = useDAppState();

  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

  //userInfo.magnite 가져와야함
  const magnite = userInfo?.magnite || 0;
  
  // 최대 수량 (현재는 magnite 갯수로 제한, 배틀코인 1개당 마그나이트 100개)
  const maxCount = Math.floor(magnite / 100);
  
  // 총 가격 계산
  const totalPrice = (popupData?.data?.price || 0) * count;

  // 입력값 변경 핸들러
  const handleChange = (event) => {
    const inputValue = event.target.value.replace(/,/g, '');
    const regex = /^[0-9\b]+$/;

    if (inputValue === "" || regex.test(inputValue)) {
      const value = Number(inputValue);
      if (value >= 1 && value <= maxCount) {
        setCount(value);
      }
    }
  };

  const onFocus = (event) => {
    const inputValue = event.target.value.replace(/,/g, '');
    const regex = /^[0-9\b]+$/;

    if (inputValue === "" || regex.test(inputValue)) {
      setCount(Number(inputValue) || 1);
    }
  };

  const onBlur = (event) => {
    const inputValue = event.target.value.replace(/,/g, '');
    const regex = /^[0-9\b]+$/;

    if (inputValue === "" || !regex.test(inputValue)) {
      setCount(1);
      return;
    }

    const value = Number(inputValue);
    if (value < 1) {
      setCount(1);
    } else if (value > maxCount) {
      setCount(maxCount);
    } else {
      setCount(value);
    }
  };  

  const buy = async (season_id, coin_cnt) => {
    //closePopup();
    
    const data = await window.DApp.post("/battleleague/battlecoin/coinpurchase", {
      season_id : season_id,
      server_tag : sessionStorage.getItem("server_tag"),      
      coin_cnt : coin_cnt
    });

    if (data && data.rs == 0)
    {
      window.DApp.request("mail_check");
      window.DApp.emit("dapp.popup", {
        err: 0,
        title: "Success",
        msg: "Your purchase has been completed.",
      });
      
      setUserInfo(prev => ({
        ...prev,        
        magnite: data.curr_mig,
      }));
    }
    else if(!data || !data.msg)
    {
      window.DApp.emit("dapp.popup", {
        err: 1,
        title: "Error",
        msg: "An error occurred while purchasing the product.",
        isRefresher: false,
      });
    }
  }

  // useEffect(() => {
  //   if (popupData != null) {
  //     console.log(popupData);
  //   }
  //   console.log(popupData);
  // }, [popupData]);

  return (
    <>
      <div className="bg"></div>
      <div className="container">
        <div className="title">{popupData.data.goods_title}</div>
        <>
          <div className="content">
            <div className="shopImgBox">
              <img src={popupData.data.goods_image} alt="default" />
            </div>

            <div className={styles.iconTextWrapper}>
              <img src={
                          process.env.REACT_APP_TEST_PATH +
                          "/images/icon/magnite.png"
                        } 
                        alt="metod" style={{ width: "20px" }} className="icon" />
              <span className={styles.iconText}>{(totalPrice).toLocaleString()}</span>
            </div>
          </div>
          {/* 수량 - ExchangeRightBottom contentBottom 스타일 적용 */}
          <div className={exStyles.contentBottom}>
            <div className={`${exStyles.countBox} font-b3-b`}>
              <button
                className={exStyles.countBoxMinus}
                onClick={() => {
                  setCount((prev) => (prev > 1 ? prev - 1 : 1));
                }}
              >
                -
              </button>

              <input
                type="text"
                className={`${exStyles.countBoxNumber} font-b3-b`}
                placeholder="1"
                value={count}
                onChange={handleChange}
                onFocus={onFocus}
                onBlur={onBlur}
              />

              <button
                className={exStyles.countBoxPlus}
                onClick={() => {
                  setCount((prev) => (prev < maxCount ? prev + 1 : maxCount));
                }}
              >
                +
              </button>
            </div>
            <button
              className={`${exStyles.magnetBtn} ${styles.resetBtn} font-b4-b`}
              onClick={() => setCount(1)}
            >
              {isKorean ? "초기화" : "Reset"}
            </button>
            <button
              className={`${exStyles.magnetBtn} bg-red-400 font-b4-b white`}
              onClick={() => setCount(maxCount)}
            >
              {isKorean ? "최대" : "Max"}
            </button>
          </div>
          <div className="shopInfo font-b3-r text-center">
            After purchasing the package,
            <br />
            refunds or exchanges are not available.
          </div>
        </>

        <div className="popupButtons font-b3-b">
          <div className="cancelBtn" onClick={closePopup}>
            Cancel
          </div>
          <div
            className="signBtn"
            onClick={() => {
              if (popupData?.data?.season_id) {
                buy(popupData.data.season_id, count);
              }
            }}
          >            
            Buy
          </div>
        </div>
      </div>
    </>
  );
}
