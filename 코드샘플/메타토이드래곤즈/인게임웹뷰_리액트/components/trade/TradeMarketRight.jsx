// TradeRightContent.jsx
import { useEffect, useMemo, useState, useCallback } from "react";
import { dummyItems, myItems, myHistory } from "./dummyItems";
import ItemGrid from "./ItemGrid";
import TradeFilterBar from "./TradeFilterBar";
import TradeModal from "./TradeModal";
import TradeFilterModal from "./TradeFilterModal";
import { STAT_OPTIONS, CATEGORY_OPTIONS, GRADE_OPTIONS, TYPE_OPTIONS } from "./StatOptions";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";
import { TAB } from "./Tabs";
import styles from "../../styles/Trade.module.css";
import { usePopup } from "../../context/PopupContext";

export default function TradeRightContent({ tab }) {
  const { isLoggedIn, setUserInfo } = useDAppState();
  const { openPopup } = usePopup();
  const [sortType, setSortType] = useState(6);
  const [categoryFilters, setCategoryFilters] = useState([]);
  const [statFilters, setStatFilters] = useState([]); // ← 배열
  const [gradeFilters, setGradeFilters] = useState([]); // ← 배열
  const [typeFilters, setTypeFilters] = useState([]); // ← 배열
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(0);
  const [loading, setLoading] = useState(false);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);

      switch (tab) {
        case TAB.GEM_MARKET:
          setItems(await window.DApp.post(`trade/list`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
            sort: sortType,
            filter_category: JSON.stringify(categoryFilters),
            filter_grade: JSON.stringify(gradeFilters),
            filter_stat: JSON.stringify(statFilters),
            page: page,
          }));
          break;
        case TAB.GEM_SELLING:
          setItems(await window.DApp.post(`trade/mylist`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
          }));
          break;
        case TAB.MY_GEMS:
          setItems(await window.DApp.post(`trade/myitem`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
          }));
          break;
        case TAB.GEM_HISTORY:
          const gemhistory = await window.DApp.post(`trade/history`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
          });
          setItems(gemhistory);

          gemhistory.push?.forEach((v) => {
              if(v.api === "magnite_update")
              {
                setUserInfo(prev => ({
                  ...prev,
                  magnite: v.magnite,
                }));
              }
          });           
          break;

        case TAB.ITEM_MARKET:
          setItems(await window.DApp.post(`trade/list`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
            sort: 1,
            filter_grade: JSON.stringify(typeFilters),
            page: page,
          }));
          break;
        case TAB.ITEM_SELLING:
          setItems(await window.DApp.post(`trade/mylist`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
          }));
          break;
        case TAB.MY_ITEMS:
          setItems(await window.DApp.post(`trade/myitem`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
          }));
          break;
        case TAB.ITEM_HISTORY:
          const itemhistory = await window.DApp.post(`trade/history`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
          });
          setItems(itemhistory);

          itemhistory.push?.forEach((v) => {
              if(v.api === "magnite_update")
              {
                setUserInfo(prev => ({
                  ...prev,
                  magnite: v.magnite,
                }));
              }
          });           
          break;
      }
    } catch (err) {
      console.error("Trade PAGE ERROR ", err);
    } finally {
      setLoading(false);
    }
  }, [tab, sortType, categoryFilters, statFilters, gradeFilters, typeFilters, page]);

  const errormsg = (rs) => {
    console.log(rs);
    switch(rs)
    {
      case 2901: return"Purchase failed. Please try again later. (Error: 2901)";
      case 2902: return"You don’t have enough quantity. Please check again.";
      case 2903: return"Failed to list the item for sale. Please check the item again. (Error: 2903)";
      case 2904: return"Failed to cancel the listing. Please try again later. (Error: 2904)";
      case 2905: return"Failed to cancel the listing. Please try again later. (Error: 2905)";
      case 2906: return"This item has already been sold.";
      case 2907: return"You don’t have enough items. Please check again.";
      case 2908: return"Purchase failed. Please try again later. (Error: 2908)";
      case 2909: return"Purchase failed. Please try again later. (Error: 2909)"; 
    }
    return null;
  }

  const confirmItem = useCallback(async (type, item) => {
    try {
      console.log(item);
      setLoading(true);

      switch (tab) {
        case TAB.GEM_MARKET:
          const gem_buy = (await window.DApp.post(`trade/buy`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
            tm_idx: item.tm_idx,
            amount: 1,
          }));
          
          if(gem_buy.rs == 0)
          {
            openPopup({ type: "MessagePopup", title: "Success", msg: "Your purchase was successful. Please check your DApp inventory.", isRefresher: false });

            gem_buy.push?.forEach((v) => {
              if(v.api === "magnite_update")
              {
                setUserInfo(prev => ({
                  ...prev,
                  magnite: v.magnite,
                }));
              }
            });            
          }
          else
          {
            openPopup({ type: "MessagePopup", title: "Error", msg: errormsg(gem_buy.rs) ?? "An error occurred during the purchase. Please try again later.", isRefresher: false });
          }
          break;
        case TAB.GEM_SELLING:
          const gem_cancel = (await window.DApp.post(`trade/unregist`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
            tm_idx: item.tm_idx,
          }));

          if(gem_cancel.rs == 0)
            openPopup({ type: "MessagePopup", title: "Success", msg: "The listing for this item has been cancelled.", isRefresher: false });
          else
            openPopup({ type: "MessagePopup", title: "Error", msg: errormsg(gem_cancel.rs) ?? "An error occurred while cancelling the listing. Please try again later.", isRefresher: false });
          break;
        case TAB.MY_GEMS:
          const gem_regist = (await window.DApp.post(`trade/regist`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 1,
            no: item.iv_idx,
            price: item.price,
            qty: 1,
          }));

          if(gem_regist.rs == 0)
            openPopup({ type: "MessagePopup", title: "Success", msg: "The item has been listed on the market.", isRefresher: false });
          else
            openPopup({ type: "MessagePopup", title: "Error", msg: errormsg(gem_regist.rs) ?? "Failed to list the item on the market. Please try again later.", isRefresher: false });
          break;

        case TAB.ITEM_MARKET:
          const item_buy = (await window.DApp.post(`trade/buy`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
            tm_idx: item.tm_idx,
            amount: item.count,
          }));

          if(item_buy.rs == 0)
          {
            openPopup({ type: "MessagePopup", title: "Success", msg: "Your purchase was successful. Please check your DApp inventory.", isRefresher: false });
            item_buy.push?.forEach((v) => {
              if(v.api === "magnite_update")
              {
                setUserInfo(prev => ({
                  ...prev,
                  magnite: v.magnite,
                }));
              }
            });        
          }
          else
            openPopup({ type: "MessagePopup", title: "Error", msg: errormsg(item_buy.rs) ?? "An error occurred during the purchase. Please try again later.", isRefresher: false });
          break;
        case TAB.ITEM_SELLING:
          const item_cancel = (await window.DApp.post(`trade/unregist`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
            tm_idx: item.tm_idx,
          }));

          if(item_cancel.rs == 0)
            openPopup({ type: "MessagePopup", title: "Success", msg: "The listing for this item has been cancelled.", isRefresher: false });
          else
            openPopup({ type: "MessagePopup", title: "Error", msg: errormsg(item_cancel.rs) ?? "An error occurred while cancelling the listing. Please try again later.", isRefresher: false });
          break;
        case TAB.MY_ITEMS:
          console.log(item);
          const item_regist = (await window.DApp.post(`trade/regist`, {
            server_tag: sessionStorage.getItem("server_tag"),
            type: 2,
            no: item.item_no,
            price: item.price,
            qty: item.count,
          }));
          if(item_regist.rs == 0)
            openPopup({ type: "MessagePopup", title: "Success", msg: "The item has been listed on the market.", isRefresher: false });
          else
            openPopup({ type: "MessagePopup", title: "Error", msg: errormsg(item_regist.rs) ?? "Failed to list the item on the market. Please try again later.", isRefresher: false });
          break;
      }
    } catch (err) {
      console.error("Trade PAGE ERROR ", err);
    } finally {
      setLoading(false);
      fetchData();
    }
  }, [tab]);

  useEffect(() => {
    if (categoryFilters.length == 0)
      setCategoryFilters(CATEGORY_OPTIONS.map(c => c.key));
    if (gradeFilters.length == 0)
      setGradeFilters(GRADE_OPTIONS.map(g => g.key));
    if (statFilters.length == 0)
      setStatFilters(STAT_OPTIONS.map(s => s.key));
    if (typeFilters.length == 0)
      setTypeFilters(TYPE_OPTIONS.map(s => s.key));

    if (isLoggedIn)
      fetchData();
  }, [isLoggedIn, tab, sortType, categoryFilters, statFilters, gradeFilters, typeFilters, page]);

  useEffect(()=>{
    // window.DApp.emit("dapp.loading", {
    //         isLoader: loading,
    //         msg: "Wait...",
    // });

  }, [loading]);

  const { userInfo } = useDAppState();

  const [modal, setModal] = useState({
    open: false,
    type: null, // "buy" | "sell" | "cancel"
    item: null,
  });

  const handleAction = (item) => {
    if (tab === TAB.MY_GEMS) {
      setModal({ open: true, type: "gem_sell", item });
    }
    if (tab === TAB.MY_ITEMS) {
      setModal({ open: true, type: "item_sell", item });
    }
    if (tab === TAB.GEM_MARKET) {
      setModal({ open: true, type: "gem_buy", item });
    }
    if (tab === TAB.ITEM_MARKET) {
      setModal({ open: true, type: "item_buy", item });
    }
    if (tab === TAB.GEM_SELLING || tab === TAB.ITEM_SELLING) {
      setModal({ open: true, type: "cancel", item });
    }
  };

  const handlePage = (idx) => {
    setPage(idx);
  };

  const filteredItems = useMemo(() => {
    let filtered = [];
    if (tab === TAB.MY_GEMS)
      filtered = items.my_item_list ? Object.values(items.my_item_list).map(item => ({
        ...item,
        name : item.item_desc.name,
        image : item.item_desc.image,
        stats : item.item_desc.stats,
        reinforce : item.item_desc.reinforce
      })) : [];
    else if (tab === TAB.MY_ITEMS) {
      filtered = items.my_item_list ? Object.values(items.my_item_list).map(item => ({
        ...item,
        name: item.item_desc?.Category == "Advanced" ? "Advanced Skill Cube" : "Standard Skill Cube",
        image: item.item_desc?.image,
        stats: item.item_desc?.stats,
        reg_qty: item.qty
      })) : [];
    }
    else if (tab === TAB.GEM_MARKET) {
      filtered = items.trade_list ? Object.values(items.trade_list) : [];
    }
    else if (tab === TAB.ITEM_MARKET) {
      filtered = items.trade_list ? Object.values(items.trade_list).map(item => ({
        ...item,
        name: item.grade == "Advanced" ? "Advanced Skill Cube" : "Standard Skill Cube",        
      })) : [];
    }
    else if (tab === TAB.GEM_SELLING){
      filtered = items.my_sales_list ? Object.values(items.my_sales_list) : [];
    }
    else if (tab === TAB.ITEM_SELLING){
      filtered = items.my_sales_list ? Object.values(items.my_sales_list).map(item => ({
        ...item,
        name: item.grade == "Advanced" ? "Advanced Skill Cube" : "Standard Skill Cube",        
      })) : [];
    }
    else if (tab === TAB.GEM_HISTORY)
      filtered = items.sold_item_list ? Object.values(items.sold_item_list) : [];
    else if (tab === TAB.ITEM_HISTORY) {
      filtered = items.sold_item_list ? Object.values(items.sold_item_list).map(item => ({
        ...item,
        name: item.grade == "Advanced" ? "Advanced Skill Cube" : "Standard Skill Cube",               
      })) : [];
    }
    return filtered;    
  }, [
    items
  ]);



  return (
    <>
      {/* 히스토리 탭이 아닐 때만 필터 + 그리드 */}
      {(tab === TAB.GEM_MARKET || tab === TAB.ITEM_MARKET) && (
        <>
          <TradeFilterBar
            tab={tab}
            sortType={sortType}
            setSortType={setSortType}
            categoryFilters={categoryFilters}
            statFilters={statFilters}
            gradeFilters={gradeFilters}
            typeFilters={typeFilters}
            onOpenFilter={() => setIsFilterOpen(true)}
          />
        </>
      )}
      {
        isFilterOpen && (
          <TradeFilterModal
            categoryFilters_origin={categoryFilters}
            setCategoryFilters_origin={setCategoryFilters}
            statFilters_origin={statFilters}
            setStatFilters_origin={setStatFilters}
            gradeFilters_origin={gradeFilters}
            setGradeFilters_origin={setGradeFilters}
            typeFilters_origin={typeFilters}
            setTypeFilters_origin={setTypeFilters}
            tab={tab}
            onClose={() => setIsFilterOpen(false)}
          />
        )
      }
      <div className={styles.gridScrollArea}>
        <ItemGrid
          items={filteredItems}
          mode={tab}
          onAction={handleAction}
          onPage={handlePage}
          page={page}
          maxPage={items?.max_page ?? 0}
        />
      </div>


      {/* ✅ 모달은 항상 렌더 */}
      <TradeModal
        modal={modal}
        onClose={() =>
          setModal({ open: false, type: null, item: null })
        }
        onConfirm={(type, item, count) => {
          confirmItem(type, item, count);


          setModal({ open: false, type: null, item: null });
        }}
      />
    </>
  );
}
