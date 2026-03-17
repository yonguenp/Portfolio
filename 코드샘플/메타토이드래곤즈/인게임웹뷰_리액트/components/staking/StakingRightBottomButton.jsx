import styles from "../../styles/StakingRightBottomButton.module.css";
import { useSelector, useDispatch } from "react-redux";
import { usePopup } from "../../context/PopupContext";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function StakingRightBottomButton() {
  //아이템 몇개 선택했는지 , 필터는 뭐 선택되있는지 가져와야해
  const { userInfo } = useDAppState();
  const { selectedStakingData, filterMenu, stakingData, stakingStats } =
    useSelector((state) => state.staking);

  //   const { stakingData, stakingStats } = useSelector(
  //   (state) => state.staking,
  //   (l, r) => l.stakingData.length == r.stakingData.length
  // );

  const { openPopup } = usePopup();
  console.log(selectedStakingData);
  return (
    <div className={styles.container}>
      {/* <div className={`${styles.buttonBox} bg-red-400 mb-4`}>
        <div
          className="font-b3-b white"
          onClick={() =>
            openPopup({ type: "GenesisCardActivate", data: "카드데이터" })
          }
        >
          Activate Genesis Card Reward System
        </div>
      </div> */}

      <button
        disabled={selectedStakingData.length < 1}
        className={`${styles.buttonBox} bg-red-400`}
        onClick={async () => {
          if (filterMenu === 0) {
            openPopup({ type: "GenesisInsertCard", data: "카드데이터" });
          } else {
            openPopup({ type: "GenesisWithdrawCard", selected: selectedStakingData});
            
          }
        }}
      >
        {/* {selectedData.filterMenu === '0' ? 'Uninserted' : 'Inserted'} */}
        {/* <PlayWallet style={{ marginTop: "-1px" }} /> */}
        <div className="font-b3-b white">
          {filterMenu === 0 ? "Insert" : "Remove"}
          {selectedStakingData.length > 0 &&
            " " + selectedStakingData.length}{" "}
          Dragon Cards
        </div>
      </button>
    </div>
  );
}
