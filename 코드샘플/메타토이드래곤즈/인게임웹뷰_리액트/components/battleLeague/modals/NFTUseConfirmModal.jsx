import { CheckCircle, Package } from 'lucide-react';
import styles from "../../../styles/BattleLeagueRight.module.css";

export default function NFTUseConfirmModal({ 
  isOpen, 
  onClose, 
  onConfirm,
  nftData
}) {
  if (!isOpen || !nftData) return null;

  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <div className={styles.prizeModalContent} onClick={(e) => e.stopPropagation()}>
        <div className={styles.prizeModalHeader}>
          <div className={styles.prizeModalTitle}>
            {isKorean ? "Battle Coin 사용 확인" : "Battle Coin Use Confirmation"}
          </div>
        </div>
        
        <div className={styles.prizeModalBody}>
          <div className={`${styles.prizeModalIcon} ${styles.prizeModalIconSuccess}`}>
            {(nftData?.meta?.image || process.env.REACT_APP_TEST_PATH + "/images/icon/battle_coin.png") && (
              <img 
                src={nftData?.meta?.image || process.env.REACT_APP_TEST_PATH + "/images/icon/battle_coin.png"} 
                alt={nftData?.id}
                style={{ width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'left center', borderRadius: '8px' }}
              />
            )}
          </div>
          
          <div className={styles.prizeModalMessage}>
            <h3 className={`${styles.prizeModalMessageTitle} ${styles.prizeModalMessageSuccess}`}>
              {isKorean ? "Battle Coin을 사용하시겠습니까?" : "Would you like to use the Battle Coin?"}
            </h3>
            <div className={styles.prizeModalMessageText}>
              {isKorean 
                ? "사용하시면 우편함으로 가게됩니다. 정말로 사용하시겠습니까?" 
                : "Using it will send it to your mailbox. Are you sure you want to use it?"}
            </div>
          </div>
          
          {/* <div className={styles.prizeModalReward}>
            <div className={styles.prizeModalRewardAmount}>
              <Package className={styles.prizeModalRewardIcon} />
              <span className={styles.prizeModalRewardValue}>
                #{nftData?.id}
              </span>
            </div>
            <div className={styles.prizeModalRewardLabel}>
              {isKorean ? "우편함으로 전송" : "Will be sent to mailbox"}
            </div>
          </div> */}
          
          <div style={{ display: 'flex', gap: '12px'}}>
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
              {isKorean ? "사용하기" : "Use"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
