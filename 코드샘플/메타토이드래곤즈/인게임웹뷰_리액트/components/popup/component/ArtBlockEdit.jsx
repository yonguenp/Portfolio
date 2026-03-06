import { useEffect, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import { ReactComponent  as ArrowBig} from '../../../assets/svg/common/arrow(swap)_big.svg'
import { ReactComponent  as Plus} from '../../../assets/svg/artblock/plus.svg'
import { ReactComponent  as Minus} from '../../../assets/svg/artblock/minus.svg'
import { useDAppState } from '../../../DApp/src/providers/DAppProvider';

import { updateSwapInfo } from "../../../data/artBlockSlice";
import { useDispatch } from "react-redux";

//어떤 옵션인지, 각종 정보들 같이 묶어서 가져와야할듯함
export default function ArtBlockEdit(props) {
    
    //const data = useSelector(state => state.user);        
    const dispatch = useDispatch();
    const { userInfo } = useDAppState();
    const {popupData, closePopup} = props;
    const { swapInfo, artInfo, editor, availPoint } = useSelector((state) => state.artblock);

    const stat = popupData.name.toLowerCase();

    /*
        console.log(popupData);
        data:
            data:{maxPercent: '' ....},
            myInfo: {buff:0, level:0, next_buff:0.001, next_point:5, point: 0 , stat:'pvp_dmg'}
            name: 'PvP_DMG'
            ....
    */
    const [hideNextLevel, setHideNextLevel] = useState(false);

    const [gage, setGage] = useState(0);
    const [value, setValue] = useState(0);
    const [firstValue, setFirstValue] = useState(0);   // 50 현재 나의 Active Orb

    const [activePoint, setActivePoint] = useState(0);
    const [inActivePoint, setInActivePoint] = useState(0);

    const [activePointText, setActivePointText] = useState('');
    const [inActivePointText, setInActivePointText] = useState('');
    const [thumbMove, setThumbMove] = useState(false);

    const [availableValue, setAvailableValue] = useState(0);
    const [disabled, setDisabled] = useState(false);    

    // 내가 올릴수 있는 최대치 레벨
    const [availableLevel, setAvailableLevel] = useState(0);    

    const [prevLevel, setPrevLevel] = useState(0);
    const [prevPercent, setPrevPercent] = useState(0.00);
    const [prevPoint, setPrevPoint] = useState(0);

    const [nextLevel, setNextLevel] = useState('');
    const [nextPercent, setNextPercent] = useState(0.00);
    const [nextPoint, setNextPoint] = useState(0);

    const sliderRef = useRef(null);
    const thumbRef = useRef(null);

    const [touchPosition, setTouchPosition] = useState({ x: 0, y: 0 });

    const maxPercent = popupData?.data?.data?.maxPercent || 0;
    const maxPoint = popupData?.data?.data?.maxPoint || 0;

    const maxPointNum = parseInt(maxPoint.replace(/\D/g, ''), 10)

    /*
    const stepNumber = maxPercent === '50%' ? '50stepNumber' : '60stepNumber';
    const stepPoint = maxPoint === '18,000' ? '18000p' :
                      maxPoint === '9,500' ? '9500p' :  
                      maxPoint === '6,000' ? '6000p' :
                      maxPoint === '7,500' ? '7500p' : '0';
    const stepPointArray = ArtBlockLevel.map(item => item[stepPoint]);         
    */


    //다음 레벨 퍼센테이지 조정
    function totalLevelPercent(level, mode) {
        const arr =  artInfo.info[stat];
        if(arr == undefined)
            return;
        
        if(mode === 'default'){
            let totalPrevSum = 0;

            for(var i = 0 ; i <= level ; i++){       
                totalPrevSum += parseFloat(arr[i].buff)
            }
            setPrevPercent(totalPrevSum.toFixed(4));
        } 
        
        let totalSum = 0;
        for(var i = 0 ; i <= level ; i++){            
            //console.log(parseFloat(arr[i].buff));
            totalSum += parseFloat(arr[i].buff)
        }
        setNextPercent(totalSum.toFixed(4));
    }

    //현재 포인트 총 몇까지 썻나, 현재 내 레벨 기준으로
    function totalLevelPoint(mode = ''){
        //const arr =  artInfo.info[stat];
        if(mode === ''){
            //const totalPoint1 = ArtBlockLevel.filter(item => item.level <= level).reduce((acc, cur) => acc + parseFloat(cur[stepPoint]), 0);
            //const totalPoint2 = ArtBlockLevel.filter(item => item.level <= level+1).reduce((acc, cur) => acc + parseFloat(cur[stepPoint]), 0);
            
            setValue(popupData.data?.myInfo?.data[stat]?.point);
            setFirstValue(popupData.data?.myInfo?.data[stat]?.point);
        }else{
            //const totalPoint = ArtBlockLevel.filter(item => item.level <= level).reduce((acc, cur) => acc + parseFloat(cur[stepPoint]), 0);
            //setValue(totalPoint);
            setValue(availableValue);
        }
    }
    
    function countFunc(mode) {
        //prevLevel : 현재 레벨
        //Math.round(value) : 현재 게이지
        //게이지 thumb 위치도 줄면서 , value 도 줄어야함

        if(hideNextLevel){
            setHideNextLevel(false);
            return;
        }

        const arr =  artInfo.info[stat];
        if(arr === undefined)
            return;
        
        if(mode === 'minus'){
            if(nextLevel > 0){

                setNextLevel(prev => prev-1)
                totalLevelPercent(nextLevel-1);

                setValue(prev => prev - arr[nextLevel].point);
                setPrevPoint(arr[nextLevel-1].point);
                setNextPoint(arr[nextLevel].point);
            }
        }else if(mode === 'plus'){
            if(nextLevel < 100){
                setNextLevel(prev => prev+1)
                totalLevelPercent(nextLevel+1)

                setValue(prev => prev + arr[nextLevel+1].point);

                setPrevPoint(arr[nextLevel+1].point);

                if(nextLevel === 99){
                    setNextPoint(0);
                }else{
                    setNextPoint(arr[nextLevel+2].point);
                }                
            }
        }else if(mode === 'max'){

            if(availableLevel < 0){
                return;
            }

            setNextLevel(availableLevel)
            totalLevelPercent(availableLevel)
            totalLevelPoint('max')

            //let prevPointArr = ArtBlockLevel.filter(item => item.level === availableLevel-1 || item.level === availableLevel+1 )
            //setPrevPoint(prevPointArr[0][stepPoint]);
            //setNextPoint(prevPointArr[1][stepPoint]);

            let totalSum = 0;
            for(var i = 0 ; i <= availableLevel ; i++){
                totalSum += parseFloat(arr[i].point)
            }

            setValue(totalSum);

            setPrevPoint(arr[availableLevel-1]?.point | 0);

            if(availableLevel > 99){
                setNextPoint(0);
            }else{
                setNextPoint(arr[availableLevel+1].point);
            }
            
            setThumbMove(true);
        }
    }

    useEffect(() => {
        setThumbMove(true);

        //console.log((firstValue + popupData.data?.myInfo?.remain_point), value);
        
        (firstValue + popupData.data?.myInfo?.remain_point || 0 ) >= value ? setDisabled(false) : setDisabled(true);

        const point = value - firstValue;
        //console.log(firstValue);
        setActivePointText(<>{firstValue} {thumbMove && point !== 0 ? <span className={`font-b7-r ${(firstValue + popupData.data?.myInfo?.remain_point ) > value ? `green-500` : `red-500`}`}>({point > 0 ? '+' : ''}{point.toLocaleString()})</span> : ''}</>);
        
        //더 낮은 레벨로 갈때
        if(point < 0){
           setInActivePoint(popupData.data?.myInfo?.remain_point + Math.abs(point));
           setInActivePointText(
            <>
                <span className="white">{parseInt(popupData.data?.myInfo?.remain_point)} </span> 
                {thumbMove ? 
                    <span className={`font-b7-r white`}>(+{Math.abs(point).toLocaleString()})</span> 
                : ''}
            </>
            );           
        }else{           
           setInActivePoint(popupData.data?.myInfo?.remain_point - Math.abs(point));
           setInActivePointText(
            <>
                <span className="white">
                    {parseInt(popupData.data?.myInfo?.remain_point)} 
                </span>
                {
                    prevLevel !== nextLevel &&
                    thumbMove ? 
                        <span className={`font-b7-r 
                        ${(firstValue + popupData.data?.myInfo?.remain_point ) > value ? 'white' : 'red-400'}                            
                    `}>
                        (-{Math.abs(point).toLocaleString()})
                    </span> 
                    : ''
                }
            </>
           );
        }        
        //checkAvailableLevel(popupData.data?.myInfo?.remain_point + Math.abs(point));        
    },[value])

    useEffect(() => {
        if (!popupData.data || !popupData.data.myInfo) {return;}
        //const prevLevel = popupData.data?.myInfo?.level;

        const prevLevel = popupData.data?.myInfo?.data[stat]?.level || 0;

        setPrevLevel(prevLevel);
        setNextLevel(prevLevel);
        
        const arr =  artInfo.info[stat];
        
        setPrevPoint(arr && prevLevel > 0 ? arr[prevLevel].point : 0);
        setNextPoint(arr && prevLevel < 100 ? arr[prevLevel + 1].point : 0);
                       
        totalLevelPercent(prevLevel, 'default');

        totalLevelPoint();

        checkAvailableLevel(popupData.data.myInfo.data[stat]?.point + popupData.data?.myInfo?.remain_point);
        


    },[])

    const checkAvailableLevel = (point) => {

        if(!point){
            point = popupData.data?.myInfo?.remain_point;
        }

        let artblockPointSum = 0;
        const arr =  artInfo.info[stat];
        if(arr === undefined)
        {
            setAvailableLevel(0);
            setAvailableValue(0);
            return;
        }
        //해당 stat 의 maxPoint 보다 내 InActive가 많으면 아래 로직 안타도돼        

        // artblockPointSum 특정 레벨까지 필요한 buffPoint 총합

        for (let i = 0; i <= 100; i++) {
            const cur = arr[i];
            if(artblockPointSum > parseInt(point)){

                setAvailableLevel(i-2);
                artblockPointSum -= parseFloat(cur.point)
                setAvailableValue(artblockPointSum);
                //console.log(artblockPointSum, ' 레벨 : ' + (i-1))
                //artblockPointSum -= parseFloat(cur.point)                
                //result -= parseFloat(cur[stepPoint]);
                break; // 루프를 중단하고 빠져나옴
            }
            artblockPointSum += parseFloat(cur.point)
            setAvailableLevel(i);
            setAvailableValue(artblockPointSum);
        }
        /*
        if(point >= maxPointNum){
            setAvailableLevel(100);
            setAvailableValue(maxPointNum);
        } 
         */       
    }

    // 마우스 이동 이벤트 핸들러
    const handleTouchStart = (event) => {
        setThumbMove(true);

        const touch = event.touches[0];

        setTouchPosition({
            x: touch.clientX,
            y: touch.clientY
        });
        
        // 슬라이더의 값 업데이트
        const sliderRect = sliderRef.current.getBoundingClientRect();
        const offsetX = touch.clientX - sliderRect.left;
        const trackWidth = sliderRect.width;

        let newValue = (offsetX / trackWidth) * 100;
        newValue = Math.min(100, Math.max(0, newValue));

        const arr =  artInfo.info[stat];

        let totalSum = 0;
        for(var i = 0 ; i <= Math.round(newValue) ; i++){
            totalSum += parseFloat(arr[i].point)
        }

        setGage(newValue);
        setValue(totalSum);

        // Math.round(newValue) == 왔다리갔다리 하는 레벨
        if(Math.round(newValue) >= 0 && Math.round(newValue) <= 100){

            // let prevPointArr = ArtBlockLevel.filter(item => item.level === Math.round(newValue)-1 
            //                                                 || item.level === Math.round(newValue)
            //                                                 || item.level === Math.round(newValue)+1 )
            
            const arr =  artInfo.info[stat];

            if(Math.round(newValue) === 0){
                setPrevPoint(0);
                setNextPoint(arr[1].point);
                totalLevelPercent(0);

            }else if(Math.round(newValue) === 100){                
                setPrevPoint(arr[100].point);
                setNextPoint(0);
                totalLevelPercent(100);
            }else{
                setPrevPoint(arr[Math.round(newValue)].point);
                setNextPoint(arr[Math.round(newValue)+1].point);

                totalLevelPercent(Math.round(newValue));
            }

            //팝업창 하단 - , + 버튼 값 변경
            setNextLevel(Math.round(newValue));
        }

        // 슬라이더 썸 위치 조정
        const thumbWidth = thumbRef.current.offsetWidth;        
        const thumbPosition = (newValue / 100) * (trackWidth - thumbWidth);

        if(thumbPosition === 0 ){
            thumbRef.current.style.left = '-10px';      
        }else{
            //thumbRef.current.style.left = thumbPosition + 'px';      
        }
    };

    const artBlockSave = async() => {
        if(disabled === true || value == firstValue){
            return;
        }

        try {            
            // let tmp_state = {};

            // for (const key of Object.keys(swapInfo?.data)) {

            //     const v = swapInfo?.data[key]; //atk, def ... 
            //     const level = v.level + editor[key].val; //각 스탯별 레벨
            //     const buff = Number([...Array(level + 1).keys()].reduce((pSum, a) => pSum + artInfo.info[key][a]?.buff, 0).toFixed(4));
            //     const point = Number([...Array(level + 1).keys()].reduce((pSum, a) => pSum + artInfo.info[key][a]?.point, 0).toFixed(4));            

            //     tmp_state = {
            //         ...tmp_state,
            //         [key]: {
            //             buff: buff,
            //             point: point,
            //             level: level,
            //             stat: key
            //         }
            //     };            
            // }

            // tmp_state[stat].buff = Number(nextPercent);
            // tmp_state[stat].point = value;
            // tmp_state[stat].level = nextLevel;

            // const param = {
            //     ...tmp_state,
            //     used_point: Math.abs(swapInfo.total_point - inActivePoint),
            //     total_point: swapInfo.total_point,
            //     remain_point: inActivePoint,
            // }
            
            const data = await window.DApp.post("artblock/edit", {
                stat : stat,
                point : value,
                //new_state : JSON.stringify(param),            
                //addr: userInfo?.addr,
                //tokens: selectedStakingData.join(","),
                server_tag : sessionStorage.getItem("server_tag")
            });

            //update 성공
            if (data && data.rs == 0)
            {
                dispatch(updateSwapInfo(data.current));

                closePopup(); 

                window.DApp.emit("dapp.popup", {
                    err: 0,
                    title: "Success",
                    msg: "Save to stat point has been completed.",
                });               
            }
            else if(!data || !data.msg)
            { //실패
                window.DApp.emit("dapp.popup", {
                    err: 1,
                    title: "Error",
                    msg: "Failed save to stat point.",
                });
            }

        } catch (error) {            
            console.error('Error fetching data:', error);
            window.DApp.emit("dapp.loading", {isLoader: false});
        }

    }

    return (
        <div>
            <div className="bg"></div>
            <div className="container">                    
                <div className="flex white gap-4 mb-8 ">
                    <div><img src={process.env.REACT_APP_TEST_PATH+"/images/artblock/gem/"+popupData.name+".png"} alt='gem' style={{width:'24px'}}/></div>
                    <div className="font-b2-b flex justify-center align-center">{popupData.name.replaceAll("_", " ")}</div>
                </div>
                <div className="statLevelBox  text-start">
                    <div className="statLevelBoxDiv font-b3-r gray-500">
                        <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "스탯 레벨" : "Stat Level"}</div>
                        <div>{popupData.name.replaceAll("_", " ")}</div>
                    </div>
                    <div className="statLevelBoxDiv font-b3-b green-500 text-start">
                        <div>Lv.{prevLevel}</div>
                        <div>{ (prevPercent * 100).toFixed(2) }%</div>
                    </div>
                    <div className="statLevelBoxDiv flex align-center justify-center">
                        {
                            !hideNextLevel && <ArrowBig fill='#3A3838'/>
                        }                        
                    </div>
                    <div className="statLevelBoxDiv font-b3-b green-500 text-start">
                        {
                            !hideNextLevel && <>
                            <div>Lv.{nextLevel}</div>
                            <div>{ (nextPercent * 100).toFixed(2) }%</div>
                            </>
                        }                        
                    </div>
                </div>
                
                <div className="content mt-8 mb-8 p-16">
                    <div className={`gray-500 artBlockContentBottom mb-8`}>
                        <div className={`artBlockContentBottomOption1`}>
                            <div className="font-b4-r">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "활성 구슬" : "Active Orb"}</div>
                            <div className="font-b4-b green-500">{activePointText}</div>
                        </div>
                        <div className={`artBlockContentBottomOption2`}>
                            <div className="font-b4-r">{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "비활성 구슬" : "InActive Orb"}</div>                            
                            <div className={`font-b4-b`}>{inActivePointText}</div>
                        </div>
                    </div>
                    
                    <div className="slider" ref={sliderRef}>                        
                        <div className="slider-track" onTouchMove={handleTouchStart}>
                            <span
                                className= {
                                    (firstValue + popupData.data?.myInfo?.remain_point ) > value ?
                                    "slider-thumb z-5"
                                    : "slider-thumb_disable z-5"
                                }
                                ref={thumbRef}
                                //style={{ left: `calc(${nextLevel+'% - 20px'})` }}
                                //style={{ left: `calc(${nextLevel >= 10 ? nextLevel+'% - 20px' : nextLevel+'% - 3px'})` }}
                                // style={{ left: `calc(${nextLevel+'% - 5px'})` }}
                                style={{ left: `${nextLevel === 0 ? '-10px' : 'calc('+ nextLevel +'% - 10px)'}` }}
                                //onMouseDown={handleMouseDown}
                            ></span>
                        </div>
                        <div className="slider-white-gage" style={{ backgroundColor: 'white', width:`calc(${availableLevel + '%' + availableLevel < 90 ? '+ 10px' : ''})`, height:'10px'}}></div>                        
                        {
                            (firstValue + popupData.data?.myInfo?.remain_point ) > value ?
                            <div className="slider-green-gage" style={{ backgroundColor: '#3CC065', width:`calc(${nextLevel+'%'})`, height:'10px'}}></div>
                            : <div className="slider-green-gage" style={{ backgroundColor: '#f33535', width:`calc(${nextLevel+'%'})`, height:'10px'}}></div>                        
                        }
                        {/* <span className="slider-value white">{Math.round(value)}</span> */} 
                    </div>

                    <div className="mt-4 font-b4-r gray-500 text-end">
                        {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "최대 :" : "Max :"} + {maxPoint}
                        {/* / {maxPercent} */}
                    </div>
                    <div className="flex justify-between gap-8 white font-b4-r">
                        <div className="countBtn flex gap-4 justify-center align-center pt-4 pb-4 bg-gray-800"
                            onClick={()=>countFunc('minus')}
                        >
                            <Minus/>
                            <div>{prevPoint}</div>
                        </div>
                        <div className="countBtn flex gap-4 justify-center align-center pt-4 pb-4 bg-gray-800"
                            onClick={()=>countFunc('plus')}
                        >
                            <Plus/>
                            <div>{nextPoint}</div>
                        </div>
                        <div className={`countBtn flex gap-4 justify-center align-center pt-4 pb-4 bg-gray-800
                            
                            `}
                            onClick={()=>countFunc('max')}
                        >
                            <Plus/>
                            <div>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "최대" : "Max"}</div>
                        </div>
                    </div>
                    {/* <input type="range" className="artBlockEditRange"/> */}
                </div>
               
                <div className="popupButtons font-b3-b">
                    <div className="cancelBtn bg-gray-700" onClick={closePopup}>{navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "취소" : "Cancel"}</div>                
                    <div className={`signBtn bg-red-400 ${disabled === true || value == firstValue ? 'opacity-50' : ''}`}
                        disabled = {value == firstValue}
                        onClick={() => artBlockSave()}
                    >
                        {navigator.language == "ko-KR" && sessionStorage.getItem("web2") === "true" ? "저장" : "Save"}
                    </div>
                </div>
            </div>
        </div> 
    )
}