import { useState, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import styles from "../../styles/BattleLeagueRight.module.css";
import { usePopup } from "../../context/PopupContext";
import NFTPurchaseTab from './NFTPurchaseTab';
import ParticipationTab from './ParticipationTab';
import GuideTab from './GuideTab';
import RankingTab from './RankingTab';
import LotteryTab from './LotteryTab';
import ItemsTab from './ItemsTab';
import DashboardModal from './modals/DashboardModal';
import PrizeClaimModal from './modals/PrizeClaimModal';
import LotteryResultModal from './modals/LotteryResultModal';
import PrizeConfirmModal from './modals/PrizeConfirmModal';
import { fetchClaimCheckLotteryData, claimConfirmLottery, fetchClaimCheckRankingData, claimConfirmRanking } from "../../data/battleLeagueSlice";

import { 
  Package, User, Target, Crown, Dice6, Gift,
  Trophy
} from 'lucide-react';
import LotteryResultTab from './LotteryResultTab';
import { fetchDashboardData, clearLotteryResultData, fetchLotteryResultData, fetchParticipationData } from '../../data/battleLeagueSlice';

export default function BattleLeagueRight({ 
  data, 
  selectedIndex, 
  seasons
}) {

  const dispatch = useDispatch();  
  
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
  const scrollRef = useRef(null);
  
  // Redux에서 데이터 가져오기
  const { dashboardData, lotteryResultData, participationData, claimCheckLotteryData, claimCheckRankingData } = useSelector((state) => state.battleLeague || {});
  
  // State variables
  const [prizeDistributionTab, setPrizeDistributionTab] = useState('purchase');
  const [selectedCategory, setSelectedCategory] = useState('arena');
  const [showDashboardModal, setShowDashboardModal] = useState(false);
  const [showRankingPrizeModal, setShowRankingPrizeModal] = useState(false);  
  const [showLotteryPrizeModal, setShowLotteryPrizeModal] = useState(false);
  const [showLotteryResultModal, setShowLotteryResultModal] = useState(false);
  const [lotteryResult, setLotteryResult] = useState(null);
  const [prizeClaimResult, setPrizeClaimResult] = useState(false);
  const [userNFTs, setUserNFTs] = useState([]);
  const [isWinner, setIsWinner] = useState(false);
  
  // 상금 수령 확인 모달 상태
  const [showPrizeConfirmModal, setShowPrizeConfirmModal] = useState(false);
  const [confirmPrizeData, setConfirmPrizeData] = useState(null);
  
  // Dashboard Modal 관련 state
  const [currentSeason, setCurrentSeason] = useState(null);

  // selectedIndex에 따라 카테고리 매핑
  const getCategoryFromIndex = (index) => {
    const season = seasons[index];
    if (!season) return 'arena';
    
    switch (season.subtitle) {
      case 'The Arena':
        return 'arena';
      case 'Boss Raid':
        return 'bossRaid';
      case 'The Union':
        return 'union';
      default:
        return 'arena';
    }
  };
  
  // selectedIndex가 변경될 때만 실행되도록 useEffect 수정
  useEffect(() => {
    // selectedIndex에 해당하는 시즌이 없으면 실행하지 않음
    const currentSeason = seasons[selectedIndex];
    if (!currentSeason) return;
    
    if (scrollRef.current) {
      scrollRef.current.scrollTop = 0;
    }
    
    // selectedIndex가 변경될 때마다 카테고리도 업데이트
    const newCategory = getCategoryFromIndex(selectedIndex);
    setSelectedCategory(newCategory);
    setCurrentSeason(currentSeason);

    setPrizeDistributionTab('purchase');
    
    // 시즌 변경 시 추첨 결과 데이터 초기화
    dispatch(clearLotteryResultData());

    // 내 현황 데이터 기본적으로 가져오기 (현재 선택된 시즌만)
    const currentSeasonId = currentSeason.id;
    if (currentSeasonId) {
      dispatch(fetchParticipationData(currentSeasonId));
    }

  }, [dispatch, selectedIndex]); // selectedIndex만 dependency로 사용

  // 현재 선택된 시즌 ID (다른 useEffect에서 사용)
  const currentSeasonId = seasons[selectedIndex]?.id;

  // ParticipationTab 진입 시 대시보드 데이터 가져오기
  useEffect(() => {
    if (prizeDistributionTab === 'participation' && currentSeasonId) {
      dispatch(fetchDashboardData(currentSeasonId));
    }
  }, [dispatch, prizeDistributionTab, selectedIndex, currentSeasonId]);

  // LotteryResultTab 진입 시 추첨 결과 데이터 가져오기
  useEffect(() => {
    if (prizeDistributionTab === 'lotteryResult' && currentSeasonId) {
      dispatch(fetchLotteryResultData(currentSeasonId));
    }
  }, [dispatch, prizeDistributionTab, selectedIndex, currentSeasonId]);

  // 3개 추첨번호 결과
  const handleLotteryResultCheck = (winner = false, lotteryPeriod = null, userNFTsList = []) => {
    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
    
    // 추첨 날짜 확인
    
    if (lotteryPeriod) {
      const currentDate = new Date();
      const lotteryDate = new Date(lotteryPeriod);
      
      // 추첨 날짜가 아직 도래하지 않았다면 에러 모달 표시
      if (currentDate < lotteryDate) {
        const formatDate = (dateString) => {
          const date = new Date(dateString);
          const month = String(date.getMonth() + 1).padStart(2, '0');
          const day = String(date.getDate()).padStart(2, '0');
          return `${month}.${day}`;
        };
        
        setPrizeClaimResult({
          isSuccess: false,
          type: 'lottery',
          title: isKorean ? '추첨 기간 안내' : 'Raffle Period Notice',
          message: isKorean 
            ? `추첨은 ${formatDate(lotteryPeriod)} 이후에 진행됩니다.\n추첨 기간이 되면 결과를 확인하실 수 있습니다.`
            : `The raffle draw will be held after ${formatDate(lotteryPeriod)}.\nYou can check the results during the raffle period.`
        });
        return;
      }
    }    

    // 사용자 NFT 확인
    const userNFTs = userNFTsList.length > 0 ? userNFTsList : (participationData?.nftList?.nfts || []);
    
    // NFT가 없으면 참여 대상자가 아님
    /*
    if (!userNFTs || userNFTs.length === 0) {
      setPrizeClaimResult({
        isSuccess: false,
        type: 'lottery',
        title: isKorean ? '참여 대상 안내' : 'Eligibility Notice',
        message: isKorean 
          ? '보유하신 Battle Coin 이 없어 이벤트 참여 대상자가 아닙니다.\n구매하신 후 다시 시도해주세요.'
          : 'You do not have any Battle Coin.\nPlease purchase a Battle Coin to participate in the raffle.'
      });
      return;
    }
       */

    // Redux에서 가져온 추첨 결과 데이터 사용
    const lotteryNumbers = lotteryResultData?.lotteryResult?.winningNumbers || [];

    setUserNFTs(userNFTs);

    setIsWinner(winner);
    setLotteryResult({
      isWinner: winner,      
      lotteryNumbers: lotteryNumbers,
      message: isKorean ? "추첨 결과" : "Lottery Result"
    });
    
    setShowLotteryResultModal(true);
  };

  // 랭킹 상금 수령 확인
  const handleRankingPrizeClaim = async () => {
    const currentSeasonId = seasons[selectedIndex]?.id;
    if (!currentSeasonId) return;

    try {
      // API 호출 결과를 기다림
      const result = await dispatch(fetchClaimCheckRankingData(currentSeasonId));      
      const rankingData = result.payload || claimCheckRankingData;

      setConfirmPrizeData({
        type: 'ranking',
        amount: formatAmount(rankingData?.total_rewards || 0) + "",
        title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "랭킹 상금" : "Ranking Prize",
        seasonId: currentSeasonId
      });
      setShowPrizeConfirmModal(true);
    } catch (error) {
      console.error('랭킹 상금 확인 API 에러:', error);
      // 에러 발생 시에도 기존 데이터로 모달 열기
      setConfirmPrizeData({
        type: 'ranking',
        amount: formatAmount(claimCheckRankingData?.total_rewards || participationData?.userInfo?.prizeClaims?.rankingPrize?.amount || 0) + "",
        title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "랭킹 상금" : "Ranking Prize",
        seasonId: currentSeasonId
      });
      setShowPrizeConfirmModal(true);
    }
  };

  // 실제 랭킹 상금 수령 처리
  const handleConfirmRankingPrizeClaim = async () => {
    const currentSeasonId = seasons[selectedIndex]?.id;
    if (!currentSeasonId) return;

    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

    try {
      // API 호출로 실제 랭킹 상금 수령 처리
      const result = await dispatch(claimConfirmRanking(currentSeasonId));
      
      // API 응답 결과 확인
      const claimResult = result.payload || result;
      
      // API 응답에서 성공 여부 확인 (rs === 0이면 성공)
      const isSuccess = claimResult?.rs === 0 || claimResult?.success === true;
      
      if (isSuccess) {
        // 수령 성공
        setPrizeClaimResult({
          isSuccess: true,
          reward: claimResult?.total_rewards || claimCheckRankingData?.total_rewards || 0,
          type: 'ranking',
          message: isKorean ? '랭킹 상금이 성공적으로 지급되었습니다!' : 'Ranking prize has been successfully distributed!'
        });
        
        // 수령 후 participationData 최신화
        dispatch(fetchParticipationData(currentSeasonId));
      } else {
        // 수령 실패 (당첨되지 않음)
        setPrizeClaimResult({
          isSuccess: false,
          type: 'ranking',
          message: claimResult?.msg || (isKorean ? '랭킹 상금 수령 중 오류가 발생했습니다. Err(101)' : 'An error occurred while claiming the ranking prize. Err(101)')
        });
      }
    } catch (error) {
      console.error('랭킹 상금 수령 API 에러:', error);
      // 에러 발생 시
      setPrizeClaimResult({
        isSuccess: false,
        type: 'ranking',
        message: isKorean ? '랭킹 상금 수령 중 오류가 발생했습니다.' : 'An error occurred while claiming the ranking prize.'
      });
    }
    
    setShowPrizeConfirmModal(false);
    setConfirmPrizeData(null);
  };

  // 추첨 상금 수령 확인
  const handleLotteryPrizeClaim = async () => {
    const currentSeasonId = seasons[selectedIndex]?.id;
    if (!currentSeasonId) return;

    try {
      // API 호출 결과를 기다림
      const result = await dispatch(fetchClaimCheckLotteryData(currentSeasonId));      
      const lotteryData = result.payload || claimCheckLotteryData;

      setConfirmPrizeData({
        type: 'lottery',
        amount: formatAmount(lotteryData?.total_rewards || 0) + " EV",
        title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨 상금" : "Lottery Prize",
        seasonId: currentSeasonId
      });
      setShowPrizeConfirmModal(true);
    } catch (error) {
      console.error('추첨 상금 확인 API 에러:', error);
      // 에러 발생 시에도 기존 데이터로 모달 열기
      setConfirmPrizeData({
        type: 'lottery',
        amount: formatAmount(claimCheckLotteryData?.total_rewards || 0) + " EV",
        title: navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨 상금" : "Lottery Prize",
        seasonId: currentSeasonId
      });
      setShowPrizeConfirmModal(true);
    }
  };

  // 실제 추첨 상금 수령 처리
  const handleConfirmLotteryPrizeClaim = async () => {
    const currentSeasonId = seasons[selectedIndex]?.id;
    if (!currentSeasonId) return;

    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

    try {
      // API 호출로 실제 추첨 상금 수령 처리
      const result = await dispatch(claimConfirmLottery(currentSeasonId));
      
      // API 응답 결과 확인
      const claimResult = result.payload || result;
      
      // API 응답에서 성공 여부 확인 (rs === 0이면 성공)
      const isSuccess = claimResult?.rs === 0 || claimResult?.success === true;
      
      if (isSuccess) {
        // 수령 성공
        setPrizeClaimResult({
          isSuccess: true,
          reward: claimResult?.total_rewards || claimCheckLotteryData?.total_rewards || 0,
          type: 'lottery',
          message: isKorean ? '추첨 상금이 성공적으로 지급되었습니다!' : 'Lottery prize has been successfully distributed!'
        });
        
        // 수령 후 participationData 최신화
        dispatch(fetchParticipationData(currentSeasonId));
      } else {
        // 수령 실패 (당첨되지 않음)
        setPrizeClaimResult({
          isSuccess: false,
          type: 'lottery',
          message: claimResult?.msg || (isKorean ? '추첨 상금 수령 중 오류가 발생했습니다. Err(101)' : 'An error occurred while claiming the lottery prize. Err(101)')
        });
      }
    } catch (error) {
      console.error('추첨 상금 수령 API 에러:', error);
      // 에러 발생 시
      setPrizeClaimResult({
        isSuccess: false,
        type: 'lottery',
        message: isKorean ? '추첨 상금 수령 중 오류가 발생했습니다.' : 'An error occurred while claiming the lottery prize.'
      });
    }
    
    setShowPrizeConfirmModal(false);
    setConfirmPrizeData(null);
  };

  // MBX 아이콘 (임시로 기본 이미지 사용)
  const mbxIcon = '/images/icon/mbx.png';


  return (
    <>
    <div className={styles.container}>
      <div className={styles.rightContent} ref={scrollRef}>
        {/* Right Content - Season Details */}
        <div className={styles.contentWrapper}>
          <div className={styles.contentInner}>

            {/* Categories */}
            <div className={styles.categoriesSection}>
              {/* Tab Navigation */}
              <div className={styles.tabNavigation}>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'purchase' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('purchase')}
                >
                  <Package className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "NFT 구매" : "Purchase"}
                </button>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'lotteryResult' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('lotteryResult')}
                >
                  <Trophy className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨 결과" : "Raffle Result"}
                </button>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'participation' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('participation')}
                >
                  <User className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "내 현황" : "My Status"}
                </button>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'guide' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('guide')}
                >
                  <Target className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "라운드 가이드" : "Round Guide"}
                </button>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'ranking' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('ranking')}
                >
                  <Crown className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "랭킹 보상" : "Ranking Reward"}
                </button>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'lottery' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('lottery')}
                >
                  <Dice6 className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨 보상" : "Raffle Reward"}
                </button>
                <button
                  className={`${styles.tabButton} ${
                    prizeDistributionTab === 'items' ? styles.tabButtonActive : ''
                  }`}
                  onClick={() => setPrizeDistributionTab('items')}
                >
                  <Gift className={styles.tabIcon} />
                  {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "게임 아이템 보상" : "Game Item Reward"}
                </button>
              </div>

              {/* Tab Content */}
              <div className={styles.tabContent}>
                {prizeDistributionTab === 'purchase' && (
                  <NFTPurchaseTab 
                    selectedIndex={selectedIndex}
                    selectedSeason={seasons}
                    handleLotteryResultCheck={handleLotteryResultCheck} 
                  />
                )}
                {prizeDistributionTab === 'lotteryResult' && (
                  <LotteryResultTab 
                    handleLotteryResultCheck={handleLotteryResultCheck} 
                    selectedIndex={selectedIndex}
                    selectedSeason={seasons}
                    participationData={participationData}
                  />
                )}
                {prizeDistributionTab === 'participation' && (
                  <ParticipationTab 
                    setShowDashboardModal={setShowDashboardModal}
                    setShowRankingPrizeModal={setShowRankingPrizeModal}
                    setShowLotteryPrizeModal={setShowLotteryPrizeModal}
                    handleRankingPrizeClaim={handleRankingPrizeClaim}
                    handleLotteryPrizeClaim={handleLotteryPrizeClaim}
                  />
                )}
                {prizeDistributionTab === 'guide' && (
                  <GuideTab />
                )}
                {prizeDistributionTab === 'ranking' && (
                  <RankingTab selectedCategory={selectedCategory} />
                )}
                {prizeDistributionTab === 'lottery' && (
                  <LotteryTab/>
                )}
                {prizeDistributionTab === 'items' && (
                  <ItemsTab />
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    {/* Dashboard Modal */}
    <DashboardModal 
      isOpen={showDashboardModal}
      onClose={() => setShowDashboardModal(false)}
      currentSeason={currentSeason}
      rankingData={dashboardData || []}
      mbxIcon={mbxIcon}
    />

    {/* Prize Confirm Modal */}
    <PrizeConfirmModal 
      isOpen={showPrizeConfirmModal}
      onClose={() => {
        setShowPrizeConfirmModal(false);
        setConfirmPrizeData(null);
      }}
      onConfirm={() => {
        if (confirmPrizeData?.type === 'ranking') {
          handleConfirmRankingPrizeClaim();
        } else if (confirmPrizeData?.type === 'lottery') {
          handleConfirmLotteryPrizeClaim();
        }
      }}
      prizeData={confirmPrizeData}
      mbxIcon={mbxIcon}
    />

    {/* Prize Claim Result Modal */}
    <PrizeClaimModal 
      prizeClaimResult={prizeClaimResult}
      onClose={() => setPrizeClaimResult(null)}
    />
    
    {/* Lottery Result Modal */}
    <LotteryResultModal 
      isOpen={showLotteryResultModal}
      onClose={() => setShowLotteryResultModal(false)}
      lotteryResultData={lotteryResultData}      
      isWinner={isWinner}
      userNFTs={userNFTs}
      lotteryPeriod={seasons[selectedIndex]?.lotteryDate}
    />

    </>
  );
}
