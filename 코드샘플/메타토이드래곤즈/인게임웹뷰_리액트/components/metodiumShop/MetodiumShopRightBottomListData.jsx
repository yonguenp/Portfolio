import styles from "../../styles/MetodiumShopRightBottomListData.module.css";
import { useSelector, useDispatch } from "react-redux";
import { setIsActive } from "../../data/shopSlice";

import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function MetodiumShopRightBottomListData(props) {
  const { isOpen, openPopup, closePopup } = usePopup();
  const { dataArr, filter, selectedItem } = useSelector(
    (state) => state.metodiumShop
  );
  const { isLoggedIn, userInfo } = useDAppState();
  const dispatch = useDispatch();
  const STR_TO_LIMIT_TYPE = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? [
    "무제한",
    "일일 구매 제한",
    "주 구매 제한",
    "월 구매 제한",
    "계정당 구매 제한",
  ]
  :
  [
    "No Limit",
    "per day",
    "per week",
    "per month",
    "per account",
  ];

  const handleClick = (index) => {
    dispatch(setIsActive(index));
  };
  

  const getDate = (index) => {
    const now = new Date();
    
    switch(index)
    {
      case 1:
        const tomorrow = new Date(now);
        tomorrow.setDate(now.getDate() + 1);
        tomorrow.setHours(0, 0, 0, 0); // 자정으로 설정 (0시 0분 0초)
        if(tomorrow < now)
          tomorrow.setDate(now.getDate() + 1);
        return " | " + tomorrow.toLocaleString();
      case 2:
        const day = now.getDay();
        const diff = (day === 0 ? -6 : 1 - day); // 일요일이면 -6, 그 외에는 1 - day
        const monday = new Date(now);
        monday.setDate(now.getDate() + diff);
        monday.setHours(0, 0, 0, 0); // 자정으로 설정 (0시 0분 0초)
        if(monday < now)
          monday.setDate(now.getDate() + 7);
        return " | " + monday.toLocaleString();
      case 3:
        const nextMonth = now.getMonth() + 1; // 현재 월 + 1
        const year = now.getFullYear();    
        let nextMonthFirst = new Date(year, nextMonth, 1);  
        nextMonthFirst.setHours(0, 0, 0, 0); // 자정으로 설정 (0시 0분 0초)
        if(nextMonthFirst < now)
        {
          nextMonthFirst = new Date(year, nextMonth + 1, 1);  
          nextMonthFirst.setHours(0, 0, 0, 0); // 자정으로 설정 (0시 0분 0초)
        }
        return " | " + nextMonthFirst.toLocaleString();
      default:
        return "";
    }
    
  }
  //안팔린거랑 , 팔린거 따로 찢어서 돌려야될거같음 , 애초에 API 단에서 뒤로 재껴서 줄거 아니면 ...

  return (
    <div className={styles.listBottom}>
      {dataArr
        .filter((v) => {
          switch (filter) {
            case 0:
            case "0": {
              return true;
              break;
            }
            case 1:
            case "1": {
              //console.log(v);
              if (v.filter == 1) return true;
              return false;
              break;
            }
            case 2:
            case "2": {
              if (v.limit_type == 1) return true;
              return false;
              break;
            }
            case 3:
            case "3": {
              if (v.limit_type == 2) return true;
              return false;

              break;
            }
            case 4:
            case "4": {
              if (v.limit_type == 3) return true;
              return false;
              break;
            }
            case 5:
            case "5": {
              if (v.filter == 2) return true;
              return false;
              break;
            }
            default: {
              return false;
              break;
            }
          }
        })
        .map((item, index) => {
          let soldOut = false;
          let disabled = false;
          let Exceeded = false;
          if (!isLoggedIn) {
            disabled = true;
          }
          
          if (!disabled && item?.buy_count) {
            if (item.limit_type == 0) {
              soldOut = false;
            } else {
              if (Number(item?.limit_amount) - Number(item?.buy_count) < 1) {
                soldOut = true;
                disabled = true;
              } else {
                soldOut = false;
              }
            }
          } else {
            //로그인 안함
            soldOut = false;
          }
          
          if(item.price > userInfo?.magnite)
          {
             Exceeded = true;
          }

          return (
            <div
              id={`dataList_${index}`}
              className={`
                        ${styles.listBottomListDetail}                  
                        ${
                          selectedItem?.goods_id === item.goods_id
                            ? styles.active
                            : ""
                        }
                        ${soldOut ? "opacity-50 gray-500" : ""}
                        
                      `}
              key={index + 1}
              onClick={() => {
                handleClick(item);
              }}
            >
              <div className={styles.listImgBox}>
                <img
                  src={item.goods_image}
                  alt="dataList"
                  style={{
                    width: "64px",
                  }}
                />
              </div>
              <div className={styles.listDetail}>
                <div className={styles.listDetailSkin}>
                  <div className={styles.listTitleBox}>
                    <div className={`font-b3-b ${soldOut ? "" : `green-500`}`}>
                      <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? 
                      `${
                        STR_TO_LIMIT_TYPE[item.limit_type]
                      } ${item.limit_type > 0 ? item.limit_amount : ""}`
                      : `${item.limit_type > 0 ? item.limit_amount : ""} ${
                        STR_TO_LIMIT_TYPE[item.limit_type]
                      }`}</div>
                      {item.limit_type != 0 && (
                        <>
                          <div>|</div>
                          <div>
                            {soldOut
                              ? "Sold Out" + getDate(item.limit_type)
                              : `${
                                  Number(item?.limit_amount) -
                                  Number(item?.buy_count)
                                }/${item.limit_amount}`}
                          </div>
                        </>
                      )}
                    </div>
                    <div className={`font-b1-b ${false ? "" : `white`}`}>
                      {item.goods_title}
                    </div>
                  </div>
                  <div
                    id={`buyBtn_${index}`}
                    className={`${styles.listMtdzBtnBox} font-b3-b white redBtn_${index}`}
                    onClick={(e) => {
                      if (!disabled) {
                        if(Exceeded)
                          openPopup({ type: "MessagePopup", title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "오류" : "Error", msg: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트 수량을 확인해주세요." : "Please check your Magnite amount.", isRefresher: false });
                        else
                          openPopup({ type: "MetodiumShop", data: item });
                      }
                    }}
                  >
                    <button
                      className={`${
                        styles.listMtdzBtn
                      }  bg-red-400 ${ Exceeded ? "gray" : "white" } font-b3-b ${
                        disabled || Exceeded ? "opacity-50" : ""
                      }`}
                    >
                      <img
                        src={
                          process.env.REACT_APP_TEST_PATH +
                          "/images/icon/magnite.png"
                        }
                        alt="metod"
                        style={{ width: "20px" }}
                      />
                      <p>{Number(item.price).toLocaleString()}</p>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
    </div>
  );
}
