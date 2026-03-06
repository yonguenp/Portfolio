import { PartyPopper, Calendar } from "lucide-react";
import styles from "../../../styles/BattleLeagueRight.module.css";

export default function LotteryResultModal({ 
  isOpen, 
  onClose, 
  lotteryResultData,
  isWinner = false,
  userNFTs = [],
  lotteryPeriod
}) {
  if (!isOpen || !lotteryResultData) return null;

  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
  const winners = lotteryResultData?.lotteryResult?.winners || [];
  
  // 날짜 포맷팅 함수
  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${month}.${day}`;
  };
  
  // 상위 3명과 나머지 분리
  const topWinners = winners.slice(0, 3);
  const additionalWinners = winners.slice(3);

  // 사용자가 보유한 NFT 번호 추출 (tokenId만 숫자로)
  const userNFTNumbers = userNFTs.map(nft => {
    // tokenId에서 숫자만 추출 (예: "2468" -> "2468")
    return nft.tokenId?.toString() || nft.id?.replace('#', '');
  }).filter(Boolean);

  // 특정 당첨 번호가 사용자의 NFT인지 확인하는 함수
  const isUserWinningNumber = (winningNumber) => {
    const numberStr = winningNumber?.toString().replace('#', '');
    return userNFTNumbers.includes(numberStr);
  };

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <div className={styles.lotteryModalContent} onClick={(e) => e.stopPropagation()}>
        <div className={styles.lotteryModalHeader}>
          <div className={styles.lotteryModalTitle}>
            {isKorean ? "당첨자 발표" : "Winner Announcement"}
          </div>
          {/* {lotteryPeriod && (
            <div className={styles.lotteryModalDate}>
              <Calendar className={styles.lotteryModalDateIcon} />
              <span>{isKorean ? "추첨일: " : "Draw Date: "}{formatDate(lotteryPeriod)}</span>
            </div>
          )} */}
          <div className={styles.lotteryModalDescription}>
            {isWinner
              ? (isKorean ? "배틀코인 추첨 당첨자 발표" : "Are you one of the lucky winners?")
              : (isKorean ? "배틀코인 추첨 당첨자 발표" : "Unfortunately, You did not win this Raffle.")
            }
          </div>
        </div>
        
        <div className={styles.lotteryModalBody}>
          {/* <div className={styles.lotteryModalIcon}>
            <PartyPopper className={styles.lotteryModalIconSvg} />
          </div> */}
          
          <div className={styles.lotteryModalBody}>
            {/* <div className={styles.lotteryModalContentSubtitle}>
              {isKorean ? "TOP 3 당첨자" : "TOP 3 Winners"}
            </div> */}
            <div className={styles.lotteryNumbersContainer}>
              {topWinners.map((winner, index) => {
                const isUserNFT = isUserWinningNumber(winner.winningNumber);
                return (
                  <div 
                    key={index} 
                    className={`${styles.lotteryNumber} ${isUserNFT ? styles.lotteryNumberWinner : ''}`}
                  >
                    <div className={styles.lotteryRankBadge}>
                      {isKorean ? `${index + 1}등` : `#${index + 1}`}
                    </div>
                    <span className={styles.lotteryNumberText}>{winner.winningNumber}</span>
                    <div className={styles.lotteryPrizeText}>
                      {Math.floor(winner.prizeAmount).toLocaleString()} {winner.prizeUnit}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* 추가 당첨자 리스트 */}
          {additionalWinners.length > 0 && (
            <div className={styles.additionalWinnersSection}>
              {/* <div className={styles.lotteryModalContentSubtitle}>
                {isKorean ? "추가 당첨자" : "Additional Winners"}
              </div> */}
              <div className={styles.additionalWinnersGrid}>
                {additionalWinners.map((winner, index) => {
                  const isUserNFT = isUserWinningNumber(winner.winningNumber);
                  return (
                    <div 
                      key={index} 
                      className={`${styles.additionalWinnerGridItem} ${isUserNFT ? styles.additionalWinnerGridItemWinner : ''}`}
                    >
                      {winner.winningNumber}
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
