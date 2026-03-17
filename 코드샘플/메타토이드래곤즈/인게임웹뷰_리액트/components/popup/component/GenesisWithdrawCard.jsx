import React, { useState } from "react";
import { usePopup } from "../../../context/PopupContext";
import { useSelector, useDispatch } from "react-redux";
import { fetchStakingData } from "../../../data/stakingSlice";

export default function GenesisWithdrawCard(props) {
  const { popupData, closePopup } = props;
  const { openPopup } = usePopup();
  const [check1, setCheck1] = useState(false);

  const selectedItems = popupData?.selected || [];

  const dispatch = useDispatch();

  const refreshStakingList = () => {
    dispatch(fetchStakingData());
    console.log("Staking Data Re-fetched after withdraw.");
  };

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6 red-500">Withdraw Dragon Card</div>
        </div>
        <div
          className={`content_genesis mb-8 align-center justify-start gap-6 ${
            check1 === true ? "genesisCardCheckBoxActive" : ""
          }`}
        >
          <input
            type="checkBox"
            id="check1"
            className="genesisCardCheckBox"
            value={check1}
            onChange={() => setCheck1((prev) => !prev)}
          />
          <div className="gray-500">
            <label htmlFor="check1">
            Agree to withdraw
              
            </label>
          </div>
        </div>
        
        <div className="popupButtons font-b3-b white ">
          <button className="cancelBtn bg-gray-700 white" onClick={closePopup}>
            Cancel
          </button>
          <button
            className={`signBtn white ${
              check1 ? "" : " opacity-50 "
            }`}
            disabled = {!check1}
            onClick={async () => {
              const selected = selectedItems.join(',');

              const insert_data = await window.DApp.post(`staking/withdraw`, {
                server_tag: sessionStorage.getItem("server_tag"),
                selected: selected,
              });

              if (insert_data && insert_data.rs === 0) {

                refreshStakingList();

                openPopup({
                  type: "MessagePopup",
                  title: "Success", // 팝업 제목
                  msg: "DApp: Cards inserted successfully.", // 팝업 내용
                  // isRefresher: true
                });

              } else {
                openPopup({
                  type: "MessagePopup",
                  title: "Error", // 팝업 제목
                  msg: insert_data?.msg || "An error occurred.", // 팝업 내용
                  isRefresher: true
                });
              }
            }}
          >
            I Agree
          </button>
        </div>
      </div>
    </div>
  );
}
