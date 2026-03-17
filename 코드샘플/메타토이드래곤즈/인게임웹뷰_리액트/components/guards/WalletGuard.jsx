import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import ConnectWallet from "../common/ConnectWallet";

export default function WalletGuard({ children, forced = false }) {
  const { userInfo } = useDAppState();
  console.log(userInfo.addr, forced);
  if (forced) {
    return (
      <>
        {!userInfo.addr && <ConnectWallet />}
        {children}
      </>
    );
  } else {
    return <>{userInfo.addr ? children : <ConnectWallet />}</>;
  }
}
