import styles from "../../pages/convert/index.module.css";
import { ReactComponent as WalletChange } from "../../assets/svg/header/wallet_change.svg";
import { useEffect, useState } from "react";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
export default function ExportConvertItem({ addr, dir, data }) {
  const [gem, setGem] = useState(null);
  const ITEM_ID_STR = {
    180000001: "Standard Skill Cube",
    180000002: "Advanced Skill Cube",
  };
  const { userInfo } = useDAppState();
  const find_my_gemblock = async (gem_id) => {
    const respdata = await window.DApp.post(`export/export_get_gemblock`, {
      server_tag : sessionStorage.getItem("server_tag")
    });
    if (respdata.nfts) {
      const [myGem] = respdata.nfts.filter((v) => Number(v.tag) == Number(gem_id));
      setGem(myGem);
      console.log(myGem);
      console.log("내 쨈");
    }
    console.log(data);
  };
  useEffect(() => {
    if (!data) return;
    if (!userInfo.user) return;
    if (dir == "gem_convert") find_my_gemblock(data);
  }, [data, dir, userInfo]);
  return (
    <div className={styles.container}>
      <div className={styles.topBar}>
        <h1 className={styles.title}>Convert</h1>
        <button
          className="headerRightButton font-b3-b white"
          onClick={() => {}}
        >
          <WalletChange fill="white" />{" "}
          {addr?.replace(/(.{6}).*(.{4})+/, "$1...$2")}
        </button>
      </div>

      <div className={styles.itemBox}>
        <img
          alt="item_img"
          className={styles.item_img}
          src={
            dir == "item_convert"
              ? `https://mtw-assets.s3.ap-northeast-2.amazonaws.com/passive/${data}.png`
              : gem
              ? `https://mtw-assets.s3.ap-northeast-2.amazonaws.com/gemblock/${gem?.id}.png`
              : "Loading"
          }
        />
        <span className={styles.item_name}>
          {dir == "item_convert"
            ? ITEM_ID_STR[data]
            : `[${gem?.grade}] ${gem?.name} +${gem?.level}`}
        </span>
      </div>
      <span>Item will be transformed into NFT</span>
      <div className={styles.buttonBoxRow}>
        <button
          className={`${styles.grayBtn} white `}
          onClick={() => {
            window.DApp.request("close");
          }}
        >
          Cancel
        </button>
        <button
          className={`${styles.wemixBtn} white`}
          onClick={() => {
            window.DApp.genrTx(`export.${dir}`, {
              item_id: data,
            });
          }}
        >
          {/* <PlayWallet fill={"#FFFFFF"} /> */}
          Sign
        </button>
      </div>
    </div>
  );
}
