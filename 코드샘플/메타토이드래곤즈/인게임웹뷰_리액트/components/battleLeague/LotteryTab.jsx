import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Dice6, Medal, Trophy } from 'lucide-react';
import styles from "../../styles/BattleLeagueRight.module.css";
import { fetchLotteryData } from "../../data/battleLeagueSlice";

// Raffle Reward Tab
export default function LotteryTab() {
  const dispatch = useDispatch();
  
  // Redux에서 데이터 가져오기
  const {     
    lotteryData, 
    loading,
    selectedSeasonIndex,
    selectedCategory,
    seasons
  } = useSelector((state) => state.battleLeague || {});

  // 현재 시즌 ID 가져오기
  const currentSeasonId = seasons[selectedSeasonIndex]?.id;

  // 추첨 데이터 API 호출
  useEffect(() => {

    dispatch(fetchLotteryData({ seasonId: currentSeasonId, category: selectedCategory }));
    
  }, [dispatch, currentSeasonId, selectedSeasonIndex, seasons]);

  // 로딩 상태 처리
  if (loading?.lottery) {
    return (
      <div>
        <div className={styles.loadingSpinner}>Loading...</div>
      </div>
    );
  }

  // Redux에서 가져온 데이터 사용 (실패 시 기본값 포함)
  const lotteryInfo = lotteryData?.raffleRewards?.[selectedCategory] || [];

  //console.log("lotteryInfo:", lotteryInfo);

  const rules = lotteryData?.rules || [];

  // echovoucher 이미지
  const echovoucherIcon = '/images/icon/echovoucher.png';
  const magniteIcon = '/images/icon/magnite.png';

  //is_Korean
  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

  // 랭킹 아이템 렌더링 함수
  const renderLotteryItem = (lotteryItem) => {
    // 1위만 노란색 배경 (topRankCard), 나머지는 투명 배경 (rankGroupCard)
    if (lotteryItem.isTopRank || lotteryItem.rank === 1) {
      return (
        <div key={lotteryItem.id} className={styles.topRankCard}>
          <div className={styles.topRankContent}>
            <div className={styles.topRankLeft}>
              <div className={styles.topRankBadge}>
                {lotteryItem.iconType === "Trophy" ? <Trophy className="w-4 h-4" /> : <Medal className="w-4 h-4" />}
              </div>
              <div className={styles.topRankInfo}>
                <div className={styles.topRankTitle}>{lotteryItem.title}</div>
                {lotteryItem.rewardCnt && <div className={styles.topRankSubtitle}>x{lotteryItem.rewardCnt}</div>}
              </div>
            </div>
            <div className={styles.topRankPrize}>
              <img src={currentSeasonId === 'R1' ? echovoucherIcon : magniteIcon} alt="EV" className={styles.evIcon} />
              <div className={styles.topRankPrizeAmount}>{lotteryItem.prize}</div>
            </div>
          </div>
        </div>
      );
    } else {
      return (
        <div key={lotteryItem.id} className={styles.rankGroupCard}>
          <div className={styles.rankGroupContent}>
            <div className={styles.rankGroupLeft}>
              <div className={styles.rankGroupBadge}>
                {lotteryItem.iconType === "Trophy" ? <Trophy className="w-4 h-4" /> : 
                 lotteryItem.iconType === "Medal" ? <Medal className="w-4 h-4" /> : 
                 lotteryItem.rank}
              </div>
              <div className={styles.rankGroupInfo}>
                <div className={styles.rankGroupTitle}>{lotteryItem.title}</div>
                {lotteryItem.rewardCnt && <div className={styles.rankGroupSubtitle}>x{lotteryItem.rewardCnt}</div>}
              </div>
            </div>
            <div className={styles.rankGroupRight}>
              <img src={currentSeasonId === 'R1' ? echovoucherIcon : magniteIcon} alt="EV" className={styles.evIcon} />
              <div className={styles.rankGroupPrizeAmount}>{lotteryItem.prize}</div>
            </div>
          </div>
        </div>
      );
    }
  }


  return (
    <div className={styles.lotteryContent}>
      {/* 추첨 정보
      <div className={styles.lotteryInfo}>
        <div className={styles.lotteryInfoTitle}>{lotteryInfo.title}</div>
        <div className={styles.lotteryInfoDetails}>
          {lotteryInfo?.details?.map((detail) => (
            <div key={detail.id} className={styles.lotteryInfoRow}>
              <span className={styles.lotteryInfoLabel}>{detail.label}</span>
              <span className={detail.isHighlight ? styles.lotteryInfoValueHighlight : styles.lotteryInfoValue}>
                {detail.value}
              </span>
            </div>
          ))}
        </div>
      </div>
       */}

      {/* 개인 랭킹 상금 분배표 */}
      <div className={styles.rankingContainer}>
        <div className={styles.rankingHeader}>
          <div className={styles.rankingHeaderTitle}>{lotteryInfo?.title || 'Raffle Reward'}</div>
          <div className={styles.rankingHeaderRight}>
            <img src={currentSeasonId === 'R1' ? echovoucherIcon : magniteIcon} alt="EV" className={styles.evIcon} />
            <div className={styles.rankingTotalBadge}>Total {lotteryInfo?.totalPrize || '0'}</div>
          </div>
        </div>

        <div className={styles.rankingList}>
          {lotteryInfo?.ranks?.map((rankItem) => {                       
            return renderLotteryItem(rankItem);
          })}
        </div>
      </div>

      {/* 규칙 섹션들 */}
      {rules?.map((rule) => (
        <div key={rule.id} className={styles.rulesSection}>
          <div className={styles.rulesHeader}>
            <div className={styles.rulesIcon}>{rule.stepNumber}</div>
            <span className={styles.rulesLabel}>{isKorean ? rule.titleKr : rule.title}</span>
          </div>
          <div className={styles.rulesContent}>
            {rule.items.map((item) => (
              <div key={item.id} className={styles.ruleItem}>
                <span className={styles.ruleBullet}>•</span>
                <span className={styles.ruleText}>{isKorean ? item.textKr : item.text}</span>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
