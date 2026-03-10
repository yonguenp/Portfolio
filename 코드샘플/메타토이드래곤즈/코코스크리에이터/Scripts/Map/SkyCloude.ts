
import { _decorator, Component, Node, randomRange, Sprite, Layers, Vec3, tween, SpriteFrame, CCInteger, color } from 'cc';
const { ccclass, property } = _decorator;
 
@ccclass('SkyCloude')
export class SkyCloude extends Component 
{
    @property(SpriteFrame)
    cloudSpr : SpriteFrame = null;

    @property(CCInteger)
    CloudMinY : number = 300

    @property(CCInteger)
    CloudMaxY : number = 1100

    @property(CCInteger)
    CloudLeft : number = -800

    @property(CCInteger)
    CloudRight : number = 2400

    static cloudCount : number = 0

    onLoad()
    {
        if(this.cloudSpr == null)
        {
            this.enabled = false;
            return;
        }
        SkyCloude.cloudCount = 0

        for(var i = 0; i < 20; i++)
            this.createCloud(false)
    }

    update(dt : number)
    {
        this.createCloud()
    }

    createCloud(isFadeIn : boolean = true)
    {
        if(randomRange(0, 100) > 90 && SkyCloude.cloudCount < 32)
        {
            SkyCloude.cloudCount += 1

            let obj = new Node()
            obj.parent = this.node
            obj.layer = Layers.nameToLayer("DEFAULT")
            obj.setScale(new Vec3(randomRange(1, 2), randomRange(1, 1.5), 1))

            let objSpr = obj.addComponent(Sprite)
            objSpr.spriteFrame = this.cloudSpr

            let endPos : number;
            let yPos : number = randomRange(this.CloudMinY, this.CloudMaxY)

            obj.position = new Vec3(randomRange(this.CloudLeft, this.CloudRight), yPos, 0)
            endPos = randomRange(this.CloudLeft, this.CloudRight)

            let duration = randomRange(20, 30);

            if(isFadeIn)
                obj.getComponent(Sprite).color = color(255,255,255,0)

            tween(obj).to(duration, {position: new Vec3(endPos, yPos, 0)}, {onComplete:()=>{ SkyCloude.cloudCount -= 1; obj.destroy()}}).union().start()
            tween(obj.getComponent(Sprite).color).to(randomRange(3,6), { a:255 }).delay(duration-12).to(randomRange(3,6), { a:0 }).union().start();
        }
    }
}
