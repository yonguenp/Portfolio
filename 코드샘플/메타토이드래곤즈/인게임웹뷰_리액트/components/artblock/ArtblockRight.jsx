import styles from '../../styles/ArtblockRight.module.css'
import {ArtBlockOption, ArtBlockOptionLimit} from '../../data/data'
import { usePopup } from '../../context/PopupContext';

export default function ArtblockRight(props) {  

  const { openPopup } = usePopup();

  const {swapInfo} = props;

  //const arkBlockKeys = Object.keys(ArtBlockOptionLimit).map(key => key.replace(/_/g, ' '));  
  const arkBlockKeys = Object.keys(ArtBlockOptionLimit).map(key => key);
  //console.log(artblockLevelData)

  const now = Math.round((new Date()).getTime() / 1000);
  const remain = swapInfo?.reset_time - now;
  
  //아트블록  컴포넌트
  const Buster = (props) => {    
    //swapInfo?.data[lower_name]
    //const name = props.name.replace(/_/g, ' ');
    const name = props.name;        
    const lower_name = props.name.toLowerCase();

    
    const buff = (remain > 0 && props?.myInfo.data[lower_name]?.buff) || 0;
    const level = (remain > 0 && props?.myInfo.data[lower_name]?.level) || 0;
    const point = (remain > 0 && props?.myInfo.data[lower_name]?.point) || 0;
    
    return (
      <div className={styles.busterBox}>
        <div className={styles.busterBoxTop}>
          <div className={styles.busterBoxTopLeft}>
            <div><img src={process.env.REACT_APP_TEST_PATH+"/images/artblock/gem/"+name+".png"} alt='gem' style={{width:'24px'}}/></div>
            <div className="font-b3-b flex justify-center align-center">{name.replaceAll("_", " ")}{/* / {props.data.maxPercent} / {props.data.maxPoint} */}</div>
          </div>
          <div className={styles.busterBoxTopRight}>
            <div className="font-b3-r gray-500  flex justify-center align-center">Lv. {level} </div>
            <button 
              className={`${styles.busterBoxTopRightButton} flex justify-center align-center green-500 font-b4-r bg-red-500 white`}
              onClick={() => openPopup({ type: 'ArtBlockEdit', name: `${name}`, data: props })} 
            >
              {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "+ 수정" : "+ Edit"}
            </button>
          </div>  
        </div>
        <div className={`gray-500 ${styles.busterBoxBottom}`}>
          <div className={styles.busterBoxBottomOption1}>
            <div className="font-b6-r">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭 구슬" : "Active Orb"}</div>
            <div className="font-b4-r green-500">{point}</div>
          </div>
          <div className={styles.busterBoxBottomOption2}>
            <div className="font-b6-r">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭량" : "Amplifier"}</div>
            <div className="font-b4-r green-500">+{ (buff * 100).toFixed(2) }%</div>
          </div>
        </div>
      </div>
    )
  }

  //console.log("ArtblockRight", props);
  return (
    <div className={styles.container}>
      {/* <div className={styles.artblockRightTopBox}>
        <div className={styles.artblockRightTopTotalEnergyBox}>
          <div className="font-b3-r gray-500">Total Energy</div>
          <div className="font-h5 text-end">
            {data.user?.userInfo.wallet_addr ? '20,000' : '0'}</div>
        </div>
        <div className={styles.artblockRightTopCenter}></div>
        <div className={styles.artblockRightTopEnergyStatusBox}>
          <div className="font-b3-r gray-500 mb-4">Energy Status</div>
          <div className="gray-500">
            <div className="flex justify-between">
              <div className="font-b4-b green-500">Activate: {data.user?.userInfo.wallet_addr ? '16,000' : '0'}</div>
              <div className="font-b4-b white">Available: {data.user?.userInfo.wallet_addr ? '4,000' : '0'}</div>
            </div>
            <div className="mt-2">
              <div className={styles.artblockRightTopEnergyBar}>
                <div 
                  className={`${styles.artblockRightTopEnergyBarProcess} bg-green-500`} 
                  style={data.user?.userInfo.wallet_addr ? {width:'80%'} : {width:'0%'}}>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div> */}

      <div className={`${styles.artblockRightBottomBox} white`}> 
        {
          swapInfo.data &&
            arkBlockKeys.map(function(data, index){              
              return(
                //myInfo={swapInfo?.data[lower_name]}
                <Buster name={data} data={ArtBlockOptionLimit[data]} myInfo={swapInfo} key={index}/>
              )          
            })
        }
      </div>
    </div>
  )
}
