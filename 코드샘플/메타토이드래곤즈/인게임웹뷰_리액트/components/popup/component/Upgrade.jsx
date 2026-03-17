import React from "react";
import { ReactComponent as Arrow } from "../../../assets/svg/common/arrow(swap).svg";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";

export default function Upgrade(props) {
  
  const { userInfo, setUserInfo } = useDAppState();
  const { popupData, closePopup, onRefresh } = props;
  const { data } = popupData;

  const UpgradeForge = async () => {    
    //closePopup();
    const data = await window.DApp.post('forge/levelup', {
      server_tag: sessionStorage.getItem("server_tag")
    });

    if (data && data.rs == 0)
    {
      setUserInfo(prev => ({
        ...prev,
        magnite: data.magnite,
      }));

      window.DApp.emit("dapp.popup", {
        err: 0,
        title: "Success",
        msg: "Your Magnite Forge has been Upgraded.",
      });     
    }
    
    onRefresh();
  }

  let enough = false;
  if(data.next_level_info.cost <= (userInfo?.magnite || 0))
  {
    enough = true;
  }

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "단계 확장" : "Upgrade"}</div>
        </div>
        <div className="content" style={{ padding: "16px" }}>
          <div className="upgradeBox">
            <div className="gray-500">
              <div className="text-end font-b4-b">{`Lv.${Number(
                data.forge_level
              )}`}</div>
              <div className="text-end font-b4-b">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "일일 최대" : "Daily Max"}</div>
              <div className="upgradeBoxMtdz mt-4">
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
                <div className="font-b3-b white">
                  {data.curr_level_info.limit.toFixed(1)}
                </div>
              </div>
            </div>
            <div className="">
              <Arrow fill="#3A3838" />
            </div>
            <div className="gray-500 ">
              <div className="text-end green-500 font-b4-b">{`Lv.${
                Number(data.forge_level) + 1
              }`}</div>
              <div className="text-end green-500 font-b4-b">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "일일 최대" : "Daily Max"}</div>
              <div className="upgradeBoxMtdz mt-4">
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
                <div className="font-b3-b white">
                  {data.next_level_info.limit.toFixed(1)}
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="font-b3-r gray-500 text-center">
          {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "마그나이트 제련소를 업그레이드하세요." 
  : "Upgrade the Magnite Forge."}

        </div>
        <div className="popupButtons font-b3-b">
          <div className="cancelBtn bg-gray-700" onClick={closePopup}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
  ? "취소" 
  : "Cancel"}
          </div>
          <div
            className={`signBtn bg-red-400 ${ enough ? "" : "opacity-50"}`}
            onClick={() => {
              if(enough)
                UpgradeForge();
            }}
          >
            <img
              src={
                process.env.REACT_APP_TEST_PATH + "/images/icon/magnite.png"
              }
              alt="metod"
            />
            <div>{data.next_level_info.cost}</div>
          </div>
        </div>
      </div>
    </div>
  );
}
