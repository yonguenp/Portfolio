const Magnite = (props) => {
  const magnite = parseInt(props.magnite).toLocaleString();
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
          src={process.env.REACT_APP_TEST_PATH + "/images/icon/magnite.png"}
          alt="magnite"
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
          {magnite}
        </div>
      </div>
    </>
  );
};

export default Magnite;
