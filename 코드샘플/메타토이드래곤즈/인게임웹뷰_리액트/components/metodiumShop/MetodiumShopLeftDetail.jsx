import styles from "../../styles/MetodiumShopLeftDetail.module.css";
import { useSelector } from "react-redux";

export default function MetodiumShopLeftDetail() {
  const selectedItem = useSelector((state) => state.metodiumShop.selectedItem);
  // api 던져줄때 옵션 제목값도 같이 넘어오면 훨씬 수월해짐, 이건 일단 대기
  // if(selectedData.filter === 1){ // Gem Block
  //     const detailList = ['Item Name', 'Rank', 'Base Stat', 'Optional Stat'];
  // }else if(selectedData.filter === 3){  // GoldBox
  //     const detailList = ['Reward1','Reward2','Reward3'];
  // }else if(selectedData.filter === 4){ // Ticket
  //     const detailList = ['Rank', 'Reward'];
  // }else if(selectedData.filter === 5){
  //     const detailList = ['Level', 'Reward'];
  // }

  return (
    <div
      className={styles.bottomDetailBox}
      // style={{ marginTop: addr ? "1rem" : "0rem" }}
      style={{ marginTop: "1rem" }}
    >
      <div className={styles.bottomDetail}>
        {selectedItem ? (
          <>
            <div className="text-start font-b3-b mb-8">
              {selectedItem.goods_title}
            </div>

            { Object.keys(selectedItem.rewards[0])?.map((v) => {
              const reward = selectedItem.rewards[0][v];
              return (
                <div className={styles.bottomDetailInfo} key={v}>
                  <div>
                    <img
                      src={`https://mtw-assets.s3.ap-northeast-2.amazonaws.com/webview-assets/metod_shop/${reward.icon_image}`}
                      alt="item"
                      style={{ width: "48px" }}
                    />
                  </div>
                  <div>
                    {reward?.item_prefix ? (
                      <>
                        <div className="text-start font-b4-b green-500">
                          {reward.item_prefix}
                        </div>
                        <div className="text-start font-b4-b">
                          {reward.item_name}
                        </div>
                      </>
                    ) : (
                      <div className="text-start font-b4-b">
                        {reward.item_name}
                      </div>
                    )}
                    <div className="text-start font-b4-b gray-500">{`x${reward.amount}`}</div>
                  </div>
                </div>
              );
            })}
          </>
        ) : (
          <>
            <div className={styles.bottomDetailInfo_noData}>
              <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "선택된 상품이 없습니다." : "No Selected item"}</div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
