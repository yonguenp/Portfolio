import { useSelector } from "react-redux";

const StatPoint = (props) => {
  const {swapInfo} = useSelector((state) => state.artblock);

  const StatPoint = swapInfo?.total_point?swapInfo.total_point.toLocaleString() : 0;
  //.toLocaleString();

  return (
    <>
      <div
        style={{
          display: "flex",
          padding: "2px 10px",
          borderRadius: "25px",
          border: "1px",
          background: "#3A3838",
        }}
      >
        <img
          src={process.env.REACT_APP_TEST_PATH + "/images/icon/buff_point.png"}
          alt="StatPoint"
          style={{ width: "24px", height: "24px" }}
        />
        <div
          style={{
            display: "flex",
            alignItems: "center",
            marginLeft: "4px",
            marginRight: "3px",
            color: "white",
            fontFamily: "Roboto",
          }}
        >
          {StatPoint}
        </div>
      </div>
    </>
  );
};

export default StatPoint;
