import React, { useState } from "react";
import { usePopup } from "../../../context/PopupContext";

export default function ForceClaim(props) {
  const { popupData, closePopup } = props;
  const { data } = popupData;
  const { openPopup } = usePopup();

  const [ok, setOk] = useState(false);

  const [check1, setCheck1] = useState(true);
  const [check2, setCheck2] = useState(true);

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6 red-500">Caution</div>
          {data == 0 ? (
            <>
            <div className="font-b3-r gray-500">
            The daily vault is depleted.
            <br />
            Do you still want to claim?
            </div>
            <div className="font-b3-r red-500">
            * The claim timer will restart.
            </div>
            </>
          ):(
            <>
            <div className="font-b3-r gray-500">
            Insufficient daily vault.
            <br />
            Do you still want to claim?
            </div>
            <div className="font-b3-r red-500">
            * You might receive less than you estimated.
            <br />
            * If another wallet claims it first, you may not get it.
            <br />
            * The claim timer will restart.
            </div>
            </>
          )}
        </div>


        <div className="popupButtons font-b3-b">
          <button className="cancelBtn white bg-gray-700" onClick={closePopup}>
            Cancel
          </button>
          <button
                className={`signBtn white ${
                check1 === true && check2 === true ? "" : " opacity-50 "
                }`}
                onClick={() => window.DApp.genrTx("genesis.claim", { force:1 })}
            >
                {/* { data != 0 && (<PlayWallet fill={"#FFFFFF"} />) } */}
                { data == 0 ? 'OK' : 'Sign' }
            </button>          
        </div>
      </div>
    </div>
  );
}
