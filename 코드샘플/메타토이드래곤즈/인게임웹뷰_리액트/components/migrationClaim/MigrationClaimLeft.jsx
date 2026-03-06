import { useState, useEffect } from 'react';
import styles from "../../styles/MigrationClaimLeft.module.css";
import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";
import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { ReactComponent as QuestShield } from "../../assets/svg/common/quest_shield.svg";
import { ReactComponent as QuestShieldBlur } from "../../assets/svg/common/quest_shield_blur.svg";
export default function MigrationClaimLeft({data}) {
  const { openPopup } = usePopup();
  
  const [ allocated_amount, setTotalPoint ]  = useState(0);
  const [ claimed_amount, setUsedPoint ] = useState(0);
  const [ remain_amount, setRemainPoint ] = useState(0);
  const [timeVal, setTimeVal] = useState(null);

  useEffect(() => {
    setTotalPoint(data.allocated);
    setUsedPoint(data.claimed);
    setRemainPoint(data.remain);
  }, [data])

  return (
    <div className={styles.container}>
        <div className={`${styles.containerLeftBox} bg-gray-900`}>
          <div className="mb-8 font-b2-b">Echo Voucher Information</div>
          <div
            className={`${styles.containerLeftBoxInner} bg-gray-900 font-b4-r`}
          >
            <div className={`${styles.rewardBox}`}>
              <div className="flex align-center justify-start font-b2-r white-500 mb-4 gap-2">
                <div>Allocated Amount</div>
                {/* <QuestionBig
                  onClick={() =>
                    openPopup({ type: "ArtBlockBuffPointManual", data: "card" })
                  }
                /> */}
              </div>
              <div className={`${styles.rewardBoxTop} mb-8`}>
                <div className="font-b1-b">{allocated_amount}</div>
              </div>

              <div className="flex align-center justify-start font-b2-r white-500 mb-4 gap-2">
                <div>Claimed Amount</div>
              </div>
              <div className={`${styles.rewardBoxTop} mb-8`}>
                <div className="font-b1-b green-500">{claimed_amount}</div>
              </div>

              <div className="flex align-center justify-start font-b2-r white-500 mb-4 gap-2">
                <div>Remaining Amount</div>
              </div>
              <div className={`${styles.rewardBoxTop} mb-8`}>
                <div className="font-b1-b yellow-500">{remain_amount}</div>
              </div>
            </div>
          </div>
        </div>

    </div>
  );
}
