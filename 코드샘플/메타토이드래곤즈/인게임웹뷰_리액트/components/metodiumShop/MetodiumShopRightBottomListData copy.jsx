import styles from "../../styles/MetodiumShopRightBottomListData.module.css";
import { useSelector, useDispatch } from "react-redux";
import { setIsActive } from "../../data/shopSlice";
import { useRef, useEffect } from "react";
import { usePopup } from "../../context/PopupContext";

export default function MetodiumShopRightBottomListData(props) {
  const { isOpen, openPopup, closePopup } = usePopup();
  const { dataArr, selectedItem } = useSelector((state) => state.metodiumShop);
  //console.log(dataArr);
  const dummyArray = Array(4).fill(null);
  const dispatch = useDispatch();

  const selectedData = useSelector((state) => state.metodiumShop);

  const handleClick = (index) => {
    dispatch(setIsActive(selectedData.dataArr === index ? null : index));
  };
  const ref = useRef(null);
  //안팔린거랑 , 팔린거 따로 찢어서 돌려야될거같음 , 애초에 API 단에서 뒤로 재껴서 줄거 아니면 ...
  useEffect(() => {
    if (!ref) return;
    console.log(ref);
    console.log("enend");
  }, [ref]);
  return (
    <div
      ref={ref}
      className={styles.listBottom}
      style={{
        height: "auto",
      }}
    >
      <>
        {dataArr.map((item, index) =>
          //팔리지 않은거
          index <= 2 ? (
            <div
              id={`dataList_${index}`}
              className={`
                        ${styles.listBottomListDetail}                  
                        ${
                          selectedData.dataArr === index + 1
                            ? styles.active
                            : ""
                        }
                      `}
              key={index + 1}
              onClick={(e) => {
                if (!e.target.classList.contains("redBtn_" + index)) {
                  handleClick(index + 1);
                }
              }}
            >
              <div className={styles.listImgBox}>
                <img
                  src={
                    selectedData.filter === 0
                      ? process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                      : selectedData.filter === 1
                      ? process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                      : selectedData.filter === 2
                      ? process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                      : selectedData.filter === 3
                      ? process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                      : selectedData.filter === 4
                      ? process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                      : selectedData.filter === 5
                      ? process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                      : process.env.REACT_APP_TEST_PATH +
                        "/images/shop/starter_pack.png"
                  }
                  alt="dataList"
                  style={{
                    width: "64px",
                  }}
                />
              </div>
              <div className={styles.listTitleBox}>
                <div className={`font-b3-b green-500`}>
                  <div>1 per account</div>
                  <div>|</div>
                  <div>1/1</div>
                </div>
                <div className={`font-b1-b white`}>Starter_Pack</div>
              </div>
              <div
                id={`buyBtn_${index}`}
                className={`${styles.listMtdzBtnBox} font-b3-b white redBtn_${index}`}
                onClick={(e) => {
                  if (e.target.classList.contains("redBtn_" + index)) {
                    openPopup({ type: "MetodiumShop", data: index });
                  }
                }}
              >
                <div
                  className={`${styles.listMtdzBtn}  bg-red-400 redBtn_${index}`}
                >
                  <div className={`redBtn_${index}`}>
                    <img
                      src={
                        process.env.REACT_APP_TEST_PATH +
                        "/images/icon/magnite.png"
                      }
                      alt="me tod"
                      style={{ width: "20px" }}
                    />
                  </div>
                  <div className={`redBtn_${index}`}>50</div>
                </div>
              </div>
            </div>
          ) : (
            <div
              id={`dataList_${index}`}
              className={`
                        ${styles.listBottomListDetail}                  
                        ${
                          selectedData.dataArr === index + 1
                            ? styles.active
                            : ""
                        }
                        opacity-50
                        gray-500
                      `}
              key={index + 1}
            >
              <div className={styles.listImgBox}>
                <img
                  src={
                    item.goods_image
                    // selectedData.filter === 0
                    //   ? process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                    //   : selectedData.filter === 1
                    //   ? process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                    //   : selectedData.filter === 2
                    //   ? process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                    //   : selectedData.filter === 3
                    //   ? process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                    //   : selectedData.filter === 4
                    //   ? process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                    //   : selectedData.filter === 5
                    //   ? process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                    //   : process.env.REACT_APP_TEST_PATH +
                    //     "/images/shop/starter_pack.png"
                  }
                  alt="dataList"
                  style={{
                    width: "64px",
                  }}
                />
              </div>
              <div className={styles.listTitleBox}>
                <div className={`font-b3-b`}>
                  <div>1 per account</div>
                  <div>|</div>
                  <div>Sold out</div>
                </div>
                <div className={`font-b1-b`}>{item.goods_title}</div>
              </div>
              <div
                id={`buyBtn_${index}`}
                className={`${styles.listMtdzBtnBox} font-b3-b white redBtn_${index}`}
                onClick={(e) => {
                  if (e.target.classList.contains("redBtn_" + index)) {
                    openPopup({ type: "MetodiumShop", data: index });
                  }
                }}
              >
                <div
                  className={`${styles.listMtdzBtn}  bg-red-400 redBtn_${index}`}
                >
                  <div className={`redBtn_${index}`}>
                    <img
                      src={
                        process.env.REACT_APP_TEST_PATH +
                        "/images/icon/magnite.png"
                      }
                      alt="me tod"
                      style={{ width: "20px" }}
                    />
                  </div>
                  <div className={`redBtn_${index}`}>{item.price}</div>
                </div>
              </div>
            </div>
          )
        )}
      </>
    </div>
  );
}
