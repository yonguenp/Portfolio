import styles from "../../styles/MetodiumShopLeft.module.css";
import { useSelector } from "react-redux";


import MetodiumShopLeftDetail from "./MetodiumShopLeftDetail";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
export default function MetodiumShopLeft(props) {
  const { userInfo } = useDAppState();

  return (
    <div className={styles.container}>
        <MetodiumShopLeftDetail/>
    </div>
  );
}
