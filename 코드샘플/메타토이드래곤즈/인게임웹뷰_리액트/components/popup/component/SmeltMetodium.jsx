import React from "react";
import { ReactComponent as Arrow } from "../../../assets/svg/common/arrow(swap).svg";
import BigNumber from "bignumber.js";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";
import Magnet from "../../common/Magnet";
import Magnite from "../../common/Magnite";

export default function SmeltMetodium(props) {
  const { popupData, closePopup, onRefresh } = props;
  const { data } = popupData;
  const tmp_swapinfo = sessionStorage.getItem("exchange.info");
  const swapInfo = JSON.parse(tmp_swapinfo);
  const { userInfo, setUserInfo } = useDAppState();

  const smelt = async (amount) => {
    const data = await window.DApp.post("forge/exchange", {
      server_tag: sessionStorage.getItem("server_tag"),
      amount: amount,
    });
    
    if (data && data.rs == 0)
    {
      setUserInfo(prev => ({
        ...prev,
        magnet: data.magnet,
        magnite: data.magnite,
      }));
    }
    onRefresh();
    closePopup();
  }

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6">Smelt Magnite</div>
        </div>
        <div className="content" style={{ padding: "16px" }}>
          <div className="smeltBox">
            <div className="smelt_receipt_label">
              <p className="green-500 font-b4-b">Cost</p>
              <div className="upgradeBoxMtdz">
                <div>
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnetblock.png"
                    }
                    alt="magnet"
                    style={{ width: "1rem" }}
                  />
                </div>
                <div className="font-b4-r gray-500">
                  {Number(data?.amount || 0).toLocaleString()}
                </div>
              </div>
            </div>
            {/* <div className="smelt_receipt_label">
              <p className="green-500 font-b4-b">Fixed Fee</p>
              <div className="upgradeBoxMtdz">
                <div>
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnetblock.png"
                    }
                    alt="magnet"
                    style={{ width: "1rem" }}
                  />
                </div>
                <div className="font-b4-r gray-500">5</div>
              </div>
            </div> */}
            <div className="smeltBoxBottom">
              <p className="font-b4-b green-500">Total Cost</p>
              <div className="flex-center font-b3-b white">
                <img
                  src={
                    process.env.REACT_APP_TEST_PATH +
                    "/images/icon/magnetblock.png"
                  }
                  alt="magnet"
                  style={{ width: "24px" }}
                />
                {(Number(data?.amount || 0)).toLocaleString()}
              </div>
            </div>
          </div>
        </div>

        <div className="smeltGreenBox">
          <div className="font-b4-b green-500">Returns</div>
          <div className="smeltGreenBoxMtdz">
            <div>
              <img
                src={
                  process.env.REACT_APP_TEST_PATH +
                  "/images/icon/magnite.png"
                }
                alt="mtdz"
                style={{ width: "24px" }}
              />
            </div>
            <div className="font-b3-b white">{`${Number(
              new BigNumber(data?.amount || 0)
                .dividedBy(
                  swapInfo?.forge_swap_info?.[data?.userState?.arena_rank > 100 ? data?.userState?.arena_rank - 100 : data?.userState?.arena_rank]
                )
                .toFixed(3)
            )}`}</div>
          </div>
        </div>

        <div className="popupButtons font-b3-b">
          <button className="cancelBtn bg-gray-700 white" onClick={closePopup}>
            Cancel
          </button>
          <div
            className="signBtn bg-red-400"
            onClick={() => {
              smelt(new BigNumber(data?.amount || 0)
                  .dividedBy(
                    swapInfo?.forge_swap_info?.[data?.userState?.arena_rank > 100 ? data?.userState?.arena_rank - 100 : data?.userState?.arena_rank]
                  )
                  .toString());
            }}
          >
            {/* <PlayWallet style={{ marginTop: "-1px" }} /> */}
            <div>Smelt!</div>
          </div>
        </div>
      </div>
    </div>
  );
}
