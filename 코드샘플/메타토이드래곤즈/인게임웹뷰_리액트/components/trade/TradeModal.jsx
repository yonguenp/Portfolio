import ItemCard from "./ItemCard";
import { useState, useEffect } from "react";
import styles from "../../styles/Trade.module.css";
import { useDAppState } from "../../DApp/src/providers/DAppProvider";

export default function TradeModal({ modal, onClose, onConfirm }) {
  const { type, item } = modal;
  const [price, setPrice] = useState(item?.price ?? 0);
  
  const [count, setCount] = useState(0);
  const [unitPrice, setUnitPrice] = useState(0);
  const [totalPrice, setTotalPrice] = useState(0);
   const { userInfo } = useDAppState();

  const onValueChange = (setFn, value) => {
    value = Number(value);
    if(value > 0)
      setFn(value);
    else
      setFn(0);
  };

  useEffect(() => {
    if (!item) return;
    
    setTotalPrice(unitPrice * count);
    setPrice(totalPrice);

  }, [unitPrice, count]);

  useEffect(() => {
    if (!item) return;
    
    setPrice(0);
    setCount(0);
    setUnitPrice(0);
    setTotalPrice(0);

    if(type === "gem_buy")
    {
      setPrice(item.price);
      setTotalPrice(item.price);
    }

    if(type === "item_buy")
    {
      setUnitPrice(item.price);
    }
  }, [item]);

  if (!modal.open || !item) return null;

  const titleMap = {
    buy: "Buy",
    sell: "Sell",
    cancel: "Cancel",
  };
  console.log(item, price, count);
  return (
    <div className={styles.overlay}>
      <div className={styles.modal}>
        <h3>{titleMap[type]}</h3>

        <div className={styles.modalItem}>
          <ItemCard item={item} compact={true} />
        </div>

        {type === "buy" && (
          <div className={styles.priceBox}>
            Price <strong>{item.price}</strong>
          </div>
        )}

        {type === "gem_sell" && (
          <div className={styles.priceBox}>
            <input
              type="number"
              value={Number(price)}
              onChange={(e) => onValueChange(setPrice,e.target.value)}
              placeholder="Sell Price"
              className={styles.input}
            />
            <span className={styles.unit}>MN</span>
          </div>
        )}

        {type === "item_buy" && (
          <>
          <div className={styles.priceBox}>
            <span className={styles.unit}>Buy Count</span>
            <input
              type="number"
              value={Number(count)}
              onChange={(e) => onValueChange(setCount,e.target.value)}
              placeholder="Buy Count"
              className={styles.input}
            />         
          </div>
          
          <div className={styles.price}>
            <img
              src="/images/icon/magnite.png"
              alt="magnite"
              className={styles.priceIcon}
            />
            <span className={styles.priceValue}>
              <strong>{totalPrice.toLocaleString()}</strong>
            </span>
          </div>
          </>
        )}

        {type === "gem_buy" && (          
          <div className={styles.price}>
            <img
              src="/images/icon/magnite.png"
              alt="magnite"
              className={styles.priceIcon}
            />
            <span className={styles.priceValue}>
              <strong>{price.toLocaleString()}</strong>
            </span>
          </div>
        )}

        {type === "item_sell" && (
          <>
          <div className={styles.priceBox}>
            <span className={styles.unit}>Sell Count</span>
            <input
              type="number"
              value={Number(count)}
              onChange={(e) => onValueChange(setCount, Number(e.target.value))}
              placeholder="Sell Count"
              className={styles.input}
            />         
          </div>
          <div className={styles.priceBox}>
            <span className={styles.unit}>Price Each</span>
            <input
              type="number"
              value={Number(unitPrice)}
              onChange={(e) => onValueChange(setUnitPrice, Number(e.target.value))}
              placeholder="Price Each"
              className={styles.input}
            />         
          </div>

          <div className={styles.price}>
            <img
              src="/images/icon/magnite.png"
              alt="magnite"
              className={styles.priceIcon}
            />
            <span className={styles.priceValue}>
              <strong>{totalPrice.toLocaleString()}</strong>
            </span>
          </div>
          </>
        )}

        {type === "cancel" && (
          <p className={styles.warning}>
            Are you sure you want to cancel this listing?
          </p>
        )}

        {(type === "gem_sell" || type === "item_sell") && (
          <p className={styles.warning}>
            Are you sure you want to sell this item at the listed price?
          </p>
        )}

        {(type === "gem_buy" || type === "item_buy") && (
          <p className={styles.warning}>
            Are you sure you want to buy this at this price?
          </p>
        )}

        <div className={styles.actions}>
          <button className={styles.cancel} onClick={onClose}>
            Cancel
          </button>
          <button
            className={styles.confirm}
            disabled={ type === "gem_sell" ? price === 0 : (type === "item_sell" ? (unitPrice === 0 || count === 0) : (type === "cancel" ? false : (type === "item_buy" ? (count === 0 || totalPrice > userInfo?.magnite) : (price === 0 || price > userInfo?.magnite)))) }
            onClick={() => onConfirm(type, { ...item, price: type === "item_sell" ? unitPrice : price, count: count })}
          >
            Confirm
          </button>
        </div>
      </div>
    </div>
  );
}
