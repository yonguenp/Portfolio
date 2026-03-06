import styles from "../../styles/MetodiumShopLeft.module.css";

export default function ConnectWallet() {
  return (
    <div className={styles.topInfoBox}>
      <div className={styles.topInfo}>
        <div className={styles.topInfoTitle_noWallet}>
          <div className={`font-b2-b`}>Connect PLAY Wallet</div>
          <div className={`${styles.topInfoTitleBottom_noWallet}`}>
            Don't have a PLAY Wallet account yet?
            <br />
            You can create an account right now.
          </div>
        </div>
      </div>
      <button
        className={`${styles.topInfoButton_noWallet} white`}
        onClick={() => {          
          try {
            window.DApp.connect();  
          } catch (e) {
            // to test
            window.DApp.call(
              JSON.stringify({  
                key: "connect",
                param: "0x354b087c34962775cf6c8a0fc016c7dd93740cf3"
                //"0x282561fcdb18bfba2cac8e48f3c30883e44d3e00",
                //0x607137c376ade7653e263554bce011893788e57e
              })
            );
          }
        }}
      >
        {/* <PlayWallet style={{ marginTop: "-1px" }} /> Connect Wallet */}
      </button>
    </div>
  );
}
