
import { _decorator, Component, Node,  Vec3, Vec2} from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ShakeEffect
 * DateTime = Tue May 10 2022 17:30:24 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = ShakeEffect.ts
 * FileBasenameNoExtension = ShakeEffect
 * URL = db://assets/Scripts/ShakeEffect.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('ShakeEffect')
export class ShakeEffect extends Component {

    @property(Node)
    shakeObject: Node = null;                   // shake할 대상 오브젝트

    shakeDuration: number = 0;                  // shake 지속 시간
    shakeStrength: Vec2 = new Vec2();           // shake 적용값 (shake 범위)

    originObjectPos: Vec3 = new Vec3();         // shake 대상의 원래 위치
    currentObjectPos: Vec3 = new Vec3();        // shake 대상의 현재 위치

    isPlayingEffect: boolean = false;           // 현재 shake effect 중인지 체크
    isEffectPause: boolean = false;             // 현재 shake effect 정지 상태 체크

    start() {
        this.originObjectPos = this.shakeObject.position.clone();
    }

    onDisable() {
        this.StopShakeEffect();
    }

    // ShakeEffect 실행 -> 해당 함수 호출하여 시작
    StartShakeEffect(duration:number, strengthX:number, strengthY:number) {
        if (this.isPlayingEffect) {return;}         // 현재 이펙트가 실행중인지 체크

        this.shakeDuration = duration;
        this.shakeStrength = new Vec2(strengthX, strengthY);

        this.currentObjectPos = this.shakeObject.position.clone();

        // 이펙트 실행
        this.schedule(this.PlayShakeObjectEffect);

        // 이펙트 종료 시간 설정
        // this.scheduleOnce(this.StopShakeEffect, this.shakeDuration);

        this.isPlayingEffect = true;
        this.isEffectPause = false;
    }

    // ShakeEffect 종료
    StopShakeEffect() {
        this.unschedule(this.PlayShakeObjectEffect);
        
        this.shakeDuration = 0;
        this.shakeStrength = null;

        this.shakeObject.position = this.currentObjectPos;

        this.isPlayingEffect = false;
        this.isEffectPause = true;
    }

    // ShakeEffect 일시 정지
    PauseShakeEffect() {
        this.isEffectPause = true;
    }

    // ShakeEffect 일시 정지 해제
    ResumeShakeEffect() {
        this.isEffectPause = false;
    }


    private PlayShakeObjectEffect() {
        if (this.isEffectPause) {return;}   // 현재 일시정지 중

        let randomX = this.GetRandomRange(-this.shakeStrength.x, this.shakeStrength.x);
        let randomY = this.GetRandomRange(-this.shakeStrength.y, this.shakeStrength.y);

        // x값 보정
        let resultX = this.currentObjectPos.x;
        let minResultX = this.currentObjectPos.x - randomX;
        let maxResultX = this.currentObjectPos.x + randomX;
        if (this.currentObjectPos.x < 0) {
            resultX = resultX > minResultX ? minResultX : resultX;
            resultX = resultX < maxResultX ? maxResultX : resultX;
        }
        else {
            resultX = resultX > maxResultX ? maxResultX : resultX;
            resultX = resultX < minResultX ? minResultX : resultX;
        }

        // y값 보정
        let resultY = this.currentObjectPos.y;
        let minResultY = this.currentObjectPos.y - randomY;
        let maxResultY = this.currentObjectPos.y + randomY;
        if (this.currentObjectPos.y < 0) {
            resultY = resultY > minResultY ? minResultY : resultY;
            resultY = resultY < maxResultY ? maxResultY : resultY;
        }
        else {
            resultY = resultY > maxResultY ? maxResultY : resultY;
            resultY = resultY < minResultY ? minResultY : resultY;
        }

        console.log("resultX ==>", resultX, "  resultY ==>", resultY);
        

        // 최종 결과값
        let resultPos = new Vec3(resultX, resultY, this.currentObjectPos.z);

        // 적용
        this.shakeObject.position = resultPos;
    }

    private GetRandomRange(min:number, max:number):number {
        let random = Math.random();
        return random * (max - min) + min;
    }
}