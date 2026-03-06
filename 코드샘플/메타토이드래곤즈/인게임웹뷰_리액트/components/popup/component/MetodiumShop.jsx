import React, { useEffect, useState } from "react";
import styles from "../../../styles/MetodiumShopPopup.module.css";
import { useDAppState } from "../../../DApp/src/providers/DAppProvider";

export default function MetodiumShop(props) {
  const { popupData, closePopup } = props;

  const [title, setTitle] = useState("Purchase");
  const [popupType, setPopupType] = useState("");
  const [goldBoxChoose, setGoldBoxChoose] = useState(null);
  const { userInfo, setUserInfo } = useDAppState();
  //지금은 임시로 필터로 잡았지만 해당 item 에 대한 id 및 type 등등 다 받아와야함

  const buy = async (good_id, amount) => {
    //closePopup();
    
    const data = await window.DApp.post("shop/buy", {        
      server_tag : sessionStorage.getItem("server_tag"),
      goods_id : good_id,
      count : amount
    });
    
    // console.log('buy : ', data);

    if (data && data.rs == 0)
    {
      window.DApp.request("mail_check");
      window.DApp.emit("dapp.popup", {
        err: 0,
        title: "Success",
        msg: "Your purchase has been completed.",
      });
      
      setUserInfo(prev => ({
        ...prev,
        magnet: data.magnet,
        magnite: data.magnite,
      }));
    }
    else if(!data || !data.msg)
    {
      window.DApp.emit("dapp.popup", {
        err: 1,
        title: "Error",
        msg: "An error occurred while purchasing the product.",
      });
    }
  }

  useEffect(() => {
    if (popupData != null) {
      console.log(popupData.type);
      if (popupData.type === 3) {
        setPopupType("goldBox");
        setTitle("Use Goldbox");
      } else if (popupData.type === 1) {
        setPopupType("gemBlock");
        setTitle("Use Meta Toy Squad");
      } else if (popupData.type === 5) {
        setPopupType("figure");
        setTitle("Use Meta Figure");
      } else if (popupData.type === 9) {
        setPopupType("goldBox2");
        setTitle("Use Goldbox");
      }
    }
    console.log(popupData);
  }, [popupData]);

  return (
    <>
      <div className="bg"></div>
      <div className="container">
        <div className="title">{popupData.data.goods_title}</div>
        <>
          <div className="content">
            <div className="shopImgBox">
              <img src={popupData.data.goods_image} alt="default" />
            </div>

            <div className={styles.iconTextWrapper}>
              <img src={
                          process.env.REACT_APP_TEST_PATH +
                          "/images/icon/magnite.png"
                        } 
                        alt="metod" style={{ width: "20px" }} className="icon" />
              <span className={styles.iconText}>{(popupData.data.price).toLocaleString()}</span>
            </div>
          </div>
          <div className="shopInfo font-b3-r text-center">
            After purchasing the package,
            <br />
            refunds or exchanges are not available.
          </div>
        </>

        <div className="popupButtons font-b3-b">
          <div className="cancelBtn" onClick={closePopup}>
            Cancel
          </div>
          <div
            className="signBtn"
            onClick={() =>
              buy(popupData.data.goods_id, 1)
            }
          >            
            Buy
          </div>
        </div>
      </div>
    </>
  );
}
