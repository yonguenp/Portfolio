import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import styles from "../../styles/BattleLeagueRight.module.css";
import { fetchGuideData } from "../../data/battleLeagueSlice";

export default function GuideTab() {
  const dispatch = useDispatch();
  
  // Redux에서 데이터 가져오기
  const { 
    guideData, 
    loading,
    selectedSeasonIndex,
    seasons
  } = useSelector((state) => state.battleLeague || {});

  const currentSeasonId = seasons[selectedSeasonIndex]?.id;

  // 가이드 데이터 API 호출
  useEffect(() => {
    // 시즌 ID가 있고, 현재 로딩 중이 아닐 때만 API 호출
    if (currentSeasonId && !loading?.guide) {      
      dispatch(fetchGuideData(currentSeasonId));
    }
  }, [dispatch, currentSeasonId, selectedSeasonIndex, seasons]);

  // 로딩 상태 처리
  if (loading?.guide) {
    return (
      <div className={styles.participationContent}>
        <div className={styles.loadingSpinner}>
          {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "로딩 중..." : "Loading..."}
        </div>
      </div>
    );
  }

  // Redux에서 가져온 데이터 사용 (실패 시 기본값 포함)
  const sections = guideData?.sections || [];

  // 가이드 섹션 렌더링 함수
  const renderGuideSection = (section) => {
    const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";
    
    return (
      <div key={section.id} className={styles.rulesSection}>
        <div className={styles.rulesHeader}>
          <div className={styles.rulesIcon}>{section.stepNumber}</div>
          <span className={styles.rulesLabel}>{isKorean ? section.titleKr : section.title}</span>
        </div>
        <div className={styles.rulesContent}>
          {section?.rules?.map((rule) => (
            <div key={rule.id} className={styles.ruleItem}>
              <span className={styles.ruleBullet}>•</span>
              <span className={styles.ruleText}>{isKorean ? rule.textKr : rule.text}</span>
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <div className={styles.guideContent}>
      {sections?.map((section) => renderGuideSection(section))}
    </div>
  );
}
