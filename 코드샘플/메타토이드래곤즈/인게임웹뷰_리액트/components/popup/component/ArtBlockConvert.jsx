import { useEffect, useState } from "react";
import { usePopup } from "../../../context/PopupContext";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";
import { useDispatch } from "react-redux";
import { updateSwapInfo } from "../../../data/artBlockSlice";

export default function ArtBlockConvert(props) {
  const { popupData, closePopup } = props;

  // Convert 모드 , From ,to
  // convertMode === false 일때 Amplifier Orb 가 상단
  const [convertMode, setConvertMode] = useState(true);
  const [disabled, setDisabled] = useState(false);

  const { openPopup } = usePopup();
  const dispatch = useDispatch();

  const ResetTime = new Date(popupData?.data?.swapInfo?.reset_time * 1000);
  //console.log(new Date(popupData.data.swapInfo.reset_time/100));
  //console.log(popupData?.data?.userInfo);

  const BuffPointBalance = popupData?.data?.swapInfo?.unlocked || 0;
  const MagnitePoint = popupData?.data?.userInfo?.magnite || 0;
  const TotalBuffPointBalance = popupData?.data?.swapInfo?.total_point || 0;
  const [inputValues, setInputValues] = useState({ input1: 0, input2: 0 });
  const { userInfo, setUserInfo } = useDAppState();

  const now = new Date();
    // 한국 시간 기준으로 맞추기
  const kstNow = new Date(now.toLocaleString("en-US", { timeZone: "Asia/Seoul" }));
  
  const ratio = 32;
  const maxAmount = 80000; 

  const handleChange = (event, inputName) => {
    //const inputValue = event.target.value;
    const inputValue = event.target.value.replace(/^0+/, "");

    const regex = /^[0-9\b]+$/;

    if (inputName == "input2") {
      if (inputValue === "" || regex.test(inputValue)) {        
        if (inputValue === "" || inputValue < 0) {
          setInputValues({
            ...inputValues,
            input1: 0,
            input2: 0,
          });
          return;
        }

        let val = inputValue;
        if(val < 10)
          val = inputValue * ratio;
        
        if(val + Number(TotalBuffPointBalance) > maxAmount)
        {
          val = (maxAmount - Number(TotalBuffPointBalance));
        }
        
        setInputValues({
          ...inputValues,
          input1: Math.ceil(val / ratio),
          input2: val,
        });
      }
    }
    else {
      if (inputValue === "" || regex.test(inputValue)) {
        if (inputValue === "" || inputValue < 0) {
          setInputValues({
            ...inputValues,
            input1: 0,
            input2: 0,
          });
          return;
        }

        let val = inputValue;
        if((val * 10) + Number(TotalBuffPointBalance) > maxAmount)
        {
          val = (maxAmount - Number(TotalBuffPointBalance)) / ratio;
        }

        // const factor = convertMode ? 10 : 0.1;
        setInputValues({
          ...inputValues,
          input1: val,
          input2: val * ratio,
        });
      }
    }
  };

  useEffect(() => {
    const isDisabled = convertMode
      ? inputValues.input1 > Number(MagnitePoint)
      : inputValues.input1 > Number(BuffPointBalance);
    const isZero = inputValues.input1 === 0 || inputValues.input2 === 0;

    setDisabled(isDisabled || isZero);
  }, [inputValues]);

  function maxFunc(mode) {
    //const factor = mode ? 10 : 0.1;
    // const value = mode
    //   ? Math.floor(MetodiumPoint)
    //   : Math.floor(BuffPointBalance);
    let value = MagnitePoint;

    if((value * ratio) + Number(TotalBuffPointBalance) > maxAmount)
    {
      value = (maxAmount - Number(TotalBuffPointBalance)) / ratio;
    }
    setInputValues({ ...inputValues, input1: value, input2: value * ratio });
  }

  async function buyBuffPoint() {
    if (disabled) {
      return;
    }

    setInputValues({
      ...inputValues,
      input1: inputValues.input1,
      input2: inputValues.input1 * 10,
    });
    //window.DApp.genrTx(convertMode ? "artblock.deposit" : "artblock.withdraw", {metod_amount: 0})

    if (Number(inputValues.input1) < 1) {
      openPopup({ type: "MessagePopup", title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "오류" : "Error", msg: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "수량을 확인해주세요." : "Please check the total balance.", isRefresher: false });
      return;
    }

    if (Number(inputValues.input2) + Number(TotalBuffPointBalance) > maxAmount) {
      openPopup({ type: "MessagePopup", title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "오류" : "Error", msg: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "수량을 확인해주세요." : "Please check the total balance.", isRefresher: false });
      return;
    }

    //console.log("메토디움(" + inputValues.input1 + ")에서 버프로 " + inputValues.input2);
    const data = await window.DApp.post("artblock/buy", {        
      server_tag : sessionStorage.getItem("server_tag"),
      magnite : inputValues.input1,
    });
    
    // console.log('buy : ', data);

    if (data && data.rs == 0)
    {
      window.DApp.emit("dapp.popup", {
        err: 0,
        title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "성공" : "Success",
        msg: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "구매에 성공하였습니다." : "Your purchase has been completed.",
      });
      
      setUserInfo(prev => ({
        ...prev,
        magnet: data.magnet,
        magnite: data.magnite,
      }));

      dispatch(updateSwapInfo(data.current));
    }
    else if(!data || !data.msg)
    {
      window.DApp.emit("dapp.popup", {
        err: 1,
        title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "오류" : "Error",
        msg: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "구매 시도 중 오류가 발생하였습니다." : "An error occurred while purchasing the product.",
      });
    }
  }

  return (
    <div>
      <div className="bg"></div>
      <div className="container" style={{ minWidth: "255.141px" }}>
        <div className="textBox">
          <div className="font-h6">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "구매" : "Purchase"}</div>
        </div>
        <div className="content mb-0 ">
          <div className="convertFrom flex align-center justify-between">
            <div className="font-b4-r gray-500">From</div>
            <div
              className="convertFromMaxBtn font-b4-b bg-red-400 white"
              onClick={() => maxFunc(convertMode)}
            >
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "최대" : "Max"}
            </div>
          </div>

          <div className="convertFrom flex align-center justify-between pb-0 pt-4">
            <div className="w-100 flex align-center justify-between">
              {convertMode === false ? (
                <div className={`flex align-center gap-2`}>
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/buff_point.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div className="font-b3-b white ">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭 구슬" : "Amplifier Orb"}</div>
                </div>
              ) : (
                <div className={`flex align-center gap-2`}>
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnite.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div className="font-b3-b white ">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트" : "Magnite"}</div>
                </div>
              )}
              <div className="fromtoMetodiumCount">
                <input
                  type="tel"
                  className="fromtoMetodiumCountInput font-b3-b"
                  placeholder="10"
                  value={inputValues.input1}
                  onChange={(event) => handleChange(event, "input1")}
                />
              </div>
            </div>
          </div>
          <div className="convertFrom flex align-center justify-between pt-0">
            <div className="font-b4-r gray-500">
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "수량 : " : "Balance : "} {convertMode === true ? MagnitePoint : BuffPointBalance}
            </div>
          </div>
        </div>
        {/* Swap Icon */}
        <div className="swapIconBox ">
          <div
            className="swapIcon flex justify-center align-center"
          // onClick={() => {
          //   console.log('스왑 없어짐');
          //   //setConvertMode((prev) => !convertMode);
          //   //setInputValues({ ["input1"]: 0, ["input2"]: 0 });
          // }}
          >
            <img
              src={
                process.env.REACT_APP_TEST_PATH + "/images/artblock/swap.png"
              }
              alt=""
              style={{ width: "24px", height: "24px" }}
            />
          </div>
        </div>

        <div className="content mt-16">
          <div className="convertFrom flex align-center justify-between">
            <div className="font-b4-r gray-500">To</div>
            {/* <div className="convertFromMaxBtn font-b4-b bg-red-400 white">Max</div> */}
          </div>

          <div className="convertFrom flex align-center justify-between pb-0 pt-4">
            <div className="w-100 flex align-center justify-between">
              {convertMode === false ? (
                <div className={`flex align-center gap-2`}>
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/magnite.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div className="font-b3-b white ">Magnite</div>
                </div>
              ) : (
                <div className={`flex align-center gap-2`}>
                  <img
                    src={
                      process.env.REACT_APP_TEST_PATH +
                      "/images/icon/buff_point.png"
                    }
                    alt="metod"
                    style={{ width: "24px", height: "24px" }}
                  />
                  <div className="font-b3-b white ">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭 구슬" : "Amplifier Orb"}</div>
                </div>
              )}
              <div className="fromtoMetodiumCount">
                <input
                  type="tel"
                  className="fromtoMetodiumCountInput font-b3-b"
                  placeholder="10"
                  value={inputValues.input2}
                  onChange={(event) => handleChange(event, "input2")}
                />
              </div>
            </div>
          </div>
          <div className="convertFrom flex align-center justify-between pt-0">
            {/* <div className="font-b4-r gray-500">Balance: 4,000</div>                     */}
          </div>
        </div>
        {convertMode && (
          <div className="font-b3-r gray-500 text-center">
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭 구슬은 " : "Amplifier Orbs are valid until "}
            <span className="green-500">{ResetTime.toLocaleString()}</span>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? " 까지 유효합니다." : ""}
          </div>
        )}
        <div className={`popupButtons font-b3-b ${convertMode ? "" : "mt-0"}`}>
          <div className="cancelBtn bg-gray-700" onClick={closePopup}>
            { navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "취소" : "Cancel" }
          </div>
          <div
            className={`signBtn ${disabled ? "opacity-50" : ""}`}
            onClick={() => buyBuffPoint()}
          >
            {/* <PlayWallet fill={"#FFFFFF"} /> */}
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "구매" : "Purchase"}
          </div>
        </div>
      </div>
    </div>
  );
}
