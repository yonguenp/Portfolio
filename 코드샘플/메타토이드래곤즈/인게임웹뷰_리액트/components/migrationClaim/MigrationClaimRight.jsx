import { useState, useEffect } from 'react';
import styles from "../../styles/MigrationClaimRight.module.css";
import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { ReactComponent as QuestShield } from "../../assets/svg/common/quest_shield.svg";

export default function MigrationClaimRight({ data, setData }) {
  const { openPopup } = usePopup();

  const { userInfo, setUserInfo } = useDAppState();
  const [today, setToday] = useState(false);
  const [missionComplete, setMissionComplete] = useState(0);
  const [perClaim, setPerClaim] = useState('-');
  const [claimCount, setClaimCount] = useState(0);
  const [remainCount, setRemainCount] = useState(0);
  useEffect(() => {
    //console.log(data);
    setToday(data.today);
    setPerClaim(data.per_amount);
    setClaimCount(data.claim_count);
    setRemainCount(data.remain_count);
    setMissionComplete(data.mission_cleared ? 1 : 0);
  }, [data])

  const onClaim = async () => {
    if (!today || missionComplete <= 0 || remainCount <= 0)
      return;

    const response = await window.DApp.post(`echovoucher/claim`, {
      server_tag: sessionStorage.getItem("server_tag")
    });

    if (response && response.rs == 0) {
      if (response.info) {
        setData(response.info);
        setUserInfo(prev => ({
          ...prev,
          echovoucher: response.info.withdraw_amount,
        }));
      }

      openPopup({ type: "MessagePopup", title: "Success", msg: "Success Echo Voucher Claimed.", isRefresher: false })
    }
    else if(!response || !response.msg)
    {      
        openPopup({ type: "MessagePopup", title: "Fail", msg: "Claim Echo Voucher Fail", isRefresher: false })
    }
  }

  return (
    <div className={styles.container2}>
      <div className={styles.container}>
        <div className={`${styles.containerRightBox} bg-gray-900`}>

          <div className="font-b3-r">Echo Voucher Claim Status</div>
          <div className={`${styles.containerRightBoxInner} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox}`}>
              <div className="flex align-center justify-start font-b4-r white-500">
                {/* 클레임임당 금액 */}
                <div>Amount Per Claim</div>
              </div>
              <div className={`${styles.rewardBoxTop}`}>
                <div className="font-b5-r">{perClaim}</div>
              </div>
              <div className="flex align-center justify-start font-b4-r white-500">
                {/* 클레임 횟수 */}
                <div>Claimed Count</div>
              </div>

              <div className={styles.sliderWrapper}>
                <div className={styles.sliderTrack}>
                  <div
                    className={styles.sliderClaimed}
                    style={{
                      width: `${(claimCount / (claimCount + remainCount || 1)) * 100}%`,
                    }}
                  />
                </div>
                <div className={styles.sliderLabels}>
                  <span className={styles.claimed}>Claimed Count {claimCount}</span>
                  <span className={styles.remain}>Remain Count {remainCount}</span>
                </div>
              </div>
              <br />
            </div>
          </div>
          <br />
          <div className="font-b3-r">Misstion Status</div>
          <div className={`${styles.containerRightBoxInner} bg-gray-900 font-b4-r`}>
            <div className={`${styles.rewardBox}`}>
              <div className={`flex align-center justify-start font-b4-r ${missionComplete > 0 ? 'white' : 'red'}-500 mb-4 gap-8`}>
                <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/check.png"} style={
                  missionComplete > 0 ? { width: "16px", filter: "invert(51%) sepia(96%) saturate(441%) hue-rotate(90deg)" } : { width: "16px" }} />
                <div>{missionComplete > 0 ? (missionComplete == 1 ? "Daily Mission Completed" : "Completed " + missionComplete + " Daily Missions") : "Daily Mission is not completed yet."}</div>
              </div>
            </div>
          </div>
          <br />
          {/* <div
                className={`${styles.extractorBoxBottom2} font-b3-b`}
                // onClick={() => openPopup({ type: "ArtBlockConvert", data: props })}
              >
                <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/plus.png"} style={{ width: "16px" }}/>
                <span>Submit</span>
              </div> */}
          <div className={`${styles.extractorBoxBottom2} font-b3-b`} style={today && missionComplete > 0 && remainCount > 0 ? { background: "#ff9725" } : { background: "#797877" }} onClick={onClaim}>
            <img src={process.env.REACT_APP_TEST_PATH + "/images/icon/echovoucher.png"} style={{ width: "20px" }} />
            <span>{!today ? 'Already Claimed Today' : 'Claim Echo Voucher'}</span>
          </div>
          <br />
          <div className="font-b3-r">⚠️ Complete all daily missions each day to receive an Echo Voucher, up to 30 times in total.</div>
        </div>
      </div>
    </div>
  );
}
