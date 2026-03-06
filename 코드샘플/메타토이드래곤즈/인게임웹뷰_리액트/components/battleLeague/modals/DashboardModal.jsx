import { useState } from 'react';
import { BarChart3 } from 'lucide-react';
import styles from "../../../styles/BattleLeagueRight.module.css";

export default function DashboardModal({ 
  isOpen, 
  onClose, 
  currentSeason, 
  rankingData,
  mbxIcon 
}) {
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  if (!isOpen) return null;

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <div className={styles.modalContent} onClick={(e) => e.stopPropagation()}>
        {/* <div className={styles.modalHeader}>
          <div className={styles.modalTitle}>
            <BarChart3 className={styles.modalTitleIcon} />
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "실시간 대시보드" : "Real-time Dashboard"}
          </div>
          <div className={styles.modalDescription}>
            Boss Raid 라운드 2 실시간 현황
          </div>
        </div> */}
        
        <div className={styles.dashboardContent}>              
          <div className={styles.dashboardRankingContentWrapper}>
            <div className={styles.dashboardRankingContent}>
              {/* Ranking List */}
              {rankingData && rankingData.length > 0 ? (
                <div className={styles.rankingList}>
                  {rankingData
                    .slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage)
                    .map((winner, index) => {
                      const actualRank = (currentPage - 1) * itemsPerPage + index + 1;
                      return (
                        <div key={actualRank} className={styles.rankingItem}>
                          <div className={styles.rankingItemContent}>
                            <div className={styles.rankingLeft}>
                              <div className={`${styles.rankBadge} ${styles[`rankBadge${actualRank}`]}`}>
                                {actualRank}
                              </div>
                              <div className={styles.rankingInfo}>
                                <div className={styles.rankingName}>{winner.name}</div>
                              </div>
                            </div>                             
                            <div className={styles.rankingScoreSection}>
                              <div className={styles.rankingScoreValue}>
                                {winner.score.toLocaleString()}
                              </div>
                              {/* <div className={styles.rankingScoreLabel}>
                                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "점수" : "Score"}
                              </div> */}
                            </div>                             
                          </div>
                        </div>
                      );
                    })}
                </div>
              ) : (
                <div className={styles.noDataMessage}>
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                    ? "데이터가 없습니다." 
                    : "No data available."}
                </div>
              )}
            </div>
          </div>

          {/* Pagination - 스크롤 영역 밖, 데이터가 있을 때만 표시 */}
          {rankingData && rankingData.length > itemsPerPage && (
            <div className={styles.pagination}>
              <button
                className={`${styles.paginationButton} ${currentPage === 1 ? styles.paginationButtonDisabled : ''}`}
                onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
              >
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "이전" : "Previous"}
              </button>
              
              {(() => {
                const totalPages = Math.ceil(rankingData.length / itemsPerPage);
                const maxVisiblePages = 5;
                let startPage = Math.max(1, currentPage - 2);
                let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);
                
                if (endPage - startPage + 1 < maxVisiblePages) {
                  startPage = Math.max(1, endPage - maxVisiblePages + 1);
                }
                
                const pages = [];
                for (let i = startPage; i <= endPage; i++) {
                  pages.push(i);
                }
                
                return pages.map((pageNum) => (
                  <button
                    key={pageNum}
                    className={`${styles.paginationPage} ${currentPage === pageNum ? styles.paginationPageActive : ''}`}
                    onClick={() => setCurrentPage(pageNum)}
                  >
                    {pageNum}
                  </button>
                ));
              })()}
              
              <button
                className={`${styles.paginationButton} ${currentPage === Math.ceil(rankingData.length / itemsPerPage) ? styles.paginationButtonDisabled : ''}`}
                onClick={() => setCurrentPage(Math.min(Math.ceil(rankingData.length / itemsPerPage), currentPage + 1))}
                disabled={currentPage === Math.ceil(rankingData.length / itemsPerPage)}
              >
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "다음" : "Next"}
              </button>
            </div>
          )}

          <button 
            className={styles.confirmButton}
            onClick={onClose}
          >
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "확인" : "Confirm"}
          </button>
        </div>
      </div>
    </div>
  );
}
