import { useState, useEffect } from 'react';
import styles from "../../styles/ArtblockLeft.module.css";
import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";
import { usePopup } from "../../context/PopupContext";
import WalletGuard from "../guards/WalletGuard";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function ArtblockLeft(props) {
  const { openPopup } = usePopup();

  const { swapInfo } = props;

  const [ total_point, setTotalPoint ]  = useState(swapInfo?.total_point?swapInfo.total_point.toLocaleString() : 0);
  const [ used_point, setUsedPoint ] = useState(swapInfo?.used_point?swapInfo.used_point.toLocaleString() : 0);
  const [ remain_point, setRemainPoint ] = useState(swapInfo?.remain_point?swapInfo.remain_point.toLocaleString() : 0);
  const unlocked = swapInfo?.unlocked ? swapInfo.unlocked.toLocaleString() : 0;
  const [timeVal, setTimeVal] = useState(null);

  let interval = null;
  const calRemain = () => {
    if (swapInfo != null) {
      const now = Math.round((new Date()).getTime() / 1000);
      let remain = swapInfo.reset_time - now;

      if (remain > 0) {
        const day = parseInt(remain / 86400);
        remain -= (day * 86400);
        const hour = parseInt(remain / 3600);
        remain -= (hour * 3600);
        const min = parseInt(remain / 60);
        remain -= (min * 60);
        const sec = remain;

        let stringCount = 0;
        let val = '';
        if ((day > 0 && stringCount == 0)) {
          stringCount++;
          val += day + (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '일 ' : 'days ');
        }

        if ((hour > 0 && stringCount == 0) || stringCount < 2) {
          stringCount++;
          val += hour + (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '시간 ' : 'hours ');
          if (stringCount >= 2) {
            if(interval != null)
              clearInterval(interval);
            interval = setInterval(calRemain, 3600 * 1000);
          }
        }

        if ((min > 0 && stringCount == 0) || stringCount < 2) {
          stringCount++;
          val += min + (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '분 ' : 'mins ');
          if (stringCount >= 2) {
            if(interval != null)
              clearInterval(interval);
            interval = setInterval(calRemain, 3600 * 1000);
          }
        }

        if (stringCount < 2) {
          val += sec + (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '초 ' : 'secs ');
          stringCount++;

          if(interval != null)
            clearInterval(interval);
          interval = setInterval(calRemain, 1000);
        }
        else {
          val += '..';
        }

        val += (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? ' 남음' : ' left');

        setTimeVal(val);

        setTotalPoint(swapInfo.total_point.toLocaleString());
        setUsedPoint(swapInfo.used_point.toLocaleString());
        setRemainPoint(swapInfo.remain_point.toLocaleString());
      }
      else {
        setTotalPoint(0);
        setUsedPoint(0);
        setRemainPoint(0);
        setTimeVal(navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "드래곤 증폭기가 갱신되었습니다." : "Amplifier Orbs are updated.");
      }
    }
  }

  useEffect(() => {
    calRemain();
  })

  return (
    <div className={styles.container}>
        <div className={`${styles.containerLeftBox} bg-gray-900`}>
          <div className="mb-8 font-b3-b">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "상태" : "Status" }</div>
          <div
            className={`${styles.containerLeftBoxInner} bg-gray-900 font-b4-r`}
          >
            <div className={`${styles.rewardBox}`}>
              <div className="flex align-center justify-start font-b4-r green-500 mb-4 gap-2">
                <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭 구슬" : "Amplifier Orb" }</div>
                <QuestionBig
                  onClick={() =>
                    openPopup({ type: "ArtBlockBuffPointManual", data: "card" })
                  }
                />
              </div>
              <div className={`${styles.rewardBoxTop} mb-8`}>
                <img
                  src="/images/icon/buff_point.png"
                  alt="metod"
                  style={{ width: "24px", height: "24px" }}
                />
                <div className="font-b1-b">{total_point}</div>
              </div>
              <div className={`${styles.rewardBoxBottom} font-b4-r`}>
                <div className={`${styles.rewardBoxButtons} bg-gray-800`}>
                  <div className="ml-8 gray-500">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "총 활성" : "Total Active"}</div>
                  <div className="ml-8 font-b4-b green-500">{used_point}</div>
                </div>
                <div className={`${styles.rewardBoxButtons} bg-gray-800`}>
                  <div className="ml-8 gray-500">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "비활성" : "Inactive"}</div>
                  <div className="ml-8 font-b4-b">{remain_point}</div>
                </div>
              </div>
              <div
                className={`${styles.extractorBoxBottom} bg-red-400 font-b3-b`}
                onClick={() => openPopup({ type: "ArtBlockConvert", data: props })}
              >
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "구매" : "Purchase"}
              </div>
              <div> <br></br> </div>
              {
                swapInfo && swapInfo.reset_time > Math.round((new Date()).getTime() / 1000) && 
                (
                  <div className="flex align-center justify-start font-b4-r yellow-500 mb-4 gap-2">
                    {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "남은 초기화 시간" : "Remain Reset Time"}
                  </div>
                )
              }
              <p>{timeVal}</p>
              { 
                swapInfo && swapInfo.reset_time < Math.round((new Date()).getTime() / 1000) && 
                (<p> {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "페이지를 새로고침해주세요." : "Please reload this page."}</p>)
              }
              {/* <div className={`${styles.rewardBoxBottom} font-b4-r`}>
                            <div className={styles.rewardBoxes}>
                                <div className="font-b6-r gray-500">Unlocked</div>  
                                <div className="font-b4-b white">999,999</div>
                            </div>
                            <div className={styles.rewardBoxes}>
                                <div className="font-b6-r gray-500">Unlocked</div>
                                <div className="font-b4-b white">999,999</div>
                            </div>
                        </div> */}
            </div>
          </div>
        </div>
        {/* 
            <div className={`${styles.containerLeftBox} bg-gray-900`}>
                <div className="mb-8 font-b3-b">
                Swap
                </div>
                <div className={`${styles.containerLeftBoxInner} bg-gray-900 font-b4-r`}>
                    <div className="w-100">
                        <div className={`${styles.rewardBox}`}>
                            <div className="font-b4-r gray-500 mb-4 flex justify-between">
                                <div>From</div>
                                <div className={`${styles.maxBtn} bg-red-400 font-b4-b white`}>Max</div>
                            </div>
                            <div className={`${styles.rewardBoxTop}`}>
                                <img src="/images/icon/magnite.png" alt="metod" style={{width:'24px', height:'24px'}}/>                      
                                <div className="w-100 font-b3-b flex align-center">
                                    <div>Metodium</div>
                                    <div className={`${styles.fromtoMetodiumCount}`}>
                                        <input type="text" className={`${styles.fromtoMetodiumCountInput} font-b3-b`} placeholder='10'/>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className={`${styles.rewardBox}`}>
                            <div className="font-b4-r gray-500 mb-4">
                                <div>To</div>
                            </div>
                            <div className={`${styles.rewardBoxTop}`}>
                                <img src="/images/icon/magnite.png" alt="metod" style={{width:'24px', height:'24px'}}/>                      
                                <div className="w-100 font-b3-b flex align-center">
                                    <div>Energy</div>
                                    <div className={`${styles.fromtoMetodiumCount}`}>
                                        <input type="text" className={`${styles.fromtoMetodiumCountInput} font-b3-b`} placeholder='10'/>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
             */}
    </div>
  );
}
