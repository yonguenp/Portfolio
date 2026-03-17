import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { 
  Trophy, User, Users, CheckCircle, BarChart3, 
  Package, Skull, ExternalLink, Zap, Gift
} from 'lucide-react';
import styles from "../../styles/BattleLeagueRight.module.css";
import { fetchParticipationData } from "../../data/battleLeagueSlice";
import NFTUseConfirmModal from './modals/NFTUseConfirmModal';

// My Status Tab
export default function ParticipationTab({
  setShowDashboardModal,
  handleRankingPrizeClaim,
  handleLotteryPrizeClaim
}) {
  const dispatch = useDispatch();
  
  // Redux에서 데이터 가져오기
  const { 
    participationData, 
    loading, 
    selectedSeasonIndex, 
    selectedCategory,
    seasons 
  } = useSelector((state) => state.battleLeague || {});
  
  // NFT 사용 확인 모달 상태
  const [showNFTUseModal, setShowNFTUseModal] = useState(false);
  const [selectedNFT, setSelectedNFT] = useState(null);
  
  // 보상 수령 기간 체크 함수
  const isRewardPeriod = () => {
    const currentSeason = seasons[selectedSeasonIndex];

    if (!currentSeason?.rewardStartDate || !currentSeason?.rewardEndDate) {
      return false;
    }
    
    // 현재 날짜 (Date 객체)
    const currentDate = new Date();
    
    // rewardStartDate와 rewardEndDate를 Date 객체로 변환
    // 형식: "2025-11-18 00:00:00" -> Date 객체
    const parseDateString = (dateString) => {
      // "2025-11-18 00:00:00" 형식을 파싱
      // 공백을 'T'로 바꾸고 로컬 시간대로 파싱
      const formattedDate = dateString.replace(' ', 'T');
      const parsedDate = new Date(formattedDate);
      
      // Invalid Date 체크
      if (isNaN(parsedDate.getTime())) {
        console.error('Invalid date string:', dateString);
        return null;
      }
      
      return parsedDate;
    };
    
    const startDate = parseDateString(currentSeason.rewardStartDate);
    const endDate = parseDateString(currentSeason.rewardEndDate);

    // 날짜 파싱 실패 시 false 반환
    if (!startDate || !endDate) {
      console.error('Failed to parse dates:', {
        rewardStartDate: currentSeason.rewardStartDate,
        rewardEndDate: currentSeason.rewardEndDate
      });
      return false;
    }
    
    return currentDate >= startDate && currentDate <= endDate;
  };
  
  
  // 현재 시즌 ID 가져오기
  const currentSeasonId = seasons[selectedSeasonIndex]?.id;
  
  // API 호출 및 보상 기간 체크 (시즌 ID가 변경될 때만)
  useEffect(() => {        
    // 시즌 ID가 있고, 현재 로딩 중이 아닐 때만 API 호출
    if (currentSeasonId && !loading?.participation) {      
      dispatch(fetchParticipationData(currentSeasonId));
    }
    
    // 시즌 변경 시 보상 기간 체크
    if (selectedSeasonIndex !== undefined && seasons[selectedSeasonIndex]) {
      isRewardPeriod();
    }
  }, [dispatch, currentSeasonId, selectedSeasonIndex, seasons]); // loading 제거로 무한 루프 방지
  
  // NFT 사용 핸들러
  const handleNFTUse = (nft) => {
    if (nft?.status === 'available') {
      setSelectedNFT(nft);
      setShowNFTUseModal(true);
    }
  };

  // NFT 사용 확인
  const handleConfirmNFTUse = async () => {
    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

    if (!currentSeasonId || !selectedNFT?.id) {
      window.DApp.emit("dapp.popup", {
        err: 1,
        title: isKorean ? "오류" : "Error",
        msg: isKorean ? "시즌 ID 또는 NFT 토큰 ID가 없습니다!" : "Season ID or NFT token ID is not found!"
      });
      return;
    }
    
    try {
      console.log('NFT 사용 확인:', selectedNFT);

      // 우편함 아이템 전송 API 호출 (DApp에서 자동으로 로딩 처리)
      const response = await window.DApp.post(`/battleleague/participation/nftuse`, {
        season_id: currentSeasonId || '',
        iv_id: selectedNFT?.iv_id || '',
        nft_id: selectedNFT?.id || '',
        server_tag: sessionStorage.getItem("server_tag"),
      });

      // API 응답 확인
      if (response && response.rs === 0) {
        // DApp 모달 형식으로 성공 메시지 표시
        window.DApp.emit("dapp.popup", {
          err: 0,
          title: isKorean ? "성공" : "Success",
          msg: isKorean ? "아이템이 우편함으로 전송되었습니다!" : "Item has been sent to your mailbox!"
        });
        
        // 참여 데이터 다시 불러오기 (NFT 상태 업데이트를 위해)
        if (currentSeasonId) {
          dispatch(fetchParticipationData(currentSeasonId));
        }
      }
      // 에러는 DApp.post 내부에서 자동으로 팝업 표시
    } catch (error) {
      console.error('NFT 사용 API 에러:', error);
    } finally {
      setShowNFTUseModal(false);
      setSelectedNFT(null);
    }
  };
  
  // 로딩 상태 처리
  if (loading?.participation) {
    return (
      <div className={styles.participationContent}>
        <div className={styles.loadingSpinner}>
          {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "로딩 중..." : "Loading..."}
        </div>
      </div>
    );
  }
  
  // API에서 반환된 데이터 사용 (실패 시 기본값 포함)
  const gameUserInfo = participationData?.userInfo; // 게임 참여자 정보
  const nftList = participationData?.nftList;
  
  // 테마 설정
  const getThemeColors = () => {
    const themeConfig = {
      arena: {
        statusText: 'The Arena',
        icon: Trophy,
        colors: {
          borderColor: 'border-purple-500/30',
          borderColorLight: 'border-purple-500/20',
          iconColor: 'text-purple-400',
          labelColor: 'text-purple-200',
          valueColor: 'text-purple-300',
          bgGradient: 'bg-gradient-to-br from-purple-500 to-pink-500',
          badgeColor: 'bg-purple-500/20 text-purple-300',
          buttonGradient: 'bg-gradient-to-r from-purple-500 to-pink-500 hover:from-purple-600 hover:to-pink-600',
          buttonGradientSecondary: 'bg-gradient-to-r from-purple-400 to-pink-400 hover:from-purple-500 hover:to-pink-500'
        }
      },
      bossRaid: {
        statusText: 'Boss Raid',
        icon: Skull,
        colors: {
          borderColor: 'border-red-500/30',
          borderColorLight: 'border-red-500/20',
          iconColor: 'text-red-400',
          labelColor: 'text-red-200',
          valueColor: 'text-red-300',
          bgGradient: 'bg-gradient-to-br from-red-500 to-orange-500',
          badgeColor: 'bg-red-500/20 text-red-300',
          buttonGradient: 'bg-gradient-to-r from-red-500 to-orange-500 hover:from-red-600 hover:to-orange-600',
          buttonGradientSecondary: 'bg-gradient-to-r from-yellow-500 to-amber-500 hover:from-yellow-600 hover:to-amber-600'
        }
      },
      union: {
        statusText: 'The Union',
        icon: Users,
        colors: {
          borderColor: 'border-blue-500/30',
          borderColorLight: 'border-blue-500/20',
          iconColor: 'text-blue-400',
          labelColor: 'text-blue-200',
          valueColor: 'text-blue-300',
          bgGradient: 'bg-gradient-to-br from-blue-500 to-teal-500',
          badgeColor: 'bg-blue-500/20 text-blue-300',
          buttonGradient: 'bg-gradient-to-r from-blue-500 to-teal-500 hover:from-blue-600 hover:to-teal-600',
          buttonGradientSecondary: 'bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600'
        }
      }
    };
    
    return themeConfig[selectedCategory] || themeConfig.arena;
  };

  const theme = getThemeColors();
  const StatusIcon = theme.icon;

  return (
    <div className={styles.participationContent}>
      {/* 랭킹전 참여 상태 섹션 */}
      <div className={`${styles.participationCard} ${styles[`${selectedCategory}Card`]}`}>
        <div className={styles.participationHeader}>
          <StatusIcon className={`${styles.participationHeaderIcon} ${styles[`participation${selectedCategory.charAt(0).toUpperCase() + selectedCategory.slice(1)}Icon`]}`} />
          <span className={`${styles.statusLabel} ${styles[`${selectedCategory}Label`]}`}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "랭킹전 참여 상태" : "Ranking Battle Status"}
          </span>
        </div>
        
        {/* User Info Grid */}
        <div className={styles.participationDetails}>
          <div className={`${styles.participationDetailItem} ${styles[`${selectedCategory}DetailItem`]}`}>
            <div className={styles.participationDetailItemRow}>
              <User className={`${styles.participationDetailIcon} ${styles[`participation${selectedCategory.charAt(0).toUpperCase() + selectedCategory.slice(1)}Icon`]}`} />
              <div className={`${styles.participationDetailLabel} ${styles[`${selectedCategory}Label`]}`}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "게임 아이디" : "Game ID"}
              </div>
            </div>
            <div className={styles.participationDetailValue}>{gameUserInfo?.gameId}</div>
          </div>
          
          <div className={`${styles.participationDetailItem} ${styles[`${selectedCategory}DetailItem`]}`}>
            <div className={styles.participationDetailItemRow}>
              <Users className={`${styles.participationDetailIcon} ${styles[`participation${selectedCategory.charAt(0).toUpperCase() + selectedCategory.slice(1)}Icon`]}`} />
              <div className={`${styles.participationDetailLabel} ${styles[`${selectedCategory}Label`]}`}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "조합명" : "Union Name"}
              </div>
            </div>
            <div className={styles.participationDetailValue}>{gameUserInfo?.unionName}</div>
          </div>
        </div>
        
        <div className={`${styles.participationStatusCard} ${styles[`${selectedCategory}StatusCard`]}`}>
          <div className={styles.participationStatusContent}>
            <div className={styles.participationStatusLeft}>
              <div className={`${styles.participationStatusIconLarge} ${styles[`${selectedCategory}StatusIcon`]}`}>
                <CheckCircle />
              </div>
              <div className={styles.participationStatusInfo}>
                {/* <div className={styles.participationStatusTitle}>{gameUserInfo?.participationStatus.statusText}</div> */}
                <div className={`${styles.participationStatusDescription} ${styles[`${selectedCategory}Label`]}`}>
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                    ? `${theme.statusText} 랭킹전 참여여부` 
                    : `Participating in ${theme.statusText} Ranking Battle`}
                </div>
                {/* <div className={styles.participationServerNotice}>
                  <div className={styles.participationPulseDot}></div>
                  {gameUserInfo?.participationStatus.server}
                  <span className={styles.participationServerText}>Luna Only</span>
                </div> */}
              </div>
            </div>
            <div className={styles.participationStatusRight}>
              <span className={`${styles.participationStatusBadgeText} ${styles[`${selectedCategory}Badge`]}`}>
                {gameUserInfo?.participationStatus.isActive 
                  ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '활성' : 'Active')
                  : (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '비활성' : 'Inactive')}
              </span>
              {/* <div className={styles.participationStatusTime}>
                {gameUserInfo?.participationStatus.lastActivity}
              </div> */}
            </div>
          </div>
          
          {/* Stats Grid */}
          <div className={styles.participationStatsGrid}>
            <div className={styles.participationStatItem}>
              <div className={`${styles.participationStatLabel} ${styles[`${selectedCategory}Label`]}`}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "현재 점수" : "Current Score"}
              </div>
              <div className={styles.participationStatValue}>{gameUserInfo?.currentStats.scoreText || '0'}</div>
            </div>
            <div className={styles.participationStatItem}>
              <div className={`${styles.participationStatLabel} ${styles[`${selectedCategory}Label`]}`}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "현재 랭킹" : "Current Ranking"}
              </div>
              <div className={`${styles.participationStatValue} ${styles[`${selectedCategory}Value`]}`}>{gameUserInfo?.currentStats.rankingText || '0'}</div>
            </div>
          </div>
          
          {/* Action Buttons */}
          <div className={styles.participationActionButtons}>
            <button 
              className={styles.participationBuyButton}
              onClick={() => setShowDashboardModal(true)}
            >
              <BarChart3 style={{ width: '16px', height: '16px' }}/>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "실시간 대시보드" : "Real-time Dashboard"}
            </button>
            
            <div className={styles.participationActionButtonsRow}>
              <button 
                className={`${styles.participationRankingButton} ${styles[`${selectedCategory}Button`]}`}
                onClick={() => handleRankingPrizeClaim()}
                disabled={!gameUserInfo?.prizeClaims.rankingPrize.available || !isRewardPeriod()}
              >
                <Trophy style={{ width: '16px', height: '16px' }}/>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                  ? `랭킹 상금 수령`  //(${gameUserInfo?.prizeClaims.rankingPrize.amount})
                  : `Claim Ranking Prize`}
                  {/* (${gameUserInfo?.prizeClaims.rankingPrize.amount}) */}
              </button>
              <button 
                className={`${styles.participationLotteryButton} ${styles[`${selectedCategory}SecondaryButton`]}`}
                onClick={() => handleLotteryPrizeClaim()}
                disabled={!gameUserInfo?.prizeClaims.lotteryPrize.available || !isRewardPeriod()}                
              >
                <Gift style={{ width: '16px', height: '16px' }}/>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                  ? `추첨 보상` //(${gameUserInfo?.prizeClaims.lotteryPrize.amount})
                  : `Claim Raffle Prize`}
                  {/*  (${gameUserInfo?.prizeClaims.lotteryPrize.amount}) */}
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* NFT 보유 내역 섹션 */}
      <div className={`${styles.participationBattleCoinStatus} ${styles[`${selectedCategory}Card`]}`}>
        <div className={styles.participationCoinHeader}>
          <div className={styles.participationCoinTitle}>
            <Package className={`${styles.participationCoinIcon} ${styles[`participation${selectedCategory.charAt(0).toUpperCase() + selectedCategory.slice(1)}Icon`]}`} />
            <span className={`${styles.participationCoinCount} ${styles[`${selectedCategory}Count`]}`}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "보유 배틀코인" : "Owned Battle Coins"}
            </span>
          </div>
          {/* <div className={`${styles.participationStatusBadge} ${styles[`${selectedCategory}Badge`]}`}>{theme.statusText}</div> */}
        </div>
        
        {/* NFT 요약 정보 */}
        <div className={styles.participationNFTSummary}>
          <div className={styles.participationNFTSummaryItem}>
            <div className={`${styles.participationNFTSummaryValue} ${styles[`${selectedCategory}Value`]}`}>{nftList?.summary.total}</div>
            <div className={`${styles.participationNFTSummaryLabel} ${styles[`${selectedCategory}Label`]}`}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "보유 개수" : "Total Owned"}
            </div>
          </div>
          <div className={styles.participationNFTSummaryItem}>
            <div className={`${styles.participationNFTSummaryValue} ${styles[`${selectedCategory}Value`]}`}>{nftList?.summary.available}</div>
            <div className={`${styles.participationNFTSummaryLabel} ${styles[`${selectedCategory}Label`]}`}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "사용 가능" : "Available"}
            </div>
          </div>
          <div className={styles.participationNFTSummaryItem}>
            <div className={`${styles.participationNFTSummaryValue} ${styles[`${selectedCategory}Value`]}`}>{nftList?.summary.used}</div>
            <div className={`${styles.participationNFTSummaryLabel} ${styles[`${selectedCategory}Label`]}`}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "사용됨" : "Used"}
            </div>
          </div>
        </div>

        <div className={styles.coinList}>          
          {nftList?.nfts.map((nft, index) => (
            <div key={nft?.tokenId || `nft-${index}`} className={styles.coinItem}>
              <div className={styles.coinLeft}>
                <div className={`${styles.participationCoinIconSmall} ${styles[`${selectedCategory}IconSmall`]}`}>
                  {(nft?.meta?.image || process.env.REACT_APP_TEST_PATH + "/images/icon/battle_coin.png") && (
                    <img 
                      src={nft?.meta?.image || process.env.REACT_APP_TEST_PATH + "/images/icon/battle_coin.png"} 
                      alt={nft?.id}
                      style={{ width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'left center', borderRadius: '8px' }}
                    />
                  )}
                </div>
                <div className={styles.coinInfo}>
                  <div className={styles.coinName}>#{nft?.id}</div>
                  <div className={`${styles.coinId} ${styles[`${selectedCategory}Label`]}`}>
                    {nft?.status === 'available' 
                      ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '사용 가능' : 'Available')
                      : (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '사용됨' : 'Used')}
                  </div>
                </div>
              </div>
              <div className={styles.coinRight}>
                <button 
                  className={`${styles.useButton} ${nft.status === 'available' ? styles[`${selectedCategory}Button`] : ''}`}
                  disabled={nft.status === 'used'}
                  onClick={() => handleNFTUse(nft)}
                >
                  {nft.status === 'available' 
                    ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '사용' : 'Use')
                    : (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? '사용됨' : 'Used')}
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Import 안내 문구 */}
        {currentSeasonId === 'R1' && (
        <div className={styles.participationNoticeCard}>
          <div className={styles.participationNoticeHeader}>
            <ExternalLink className={styles.participationNoticeIcon} />
            <span className={styles.participationNoticeTitle}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "안내" : "Notice"}
            </span>
          </div>          
            <div className={styles.participationNoticeText}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                ? "MBX Station에서 배틀코인을 Import 해야합니다." 
                : "You need to Import Battle Coins to MBX Station."}
            </div>
        </div>
        )}

        {/* 액션 버튼 */}
        <div className={styles.participationBottomActionButtons}>
          {/* <button className={`${styles.participationBottomActionButton} ${styles.participationBottomActionButtonPrimary} ${styles[`${selectedCategory}Primary`]}`}>
            <Zap />
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "모두 사용" : "Use All"}
          </button> */}
          {/* <button className={`${styles.participationBottomActionButton} ${styles.participationBottomActionButtonSecondary}`}>
            <Package />
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추가 구매" : "Buy More"}
          </button> */}
        </div>
      </div>
      
      {/* NFT 사용 확인 모달 */}
      <NFTUseConfirmModal 
        isOpen={showNFTUseModal}
        onClose={() => {
          setShowNFTUseModal(false);
          setSelectedNFT(null);
        }}
        onConfirm={handleConfirmNFTUse}
        nftData={selectedNFT}
      />
    </div>
  );
}
