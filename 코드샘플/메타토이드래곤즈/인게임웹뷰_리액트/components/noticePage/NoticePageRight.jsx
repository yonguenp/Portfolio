import { useState, useMemo , useEffect, useRef  } from 'react';
import styles from "../../styles/NoticePageRight.module.css";
import { usePopup } from "../../context/PopupContext";

export default function NoticePageRight({ data , selectedIndex }) {
  const { openPopup } = usePopup();
  const scrollRef = useRef(null);
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = 0;
    }
  }, [selectedIndex]);

  if (!data) 
  {
    return <div></div>;
  }
  
  const imageUrl = `https://d1zh71njdecog6.cloudfront.net/banner/announcement/en/${data.image}`;
console.log(imageUrl);
   let parsedMsg = data.msg
    .replace(/\\n/g, '\n')
    .replace(/\^/g, ',')
    .replace(/<color=(#[0-9a-fA-F]{6})>(.*?)<\/color>/g, '<span style="color:$1">$2</span>');


  return (
    <div className={styles.container}>
      <div className={styles.rightContent} ref={scrollRef}>
        <img src={imageUrl} alt={data.title} className={styles.banner} />
        <div
          className={styles.message}
          dangerouslySetInnerHTML={{ __html: parsedMsg.replace(/\n/g, '<br/>') }}
        />
      </div>
    </div>
  );
}

