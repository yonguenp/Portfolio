import { useEffect, useRef, useState } from 'react';
import styles from '../../styles/ArtblockStatus.module.css'

export default function ArtblockStatus() {

    const [value, setValue] = useState(0);

    const controlRef = useRef(null);
    const leftBarRef = useRef(null);
    const rightBarRef = useRef(null);
    const perRef = useRef(null);

    useEffect(() => {
        const control = controlRef.current;
        const leftBar = leftBarRef.current;
        const rightBar = rightBarRef.current;
        const per = perRef.current;

        control.addEventListener('input', function(event) {
            progress(event.target.valueAsNumber);
        });
        control.addEventListener('change', function(event){
            progress(event.target.valueAsNumber);
        });

        function progress(value) {  
            setValue(value);
            per.innerHTML=value +'%';
            if (value <= 50) {
                var degree = 18 * value /5;
                console.log(degree)
                rightBar.style.transform = "rotate("+degree+"deg)";
                leftBar.style.transform = "rotate(0deg)";
            } else {
                var degree = 18 * (value - 50) / 5;
                rightBar.style.transform = "rotate(180deg)";
                leftBar.style.transform = "rotate("+degree+"deg)";
            }
        }
        progress(control.value);
      }, []);    

  return (
    <div className={`${styles.rewardBoxBottom} font-b4-r`}>
        <div>
            <div className={styles.circle_progress}>    
                <span className={styles.left}>  
                    {/* <span className={styles.barCircleTest}></span> */}
                    <span className={styles.bar} ref={leftBarRef}></span>
                </span>
                <span className={styles.right}>
                    <span className={styles.bar} ref={rightBarRef}></span>
                </span>
                <div className={styles.value} ref={perRef}></div>
            </div>
            <input 
                id="control"
                type="range"
                ref={controlRef}
                value={value}                        
            />
        </div>
    </div> 
  )
}



