
import { _decorator, Component, Node, sp, resources, SpriteFrame, Sprite, Vec2, Enum, UITransform } from 'cc';
const { ccclass, property, executeInEditMode, disallowMultiple } = _decorator;

/*
 이건 아직 테스트 중인 코드.
 */
 const eRendererOrder = Enum({
    Tail: 0,
    Neck: 1,
    Leg: 2,
    Leg_left: 3,
    Leg_right: 4,
    Wing: 5,
    Arm_left: 6,
    Body: 7,
    Cloth: 8,
    Head: 9,
    Eye: 10,
    Mouse: 11,
    Ear: 12,
    Arm_right: 13,
    Bone: 14,
});

const eRendererType = Enum({
    Root: "root",
    BoneBase: "bone10",
    Bone: "bone",
    Tail: "bone6",
    Leg: "bone9",
    Neck: "n",
    Head: "bone4",
    Eye: "e",
    Mouse: "m",
    Ear: "h2",
    Leg_left: "bone8",
    Leg_right: "bone7",
    Wing: "bone5",
    Arm_left: "bone3",
    Body: "b",
    Cloth: "c",
    Arm_right: "bone2",
});
 
@ccclass('DragonSpine')
@disallowMultiple
// @inspector("packages://Scrpits/Test/DSpine.js")
@executeInEditMode
// @playOnFocus
export default class DragonSpine extends Component {
    // [1]
    // dummy = '';
    override: true;
    // [2]
    // @property
    // serializableDummy = 0;
    @property(sp.SkeletonData)
    json: sp.SkeletonData = null;
    bones = {};
    slots = {};
    slotsOrder = {};
    editor = false;

    onLoad () {
        this.createJson();
        this.test();
    }

    start () {
        // this.setJsonAnimation();
    }

    createJson () : void {
        if (this.editor) {
            return;
        }

        // this.node.removeAllChildren();
        // this.editor = true;
        // let data = this.json;
        // let bones = data.skeletonJson.bones;
        // let slots = data.skeletonJson.slots;
        // let skins = data.skeletonJson.skins;
        
        // var i = 0;
        // bones.forEach(boneData => {
        //     const name = boneData.name;

        //     let parent = this.node;
        //     if(this.objectCheck(boneData, 'parent')) {
        //         if (this.bones[boneData.parent] != null) {
        //             parent = this.bones[boneData.parent];
        //         }
        //     }

        //     let boneNode = new Node(name);
        //     let boneNodeTransForm = boneNode.addComponent(UITransform);
        //     boneNodeTransForm.setContentSize(0, 0);
        //     parent.addChild(boneNode);
        //     if(this.objectCheck(boneData, 'x') && this.objectCheck(boneData, 'y')) {
        //         boneNode.setPosition(boneData.x, boneData.y);
        //     }
        //     if(this.objectCheck(boneData, 'scaleX') && this.objectCheck(boneData, 'scaleX')) {
        //         boneNode.setScale(boneData.scaleX, boneData.scaleY);
        //     }
        //     if(this.objectCheck(boneData, 'rotation')) {
        //         boneNode.angle = boneData.rotation;
        //     }
            
        //     this.bones[name] = boneNode;
        // });

        // slots.forEach(slotData => {
        //     let parent = this.node;
        //     const parentName = slotData.bone;
        //     if (this.bones[parentName] != null) {
        //         parent = this.bones[parentName];
        //     }

        //     const name = slotData.name;
        //     let slotNode = new Node(name);
        //     parent.addChild(slotNode);
        //     // console.log(parentName, slotNode.parent.name);
            
        //     this.slots[name] = slotNode;
        //     this.slotsOrder[name] = i;
        //     i++;
        // });

        // skins.forEach(skinsData => {
        //     let datas = skinsData.attachments;
        //     for(var value in datas){
        //         const name = value;
        //         let skinData = datas[name][name];
        //         let slotNode = this.slots[name];
        //         // let slotOrder = this.slotsOrder[name];
                
        //         if (slotNode != null) {
        //             let slotTransform = slotNode.addComponent(UITransform);
        //             let slotSprite = slotNode.addComponent(Sprite);

        //             resources.load(`part/${data.name}/${name}/spriteFrame`, SpriteFrame, function (err, target) {
        //                 if(slotSprite.spriteFrame != null) {
        //                     console.log(`part/${data.name}/${name}`, slotSprite.spriteFrame);
        //                 }
        //                 slotSprite.spriteFrame = target;
        //                 // slotSprite.trim = false;
        //                 slotNode.setPosition(skinData.x, skinData.y);
        //                 slotTransform.setContentSize(skinData.width, skinData.height);
        //                 slotNode.angle = skinData.rotation;
        //                 console.log(`part/${data.name}/${name}`, skinData, target);
        //             });
        //         }
        //     }
        // });
    }

