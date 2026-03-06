import React from "react";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";

export default function Exchange(props) {
  
  const { userInfo, setUserInfo } = useDAppState();
  const { popupData, closePopup, onRefresh } = props;
  
  const build = async () => {    
    closePopup();
    const data = await window.DApp.post('forge/build', {
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
        msg: "Your Magnite Forge has been built.",
      });      
    }

    onRefresh();
  }

  let enough = false;
  if(50 <= (userInfo?.magnite || 0))
  {
    enough = true;
  }

  return (
    <div>
      <div className="bg"></div>
      <div className="container">
        <div className="textBox">
          <div className="font-h6">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트 제련소 건설하기" : "Build the Magnite Forge"}</div>
          <div className="font-b3-r gray-500">
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트를 사용하여 마그나이트 포지를 건설합니다." : "Building the Magnite Forge using Magnite."}
          </div>
        </div>
        <div className="popupButtons font-b3-b">
          <div className="cancelBtn bg-gray-700" onClick={closePopup}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "취소" : "Cancel"}
          </div>
          <div            
            className={`signBtn bg-red-400 ${ enough ? "" : "opacity-50"}`}
            onClick={() => {              
              if(enough)
                build();
            }}
          >
            <img
              src={
                process.env.REACT_APP_TEST_PATH + "/images/icon/magnite.png"
              }
              alt="metod"
            />
            <div>50</div>
          </div>
        </div>
      </div>
    </div>
  );
}
