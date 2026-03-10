import { _decorator, Node, tween, instantiate, Vec3, Label, Color, math, Tween } from 'cc';
import { GameManager } from './GameManager';
import { ResourceManager, ResourcesType } from './ResourceManager';

export enum eDamageType 
{
    CRITICAL,
    ELEMENT_FIRE,
    ELEMENT_WATER,
    ELEMENT_EARTH,
    ELEMENT_WIND,
    ELEMENT_LIGHT,
    ELEMENT_DARK,
    MISS
}

class DamageTween {
    public TargetTween: Tween<any>[] = null;
    public TargetNode: Node = null;
}

export class DamageManager
{
    private static instance : DamageManager = null;    
    private static parentNode : Node = null;
    private static tweens: DamageTween[] = null;

    static InstanceCheck()
    {
        if(DamageManager.instance == null)
        {
            DamageManager.instance = new DamageManager();
            if(this.tweens == null) {
                this.tweens = [];
            }
        }
    }

    static SetDamageLayer(target : Node)
    {
        DamageManager.parentNode = target
    }
    
    static Damage(type : eDamageType, damage : number = 0)
    {
        DamageManager.InstanceCheck()
        if(DamageManager.parentNode == null)
            return

        let newNode : Node = null;
        let prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["damage"];
        if(prefab != null)
        {
            newNode = instantiate(prefab)
        }
        newNode.getComponent(Label).string = Math.floor(damage).toString()
        newNode.parent = DamageManager.parentNode
        newNode.position = Vec3.ZERO

        this.TypeDamege(newNode, type);
    }
    
    static DamageByNode(targetLayer: Node, globalPos: Vec3, type : eDamageType, damage : number = 0)
    {
        DamageManager.InstanceCheck()
        if(targetLayer == null)
            return
        
        if(damage <= 0)
        {
            damage = 0
            type = eDamageType.MISS
        }

        let newNode : Node = null;
        let prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["damage"];
        if(prefab != null)
        {
            newNode = instantiate(prefab)
        }
        newNode.getComponent(Label).string = Math.floor(damage).toString()
        newNode.parent = targetLayer
        newNode.worldPosition = new Vec3(globalPos.x, globalPos.y + 60, globalPos.z);
        
        this.TypeDamege(newNode, type);
    }

    static TypeDamege(newNode: Node, type: eDamageType){
        let x = math.randomRange(-60, 60)
        let newVec : Vec3 = newNode.worldPosition

        switch(type)
        {
            case eDamageType.MISS : 
                newNode.getComponent(Label).string = "MISS"
            break
            case eDamageType.CRITICAL:
                newNode.getComponent(Label).color = new Color(255,0,0,255);

                let newTween = new DamageTween();
                newTween.TargetTween = [];
                newTween.TargetNode = newNode;

                newTween.TargetTween.push(tween(newNode).by(0.4, {scale:new Vec3(0.25, 0.25)}, {easing:"quintOut"}).union().start());
                newTween.TargetTween.push(tween(newVec).parallel(
                    tween().by(0.6, {x : x}, {onUpdate:()=>{
                        if(newTween.TargetNode != null)
                        {
                            newNode.worldPosition = newVec
                        }
                    }}),
                    tween().by(0.5, {y : 25}, {easing:"quadOut", onUpdate:()=>{
                        if(newTween.TargetNode != null)
                        {
                            newNode.worldPosition = newVec
                        }
                    }})).union().start());
                newTween.TargetTween.push(tween(newNode.getComponent(Label).color).delay(0.4).to(0.25, {a : 0}, { onComplete:()=>{ this.tweens = this.tweens.filter(element => element.TargetNode != newNode); newNode.destroy(); } }).union().start());
                this.tweens.push(newTween);
            break

            case eDamageType.ELEMENT_FIRE : 
                newNode.getComponent(Label).color.fromHEX("#FF6A4E");
            break

            case eDamageType.ELEMENT_WATER : 
                newNode.getComponent(Label).color.fromHEX("#55B4FF");
            break

            case eDamageType.ELEMENT_EARTH : 
                newNode.getComponent(Label).color.fromHEX("#C07C17");
            break

            case eDamageType.ELEMENT_WIND :  
                newNode.getComponent(Label).color.fromHEX("#34FDF5");
            break

            case eDamageType.ELEMENT_LIGHT : 
                newNode.getComponent(Label).color.fromHEX("#FDF534");
            break

            case eDamageType.ELEMENT_DARK : 
                newNode.getComponent(Label).color.fromHEX("#9D34FD");
            break
        }
        
        if(type > eDamageType.CRITICAL)
        {
            let newTween = new DamageTween();
            newTween.TargetTween = [];
            newTween.TargetNode = newNode;

            newTween.TargetTween.push(tween(newNode).by(0.9, {scale:new Vec3(0.15, 0.15)}, {easing:"elasticOut"}).to(0.5, {scale:new Vec3(0.25,0.25)}, {easing:"sineOut"}).union().start());
            newTween.TargetTween.push(tween(newVec).parallel(
                tween().by(0.4, {x : x}, {onUpdate:()=>{
                    if(newTween.TargetNode != null)
                    {
                        newNode.worldPosition = newVec
                    }
                }}),
                tween().by(0.2, {y : 20}, {easing:"quadOut", onUpdate:()=>{
                    if(newTween.TargetNode != null)
                    {
                        newNode.worldPosition = newVec
                    }
                }}).by(0.2, {y : -20}, {easing:"quadIn", onUpdate:()=>{
                    if(newTween.TargetNode!= null)
                    {
                        newNode.worldPosition = newVec
                    }
                }})).union().start());
            newTween.TargetTween.push(tween(newNode.getComponent(Label).color).delay(0.5).to(0.5, {a : 0}, { onComplete:()=>{ this.tweens = this.tweens.filter(element => element.TargetNode != newNode); newNode.destroy(); } }).union().start());
            
            this.tweens.push(newTween);
        }
    }

    static AllClear() {
        if(this.tweens == null) {
            return;
        }
        while(this.tweens.length > 0) {
            let popTweens = this.tweens.pop();
            if(popTweens == null) {
                continue;
            }

            popTweens.TargetTween.forEach(element => element.stop());
            popTweens.TargetNode.destroy();
        }
    }
}