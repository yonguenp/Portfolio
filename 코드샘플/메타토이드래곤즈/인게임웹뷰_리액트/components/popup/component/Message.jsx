import React from "react";
import { ReactComponent as Arrow } from "../../../assets/svg/common/arrow(swap).svg";

export default function MessagePopup(props) {
  const { popupData, closePopup } = props;
  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6" style={{ marginBottom: "1rem" }}>
            {popupData.title}
          </div>
        </div>
        <div className="shopInfo font-b3-r text-center">{popupData.msg}</div>
        <div className="popupButtons font-b3-b">
          <div
            className="signBtn"
            onClick={() => {
              if (popupData.isWVCloser) {
                window.DApp.request("close");
              } else {
                if (popupData.isRefresher)
                  window.location.reload();
              }
              closePopup();
            }}
          >
            OK
          </div>
        </div>
      </div>
    </div>
  );
}
