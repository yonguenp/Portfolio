import React from 'react'
import { ReactComponent  as Level} from '../../../assets/svg/exchange/level.svg'
import { ReactComponent  as Vip} from '../../../assets/svg/exchange/vip3.svg'
import { ArtBlockOptionLimit } from '../../../data/data'


export default function ArtBlockBuffPointManual(props) {

  const {popupData, closePopup} = props;

  const arkBlockKeys = Object.keys(ArtBlockOptionLimit).map(key => key);
  const dummyArray = Array(14).fill(null); 

  const now = new Date();
    // 한국 시간 기준으로 맞추기
  const kstNow = new Date(now.toLocaleString("en-US", { timeZone: "Asia/Seoul" }));
  
  const ratio = "1:32";
  const maxAmount = "80,000"; 

  return (
    <div>
        <div className="bg"></div>
        <div className="container">
            <div className="textBox">
                <div className="font-h6">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "증폭 구슬" : "Amplifier Orb"}</div>
                <div className="font-b4-r gray-500">
                    {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? (
                        <>
                        <span>마그나이트는 <span className="green-500">{ratio} 비율</span>로 증폭 구슬로 전환할 수 있습니다.</span><br />
                        증폭 효과는 계정 내 모든 드래곤에게 적용됩니다.<br />
                        각 계정은 최대 <span className="green-500">{maxAmount} 증폭 구슬</span>을 보관할 수 있습니다.<br />
                        <span className="yellow-500">모든 증폭 효과는 아레나 시즌 시작 시 초기화됩니다.</span>
                        </>
                    ) : (
                        <>
                        <span>You can convert Magnite into Amplifier Orbs at a <span className="green-500">{ratio} ratio</span>.</span><br />
                        The Amplifier effect applies to all dragons in your account.<br />
                        Each account can hold up to <span className="green-500">{maxAmount} Amplifier Orbs</span>.<br />
                        <span className="yellow-500">All Amplifiers will reset at the start of each Arena Season.</span>
                        </>
                    )}
                </div>
            </div>
               
            <div className="costBoxListDataHeader font-b4-r white">
                <div className="theadRank">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "드래곤 증폭" : "Dragon Amplifier"}</div>
                <div className="theadCost border">
                    <div>
                        {/* <img src={process.env.REACT_APP_TEST_PATH+"/images/icon/magnetblock.png"} alt=""  style={{width:'24px'}}/> */}
                    </div>
                    <div>
                        {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "최대 증폭" : "Max Amplifier"}
                    </div>
                </div>
                <div className="theadCost">
                    <div>
                        <img src={process.env.REACT_APP_TEST_PATH+"/images/icon/buff_point.png"} alt="" style={{width:'24px'}}/>
                    </div>
                    <div>
                        {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "필요 증폭 구슬(최대)" : "Amplifier Orb(Max)"}
                    </div>
                </div>
            </div>       
            <div className="costBox font-b3-b">                
                <div className="costBoxListData font-b4-r white">
                   <table className="costBoxTable">
                        <tbody>
                            {
                                arkBlockKeys.map((data, index) => (
                                    <tr>
                                        <td className="rank font-b4-b">
                                            <div>
                                                <img src={process.env.REACT_APP_TEST_PATH+`/images/artblock/gem/${data.replace(/ /g, '_')}.png`} 
                                                style={{width:'24px'}}
                                                alt=""/>
                                                {data}
                                            </div>
                                        </td>
                                        <td>+{ArtBlockOptionLimit[data].maxPercent}</td>
                                        <td>{ArtBlockOptionLimit[data].maxPoint}</td>
                                    </tr>
                                ))
                            }
                        </tbody>
                   </table>
                </div>
            </div>

            <div className="singleBtn font-b3-b white" onClick={closePopup}>
                Close
            </div>
        </div>
    </div>
  )
}
