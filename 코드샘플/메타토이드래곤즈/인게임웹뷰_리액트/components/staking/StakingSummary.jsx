import styles from '../../styles/StakingSummary.module.css';

export default function StakingSummary() {
    return (
        <div className={styles.container}>
            <div className={styles.mySummarys}>
                <div className={styles.mySummary}>
                    <div className={styles.summaryTitle}>나의 스테이킹 NFT 내역</div>
                    <hr className={styles.summaryHr}/>
                    <div className={styles.summaryThead}>
                        <div>Grade</div>
                        <div>채굴량</div>
                        <div>보유량</div>
                        <div>총 채굴량</div>
                    </div>
                    <div className={styles.summaryThead}>
                        <div>Legendary</div>
                        <div>1.25/6H</div>
                        <div>100</div>
                        <div>20.5/6H</div>
                    </div>
                    <div className={styles.summaryThead}>
                        <div>Unique</div>
                        <div>1.25/6H</div>
                        <div>100</div>
                        <div>20.5/6H</div>
                    </div>
                    <div className={styles.summaryThead}>
                        <div>Rare</div>
                        <div>1.25/6H</div>
                        <div>100</div>
                        <div>20.5/6H</div>
                    </div>
                    <div className={styles.summaryThead}>
                        <div>Uncommon</div>
                        <div>1.25/6H</div>
                        <div>100</div>
                        <div>20.5/6H</div>
                    </div>
                    <div className={styles.summaryThead}>
                        <div>Common</div>
                        <div>1.25/6H</div>
                        <div>100</div>
                        <div>20.5/6H</div>
                    </div>
                </div>

                <div className={styles.totalMining}>
                    <div className={styles.totalMiningTitle}>현재 예상 총 채굴량</div>
                    <hr className={styles.summaryHr}/>
                    <div className={styles.totalMiningValue}>24219 $METOD</div>
                </div>
            </div>            

            <div className={styles.claimButtonBox}>
                <div className={styles.claimButton}>
                    <div className={styles.claimButtonInner}>
                        <span>04:03:11 후 클레임 가능</span>
                    </div>                
                </div>
            </div>

            {/* <div className={styles.claimButtonBox}>
                <div className={styles.claimButton2}>
                    <span>04:03:11 후 클레임 가능</span>
                </div>                 
            </div> */}
        </div>
    )
}