import styles from "../../styles/Sidebar.module.css";
import { Link, useLocation } from "react-router-dom";

import { ReactComponent as Tradable } from "../../assets/svg/sidebar/tradable.svg";
import { ReactComponent as Wallet } from "../../assets/svg/sidebar/wallet.svg";
import { ReactComponent as Genesis } from "../../assets/svg/sidebar/genesis.svg";

import { ReactComponent as Exchange } from "../../assets/svg/sidebar/exchange.svg";
import { ReactComponent as ArtBlock } from "../../assets/svg/sidebar/artBlock.svg";
import { ReactComponent as Market } from "../../assets/svg/sidebar/market.svg";
import { ReactComponent as MetodiumShop } from "../../assets/svg/sidebar/craft3.svg";
import { ReactComponent as MigrationClaim } from "../../assets/svg/sidebar/_exchange.svg";
import { ReactComponent as DeskPage } from "../../assets/svg/sidebar/deskpage.svg";
import { ReactComponent as OpenEventPage } from "../../assets/svg/sidebar/event.svg";
import { ReactComponent as NoticePage } from "../../assets/svg/sidebar/notice.svg";
import { ReactComponent as SeasonHistory } from "../../assets/svg/sidebar/seasonHistory.svg";
import { ReactComponent as BattleLeague } from "../../assets/svg/sidebar/sword.svg";
import { ReactComponent as DAppInventory } from "../../assets/svg/sidebar/wallet.svg";
import React, { useEffect, useState } from "react";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function Sidebar({ paddingRight }) {
  const location = useLocation();
  const [showEventIcon, setShowEventIcon] = useState(false);
  const { userInfo, isLoggedIn, useChampion } = useDAppState();
  const [web2, setWeb2] = useState(false);

  useEffect(() => {
      if (Number(sessionStorage.getItem('web3event'))) {
        setShowEventIcon(true);
      }

      setWeb2(sessionStorage.getItem("web2") === "true");
  }, [isLoggedIn]);

  // 메뉴 데이터 객체
  const menuItems = {
    noticePage: {
      icon: NoticePage,
      ko: "공지사항",
      en: "Notice",
      hasLineBreak: false
    },
    openEventPage: {
      icon: OpenEventPage,
      ko: "이벤트",
      en: "Event",
      hasLineBreak: false
    },
    exchange: {
      icon: Exchange,
      ko: "마그나이트 제련소",
      en: "Magnite Forge",
      hasLineBreak: true
    },
    metodiumShop: {
      icon: MetodiumShop,
      ko: "마그나이트 상점",
      en: "Magnite Shop",
      hasLineBreak: true
    },
    deskPage: {
      icon: DeskPage,
      ko: "챔피언 오라클",
      en: "Champion Oracle",
      hasLineBreak: true
    },
    artblock: {
      icon: ArtBlock,
      ko: "드래곤 증폭기",
      en: "Dragon Amplifier",
      hasLineBreak: true
    },
    migrationClaim: {
      icon: MigrationClaim,
      ko: "에코 바우처 클레임",
      en: "Echo Voucher Claim",
      hasLineBreak: true
    },
    seasonHistory: {
      icon: SeasonHistory,
      ko: "시즌 히스토리",
      en: "Season History",
      hasLineBreak: true
    },
    battleLeague: {
      icon: BattleLeague,
      ko: "배틀 리그",
      en: "Battle League",
      hasLineBreak: true
    },
    dappInventory: {
        icon: DAppInventory,
        ko: "디앱 인벤토리",
        en: "DApp Inventory",
        hasLineBreak: true
    },
    staking: {
      icon: Genesis,
      ko: "드래곤카드",
      en: "Dragon Card",
      hasLineBreak: true
    },
    trade: {
      icon: Market,
      ko: "거래소",
      en: "Trading Market",
      hasLineBreak: true
    }
  };

  // 텍스트 가져오기 함수
  const getMenuText = (menuKey) => {
    const menu = menuItems[menuKey];
    if (!menu) return "";
    
    const isKorean = navigator.language === "ko-KR" && web2;
    return isKorean ? menu.ko : menu.en;
  };

  // 메뉴 아이템 렌더링 함수
  const renderMenuItem = (menuKey, path, isActive = false, condition = true) => {
    if (!condition) return null;
    
    const menu = menuItems[menuKey];
    if (!menu) return null;
    
    const IconComponent = menu.icon;
    const text = getMenuText(menuKey);
    
    return (
      <li className={styles.sidebarListItem} key={menuKey}>
        <Link to={path}>
          <IconComponent
            fill={isActive ? "#F90052" : "#9E9E9E"}
          />
          <span
            className={`${styles.sidebarListItemText} ${
              isActive ? styles.active : ""
            }`}
          >
            {menu.hasLineBreak ? (
              text.split(' ').map((word, index) => (
                <React.Fragment key={index}>
                  {word}
                  {index === 0 && <br />}
                </React.Fragment>
              ))
            ) : (
              text
            )}
          </span>
        </Link>
      </li>
    );
  };
  
  return (
    <div className={styles.container}>
      <div className={styles.sidebarWrapper}>
        <ul className={styles.sidebarList}>
          {renderMenuItem('noticePage', '/noticePage', location.pathname === '/noticePage', web2 === false)}
          {renderMenuItem('openEventPage', '/openEventPage', location.pathname === '/openEventPage', web2 === false && showEventIcon)}
          {renderMenuItem('trade', '/trade', location.pathname === '/trade', true)}
          {renderMenuItem('metodiumShop', '/metodiumShop', location.pathname === '/metodiumShop' || location.pathname === '/', true)}
          {renderMenuItem('artblock', '/artblock', location.pathname === '/artblock', true)}
          {renderMenuItem('migrationClaim', '/migrationClaim', location.pathname === '/migrationClaim', isLoggedIn && userInfo?.mbx_migration)}
          {renderMenuItem('deskPage', '/deskPage', location.pathname === '/deskPage', useChampion)}          
          {renderMenuItem('battleLeague', '/battleLeague', location.pathname === '/battleLeague', true)}
          {renderMenuItem('dappInventory', '/nftInventory', location.pathname === '/nftInventory', web2 === false)}
          {renderMenuItem('staking', '/staking', location.pathname === '/staking', web2 === false)}
          {renderMenuItem('exchange', '/exchange', location.pathname === '/exchange', true)}
          {/* <li className={styles.sidebarListItem}>
            <Link to="/mint">
              <ArtBlock
                fill={location.pathname === "/mint" ? "#F90052" : "#9E9E9E"}
              />
              <span
                className={`${styles.sidebarListItemText} 
                                            ${
                                              location.pathname === "/mint"
                                                ? styles.active
                                                : ""
                                            }
                                            `}
              >
                MINT
              </span>
            </Link>
          </li> */}
          {/* <li className={styles.sidebarListItem}>
            <Link to="/itemMarket">
              <Market
                fill={
                  location.pathname === "/itemMarket" ? "#F90052" : "#9E9E9E"
                }
              />
              <span
                className={`${styles.sidebarListItemText} 
                                            ${
                                              location.pathname ===
                                              "/itemMarket"
                                                ? styles.active
                                                : ""
                                            }
                                            `}
              >
                Item
                <br />
                Market
              </span>
            </Link>
          </li>

          <li className={styles.sidebarListItem}>
            <Link to="/wallet">
              <Wallet
                fill={location.pathname === "/wallet" ? "#F90052" : "#9E9E9E"}
              />
              <span
                className={`${styles.sidebarListItemText} 
                                            ${
                                              location.pathname === "/wallet"
                                                ? styles.active
                                                : ""
                                            }
                                            `}
              >
                Wallet
              </span>
            </Link>
          </li> */}
          { 
          /*
          sessionStorage.getItem("server_tag") == 1 && (<li className={styles.sidebarListItem}>
            <Link to="/staking">
              <Genesis
                fill={location.pathname === "/staking" ? "#F90052" : "#9E9E9E"}
              />
              <span
                className={`${styles.sidebarListItemText} 
                                            ${
                                              location.pathname === "/staking"
                                                ? styles.active
                                                : ""
                                            }
                                            `}
              >
                Genesis
                <br />
                Card
              </span>
            </Link>
          </li>)
          */
          }
          {/*
          <li className={styles.sidebarListItem}>
            <Link to="/nftInventory">
              <Tradable
                fill={
                  location.pathname === "/nftInventory" ? "#F90052" : "#9E9E9E"
                }
              />
              <span
                className={`${styles.sidebarListItemText} 
                                            ${
                                              location.pathname ===
                                              "/nftInventory"
                                                ? styles.active
                                                : ""
                                            }
                                            `}
              >
                My
                <br />
                Inventory
              </span>
            </Link>
          </li>
           */}
        </ul>
      </div>
    </div>
  );
}