    objectCheck(object, key: string): boolean {
        if (Object.getOwnPropertyDescriptor(object, key) == undefined) return false;
        return true;
    }

    changeParent(node, newParent) : void {
        if(node.parent == newParent) return;
        var getWorldRotation = function (node) {
            var currNode = node;
            var resultRot = currNode.angle;
            do {
                currNode = currNode.parent;
                resultRot += currNode.angle;
            } while(currNode.parent != null);
            resultRot = resultRot % 360;
            return resultRot;
        };
        
        var oldWorRot = getWorldRotation(node);
        var newParentWorRot = getWorldRotation(newParent);
        var newLocRot = oldWorRot - newParentWorRot;
    
        var oldWorPos = node.convertToWorldSpaceAR(new Vec2(0,0));
        var newLocPos = newParent.convertToNodeSpaceAR(oldWorPos);
    
        node.parent = newParent;
        node.position = newLocPos;
        node.angle = newLocRot;
    }

    setOrder(node: Node, zIndex: number, sibling: number) : void
    {
        if (node == null) {
            return;
        }

        if (sibling > -1) {
            node.setSiblingIndex(sibling);
            // node._localZOrder = sibling;
        }
    }

    test(): void {
        this.setOrder(this.node, 0, eRendererOrder.Neck);

        let Root = this.node.getChildByName(eRendererType.Root);
        let BoneBase = Root.getChildByName(eRendererType.BoneBase);

        let Leg = BoneBase.getChildByName(eRendererType.Leg);
        let LegLeft = Leg.getChildByName(eRendererType.Leg_left);
        let LegRight = Leg.getChildByName(eRendererType.Leg_right);

        let Bone = BoneBase.getChildByName(eRendererType.Bone);
        let Tail = Bone.getChildByName(eRendererType.Tail);
        let Neck = Bone.getChildByName(eRendererType.Neck);
        let Head = Bone.getChildByName(eRendererType.Head);
        let Eye = Head.getChildByName(eRendererType.Eye);
        let Mouse = Head.getChildByName(eRendererType.Mouse);
        let Ear = Head.getChildByName(eRendererType.Ear);
        let Wing = Bone.getChildByName(eRendererType.Wing);
        let Arm_left = Bone.getChildByName(eRendererType.Arm_left);
        let Body = Bone.getChildByName(eRendererType.Body);
        let Cloth = Bone.getChildByName(eRendererType.Cloth);
        let Arm_right = Bone.getChildByName(eRendererType.Arm_right);

        Tail.setParent(BoneBase, true);
        this.setOrder(Tail, 0, eRendererOrder.Tail);

        this.setOrder(Leg, 0, eRendererOrder.Leg);
        this.setOrder(LegLeft, 0, eRendererOrder.Leg_left);
        this.setOrder(LegRight, 0, eRendererOrder.Leg_right);

        this.setOrder(Bone, 0, eRendererOrder.Bone);
        this.setOrder(Neck, 0, eRendererOrder.Neck);
        this.setOrder(Wing, 0, eRendererOrder.Wing);
        this.setOrder(Arm_left, 0, eRendererOrder.Arm_left);
        this.setOrder(Body, 0, eRendererOrder.Body);
        this.setOrder(Cloth, 0, eRendererOrder.Cloth);
        this.setOrder(Head, 0, eRendererOrder.Head);
        this.setOrder(Eye, 0, eRendererOrder.Eye);
        this.setOrder(Mouse, 0, eRendererOrder.Mouse);
        this.setOrder(Ear, 0, eRendererOrder.Ear);
        this.setOrder(Arm_right, 0, eRendererOrder.Arm_right);
        // this.nodeLog(Tail);
        // this.nodeLog(Leg);
        // this.nodeLog(LegLeft);
        // this.nodeLog(LegRight);
        // this.nodeLog(Bone);
        // this.nodeLog(Neck);
        // this.nodeLog(Head);
        // this.nodeLog(Eye);
        // this.nodeLog(Mouse);
        // this.nodeLog(Ear);
        // this.nodeLog(Wing);
        // this.nodeLog(Arm_left);
        // this.nodeLog(Body);
        // this.nodeLog(Arm_right);
        // this.ai.set();
    }

