import React, {useState} from "react";
import {usePopup} from "../../../context/PopupContext";
import { useSelector, useDispatch } from "react-redux";
// 💡 fetchStakingData Action을 import (경로는 맞춰야 합니다)
import { fetchStakingData } from "../../../data/stakingSlice";

export default function GenesisInsertCard(props) {
  const {openPopup, closePopup} = usePopup();

  const dispatch = useDispatch();

  const selected = useSelector((state) => state.staking.selectedStakingData);
  const [check1, setCheck1] = useState(false);
  const [check2, setCheck2] = useState(false);
  const [check3, setCheck3] = useState(false);
  const [check4, setCheck4] = useState(false);

  const refreshStakingList = () => {
    dispatch(fetchStakingData());
    console.log("Staking Data Re-fetched after insert.");
  };

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6">Insert Dragon Card</div>
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
              When you first insert a Dragon Card, there is a{" "}
              <span className="green-500">24 hours</span> waiting period before it becomes active.
            </label>
          </div>
        </div>
        <div
          className={`content_genesis mb-8 align-center justify-start gap-6 ${
            check2 === true ? "genesisCardCheckBoxActive" : ""
          }`}
        >
          <input
            type="checkBox"
            id="check2"
            className="genesisCardCheckBox"
            value={check2}
            onChange={() => setCheck2((prev) => !prev)}
          />
          <div className="gray-500">
            <label htmlFor="check2">
              After your Dragon Card is activated, you must visit the "Dragon Card" tab at least once to start the claim cycle.
            </label>
          </div>
        </div>
        <div
          className={`content_genesis mb-8 align-center justify-start gap-6 ${
            check3 === true ? "genesisCardCheckBoxActive" : ""
          }`}
        >
          <input
            type="checkBox"
            id="check3"
            className="genesisCardCheckBox"
            value={check3}
            onChange={() => setCheck3((prev) => !prev)}
          />
          <div className="gray-500">
            <label htmlFor="check3">
              Once the cycle begins, claims are available for collection every 6 hours.{" "}
              <span className="green-500">6 hours</span>
            </label>
          </div>
        </div>
        <div
          className={`content_genesis mb-8 align-center justify-start gap-6 ${
            check4 === true ? "genesisCardCheckBoxActive" : ""
          }`}
        >
          <input
            type="checkBox"
            id="check4"
            className="genesisCardCheckBox"
            value={check4}
            onChange={() => setCheck4((prev) => !prev)}
          />
          <div className="gray-500">
            <label htmlFor="check4">
              Dragon Cards can be removed after{" "}
              <span className="green-500">30 days</span> from insertion.
            </label>
          </div>
        </div>
        <div className="popupButtons font-b3-b white ">
          <button className="cancelBtn bg-gray-700 white" onClick={closePopup}>
            Cancel
          </button>
          <button
            className={`signBtn white ${
              check1 && check2 && check3 && check4 ? "" : " opacity-50 "
            }`}
            disabled={!check1 || !check2 || !check3 || !check4}
            onClick={async () => {
              //인서트!
              const insert_data = await window.DApp.post(`staking/insert`, {
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
            OK
          </button>
        </div>
      </div>
    </div>
  );
}
