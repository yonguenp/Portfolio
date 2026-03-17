import React, { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux"; // 💡 useDispatch 추가
import { getGradeForItem } from "../../../utils/itemUtils";
import { usePopup } from "../../../context/PopupContext";
import { fetchInven } from "../../../data/nftSlice"; // 경로를 맞춰주세요!

export default function NFTInventory(props) {
  const selectedData = useSelector((state) => state.nftInventory.selectedItem);
  const collection = useSelector((state) => state.nftInventory.filter);
  const { popupData, closePopup } = props;
  const { data } = popupData;
  const [title, setTitle] = useState("Use Item");
  const [popupType, setPopupType] = useState("");
  const [goldBoxChoose, setGoldBoxChoose] = useState(null);
  const [convertName, setConvertName] = useState("");
  //지금은 임시로 필터로 잡았지만 해당 item 에 대한 id 및 type 등등 다 받아와야함
  const { openPopup } = usePopup();
  const [quantity, setQuantity] = useState(1);
  const [maxCount, setMaxCount] = useState(0);
  // 💡 useDispatch 훅 사용
  const dispatch = useDispatch();

  // 💡 인벤토리 목록을 새로고침하는 함수 정의
  const refreshInventoryList = () => {
    // Redux를 사용하여 인벤토리 데이터를 다시 서버에서 불러옵니다.
    // 이 함수가 실행되면, Redux Store의 NFT 목록이 업데이트되고
    // 이 데이터를 사용하는 다른 컴포넌트(인벤토리 목록 컨테이너)가 자동으로 리렌더링됩니다.
    dispatch(fetchInven());
    console.log("NFT Inventory Data Re-fetched by Redux Action.");
  };
  const btnStyle = {
    width: "28px",
    height: "28px",
    background: "#222",
    border: "1px solid #444",
    borderRadius: "6px",
    color: "#fff",
    cursor: "pointer",
    fontSize: "14px",
    lineHeight: "1",
  };

  const inputStyle = {
    width: "50px",
    height: "28px",
    background: "#111",
    border: "1px solid #444",
    borderRadius: "6px",
    color: "#fff",
    textAlign: "center",
  };

  useEffect(() => {
    // console.log('여기 데이터');
    // console.log(data)
    if (collection != null) {
      if (collection == "goldbox") {
        if (data.is_p) {
          setPopupType("goldBox2");
          setTitle("Use Goldbox");
        } else {
          setPopupType("goldBox");
          setTitle("Use Goldbox");
        }
      } else if (collection === "gemblock") {
        setPopupType("parts");
        setTitle("Convert Gemblock");
        setConvertName("Gem Block");
      } else if (collection == "metafigure") {
        setPopupType("figure");
        setTitle("Use Meta Figure");
        setConvertName("Meta Figure");
      } else if (collection == "passive") {
        setPopupType("passive");
        setTitle("Use Passive");
      } else if (collection == "dragonticket") {
        setPopupType(null);
        setTitle("Use Dragon Ticket");
      } else if (collection == "petticket") {
        setPopupType(null);
        setTitle("Use Pet Ticket");
      } else if (collection == "mts") {
        setPopupType("gemBlock");
        setTitle("Use Meta Toy Squad");
      }
    }

    if(popupType === "passive")
    {
      const maxCount = data?.fullList?.items?.passive?.list?.filter(
        item => item.name === selectedData?.name
      ).length ?? 0;
      
      setQuantity(1);
      setMaxCount(Math.min(maxCount, 100));
    }
    else
    {
      setQuantity(1);
      setMaxCount(0);
    }
  }, [popupType, data]);

  //console.log(data, popupData);

  return (
    <>
      <div className="bg"></div>
      <div className="container">
        <div className="title">{title}</div>
        {
          //일반 골드박스 일때
          popupType === "goldBox" ? (
            <>
              <div className="content">
                <div className="imgBox2">
                  <div className="imgBoxDetail">
                    <img
                      src={
                        process.env.REACT_APP_TEST_PATH +
                        "/images/inventory/goldBox/VIP emblem.png"
                      }
                      alt="goldbox"
                    />
                    <div>Global Buff</div>
                  </div>
                  <div className="imgBoxLine"></div>
                  <div className="imgBoxDetail">
                    <img
                      src={
                        process.env.REACT_APP_TEST_PATH +
                        "/images/inventory/goldBox/24h_254_Boost.png"
                      }
                      alt="goldbox"
                    />
                    <div>Mining Boost</div>
                  </div>
                </div>
                {/* <div className="imgDesc">[Common] Dragon Gacha Ticket</div> */}
              </div>
              <div className="info font-b3-r">
                The item will be sent to your mailbox.
              </div>
            </>
          ) : //상품 많은 골드박스 일때
            popupType === "goldBox2" ? (
              <>
                <div className="content_goldBox">
                  <div>
                    <div className="content">
                      <div className="imgBox2">
                        <div
                          className={`imgBoxDetail_goldBox ${goldBoxChoose === "D"
                              ? "imgBoxDetail_goldBox_active"
                              : ""
                            }`}
                          onClick={() => setGoldBoxChoose("D")}
                        >
                          <img
                            src={
                              process.env.REACT_APP_TEST_PATH +
                              "/images/inventory/goldBox/Good.png"
                            }
                            alt="goldbox"
                          />
                          <div>Premium Gacha Ticket</div>
                        </div>
                        <div
                          className={`imgBoxDetail_goldBox ${goldBoxChoose === "P"
                              ? "imgBoxDetail_goldBox_active"
                              : ""
                            }`}
                          onClick={() => setGoldBoxChoose("P")}
                        >
                          <img
                            src={
                              process.env.REACT_APP_TEST_PATH +
                              "/images/inventory/goldBox/icon_good_pet.png"
                            }
                            alt="goldbox"
                          />
                          <div>Premium Pet Gacha Ticket</div>
                        </div>
                      </div>
                    </div>
                    <div className="info_goldBox">
                      This account has Global Buff applied.
                      <br />
                      Please choose one of the rewards.
                    </div>
                  </div>
                  <div>
                    <div className="content" style={{ padding: "15px 16.5px" }}>
                      <div className="imgBox2">
                        <div className="imgBoxDetail">
                          <img
                            src={
                              process.env.REACT_APP_TEST_PATH +
                              "/images/inventory/goldBox/24h_254_Boost.png"
                            }
                            alt="goldbox"
                          />
                          <div>Mining Boost</div>
                        </div>
                      </div>
                    </div>
                    <div className="info_goldBox">
                      Fixed rewards.
                      <br />
                      Items will be sent to your mailbox.
                    </div>
                  </div>
                </div>
              </>
            ) : //젬블럭 일때
              popupType === "gemBlock" ? ( // 보상이 젬블록 일 때
                <>
                  <div className="content">
                    <div className="imgBox2">
                      {data?.reward_details &&
                        data.reward_details.map((v, idx) => {
                          console.log(idx, data.reward_details.length);
                          return (
                            <React.Fragment key={idx}>
                              <div className="imgBoxDetail">
                                <img
                                  src={
                                    process.env.REACT_APP_TEST_PATH +
                                    `/images/inventory/gemBlock/${v.name}(Back).png`
                                  }
                                  alt="gemBlock"
                                />
                                <div>{v.name}</div>
                              </div>
                              {idx + 1 != data.reward_details.length && (
                                <div className="imgBoxLine"></div>
                              )}
                            </React.Fragment>
                          );
                        })}
                    </div>
                    {/* <div className="imgDesc">[Common] Dragon Gacha Ticket</div> */}
                  </div>
                  <div className="info">
                    {`Receive ${data.reward_details.length} random Gem Blocks via mailbox.`}
                  </div>
                </>
              ) : //피규어 일때
                popupType === "figure" ? (
                  <>
                    <div className="content">
                      <div className="imgBox2">
                        <div className="imgBoxDetail">
                          <img src={data.selectedItem.image} alt="figure" />
                          <div>{convertName}</div>
                        </div>
                      </div>
                      {/* <div className="imgDesc">[Common] Dragon Gacha Ticket</div> */}
                    </div>
                    <div className="info">The item will be sent to your mailbox.</div>
                  </>
                ) : //장비 일때
                  popupType === "parts" ? (
                    <>
                      <div className="content">
                        <div className="imgBox2">
                          <div className="imgBoxDetail">
                            <img src={data.selectedItem.image} alt="figure" />
                            <div>{convertName}</div>
                          </div>
                        </div>
                        {/* <div className="imgDesc">[Common] Dragon Gacha Ticket</div> */}
                      </div>
                      <div className="info">Please see the Gem Block in the Dragon tab.</div>
                    </>
                  ) :
                    popupType === "passive" ? (
                      <>
                        <div className="content">
                          <div className="imgBox2">
                            <img src={data.selectedItem.image} alt="passive" />
                            <div>{convertName}</div>
                            <div
                              style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "6px",
                                marginTop: "8px",
                                flexWrap: "nowrap",
                              }}
                            >
                              <span
                                style={{
                                  fontSize: "12px",
                                  color: "#ccc",
                                  whiteSpace: "nowrap",
                                }}
                              >
                                Quantity
                              </span>

                              <button
                                onClick={() => setQuantity(q => Math.max(1, q - 1))}
                                style={btnStyle}
                              >
                                −
                              </button>

                              <input
                                type="number"
                                min={1}
                                value={quantity}
                                onChange={(e) =>
                                  setQuantity(Math.max(1, Number(e.target.value)))
                                }
                                style={inputStyle}
                              />

                              <button
                                onClick={() => setQuantity(q => Math.min(maxCount, q + 1))}
                                style={btnStyle}
                              >
                                +
                              </button>

                              <button
                                onClick={() => setQuantity(maxCount)}
                                style={{ ...btnStyle, padding: "0 8px", width: "auto" }}
                              >
                                MAX
                              </button>
                            </div>

                          </div>
                        </div>
                        <div className="info">The item will be sent to your mailbox.</div>
                      </>
                    ) : (
                      //일반 드래곤 가챠일때
                      <>
                        <div className="content">
                          <div className="imgBox">
                            <img
                              src={
                                data?.selectedItem?.image ||
                                process.env.REACT_APP_TEST_PATH +
                                "/images/inventory/ticket/1_icon_common.png"
                              }
                              alt="default"
                            />
                          </div>
                          <div className="imgDesc">{data?.reward_details?.[0]?.name}</div>
                        </div>
                        <div className="info">The item will be sent to your mailbox.</div>
                      </>
                    )
        }

        <div className="popupButtons font-b3-b">
          <button className="cancelBtn white" onClick={closePopup}>
            Cancel
          </button>
          <button
            disabled={popupType == "goldBox2" && goldBoxChoose == null}
            className="signBtn white "
            onClick={async () => {
              if (!collection && !data?.selectedItem?.iv_id) return;

              const gradeValue = getGradeForItem(data.selectedItem);

              let apiParams = {
                collection: collection,
                iv_ids: "[" + data.selectedItem.iv_id + "]",
                grade: gradeValue,
                server_tag: sessionStorage.getItem("server_tag"),
                user_no: sessionStorage.getItem("user_no"),
              };

              if (popupType === 'goldBox2' && goldBoxChoose) {
                apiParams.grade = 'repeat';
                apiParams.gb_item = goldBoxChoose;
              }

              if (collection === 'gemblock') {
                apiParams.attributes = JSON.stringify(data.selectedItem.attributes);
              }
              
              if (popupType === 'passive') {
                apiParams.iv_ids = JSON.stringify(data?.fullList?.items?.passive?.list
                    ?.filter(item => item.name === selectedData?.name)
                    .slice(0, quantity).map(item => item.iv_id) ?? []);
              }

              const result = await window.DApp.post(
                `inventory/useitem`,
                apiParams
              );

              if (result && result.rs === 0) {
                // 성공했을 때 (rs 코드가 0일 경우)

                // 💡 1. 인벤토리 새로고침 함수 실행
                refreshInventoryList();

                openPopup({
                  type: "MessagePopup",
                  title: result?.title || "Successfully sent to Mailbox",
                  msg: "The item has been successfully used.",
                  // isRefresher: true, // 이 팝업이 닫힌 후 인벤토리 목록을 새로고침하려면 true
                });
              } else {
                // 실패했을 때
                openPopup({
                  type: "MessagePopup",
                  title: "Error",
                  // 서버가 보내준 에러 메시지가 있다면 보여주고, 없다면 기본 메시지를 보여줍니다.
                  msg: result?.msg || "An unexpected error occurred.",
                  isRefresher: true,
                });
              }
            }}
          >
            {/* <PlayWallet fill={"#FFFFFF"} /> */}
            Confirm
          </button>
        </div>
      </div>
    </>
  );
}
