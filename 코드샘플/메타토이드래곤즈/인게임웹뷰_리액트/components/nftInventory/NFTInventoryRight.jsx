import { useSelector } from "react-redux";
import styles from "../../styles/NFTInventoryRight.module.css";
import NFTInventoryRightTopFilter from "./NFTInventoryRightTopFilter";
import NFTInventoryRightBottomListData from "./NFTInventoryRightBottomListData";

export default function NFTInventoryRight(props) {
  return (
    <div className={styles.container}>
      <NFTInventoryRightTopFilter />
      <NFTInventoryRightBottomListData />
    </div>
  );
}
