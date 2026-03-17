import React, { useState } from "react";

export default function Exchange(props) {
  const { popupData, closePopup } = props;
  const [ok, setOk] = useState(false);

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        {ok === false ? (
          <>
            <div className="textBox">
              <div className="font-h6">Reconnect Wallet</div>
              <div className="font-b3-r gray-500">
                Are you trying to reconnect the wallet ?
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
                  console.log("forced");
                  window.DApp.request("force_connect");
                }}
              >
                {/* <PlayWallet fill={"#FFFFFF"} /> */}
                <div>Connect</div>
              </button>
            </div>
          </>
        ) : (
          <>
            <div className="textBox">
              <div className="font-h6">Failed</div>
              <div className="font-b3-r gray-500">
                지갑 주소가 다릅니다.
                <br />
                올바른 지갑이 연결되었는지 확인하세요.
              </div>
            </div>
            <div className="popupButtons font-b3-b">
              <div className="bg-red-400 cancelBtn" onClick={closePopup}>
                OK
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
