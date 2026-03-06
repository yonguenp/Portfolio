import React, { useState } from "react";

export default function MBXStationPopup(props) {
  const { popupData, closePopup } = props;
  const [ok, setOk] = useState(false);

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
            <div className="textBox">
              <div className="font-h6">MBX STATION</div>
              <div className="font-b3-r gray-500">
                Are you trying to navigate to MBX Station ?
              </div>
            </div>
            <div className="popupButtons font-b3-b">
              <button
                className="cancelBtn bg-gray-700 white"
                onClick={closePopup}
              >
                Cancel
              </button>
              <button
                className="signBtn bg-red-400 white"
                onClick={() => {
                  if (Number(sessionStorage.getItem("test_mode"))){
                    window.open("https://station.marblex.io/en/detail/591ae6c17cd586f562ce", "_blank");
                  }else{
                    window.DApp.request("openexternal", {url:"https://station.marblex.io/en/detail/591ae6c17cd586f562ce"});
                  }
                  closePopup();
                }}
              >
                <div>OK</div>
              </button>
            </div>
      </div>
    </div>
  );
}
