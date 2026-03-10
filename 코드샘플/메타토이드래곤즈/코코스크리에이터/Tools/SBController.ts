
import { _decorator, Component, EventKeyboard, KeyCode, Vec3, systemEvent, SystemEvent, input, Input } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SBController
 * DateTime = Thu Dec 30 2021 14:06:09 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = SBController.ts
 * FileBasenameNoExtension = SBController
 * URL = db://assets/Scripts/Tools/SBController.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('SBController')
export class SBController extends Component {
    private keysDic: {} = {};
    public get KeysDic(): Object {
        return this.keysDic;
    }
    move: boolean = false;
    @property
    isRight: boolean = false;
    @property
    speed: number = 300;
    @property
    keyboard: boolean = false;
    isKeyboard: boolean = false;
    run: number = 1;

    onLoad() {
        if(this.keyboard) {
            this.SetKeyboard();
        }
    }

    onDestroy () {
        this.UnsetKeyboard();
    }

    SetKeyboard(): void {
        if (this.isKeyboard) {
            return;
        }

        this.isKeyboard = true;
        input.on(Input.EventType.KEY_DOWN, this.onKeyDown, this);
        input.on(Input.EventType.KEY_UP, this.onKeyUp, this);
    }

    UnsetKeyboard(): void {
        if (!this.isKeyboard) {
            return;
        }

        this.isKeyboard = false;
        input.off(Input.EventType.KEY_DOWN, this.onKeyDown, this);
        input.off(Input.EventType.KEY_UP, this.onKeyUp, this);
    }

    update(dt) {
        if(this.keyboard) {
            this.onController(dt, false);
        }
    }

    onKeyDown (e: EventKeyboard): void {
        switch (e.keyCode)
        {
            case KeyCode.KEY_W:
            case KeyCode.ARROW_UP:
                this.onEnterEvent("up");
                break;
            case KeyCode.KEY_S:
            case KeyCode.ARROW_DOWN:
                this.onEnterEvent("down");
                break;
            case KeyCode.KEY_A:
            case KeyCode.ARROW_LEFT:
                this.onEnterEvent("left");
                break;
            case KeyCode.KEY_D:
            case KeyCode.ARROW_RIGHT:
                this.onEnterEvent("right");
                break;
            case KeyCode.SHIFT_LEFT:
            case KeyCode.SHIFT_RIGHT:
                this.onEnterEvent("shift");
                break;
        }
    }

    onKeyUp (e: EventKeyboard): void {
        switch (e.keyCode)
        {
            case KeyCode.KEY_W:
            case KeyCode.ARROW_UP:
                this.onExitEvent("up");
                break;
            case KeyCode.KEY_S:
            case KeyCode.ARROW_DOWN:
                this.onExitEvent("down");
                break;
            case KeyCode.KEY_A:
            case KeyCode.ARROW_LEFT:
                this.onExitEvent("left");
                break;
            case KeyCode.KEY_D:
            case KeyCode.ARROW_RIGHT:
                this.onExitEvent("right");
                break;
            case KeyCode.SHIFT_LEFT:
            case KeyCode.SHIFT_RIGHT:
                this.onExitEvent("shift");
                break;
        }
    }

    onEnterEvent (e: string): void {
        switch (e)
        {
            case "up":
            case "Up":
            case "UP":
                this.keysDic[KeyCode.ARROW_UP] = true;
                break;
            case "down":
            case "Down":
            case "DOWN":
                this.keysDic[KeyCode.ARROW_DOWN] = true;
                break;
            case "left":
            case "Left":
            case "LEFT":
                this.keysDic[KeyCode.ARROW_LEFT] = true;
                break;
            case "right":
            case "Right":
            case "RIGHT":
                this.keysDic[KeyCode.ARROW_RIGHT] = true;
                break;
            case "shift":
            case "Shift":
            case "SHIFT":
                this.keysDic[KeyCode.SHIFT_LEFT] = true;
                break;
        }
    }

    onExitEvent (e: string): void {
        switch (e)
        {
            case "up":
            case "Up":
            case "UP":
                this.keysDic[KeyCode.ARROW_UP] = false;
                break;
            case "down":
            case "Down":
            case "DOWN":
                this.keysDic[KeyCode.ARROW_DOWN] = false;
                break;
            case "left":
            case "Left":
            case "LEFT":
                this.keysDic[KeyCode.ARROW_LEFT] = false;
                break;
            case "right":
            case "Right":
            case "RIGHT":
                this.keysDic[KeyCode.ARROW_RIGHT] = false;
                break;
            case "shift":
            case "Shift":
            case "SHIFT":
                this.keysDic[KeyCode.SHIFT_LEFT] = false;
                break;
        }
    }

    IsPushKey(keyType: KeyCode){
        if(this.keysDic == null) {
            return false;
        }
        return this.keysDic[keyType];
    }

