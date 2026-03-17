import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Trophy, Medal } from 'lucide-react';
import styles from "../../styles/BattleLeagueRight.module.css";
import { fetchRankingData } from "../../data/battleLeagueSlice";
import { getServerTag, ServerTag } from "../../data/data";

// Ranking Reward Tab
export default function RankingTab() {
  const dispatch = useDispatch();
  
  // Redux에서 데이터 가져오기
  const { 
    rankingData, 
    loading,
    selectedSeasonIndex,
    selectedCategory,
    seasons 
  } = useSelector((state) => state.battleLeague || {});

  // 현재 시즌 ID 가져오기
  const currentSeasonId = seasons[selectedSeasonIndex]?.id;
  
  // server_tag enum 사용
  const serverTag = getServerTag();

  //console.log("serverTag:", serverTag);
  //console.log("ServerTag:", ServerTag);

  // 랭킹 데이터 API 호출
  useEffect(() => {
    
    // 시즌 ID가 있고, 현재 로딩 중이 아닐 때만 API 호출
    if (currentSeasonId && !loading?.ranking) {
      //console.log("랭킹 데이터 API 호출 시도:", currentSeasonId);
      dispatch(fetchRankingData(currentSeasonId));
    } else {
      //console.log("랭킹 데이터 API 호출 조건 미충족 - currentSeasonId:", !!currentSeasonId, "loading:", loading?.ranking);
    }
  }, [dispatch, currentSeasonId, selectedSeasonIndex, seasons]); // loading 제거로 무한 루프 방지

  // 로딩 상태 처리
  if (loading?.ranking) {
    return (
      <div className={styles.rankingContainer}>
        <div className={styles.loadingSpinner}>Loading...</div>
      </div>
    );
  }

  // Redux에서 가져온 데이터 사용 (실패 시 기본값 포함)
  const individualRanking = rankingData?.individualRanking || {};
  const unionRanking = rankingData?.unionRanking || {};
  const currentRanking = individualRanking?.[selectedCategory];
  
  const echovoucherIcon = '/images/icon/echovoucher.png';
  const magniteIcon = '/images/icon/magnite.png';

  // R4 이상에서 서버별 값 가져오는 헬퍼 함수
  const getServerSpecificValue = (value) => {
    // R4 이상인지 확인 (숫자 부분만 추출해서 비교)
    const seasonNumber = currentSeasonId ? parseInt(currentSeasonId.replace('R', '')) : 0;
    const isR4OrAbove = seasonNumber >= 4;
    
    if (!isR4OrAbove) {
      // R1, R2, R3는 기존 방식대로 반환
      return value;
    }
    
    // 문자열이고 "|"가 포함되어 있는지 체크
    if (typeof value === 'string' && value.includes('|')) {
      // "|"로 split해서 배열로 만들기
      const valueArray = value.split('|');
      // serverTag "1" -> index 0, "2" -> index 1, "3" -> index 2
      const serverIndex = parseInt(serverTag) - 1;
      return valueArray[serverIndex] !== undefined ? valueArray[serverIndex].trim() : (valueArray[0]?.trim() || '');
    }
    
    // 배열인 경우도 처리 (혹시 모를 경우를 대비)
    if (Array.isArray(value)) {
      const serverIndex = parseInt(serverTag) - 1;
      return value[serverIndex] !== undefined ? value[serverIndex] : (value[0] || '');
    }
    
    // 배열이 아니고 "|"도 없으면 기존 값 반환
    return value;
  };

  // 랭킹 아이템 렌더링 함수
  const renderRankItem = (rankItem, isUnion = false) => {
    if (rankItem.isTopRank) {
      return (
        <div key={rankItem.id} className={styles.topRankCard}>
          <div className={styles.topRankContent}>
            <div className={styles.topRankLeft}>
              <div className={styles.topRankBadge}>
                {rankItem.iconType === "Trophy" ? <Trophy className="w-4 h-4" /> : <Medal className="w-4 h-4" />}
              </div>
              <div className={styles.topRankInfo}>
                <div className={styles.topRankTitle}>{rankItem.title}</div>
                {rankItem.subtitle && <div className={styles.topRankSubtitle}>{rankItem.subtitle || 'Prize Distribution'}</div>}
              </div>
            </div>
            <div className={styles.topRankPrize}>
              <img src={currentSeasonId === 'R1' ? echovoucherIcon : magniteIcon} alt="EV" className={styles.evIcon} />
              <div className={styles.topRankPrizeAmount}>{getServerSpecificValue(rankItem.prize)} {getServerSpecificValue(rankItem.prizeUnit)}</div>
            </div>
          </div>
        </div>
      );
    } else {
      return (
        <div key={rankItem.id} className={styles.rankGroupCard}>
          <div className={styles.rankGroupContent}>
            <div className={styles.rankGroupLeft}>
              <div className={styles.rankGroupBadge}>
                {rankItem.iconType === "Trophy" ? <Trophy className="w-4 h-4" /> : 
                 rankItem.iconType === "Medal" ? <Medal className="w-4 h-4" /> : 
                 rankItem.rank}
              </div>
              <div className={styles.rankGroupInfo}>
                <div className={styles.rankGroupTitle}>{rankItem.title}</div>
              </div>
            </div>
            <div className={styles.rankGroupRight}>
              <img src={currentSeasonId === 'R1' ? echovoucherIcon : magniteIcon} alt="EV" className={styles.evIcon} />
              <div className={styles.rankGroupPrizeAmount}>{getServerSpecificValue(rankItem.prize)} {getServerSpecificValue(rankItem.prizeUnit)}</div>
            </div>
          </div>
        </div>
      );
    }
  };

  return (
    <>

      {/* Union일 때만 조합 랭킹 상금 분배표 추가 */}
      {selectedCategory === 'union' ? (
        <div className={styles.rankingContainer}>
          <div className={styles.rankingHeader}>
            <div className={styles.rankingHeaderTitle}>{unionRanking?.title || 'Prize Distribution'}</div>
            <div className={styles.rankingHeaderRight}>
              <img src={magniteIcon} alt="MN" className={styles.evIcon} />
              <div className={styles.rankingTotalBadge}>Total {getServerSpecificValue(unionRanking?.totalPrize)}</div>
            </div>
          </div>

          <div className={styles.rankingList}>
            {unionRanking?.ranks?.map((rankItem) => renderRankItem(rankItem, true))}
          </div>          
        </div>
      ) :
      <>      
        {/* 개인 랭킹 상금 분배표 */}
        <div className={styles.rankingContainer}>
          <div className={styles.rankingHeader}>
            <div className={styles.rankingHeaderTitle}>{currentRanking?.title || 'Prize Distribution'}</div>
            <div className={styles.rankingHeaderRight}>
              <img src={currentSeasonId === 'R1' ? echovoucherIcon : magniteIcon} alt="EV" className={styles.evIcon} />
              <div className={styles.rankingTotalBadge}>Total {getServerSpecificValue(currentRanking?.totalPrize)}</div>
            </div>
          </div>

          <div className={styles.rankingList}>
            {currentRanking?.ranks?.map((rankItem) => {
              return renderRankItem(rankItem);
            })}
          </div>
        </div>
      </>
      }

      {/* 주의사항 */}
      <div className={styles.rulesSection}>
        <div className={styles.rulesHeader}>          
          <span className={styles.rulesLabel}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "주의사항" : "How to claim your Prize"}
          </span>
        </div>
        <div className={styles.rulesContent}>
          <div className={styles.ruleItem}>
            <span className={styles.ruleBullet}>•</span>
            <span className={styles.ruleText}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                ? "랭킹은 실시간으로 업데이트됩니다." 
                : "Check the My Status tab and Click the ‘Claim Ranking Prize’ button after the round ends."}
            </span>
          </div>
          {currentSeasonId === 'R1' && (
            <div className={styles.ruleItem}>
              <span className={styles.ruleBullet}>•</span>
              <span className={styles.ruleText}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                  ? "시즌 종료 후 상금이 지급됩니다." 
                  : "You can then convert your 'Echo Vouchers' to gMBX in the MBX Station."}
              </span>
            </div>
          )}          
        </div>
      </div>

    </>
  );
}