    // update (deltaTime: number) {
    //     // [4]
    // }
    setBone(): void {
        let Root = this.node.getChildByName(eRendererType.Root);
        let BoneBase = this.setBoneData(Root, eRendererType.BoneBase);
        let Leg = this.setBoneData(BoneBase, eRendererType.Leg);
        this.setBoneData(Leg, eRendererType.Leg_left);
        this.setBoneData(Leg, eRendererType.Leg_right);

        let Bone = this.setBoneData(BoneBase, eRendererType.Bone);
        this.setBoneData(BoneBase, eRendererType.Tail);
        this.setBoneData(Bone, eRendererType.Head);
        this.setBoneData(Bone, eRendererType.Wing);
        this.setBoneData(Bone, eRendererType.Arm_left);
        this.setBoneData(Bone, eRendererType.Arm_right);

        // let frame = this.bones[RendererType.Tail].getChildByName('t').getComponent(Sprite);
        // console.log(frame.spriteFrame);
    }
    
    boneDatas = {};
    setBoneData(parent, name): Node {
        this.bones[name] = parent.getChildByName(name);
        const position = this.bones[name].getPosition();
        const rotation = this.bones[name].angle;
        this.boneDatas[name] = { x: position.x, y: position.y, z: position.z, angle: rotation };
        return this.bones[name];
    }

    animsTime = {};
    anims = {};
    setJsonAnimation(): void {
        this.setBone();

        // let data = this.json;
        // let animations = data.skeletonJson.animations;

        // for(const animationName in animations)
        // {
        //     let animation = animations[animationName];
        //     let animationData = animation.bones;

        //     for(const boneName in animationData)
        //     {
        //         let boneAnimation = animationData[boneName];
                

        //         for(const boneAnimName in boneAnimation)
        //         {
        //             let anim = boneAnimation[boneAnimName];

        //             let func: Function = null;
        //             switch(boneAnimName)
        //             {
        //                 case 'translate': {
        //                     func = this.createTranslate;
        //                 } break;
        //                 case 'rotate': {
        //                     func = this.createRotate;
        //                 } break;
        //             }
        //             this.createAnim(animationName, boneName, boneAnimName, anim, this.valueChange, func);
        //         }
        //     }
        // }
        
        // this.setAnimTime();

        // console.log(this.boneDatas);
        // console.log(this.anims);
        // console.log(this.json);
    }

