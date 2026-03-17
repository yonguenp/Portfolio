import { useState, useEffect } from 'react';
import styles from "../../styles/OpenEventPageLeft.module.css";
import { usePopup } from "../../context/PopupContext";

export default function OpenEventPageLeft({ datas, selectedIndex, setSelectedIndex }) {
  const { openPopup } = usePopup();
  const [now, setNow] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => {
      setNow(new Date());
    }, 60 * 1000); // 1분마다 갱신

    return () => clearInterval(timer);
  }, []);

  function formatRemainingTime(endTimeStr) {
    const end = new Date(endTimeStr);
    const diff = end.getTime() - now.getTime();
  
    if (diff <= 0) 
      return "Ended Event!";
  
    const totalMinutes = Math.floor(diff / (1000 * 60));
    const days = Math.floor(totalMinutes / (60 * 24));
    const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
    const minutes = totalMinutes % 60;
  
    if (days > 0) return `${days} day${days > 1 ? "s" : ""} ${hours} hour${hours > 1 ? "s" : ""} left`;
    if (hours > 0) return `${hours} hour${hours > 1 ? "s" : ""} ${minutes} minute${minutes > 1 ? "s" : ""} left`;
    return `${minutes} minute${minutes > 1 ? "s" : ""} left`;
  }

  return (
    <div className={styles.container}>
      {datas?.map((item, idx) => (
        <div
          key={item.key}
          className={
            `${styles.extractorBoxBottom} font-b3-b cursor-pointer ` +
            (selectedIndex === item.key ? "bg-red-400 text-white" : "bg-gray-600 text-gray-300")
          }
          onClick={() => setSelectedIndex(item.key)}
        >
          <div className={selectedIndex === item.key ? styles.eventName_select : styles.eventName}>{item.event_name}</div>
          <div className={`text-sm mt-1 ${selectedIndex === item.key ? "text-white" : "text-gray-300"}`}>
          {formatRemainingTime(item.period[1])}
          </div>
        </div>
      ))}
    </div>

  );
}
