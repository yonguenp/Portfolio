import styles from "../../styles/BattleLeagueLeft.module.css";
import { usePopup } from "../../context/PopupContext";

export default function BattleLeagueLeft({ datas, selectedIndex, setSelectedIndex, seasons }) {
  const { openPopup } = usePopup();  

  // 시즌 서브타이틀 함수
  const getSeasonSubtitle = (season) => {
    return season.subtitle;
  };

  // 상태 배지 컴포넌트
  const getStatusBadge = (status) => {
    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
    
    switch (status) {
      case 'ongoing':
      case 'selling':
        return <div className={`${styles.statusBadge} ${styles.statusBadgeOngoing}`}>{isKorean ? '진행중' : 'Ongoing'}</div>;
      case 'completed':
        return <div className={`${styles.statusBadge} ${styles.statusBadgeCompleted}`}>{isKorean ? '완료' : 'Completed'}</div>;
      case 'upcoming':
        return <div className={`${styles.statusBadge} ${styles.statusBadgeUpcoming}`}>{isKorean ? '예정' : 'Upcoming'}</div>;
      default:
        return <div className={`${styles.statusBadge} ${styles.statusBadgeDefault}`}></div>;
    }
  };

  // MBX 아이콘
  //const mbxIcon = <img src="/images/icon/mbx.png" alt="MBX" className={styles.mbxIcon} />;
  const echovoucherIcon = <img src="/images/icon/echovoucher.png" alt="ECHOVOUCHER" className={styles.echovoucherIcon} />;
  const magniteIcon = <img src="/images/icon/magnite.png" alt="MAGNITE" className={styles.echovoucherIcon} />;

  // Sort seasons by sortOrder in ascending order
  const sortedSeasons = [...seasons].sort((a, b) => {
    const sortOrderA = a.sortOrder || 0;
    const sortOrderB = b.sortOrder || 0;
    return sortOrderA - sortOrderB;
  });

  return (
    <div className={styles.container}>
      <div className={styles.BattleLeagueContainer}>
        {/* Left Sidebar - Season Selection */}
        <div className={styles.seasonSidebar}>
          <div className={styles.sidebarContent}>
            
            
            <div className={styles.seasonList}>
              {sortedSeasons.map((season) => {
                const originalIndex = seasons.indexOf(season);
                const isSelected = selectedIndex === originalIndex;
                
                return (
                  <div
                    key={season.id}
                    className={styles.seasonWrapper}
                  >
                    <div
                      onClick={() => setSelectedIndex(originalIndex)}
                      className={`${styles.seasonItem} ${
                        isSelected ? styles.seasonItemActive : styles.seasonItemInactive
                      }`}
                    >
                      <div className={styles.seasonContent}>
                        <div className={styles.seasonHeader}>
                          <div className={`${styles.seasonName} ${
                            isSelected ? styles.seasonNameActive : styles.seasonNameInactive
                          }`}>
                            {season.id}
                          </div>
                          {getStatusBadge(season.status)}
                        </div>
                        
                        <div className={`${styles.seasonSubtitle} ${
                          isSelected ? styles.seasonSubtitleActive : styles.seasonSubtitleInactive
                        }`}>
                          {getSeasonSubtitle(season)}
                        </div>
                        
                        <div className={styles.seasonPrize}>
                          <span className={styles.echovoucherIcon}>{season.id === 'R1' ? echovoucherIcon : magniteIcon}</span>
                          <span className={`${styles.prizeAmount} ${
                            isSelected ? styles.prizeAmountActive : styles.prizeAmountInactive  
                          }`}>
                            {season.totalPrize.toLocaleString()}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
              
              {/* Upcoming seasons placeholder */}
              {/* <div className={styles.upcomingSeason}>
                <div className={styles.upcomingContent}>
                  <div className={styles.upcomingTitle}>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "S2 곧 출시" : "S2 Coming Soon"}</div>
                  <div className={styles.upcomingDate}>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "2025년 12월 예정" : "Expected December 2025"}</div>
                </div>
              </div> */}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
