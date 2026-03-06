import styles from "../../styles/StakingRightBottomListData.module.css";
import { useSelector, useDispatch } from "react-redux";
import { setIsActive } from "../../data/stakingSlice";
import WaitingNFTCard from "./card/WaitingNFTCard";
import { ReactComponent as Locked } from "../../assets/svg/staking/lock.svg";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import NFTCard from "./card/NFTCard";
import { useRef, useEffect, useState } from "react";

// ======================= 1. 여기에 더미 데이터 붙여넣기 =======================
const dummyStakingData = [
    // 1. 스테이킹 안 된 아이템 (Uninserted)
    { iv_id: 101, name: 'Uninserted NFT #1', image: 'https://sandboxnetwork.mypinata.cloud/ipfs/QmVHYYbnjqcCBDSdbFDtGdNxQsn8XsYaY4aPKgHf9hFtjG', tokenId: 1001, grade: 'Normal', deposit_at: null, withdrawable: false, selectable: true, isInserted: 0, state: 2 },
    // 2. 스테이킹 중인 아이템 (Inserted/Running)
    { iv_id: 201, name: 'Inserted NFT #1', image: 'https://sandboxnetwork.mypinata.cloud/ipfs/QmVHYYbnjqcCBDSdbFDtGdNxQsn8XsYaY4aPKgHf9hFtjG', tokenId: 2001, grade: 'Rare', deposit_at: '2025-10-16 14:30:00', withdrawable: false, selectable: false, isInserted: 1, state: 0 },
    // 3. 스테이킹 대기 중인 아이템 (Waiting)
    { iv_id: 301, name: 'Waiting NFT #1', image: 'https://sandboxnetwork.mypinata.cloud/ipfs/QmVHYYbnjqcCBDSdbFDtGdNxQsn8XsYaY4aPKgHf9hFtjG', tokenId: 3001, grade: 'Super Rare', deposit_at: '2025-10-16 16:00:00', selectable: false, isInserted: 1, state: 1 },
    // 4. 스테이킹 안 된 아이템 2
    { iv_id: 102, name: 'Uninserted NFT #2', image: 'https://sandboxnetwork.mypinata.cloud/ipfs/QmVHYYbnjqcCBDSdbFDtGdNxQsn8XsYaY4aPKgHf9hFtjG', tokenId: 1002, grade: 'Normal', deposit_at: null, withdrawable: false, selectable: true, isInserted: 0, state: 2 },
    // 5. 스테이킹 중인 아이템 2
    { iv_id: 202, name: 'Inserted NFT #2', image: 'https://sandboxnetwork.mypinata.cloud/ipfs/QmVHYYbnjqcCBDSdbFDtGdNxQsn8XsYaY4aPKgHf9hFtjG', tokenId: 2002, grade: 'Ultra Rare', deposit_at: '2025-10-15 11:00:00', withdrawable: false, selectable: false, isInserted: 1, state: 0 },
];
// ========================================================================

export default function StakingRightBottomListData(props) {
  const dispatch = useDispatch();
  const ref = useRef(null);
  const { userInfo } = useDAppState();

    // ======================= 2. 이 부분을 수정 =======================
    const listDataArray = useSelector((state) => state.staking.stakingData); // 기존 코드 주석 처리

    console.log("Redux에서 받은 실제 데이터:", listDataArray);

    // const listDataArray = dummyStakingData; // 임시로 더미 데이터 사용
    // =============================================================

  const [gap, setGap] = useState(0);
  const filterMenu = useSelector((state) => state.staking.filterMenu);

  return (
    <>
      {listDataArray.length > 0 ? (
        <div
          className={styles.listBottom}
          ref={ref}
        >
          {userInfo?.user_no ? (
            listDataArray
              .filter((item) => filterMenu == item.isInserted)
              .map((item, index) => {
                if (item.state == 0 || item.state == 2) {
                  return <NFTCard key={index} item={item} />;
                } else if (item.state == 1) {
                  return <WaitingNFTCard key={index} item={item} />;
                }
              })
          ) : (
            <div className={`${styles.listBottomNoData} font-b3-r gray-800`}>
              Please connect wallet.
            </div>
          )}
        </div>
      ) : (
        <div className={`${styles.listBottomNoData} font-b3-r gray-800`}>
          No information to display.
        </div>
      )}
    </>
  );
}
