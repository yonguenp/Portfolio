import React, { useState } from 'react'

export default function GenesisCardActivate(props) {

  const {popupData, closePopup} = props;

  const [ok, setOk] = useState(false);

  return (
    <div>
        <div className="bg"></div>
        <div className="container">
            
            {
                ok === false ?
                    <>
                        <div className="textBox">
                            <div className="font-h6">Activate</div>
                            <div className="font-b3-r gray-500">First insert activates with a fee of 10 Magnite.</div>
                        </div>
                        <div className="popupButtons font-b3-b">
                            <div className="cancelBtn bg-gray-700" onClick={closePopup}>Cancel</div>                
                            <div className="signBtn bg-red-400" onClick={()=> setOk(true)}>
                                <img src={process.env.REACT_APP_TEST_PATH+"/images/icon/magnetblock.png"} alt="metod"/>
                                <div>10</div>                        
                            </div>
                        </div>
                    </>
                :
                    <>
                    <div className="textBox">
                        <div className="font-h6">Success</div>
                        <div className="font-b3-r gray-500">Now you can Insert Dragon Card</div>
                    </div>
                    <div className="popupButtons font-b3-b">
                        <div className="bg-red-400 cancelBtn" onClick={closePopup}>OK</div>
                    </div>
                    </>
            }
        </div>
    </div>
  )
}
