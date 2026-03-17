import React, { memo } from "react";
import { useLocation } from "react-router-dom";
import { useSelector, useDispatch } from "react-redux";
import Magnite from "./Magnite";
import EchoVoucher from "./EchoVoucher";
import ChampChip from "./ChampChip";
import Magnet from "./Magnet";
import MagnetLock from "./MagnetLock";
import { ReactComponent as Wallet } from "../../assets/svg/sidebar/wallet.svg";
import { ReactComponent as WalletChange } from "../../assets/svg/header/wallet_change.svg";
import { ReactComponent as NoWallet } from "../../assets/svg/sidebar/noWallet.svg";
import { ReactComponent as Tradable } from "../../assets/svg/sidebar/tradable.svg";
import { ReactComponent as Exchange } from "../../assets/svg/sidebar/exchange.svg";
import { ReactComponent as MetodiumShop } from "../../assets/svg/sidebar/craft3.svg";
import { ReactComponent as ArtBlock } from "../../assets/svg/sidebar/artBlock.svg";
import { ReactComponent as Market } from "../../assets/svg/sidebar/market.svg";
import { ReactComponent as Genesis } from "../../assets/svg/sidebar/genesis.svg";
import { ReactComponent as MigrationClaim } from "../../assets/svg/sidebar/_exchange.svg";
import { ReactComponent as DeskPage } from "../../assets/svg/sidebar/deskpage.svg";
import { ReactComponent as OpenEventPage } from "../../assets/svg/sidebar/event.svg";
import { ReactComponent as NoticePage } from "../../assets/svg/sidebar/notice.svg";
import { ReactComponent as SeasonHistory } from "../../assets/svg/sidebar/seasonHistory.svg";
import { ReactComponent as BattleLeague } from "../../assets/svg/sidebar/sword.svg";
import { ReactComponent as Trade } from "../../assets/svg/sidebar/market.svg";

import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import StatPoint from "./StatPoint";

