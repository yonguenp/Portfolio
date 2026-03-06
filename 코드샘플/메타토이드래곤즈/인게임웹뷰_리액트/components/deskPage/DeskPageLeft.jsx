import { useState, useEffect } from 'react';
import styles from "../../styles/DeskPageLeft.module.css";
import { ReactComponent as QuestionBig } from "../../assets/svg/common/question_big.svg";
import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { ReactComponent as QuestShield } from "../../assets/svg/common/quest_shield.svg";
import { ReactComponent as QuestShieldBlur } from "../../assets/svg/common/quest_shield_blur.svg";
export default function DeskPageLeft({ layoutType, setLayoutType }) {
  const { openPopup } = usePopup();

  useEffect(() => {

  })

  const renderIcon = (type) => {
    switch (type) {
      case "claim_prize":
        return <span>Prize</span>;
      case "claim_win":
        return <span>Winnings</span>;
      default:
        return null;
    }
  };

  return (
    <div className={styles.container}>
      {[
        // "purchase", "exchange",
         "claim_win", "claim_prize"].map((type) => (
        <div
          key={type}
          className={`${styles.extractorBoxBottom} font-b3-b cursor-pointer ${
            layoutType === type ? "bg-red-400 text-white" : "bg-gray-600 text-gray-300"
          }`}
          onClick={() => setLayoutType(type)}
        >
          <div className="flex items-center">
            {renderIcon(type)}
          </div>
        </div>
      ))}
    </div>

  );
}
