import { useState, useEffect, useRef } from 'react';
import styles from "../../styles/OpenEventPageRight.module.css";
import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function OpenEventPageRight({ data, selectedTab }) {
  const { openPopup } = usePopup();

  const rankScrollRef = useRef(null);
  const rewardScrollRef = useRef(null);
  const [showScrollArrow, setShowScrollArrow] = useState(true);
  const [hoveredName, setHoveredName] = useState('');
  const [hoverPos, setHoverPos] = useState({ x: 0, y: 0 });
  const [rankData, setRankData] = useState([]);

  const handleMouseEnter = (e, name) => {
    setHoveredName(name);
    setHoverPos({ x: e.clientX, y: e.clientY });
  };

  const handleMouseMove = (e) => {
    const tooltipWidth = 120;
    const tooltipHeight = 30;
    const padding = 10;

    const posX = (e.clientX + tooltipWidth + padding > window.innerWidth)
      ? e.clientX - tooltipWidth - padding
      : e.clientX + padding;

    const posY = (e.clientY + tooltipHeight + padding > window.innerHeight)
      ? e.clientY - tooltipHeight - padding
      : e.clientY + padding;

    setHoverPos({ x: posX, y: posY });
  };

  const handleMouseLeave = () => {
    setHoveredName('');
  };

  const getItemName = (no) => {
    switch (no) {
      case 10000005:
      case 'dia': return 'Diamond';
      case 10000023:
      case 'magnite': return "MAGNITE";
      case 10000024:
      case 'mbx': return "gMBX";

      case 30000006: return '[COMMON] Dragon Gacha';
      case 30000007: return '[UNCOMMON] Dragon Gacha';
      case 30000008: return '[RARE] Dragon Gacha';
      case 30000009: return '[UNIQUE] Dragon Gacha';
      case 30000010: return '[LEGENDARY] Dragon Gacha';

      case 30000003: return 'Premium Gacha Ticket';
      case 30000004: return 'Premium Pet Gacha Ticket';
      default:
        return 'unknown';
    }
  };

  const getFontSizeForValue = (value) => {
    if (value > 9999999) return '4px';
    if (value > 999999) return '6px';
    if (value > 99999) return '8px';
    if (value > 9999) return '10px';
    if (value > 999) return '12px';
    return '14px';
  };

  const getItemImageFileName = (no) => {
    switch (no) {
      case 10000005:
      case 'dia': return 'shop/icon/diamond.png';
      case 10000023:
      case 'magnite': return "icon/magnite.png";
      case 10000024:
      case 'mbx': return "icon/mbx.png";
      case 30000006: return "shop/ticket_icon/C.png";
      case 30000007: return "shop/ticket_icon/R.png";
      case 30000008: return "shop/ticket_icon/U.png";
      case 30000009: return "shop/ticket_icon/UC.png";
      case 30000010: return "shop/ticket_icon/L.png";

      case 30000003: return "shop/ticket_icon/Verygood.png";
      case 30000004: return 'shop/ticket_icon/Pet_Verygood';
      default:
        return "icon/icon.png";
    };
  };

  const formatNumberShort = (num) => {
    if (!num) return "";

    const format = (value, unit) => {
      const fixed = (value).toFixed(2);
      const trimmed = fixed.replace(/\.?0+$/, "");
      return `${trimmed}${unit}`;
    };

    if (num >= 1_000_000_000) return format(num / 1_000_000_000, "b");
    else if (num >= 1_000_000) return format(num / 1_000_000, "m");
    else return num.toLocaleString();
    // return num.toString();
  };

  function getEnglishRank(rank) {
    if (typeof rank !== "number" || rank <= 0) return "-";

    const j = rank % 10,
      k = rank % 100;

    let suffix = "th";
    if (j === 1 && k !== 11) suffix = "st";
    else if (j === 2 && k !== 12) suffix = "nd";
    else if (j === 3 && k !== 13) suffix = "rd";

    return `${rank}${suffix}`;
  }

  const fetchRank = async () => {
    if (selectedTab <= 0)
      return;

    const result = await window.DApp.post(`event/ranking`, {
      server_tag: sessionStorage.getItem("server_tag"),
      event_key: selectedTab
    });

    if (result && result.rs == 0) {
      setRankData(result);
    }
  }

  useEffect(() => {
    if (rankScrollRef.current) rankScrollRef.current.scrollTop = 0;
    if (rewardScrollRef.current) rewardScrollRef.current.scrollTop = 0;

    fetchRank();
  }, [selectedTab]);

  useEffect(() => {
    if (!data) return;

    const scrollEl = rewardScrollRef.current;
    if (!scrollEl) return;

    const handleScroll = () => {
      const isAtBottom =
        scrollEl.scrollTop + scrollEl.clientHeight >= scrollEl.scrollHeight - 1;
      setShowScrollArrow(!isAtBottom);
    };

    scrollEl.addEventListener("scroll", handleScroll);
    handleScroll();

    return () => {
      scrollEl.removeEventListener("scroll", handleScroll);
    };
  }, [data, selectedTab]);

  if (!data || Object.keys(data).length === 0)
    return <div>Loading... </div>;

  const { rewards } = data;
  const { ranking, mine } = rankData;

  const imageUrl = `https://d2efgqatv3752r.cloudfront.net/banner/event/en/${data.image ? data.image : 'notice_event_all.jpg'}`;

  return (
    <div className={styles.container}>
      {/* 왼쪽 순위 영역 */}
      <div className={styles.container_left}>
        {/* 카테고리 헤더 */}
        <div className={`${styles.containerLeftBox} ${styles.categoryHeader}`}>
          <div className={styles.containerLeftBoxInner}>
            <span className={`${styles.rankText} ${styles.headerText}`}>Rank</span>
            <div className={`${styles.nameWrapper} ${styles.headerText}`}>{data.key == 1000004 || data.key == 1000007 ? 'Guild Name' : 'Nick Name'}</div>
            <span className={`${styles.pointText} ${styles.headerText}`}>Points</span>
          </div>
        </div>
        <div ref={rankScrollRef} className={styles.rankListScroll}>
          {
            ranking && ranking.length > 0 ?
              Object.entries(ranking).map(([key, val], index) => {
                let rankColorClass = "";
                if (val.rank === 1) rankColorClass = styles.goldBox;
                else if (val.rank === 2) rankColorClass = styles.silverBox;
                else if (val.rank === 3) rankColorClass = styles.bronzeBox;

                return (
                  val.point > 0 ?
                    <div key={key} className={`${styles.containerLeftBox} ${rankColorClass}`}>
                      <div className={styles.containerLeftBoxInner}>
                        <span className={styles.rankText}>{getEnglishRank(val.rank)}</span>

                        <div className={styles.nameWrapper}>
                          {val.nick}
                        </div>

                        {val.point !== 0 && (
                          <span className={styles.pointText}>
                            {formatNumberShort(val.point)}
                          </span>
                        )}
                      </div>
                    </div>
                    : <div></div>
                );
              })
              :
              <div>
                There are currently no recorded rankings.
              </div>
          }
        </div>

        <div className={styles.scrollMask} />

        {/* 내 순위 박스 */}
        <div className={`${styles.containerLeftBox} ${styles.containerLeftBoxMyRank} ${mine?.rank === 1
          ? styles.goldBox
          : mine?.rank === 2
            ? styles.silverBox
            : mine?.rank === 3
              ? styles.bronzeBox
              : ""
          }`}
        >
          <div className={styles.containerLeftBoxInner}>
            <span className={styles.rankText}>My Rank</span>

            <span className={styles.nameWrapper}>{mine?.point > 0 ? getEnglishRank(mine?.rank) : '-'}</span>

            {mine?.point ? (
              <span className={styles.pointText}>
                {formatNumberShort(mine.point)}
              </span>
            ) : null}
          </div>
        </div>
      </div>

      <div className={styles.verticalDivider} />

      {/* 오른쪽 보상 영역 */}
      <div ref={rewardScrollRef} className={styles.container_right}>
        <img src={imageUrl} alt={data.title} className={styles.banner} />
        {
          data.event_desc ? (
            <>
              <div className={styles.containerRightBoxInner}>
                <div dangerouslySetInnerHTML={{ __html: data.event_desc }} />
                <br />
                <p style={{ color: 'orange', fontSize: '12px' }}>
                  🚨 Please check the community for event details.
                </p>
                <p style={{ color: 'yellow', fontSize: '12px' }}>
                  ⚠️ When rankings are confirmed, accounts with linked wallets will receive Web3 rewards.
                </p>
              </div>
            </>
          ) :
            (
              <>
                <div className={styles.containerRightBoxInner}>
                  <p style={{ color: 'orange', fontSize: '12px' }}>
                    🚨 Please check the community for event details.
                  </p>
                  <p style={{ color: 'yellow', fontSize: '12px' }}>
                    ⚠️ When rankings are confirmed, accounts with linked wallets will receive Web3 rewards.
                  </p>
                </div>
              </>
            )
        }
        <br />
        <div className={styles.containerRightBox}>
          <div className={styles.rewardTitle}>Reward</div>
          {/* 테이블 헤더 */}
          <div className={styles.rewardRow + ' ' + styles.rewardHeader}>
            <div className={styles.rewardCell + ' ' + styles.top}>Rank</div>
            {
              rewards.length > 0 && rewards[0].web3 ?
                data.key == 1000004 || data.key == 1000007 ? <div className={styles.rewardCell + ' ' + styles.top} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>To Leader
                  <p style={{ color: 'orange' }}>
                    Wallet Linked
                  </p>
                </div>
                  : <div className={styles.rewardCell + ' ' + styles.top} style={{ color: 'orange' }}>Wallet Linked</div>
                : <></>
            }
            {
              rewards.length > 0 && rewards[0].web2 ?
                data.key == 1000004 || data.key == 1000007 ? <div className={styles.rewardCell + ' ' + styles.top}>Each Members</div>
                  : <div className={styles.rewardCell + ' ' + styles.top}>{rewards.length > 0 && rewards[0].web3 ? "Not Linked" : "Rank Reward"}</div>
                : <></>
            }
          </div>
          <div className={styles.rewardListScroll}>
            {/* 보상 데이터 렌더링 */}
            {rewards.map((val, index) => (
              <div
                key={index}
                className={styles.rewardRow}
              >
                {/* 랭크 셀 */}
                <div className={styles.rewardCell}>
                  {val.rank_range && val.rank_range.length > 0 ? (
                    val.rank_range.length === 1
                      ? getEnglishRank(Number(val.rank_range[0]))
                      :
                      <span style={{ fontSize: '10px' }}>
                        {`${getEnglishRank(Number(val.rank_range[0]))} ~ ${getEnglishRank(Number(val.rank_range[1]))}`}
                      </span>
                  ) : (
                    <div className={styles.emptyReward}>Ptc.</div>
                  )}
                </div>
                {/* Web3 보상 셀 */}

                {val.web3 && val.web3.length > 0 ? (
                  <div className={styles.rewardCell + ' ' + styles.rewardWeb3}>
                    {
                      val.web3.map((item, idx) => (
                        <div
                          className={styles.rewardGroup}
                          key={`web3-${item[1]}-${idx}`}
                          onMouseEnter={(e) => handleMouseEnter(e, getItemName(item[1]) + " " + formatNumberShort(item[2]))}
                          onMouseMove={handleMouseMove}
                          onMouseLeave={handleMouseLeave}
                        >
                          <img
                            src={`${process.env.REACT_APP_TEST_PATH}/images/${getItemImageFileName(item[1])}`}
                            alt={item[1]}
                            className={styles.rewardIcon}
                          />
                          <div className={styles.rewardValue} style={{ fontSize: '10px', transform: 'translateY(-7px)' }}>
                            {formatNumberShort(item[2])}
                          </div>
                        </div>
                      ))}
                  </div>
                ) : (
                  (rewards.length > 0 && rewards[0].web3 && data.key == 1000004 || data.key == 1000007) && (
                    <div className={styles.rewardCell + ' ' + styles.rewardWeb3}>
                      <div className={styles.emptyReward}>-</div>
                    </div>
                  )
                )}
                {/* Web2 보상 셀 */
                  val.web2 ?
                    (
                      <div className={styles.rewardCell + ' ' + styles.rewardWeb2 +
                        (data.key != 1000004 && data.key != 1000007 && val.web3?.length === 0 ? ' ' + styles.rewardWeb2Full : '')
                      }>
                        {val.web2.length > 0 ? val.web2.map((item, idx) => (
                          <div
                            className={styles.rewardGroup}
                            key={`web2-${item[1]}-${idx}`}
                            onMouseEnter={(e) => handleMouseEnter(e, getItemName(item[1]) + " " + formatNumberShort(item[2]))}
                            onMouseMove={handleMouseMove}
                            onMouseLeave={handleMouseLeave}
                          >
                            <img
                              src={`${process.env.REACT_APP_TEST_PATH}/images/${getItemImageFileName(item[1])}`}
                              alt={item[1]}
                              className={styles.rewardIcon}
                            />
                            <div className={styles.rewardValue} style={{ fontSize: '10px', transform: 'translateY(-7px)' }}>
                              {formatNumberShort(item[2])}
                            </div>
                          </div>
                        )) : (
                          <div className={styles.emptyReward}>-</div>
                        )}
                      </div>
                    ) 
                    : (<></>)
                }
              </div>
            ))}
          </div>
          {showScrollArrow && (
            <img
              src={process.env.REACT_APP_TEST_PATH + "/images/icon/arrow-btot-white.png"}
              alt="scroll down"
              className={styles.scrollArrow}
            />
          )}
        </div>
      </div>
      {
        hoveredName && (
          <div
            className={styles.rewardTooltip}
            style={{
              top: hoverPos.y + 10 + 'px',  // 마우스 아래 10px 위치
              left: hoverPos.x + 10 + 'px',
            }}
          >
            {hoveredName}
          </div>
        )
      }
    </div>
  );
};