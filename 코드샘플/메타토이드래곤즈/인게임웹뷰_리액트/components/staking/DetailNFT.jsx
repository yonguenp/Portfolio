import styles from '../../styles/DetailNFT.module.css';

export default function DetailNFT() {
    return (
        <div className={styles.container}>
            <div className={styles.imageContainer}>
                <div className={styles.imageWrapper}>
                    <img src={process.env.REACT_APP_TEST_PATH+"/images/NFT/sample.png"} alt=""/>
                </div>
            </div>
            <div className={styles.infoContainer}>
                <div className={styles.infoBox}>
                    <span className={styles.infoLabel}>Num</span>
                    <span className={styles.infoValue}>#9999</span>
                </div>
                <div className={styles.infoBox}>
                    <span className={styles.infoLabel}>Epoch</span>
                    <span className={styles.infoValue}>12D 10:31:55</span>
                </div>
                <div className={styles.infoBox}>
                    <span className={styles.infoLabel}>Rewards</span>
                    <span className={styles.infoValue}>1.25/6H</span>
                </div>
            </div>
        </div>

    )
}