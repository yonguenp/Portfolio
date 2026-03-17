import { CheckCircle, X } from 'lucide-react';
import styles from "../../../styles/BattleLeagueRight.module.css";

//Claim Lottery Prize
export default function PrizeClaimModal({ 
  prizeClaimResult, 
  onClose 
}) {

  if (!prizeClaimResult) return null;  

  const evIcon = '/images/icon/echovoucher.png';
  const magniteIcon = '/images/icon/magnite.png';

  // 숫자만 추출하고 콤마 포맷팅하는 함수
  const formatAmount = (amount) => {

    if (!amount) return '0';
    
    // 숫자만 추출 (예: "25000 EV" -> "25000")
    const numericValue = amount.toString().replace(/[^\d.]/g, '');
    
    // 소수점이 있는 경우와 없는 경우 처리
    if (numericValue.includes('.')) {
      const [integerPart, decimalPart] = numericValue.split('.');
      // .00인 경우 제거
      if (decimalPart === '00') {
        return parseInt(integerPart).toLocaleString();
      } else {
        return `${parseInt(integerPart).toLocaleString()}.${decimalPart}`;
      }
    } else {
      return parseInt(numericValue).toLocaleString();
    }
  };

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <div className={styles.prizeModalContent} onClick={(e) => e.stopPropagation()}>
        <div className={styles.prizeModalHeader}>
          <div className={styles.prizeModalTitle}>
            {prizeClaimResult.title || (
              prizeClaimResult.isSuccess 
                ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '상금 수령 완료' : 'Prize Claim Success')
                : (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '상금 수령 실패' : 'Prize Claim Failed')
            )}
          </div>
        </div>
        
        <div className={styles.prizeModalBody}>
          <div className={`${styles.prizeModalIcon} ${
            prizeClaimResult.isSuccess ? styles.prizeModalIconSuccess : styles.prizeModalIconError
          }`}>
            {prizeClaimResult.isSuccess ? (
              <CheckCircle className={styles.prizeModalIconSvg} />
            ) : (
              <X className={styles.prizeModalIconSvg} />
            )}
          </div>
          
          <div className={styles.prizeModalMessage}>
            {prizeClaimResult.message && (
              <div className={styles.prizeModalMessageText}>
                {prizeClaimResult.message}
              </div>
            )}
          </div>
          
          {prizeClaimResult.isSuccess && prizeClaimResult.reward && (
            <div className={styles.prizeModalReward}>
              <div className={styles.prizeModalRewardAmount}>
                <img src={prizeClaimResult.seasonId === 'R1' ? evIcon : magniteIcon} alt={prizeClaimResult.seasonId === 'R1' ? "EV" : "MN"} className={styles.prizeModalRewardIcon} />
                <span className={styles.prizeModalRewardValue}>
                  {formatAmount(prizeClaimResult.reward)}
                </span>
              </div>
              {/* <div className={styles.prizeModalRewardLabel}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "게임 내 지급 완료" : "Reward Distributed in Game"}
              </div> */}
            </div>
          )}
          
          <button 
            className={styles.prizeModalConfirmButton}
            onClick={onClose}
          >
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "확인" : "Confirm"}
          </button>
        </div>
      </div>
    </div>
  );
}