const Header = ({ paddingRight }) => {
  //const navigate = useNavigate();
  const location = useLocation();
  //const queryParams = new URLSearchParams(location.search);
  //const token = queryParams.get('token') ?? 'testToken';

  const { isOpen, openPopup, closePopup } = usePopup();

  const { userInfo, isLoggedIn } = useDAppState();
  //useSelector(state => state.user);

  const headerTitle =
    location.pathname === "/metodiumShop"
      ? navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트 상점" : "Magnite Shop"

      : location.pathname === "/nftInventory"
      ? "DApp Inventory"

      : location.pathname === "/exchange"
      ? navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "마그나이트 제련소" : "Magnite Forge"

      : location.pathname === "/staking"
      ? "Dragon Card"
      : location.pathname === "/market"
      ? "Item Market"
      : location.pathname === "/artblock"
      
      ? navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "드래곤 증폭기" : "Dragon Amplifier"
      : location.pathname === "/migrationClaim"
      ? "Echo Voucher Claim"
      : location.pathname === "/deskPage"
      ? "Champion Oracle"
      : location.pathname === "/openEventPage"
      ? "Event"
      : location.pathname === "/noticePage"
      ? "Notice"
      : location.pathname === "/seasonHistory"
      ? navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "시즌 기록" : "Season History"            
      : location.pathname === "/battleLeague"
      ? navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "배틀 리그" : "Battle League"
      : location.pathname === "/trade"
      ? "Trading Market"
      : "MTDZ"            
      ;

  const walletChange = () => {
    if (userInfo.addr) {
      openPopup({ type: "WalletChange", data: "" });
      //dispatch(userLogout());
    } else {
      //dispatch(fetchUserFromToken("testToken"));
      window.DApp.connect();
    }
  };

  const headerIcons = {
    "/noticePage": [

    ],
    "/openEventPage": [

    ],
    "/exchange": [      
      { id: "my-tooltip2", component: <MagnetLock magnet={userInfo?.magnet?.locked || 0} /> },
      { id: "my-tooltip2", component: <Magnet magnet={userInfo?.magnet?.unlocked || 0} /> },
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
    ],
    "/metodiumShop": [
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
    ],
    "/deskPage": [
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
      { id: "my-tooltip4", component: <ChampChip champoracle={userInfo?.champoracle || 0} /> },
    ],
    "/artblock": [
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
      { id: "my-tooltip5", component: <StatPoint/> },
    ],
    "/migrationClaim": [
      { id: "my-tooltip3", component: <EchoVoucher echovoucher={userInfo?.echovoucher || 0} /> },
    ],
    "/seasonHistory": [
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
    ],
    "/battleLeague": [
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
      { id: "my-tooltip3", component: <EchoVoucher echovoucher={userInfo?.echovoucher || 0} /> },
    ],
    "/staking": [
      { id: "my-tooltip2", component: <Magnet magnet={userInfo?.magnet?.unlocked || 0} /> },
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
    ],
    "/trade": [
      { id: "my-tooltip1", component: <Magnite magnite={userInfo?.magnite || 0} /> },
    ],
  };

  const OpenExternalLink = () => {
      openPopup({ type: "MBXStationPopup", data: "" });
  };

  let web2 = sessionStorage.getItem("web2") === "true";
  return (
    <header className="header">
      <div className="headerLeft font-b2-b">
        <div className="headerTitle">
          {
          location.pathname === "/" ? 
            web2 ? 
            <Exchange fill="white" />
            :
            <NoticePage fill="white" />
           : location.pathname === "/noticePage" ? ( 
            <NoticePage fill="white" />
          ) : location.pathname === "/nftInventory" ? (
            <Tradable fill="white" />
          ) : location.pathname === "/exchange" ? (
            <Exchange fill="white" />
          ) : location.pathname === "/staking" ? (
            <Genesis fill="white" />
          ) : location.pathname === "/artblock" ? (
            <ArtBlock fill="white" />
          ) : location.pathname === "/market" ? (
            <Market fill="white" />
          ) : location.pathname === "/wallet" ? (
            <Wallet fill="white" />
          ) : location.pathname === "/migrationClaim" ? (
            <MigrationClaim fill="white" />
          ) : location.pathname === "/deskPage" ? (
            <DeskPage fill="white" />
          ) : location.pathname === "/openEventPage" ? (
            <OpenEventPage fill="white" />
          ) : location.pathname === "/metodiumShop" ? (
            <MetodiumShop fill="white" />
          ) : location.pathname === "/seasonHistory" ? (
            <SeasonHistory fill="white" />
          ) : location.pathname === "/battleLeague" ? (
            <BattleLeague fill="white" />
          ) : location.pathname === "/trade" ? (
            <Trade fill="white" />
          ) : (            
            <></>
          )}
          {headerTitle}
        </div>
        <div className="headerCenter">
      
        {
          headerIcons[location.pathname]?.length > 0 &&
          headerIcons[location.pathname].map((item, idx) => (
            <div key={idx} data-tooltip-id={item.id} data-tooltip-offset={5}>
              {item.component}
            </div>
          ))
        }

        </div>
      </div>
      

      <div
        className="headerRight"
        style={{ paddingRight: `${paddingRight}px` }}
      >
        {/* <div className="headerRightUserInfo">
          {userInfo?.user_no && <span>UID : {userInfo.user_no}</span>}
          {userInfo?.nick && <span>Name : {userInfo.nick}</span>}
        </div> */}
        { 
          sessionStorage.getItem("web2") !== "true" ?
          <button
            className="headerRightButton font-b3-b white"
            onClick={OpenExternalLink}
          >
               <>
              <img
              src={process.env.REACT_APP_TEST_PATH + "/images/icon/mbx.png"}
              alt=""
              style={{ width: "20px", height: "20px" }}
              />
                MBX Station
              </>

          </button>
          : <></>
        }
        {/* <img className="profileImg" src="/images/icon/icon.png" alt="" />
                <span>런천미트</span>
                <CloseIcon htmlColor='black' className="closeIcon" /> */}
      </div>
      {/* <Tooltip place="top" effect="solid" /> */}
    </header>
  );
};

export default memo(Header);
