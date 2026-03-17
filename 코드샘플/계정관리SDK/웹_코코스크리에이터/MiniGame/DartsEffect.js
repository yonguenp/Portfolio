// Learn cc.Class:
//  - https://docs.cocos.com/creator/manual/en/scripting/class.html
// Learn Attribute:
//  - https://docs.cocos.com/creator/manual/en/scripting/reference/attributes.html
// Learn life-cycle callbacks:
//  - https://docs.cocos.com/creator/manual/en/scripting/life-cycle-callbacks.html

cc.Class({
    extends: cc.Sprite,

    properties: {
        img : [cc.SpriteFrame],
        effectEnable : true,
    },

    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    onEffect : function() {
        if(!this.effectEnable)
            return;
        this.node.active = true;        
        if(this.img.length > 0)
        {
            this.spriteFrame = this.img[parseInt(Math.random() * (this.img.length - 1) + 0.5)];
        }    
    },
});