    setAnimTime(): void {
        let prevAnim = null;
        let curAnim = null;
        var lastTime = {};
        let anims = this.anims;
        for (const animName in anims)
        {
            let anim = anims[animName];
            for(var time in anim)
            {
                curAnim = anim[time];

                for (const animType in curAnim)
                {
                    let curBoneAnim = curAnim[animType];
                    curBoneAnim.forEach(element => {
                        if (lastTime[animType] == undefined) {
                            lastTime[animType] = {};
                        }
                        if (lastTime[animType][element.bone] == undefined) {
                            lastTime[animType][element.bone] = [];
                        }
                        lastTime[animType][element.bone].push(time);
                    });
                }
            }

            var lTime = 0.0;
            for(var time in anim)
            {
                curAnim = anim[time];

                for (const animType in curAnim)
                {
                    let curBoneAnim = curAnim[animType];
                    curBoneAnim.forEach(element => {
                        var times = lastTime[animType][element.bone];
                        times.forEach(curTime => {
                            if ((element.nextTime < 0 || element.nextTime > curTime) && time != curTime)
                                element.nextTime = curTime;
                            if ((element.nextTime >= 0 || element.nextTime < curTime))
                                lTime = curTime;
                        });

                        if(element.nextTime == -1)
                            element.nextTime = lTime;
                    });
                }

                console.log(time, curAnim);
            }

            // for (const animType in prevAnim)
            // {
            //     let prevBoneAnim = prevAnim[animType];
            //     prevBoneAnim.forEach(element => {
            //         if (lastTime[animType][element.bone] != undefined)
            //             element.nextTime = -1;
            //     });
            // }
            prevAnim = null;
        }
    }

    valueChange(targetValue: {}, type: string, boneName: string, boneDatas: {})
    {
        if(targetValue[type] == undefined) {
            targetValue[type] = Math.floor(boneDatas[boneName][type] * 100) * 0.01;
        } else {
            const value = boneDatas[boneName][type] + targetValue[type];
            targetValue[type] = Math.floor(value * 100) * 0.01;
        }
    }

    createAnim(animationName: string, boneName: string, boneAnimName: string, anim: [{time: number},{time: number}], check: Function, func: Function): void {
        if (func == null) {
            return;
        }

        anim.forEach(targetValue => {
            var time = this.setAnims(animationName, boneAnimName, targetValue);
            
            func(this.anims, this.boneDatas, animationName, boneName, boneAnimName, time, check, targetValue);
            // console.log(animationName, boneName, boneAnimName, targetValue);
        });
    }

    setAnims(animationName, boneAnimName, targetValue: {time: number}): number
    {
        var time = 0.0;
        if (this.objectCheck(targetValue, 'time')) {
            time = targetValue.time;
        }

        if (this.anims[animationName] == undefined) {
            this.anims[animationName] = {};
        }

        if (this.anims[animationName][time] == undefined) {
            this.anims[animationName][time] = {};
        }

        if (this.anims[animationName][time][boneAnimName] == undefined) {
            this.anims[animationName][time][boneAnimName] = [];
        }

        return time;
    }

    createTranslate(anims: {}, boneDatas: {}, animationName: string, boneName: string, boneAnimName: string, time: number, check: Function, targetValue: {x: number, y: number, z: number}): void {
        check(targetValue, 'x', boneName, boneDatas);
        check(targetValue, 'y', boneName, boneDatas);
        check(targetValue, 'z', boneName, boneDatas);

        anims[animationName][time][boneAnimName].push({ bone: boneName, x: targetValue.x, y: targetValue.y, z: targetValue.z, nextTime: -1 });
    }

    createRotate(anims: {}, boneDatas: {}, animationName: string, boneName: string, boneAnimName: string, time: number, check: Function, targetValue: {angle: number}): void {
        check(targetValue, 'angle', boneName, boneDatas);
        anims[animationName][time][boneAnimName].push({ bone: boneName, angle: targetValue.angle, nextTime: -1 });
    }

    startAnimation(): void {

    }

    updateAnimation(dt): void {
    }

    stopAnimation(): void {

    }

    update(dt) {
        this.updateAnimation(dt);
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */
