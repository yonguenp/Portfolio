import React, { useState } from "react";
import { usePopup } from "../../../context/PopupContext";
import { useDispatch } from "react-redux";
import { fetchStakingData } from "../../../data/stakingSlice";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";

export default function GenesisClaim(props) {
  const { popupData, closePopup } = props;
  const { data } = popupData;
  const { openPopup } = usePopup();

  const [ok, setOk] = useState(false);

  const [check1, setCheck1] = useState(true);
  const [check2, setCheck2] = useState(true);

  // 💡 useDispatch 훅 초기화
  const dispatch = useDispatch();

  // 💡 스테이킹 목록 새로고침 함수 정의
  const refreshStakingList = () => {
    dispatch(fetchStakingData());
  };

  const { addMagnetUnlocked } = useDAppState();

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6">Reward Claim</div>
          <div className="content mt-0 mb-0 pt-8 pb-8">
            <div className="font-b4-r green-500 mb-4">Total Rewards</div>
            <div className="flex font-b1-b white justify-center gap-4">
              <img
                src={
                  process.env.REACT_APP_TEST_PATH +
                  "/images/icon/magnetblock.png"
                }
                alt="metod"
                style={{ width: "24px" }}
              />
              <div>{data}</div>
            </div>
          </div>
          <div className="font-b3-r gray-500">
            Claiming Magnetblock.
          </div>
        </div>
        <div className="popupButtons font-b3-b">
          <button className="cancelBtn white bg-gray-700" onClick={closePopup}>
            Cancel
          </button>
          <button
            className={`signBtn white ${
              check1 === true && check2 === true ? "" : " opacity-50 "
            }`}
            onClick={async () => {
              try {
                const response = await window.DApp.post(`staking/claim`, {
                  user_no: sessionStorage.getItem("user_no"),
                  server_tag: sessionStorage.getItem("server_tag"),
                  amount: data
                });

                // API 응답 처리 (성공/실패 팝업 표시 등)
                if (response && response.rs === 0) {

                  refreshStakingList();

                  await addMagnetUnlocked(data);

                  openPopup({
                    type: "MessagePopup",
                    title: "Success",
                    msg: "Claim successful!",
                    // isRefresher: true
                  });
                } else {
                  openPopup({
                    type: "MessagePopup",
                    title: "Error",
                    msg: response?.msg || "Claim failed. Please try again.",
                    isRefresher: true
                  });
                }
              } catch (error) {
                console.error("Claim API error:", error);
                openPopup({
                  type: "MessagePopup",
                  title: "Error",
                  msg: "An error occurred during the claim process.",
                  isRefresher: true
                });
              }
            }}
          >
            {/* <PlayWallet fill={"#FFFFFF"} /> */}
            Sign
          </button>
        </div>
      </div>
    </div>
  );
}
