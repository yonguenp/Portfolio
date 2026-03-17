import { useState, useEffect } from 'react';
import styles from "../../styles/NoticePageLeft.module.css";
import { usePopup } from "../../context/PopupContext";
export default function NoticePageLeft({ datas, selectedIndex, setSelectedIndex }) {
  const { openPopup } = usePopup();

  useEffect(() => {

  })

  return (
    <div className={styles.container}>
      {datas.map((item, idx) => (
        <div
          key={item.key}
          className={`${styles.extractorBoxBottom} font-b4-b cursor-pointer ${selectedIndex === idx ? "bg-red-400 text-white" : "bg-gray-600 text-gray-300"}`}
          onClick={() => setSelectedIndex(idx)}
        >
          {item.title}
        </div>
      ))}
    </div>
  );
}
