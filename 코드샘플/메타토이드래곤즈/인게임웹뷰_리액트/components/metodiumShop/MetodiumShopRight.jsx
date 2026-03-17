import styles from "../../styles/MetodiumShopRight.module.css";
import MetodiumShopRightTopFilter from "./MetodiumShopRightTopFilter";
import MetodiumShopRightBottomListData from "./MetodiumShopRightBottomListData";
import { useDispatch, useSelector } from "react-redux";
import { useEffect } from "react";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { fetchShopData } from "../../data/shopSlice";

export default function MetodiumShopRight() {
  const { userInfo, isLoggedin } = useDAppState();
  const dispatch = useDispatch();
  const data = useSelector((state) => state.user);

  useEffect(() => {
    //console.log('useEffect :shop');
    dispatch(fetchShopData());
  }, [dispatch, isLoggedin, userInfo]);
  return (
    <div className={styles.container}>
      <MetodiumShopRightTopFilter data={data} />
      <MetodiumShopRightBottomListData data={data} />
    </div>
  );
}
