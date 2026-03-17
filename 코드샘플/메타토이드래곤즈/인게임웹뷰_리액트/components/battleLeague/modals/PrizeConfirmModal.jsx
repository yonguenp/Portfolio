import { CheckCircle, X, Star } from 'lucide-react';
import styles from "../../../styles/BattleLeagueRight.module.css";

//Claim Ranking Prize
export default function PrizeConfirmModal({ 
  isOpen, 
  onClose, 
  onConfirm,
  prizeData,
  mbxIcon 
}) {
  if (!isOpen || !prizeData) return null;

  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
  const evIcon = '/images/icon/echovoucher.png';
  const magniteIcon = '/images/icon/magnite.png';

  // 숫자만 추출하고 콤마 포맷팅하는 함수
  const formatAmount = (amount) => {
    if (!amount) return '0';
    
    // 숫자만 추출 (예: "250000 EV" -> "250000", "7500.00 EV" -> "7500.00")
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
            {isKorean ? "상금 수령 확인" : "Prize Claim Confirmation"}
          </div>
          {/* <div className={styles.prizeModalDescription}>
            {isKorean ? `${prizeData.title} 수령을 확인해주세요` : `Please confirm ${prizeData.title.toLowerCase()} claim`}
          </div> */}
        </div>
        
        <div className={styles.prizeModalBody}>
          <div className={`${styles.prizeModalIcon} ${styles.prizeModalIconSuccess}`}>
            <CheckCircle className={styles.prizeModalIconSvg} />
          </div>
          
          <div className={styles.prizeModalMessage}>
            <h3 className={`${styles.prizeModalMessageTitle} ${styles.prizeModalMessageSuccess}`}>
              {isKorean ? "상금을 수령하시겠습니까?" : "Would you like to claim the prize?"}
            </h3>
            {/* <div className={styles.prizeModalMessageText}>
              {isKorean 
                ? "수령 후에는 취소할 수 없습니다. 정말로 수령하시겠습니까?" 
                : "This action cannot be undone. Are you sure you want to claim this prize?"}
            </div> */}
          </div>
          
          <div className={styles.prizeModalReward}>
            <div className={styles.prizeModalRewardAmount}>
              <img src={prizeData.seasonId === 'R1' ? evIcon : magniteIcon} alt={prizeData.seasonId === 'R1' ? "EV" : "MN"} className={styles.prizeModalRewardIcon} />
              <span className={styles.prizeModalRewardValue}>
                {formatAmount(prizeData.amount)}
              </span>
            </div>
            {/* <div className={styles.prizeModalRewardLabel}>
              {prizeData.title}
            </div> */}
          </div>
          
          <div style={{ display: 'flex', gap: '12px', marginTop: '20px' }}>
            <button 
              className={styles.prizeModalConfirmButton}
              onClick={onClose}
              style={{ 
                background: '#616161',
                flex: 1
              }}
            >
              {isKorean ? "취소" : "Cancel"}
            </button>
            <button 
              className={styles.prizeModalConfirmButton}
              onClick={onConfirm}
              style={{ 
                background: '#4caf50',
                flex: 1
              }}
            >
              {isKorean ? "수령하기" : "Claim"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
