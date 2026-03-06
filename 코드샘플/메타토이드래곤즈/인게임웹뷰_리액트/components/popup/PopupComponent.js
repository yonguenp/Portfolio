// PopupComponent.js

import "../../styles/Popup.css";
import "./style.css";
import Popup from "reactjs-popup";
import React, { useState, useEffect } from "react";
import { usePopup } from "../../context/PopupContext";

import WalletChange from "./component/WalletChange";
import MBXStationPopup from "./component/MBXStationPopup";
import MetodiumShop from "./component/MetodiumShop";
import NFTInventory from "./component/NFTInventory";
import Exchange from "./component/Exchange";
import ExchangeforgeManual from "./component/ExchangeforgeManual";
import GenesisCardActivate from "./component/GenesisCardActivate";
import GenesisInsertCard from "./component/GenesisInsertCard";
import GenesisWithdrawCard from "./component/GenesisWithdrawCard";
import ExchangeforgeCost from "./component/ExchangeforgeCost";
import Upgrade from "./component/Upgrade";
import SmeltMetodium from "./component/SmeltMetodium";
import GenesisCardManual from "./component/GenesisCardManual";
import ArtBlockConvert from "./component/ArtBlockConvert";
import ArtBlockEdit from "./component/ArtBlockEdit";
import MessagePopup from "./component/Message";
import GenesisClaim from "./component/GenesisClaim";
import ForceClaim from "./component/ForceClaim";
import ClaimManual from "./component/ClaimManual";
import ArtBlockBuffPointManual from "./component/ArtBlockBuffPointManual";
import { AnimatePresence, motion } from "framer-motion";

const PopupComponent = () => {
  const { isOpen, popupData, openPopup, closePopup } = usePopup();
  const handlePopup = (data) => {
    openPopup({
      type: "MessagePopup",
      title: data.title,
      msg: data.msg,
      isRefresher: data?.isRefresher !== undefined ? data.isRefresher : (data?.err == 1),
      isWVCloser: data?.wvclose ? true : false,
      onRefresh: data?.onRefresh,
    });
  };
  useEffect(() => {
    window.DApp.on("dapp.popup", handlePopup);
    return () => {
      window.DApp?.removeListener("dapp.popup", handlePopup);
    };
  }, []);

  return (
    <AnimatePresence>
  {isOpen && (
    <Popup
      open
      onClose={closePopup}
      modal
      contentStyle={{
        borderRadius: "16px",
        background: "transparent",
        border: "none",
        padding: 0,
      }}
    >
      <motion.div
        key={popupData?.type}
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.9 }}
        transition={{ duration: 0.2 }}
        className="popup-motion-wrapper"
        style={{
          background: "white",
          borderRadius: "16px",
          padding: "0px",
          boxShadow: "0 10px 30px rgba(0, 0, 0, 0.2)",
        }}
      >
        {/* 팝업 컴포넌트 조건부 렌더링은 이 안에! */}
        {popupData?.type === "WalletChange" && (
          <WalletChange popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "MBXStationPopup" && (
          <MBXStationPopup popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "NFTInventory" && (
          <NFTInventory popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "Exchange" && (
          <Exchange popupData={popupData} closePopup={closePopup} onRefresh={popupData?.onRefresh} />
        )}
        {popupData?.type === "ExchangeforgeManual" && (
          <ExchangeforgeManual popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "ExchangeforgeCost" && (
          <ExchangeforgeCost popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "Upgrade" && (
          <Upgrade popupData={popupData} closePopup={closePopup} onRefresh={popupData?.onRefresh}/>
        )}
        {popupData?.type === "SmeltMetodium" && (
          <SmeltMetodium popupData={popupData} closePopup={closePopup} onRefresh={popupData?.onRefresh} />
        )}
        {popupData?.type === "MetodiumShop" && (
          <MetodiumShop popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "GenesisCardActivate" && (
          <GenesisCardActivate popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "GenesisInsertCard" && (
          <GenesisInsertCard popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "GenesisWithdrawCard" && (
          <GenesisWithdrawCard popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "GenesisClaim" && (
          <GenesisClaim popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "ForceClaim" && (
          <ForceClaim popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "GenesisCardManual" && (
          <GenesisCardManual popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "ClaimManual" && (
          <ClaimManual popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "ArtBlockConvert" && (
          <ArtBlockConvert popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "ArtBlockEdit" && (
          <ArtBlockEdit popupData={popupData} closePopup={closePopup} />
        )}
        {popupData?.type === "ArtBlockBuffPointManual" && (
          <ArtBlockBuffPointManual
            popupData={popupData}
            closePopup={closePopup}
          />
        )}
        {popupData?.type === "MessagePopup" && (
          <MessagePopup popupData={popupData} closePopup={closePopup} />
        )}
      </motion.div>
    </Popup>
  )}
</AnimatePresence>
  );
};

export default PopupComponent;
