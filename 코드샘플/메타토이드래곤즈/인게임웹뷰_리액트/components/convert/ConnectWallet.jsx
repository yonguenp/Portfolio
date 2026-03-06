import styles from "../../pages/convert/index.module.css";
export default function ExportConnectWallet() {
  return (
    <div className={styles.container}>
      <h1 className={styles.title}>Connect PLAY Wallet</h1>
      <div className="font-b4-r gray-500">
        <p>Don't have a PLAY Wallet account yet?</p>
        <p>You can create an account right now.</p>
      </div>
      <div className={styles.buttonBox}>
        <button
          className={`${styles.wemixBtn} white`}
          onClick={() => {
            window.DApp.request("connect");
            // window.DApp.call(
            //   JSON.stringify({
            //     key: "connect",
            //     param: "0xbc455519ea1ba9f9f8165afe90c1b1c95bdfac4c",
            //     //0x607137c376ade7653e263554bce011893788e57e
            //   })
            // );
          }}
        >
          {/* <PlayWallet fill={"#FFFFFF"} /> */}
          Connect Wallet
        </button>
        <button
          className={`${styles.grayBtn} white `}
          onClick={() => {
            window.DApp.request("close");
          }}
        >
          Cancel
        </button>
      </div>
    </div>
  );
}