    onController (dt, direction: boolean = true, speed: number = -1): void {
        this.run = 1;
        this.move = false;
        if(speed < 0) {
            speed = this.speed;
        }
        if (this.keysDic[KeyCode.SHIFT_LEFT] || this.keysDic[KeyCode.SHIFT_RIGHT])
        {
            this.run = 2;
        }
        if (this.keysDic[KeyCode.KEY_W] || this.keysDic[KeyCode.ARROW_UP])
        {
            let position = this.node.getPosition();
            position.y += speed * dt * this.run;
            this.node.setPosition(position);
        }
        if (this.keysDic[KeyCode.KEY_S] || this.keysDic[KeyCode.ARROW_DOWN])
        {
            let position = this.node.getPosition();
            position.y -= speed * dt * this.run;
            this.node.setPosition(position);
        }
        if (this.keysDic[KeyCode.KEY_A] || this.keysDic[KeyCode.ARROW_LEFT])
        {
            let position = this.node.getPosition();
            position.x -= speed * dt * this.run;
            this.node.setPosition(position);
            if(direction) {
                const scale = this.node.getScale();
                this.node.setScale(new Vec3(this.isRight ? -Math.abs(scale.x) : Math.abs(scale.x), scale.y, scale.z));
            }
            this.move = true;
        }
        if (this.keysDic[KeyCode.KEY_D] || this.keysDic[KeyCode.ARROW_RIGHT])                                             
        {
            let position = this.node.getPosition();
            position.x += speed * dt * this.run;
            this.node.setPosition(position);
            if(direction) {
                const scale = this.node.getScale();
                this.node.setScale(new Vec3(this.isRight ? Math.abs(scale.x) : -Math.abs(scale.x), scale.y, scale.z));
            }
            this.move = true;
        }
    }

    MoveTarget(dt: number, localPos: Vec3, direction: boolean = true, speed: number = -1, yScale: number = 0.7) {
        if(speed < 0) {
            speed = this.speed;
        }

        let position = this.node.getPosition();
        let normal = new Vec3(localPos.x - position.x, localPos.y - position.y, localPos.z - position.z).normalize();
        
        if(normal.x != 0) {
            position.x += speed * dt * normal.x * (1 - (normal.y * yScale));
        
            if(normal.x > 0) {
                if(position.x > localPos.x) {
                    position.x = localPos.x;
                }
                if(direction) {
                    const scale = this.node.getScale();
                    this.node.setScale(new Vec3(this.isRight ? Math.abs(scale.x) : -Math.abs(scale.x), scale.y, scale.z));
                }
            } else if(normal.x < 0) {
                if(position.x < localPos.x) {
                    position.x = localPos.x;
                }
                if(direction) {
                    const scale = this.node.getScale();
                    this.node.setScale(new Vec3(this.isRight ? -Math.abs(scale.x) : Math.abs(scale.x), scale.y, scale.z));
                }
            }

            this.node.setPosition(position);
        }

        if(normal.y != 0) {
            position.y += speed * dt * normal.y * yScale;
        
            if(normal.y > 0) {
                if(position.y > localPos.y) {
                    position.y = localPos.y;
                }
            } else if(normal.y < 0) {
                if(position.y < localPos.y) {
                    position.y = localPos.y;
                }
            }

            this.node.setPosition(position);
        }
    }

    MoveWorldTarget(dt: number, worldPos: Vec3, direction: boolean = true, speed: number = -1) {
        if(speed < 0) {
            speed = this.speed;
        }

        let curPos = this.node.getWorldPosition();
        let normal = new Vec3(worldPos.x - curPos.x, worldPos.y - curPos.y, worldPos.z - curPos.z).normalize();
        
        if(normal.x != 0) {
            let position = this.node.getWorldPosition();
            position.x += (speed * dt * this.run * normal.x);
        
            if(normal.x > 0) {
                if(position.x > worldPos.x) {
                    position.x = worldPos.x;
                }
                if(direction) {
                    const scale = this.node.getScale();
                    this.node.setScale(new Vec3(this.isRight ? Math.abs(scale.x) : -Math.abs(scale.x), scale.y, scale.z));
                }
            } else if(normal.x < 0) {
                if(position.x < worldPos.x) {
                    position.x = worldPos.x;
                }
                if(direction) {
                    const scale = this.node.getScale();
                    this.node.setScale(new Vec3(this.isRight ? -Math.abs(scale.x) : Math.abs(scale.x), scale.y, scale.z));
                }
            }

            this.node.setWorldPosition(position);
        }

        if(normal.y != 0) {
            let position = this.node.getWorldPosition();
            position.y += speed * dt * this.run * normal.y;
        
            if(normal.y > 0) {
                if(position.y > worldPos.y) {
                    position.y = worldPos.y;
                }
            } else if(normal.y < 0) {
                if(position.y < worldPos.y) {
                    position.y = worldPos.y;
                }
            }

            this.node.setWorldPosition(position);
        }
    }
}