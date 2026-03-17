import { useState } from 'react';
import { useSelector } from 'react-redux';
import { Trophy, Clock } from 'lucide-react';
import styles from "../../styles/BattleLeagueRight.module.css";

//Raffle Result Tab
export default function LotteryResultTab({ handleLotteryResultCheck, selectedIndex, selectedSeason, participationData }) {
  const [showFireworks, setShowFireworks] = useState(false);
  const [particles, setParticles] = useState([]);
  
  // Redux에서 데이터 가져오기 (BattleLeagueRight에서 API 호출 관리)
  const { 
    lotteryResultData, 
    loading 
  } = useSelector((state) => state.battleLeague || {});
  
  // 현재 선택된 시즌의 추첨 날짜 가져오기
  const currentSeason = selectedSeason?.[selectedIndex];
  const lotteryPeriod = currentSeason?.lotteryDate;

  const currentSeasonId = currentSeason?.id || '-';
  
  // 날짜 포맷팅 함수
  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${month}.${day}`;
  };

  // 로딩 상태 처리
  if (loading?.lotteryResult) {
    return (
      <div className={styles.lotteryContent}>
        <div className={styles.loadingSpinner}>
          {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨 정보 로딩 중..." : "Loading ..."}
        </div>
      </div>
    );
  }

  // 폭죽 애니메이션 함수
  const createFireworks = () => {
    setShowFireworks(true);
    
    // 폭죽 발사 위치들 (화면 하단에서 위로 발사)
    const launchPositions = [
      { x: 20, y: 90 }, // 왼쪽
      { x: 50, y: 85 }, // 중앙
      { x: 80, y: 90 }, // 오른쪽
      { x: 35, y: 88 }, // 왼쪽 중앙
      { x: 65, y: 88 }, // 오른쪽 중앙
    ];
    
    const newParticles = [];
    let particleId = 0;
    
    // 각 발사 위치마다 폭죽 생성
    launchPositions.forEach((launchPos, launchIndex) => {
      // 폭죽이 위로 올라갈 높이 (랜덤)
      const explosionHeight = 30 + Math.random() * 40; // 30-70% 높이에서 터짐
      const explosionX = launchPos.x + (Math.random() * 20 - 10); // 발사 위치에서 약간 흔들림
      const explosionY = explosionHeight;
      
      // 폭죽 발사 지연 시간
      const launchDelay = launchIndex * 100; // 각 폭죽마다 100ms씩 지연 (더 빠르게)
      
      // 폭죽이 위로 올라가는 시간 (300ms)
      const flightTime = 300;
      
      // 폭죽 터지는 시점
      const explosionTime = launchDelay + flightTime;
      
      // 터진 후 파티클들 생성
      const particleCount = 16 + Math.floor(Math.random() * 8); // 16-23개 파티클
      
      for (let i = 0; i < particleCount; i++) {
        const angle = (360 / particleCount) * i + Math.random() * 30 - 15; // 더 넓게 퍼짐
        const velocity = 120 + Math.random() * 180; // 폭발 속도
        
        // 삼각함수로 X, Y 방향 계산
        const angleRad = (angle * Math.PI) / 180;
        const explodeX = Math.cos(angleRad) * velocity;
        const explodeY = Math.sin(angleRad) * velocity;
        
        // 폭죽이 발사점에서 폭발점까지 이동하는 거리 (픽셀 단위)
        const moveX = (explosionX - launchPos.x) * 10; // 10px per %
        const moveY = (explosionY - launchPos.y) * 10; // 10px per %
        
        const particle = {
          id: particleId++,
          angle: angle,
          velocity: velocity,
          explodeX: explodeX,
          explodeY: explodeY,
          moveX: moveX,
          moveY: moveY,
          delay: explosionTime + Math.random() * 100, // 폭발 시간에 약간의 랜덤 지연
          centerX: explosionX,
          centerY: explosionY,
          size: 8 + Math.random() * 10, // 8-18px 크기 (더 크게)
          color: Math.floor(Math.random() * 360),
          // 폭죽 발사 정보 추가
          launchX: launchPos.x,
          launchY: launchPos.y,
          launchDelay: launchDelay,
          flightTime: flightTime
        };
        newParticles.push(particle);
      }
    });
    
    setParticles(newParticles);
    
    // 디버깅용 로그
    //console.log('폭죽 생성:', newParticles.length, '개 파티클');
    //console.log('첫 번째 파티클:', newParticles[0]);
    
    // 2초 후 애니메이션 종료 (폭죽 발사 + 터지는 시간 고려)
    setTimeout(() => {
      setShowFireworks(false);
      setParticles([]);
    }, 2000);
  };

  // 버튼 클릭 핸들러
  const handleButtonClick = () => {
    // 사용자가 소유한 NFT 리스트
    const userNFTs = participationData?.nftList?.nfts || [];
    
    // 당첨자 리스트에서 당첨 번호들 추출
    const winners = lotteryResultData?.lotteryResult?.winners || [];
    const winningNumbers = winners.map(winner => winner.winningNumber);
    
    // 사용자가 소유한 NFT 중에 당첨번호가 있는지 확인
    const isWinner = userNFTs.some(nft => 
      winningNumbers.some(winningNum => String(nft.id) === String(winningNum))
    );

    // handleLotteryResultCheck에 lotteryPeriod와 userNFTs 전달하여 날짜 및 참여 자격 확인
    handleLotteryResultCheck(isWinner, lotteryPeriod, userNFTs);

    /*
    if (isWinner) {
      createFireworks();
    }
    if (handleLotteryResultCheck) {
      setTimeout(() => {
        handleLotteryResultCheck(isWinner);
      }, isWinner ? 500 : 0); // 당첨일 때만 0.5초 지연
    }
    */
  };

  // Redux에서 가져온 데이터 사용 (실패 시 기본값 포함)
  const lotteryInfo = lotteryResultData?.lotteryResult || {};
  const rules = lotteryResultData?.rules || [];

  return (
    <div className={styles.lotteryResultContainer}>
      {/* 폭죽 애니메이션 */}
      {showFireworks && (
        <div className={styles.fireworksContainer}>
          {particles.map((particle) => (
            <div
              key={particle.id}
              className={styles.fireworkParticle}
              style={{
                '--angle': `${particle.angle}deg`,
                '--velocity': `${particle.velocity}px`,
                '--delay': `${particle.delay}ms`,
                '--hue': `${particle.color}deg`,
                '--centerX': `${particle.centerX}%`,
                '--centerY': `${particle.centerY}%`,
                '--size': `${particle.size}px`
              }}
            />
          ))}
        </div>
      )}
      
      {/* 추첨 결과 헤더 */}
      <div className={styles.lotteryResultHeader}>
        <div className={styles.lotteryResultHeaderContent}>
          {/* 총 상금 표시 */}
          <div className={styles.lotteryPrizeSection}>
            <div className={styles.lotteryPrizeTitle}>
              <Trophy className={styles.trophyIcon} />
              <span className={styles.prizeLabel}>
                {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "총 상금" : "Total Prize"}
              </span>
            </div>
            <div className={styles.lotteryPrizeAmount}>
              <span className={styles.prizeValue}>                
                {(() => {
                  if (currentSeasonId === 'R1') {
                    return '30,000';
                  } else if (currentSeasonId === 'R2') {
                    return '20,000';
                  } else if (currentSeasonId === 'R3') {
                    return '5,000';
                  } else {
                    return '0';
                  }
                })()}
              </span>
              <span className={styles.prizeUnit}>
                {(() => {
                  if (currentSeasonId === 'R1') {
                    return 'MBX!';
                  } else if (currentSeasonId === 'R2') {
                    return 'MN!';
                  } else if (currentSeasonId === 'R3') {
                    return 'MN!';
                  } else {
                    return 'MN!';
                  }
                })()}
              </span>
            </div>
          </div>

          {/* 메인 메시지 */}
          <div className={styles.lotteryResultMessage}>
            {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "행운의 당첨자는 누구일까요?" : "Who will be the lucky winner?"}
          </div>

          {/* 추첨 번호 표시 영역 */}
          {/* <div className={styles.lotteryNumbersSection}>
            {(() => {
              const currentDate = new Date();
              const lotteryRevealDate = new Date('2025-11-15'); // 11월 15일
              const isLotteryRevealed = currentDate >= lotteryRevealDate;
              
              if (isLotteryRevealed) {
                // 추첨 결과 공개 후
                return (
                  <div className={styles.lotteryResultNumbersContainer}>
                    <div className={styles.winningNumbersLabel}>
                      {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "당첨 번호" : "Winning Numbers"}
                    </div>
                    <div className={styles.winningNumbersDisplay}>
                      <div className={styles.lotteryNumberRevealed}>#{lotteryInfo.winners[0].number}</div>
                      <div className={styles.lotteryNumberRevealed}>#{lotteryInfo.winners[1].number}</div>
                      <div className={styles.lotteryNumberRevealed}>#{lotteryInfo.winners[2].number}</div>                      
                    </div>
                  </div>
                );
              } else {
                // 추첨 결과 공개 전
                return (
                  <div className={styles.lotteryResultNumbersContainer}>
                    <div className={styles.hiddenNumbersDisplay}>
                      <div className={styles.lotteryNumberHidden}>#?</div>
                      <div className={styles.lotteryNumberHidden}>#?</div>
                      <div className={styles.lotteryNumberHidden}>#?</div>
                    </div>
                    <div className={styles.lotteryRevealDate}>
                      {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "11월 19일 이후 공개됩니다." : "Will be revealed after November 19th."}
                    </div>
                  </div>
                );
              }
            })()}
          </div> */}

          {/* 나의 추첨 결과 확인 버튼 */}
          <div className={styles.lotteryResultButtonContainer}>
            <button 
              className={`${styles.lotteryResultButton} ${showFireworks ? styles.buttonExploding : ''}`}
              onClick={handleButtonClick}
            >
              <Trophy className={styles.buttonIcon} />
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "나의 추첨 결과 확인하기" : "Check My Raffle Result"}
            </button>
          </div>
        </div>
      </div>

      {/* 추가 정보 섹션 */}
      <div className={styles.lotteryInfoSection}>
        <div className={styles.lotteryInfoContent}>
          <div className={styles.lotteryInfoHeader}>
            <Clock className={styles.infoIcon} />
            <span className={styles.infoLabel}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨 안내" : "Raffle Guide"}
            </span>
          </div>
          {/* {lotteryPeriod && (
            <div className={styles.lotteryInfoDate}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "추첨일: " : "Draw Date: "}
              <span className={styles.lotteryDateValue}>{formatDate(lotteryPeriod)}</span>
            </div>
          )} */}
          <div className={styles.lotteryInfoList}>
            <div className={styles.lotteryInfoItem}>
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "• 추첨은 코인 번호를 기준으로 진행됩니다" : "• Raffle is conducted based on Battle Coin numbers."}
            </div>
            <div className={styles.lotteryInfoItem}>
              {currentSeasonId === 'R1' 
                ? (navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" 
                  ? "• MBX Station에서 DAPP 재화로 Import 후 상금을 수령 할 수 있습니다." 
                  : "• You can receive rewards by importing Battle Coin NFT to dApp currency in MBX Station.")
                : ""
              }
            </div>            
          </div>
        </div>
      </div>

      {/* 폭죽 애니메이션 */}
      {showFireworks && (
        <div className={styles.fireworksContainer}>
          {particles.map((particle) => (
            <div
              key={particle.id}
              className={styles.fireworkParticle}
              style={{
                '--angle': `${particle.angle}deg`,
                '--velocity': `${particle.velocity}px`,
                '--explodeX': `${particle.explodeX}px`,
                '--explodeY': `${particle.explodeY}px`,
                '--moveX': `${particle.moveX}px`,
                '--moveY': `${particle.moveY}px`,
                '--delay': `${particle.delay}ms`,
                '--centerX': `${particle.centerX}%`,
                '--centerY': `${particle.centerY}%`,
                '--launchX': `${particle.launchX}%`,
                '--launchY': `${particle.launchY}%`,
                '--size': `${particle.size}px`,
                '--color': particle.color
              }}
            />
          ))}
        </div>
      )}
    </div>
  );
}
