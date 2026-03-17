import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { 
  Swords, Trophy, Calendar, Clock, 
  Gift, Users, Skull
} from 'lucide-react';
import styles from "../../styles/BattleLeagueRight.module.css";
import MagniteShop from './modals/MagniteShop';
import Popup from "reactjs-popup";
import { AnimatePresence, motion } from "framer-motion";
import "../../styles/Popup.css";

// Popup contentStyle 상수
const popupContentStyle = {
  borderRadius: "16px",
  background: "transparent",
  border: "none",
  padding: 0,
};

export default function NFTPurchaseTab({ selectedIndex = 0, selectedSeason = [], handleLotteryResultCheck }) {

  const { 
    loading 
  } = useSelector((state) => state.battleLeague || {});

  // 실시간 카운트다운을 위한 상태
  const [remainingTime, setRemainingTime] = useState('');
  const [isSalePeriod, setIsSalePeriod] = useState(false);
  const [openMagniteShop, setOpenMagniteShop] = useState(false);
  const [magniteShopData, setMagniteShopData] = useState(null);

  // 현재 선택된 시즌 정보 가져오기 (기본값 설정)
  const currentSeason = selectedSeason?.[selectedIndex] || { id: 'R0', subtitle: 'The Arena' };
  const currentSeasonId = currentSeason?.id || '-';

  // Redux에서 가져온 동적 데이터
  const apiData = currentSeason || {};
  
  // 판매 기간 체크
  useEffect(() => {
    const checkSalePeriod = () => {
      if (!apiData?.saleStartDate || !apiData?.saleEndDate) {
        setIsSalePeriod(false);
        return;
      }
      
      const currentDate = new Date();
      const startDate = new Date(apiData.saleStartDate);
      const endDate = new Date(apiData.saleEndDate);
      
      // 현재 날짜가 판매 기간 내에 있는지 확인
      setIsSalePeriod(currentDate >= startDate && currentDate <= endDate);
    };
    
    checkSalePeriod();
    
    // 1분마다 판매 기간 체크 (실시간 업데이트)
    const interval = setInterval(checkSalePeriod, 60000);
    
    return () => clearInterval(interval);
  }, [apiData?.saleStartDate, apiData?.saleEndDate]);
  
  // 실시간 카운트다운 업데이트
  useEffect(() => {
    const updateCountdown = () => {
      if (!apiData?.saleEndDate) {
        const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
        setRemainingTime(isKorean ? '날짜 정보 없음' : 'No date information');
        return;
      }

      try {
        // 문자열 날짜를 Date 객체로 변환
        const endDate = new Date(apiData.saleEndDate);
        const currentDate = new Date();
        
        // 유효한 날짜인지 확인
        if (isNaN(endDate.getTime())) {
          const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
          setRemainingTime(isKorean ? '잘못된 날짜 형식' : 'Invalid date format');
          return;
        }

        const timeDiff = endDate.getTime() - currentDate.getTime();
        
        // 시간이 이미 지났는지 확인
        if (timeDiff <= 0) {
          const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
          setRemainingTime(isKorean ? '판매 종료' : 'Sale Ended');
          return;
        }

        // 밀리초를 일, 시간, 분, 초로 변환
        const days = Math.floor(timeDiff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((timeDiff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((timeDiff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((timeDiff % (1000 * 60)) / 1000);

        // 사용자 친화적인 형식으로 반환
        const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
        let timeString = '';
        if (days > 0) {
          timeString = isKorean ? `${days}일 ${hours}시간 ${minutes}분 ${seconds}초` : `${days}d ${hours}h ${minutes}m ${seconds}s`;
        } else if (hours > 0) {
          timeString = isKorean ? `${hours}시간 ${minutes}분 ${seconds}초` : `${hours}h ${minutes}m ${seconds}s`;
        } else if (minutes > 0) {
          timeString = isKorean ? `${minutes}분 ${seconds}초` : `${minutes}m ${seconds}s`;
        } else {
          timeString = isKorean ? `${seconds}초` : `${seconds}s`;
        }
        
        setRemainingTime(timeString);
      } catch (error) {
        console.error('날짜 계산 오류:', error);
        const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
        setRemainingTime(isKorean ? '날짜 계산 오류' : 'Date calculation error');
      }
    };

    // 즉시 한 번 실행
    updateCountdown();
    
    // 1초마다 업데이트
    const interval = setInterval(updateCountdown, 1000);
    
    // 컴포넌트 언마운트 시 인터벌 정리
    return () => clearInterval(interval);
  }, [apiData?.saleEndDate]);

  // 로딩 상태 처리
  if (loading?.nftPurchase) {
    return (
      <div className={styles.loadingSpinner}>
        {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "NFT 구매 정보 로딩 중..." : "Loading NFT purchase information..."}
      </div>
    );
  }

  //console.log('Current season:', currentSeason);

  // subtitle에 따라 카테고리 결정
  const getCategoryFromSubtitle = (subtitle) => {
    switch (subtitle) {
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

  const selectedCategory = getCategoryFromSubtitle(currentSeason.subtitle);
  
  // 날짜 포맷팅 함수 (언어에 따라 다른 형식 적용)
  const formatDate = (dateString) => {
    if (!dateString) return '';
    
    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
    
    try {
      // 날짜 문자열 파싱 (YYYY-MM-DD 또는 YYYY-MM-DD HH:mm:ss)
      const dateOnly = dateString.split(' ')[0];
      const [year, month, day] = dateOnly.split('-');
      
      if (isKorean) {
        // 한국어: YYYY.MM.DD 형식
        return `${year}.${month}.${day}`;
      } else {
        // 영어: MMM DD, YYYY 형식 (예: Jan 15, 2024)
        const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 
                           'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const monthIndex = parseInt(month, 10) - 1;
        const dayNum = parseInt(day, 10);
        return `${monthNames[monthIndex]} ${dayNum}, ${year}`;
      }
    } catch (error) {
      console.error('날짜 포맷팅 에러:', error);
      return dateString.split(' ')[0];
    }
  };
  
  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

  // 카테고리별 데이터 정의 (스타일은 정적, 동적 데이터는 Redux에서)
  const categoryData = {
    arena: {
      scheduleIcon: Swords,
      // Redux에서 가져온 동적 데이터 (slice에서 이미 기본값 처리됨)
      name: apiData?.name,
      subtitle: apiData?.subtitle,
      description: apiData?.description,
      price: apiData?.price,
      remainingTime: apiData?.remainingTime,
      noticeItems: isKorean ? apiData?.noticeItemsKr : apiData?.noticeItems,
      // 정적 스타일 (API에서 가져올 필요 없음)
      cardStyle: styles.arenaCard,
      headerStyle: styles.arenaHeader,
      iconStyle: styles.arenaIcon,
      infoStyle: styles.arenaInfo,
      nameStyle: styles.arenaName,
      subtitleStyle: styles.arenaSubtitle,
      descriptionStyle: styles.arenaDescription,
      priceStyle: styles.arenaPrice,
      purchaseButtonStyle: styles.arenaPurchaseButton
    },
    bossRaid: {
      scheduleIcon: Skull,
      // Redux에서 가져온 동적 데이터 (slice에서 이미 기본값 처리됨)
      name: apiData?.name,
      subtitle: apiData?.subtitle,
      description: apiData?.description,
      price: apiData?.price,
      remainingTime: apiData?.remainingTime,
      noticeItems: isKorean ? apiData?.noticeItemsKr : apiData?.noticeItems,
      // 정적 스타일 (API에서 가져올 필요 없음)
      cardStyle: styles.bossRaidCard,
      headerStyle: styles.bossRaidHeader,
      iconStyle: styles.bossRaidIcon,
      infoStyle: styles.bossRaidInfo,
      nameStyle: styles.bossRaidName,
      subtitleStyle: styles.bossRaidSubtitle,
      descriptionStyle: styles.bossRaidDescription,
      priceStyle: styles.bossRaidPrice,
      purchaseButtonStyle: styles.bossRaidPurchaseButton
    },
    union: {
      scheduleIcon: Users,
      // Redux에서 가져온 동적 데이터 (slice에서 이미 기본값 처리됨)
      name: apiData?.name,
      subtitle: apiData?.subtitle,
      description: apiData?.description,
      price: apiData?.price,
      remainingTime: apiData?.remainingTime,
      noticeItems: isKorean ? apiData?.noticeItemsKr : apiData?.noticeItems,
      // 정적 스타일 (API에서 가져올 필요 없음)
      cardStyle: styles.unionCard,
      headerStyle: styles.unionHeader,
      iconStyle: styles.unionIcon,
      infoStyle: styles.unionInfo,
      nameStyle: styles.unionName,
      subtitleStyle: styles.unionSubtitle,
      descriptionStyle: styles.unionDescription,
      priceStyle: styles.unionPrice,
      purchaseButtonStyle: styles.unionPurchaseButton
    }
  };

  const data = categoryData[selectedCategory];
  if (!data) return null;
  
  // 날짜 포맷팅된 스케줄 데이터 생성
  const formattedSchedule = {
    salePeriod: `${formatDate(apiData?.saleStartDate)} ~ ${formatDate(apiData?.saleEndDate)}`,
    rankingPeriod: `${formatDate(apiData?.rankingStartDate)} ~ ${formatDate(apiData?.rankingEndDate)}`,
    lotteryPeriod: `${formatDate(apiData?.lotteryDate)}`,
    rewardPeriod: `${formatDate(apiData?.rewardStartDate)} ~ ${formatDate(apiData?.rewardEndDate)}`
  };
  const ScheduleIconComponent = data.scheduleIcon;

  return (
    <div className={data.cardStyle}>
      {/* Header with icon and title */}
      <div className={data.headerStyle}>
        <div className={data.iconStyle}>
          <img 
            src={process.env.REACT_APP_TEST_PATH + "/images/icon/battle_coin.png"} 
            alt="battle_coin"
            style={{ width: '100%', height: '100%', objectFit: 'contain' , borderRadius: '20%'}}
          />
        </div>
        <div className={data.infoStyle}>
          <div className={data.nameStyle}>[{currentSeason.id}] {data.name}</div>
          <div className={data.subtitleStyle}>{data.subtitle}</div>
          <div className={data.descriptionStyle}>{data.description}</div>
          <div className={data.priceStyle}>
            <span className={styles.priceValue}>{data.price}</span>
            <span className={styles.priceUnit}>
              {currentSeasonId === 'R1' ? 'MBX' : 'MN'}
            </span>
          </div>
        </div>
      </div>

      {/* Action buttons */}
      <div className={styles.purchaseButtonContainer}>
        <button 
          className={`${styles.purchaseButton} ${data.purchaseButtonStyle}`}
          disabled={!isSalePeriod}  
          //disabled={currentSeasonId === 'R1' ? true : false}
          onClick={() => {
            //if (!isSalePeriod) return;
            if(currentSeasonId === 'R1'){              
              // if (Number(sessionStorage.getItem("test_mode"))){
              //   window.open("https://nft.marblex.io/claimPage", "_blank");
              // }else{
              //   window.DApp.request("openexternal", {url:"https://nft.marblex.io/claimPage"});
              // }  
            }
            else if (currentSeasonId === '-'){
              window.DApp.emit("dapp.popup", {
                err: 1,
                title: isKorean ? "오류" : "Error",
                msg: isKorean ? "시즌 정보가 없습니다!" : "Season information is not available!"
              });
              return;
            }else{
              setOpenMagniteShop(true);
              setMagniteShopData({ type: 1, data: {season_id: currentSeasonId, goods_title: '[' + currentSeason.id + '] Battle Coin', goods_image: process.env.REACT_APP_TEST_PATH + "/images/icon/battle_coin.png", price: 100} });
            }            
          }}
        >
          {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "구매하기" : "Purchase"}
        </button>
        {/* <button 
          className={styles.lotteryButton}
          onClick={handleLotteryResultCheck}
        >
          <Trophy />
          추첨 결과
        </button> */}
      </div>

      {/* Compact schedule info */}
      <div className={styles.scheduleCard}>
        <div className={styles.scheduleGrid}>
            <div className={styles.scheduleItem}>
              <Calendar className={styles.scheduleIcon} style={{ width: '20px', height: '20px' }} />
              <span className={styles.scheduleLabel}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "판매" : "Sale"}
                </span>
                <span className={styles.scheduleValue}>{formattedSchedule?.salePeriod}</span>
              </div>
            <div className={styles.scheduleItem}>
              <ScheduleIconComponent className={styles.scheduleIcon} style={{ width: '20px', height: '20px' }} />
              <span className={styles.scheduleLabel}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "랭킹전" : "Ranking Battle"}
              </span>
              <span className={styles.scheduleValue}>{formattedSchedule?.rankingPeriod}</span>
            </div>
            <div className={styles.scheduleItem}>
              <Gift className={styles.scheduleIcon} style={{ width: '20px', height: '20px' }} />
              <span className={styles.scheduleLabel}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨자 발표" : "Raffle Draw"}
              </span>
              <span className={styles.scheduleValue}>{formattedSchedule?.lotteryPeriod}</span>
            </div>
            <div className={styles.scheduleItem}>
              <Trophy className={styles.scheduleIcon} style={{ width: '20px', height: '20px' }} />
              <span className={styles.scheduleLabel}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "상금 수령기간" : "Reward Claim"}
              </span>
              <span className={styles.scheduleValue}>{formattedSchedule?.rewardPeriod}</span>
            </div>
        </div>
      </div>

      {/* Remaining time */}
      <div className={styles.stockCard}>
        <div className={styles.stockItem}>
          <Clock className={styles.stockIcon} style={{ width: '20px', height: '20px' }} />
          <span className={styles.stockLabel}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "남은 판매 시간" : "Remaining Sale Time"}
          </span>
          <span className={styles.stockValue}>{remainingTime}</span>
        </div>
      </div>

      {/* Server notice */}
      <div className={styles.noticeCard}>
        <div className={styles.noticeHeader}>
          <Users className={styles.noticeIcon} />
          <span className={styles.noticeLabel}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "안내사항" : "Notice"}
          </span>
        </div>
        <div className={styles.noticeContent}>
          {(() => {
            try {
              
              // 언어에 따라 다른 필드 사용
              const rawItems = data?.noticeItems;
              
              // noticeItems가 JSON 문자열인 경우 파싱
              const items = typeof rawItems === 'string' 
                ? JSON.parse(rawItems) 
                : rawItems || [];
              
              // 배열이 아니거나 비어있는 경우
              if (!Array.isArray(items) || items.length === 0) {
                return null;
              }
              
              return items.map((item, index) => (
                <div key={index} className={styles.noticeItem}>{typeof item === 'string' ? item : item.text || item}</div>
              ));
            } catch (error) {
              console.error('noticeItems 파싱 에러:', error);
              // 파싱 실패 시 빈 배열로 처리
              return null;
            }
          })()}
        </div>
      </div>

      {/* MagniteShop Modal */}
      <AnimatePresence>
        {openMagniteShop && magniteShopData && (
          <Popup
            open
            onClose={() => {
              setOpenMagniteShop(false);
              setMagniteShopData(null);
            }}
            modal
            contentStyle={popupContentStyle}
          >
            <motion.div
              key="magniteshop"
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.9 }}
              transition={{ duration: 0.2 }}
              className="popup-motion-wrapper"
            >
              <MagniteShop 
                popupData={magniteShopData}
                closePopup={() => {
                  setOpenMagniteShop(false);
                  setMagniteShopData(null);
                }}
              />
            </motion.div>
          </Popup>
        )}
      </AnimatePresence>
    </div>
  );
}
