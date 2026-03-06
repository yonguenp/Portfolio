import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Gift, Trophy, Dice6, Package } from 'lucide-react';
import styles from "../../styles/BattleLeagueRight.module.css";
import { fetchItemsData } from "../../data/battleLeagueSlice";

// 아이콘 타입 매핑
const iconTypeMapping = {
  Trophy: Trophy,
  Gift: Gift, 
  Package: Package,
  Dice6: Dice6
};

export default function ItemsTab() {
  const dispatch = useDispatch();
  
  // Redux에서 데이터 가져오기
  const { 
    itemsData, 
    loading 
  } = useSelector((state) => state.battleLeague || {});

  // 아이템 데이터 API 호출
  useEffect(() => {
    //console.log("ItemsTab - API 호출 조건 체크:");
    //console.log("itemsData:", itemsData);
    //console.log("loading.items:", loading?.items);
    
    // 아직 데이터가 없을 때만 API 호출
    if (!itemsData && !loading?.items) {
      //console.log("아이템 데이터 API 호출 시도");
      dispatch(fetchItemsData());
    } else {
      //console.log("아이템 데이터 API 호출 조건 미충족 - itemsData:", !!itemsData, "loading:", loading?.items);
    }
  }, [dispatch, itemsData, loading?.items]);

  // 로딩 상태 처리
  if (loading?.items) {
    return (
      <div className={styles.itemsContent}>
        <div className={styles.loadingSpinner}>아이템 로딩 중...</div>
      </div>
    );
  }

  // Redux에서 가져온 데이터 사용 (실패 시 기본값 포함)
  const reward = itemsData?.reward || {};
  const info = itemsData?.info || {};

  //is_Korean
  const isKorean = navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true";

  return (
    <div className={styles.itemsContent}>

      {/* reward foreach */}
      {Object.keys(reward).map((key) => (
        <div className={styles.confirmedItems} key={key}> 
          <div className={styles.confirmedItemsHeader}>
            <span className={styles.confirmedItemsLabel}>{isKorean ? reward[key].titleKr : reward[key].title}</span>
            <div className={styles.statusBadge}>{reward[key].probability}</div>
          </div>

          <div style={{display: 'flex', flexDirection: 'column', gap: '10px'}}>
          {reward[key].items.map((item) => (
            <div className={styles.confirmedItem} key={item.name}>
              {/* 
              <div className={styles.confirmedItemIcon}>
                <item.iconType />
              </div>
               */}
              <div className={styles.confirmedItemInfo}>
              <div className={styles.confirmedItemName}>{isKorean ? item.nameKr : item.name}</div>
              <div className={styles.confirmedItemDescription}>{item.description}</div>
            </div>
            <div className={styles.confirmedItemCount}>{item.quantity || item.probability}</div>
            </div>
          ))}
          </div>
        </div>
      ))}

      {info && (
        <div className={styles.itemsInfo}>
          <div className={styles.itemsInfoContent}>
              <div className={styles.itemsInfoTitle}>{isKorean ? info.titleKr : info.title}</div>
              <div className={styles.itemsInfoText}>{(isKorean ? info.rulesKr : info.rules)?.map((rule, index) => (
                <div className={styles.ruleItem} key={index}>
                  <span className={styles.ruleBullet}>•</span>
                  <span className={styles.ruleText}>{rule.text}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
