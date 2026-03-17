
import { _decorator, Component, Node, Vec3, Prefab, instantiate, Vec4, UITransform, Sprite } from 'cc';
import { EventListener } from 'sb';
import { EventManager } from '../Tools/EventManager';
import { ShadowEvent } from './ShadowEvent';
const { ccclass, property, executionOrder } = _decorator;

/**
 * Predefined variables
 * Name = DragonShadowManager
 * DateTime = Thu May 26 2022 11:05:42 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = DragonShadowManager.ts
 * FileBasenameNoExtension = DragonShadowManager
 * URL = db://assets/Scripts/Character/DragonShadowManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

class NodeLink {
    private targetNode: Node = null;
    public get Target(): Node {
        return this.targetNode;
    }
    public set Target(value: Node) {
        this.targetNode = value;
    }
    private delFlag: boolean = false;
    public get DelFlag(): boolean {
        return this.delFlag;
    }
    public set DelFlag(value: boolean) {
        this.delFlag = value;
    }
    private shadow: Node = null;
    private info: Vec4 = null;

    shadowSprite : Sprite = null;

    constructor(target: Node, prefab: Prefab, parent: Node, info: Vec4, scale: Vec3 = Vec3.ONE) {
        this.AddShadow(target, prefab, parent, info, scale);
        this.SetSprite();
    }

    private AddShadow(target: Node, prefab: Prefab, parent: Node, info: Vec4, scale: Vec3): void {
        if(target == null || prefab == null || parent == null) {
            return;
        }
        this.Target = target;
        this.info = info;

        this.shadow = instantiate(prefab);
        if(this.shadow != null) {
            this.shadow.setScale(scale);
            parent.addChild(this.shadow);
            let transform = this.shadow.getComponent(UITransform);
            if(transform != null) {
                transform.setContentSize(this.info.z, this.info.w);
            }
        }
    }

    SetSprite()
    {
        this.shadowSprite = this.shadow.getComponent(Sprite);
        this.SetSpriteEnable(false);
    }

    SetSpriteEnable(isEnable : boolean)
    {
        if(this.shadowSprite != null)
        {
            if(this.shadowSprite.enabledInHierarchy != isEnable){
                this.shadowSprite.enabled = isEnable;
            }
        }
    }

    public Update(dt: number): void {
        if(this.Target == null || this.shadow == null) {
            return;
        }

        let pos = new Vec3(this.Target.getWorldPosition());
        pos.x += this.info.x;
        pos.y += this.info.y;
        this.shadow.setWorldPosition(pos);
        this.SetSpriteEnable(true);
    }

    public ChangeParent(parent: Node): void {
        if(parent == null || this.shadow == null) {
            return;
        }

        this.shadow.parent = parent;
    }

    public Destroy() {
        if(this.shadow != null) {
            this.shadow.destroy();
            this.shadow = null;
        }
    }
}
 
@ccclass('ShadowBeacon')
@executionOrder(-1)
export class ShadowBeacon extends Component implements EventListener<ShadowEvent> {
    public static Name: string = "ShadowBeacon";
    private frameSkip: boolean = false;
    private shadowFollows: NodeLink[] = null;
    @property(Prefab)
    private shadowPrefab: Prefab = null;
    GetManagerName(): string {
        return ShadowBeacon.Name;
    }
    GetID(): string {
        return "ShadowEvent";
    }
    
    update(deltaTime: number): void {
        if(this.shadowFollows == null) {
            return;
        }

        for(var i = 0; i < this.shadowFollows.length; i++) {
            if(this.frameSkip) {
                this.frameSkip = false;
                return;
            }
            let element = this.shadowFollows[i];
            if(element == null) {
                continue;
            }
            element.Update(deltaTime);
        }

        let follows = this.shadowFollows.filter(element => element.DelFlag);
        this.shadowFollows = this.shadowFollows.filter(element => !element.DelFlag);
        follows.forEach(element => {
            element.Destroy();
        });
    }
    
    onLoad() { 
        this.shadowFollows = [];
        EventManager.AddEvent(this);
    }

    onDestroy() {
        EventManager.RemoveEvent(this);
        const followsCount = this.shadowFollows.length;
        for(var i = 0; i < followsCount; i++) {
            let element = this.shadowFollows[i];
            if(element == null) {
                continue;
            }
            element.Destroy();
        }
        
        this.shadowFollows = null;
    }

    OnEvent(eventType: ShadowEvent): void {
        if(this.shadowFollows == null || eventType == null || eventType.Data == null) {
            return;
        }

        switch(eventType.Data['state']) {
            case 'Add': {
                let targets = this.shadowFollows.findIndex(element => element.Target == eventType.Data['target']);
                if(targets == -1) {
                    let scale = Vec3.ONE;
                    if(eventType.Data['scale'] != undefined) {
                        scale = eventType.Data['scale'];
                    }
                    this.shadowFollows.push(new NodeLink(eventType.Data['target'], this.shadowPrefab, this.node, eventType.Data['info'], scale));
                }
            } break;
            case 'ChangeParent': {
                let target = this.shadowFollows.find(element => element.Target == eventType.Data['target']);
                let scale = Vec3.ONE;
                if(eventType.Data['scale'] != undefined) {
                    scale = eventType.Data['scale'];
                }
                if(target != null) {
                    target.ChangeParent(eventType.Data['parent']);
                } else {
                    target = new NodeLink(eventType.Data['target'], this.shadowPrefab, this.node, eventType.Data['info'], scale);
                    target.ChangeParent(eventType.Data['parent']);
                    this.shadowFollows.push(target);
                }
            } break;
            case 'ReturnParent': {
                let target = this.shadowFollows.find(element => element.Target == eventType.Data['target']);
                let scale = Vec3.ONE;
                if(eventType.Data['scale'] != undefined) {
                    scale = eventType.Data['scale'];
                }
                if(target != null) {
                    target.ChangeParent(this.node);
                } else {
                    this.shadowFollows.push(new NodeLink(eventType.Data['target'], this.shadowPrefab, this.node, eventType.Data['info'], scale));
                }
            } break;
            case 'Del': {
                let target = this.shadowFollows.find(element => element.Target == eventType.Data['target']);
                if(target != null) {
                    target.DelFlag = true;
                }
            } break;
        }
    }
}
/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */
