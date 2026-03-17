
import { _decorator, Component, Node, Sprite, SpriteFrame, UITransform, Vec3, Material } from 'cc';
const { ccclass, property, executionOrder } = _decorator;

/**
 * Predefined variables
 * Name = StageSelectBG
 * DateTime = Tue May 10 2022 11:10:39 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = StageSelectBG.ts
 * FileBasenameNoExtension = StageSelectBG
 * URL = db://assets/Scripts/Stage/StageSelectBG.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

const TileSize = 256;
 
@ccclass('StageSelectBG')
@executionOrder(-1)
export class StageSelectBG extends Component {
    @property(Node)
    protected front1: Node = null;
    @property(Node)
    protected front2: Node = null;
    @property(Node)
    protected front3: Node = null;
    @property(Node)
    protected ground: Node = null;
    @property(Node)
    protected back1: Node = null;
    @property(Node)
    protected back2: Node = null;
    @property(Node)
    protected back3: Node = null;
    @property(Node)
    protected cloud: Node = null;
    @property(SpriteFrame)
    protected grounds: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected clouds: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected frontGrounds1: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected frontGrounds2: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected frontGrounds3: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected backGrounds1: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected backGrounds2: SpriteFrame[] = [];
    @property(SpriteFrame)
    protected backGrounds3: SpriteFrame[] = [];
    @property(Material)
    protected customMat: Material = null;

    protected transform: UITransform = null;
    private static isRefresh = false;
    private static isRefreshIng = false;

    protected createData: {} = null;
    onLoad() {
        this.createData = {};
        this.transform = this.getComponent(UITransform);
    }

    Init () {
        this.node.on("size-changed", StageSelectBG.OnWindowResize);
    }

    onEnable() {
    }
    onDisable() {
    }

    onDestroy() {
        this.node.off("size-changed", StageSelectBG.OnWindowResize);
    }

    protected static OnWindowResize() {
        StageSelectBG.isRefresh = true;
    }

    update (deltaTime: number) {
        if(!StageSelectBG.isRefresh) {
            return;
        }

        this.Refresh();
    }

    protected Refresh() {
        if(this.transform == null) {
            return;
        }

        if(StageSelectBG.isRefreshIng) {
            return;
        }

        StageSelectBG.isRefreshIng = true;

        this.createData['size'] = Math.ceil(this.transform.contentSize.width / TileSize);
        this.CreateAndRefreshField('front1', this.front1, this.frontGrounds1);
        this.CreateAndRefreshField('front2', this.front2, this.frontGrounds2);
        this.CreateAndRefreshField('front3', this.front3, this.frontGrounds3);
        this.CreateAndRefreshField('ground', this.ground, this.grounds);
        this.CreateAndRefreshField('back1', this.back1, this.backGrounds1);
        this.CreateAndRefreshField('back2', this.back2, this.backGrounds2);
        this.CreateAndRefreshField('back3', this.back3, this.backGrounds3);
        this.CreateAndRefreshField('cloud', this.cloud, this.clouds);

        StageSelectBG.isRefresh = false;
        StageSelectBG.isRefreshIng = false;
    }

    protected CreateAndRefreshField(name: string, parent: Node, target: SpriteFrame[]) {
        if(name == "" || parent == null || target == null || target.length < 1) {
            return;
        }
        if(this.createData[name] == undefined) {
            this.createData[name] = {};
        }
        if(this.createData[name]['curNum'] == undefined) {
            this.createData[name]['curNum'] = 0;
        }
        if(this.createData[name]['curTarget'] == undefined) {
            this.createData[name]['curTarget'] = 0;
        }

        let size = this.createData['size'];
        
        while(this.createData[name]['curNum'] < size) {
            var i = this.createData[name]['curNum'];
            if(i > 50) {
                break;
            }

            let node = new Node(`${i}`);
            parent.addChild(node);
    
            let transform = node.addComponent(UITransform);
            let sprite = node.addComponent(Sprite);
            if(sprite != null) {
                sprite.spriteFrame = target[this.createData[name]['curTarget']];
                sprite.trim = false;
                if(this.customMat != null) {
                    sprite.setMaterial(this.customMat, 0);
                }
                this.createData[name]['curTarget']++;
                if(target.length <= this.createData[name]['curTarget']) {
                    this.createData[name]['curTarget'] = 0;
                }
            }
            if(transform != null) {
                transform.setContentSize(TileSize + 1, TileSize + 1);
            }
    
            node.setPosition(new Vec3(TileSize * 0.5 + TileSize * i, 0, 0));
            if(this.createData[name][i] != undefined) {
                this.createData[name][i].removeFromParent();
                delete this.createData[name][i];
            }
            this.createData[name][i] = node;
            this.createData[name]['curNum']++;
        }

        while(this.createData[name]['curNum'] > size) {
            var i = this.createData[name]['curNum'];
            if(i < 0) {
                break;
            }

            if(this.createData[name][i] != null) {
                this.createData[name][i].removeFromParent();
            }
            delete this.createData[name][i];

            this.createData[name]['curTarget']--;
            if(this.createData[name]['curTarget'] < 0) {
                this.createData[name]['curTarget'] = target.length - 1;
            }
            this.createData[name]['curNum']--;
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
